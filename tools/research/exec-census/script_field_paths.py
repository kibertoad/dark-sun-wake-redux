"""Bounded field-path selector census of an existing indexed script listing."""
import collections
import hashlib
import json
from pathlib import Path
import re
import sys

LINE = re.compile(r"((?:GPL|MAS)/\d{1,5}) ([0-9A-F]{4}): ([0-9A-F]{2})(?: (.*))?")
PATH = re.compile(r"path\(([0-9A-F]{4})(?:; ([0-9,]*))?\)")
GAP = re.compile(r"((?:GPL|MAS)/\d{1,5}) (?:(ERROR) )?stopped at 0x([0-9A-Fa-f]{1,4}): (.+)")
KEYS = {0x8025, 0x8026, 0x8027, 0x8028, 0x802B, 0x802C}


def integer(value, low, high):
    if type(value) is not int or not low <= value <= high:
        raise ValueError("field census control outside bounds")
    return value


def resource_name(value):
    tag, number = value.split("/")
    if str(int(number)) != number or not 0 <= int(number) <= 65535:
        raise ValueError("noncanonical indexed resource")
    return value


def census(raw, controls):
    required = {"listingSha256", "paths", "emptyLists", "omittedLists", "gaps", "targetField", "positiveFields"}
    if not isinstance(controls, dict) or set(controls) != required or not 0 < len(raw) <= 32 * 1024 * 1024:
        raise ValueError("field census inputs or controls differ")
    if hashlib.sha256(raw).hexdigest() != controls["listingSha256"]:
        raise ValueError("field listing SHA-256 mismatch")
    integer(controls["targetField"], 0, 255)
    for key in ("paths", "emptyLists", "omittedLists", "gaps"):
        integer(controls[key], 1 if key == "paths" else 0, 262144)
    positive = controls["positiveFields"]
    if not isinstance(positive, dict) or not positive:
        raise ValueError("positive field controls required")
    for field, count in positive.items():
        if not isinstance(field, str) or not field.isdecimal() or str(int(field)) != field:
            raise ValueError("noncanonical positive selector")
        integer(int(field), 0, 255)
        integer(count, 1, 262144)
    fields, previous, inline, final = collections.Counter(), {}, set(), set()
    paths = empty = omitted = 0
    for line in raw.decode("utf-8-sig").splitlines():
        if not line:
            continue
        if len(line) > 131072:
            raise ValueError("field listing line exceeds bound")
        gap = GAP.fullmatch(line)
        if gap:
            resumed = re.search(r"resumed at the literal target 0x([0-9A-Fa-f]{1,4})", gap[4])
            key = (resource_name(gap[1]), int(gap[3], 16), int(resumed[1], 16) if resumed else None)
            target = final if gap[2] else inline
            if key in target:
                raise ValueError("duplicate field decoder gap")
            target.add(key)
            continue
        match = LINE.fullmatch(line)
        if not match or int(match[3], 16) > 0x80:
            raise ValueError("invalid indexed field instruction")
        resource, offset = resource_name(match[1]), int(match[2], 16)
        if offset <= previous.get(resource, -1):
            raise ValueError("duplicate or unsorted field instruction")
        previous[resource] = offset
        text = match[4] or ""
        hits = list(PATH.finditer(text))
        if len(hits) != text.count("path("):
            raise ValueError("malformed field path")
        for path in hits:
            head = int(path[1], 16)
            values = path[2]
            if values is None:
                if head < 0x8000 or head in KEYS:
                    raise ValueError("required field list omitted")
                omitted += 1
            else:
                if head >= 0x8000 and head not in KEYS:
                    raise ValueError("unexpected field list")
                if not values:
                    empty += 1
                elif len(values.split(",")) > 255:
                    raise ValueError("field list exceeds byte count")
                for value in values.split(",") if values else ():
                    if not value.isdecimal() or str(int(value)) != value:
                        raise ValueError("noncanonical field selector")
                    fields[integer(int(value), 0, 255)] += 1
            paths += 1
            if paths > 262144:
                raise ValueError("field path count exceeds bound")
    if inline != final:
        raise ValueError("field decoder gap diagnostics differ")
    actual = {"paths": paths, "emptyLists": empty, "omittedLists": omitted, "gaps": len(final)}
    if any(actual[key] != controls[key] for key in actual) or any(fields[int(k)] != v for k, v in positive.items()):
        raise ValueError("field census missed controls")
    return actual | {"fieldOccurrences": dict(sorted(fields.items())), "targetField": controls["targetField"],
                     "targetOccurrences": fields[controls["targetField"]],
                     "decoderGaps": [{"resource": r, "stoppedAt": at, "resumedAt": end}
                                     for r, at, end in sorted(final, key=lambda x: (x[0], x[1]))]}


def main():
    if len(sys.argv) != 3:
        raise SystemExit("usage: script_field_paths.py LISTING CONTROLS_JSON")
    with open(sys.argv[1], "rb") as stream:
        raw = stream.read(32 * 1024 * 1024 + 1)
    with open(sys.argv[2], "rb") as stream:
        config = stream.read(65537)
    if len(config) > 65536:
        raise ValueError("field controls exceed bound")
    print(json.dumps(census(raw, json.loads(config))))


if __name__ == "__main__":
    main()
