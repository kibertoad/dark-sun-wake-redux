"""Prints the enemy spell lists and the spell weights in the load image of DSUN.EXE.

Usage: python -I enemy_spell_lists.py PATH_TO_DSUN.EXE [N,N,...]

Checks the installed DSUN.EXE's size and XXH3-128. Reads the 13 far pointers at 529C:0014 of the
load image (segment 0x1000 at the end of the MZ header), which overlay 177 indexes by byte 0x38 of
an actor's FMT-COMBAT-002 record, and prints for each the address it holds, its file offset and the
words there up to the first -1, at most 15. It then prints the 235 bytes of segment 4C78, which
overlay 174 multiplies with a random number to score a spell, and, with a comma-separated list of
decimal spell numbers, the signed byte of each. Nothing is written or executed.
"""
import struct
import sys

import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16


def file_offset(segment, offset):
    return header + (segment - 0x1000) * 16 + offset


for index in range(13):
    offset, segment = struct.unpack_from("<HH", body, file_offset(0x529C, 0x14 + 4 * index))
    segment += 0x1000
    at = file_offset(segment, offset)
    words = []
    for k in range(15):
        word = struct.unpack_from("<h", body, at + 2 * k)[0]
        if word == -1:
            break
        words.append(word)
    print(f"list {index}: {segment:04X}:{offset:04X} (file 0x{at:08X}) {words}")
weights = body[file_offset(0x4C78, 0):file_offset(0x4C78, 235)]
print(f"weights at 4C78:0000 (file 0x{file_offset(0x4C78, 0):08X}): {list(weights)}")
for n in (int(x) for x in sys.argv[2].split(",")) if len(sys.argv) > 2 else ():
    print(f"spell {n}: weight {struct.unpack_from('<b', weights, n)[0]}")
