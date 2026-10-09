"""Searches DSUN.EXE for code that could find the FBOV pack header in the file (FND-EXE-569).

Usage: python -I pack_header_readers.py INSTALLED_DSUN.EXE DISC_DSUN.EXE

Checks both files' sizes and XXH3-128. In each, decodes 16-bit x86 at every byte of the load
image and of every overlay's code and lists each instruction with an immediate or displacement
equal to 0x4246 ('FB'), 0x564F ('OV'), the low word of the pack header's file offset, that low
word plus 8 (segment_table_offset) or plus 0x0C (segment_count). The instruction found at the
overlay manager's startup is the search's positive control. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_IMM, X86_OP_MEM

EDITIONS = [(634416, "e296af55ba2ecde7e77f555c90f33d0b"), (634704, "318cd5ec0559901add3780097162a919")]

md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True
for path, identity in zip(sys.argv[1:3], EDITIONS):
    body = open(path, "rb").read()
    assert (len(body), xxhash.xxh3_128_hexdigest(body)) == identity, f"unexpected file {path}"
    header = struct.unpack_from("<H", body, 8)[0] * 16
    cblp, pages = struct.unpack_from("<HH", body, 2)
    image_end = (pages - 1) * 512 + (cblp or 512)
    pack = image_end
    assert body[pack:pack + 2] == b"FB" and body[pack + 2:pack + 4] == b"OV", "no FBOV header at the image end"
    table_offset, count = struct.unpack_from("<IH", body, pack + 8)
    low = pack & 0xFFFF
    wanted = {0x4246: "'FB'", 0x564F: "'OV'", low: "pack offset low word", (low + 8) & 0xFFFF: "+8",
              (low + 0x0C) & 0xFFFF: "+0x0C"}
    print(f"== {path}: pack header at {pack:#x}, segment table offset {table_offset:#x}, {count} records,"
          f" looking for {', '.join(f'{v:#06x} {n}' for v, n in wanted.items())}")
    spans = [("load image", header, image_end)]
    for i in range(count):
        seg, _, flags, _ = struct.unpack_from("<HHHH", body, table_offset + 8 * i)
        if flags == 3:
            payload_offset, code_size = struct.unpack_from("<IH", body, header + seg * 16 + 4)
            spans.append((f"overlay {i}", pack + 16 + payload_offset, pack + 16 + payload_offset + code_size))
    for name, start, end in spans:
        for at in range(start, end):
            ins = next(md.disasm(body[at:min(at + 15, end)], 0), None)
            if ins is None:
                continue
            for o in ins.operands:
                value = o.imm & 0xFFFF if o.type == X86_OP_IMM else o.mem.disp & 0xFFFF if o.type == X86_OP_MEM else None
                if value in wanted and (o.type != X86_OP_MEM or ins.disp_size) and (o.type != X86_OP_IMM or ins.imm_size):
                    where = (f"{0x1000 + (at - header) // 16:04X}:{(at - header) % 16:04X}" if name == "load image"
                             else f"{name} file {at:#x}")
                    print(f"{where}  {ins.mnemonic} {ins.op_str}  ({wanted[value]})")
                    break
