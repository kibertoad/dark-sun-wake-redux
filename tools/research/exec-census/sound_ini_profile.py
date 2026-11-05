"""Bounded source-label metadata for Q-SOUND-010; FMT-CONFIG-002.

Usage: python -I sound_ini_profile.py APPROVED_SOUND_INI [--spans|--inputs|--contexts]
Reports no device names; source conformance is not native parser execution.
"""
import json
import re
import sys


GROUPS = ("MainGroup", "DigitalGroup", "MusicOnlyGroup")
GROUP_KEYS = {name.lower(): name for name in GROUPS}
STRING_FIELDS = ("CardName", "RmDigitalDriverName", "RmMidiDriverName",
                 "PmDigitalDriverName", "PmMidiDriverName")
ARRAY_FIELDS = (("NumberOfMidiAddresses", "MidiAddress", "MidiAltAddress"),
                ("NumberOfDigitalAddresses", "DigitalAddress", "DigitalAltAddress"),
                ("NumberOfMidiIrqs", "MidiIrq", "MidiAltIrq"),
                ("NumberOfIrqs", "Irq", "AltIrq"),
                ("NumberOfDmas", "Dma", "AltDma"))
NUMERIC_FIELDS = ("NumberOfRecords", "CardId", "CardGroup", "MidiIrq", "MidiAltIrq",
                  "DigitalDriverChunkNumber", "MidiDriverChunkNumber",
                  "SamplePlaybackMode", "Flags", "Jumpers") + tuple(
                      name for family in ARRAY_FIELDS for name in family)
NUMERIC_KEYS = {name.lower(): name for name in NUMERIC_FIELDS}
FLAG_PROFILE = {name.lower() for name in ("DspEnabled", "DspNotEnabled",
                "MidiNotEnabled", "CdEnabled", "MultipleInstallDisabled")}
JUMPER_PROFILE = {name.lower() for name in ("MidiAddressJumper", "MidiIrqJumper",
                  "DigitalAddressJumper", "DigitalIrqJumper", "DigitalDmaJumper",
                  "AddressJumper", "IrqJumper", "DmaJumper")}
# FND-SOUND-037: exact installed table absence, consumed on the nonpositive
# nested-jumper path. Other unclassified source names stay outside this profile.
IGNORED_JUMPER_PROFILE = {"mididmajumper"}


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


def source_spans(text):
    """Source measurements only; not a replay of the native parser."""
    profile(text)
    rows, group, fields = [], None, None
    for raw in text.splitlines():
        line = raw.split(";", 1)[0].strip()
        if not line or line.startswith("//"):
            continue
        match = re.fullmatch(r"\[([A-Za-z][A-Za-z0-9]*)\]\s*(.*)", line)
        key, value = match.group(1).lower(), match.group(2).strip()
        if key in GROUP_KEYS:
            group = GROUP_KEYS[key]
        elif key == "startrecord":
            fields = {}
        elif key == "endrecord":
            strings, arrays = [], []
            for name in STRING_FIELDS:
                lengths = []
                for item in fields.get(name.lower(), []):
                    found = re.fullmatch(r"\[([^\[\]]*)\]", item)
                    if not found or len(found.group(1)) > 256:
                        raise ValueError("SOUND.INI spans: string syntax/limit")
                    lengths.append(len(found.group(1)))
                strings.append({"field": name, "nameBytes": lengths,
                                "terminatedBytes": [length + 1 for length in lengths]})
            for count_key, primary, alternate in ARRAY_FIELDS:
                counts = []
                for item in fields.get(count_key.lower(), []):
                    if not re.fullmatch(r"-?(?:0[xX][0-9A-Fa-f]{1,4}|[0-9]{1,5})", item):
                        raise ValueError("SOUND.INI spans: count syntax/limit")
                    negative = item.startswith("-")
                    magnitude = item[1:] if negative else item
                    number = int(magnitude, 16 if magnitude.lower().startswith("0x") else 10)
                    number = -number if negative else number
                    if not -32768 <= number <= 65535:
                        raise ValueError("SOUND.INI spans: count outside tool bounds")
                    counts.append(number)
                arrays.append({"field": count_key, "declaredValues": counts,
                               "primaryOccurrences": len(fields.get(primary.lower(), [])),
                               "alternateOccurrences": len(fields.get(alternate.lower(), []))})
            rows.append({"group": group, "ordinal": len(rows),
                         "sourceKeys": sum(len(items) for items in fields.values()),
                         "strings": strings, "arrays": arrays})
            fields = None
        elif fields is not None:
            if sum(len(items) for items in fields.values()) >= 512:
                raise ValueError("SOUND.INI spans: per-record key limit")
            fields.setdefault(key, []).append(value)
    return {"records": rows, "limits": "source key/value lengths and occurrences only; no native parsing, conversion, storage or retention guarantee"}


def source_inputs(text):
    """Bounded source geometry/spellings; no native parser or conversion replay."""
    profile(text)
    lines = text.split("\n")
    if text.endswith("\n"):
        lines.pop()
    if len(lines) > 4096:
        raise ValueError("SOUND.INI inputs: line count limit")
    numeric, ignored_brackets, controls = [], [], []
    max_line = max_copy = max_tag = 0
    bracket_lines = 0
    for number, raw in enumerate(lines, 1):
        line = raw[:-1] if raw.endswith("\r") else raw
        max_line = max(max_line, len(line))
        if any(ord(char) < 32 and char != "\t" or ord(char) == 127 for char in line):
            controls.append(number)
        start = line.find("[")
        if start < 0:
            continue
        bracket_lines += 1
        # Binary delivery preserves the CR before LF unless a semicolon
        # already stopped copying. Physical width excludes the CRLF ending.
        copied = raw[start:].split(";", 1)[0]
        max_copy = max(max_copy, len(copied))
        end = copied.find("]")
        if end >= 0:
            max_tag = max(max_tag, end - 1)
        source_line = line.split(";", 1)[0].strip()
        if not source_line or source_line.startswith("//"):
            ignored_brackets.append(number)
            continue
        match = re.fullmatch(r"\[([A-Za-z][A-Za-z0-9]*)\]\s*(.*)", source_line)
        if match is None:
            raise ValueError("SOUND.INI inputs: unsupported physical line syntax")
        key, value = match.group(1).lower(), match.group(2).strip()
        if key not in NUMERIC_KEYS:
            continue
        if len(value) > 64 or not re.fullmatch(r"-?(?:0[xX][0-9A-Fa-f]+|[0-9]+)", value):
            raise ValueError("SOUND.INI inputs: numeric syntax/limit")
        magnitude = value.removeprefix("-")
        base = 16 if magnitude.lower().startswith("0x") else 10
        prefix = re.match(r"[0-9xX-]*", value).group()
        numeric.append({"line": number, "field": NUMERIC_KEYS[key],
                        "sourceBytes": len(value), "sourceBase": base,
                        "sourceValue": (-1 if value.startswith("-") else 1) * int(magnitude, base),
                        "extractorAlphabetPrefixBytes": len(prefix),
                        "wholeSpellingInExtractorAlphabet": len(prefix) == len(value)})
    return {"physicalLines": len(lines), "terminalLf": text.endswith("\n"),
            "maxPhysicalLineBytes": max_line, "bracketBearingLines": bracket_lines,
            "maxBracketToCommentBytes": max_copy, "maxFirstTagBytes": max_tag,
            "sourceIgnoredBracketLines": ignored_brackets, "controlByteLines": controls,
            "numericRows": numeric,
            "limits": "source spans/spellings only; numeric prefix alphabet is not a conversion result; native contexts, storage and continuation unvalidated"}


def source_contexts(text):
    """Validate only the declared consecutive-key source profile, not native state."""
    inputs = source_inputs(text)
    if not inputs["terminalLf"] or inputs["sourceIgnoredBracketLines"] or inputs["controlByteLines"]:
        raise ValueError("SOUND.INI contexts: source delivery outside profile")
    count_keys = {family[0].lower(): family[0] for family in ARRAY_FIELDS}
    alternate_keys = {family[2].lower(): family[0].lower() for family in ARRAY_FIELDS}
    outer_keys = (set(NUMERIC_KEYS) - {"numberofrecords"}) | {key.lower() for key in STRING_FIELDS}
    records, group, entries = [], None, None

    def decimal(value, maximum):
        if not re.fullmatch(r"[0-9]{1,3}", value) or int(value) > maximum:
            raise ValueError("SOUND.INI contexts: count spelling/limit outside profile")
        return int(value)

    def record(rows):
        counts, seen_alternates, bands = {}, set(), []
        i = outer = flag_keys = jumper_keys = ignored_jumpers = alternate_count = 0
        while i < len(rows):
            line, key, value = rows[i]
            if key in alternate_keys:
                family = alternate_keys[key]
                count = counts.get(family, 0)
                if count < 2 or key in seen_alternates:
                    raise ValueError("SOUND.INI contexts: alternate order/repetition outside profile")
                # The first alternate is an outer key. Only the remaining
                # count-minus-two keys belong to its nested consecutive band.
                amount, kind, allowed = count - 2, count_keys[family], {key}
                start = i + 1
                seen_alternates.add(key)
                alternate_count += amount
                outer += 1
            elif key in ("flags", "jumpers"):
                amount = decimal(value, 256)
                kind = "Flags" if key == "flags" else "Jumpers"
                allowed = FLAG_PROFILE if key == "flags" else JUMPER_PROFILE | IGNORED_JUMPER_PROFILE
                start = i + 1
                outer += 1
                flag_keys += amount if key == "flags" else 0
                jumper_keys += amount if key == "jumpers" else 0
            else:
                if key not in outer_keys:
                    raise ValueError("SOUND.INI contexts: outer key outside profile")
                if key in count_keys:
                    if key in counts:
                        raise ValueError("SOUND.INI contexts: repeated array count outside profile")
                    counts[key] = decimal(value, 10)
                    if counts[key] == 0:
                        raise ValueError("SOUND.INI contexts: nonpositive array count outside profile")
                outer += 1
                i += 1
                continue
            children = rows[start:start + amount]
            if len(children) != amount or any(child[1] not in allowed for child in children):
                raise ValueError("SOUND.INI contexts: consecutive-key band outside profile")
            ignored = sum(child[1] in IGNORED_JUMPER_PROFILE for child in children) if key == "jumpers" else 0
            ignored_jumpers += ignored
            bands.append({"kind": kind, "line": line, "childKeys": amount,
                          "ignoredChildKeys": ignored,
                          "firstChildLine": children[0][0] if children else None,
                          "lastChildLine": children[-1][0] if children else None})
            i = start + amount
        for alternate, family in alternate_keys.items():
            if counts.get(family, 0) > 1 and alternate not in seen_alternates:
                raise ValueError("SOUND.INI contexts: missing alternate band outside profile")
        return {"group": group, "ordinal": len(records), "sourceKeys": len(rows),
                "outerKeys": outer, "flagKeys": flag_keys, "jumperKeys": jumper_keys,
                "ignoredJumperKeys": ignored_jumpers,
                "alternateKeys": alternate_count, "arrayCounts": {
                    count_keys[key]: value for key, value in counts.items()}, "bands": bands}

    for number, raw in enumerate(text.splitlines(), 1):
        line = raw.split(";", 1)[0].strip()
        if not line or line.startswith("//"):
            continue
        match = re.fullmatch(r"\[([A-Za-z][A-Za-z0-9]*)\]\s*(.*)", line)
        key, value = match.group(1).lower(), match.group(2).strip()
        if key in GROUP_KEYS:
            group = GROUP_KEYS[key]
        elif key == "startrecord":
            entries = []
        elif key == "endrecord":
            records.append(record(entries))
            entries = None
        elif entries is not None:
            if len(entries) >= 512:
                raise ValueError("SOUND.INI contexts: per-record key limit")
            entries.append((number, key, value))
    return {"records": records,
            "limits": "declared consecutive-key source profile only; no native parsing, storage, lifetime or successful setup guarantee"}


def main():
    modes = {"--spans": source_spans, "--inputs": source_inputs, "--contexts": source_contexts}
    if len(sys.argv) not in (2, 3) or (len(sys.argv) == 3 and sys.argv[2] not in modes):
        raise SystemExit("usage: sound_ini_profile.py APPROVED_SOUND_INI [--spans|--inputs|--contexts]")
    import xxhash
    with open(sys.argv[1], "rb") as stream:
        body = stream.read(128 * 1024 + 1)
    digest = xxhash.xxh3_128_hexdigest(body)
    if (len(body), digest) != (54669, "aecc7469098e0c188e0e3d056c5fd530"):
        raise ValueError("SOUND.INI profile: not the approved installed source")
    measure = modes[sys.argv[2]] if len(sys.argv) == 3 else profile
    print(json.dumps({"sourceIdentity": {"bytes": len(body), "xxh3": digest}, **measure(body.decode("ascii"))}, indent=2))


if __name__ == "__main__":
    main()
