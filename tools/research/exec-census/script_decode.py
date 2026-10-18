"""Decodes GPL and MAS script resources instruction by instruction with every opcode's operand layout.

Usage:
  python -I script_decode.py GPLDATA.GFF OBJEX.GFF TAG NUMBER     list one resource
  python -I script_decode.py GPLDATA.GFF OBJEX.GFF --sweep        check every resource
  python -I script_decode.py GPLDATA.GFF OBJEX.GFF --all          list every resource, each line
                                                                  prefixed with TAG/NUMBER

Finds each resource through the archive's directory (FMT-GFF-001 to FMT-GFF-007), appends the
stop byte 0x31 the loader adds (FND-SCRIPT-019), and decodes it from offset 0 as one instruction
after another, each read as FND-SCRIPT-024 gives its handler's operands: expressions as
RULE-SCRIPT-004's read_number, assignments as its write_variable, strings as its read_string,
field paths as 172C:299C reads them, and the menu of RULE-TALK-001 with the trace instructions
of RULE-SCRIPT-009. Instruction 0x33 decides whether an expression follows from the type byte
of the field it names, which the decoder takes from FNFO/2 of OBJEX.GFF, the 98 bytes the game
reads into DS:60ED (FND-CONFIG-153). The menu loop is decoded to its 0x4A byte; the game also
leaves it after 25 offered rows, which depends on the rows' conditions at run time.

The listing stops at the first byte whose reading is not given (an expression byte the reader
rejects, a string kind the reader leaves in place). --sweep decodes every GPL and MAS resource,
reports those that do not decode to their last byte, and checks that every literal code offset
an instruction jumps or calls to inside the same resource (0x12, 0x13, 0x29, 0x3E, 0x3F, 0x63,
0x64, the second parameter of 0x27 and each menu row's target) and every literal start of
another script (0x14) falls on an instruction start. The listing does not follow jumps.
Nothing is written or executed.
"""
import struct
import sys


def directory(body):
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
    return tables


def numbers(tables, tag):
    kind = tables[tag]
    return [n for n, _, _ in kind[1]] if kind[0] == "plain" else list(kind[2])


def resource(body, tables, tag, number):
    kind = tables[tag]
    if kind[0] == "plain":
        offset, size = next((o, s) for n, o, s in kind[1] if n == number)
    else:
        gffi_offset = next(o for n, o, s in tables[b"GFFI"][1] if n == kind[1])
        offset, size = struct.unpack_from("<II", body, gffi_offset + 4 + 8 * kind[2].index(number))
    return body[offset:offset + size]


gpl = open(sys.argv[1], "rb").read()
objex = open(sys.argv[2], "rb").read()
FIELD_TYPES = resource(objex, directory(objex), b"FNFO", 2)
assert len(FIELD_TYPES) == 98

# Operands of each opcode's handler (FND-SCRIPT-024): a count of expressions, or a name.
E = 1
LAYOUT = {
    0x00: 0, 0x01: 2, 0x02: 1, 0x03: 1, 0x04: 1, 0x05: 1, 0x06: 1, 0x07: 1, 0x08: 1, 0x09: 1,
    0x0A: 2, 0x0B: 2, 0x0C: 1, 0x0D: 0, 0x0E: 0, 0x0F: 1, 0x10: 3, 0x11: 2, 0x12: 1, 0x13: 1,
    0x14: 2, 0x15: 0, 0x16: "assign", 0x17: 1, 0x18: 1, 0x19: 0, 0x1A: 2, 0x1B: 4, 0x1C: 4,
    0x1D: 1, 0x1E: 1, 0x1F: 1, 0x20: 2, 0x21: 2, 0x22: 4, 0x23: "string", 0x24: 1, 0x25: 6,
    0x27: 2, 0x28: "number number string", 0x29: 1, 0x2A: 0, 0x2B: 0, 0x2C: "string", 0x2D: 2,
    0x2E: 1, 0x2F: 3, 0x30: 1, 0x31: 0, 0x32: 2, 0x33: "search", 0x34: 1, 0x35: 0, 0x36: 1,
    0x37: 2, 0x38: 0, 0x39: 4, 0x3A: 2, 0x3B: 1, 0x3C: 3, 0x3D: 1, 0x3E: 1, 0x3F: 1,
    0x40: "field assign", 0x41: 1, 0x42: 1, 0x43: 1, 0x44: 1, 0x45: 1, 0x46: 1, 0x47: 0,
    0x48: "menu", 0x49: 2, 0x4B: 3, 0x4F: 2, 0x50: 2, 0x51: 0, 0x52: 1, 0x54: 1, 0x58: 3,
    0x59: 3, 0x5A: 2, 0x5B: 2, 0x5C: 4, 0x5D: 1, 0x5E: 5, 0x5F: 1, 0x61: 0, 0x62: 1, 0x63: 1,
    0x64: 1, 0x65: 3, 0x66: 3, 0x67: 0, 0x68: 5, 0x69: 5, 0x6A: 7, 0x6B: 7, 0x6C: 3, 0x6D: 3,
    0x6E: 3, 0x6F: 3, 0x70: 4, 0x76: 2, 0x77: 2, 0x78: 2, 0x79: 2, 0x7A: 2, 0x7B: 2, 0x7C: 2,
    0x7D: 2, 0x7E: 2, 0x7F: 2, 0x80: 2,
}
# The fifteen opcodes whose handler stops the script read nothing (FND-SCRIPT-005).
for stop in (0x26, 0x4A, 0x4C, 0x4D, 0x4E, 0x53, 0x55, 0x56, 0x57, 0x60, 0x71, 0x72, 0x73, 0x74, 0x75):
    LAYOUT[stop] = 0
assert sorted(LAYOUT) == list(range(0x81))
TRACE = (0x23, 0x28, 0x2E, 0x4B)
FIELD_KEYS = (0x25, 0x26, 0x27, 0x28, 0x2B, 0x2C)
OPERATORS = {0xD1: "+", 0xD2: "-", 0xD3: "*", 0xD4: "/", 0xD5: "&&", 0xD6: "||", 0xD7: "==",
             0xD8: "!=", 0xD9: ">", 0xDA: "<", 0xDB: ">=", 0xDC: "<=", 0xDD: "&", 0xDE: "|",
             0xDF: "&~"}
# Offsets that name code: opcode -> indexes of the operands that are offsets in this script.
JUMPS = {0x12: (0,), 0x13: (0,), 0x29: (0,), 0x3E: (0,), 0x3F: (0,), 0x63: (0,), 0x64: (0,), 0x27: (1,)}


class Unknown(Exception):
    pass


class Reader:
    def __init__(self, code):
        self.code, self.p, self.starts, self.targets, self.calls = code, 0, set(), [], []

    def byte(self):
        if self.p >= len(self.code):
            raise Unknown("past the end")
        value = self.code[self.p]
        self.p += 1
        return value

    def word(self, signed=True):
        return int.from_bytes(bytes([self.byte(), self.byte()]), "big", signed=signed)

    def string(self):
        kind = self.code[self.p]
        if kind == 1:
            self.p += 1
            return "string1"
        if kind == 2:
            self.p += 1
            # Each byte passes through 172C:325F, which gives (b << 2) & 0xFF, never 3.
            for _ in range(299):
                self.byte()
            return "string2(299 bytes)"
        if kind != 5:
            raise Unknown(f"string kind {kind}")
        self.p += 1
        buffer, shift, length = 0, 1, 0
        while length < 299:
            if shift > 0:
                buffer = ((buffer << 8) & 0xFF00) | self.byte()
            if (buffer >> shift) & 0x7F == 3:
                return f"string5({length} characters)"
            length += 1
            shift = shift + 1 if shift < 7 else 0
        return "string5(299 characters, unterminated)"

    def field_path(self):
        # 172C:299C: a word; below 0x8000 a handle by number, else one of six keys or nothing more.
        first = self.word(False)
        if first >= 0x8000 and (first & 0x7FFF) not in FIELD_KEYS:
            return f"path({first:04X})"
        count = self.byte()
        fields = [self.byte() for _ in range(count)]
        return f"path({first:04X}; {','.join(map(str, fields))})"

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
                parts.append("{" + self.instruction(nested=True) + "}")
            elif b in (0x8F, 0xCF):
                parts.append(str(int.from_bytes(bytes([self.byte()]), "big", signed=True)))
            elif b == 0x90:
                parts.append(str(self.word()))
            elif b == 0x91:
                parts.append(str(-self.word(False)))
            elif b == 0x92:
                parts.append(self.string())
            elif b == 0xB1:
                parts.append(self.field_path())
            else:
                raise Unknown(f"expression byte {b:02X}")
            if not go_on:
                following = self.code[self.p] if self.p < len(self.code) else None
                go_on = following in OPERATORS or (level > 0 and following == 0xE1)
            if not go_on:
                return " ".join(parts)

    def literal(self, text):
        try:
            return int(text)
        except ValueError:
            return None

    def assign(self):
        code_byte = self.byte() & 0x7F
        wide = bool(code_byte & 0x40)
        if wide:
            code_byte -= 0x40
        if code_byte >= 0x10:
            return f"-> {self.field_path()}"
        number = self.byte()
        if wide:
            number = number * 256 + self.byte()
        return f"-> var{code_byte:X}[{number}]"

    def search(self):
        # 172C:084E: an expression, two bytes, then rounds of field, operator and, for
        # operators 4 to 6 and a field type other than 6 to 8 or above 9, an expression.
        parts = [self.number(), str(self.byte()), str(self.byte())]
        while True:
            if self.code[self.p] == 0x53:
                self.p += 1
            field, operator = self.byte(), self.byte()
            part = f"[{field} op{operator}"
            if 4 <= operator <= 6:
                kind = FIELD_TYPES[field] if field < len(FIELD_TYPES) else None
                if kind is None:
                    raise Unknown(f"field {field} beyond FNFO/2")
                if kind <= 5 or kind == 9:
                    part += " " + self.number()
            parts.append(part + "]")
            if self.code[self.p] != 0x53:
                return ", ".join(parts)

    def field_assign(self):
        # 172C:13E0 peeks a word: exactly 0x8000 reads nothing; otherwise a path and an expression.
        peek = int.from_bytes(self.code[self.p:self.p + 2], "big")
        if peek == 0x8000:
            return "(nothing read)"
        path = self.field_path()
        return f"{path} = {self.number()}"

    def traces(self, out):
        while self.code[self.p] in TRACE:
            out.append(self.instruction())

    def menu(self):
        out = []
        self.traces(out)
        out.append(self.number())
        while self.code[self.p] != 0x4A:
            self.traces(out)
            label, target, condition = self.number(), self.number(), self.number()
            out.append(f"row({label}, {target}, {condition})")
            value = self.literal(target)
            if value is not None:
                self.targets.append((self.p, value & 0xFFFF))
            self.traces(out)
        self.byte()
        return "; ".join(out)

    def instruction(self, nested=False):
        start = self.p
        if not nested:
            self.starts.add(start)
        opcode = self.byte()
        if opcode > 0x80:
            raise Unknown(f"opcode {opcode:02X}")
        layout = LAYOUT[opcode]
        if isinstance(layout, int):
            operands = [self.number() for _ in range(layout)]
            for index in JUMPS.get(opcode, ()):
                value = self.literal(operands[index])
                if value is not None:
                    self.targets.append((start, value & 0xFFFF))
            if opcode == 0x14:
                script, offset = self.literal(operands[1]), self.literal(operands[0])
                if script is not None and offset is not None:
                    self.calls.append((start, script & 0xFFFF, offset & 0xFFFF))
            text = ", ".join(operands)
        elif layout == "assign":
            text = f"{self.number()} {self.assign()}"
        elif layout == "string":
            text = self.string()
        elif layout == "number number string":
            text = f"{self.number()}, {self.number()}, {self.string()}"
        elif layout == "search":
            text = self.search()
        elif layout == "field assign":
            text = self.field_assign()
        else:
            text = self.menu()
        return f"{start:04X}: {opcode:02X} {text}"


def decode(code, out=None):
    """Decodes from offset 0; after a byte with no reading, resumes at the first literal code
    offset past it, as a jump or call there would, and reports each stop."""
    reader, errors = Reader(code), []
    while True:
        try:
            while reader.p < len(code):
                line = reader.instruction()
                if out is not None:
                    out.append(line)
            break
        except (Unknown, IndexError) as stop:
            errors.append(f"stopped at 0x{reader.p:04X}: {stop}")
            later = sorted(t for _, t in reader.targets if t >= reader.p)
            if not later:
                break
            errors[-1] += f"; resumed at the literal target 0x{later[0]:04X}"
            reader.p = later[0]
            if out is not None:
                out.append(errors[-1])
    return reader, "; ".join(errors) or None


tables = directory(gpl)
if sys.argv[3] == "--sweep":
    decoded, failed, bad_targets, bad_calls, checked, calls = {}, [], [], [], 0, []
    for tag in (b"GPL ", b"MAS "):
        for number in numbers(tables, tag):
            code = resource(gpl, tables, tag, number) + b"\x31"
            reader, error = decode(code)
            decoded[(tag, number)] = reader
            if error:
                failed.append(f"{tag.decode()}/{number}: {error}")
                if reader.p < len(code):
                    continue
            for at, target in reader.targets:
                checked += 1
                if target not in reader.starts:
                    bad_targets.append(f"{tag.decode()}/{number} 0x{at:04X} -> 0x{target:04X}")
            calls += [(tag, number, at, script, offset) for at, script, offset in reader.calls]
    for tag, number, at, script, offset in calls:
        callee = decoded.get((b"GPL ", script))
        if callee is None or offset not in callee.starts:
            bad_calls.append(f"{tag.decode()}/{number} 0x{at:04X} -> GPL/{script} 0x{offset:04X}")
    print(f"resources {len(decoded)}, decoded to the end without a stop {len(decoded) - len(failed)}")
    print(f"literal code offsets checked {checked}, not on an instruction start {len(bad_targets)}")
    print(f"literal script calls checked {len(calls)}, not on an instruction start {len(bad_calls)}")
    for line in failed + bad_targets + bad_calls:
        print(line)
elif sys.argv[3] == "--all":
    for tag in (b"GPL ", b"MAS "):
        for number in numbers(tables, tag):
            lines = []
            _, error = decode(resource(gpl, tables, tag, number) + b"1", lines)
            name = f"{tag.decode().strip()}/{number}"
            for line in lines:
                print(name, line)
            if error:
                print(name, "ERROR", error)
else:
    tag, number = sys.argv[3].encode().ljust(4), int(sys.argv[4])
    code = resource(gpl, tables, tag, number) + b"\x31"
    print(f"{tag.decode()!r} {number}: size {len(code) - 1}")
    lines = []
    _, error = decode(code, lines)
    print("\n".join(lines))
    if error:
        print(error)
