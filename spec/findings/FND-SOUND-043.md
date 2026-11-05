---
id: FND-SOUND-043
title: Setup lower write separates raw DOS output from translated chunks and retains word arithmetic on short writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3B03..1000:3C90
tool: tools/research/exec-census/resident_listing.py at 1c57dcee; Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

Q-CONFIG-011 retains lower write 3B03 after FND-SOUND-032's negative-count
flush route. This reads that helper and its direct raw-write child, using
FND-SOUND-031 for error/seek wrappers. Addresses/masks are hexadecimal;
counts are decimal. Native storage, stack spans, aliases and interrupt
results remain conditions, not observed outcomes.

3B03 rejects an unsigned handle at or above DS:DD38 through error 0006.
It returns zero without output for requested word zero or FFFF, detected
by incrementing the word and comparing unsigned against two. Other counts
continue. Handle-table bit 0800 invokes 05CC with zero displacement and
origin two before output; its result is ignored. The table is read again
after that child. With bit 4000 clear the original far buffer and requested
count are passed once to raw writer 3C54; its AX returns unchanged.

With bit 4000 set, 3B03 clears handle-table bit 0200 and copies source
bytes into stack scratch storage. Each source LF emits CR then LF; all
other bytes emit unchanged. The source offset advances with word
increment, so readable source and nonwrapping spans are conditions.
After each source byte the expanded length is compared against 128.
At or above that threshold it writes the chunk through 3C54. Under valid
nonwrapping stack arithmetic a chunk can contain 128 or 129 bytes: a
source LF after 127 accumulated bytes adds two before the threshold test.
The remaining tail, when nonempty, is also passed to 3C54.

A child result equal to the expanded chunk length continues; FFFF exits
immediately with FFFF. Any other unequal result exits with word arithmetic:

- For a full chunk: requested source count minus remaining source count,
  plus child AX minus expanded chunk length.
- For the final tail: requested source count plus child AX minus expanded
  tail length.

All arithmetic keeps the low word. This is not a general source-byte
consumption count. For example, a final tail consisting of 63 source LF
bytes expands to 126 bytes. A zero child result computes 63-126, returning
word FFC1 rather than zero. This is a derived arithmetic example, not a
native DOS observation. If all child lengths match, the original requested
source count returns, including when an exact last chunk leaves no tail.

Raw writer 3C54 has no handle-bound check of its own. It tests table bit
0001 and rejects through error 0005 when set. Otherwise it saves DS,
loads DS:DX from the supplied far buffer, sets BX to handle, CX to count
and AH to 40, and invokes interrupt 21. DS is restored before carry is
tested. Carry set sends returned AX to 04CE and returns FFFF. Carry clear
sets handle-table bit 1000 and returns DOS AX unchanged, including a
short or zero result. It reloads the original handle for this table write.
No local retry completes a short write.

## Interpretation

This removes the local lower-write dispatch, translation and result
arithmetic from Q-CONFIG-011. FND-SOUND-032's flush caller compares its
requested SI with that word result, then checks the stream's post-child
0200 flag. It does not receive a guaranteed normalized source-byte count.
FND-SOUND-033's retained rb handle bit 0001 also supplies a local rejection
condition if such a handle is ever sent down this write route; that does
not establish native reachability of the negative-count flush branch.

## Alternatives

Treating all writes as raw ignores bit 4000 and LF expansion. Treating a
short translated write as retry or an exact consumed-source count ignores
the immediate word-arithmetic return. Actual DOS counts, seek outcomes,
native stack/source bounds, state writers and child preservation remain
Q-CONFIG-011. The parser's diagnostic formatter/callback continuation is
still Q-CONFIG-006; reading this lower helper does not settle that broader
path. No original was run, no complete reading declared and no parity
status promoted.

## How to reproduce

Use approved installed SOUND_DS.EXE, length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Run resident_listing.py with -I and
1000:3B03..1000:3C90. Retained reports under
GAME_DIR/analysis/disc-adv-survey-20261104/ are setup-lower-write.txt and
setup-lower-write-repeat.txt, both SHA-256
b81f7830068a4e7504fc667f4ff3307af729ee483ceedd5cbdf150e33f520e6b.
Check the two zero-output count cases, ignored seek result, translation
threshold after expansion, full/tail short-result formulae and raw-writer
carry split separately. Retain listings only in the approved local store.
