"""Lists every occurrence of some ASCII texts, in either case, in a set of files.

Usage: python -I name_census.py --text TEXT [--text TEXT ...] FILE [FILE ...]

For each FILE, prints its size and XXH3-128, then each offset at which one of the TEXTs occurs
with any mix of upper and lower case, with up to 24 printable bytes around it. A file with no
occurrence of a TEXT prints that TEXT with "none". Searches the bytes as they are: text that a
program builds at run time, or keeps compressed, does not show. Nothing is written or executed.
"""
import re
import sys

import xxhash

texts, files, args = [], [], sys.argv[1:]
while args:
    if args[0] == "--text":
        texts.append(args[1].encode())
        args = args[2:]
    else:
        files.append(args[0])
        args = args[1:]
for name in files:
    body = open(name, "rb").read()
    print(f"{name}: {len(body)} bytes, XXH3-128 {xxhash.xxh3_128_hexdigest(body)}")
    for text in texts:
        hits = [m.start() for m in re.finditer(re.escape(text), body, re.IGNORECASE)]
        if not hits:
            print(f"  {text.decode()!r}: none")
        for at in hits:
            around = body[max(0, at - 8):at + len(text) + 16]
            shown = "".join(chr(c) if 32 <= c < 127 else "." for c in around)
            print(f"  {text.decode()!r} at 0x{at:08X}: {shown}")
