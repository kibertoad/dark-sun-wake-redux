"""Tallies the walk frames of every SCMD resource in installed GFF archives.

Usage: python -I scmd_walk_frames.py PATH_TO_GFF [PATH_TO_GFF ...] [NUMBER]

Reads each archive's directory (FMT-GFF-001 to FMT-GFF-007) as details_chunks.py does. An SCMD
resource is read as 31E0:1893 loads it: 0x48 bytes of 36 words, each the first frame of one
animation, then 8-byte frames (+0 loop target, +1 delay, +2 flags, +3 and +4 signed pixel steps).
For each of the walk words 1, 7, 9, 15, 17, 23, 25 and 31 (the table at DS:1413, one per direction
0 to 7) it follows the frames from the first until one has flag 1 or 2, or for 32 frames, and
prints, per direction, the six most common sequences of (x step, y step, delay, flags) with their
counts, the number of frames until both summed steps are multiples of 16, and the delays seen;
then the resources that hold the most common moving sequences. With NUMBER, it also prints each
direction's sequence for the SCMD resources with that number. Nothing is written or executed.
"""
import struct
import sys
from collections import Counter
from pathlib import Path

WALK = [1, 7, 9, 15, 17, 23, 25, 31]


def resources(body):
    assert body[:4] == b"GFFI"
    at = struct.unpack_from("<I", body, 0x0C)[0]
    count = struct.unpack_from("<H", body, at + 8)[0]
    p, plain, indexed = at + 10, {}, []
    for _ in range(count):
        tag, word = body[p:p + 4].decode("latin-1"), struct.unpack_from("<I", body, p + 4)[0]
        entries, is_indexed = word & 0x7FFFFFFF, word >> 31
        if is_indexed:
            index_number, ranges = struct.unpack_from("<II", body, p + 12)
            pairs = [struct.unpack_from("<II", body, p + 20 + 8 * k) for k in range(ranges)]
            indexed.append((tag, index_number, [n for f, c in pairs for n in range(f, f + c)]))
            p += 20 + 8 * ranges
        else:
            plain[tag] = [struct.unpack_from("<III", body, p + 8 + 12 * k) for k in range(entries)]
            p += 8 + 12 * entries
    out = [(tag, n, o, s) for tag, rows in plain.items() for n, o, s in rows]
    gffi = {n: (o, s) for n, o, s in plain.get("GFFI", [])}
    for tag, index_number, numbers in indexed:
        offset, _ = gffi[index_number]
        for i, n in enumerate(numbers):
            ro, rs = struct.unpack_from("<II", body, offset + 4 + 8 * i)
            out.append((tag, n, ro, rs))
    return out


def s8(b):
    return b - 256 if b > 127 else b


seqs = Counter()
who = {}
per_dir = {d: Counter() for d in range(8)}
total = 0
show = int(sys.argv[-1]) if sys.argv[-1].isdigit() else None
for path in sys.argv[1:]:
    if path.isdigit():
        continue
    body = Path(path).read_bytes()
    for tag, n, o, s in resources(body):
        if tag != "SCMD" or s < 0x48:
            continue
        data = body[o:o + s]
        starts = struct.unpack_from("<36H", data, 0)
        frames = [data[0x48 + 8 * k:0x48 + 8 * k + 8] for k in range((s - 0x48) // 8)]
        total += 1
        for d, w in enumerate(WALK):
            f, seq = starts[w], []
            for _ in range(32):
                if f >= len(frames):
                    seq.append(("out", f))
                    break
                fr = frames[f]
                seq.append((s8(fr[3]), s8(fr[4]), fr[1], fr[2]))
                if fr[2] & 3:
                    break
                f += 1
            key = tuple(seq)
            per_dir[d][key] += 1
            who.setdefault((d, key), []).append((Path(path).name[:3], n))
            if show is not None and n == show:
                print(Path(path).name, n, "dir", d, "start", w, starts[w], seq)
print("SCMD resources", total)
for d in range(8):
    print("dir", d)
    for key, c in per_dir[d].most_common(6):
        print("  ", c, key)

print("frames to first cell centre, per direction (sequences with movement only)")
for d in range(8):
    tally, delays = Counter(), Counter()
    for key, c in per_dir[d].items():
        if key and key[0][0] == "out":
            continue
        x = y = 0
        got = None
        for i, fr in enumerate(key):
            if fr[0] == "out":
                break
            x += fr[0]; y += fr[1]; delays[fr[2]] += c
            if (x or y) and x % 16 == 0 and y % 16 == 0:
                got = (i + 1, x, y)
                break
        tally[got] += c
    print(d, dict(tally.most_common(8)), "delays", dict(delays))

for d in (0, 2, 3):
    key = per_dir[d].most_common(2)[1][0] if d == 0 else [k for k, c in per_dir[d].most_common(3) if k[0][0] != "out"][0]
    print("dir", d, key, "held by", who[(d, key)][:60])
