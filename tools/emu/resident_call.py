"""One resident real-mode call; no game process or device emulation."""
import base64
import json
import sys

from capstone import Cs, CS_ARCH_X86, CS_MODE_16
from unicorn import (Uc, UcError, UC_ARCH_X86, UC_MODE_16, UC_HOOK_CODE,
                     UC_HOOK_INTR, UC_HOOK_INSN, UC_HOOK_MEM_WRITE)
from unicorn import __version__ as unicorn_version
from unicorn import x86_const as x86

MEMORY_END = 0x100000
RETURN_SEGMENT, RETURN_OFFSET = 0xF000, 0x1000
RETURN_ADDRESS = RETURN_SEGMENT * 16 + RETURN_OFFSET
REGISTERS = {name: getattr(x86, 'UC_X86_REG_' + name.upper()) for name in
             ['eax', 'ebx', 'ecx', 'edx', 'esi', 'edi', 'ebp', 'esp',
              'cs', 'ds', 'es', 'ss', 'fs', 'gs', 'eflags']}


def integer(n, low, high, name):
    if isinstance(n, bool) or not isinstance(n, int) or not low <= n <= high:
        raise ValueError(f'invalid {name}')
    return n


def bounded_range(address, length, name):
    integer(address, 0, MEMORY_END - 1, name)
    integer(length, 1, MEMORY_END - address, name + ' length')


def execute(packet):
    if packet.get('returnBytes', 4) != 4:
        raise ValueError('only a far root return without argument cleanup is supported')
    if packet.get('protocol') != 1 or unicorn_version != '2.1.4':
        raise ValueError('requires resident-call protocol 1 and Unicorn 2.1.4')
    image = base64.b64decode(packet['image'], validate=True)
    base = packet['imageBase']
    bounded_range(base, len(image), 'resident image')
    regions = packet['regions']
    if not regions or len(regions) > 128:
        raise ValueError('invalid code region count')
    for r in regions:
        bounded_range(r['linearStart'], r['linearEnd'] - r['linearStart'], 'code region')
    maximum = integer(packet.get('maxInstructions', 10000), 1, 100000, 'instruction bound')
    max_writes = integer(packet.get('maxWrites', 10000), 1, 100000, 'write bound')
    inputs = packet.get('registers', {})
    if any(k not in REGISTERS or k in ['cs', 'esp'] for k in inputs):
        raise ValueError('unsupported entry register; stack and CS are harness-owned')
    stack_segment = integer(packet.get('stackSegment', 0x9000), 0, 65535, 'stack segment')
    stack_offset = integer(packet.get('stackOffset', 0xE000), 4, 65532, 'stack offset')
    stack_address = stack_segment * 16 + stack_offset
    bounded_range(stack_address, 4, 'return frame')
    if stack_address < base + len(image) and stack_address + 4 > base:
        raise ValueError('return frame overlaps the resident image')
    if base <= RETURN_ADDRESS < base + len(image):
        raise ValueError('return sentinel overlaps the resident image')
    cpu = Uc(UC_ARCH_X86, UC_MODE_16)
    cpu.mem_map(0, MEMORY_END)
    cpu.mem_write(base, image)
    cpu.mem_write(RETURN_ADDRESS, b'\xf4')  # Synthetic return sentinel, never executed.
    for name, value in inputs.items():
        cpu.reg_write(REGISTERS[name], integer(value, 0, 0xFFFFFFFF if name.startswith('e') else 65535, name))
    if 'ss' in inputs and inputs['ss'] != stack_segment:
        raise ValueError('entry SS differs from the declared stack segment')
    cpu.reg_write(x86.UC_X86_REG_SS, stack_segment)
    cpu.reg_write(x86.UC_X86_REG_SP, stack_offset)
    cpu.reg_write(x86.UC_X86_REG_CS, packet['entrySegment'])
    cpu.reg_write(x86.UC_X86_REG_IP, packet['entryOffset'])
    cpu.mem_write(stack_address, RETURN_OFFSET.to_bytes(2, 'little') + RETURN_SEGMENT.to_bytes(2, 'little'))
    initial_registers = {k: cpu.reg_read(v) for k, v in REGISTERS.items()}
    observations = packet.get('observations', [])
    if len(observations) > 128:
        raise ValueError('too many observations')
    observed = []
    for o in observations:
        if not isinstance(o.get('name'), str) or not o['name']:
            raise ValueError('name each observation by its field path')
        address = integer(o['segment'], 0, 65535, 'observation segment') * 16 + integer(o['offset'], 0, 65535, 'observation offset')
        length = integer(o['length'], 1, 4096, 'observation length')
        if o['offset'] + length > 65536:
            raise ValueError('observation crosses its segment offset limit')
        bounded_range(address, length, 'observation')
        observed.append({**o, 'address': address, 'before': bytes(cpu.mem_read(address, length)).hex()})
    ports = {}
    for p in packet.get('ports', []):
        key = (integer(p['port'], 0, 65535, 'port'), p['direction'], p['width'])
        if key[1] not in ['read', 'write'] or key[2] not in [1, 2, 4] or key in ports or not p.get('name'):
            raise ValueError('name each unique port direction and width')
        if p['direction'] == 'read':
            integer(p['value'], 0, (1 << (8 * p['width'])) - 1, 'port value')
        ports[key] = p
    stops = {}
    for stub in packet.get('interrupts', []):
        vector = integer(stub['vector'], 0, 255, 'interrupt vector')
        if vector == 63 or stub.get('action') != 'stop' or not stub.get('name') or vector in stops:
            raise ValueError('interrupt stubs must name an explicit stop; overlays cannot be loaded')
        stops[vector] = stub
    trace, writes, hardware, branches = [], [], [], {}
    decoder = Cs(CS_ARCH_X86, CS_MODE_16)
    expected_branches = set()
    instruction_starts = {}
    for r in regions:
        for ins in decoder.disasm(image[r['linearStart'] - base:r['linearEnd'] - base], r['linearStart']):
            instruction_starts[ins.address] = ins.size
            if (ins.mnemonic.startswith('j') and ins.mnemonic not in ['jmp', 'ljmp']) or ins.mnemonic.startswith('loop'):
                expected_branches.add(ins.address)
    entry_address = packet['entrySegment'] * 16 + packet['entryOffset']
    if entry_address not in instruction_starts:
        raise ValueError('call entry is not a declared instruction start')
    prior_branch = None
    returned = False
    stop = None

    def location():
        segment = cpu.reg_read(x86.UC_X86_REG_CS)
        offset = cpu.reg_read(x86.UC_X86_REG_IP)
        return {'segment': segment, 'offset': offset, 'linear': segment * 16 + offset}

    def instruction(uc, address, size, _):
        nonlocal returned, prior_branch
        if prior_branch is not None:
            site, fallthrough = prior_branch
            branches.setdefault(site, set()).add('fallthrough' if address == fallthrough else 'taken')
            prior_branch = None
        if address == RETURN_ADDRESS:
            if location()['segment'] != RETURN_SEGMENT or location()['offset'] != RETURN_OFFSET or uc.reg_read(x86.UC_X86_REG_SP) != stack_offset + 4:
                raise ValueError('return frame or stack balance differs from the call')
            returned = True
            uc.emu_stop()
            return
        region = next((r for r in regions if r['linearStart'] <= address and address + size <= r['linearEnd']), None)
        if not region or instruction_starts.get(address) != size:
            raise ValueError(f'undeclared resident execution at {location()}')
        if len(trace) >= maximum:
            raise ValueError(f'instruction limit at {location()}')
        trace.append({**location(), 'fileOffset': region['start'] + address - region['linearStart'], 'size': size, 'region': region['name']})
        if address in expected_branches:
            prior_branch = (address, address + size)

    def write(uc, access, address, size, value, _):
        bounded_range(address, size, 'write')
        if any(address < r['linearEnd'] and address + size > r['linearStart'] for r in regions):
            raise ValueError(f'write to declared code at {location()}')
        if len(writes) >= max_writes:
            raise ValueError(f'write limit at {location()}')
        writes.append({**location(), 'address': address, 'width': size, 'value': value & ((1 << (8 * size)) - 1)})

    def service_location():
        return {k: trace[-1][k] for k in ['segment', 'offset', 'linear']} if trace else location()

    def interrupt(uc, vector, _):
        nonlocal stop
        if vector not in stops:
            raise ValueError(f'unmodeled interrupt {vector} at {service_location()}')
        hardware.append({'kind': 'interrupt', 'vector': vector, 'name': stops[vector]['name'], **service_location()})
        stop = stops[vector]['name']
        uc.emu_stop()

    def port_read(uc, port, width, _):
        p = ports.get((port, 'read', width))
        if p is None:
            raise ValueError(f'unmodeled port read {port}, width {width}, at {service_location()}')
        hardware.append({'kind': 'port-read', 'port': port, 'width': width, 'value': p['value'], 'name': p['name'], **service_location()})
        return p['value']

    def port_write(uc, port, width, value, _):
        p = ports.get((port, 'write', width))
        if p is None:
            raise ValueError(f'unmodeled port write {port}, width {width}, at {service_location()}')
        hardware.append({'kind': 'port-write', 'port': port, 'width': width, 'value': value, 'name': p['name'], **service_location()})

    cpu.hook_add(UC_HOOK_CODE, instruction)
    cpu.hook_add(UC_HOOK_MEM_WRITE, write)
    cpu.hook_add(UC_HOOK_INTR, interrupt)
    cpu.hook_add(UC_HOOK_INSN, port_read, None, 1, 0, x86.UC_X86_INS_IN)
    cpu.hook_add(UC_HOOK_INSN, port_write, None, 1, 0, x86.UC_X86_INS_OUT)
    try:
        cpu.emu_start(packet['entrySegment'] * 16 + packet['entryOffset'], MEMORY_END)
    except UcError as error:
        raise ValueError(f'emulator stopped at {location()}: {error}') from error
    if not returned and stop is None:
        raise ValueError(f'function stopped without returning at {location()}')
    for o in observed:
        o['after'] = bytes(cpu.mem_read(o['address'], o['length'])).hex()
        del o['address']
    return {
        'schema': 'resident-emulated-call-v1', 'unicornVersion': unicorn_version,
        'sourceIdentity': packet['sourceIdentity'], 'returned': returned, 'stop': stop,
        'steps': len(trace), 'instructions': trace, 'writes': writes, 'hardware': hardware,
        'registers': {k: cpu.reg_read(v) for k, v in REGISTERS.items()}, 'observations': observed,
        'branches': [{'fileOffset': address - base + packet['regions'][0]['start'] - (packet['regions'][0]['linearStart'] - base),
                      'linear': address, 'outcomes': sorted(branches.get(address, []))} for address in sorted(expected_branches)],
        'setup': {'memoryModel': 'ordinary RAM below one MiB; no video/card/timer model',
                  'interrupts': list(stops.values()), 'ports': list(ports.values()),
                  'entryRegisters': initial_registers,
                  'outsideResident': 'zero-filled synthetic RAM; only the far return frame is installed',
                  'returnFrameBytes': 4, 'stackSegment': stack_segment, 'stackOffset': stack_offset},
        'nativeReachability': 'unconfirmed', 'presentation': 'not tested',
    }


if __name__ == '__main__':
    try:
        raw = sys.stdin.buffer.read(4 * 1024 * 1024 + 1)
        if len(raw) > 4 * 1024 * 1024:
            raise ValueError('call packet exceeds four MiB')
        print(json.dumps(execute(json.loads(raw))))
    except (ValueError, KeyError, TypeError) as error:
        print(str(error), file=sys.stderr)
        sys.exit(1)
