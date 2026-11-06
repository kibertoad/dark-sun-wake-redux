"""Lists DSUN.EXE instructions that write to one word or byte of a segment through ES.

Usage: python -I segment_displacement_writes.py PATH_TO_DSUN.EXE SEGMENT DISPLACEMENT [--overlap]

Checks the installed DSUN.EXE's size and XXH3-128. SEGMENT is a resident segment as the load
image at segment 0x1000 names it (hexadecimal, such as 4C0E) and DISPLACEMENT a hexadecimal
offset in it. Decodes 16-bit x86 at every byte of the MZ load image and of every overlay's code
(flags-3 descriptors, FMT-EXE-002/FMT-EXE-003), and keeps each instruction with an ES segment
override whose first operand is memory with that displacement and no base or index register
(mov, the and/or/xor/add/sub/adc/sbb group, inc, dec, not, neg, xchg and the shifts), at any
operand width. By default the encoded displacement must equal DISPLACEMENT.
With --overlap, list a write whose operand byte interval covers DISPLACEMENT,
including a word/dword beginning earlier. Intervals do not wrap at offset 10000;
this is encoded access extent, not a claim about native segment-limit behavior.
Run --self-test for synthetic byte-extent and operand controls. It prints the
instruction when one of the 12 bytes before it loads a register with the segment's word: the
file word SEGMENT - 0x1000 in the load image, or in overlay code the segment word descriptor * 8
of the descriptor whose segment is SEGMENT (FMT-EXE-005). Hits whose ES load is further away,
writes through a base or index register, string instructions and block copies are not listed.
Decoding at every byte also lists instructions that start inside other instructions; each hit
is to be checked against the instructions around it. Nothing is written or executed.
"""
import struct
import sys
import runpy
from pathlib import Path

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_MEM, X86_PREFIX_ES, X86_REG_ES

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
WRITES = {"mov", "and", "or", "xor", "add", "sub", "adc", "sbb", "inc", "dec", "not", "neg",
          "xchg", "shl", "shr", "sar", "sal", "rol", "ror", "rcl", "rcr"}

def displacement_matches(start, size, target, overlap=False):
    return start <= target < start + size if overlap else start == target


def self_test():
    decoder = Cs(CS_ARCH_X86, CS_MODE_16)
    decoder.detail = True
    # Invented operands and values; no original-game bytes are fixtures.
    cases = [
        ('26c70600003412', [0, 1], [0]),
        ('26c70602003412', [2, 3], [2]),
        ('6626c706000078563412', [0, 1, 2, 3], [0]),
        ('26c6060100aa', [1], [1]),
        ('c70600003412', [], []),
        ('26c7073412', [], []),
        ('26833e000000', [], []),
        ('26c41e0000', [], []),
        ('26c706ffff3412', [], []),
    ]
    for encoded, expected_overlap, expected_exact in cases:
        ins = next(decoder.disasm(bytes.fromhex(encoded), 0), None)
        assert ins is not None
        operand = ins.operands[0]
        admitted = (ins.mnemonic in WRITES and operand.type == X86_OP_MEM
                    and operand.mem.segment == X86_REG_ES and not operand.mem.base
                    and not operand.mem.index and X86_PREFIX_ES in ins.prefix)
        for overlap, expected in [(True, expected_overlap), (False, expected_exact)]:
            actual = [n for n in range(4) if admitted and displacement_matches(
                operand.mem.disp & 0xFFFF, operand.size, n, overlap)]
            assert actual == expected, (encoded, overlap, actual, expected)
    assert displacement_matches(0xffff, 2, 0xffff, True)
    assert not displacement_matches(0xffff, 2, 0, True)
    print('20 synthetic segment-writer extent/operand controls passed')


def main():
    if len(sys.argv) not in (4, 5) or (len(sys.argv) == 5 and sys.argv[4] != '--overlap'):
        raise SystemExit('usage: segment_displacement_writes.py APPROVED_EXE SEG DISP [--overlap]')
    overlap = len(sys.argv) == 5
    body = open(sys.argv[1], "rb").read()
    assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
    segment, disp = int(sys.argv[2], 16), int(sys.argv[3], 16)
    md = Cs(CS_ARCH_X86, CS_MODE_16)
    md.detail = True
    header = struct.unpack_from("<H", body, 8)[0] * 16
    cblp, pages = struct.unpack_from("<HH", body, 2)
    image_end = (pages - 1) * 512 + (cblp or 512)
    table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
    rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
    descriptor_words = runpy.run_path(str(Path(__file__).with_name("segment_aliases.py")))["descriptor_words"]
    overlay_words = descriptor_words(rows, segment)
    spans = [("load image", header, image_end, {segment - 0x1000})]
    for i, (seg, _, flags, _) in enumerate(rows):
        if flags == 3 and overlay_words:
            payload_offset, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
            start = image_end + 16 + payload_offset
            spans.append((f"overlay {i}", start, start + code_size, overlay_words))
    print(f"segment {segment:04X}, displacement 0x{disp:X}, overlay segment words "
          f"{','.join(hex(word) for word in sorted(overlay_words)) or 'none'}")
    if overlap:
        print("mode: overlapping nonwrapping operand byte extents; candidates only")
    for name, start, end, words in spans:
        loads = {bytes([op]) + struct.pack("<H", word)
                 for word in words for op in (0xB8, 0xB9, 0xBA, 0xBB, 0xBE, 0xBF)}
        for at in range(start, end):
            ins = next(md.disasm(body[at:min(at + 15, end)], 0), None)
            if ins is None or ins.mnemonic not in WRITES or not ins.operands:
                continue
            o = ins.operands[0]
            if (o.type != X86_OP_MEM or o.mem.segment != X86_REG_ES or o.mem.base or o.mem.index
                    or not displacement_matches(o.mem.disp & 0xFFFF, o.size, disp, overlap) or X86_PREFIX_ES not in ins.prefix):
                continue
            before = body[max(start, at - 12):at]
            if not any(load in before for load in loads):
                continue
            where = (f"{0x1000 + (at - header) // 16:04X}:{(at - header) % 16:04X} (file 0x{at:08X})"
                     if name == "load image" else f"{name} file 0x{at:08X}")
            print(f"{where}  {ins.mnemonic} {ins.op_str}")


if __name__ == '__main__':
    if sys.argv[1:] == ['--self-test']:
        self_test()
    else:
        main()
