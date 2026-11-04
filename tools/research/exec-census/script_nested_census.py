"""Bounded nested instruction metadata from a controlled decoded index.

Usage: python -I script_nested_census.py LISTING CONFIG_JSON
Original-derived reports belong in GAME_DIR. No script is executed.
"""
import hashlib
import json
from pathlib import Path
import re
import runpy
import sys

BASE_PATH = Path(__file__).with_name("script_selector_census.py")
BASE = runpy.run_path(str(BASE_PATH))
HELPERS = BASE["HELPERS"]
HEAD = re.compile(r"([0-9A-F]{4}): ([0-9A-F]{2})(?: (.*))?", re.DOTALL)
MAX_NODES = 262144


def census(raw, config):
    keys = {"index", "opcode", "arity", "expectedNodes", "expectedSelected", "positiveControls"}
    if not isinstance(config, dict) or set(config) != keys:
        raise ValueError("Expected exact nested-census configuration fields")
    index = config["index"]
    if not isinstance(index, dict) or set(index) != {"listingSha256", "controls"}:
        raise ValueError("Expected base index hash and controls")
    validated = BASE["census"](raw, index)
    integer = HELPERS["integer"]
    opcode = integer(config["opcode"], 0, 128, "opcode")
    arity = integer(config["arity"], 1, 8, "arity")
    total = integer(config["expectedNodes"], 1, MAX_NODES, "expectedNodes")
    selected = integer(config["expectedSelected"], 0, MAX_NODES, "expectedSelected")
    controls = config["positiveControls"]
    if not isinstance(controls, list) or not 1 <= len(controls) <= 64:
        raise ValueError("Expected one to sixty-four nested positive controls")
    control_keys = set()
    for control in controls:
        if not isinstance(control, dict) or set(control) != {"resource", "offset", "opcode"}:
            raise ValueError("Invalid positive control")
        key = (BASE["resource_name"](control["resource"]),
               integer(control["offset"], 0, 65535, "offset"),
               integer(control["opcode"], 0, 128, "control opcode"))
        if key in control_keys:
            raise ValueError("Duplicate positive control")
        control_keys.add(key)
    tops = []
    for line in raw.decode("utf-8-sig").splitlines():
        match = HELPERS["LINE"].fullmatch(line)
        if match:
            tops.append((match[1], int(match[2], 16), match[4] or ""))
    starts = {(resource, offset) for resource, offset, _ in tops}
    seen, observed, counts, rows = set(), set(), {}, []

    def visit(text, resource, parent, depth, end):
        cursor = 0
        while cursor < len(text):
            if text[cursor] == "}":
                raise ValueError("Unmatched nested closing brace")
            if text[cursor] != "{":
                cursor += 1
                continue
            if depth > 32:
                raise ValueError("Nested depth limit exceeded")
            stop, balance = cursor + 1, 1
            while stop < len(text) and balance:
                balance += (text[stop] == "{") - (text[stop] == "}")
                stop += 1
            if balance:
                raise ValueError("Unclosed nested instruction")
            match = HEAD.fullmatch(text[cursor + 1:stop - 1])
            if not match:
                raise ValueError("Malformed nested instruction head")
            offset, op = int(match[1], 16), int(match[2], 16)
            key = (resource, offset)
            if op > 128 or offset <= parent or offset >= end or key in starts or key in seen:
                raise ValueError("Invalid or duplicate nested instruction ownership")
            seen.add(key)
            if len(seen) > MAX_NODES:
                raise ValueError("Nested node limit exceeded")
            observed.add((resource, offset, op))
            counts[op] = counts.get(op, 0) + 1
            args = HELPERS["operands"](match[3] or "")
            if op == opcode:
                if len(args) != arity:
                    raise ValueError("Selected nested instruction has wrong arity")
                values = []
                for expression in args:
                    value = BASE["literal"](expression)
                    values.append({"expression": expression, "literal": value,
                                   "lowWord": None if value is None else value & 65535,
                                   "lowByte": None if value is None else value & 255})
                rows.append({"resource": resource, "offset": offset, "parent": parent,
                             "depth": depth, "operands": values})
            visit(match[3] or "", resource, offset, depth + 1, end)
            cursor = stop

    for i, (resource, offset, text) in enumerate(tops):
        end = tops[i + 1][1] if i + 1 < len(tops) and tops[i + 1][0] == resource else 65536
        visit(text, resource, offset, 1, end)
    if len(seen) != total or len(rows) != selected or not control_keys <= observed:
        raise ValueError("Nested counts or positive controls missed")
    return {"version": 1, "listingSha256": validated["listingSha256"],
            "toolSha256": hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
            "baseToolSha256": validated["toolSha256"], "parserSha256": validated["parserSha256"],
            "indexControls": validated["controls"], "decoderErrors": validated["decoderErrors"],
            "nodes": len(seen), "selected": len(rows), "positiveControls": controls,
            "opcodeCounts": {str(k): counts[k] for k in sorted(counts)}, "rows": rows,
            "limits": ["Printed nested instructions only; native admission is not established.",
                       "Top-level starts, undecoded gaps, changed streams and aliases remain separate.",
                       "Nonliteral operands are unknown; parent/depth metadata is not a runtime trace."]}


def main():
    if len(sys.argv) != 3:
        raise ValueError("Supply listing and configuration paths")
    raw = HELPERS["read_bounded"](sys.argv[1], HELPERS["MAX_LISTING"])
    config_raw = HELPERS["read_bounded"](sys.argv[2], 65536)
    result = census(raw, json.loads(config_raw.decode("utf-8-sig")))
    result["configSha256"] = hashlib.sha256(config_raw).hexdigest()
    encoded = json.dumps(result, indent=2)
    if len(encoded.encode("utf-8")) > 16 * 1024 * 1024:
        raise ValueError("Nested report exceeds output limit")
    print(encoded)


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, UnicodeError) as error:
        print(f"nested script census: {error}", file=sys.stderr)
        sys.exit(1)
