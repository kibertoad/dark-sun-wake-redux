"""Bounded source-label metadata for Q-SOUND-010; FMT-CONFIG-002.

Usage: python -I sound_ini_profile.py APPROVED_SOUND_INI
Reports no names and does not model the original parser or run a program.
"""
import json
import re
import sys


GROUPS = ("MainGroup", "DigitalGroup", "MusicOnlyGroup")
GROUP_KEYS = {name.lower(): name for name in GROUPS}


def profile(text):
    if not isinstance(text, str) or len(text.encode("ascii")) > 128 * 1024:
        raise ValueError("SOUND.INI profile: unsupported text encoding/size")
    groups, current, record, done = {}, None, None, False
    for line_number, raw in enumerate(text.splitlines(), 1):
        if len(raw) > 4096:
            raise ValueError("SOUND.INI profile: line limit")
        line = raw.split(";", 1)[0].strip()
        if not line or line.startswith("//"):
            continue
        if done:
            raise ValueError("SOUND.INI profile: content after EndOfFile")
        match = re.fullmatch(r"\[([A-Za-z][A-Za-z0-9]*)\]\s*(.*)", line)
        if not match:
            raise ValueError(f"SOUND.INI profile: unsupported line {line_number}")
        key, value = match.group(1).lower(), match.group(2).strip()
        if key in GROUP_KEYS:
            if current is not None or value or key in groups:
                raise ValueError("SOUND.INI profile: nested/duplicate group")
            current = {"name": GROUP_KEYS[key], "declared": None, "lengths": [], "printable": True}
            groups[key] = current
        elif key == "numberofrecords":
            if current is None or record is not None or current["declared"] is not None or not re.fullmatch(r"-?\d{1,5}", value):
                raise ValueError("SOUND.INI profile: count position/value")
            count = int(value)
            if not -1 <= count <= 256:
                raise ValueError("SOUND.INI profile: count outside tool bounds")
            current["declared"] = count
        elif key == "startrecord":
            if current is None or current["declared"] is None or record is not None or value or len(current["lengths"]) >= 256:
                raise ValueError("SOUND.INI profile: record boundary/limit")
            record = {}
        elif key == "cardname":
            found = re.fullmatch(r"\[([^\[\]]*)\]", value)
            if record is None or "length" in record or not found:
                raise ValueError("SOUND.INI profile: missing/duplicate/nonbracketed name")
            name = found.group(1)
            if len(name) > 256:
                raise ValueError("SOUND.INI profile: name limit")
            record = {"length": len(name), "printable": all(32 <= ord(char) <= 126 for char in name)}
        elif key == "endrecord":
            if current is None or record is None or "length" not in record or value:
                raise ValueError("SOUND.INI profile: incomplete record")
            current["lengths"].append(record["length"])
            current["printable"] &= record["printable"]
            record = None
        elif key == "endofgroup":
            if current is None or record is not None or current["declared"] is None or value:
                raise ValueError("SOUND.INI profile: incomplete group")
            expected = max(0, current["declared"])
            if len(current["lengths"]) != expected:
                raise ValueError("SOUND.INI profile: declared/observed count mismatch")
            current = None
        elif key == "endoffile":
            if current is not None or record is not None or value:
                raise ValueError("SOUND.INI profile: premature EndOfFile")
            done = True
        # Other key/value semantics are deliberately outside this source-label query.
    if not done or current is not None or record is not None or set(groups) != set(GROUP_KEYS):
        raise ValueError("SOUND.INI profile: missing groups/terminator")
    rows = []
    for group in GROUPS:
        state = groups[group.lower()]
        lengths, printable = state["lengths"], state["printable"]
        count, maximum = len(lengths), max(lengths, default=0)
        rows.append({"group": group, "declaredCount": state["declared"], "records": count,
                     "maxNameBytes": maximum, "allPrintableAscii": printable,
                     "textLabelSlotsBytes": count * 80,
                     "joinedTextFootprint": sum(lengths) + count + 1 if count else None,
                     "smallPositivePrintableProfile": 1 <= count <= 21 and maximum <= 40 and printable})
    return {"groups": rows, "limits": "source labels only; other key semantics and native parser/state/callee outcomes unvalidated"}


def main():
    if len(sys.argv) != 2:
        raise SystemExit("usage: sound_ini_profile.py APPROVED_SOUND_INI")
    import xxhash
    with open(sys.argv[1], "rb") as stream:
        body = stream.read(128 * 1024 + 1)
    digest = xxhash.xxh3_128_hexdigest(body)
    if (len(body), digest) != (54669, "aecc7469098e0c188e0e3d056c5fd530"):
        raise ValueError("SOUND.INI profile: not the approved installed source")
    print(json.dumps({"sourceIdentity": {"bytes": len(body), "xxh3": digest}, **profile(body.decode("ascii"))}, indent=2))


if __name__ == "__main__":
    main()
