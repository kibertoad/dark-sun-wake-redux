"""Prints the byte bounds DSUN.EXE's cast lists are built from.

Usage: python -I cast_list_bounds.py PATH_TO_DSUN.EXE

Checks the installed DSUN.EXE's size and XXH3-128. Reads the load image (segment 0x1000 at the end
of the MZ header) in the data segment 57E0 and prints:
- the bytes at DS:0655 to DS:066C, which overlay 177 +0400 reads at DS:0655 + level and DS:0656 +
  level for an odd list kind and at DS:0660 + level and DS:0661 + level for an even one, as the
  first and the end of the numbers it lists;
- the bytes at DS:066C to DS:066F, which overlay 211 +1DBC reads at DS:066C + d and DS:066D + d for
  d from 0 to 2, as the first and the end of the psionic indices it lists, with the DATA numbers
  overlay 211 +10F4 casts for them (index plus 235).
Nothing is written or executed.
"""
import struct
import sys

import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
DS = 0x57E0

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16


def ds_byte(offset):
    return body[header + (DS - 0x1000) * 16 + offset]


print("odd kind, DS:0655 + level: " + " ".join(
    "%d:%d" % (level, ds_byte(0x655 + level)) for level in range(12)))
print("even kind, DS:0660 + level: " + " ".join(
    "%d:%d" % (level, ds_byte(0x660 + level)) for level in range(12)))
for d in range(3):
    first, end = ds_byte(0x66C + d), ds_byte(0x66D + d)
    print("psionic list %d: indices %d to %d, DATA %d to %d" % (d, first, end - 1, first + 235,
                                                                end - 1 + 235))
