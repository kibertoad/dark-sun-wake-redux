---
id: FND-EXE-249
title: Post-transfer helper rewrites segment words before an optional pattern and target search
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:039A..4AE5:03AA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0421..4AE5:0466
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0466..4AE5:04C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004049A..0x0004049C
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-246's optional target at `4AE5:0421` covers thirty-three instructions
and sixty-nine bytes. Its sole near call, at `4AE5:045D`, targets
`4AE5:0466`; that helper covers thirty-nine instructions and ninety-six
bytes, has no calls and ends exclusively at `4AE5:04C6`. Both return near
without additional argument cleanup.

The outer helper saves DS and ES. Current ES-relative word eight supplies
an offset split: SI receives its low four bits, while its unsigned quotient
by sixteen is added to ES-relative word `0x0010` modulo 65536 to form DS.
That word `0x0010` separately becomes ES. Incoming CX is shifted right one,
then direction is cleared. There is no zero check before the loop body.
Each iteration reads one word through DS:SI and advances SI by two, puts
that word in BX, and reads the word at ES:BX into DI.

After saving the current DS, the helper loads DS with the relocated segment
operand at shipped offset `0x0004049A`. Its raw word is `0x45E8`; the MZ
relocation at load segment `0x1000` gives `55E8:0000`, corresponding to
shipped offset `0x0004B080`. It keeps the previously read word in AX,
clears DI's low three bits and reads a replacement word through DS:DI into
DX. It writes DX to ES:BX before testing the retained old word's low bit.
Only a set low bit calls the nested helper. Afterwards it restores the
saved DS and uses sixteen-bit LOOP. At completion it restores saved ES
and DS and returns. Storage aliases and stack integrity remain conditional.

For incoming count c, the shifted count is floor(c/2). If nonzero, the
explicit loop performs that many iterations; if zero, LOOP permits 65536
iterations, assuming accesses and the nested call complete and its saved
count is restored. Counts zero and one therefore share that conditional
edge case. The caller at `4AE5:039A` reloads ES from DS-relative word
`0x012C`, loads CX from ES-relative word `0x000A` and skips the call only
when that original count is zero. Its nonzero test does not exclude one.
After return it tests the header's separate word `0x000C`, rather than
checking a result from this helper.

The nested helper saves CX and loads DS from the replacement segment in
DX. It reads bytes through ES at BX minus one, BX plus two, BX plus three
and BX plus six, each offset wrapping at sixteen bits. Its checks require
the first byte's high five bits to equal `0xB8`, the second's to equal
`0x50`, their low three bits to agree, the third byte to equal the first
and the fourth to equal the second. Any mismatch returns after restoring
CX; the segment-word replacement already made by the outer helper remains.

If the checks pass, DI starts at `0x0020` and CX comes from the replacement
segment's word `0x000C`. It reads AX from ES-relative BX plus four and
compares it to the word at DS-relative DI plus two. Equality writes DI to
ES-relative BX plus four and exits; otherwise DI advances by five modulo
65536 and LOOP continues. A nonzero loaded count allows at most that many
comparisons; zero allows 65536 if no earlier match. Exhaustion returns
without a dedicated error or rollback. The nested helper restores CX,
but not DS, AX, DX or DI; the outer helper's subsequent DS pop is needed
before its next source-word read. Both loop bounds are independent of the
native destination and table extents, which remain unproved.

## Interpretation

This follows another downstream dependency and its actual caller count.
The first write precedes optional validation and search, so their rejection
cannot be read as transactional rejection of that write. A zero guard on
the original byte count is weaker than a guard on the shifted loop count.
The nested search has its own count and output bound obligations; the outer
count alone does not establish them.

Q-EXE-001 and Q-EXE-010 retain native segment and count writers, source-word
and table admission, effective and saved-stack aliases, other incoming
transfers and independently safe output extents. No complete_reading,
native code admission or replacement inventory is established.

## Alternatives

Treating the optional checks as prerequisites for the segment-word store
is contradicted by their order. Treating incoming count one as one iteration
ignores the shift and zero-entry LOOP behavior. Treating the nested helper
as preserving DS ignores its replacement-segment load and caller restoration.

## How to reproduce

At revision `97643be`, use FND-EXE-236's source identity, source region and
default x86-bounds limits. With no seeds or summaries, independently set
entry and sole entries to `0x00040471` and `0x000404B6`. Check covered
intervals `0x00040471..0x000404B6` and `0x000404B6..0x00040516`, respective
instruction counts thirty-three and thirty-nine, and the outer near call.
The first report assumes the nested call returns; complete CFG fields are
not Standard complete readings or proofs of safe storage.

Decode both intervals in sixteen-bit mode directly from the shipped source
with Capstone. Read the caller interval `0x000403EA..0x000403FA` from
`4AE5:039A`. Query operand at `0x0004049A` with sourceKind mz, targetOffset
zero and the same source hash; verify relocation and loaded segment. Keep
source, reports and configurations in GAME_DIR and native assumptions open.
