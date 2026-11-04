"""Classify indexed top-level 0x5E operands with explicit source-index controls.

Usage: python -I script_selector_census.py LISTING CONFIG_JSON
Config: listingSha256 and controls with total, sentinel, otherLiteral,
nonliteral, sentinelLiteralSecond, sentinelNonliteralSecond, errors.
Reports derived from licensed files stay in GAME_DIR. No script is executed.
The first-operand sentinel and low-word selector relation are FND-COMBAT-099;
the indexed-start and error controls come from FND-COMBAT-100.
"""
import hashlib
import json
from pathlib import Path
import re
import runpy
import sys


PARSER = Path(__file__).with_name("script_local_paths.py")
HELPERS = runpy.run_path(str(PARSER))
CONTROL_KEYS = {"total", "sentinel", "otherLiteral", "nonliteral",
                "sentinelLiteralSecond", "sentinelNonliteralSecond", "errors"}
MAX_REPORT = 16 * 1024 * 1024


def literal(text):
    result = HELPERS["literal"](text)
    if result is not None and not -(1 << 31) <= result < (1 << 31):
        raise ValueError("Literal outside signed expression width")
    return result


def resource_name(text):
    if not re.fullmatch(r"(?:GPL|MAS)/\d{1,5}", text):
        raise ValueError("Unrecognized indexed resource")
    tag, number = text.split("/")
    value = HELPERS["integer"](int(number), 0, 65535, "Resource number")
    if text != f"{tag}/{value}":
        raise ValueError("Noncanonical resource-number alias")
    return text


def census(raw, config):
    if (not isinstance(config, dict) or not {"listingSha256", "controls"} <= set(config)
            or set(config) - {"listingSha256", "controls", "operandQueries"}):
        raise ValueError("Expected listingSha256, controls and optional operandQueries only")
    sha = hashlib.sha256(raw).hexdigest()
    if config["listingSha256"] != sha:
        raise ValueError("Listing SHA-256 mismatch")
    controls = config["controls"]
    if not isinstance(controls, dict) or set(controls) != CONTROL_KEYS:
        raise ValueError("Incomplete or unknown control fields")
    for name, value in controls.items():
        HELPERS["integer"](value, 0, 262144, name)
    if controls["total"] == 0:
        raise ValueError("Empty census control is not admitted")
    queries = config.get("operandQueries", [])
    if not isinstance(queries, list) or len(queries) > 16 or ("operandQueries" in config and not queries):
        raise ValueError("operandQueries must contain one to sixteen queries")
    domains = {}
    for query in queries:
        if not isinstance(query, dict) or set(query) != {"opcode", "arity", "operand", "count"}:
            raise ValueError("Operand query needs opcode, arity, operand and count only")
        opcode = HELPERS["integer"](query["opcode"], 0, 128, "opcode")
        arity = HELPERS["integer"](query["arity"], 1, 8, "arity")
        HELPERS["integer"](query["operand"], 0, arity - 1, "operand")
        HELPERS["integer"](query["count"], 1, 262144, "count")
        if opcode in domains:
            raise ValueError("Duplicate operand query opcode")
        domains[opcode] = {"query": query, "rows": []}
    groups = {name: {"literalNonzeroLowWord": [], "literalZeroLowWord": [], "nonliteral": []}
              for name in ("sentinel", "otherLiteral", "nonliteral")}
    errors, error_keys, inline_keys, previous, instructions = [], set(), set(), {}, 0
    for line in raw.decode("utf-8-sig").splitlines():
        if not line:
            continue
        if len(line) > 131072:
            raise ValueError("Listing line exceeds limit")
        error = re.fullmatch(r"((?:GPL|MAS)/\d{1,5}) (?:(ERROR) )?stopped at 0x([0-9A-Fa-f]{1,4}): (.+)", line)
        if error:
            if "stopped at 0x" in error[4]:
                raise ValueError("Multiple stops in one decoder record are not supported")
            record = {"resource": resource_name(error[1]), "stoppedAt": int(error[3], 16)}
            resumed = re.search(r"resumed at the literal target 0x([0-9A-Fa-f]{1,4})", error[4])
            record["resumedAt"] = int(resumed[1], 16) if resumed else None
            key = (record["resource"], record["stoppedAt"], record["resumedAt"])
            if not error[2]:
                if key in inline_keys:
                    raise ValueError("Duplicate inline decoder error")
                inline_keys.add(key)
                if len(inline_keys) > 65536:
                    raise ValueError("Inline decoder error count exceeds limit")
                continue
            if key in error_keys:
                raise ValueError("Duplicate decoder error record")
            error_keys.add(key)
            errors.append(record)
            if len(errors) > 65536:
                raise ValueError("Decoder error count exceeds limit")
            continue
        match = HELPERS["LINE"].fullmatch(line)
        if not match or not re.fullmatch(r"(?:GPL|MAS)/\d{1,5}", match[1]):
            raise ValueError("Malformed indexed instruction or decoder-error line")
        resource, at = resource_name(match[1]), int(match[2], 16)
        if int(match[3], 16) > 0x80:
            raise ValueError("Opcode outside supported indexed instruction set")
        if at <= previous.get(resource, -1):
            raise ValueError("Duplicate or unsorted indexed instruction")
        previous[resource] = at
        instructions += 1
        if instructions > 262144:
            raise ValueError("Instruction count exceeds limit")
        domain = domains.get(int(match[3], 16))
        if domain is not None:
            query = domain["query"]
            selected = HELPERS["operands"](match[4] or "")
            if len(selected) != query["arity"]:
                raise ValueError("Operand query encountered wrong instruction arity")
            expression = selected[query["operand"]]
            value = literal(expression)
            domain["rows"].append({"resource": resource, "offset": at,
                                   "expression": expression, "literal": value,
                                   "lowWord": None if value is None else value & 65535,
                                   "lowByte": None if value is None else value & 255})
        if match[3] != "5E":
            continue
        operands = HELPERS["operands"](match[4] or "")
        if len(operands) != 5:
            raise ValueError("Opcode 0x5E needs exactly five indexed operands")
        first, second = literal(operands[0]), literal(operands[1])
        group = "sentinel" if first == 32766 else "otherLiteral" if first is not None else "nonliteral"
        kind = "nonliteral" if second is None else "literalZeroLowWord" if second & 65535 == 0 else "literalNonzeroLowWord"
        groups[group][kind].append({"resource": resource, "offset": at,
                                    "first": operands[0], "second": operands[1],
                                    "firstLiteral": first, "secondLiteral": second,
                                    "selectorLowWord": None if second is None else second & 65535})
    if not inline_keys <= error_keys:
        raise ValueError("Inline decoder error has no matching final summary")
    actual = {name: sum(len(rows) for rows in group.values()) for name, group in groups.items()}
    actual["total"] = sum(actual.values())
    actual["sentinelLiteralSecond"] = len(groups["sentinel"]["literalNonzeroLowWord"]) + len(groups["sentinel"]["literalZeroLowWord"])
    actual["sentinelNonliteralSecond"] = len(groups["sentinel"]["nonliteral"])
    actual["errors"] = len(errors)
    if actual != controls:
        raise ValueError(f"Census positive controls differ from the explicit baseline: {actual}")
    for domain in domains.values():
        rows = domain["rows"]
        if len(rows) != domain["query"]["count"]:
            raise ValueError("Operand query positive count missed")
        values = [row["literal"] for row in rows if row["literal"] is not None]
        domain["counts"] = {"literal": len(values), "nonliteral": len(rows) - len(values),
                            "literalLowByteHighBitSet": sum(bool(value & 128) for value in values)}
        domain["literalRange"] = [min(values), max(values)] if values else None
    result = {"version": 1, "listingSha256": sha,
            "toolSha256": hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
            "parserSha256": hashlib.sha256(PARSER.read_bytes()).hexdigest(),
            "controls": actual, "indexedInstructions": instructions, "decoderErrors": errors,
            "inlineErrorEchoes": len(inline_keys),
            "selectorCounts": {name: {kind: len(rows) for kind, rows in group.items()} for name, group in groups.items()},
            "groups": groups,
            "scope": "Decoded top-level opcode 0x5E starts in one fingerprinted retained index.",
            "limits": ["Nested instructions and undecoded spans are not additional classified starts.",
                       "A literal selector is classified by its low word, not by a runtime observation.",
                       "Runtime entry, decoder/parameter preservation, callbacks and native effects remain unproved.",
                       "A nonliteral operand is unknown, never an implicit zero or sentinel.",
                       "Matching source-index controls does not establish a complete reading or native absence."]}
    if queries:
        result["operandDomains"] = list(domains.values())
        result["scope"] += " Count-controlled operand queries cover only their indexed top-level starts."
    return result


def main():
    if len(sys.argv) != 3:
        raise ValueError("Supply listing and control-config JSON paths")
    raw = HELPERS["read_bounded"](sys.argv[1], HELPERS["MAX_LISTING"])
    config_raw = HELPERS["read_bounded"](sys.argv[2], 65536)
    result = census(raw, json.loads(config_raw.decode("utf-8-sig")))
    result["configSha256"] = hashlib.sha256(config_raw).hexdigest()
    encoded = json.dumps(result, indent=2)
    if len(encoded.encode("utf-8")) > MAX_REPORT:
        raise ValueError("Census report exceeds output limit")
    print(encoded)


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, UnicodeError) as error:
        print(f"script selector census: {error}", file=sys.stderr)
        sys.exit(1)
