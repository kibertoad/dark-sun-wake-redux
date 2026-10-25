"""Walks FLI files with the chunk layouts of FND-EXE-613 and reports where each decoder stops.

Usage: python -I fli_chunks.py DIRECTORY NAME [NAME ...]

For each named file it follows the records and chunks of FMT-VIDEO-001 and, for each colour
(0x0B), line delta (0x0C) and run-length (0x0F) chunk, reads the data as the decoders of overlays
214 to 217 read it, without writing pixels. It counts the chunks of each type, the colour packets,
colour chunks in which a 256-colour packet is followed by another, line delta chunks with a line
count of 0, and chunks whose decoding reads past the chunk's stated end or past its record's end.
It prints, for the first record of each file, each chunk's stated end, the record's end and the
offset after the last byte its decoder read. Nothing is written.
"""
import struct
import sys
from pathlib import Path


def decode_end(body, ctype, data):
    """The offset after the last byte the decoder of chunk type ctype reads, or None."""
    p = data
    if ctype == 0x0B:
        count = struct.unpack_from("<H", body, p)[0]
        p += 2
        for _ in range(count):
            p += 2 + 3 * (body[p + 1] or 256)
        return p
    if ctype == 0x0F:
        for _ in range(200):
            packets = body[p]
            p += 1
            for _ in range(packets):
                run = struct.unpack_from("<b", body, p)[0]
                p += 1 + (1 if run >= 0 else -run)
        return p
    if ctype == 0x0C:
        lines = struct.unpack_from("<H", body, p + 2)[0]
        p += 4
        for _ in range(lines):
            packets = body[p]
            p += 1
            for _ in range(packets):
                run = struct.unpack_from("<b", body, p + 1)[0]
                p += 2 + (run if run >= 0 else 1)
        return p
    return None


def main():
    root = Path(sys.argv[1])
    for name in sys.argv[2:]:
        body = (root / name).read_bytes()
        frames = struct.unpack_from("<H", body, 6)[0]
        counts = {"0x0B": 0, "0x0C": 0, "0x0F": 0, "0x10": 0, "0x0D": 0, "colour_packets": 0,
                  "colour_256_then_more": 0, "delta_zero_lines": 0, "past_chunk": 0, "past_record": 0}
        pos = 0x80
        for record in range(frames + 1):
            size, rtype, chunks = struct.unpack_from("<IHH", body, pos)
            if rtype != 0xF1FA:
                raise ValueError(f"{name}: record {record} type {rtype:#x}")
            record_end = pos + size
            at = pos + 16
            for _ in range(chunks):
                csize, ctype = struct.unpack_from("<IH", body, at)
                key = f"0x{ctype:02X}"
                if key in counts:
                    counts[key] += 1
                if ctype == 0x0B:
                    n = struct.unpack_from("<H", body, at + 6)[0]
                    p = at + 8
                    for k in range(n):
                        size_k = body[p + 1] or 256
                        if size_k == 256 and k < n - 1:
                            counts["colour_256_then_more"] += 1
                        p += 2 + 3 * size_k
                    counts["colour_packets"] += n
                if ctype == 0x0C and struct.unpack_from("<H", body, at + 8)[0] == 0:
                    counts["delta_zero_lines"] += 1
                end = decode_end(body, ctype, at + 6)
                if end is not None and end > at + csize:
                    counts["past_chunk"] += 1
                if end is not None and end > record_end:
                    counts["past_record"] += 1
                if record == 0:
                    print(f"{name} record 0 chunk 0x{ctype:02X} at 0x{at:X}: stated end 0x{at + csize:X},"
                          f" record end 0x{record_end:X}, decoder end "
                          + (f"0x{end:X}" if end is not None else "none"))
                at += csize
            pos = record_end
        print(name, len(body), frames + 1, counts)


if __name__ == "__main__":
    main()
