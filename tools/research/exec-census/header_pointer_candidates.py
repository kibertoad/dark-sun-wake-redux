"""Bounded next-transfer candidates for the header-pointer fixup census.

This helper does not establish instruction starts, reachability, argument
position, alias behavior or pointer lifetime. Original-derived output stays
in GAME_DIR. Run with --self-test for synthetic boundary controls.
"""
import json
import sys
from capstone import Cs, CS_ARCH_X86, CS_MODE_16
from capstone import CS_GRP_CALL, CS_GRP_JUMP, CS_GRP_RET, CS_GRP_IRET, CS_GRP_INT


def next_transfer(code, start, maximum=32):
    if not 0 <= start <= len(code) or not 1 <= maximum <= 256:
        raise ValueError('invalid bounded candidate window')
    decoder = Cs(CS_ARCH_X86, CS_MODE_16)
    decoder.detail = True
    cursor = start
    for _ in range(maximum):
        if cursor == len(code):
            return {'kind': 'overlay-end', 'offset': cursor}
        instruction = next(decoder.disasm(code[cursor:cursor + 15], cursor, count=1), None)
        if instruction is None:
            return {'kind': 'undecoded', 'offset': cursor}
        groups = set(instruction.groups)
        if groups & {CS_GRP_CALL, CS_GRP_JUMP, CS_GRP_RET, CS_GRP_IRET, CS_GRP_INT}:
            kind = ('call' if CS_GRP_CALL in groups else 'jump' if CS_GRP_JUMP in groups
                    else 'return' if groups & {CS_GRP_RET, CS_GRP_IRET} else 'interrupt')
            return {'kind': kind, 'offset': cursor, 'mnemonic': instruction.mnemonic,
                    'operand': instruction.op_str}
        if instruction.mnemonic in ('hlt', 'ud2'):
            return {'kind': 'stop', 'offset': cursor, 'mnemonic': instruction.mnemonic}
        cursor += instruction.size
    return {'kind': 'window-limit', 'offset': cursor}


def report(descriptor, fixup, code, segment, trampoline, pair_end):
    return json.dumps({'source_descriptor': descriptor, 'segment_fixup': fixup,
                       'target_segment': segment, 'trampoline_offset': trampoline,
                       'instruction_status': 'candidate-only',
                       'following_transfer': next_transfer(code, pair_end)}, sort_keys=True)


def self_test():
    cases = [
        (bytes.fromhex('909a78563412'), 32, 'call', 1),
        (bytes.fromhex('eb0290909a78563412'), 32, 'jump', 0),
        (bytes.fromhex('cb9a78563412'), 32, 'return', 0),
        (bytes.fromhex('cd219a00000000'), 32, 'interrupt', 0),
        (bytes.fromhex('90'), 32, 'overlay-end', 1),
        (bytes.fromhex('9a'), 32, 'undecoded', 0),
        (bytes.fromhex('9090e80000'), 2, 'window-limit', 2),
        (bytes.fromhex('90e80000'), 2, 'call', 1),
        (bytes.fromhex('f49a00000000'), 32, 'stop', 0),
    ]
    for code, maximum, kind, offset in cases:
        actual = next_transfer(code, 0, maximum)
        assert (actual['kind'], actual['offset']) == (kind, offset), actual
    for start, maximum in [(-1, 32), (2, 32), (0, 0), (0, 257)]:
        try:
            next_transfer(b'\x90', start, maximum)
        except ValueError:
            continue
        raise AssertionError('invalid boundary admitted')
    print('13 synthetic next-transfer boundary controls passed')


if __name__ == '__main__':
    if sys.argv[1:] != ['--self-test']:
        raise SystemExit('usage: header_pointer_candidates.py --self-test')
    self_test()
