"""Lists the instructions of one GPL or MAS script resource in order from offset 0.

Usage: python -I script_listing.py GPLDATA.GFF TAG NUMBER [OPCODE=COUNT ...]

Finds resource NUMBER under TAG ("GPL " or "MAS ") through the archive's directory (FMT-GFF-001
to FMT-GFF-007), appends the stop byte 0x31 the loader adds (FND-SCRIPT-019), and decodes it as
FMT-SCRIPT-001: each instruction is an opcode byte and its parameters, each parameter an
expression read as RULE-SCRIPT-004's read_number describes. The layouts known are those of
RULE-SCRIPT-003, opcode 0x0A with two parameters, and opcodes 0x6D and 0x70 with three and four
(FND-PARTY-041); OPCODE=COUNT (hexadecimal opcode) adds an opcode that reads COUNT parameters.
The listing stops at the first opcode, expression byte, string kind or variable code whose layout
is not known, and prints the bytes there. Strings of kind 5 are printed as their length only.
The listing does not follow jumps; a script with no jump, call or nested instruction runs in this
order. Nothing is written or executed.
"""
import struct
import sys

body = open(sys.argv[1], "rb").read()
tag, number = sys.argv[2].encode().ljust(4), int(sys.argv[3])
assert body[:4] == b"GFFI"
at = struct.unpack_from("<I", body, 0x0C)[0]
count = struct.unpack_from("<H", body, at + 8)[0]
p, tables = at + 10, {}
for _ in range(count):
    name, word = body[p:p + 4], struct.unpack_from("<I", body, p + 4)[0]
    entries, indexed = word & 0x7FFFFFFF, word >> 31
    if indexed:
        index, ranges = struct.unpack_from("<II", body, p + 12)
        pairs = [struct.unpack_from("<II", body, p + 20 + 8 * k) for k in range(ranges)]
        tables[name] = ("indexed", index, [n for first, n_count in pairs for n in range(first, first + n_count)])
        p += 20 + 8 * ranges
    else:
        tables[name] = ("plain", [struct.unpack_from("<III", body, p + 8 + 12 * k) for k in range(entries)])
        p += 8 + 12 * entries
kind = tables[tag]
if kind[0] == "plain":
    offset, size = next((o, s) for n, o, s in kind[1] if n == number)
else:
    gffi_offset = next(o for n, o, s in tables[b"GFFI"][1] if n == kind[1])
    offset, size = struct.unpack_from("<II", body, gffi_offset + 4 + 8 * kind[2].index(number))
print(f"{tag.decode()!r} {number}: offset 0x{offset:X}, size {size}")
code = body[offset:offset + size] + b"\x31"

# Parameter counts of read_parameters, or "assign" for 0x16's expression and variable.
LAYOUT = {0x06: 1, 0x0A: 2, 0x0E: 0, 0x12: 1, 0x13: 1, 0x14: 2, 0x15: 0, 0x16: "assign",
          0x17: 1, 0x18: 1, 0x19: 0, 0x27: 2, 0x29: 1, 0x31: 0, 0x3E: 1, 0x3F: 1, 0x61: 0,
          0x63: 1, 0x64: 1, 0x67: 0, 0x6D: 3, 0x70: 4}
for item in sys.argv[4:]:
    key, value = item.split("=")
    LAYOUT[int(key, 16)] = int(value)
OPERATORS = {0xD1: "+", 0xD2: "-", 0xD3: "*", 0xD4: "/", 0xD5: "&&", 0xD6: "||", 0xD7: "==",
             0xD8: "!=", 0xD9: ">", 0xDA: "<", 0xDB: ">=", 0xDC: "<=", 0xDD: "&", 0xDE: "|",
             0xDF: "&~"}


class Unknown(Exception):
    pass


class Reader:
    def __init__(self):
        self.p = 0

    def byte(self):
        value = code[self.p]
        self.p += 1
        return value

    def word(self, signed=True):
        value = int.from_bytes(code[self.p:self.p + 2], "big", signed=signed)
        self.p += 2
        return value

    def string(self):
        kind = self.byte()
        if kind != 5:
            raise Unknown(f"string kind {kind}")
        buffer, shift, length = 0, 1, 0
        while True:
            if shift > 0:
                buffer = ((buffer << 8) & 0xFF00) | self.byte()
            if (buffer >> shift) & 0x7F == 3:
                return f"string5({length} characters)"
            length += 1
            shift = shift + 1 if shift < 7 else 0

    def variable(self, code_byte):
        number = self.byte()
        if code_byte & 0x40:
            number = number * 256 + self.byte()
        return f"var{code_byte & 0x3F:X}[{number}]"

    def number(self):
        parts, level = [], 0
        while True:
            b = self.byte()
            go_on = False
            if b < 0x80:
                self.p -= 1
                parts.append(str(self.word()))
            elif b in OPERATORS:
                parts.append(OPERATORS[b])
                go_on = True
            elif b == 0xE2:
                parts.append("(")
                level += 1
                go_on = True
            elif b == 0xE1:
                parts.append(")")
                level -= 1
            elif b in (0x80, 0xC0):
                parts.append("accumulator")
            elif 0x81 <= b <= 0x8A or b in (0x8D, 0x8E) or 0xC1 <= b <= 0xCA or b in (0xCD, 0xCE):
                parts.append(self.variable(b & 0x7F))
            elif b in (0x8B, 0xCB):
                high = self.word()
                parts.append(str(high * 0x10000 + self.word(False)))
            elif b in (0x8C, 0xCC):
                parts.append("{" + self.instruction() + "}")
            elif b in (0x8F, 0xCF):
                parts.append(str(int.from_bytes(bytes([self.byte()]), "big", signed=True)))
            elif b == 0x90:
                parts.append(str(self.word()))
            elif b == 0x91:
                parts.append(str(-self.word(False)))
            elif b == 0x92:
                parts.append(self.string())
            else:
                raise Unknown(f"expression byte {b:02X}")
            if not go_on:
                following = code[self.p]
                go_on = following in OPERATORS or (level > 0 and following == 0xE1)
            if not go_on:
                return " ".join(parts)

    def assign(self):
        value = self.number()
        code_byte = self.byte() & 0x7F
        wide = bool(code_byte & 0x40)
        if wide:
            code_byte -= 0x40
        if code_byte >= 0x10:
            raise Unknown(f"variable code {code_byte:02X}")
        number = self.byte()
        if wide:
            number = number * 256 + self.byte()
        return f"{value} -> var{code_byte:X}[{number}]"

    def instruction(self):
        start = self.p
        opcode = self.byte()
        layout = LAYOUT.get(opcode)
        if layout is None:
            raise Unknown(f"opcode {opcode:02X}")
        if layout == "assign":
            return f"{start:04X}: {opcode:02X} {self.assign()}"
        return f"{start:04X}: {opcode:02X} " + ", ".join(self.number() for _ in range(layout))


reader = Reader()
try:
    while reader.p < len(code):
        print(reader.instruction())
except Unknown as stop:
    print(f"stopped: {stop}; bytes from 0x{reader.p - 1:04X}: {code[reader.p - 1:reader.p + 11].hex()}")
