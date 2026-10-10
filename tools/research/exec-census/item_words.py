"""Tallies the word at 0x10 of the 23-byte item chunks in an installed GFF archive.

Usage: python -I item_words.py PATH_TO_GFF [N,N,...]

Finds every 10-byte chunk header of the form FMT-PARTY-006 gives an item chunk: chunk_type 2 or 4,
any target_chunk, kind 1, unk_03 0, any record_ref and field, and len_data 23 (0x17). For each it
reads the little-endian word at 0x10 of the 23 data bytes, which overlays 173, 189 and 213 take,
less 1, as the `DATA` number of the item's power. It prints the count of chunks, the count of
distinct words and the nonzero words with how often each occurs, and, with a comma-separated list
of decimal words, the file offset and first data word of each chunk whose word is in the list. A
header the byte pattern matches inside other data is counted too. Nothing is written or executed.
"""
import collections
import re
import sys
from pathlib import Path

body = Path(sys.argv[1]).read_bytes()
wanted = {int(x) for x in sys.argv[2].split(",")} if len(sys.argv) > 2 else set()
pattern = re.compile(rb"[\x02\x04].\x01\x00....\x17\x00", re.S)
tally, hits = collections.Counter(), []
for m in pattern.finditer(body):
    data = m.start() + 10
    word = int.from_bytes(body[data + 0x10:data + 0x12], "little")
    tally[word] += 1
    if word in wanted:
        hits.append((m.start(), int.from_bytes(body[data:data + 2], "little"), word))
print(f"{sum(tally.values())} chunks; {len(tally)} distinct words at 0x10")
print("nonzero:", sorted((k, v) for k, v in tally.items() if k))
for offset, first, word in hits:
    print(f"header 0x{offset:08X}: first word {first}, word at 0x10 {word}")
