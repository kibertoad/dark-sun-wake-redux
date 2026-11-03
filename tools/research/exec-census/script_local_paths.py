"""Bounded local-call assignment histories in a script_decode.py --all index.

Usage: python -I script_local_paths.py LISTING CONFIG_JSON
Config: resource, roots (decimal offsets), target, local, listingSha256;
optional maxStates (1..100000), maxDepth (1..49). Reports derived from the
original stay in GAME_DIR. This reads text, never executes a script.

The graph follows RULE-SCRIPT-002/003 local calls and returns, takes both
conditional outcomes, and stops at cross-script or unresolved transfers.
Other instruction continuations assume the listed code/frame remains current.
Histories describe direct named assignments, not runtime values, alias safety,
condition feasibility, error/callback effects or local-storage preservation.
"""
import hashlib
import json
from pathlib import Path
import re
import sys


MAX_LISTING = 32 * 1024 * 1024
LINE = re.compile(r"^([A-Z]+/\d+) ([0-9A-F]{4}): ([0-9A-F]{2})(?: (.*))?$")
CONDITIONAL = {0x27: 1, 0x29: 0, 0x3E: 0, 0x3F: 0, 0x63: 0}
STOPS = {0x26, 0x31, 0x4A, 0x4C, 0x4D, 0x4E, 0x53, 0x55, 0x56,
         0x57, 0x60, 0x71, 0x72, 0x73, 0x74, 0x75}
CONTROL = set(CONDITIONAL) | STOPS | {0x0E, 0x12, 0x13, 0x15, 0x17,
                                     0x18, 0x19, 0x48, 0x61, 0x64, 0x67}


def integer(value, low, high, name):
    if type(value) is not int or not low <= value <= high:
        raise ValueError(f"{name} must be an integer in {low}..{high}")
    return value


def literal(value):
    if not re.fullmatch(r"-?\d+", value or ""):
        return None
    if len(value) > 12:
        raise ValueError("Oversized numeric operand")
    return int(value)


def operands(value):
    result, current, depth = [], [], 0
    for char in value:
        if char in "({":
            depth += 1
        elif char in ")}":
            depth -= 1
        if depth < 0 or depth > 64:
            raise ValueError("Unbalanced or excessively nested operands")
        if char == "," and depth == 0:
            result.append("".join(current).strip())
            current = []
        else:
            current.append(char)
    if depth:
        raise ValueError("Unbalanced operands")
    if current:
        result.append("".join(current).strip())
    return result


def analyze(raw, config):
    allowed = {"resource", "roots", "target", "local", "listingSha256", "maxStates", "maxDepth"}
    if not isinstance(config, dict) or set(config) - allowed:
        raise ValueError("Invalid config or unknown config field")
    digest = hashlib.sha256(raw).hexdigest()
    if config.get("listingSha256") != digest:
        raise ValueError("Listing SHA-256 mismatch")
    resource = config.get("resource")
    if not isinstance(resource, str) or not re.fullmatch(r"(?:GPL|MAS)/\d{1,5}", resource):
        raise ValueError("Expected one GPL/NUMBER or MAS/NUMBER resource")
    integer(int(resource.split("/")[1]), 0, 65535, "Resource number")
    target = integer(config.get("target"), 0, 65535, "Target")
    local = integer(config.get("local"), 0, 65535, "Local number")
    limit = integer(config.get("maxStates", 20000), 1, 100000, "State limit")
    depth_limit = integer(config.get("maxDepth", 32), 1, 49, "Local call depth limit")
    roots = config.get("roots")
    if not isinstance(roots, list) or not 1 <= len(roots) <= 64:
        raise ValueError("Expected 1..64 declared roots")
    for root in roots:
        integer(root, 0, 65535, "Root")
    if len(set(roots)) != len(roots):
        raise ValueError("Duplicate root")
    code, errors = {}, []
    previous = -1
    for line in raw.decode("utf-8-sig").splitlines():
        if len(line) > 131072:
            raise ValueError("Listing line exceeds limit")
        if line.startswith(resource + " ERROR "):
            error = re.search(r"stopped at 0x([0-9A-Fa-f]{1,4}):", line)
            if not error:
                raise ValueError("Unrecognized selected-resource error record")
            errors.append(int(error[1], 16))
        match = LINE.fullmatch(line)
        if not match and line.startswith(resource + " ") and not line.startswith(resource + " ERROR "):
            raise ValueError("Malformed selected-resource listing line")
        if not match or match[1] != resource:
            continue
        at, opcode, text = int(match[2], 16), int(match[3], 16), match[4] or ""
        if at <= previous:
            raise ValueError("Duplicate or unsorted selected-resource instruction")
        code[at] = (opcode, text)
        previous = at
    if not code or any(root not in code for root in roots):
        raise ValueError("Empty selected resource or missing declared root")
    if target not in code or code[target][0] != 0x5E:
        raise ValueError("Target must be a listed opcode 0x5E instruction")
    ordered = list(code)
    following = dict(zip(ordered, ordered[1:]))
    results, assignments, effect_sites, total_states = [], {}, set(), 0
    for root in roots:
        remaining = limit - total_states
        if not remaining:
            results.append({"root": root, "visitedStates": 0, "targetHistories": [],
                            "unresolved": [{"offset": root, "reason": "state-limit"}]})
            continue
        pending, seen, hits, gaps = [(root, (), None, False)], set(), set(), set()
        discovered = set(pending)

        def enqueue(at, stack, writer, effects):
            if at not in code:
                gaps.add((at, "missing-instruction-target"))
            else:
                candidate = (at, stack, writer, effects)
                if candidate not in discovered:
                    if len(discovered) >= remaining:
                        gaps.add((at, "state-limit"))
                    else:
                        discovered.add(candidate)
                        pending.append(candidate)

        def next_at(at):
            nxt = following.get(at)
            if nxt is None or any(at < error <= nxt for error in errors):
                return -1
            return nxt

        def advance(at, stack, writer, effects):
            nxt = next_at(at)
            if nxt == -1:
                gaps.add((at, "missing-continuation-or-rejected-span"))
            else:
                enqueue(nxt, stack, writer, effects)

        while pending:
            state = pending.pop()
            if state in seen:
                continue
            seen.add(state)
            at, stack, writer, effects = state
            if at == target:
                hits.add((writer, effects))
                continue
            op, text = code[at]
            nested = re.findall(r"\{[0-9A-F]{4}: ([0-9A-F]{2})", text)
            if any(int(value, 16) in CONTROL | {0x14} for value in nested):
                gaps.add((at, "nested-control-instruction"))
                continue
            if op > 0x80:
                gaps.add((at, "unsupported-opcode"))
                continue
            if op == 0x14:
                gaps.add((at, "cross-script-call"))
                continue
            if op == 0x19:
                gaps.add((at, "return-to-unmodeled-script-context"))
                continue
            if op in STOPS:
                continue
            if op == 0x15:
                if not stack:
                    gaps.add((at, "return-to-unmodeled-local-frame"))
                elif stack[-1] == -1:
                    gaps.add((at, "missing-return-continuation-or-rejected-span"))
                else:
                    enqueue(stack[-1], stack[:-1], writer, effects)
                continue
            if op == 0x16 and " -> " in text:
                value, destination = text.rsplit(" -> ", 1)
                if destination == f"var2[{local}]":
                    writer, effects = at, bool(nested)
                    assignments[at] = {"offset": at, "literal": literal(value),
                                       "kind": "literal" if literal(value) is not None else "expression"}
                    if nested:
                        effect_sites.add(at)
                else:
                    effects = True
                    effect_sites.add(at)
            elif op == 0x16:
                gaps.add((at, "unreadable-assignment"))
                continue
            elif op not in CONTROL or nested:
                effects = True
                effect_sites.add(at)
            if op == 0x48:
                effects = True
                effect_sites.add(at)
            args = operands(text)
            if op in {0x12, 0x13, 0x64} | set(CONDITIONAL):
                index = CONDITIONAL.get(op, 0)
                if len(args) != (2 if op == 0x27 else 1):
                    gaps.add((at, "wrong-control-operand-count"))
                    continue
                dest = literal(args[index]) if len(args) > index else None
                if dest is None:
                    gaps.add((at, "computed-or-missing-transfer"))
                elif op == 0x13:
                    if len(stack) >= depth_limit:
                        gaps.add((at, "local-call-depth-limit"))
                    else:
                        enqueue(dest & 65535, stack + (next_at(at),), writer, effects)
                else:
                    enqueue(dest & 65535, stack, writer, effects)
                if op in CONDITIONAL:
                    advance(at, stack, writer, effects)
            elif op == 0x48:
                rows = re.findall(r"row\((.*?)\)(?:;|$)", text)
                if not rows:
                    gaps.add((at, "menu-without-readable-rows"))
                    continue
                if len(rows) >= 12:
                    gaps.add((at, "menu-frame-array-overlap-not-modeled"))
                    continue
                if len(stack) >= depth_limit:
                    gaps.add((at, "local-call-depth-limit"))
                    continue
                gaps.add((at, "menu-availability-and-selection-not-admitted"))
                for row in rows:
                    parts = operands(row)
                    dest = literal(parts[1]) if len(parts) == 3 else None
                    if dest is None:
                        gaps.add((at, "computed-or-missing-menu-target"))
                    else:
                        enqueue(dest & 65535, stack + (next_at(at),), writer, effects)
            else:
                advance(at, stack, writer, effects)
        total_states += len(seen)
        results.append({"root": root, "visitedStates": len(seen),
                        "targetHistories": [{"assignment": writer, "otherEffectsSinceAssignment": effects}
                                            for writer, effects in sorted(hits, key=lambda x: (-1 if x[0] is None else x[0], x[1]))],
                        "unresolved": [{"offset": at, "reason": reason} for at, reason in sorted(gaps)]})
    return {"version": 1, "resource": resource, "target": target, "local": local,
            "listingSha256": digest, "toolSha256": hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
            "maxStates": limit, "stateLimitScope": "All declared roots combined",
            "maxDepth": depth_limit, "decodeErrors": errors,
            "assignments": [assignments[at] for at in sorted(assignments)],
            "otherEffectSites": sorted(effect_sites), "roots": results,
            "scope": "First target on candidate paths in one fixed listed script, with matched local calls/returns.",
            "limits": ["Conditions and menu availability are not evaluated.",
                       "Menu selections use candidate local frames; selection and native UI effects remain unresolved.",
                       "Opcode callbacks, error effects, aliases and storage/frame lifetime are not established.",
                       "Other-effect continuations assume the same listing/frame; cross-script calls stop.",
                       "No known assignment is unknown, never an implicit zero; histories are not preserved values.",
                       "Declared roots and model caps do not establish runtime admission or complete readings."]}


def read_bounded(path, limit):
    with open(path, "rb") as source:
        result = source.read(limit + 1)
    if len(result) > limit:
        raise ValueError("Input exceeds byte limit")
    return result


def main():
    if len(sys.argv) != 3:
        raise ValueError("Supply listing and config JSON paths")
    raw = read_bounded(sys.argv[1], MAX_LISTING)
    config_raw = read_bounded(sys.argv[2], 65536)
    result = analyze(raw, json.loads(config_raw.decode("utf-8-sig")))
    result["configSha256"] = hashlib.sha256(config_raw).hexdigest()
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, UnicodeError) as error:
        print(f"script local paths: {error}", file=sys.stderr)
        sys.exit(1)
