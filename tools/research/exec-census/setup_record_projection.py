"""Local source-to-record comparison for Q-CONFIG-006/011 and Q-SOUND-010.

Research tooling from FMT-CONFIG-002 and FND-SOUND-034..038. This bounded
projection is not a generic native parser, gameplay implementation or asset pack.
Source-derived bytes must remain in GAME_DIR. No original strings are embedded.
"""
import re
import struct

from sound_ini_profile import GROUP_KEYS, ARRAY_FIELDS, profile, source_contexts

RECORD_SIZE = 254
GROUP_NAMES = {"MainGroup": "main", "DigitalGroup": "digital", "MusicOnlyGroup": "music_only"}
SCALARS = {"cardid": 0x28, "cardgroup": 0x2A,
           "numberofmidiaddresses": 0x2C, "midiaddress": 0x2E,
           "numberofdigitaladdresses": 0x42, "digitaladdress": 0x44,
           "numberofmidiirqs": 0x58, "midiirq": 0x5A,
           "numberofirqs": 0x6E, "irq": 0x70,
           "numberofdmas": 0x84, "dma": 0x86,
           "digitaldriverchunknumber": 0xD6, "mididriverchunknumber": 0xD8,
           "sampleplaybackmode": 0xDA}
STRINGS = {"cardname": (0, 0x28), "rmdigitaldrivername": (0x9A, 0xA9),
           "rmmididrivername": (0xA9, 0xB8), "pmdigitaldrivername": (0xB8, 0xC7),
           "pmmididrivername": (0xC7, 0xD6)}
ALTERNATES = {alternate.lower(): (count.lower(), SCALARS[primary.lower()])
              for count, primary, alternate in ARRAY_FIELDS}
FLAG_MASKS = {"dspenabled": 1, "midinotenabled": 8,
              "dspnotenabled": 0, "cdenabled": 0, "multipleinstalldisabled": 0}
JUMPER_MASKS = dict(zip(("midiaddressjumper", "midiirqjumper", "digitaladdressjumper",
                        "digitalirqjumper", "digitaldmajumper", "addressjumper",
                        "irqjumper", "dmajumper", "mididmajumper"),
                       (8, 16, 32, 64, 128, 1, 2, 4, 0)))


def numeric_word(value):
    """Only bounded canonical source spellings; failed scans have no invented default."""
    if not re.fullmatch(r"-?(?:0[xX][0-9A-Fa-f]{1,4}|[0-9]{1,10})", value):
        raise ValueError("setup projection: unsupported numeric spelling")
    prefix = re.match(r"[0-9xX-]+", value).group()
    if prefix.lower().startswith("0x"):
        number = int(prefix[2:] or "0", 16)
    else:
        decimal = re.match(r"-?[0-9]+", prefix)
        if decimal is None:
            raise ValueError("setup projection: unassigned numeric result")
        number = int(decimal.group(), 10)
    if abs(number) > 0xFFFFFFFF:
        return 0 if number < 0 else 0xFFFF
    return number & 0xFFFF


def _record(entries):
    body = bytearray(RECORD_SIZE)
    i = 0
    while i < len(entries):
        key, value = entries[i]
        if key in SCALARS:
            word = numeric_word(value)
            if key == "sampleplaybackmode" and word not in (0, 1):
                raise ValueError("setup projection: rejected playback mode")
            struct.pack_into("<H", body, SCALARS[key], word)
        elif key in STRINGS:
            match = re.fullmatch(r"\[([^\[\]]*)\]", value)
            if match is None:
                raise ValueError("setup projection: string syntax")
            encoded = match.group(1).encode("ascii") + bytes(1)
            begin, end = STRINGS[key]
            if len(encoded) > end - begin:
                raise ValueError("setup projection: string exceeds mapped gap")
            body[begin:begin + len(encoded)] = encoded
        elif key in ALTERNATES:
            count_key, base = ALTERNATES[key]
            count = struct.unpack_from("<H", body, SCALARS[count_key])[0]
            if not 2 <= count <= 10:
                raise ValueError("setup projection: alternate count outside profile")
            children = entries[i:i + count - 1]
            if len(children) != count - 1 or any(child[0] != key for child in children):
                raise ValueError("setup projection: alternate band mismatch")
            for index, (_, item) in enumerate(children, 1):
                struct.pack_into("<H", body, base + 2 * index, numeric_word(item))
            i += count - 1
            continue
        elif key in ("flags", "jumpers"):
            amount = int(value, 10)
            masks, offset = (FLAG_MASKS, 0xDC) if key == "flags" else (JUMPER_MASKS, 0xDE)
            children = entries[i + 1:i + 1 + amount]
            if len(children) != amount or any(child[0] not in masks for child in children):
                raise ValueError("setup projection: nested band mismatch")
            word = struct.unpack_from("<H", body, offset)[0]
            for child, _ in children:
                word |= masks[child]
            struct.pack_into("<H", body, offset, word)
            i += amount + 1
            continue
        else:
            raise ValueError("setup projection: unsupported outer key")
        i += 1
    return bytes(body)


def project_source(text):
    """Return bounded original-derived expected bytes; callers retain them locally only."""
    source_contexts(text)
    counts = {GROUP_NAMES[group["group"]]: group["declaredCount"] for group in profile(text)["groups"]}
    payloads = {name: bytearray() for name in GROUP_NAMES.values()}
    group, entries = None, None
    for raw in text.splitlines():
        line = raw.split(";", 1)[0].strip()
        if not line or line.startswith("//"):
            continue
        match = re.fullmatch(r"\[([A-Za-z][A-Za-z0-9]*)\]\s*(.*)", line)
        if match is None:
            raise ValueError("setup projection: unsupported source line")
        key, value = match.group(1).lower(), match.group(2).strip()
        if key in GROUP_KEYS:
            group = GROUP_NAMES[GROUP_KEYS[key]]
        elif key == "startrecord":
            entries = []
        elif key == "endrecord":
            payloads[group].extend(_record(entries))
            entries = None
        elif entries is not None:
            entries.append((key, value))
    result = {name: bytes(body) for name, body in payloads.items()}
    if any(len(result[name]) != max(0, count) * RECORD_SIZE for name, count in counts.items()):
        raise ValueError("setup projection: record cardinality drift")
    return {"counts": counts, "payloads": result}
