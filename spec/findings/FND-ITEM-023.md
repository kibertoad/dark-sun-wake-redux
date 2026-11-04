---
id: FND-ITEM-023
title: Four overlay-195 continuations replace the hit register value while overlay 204 retains a child-input gap
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081D31..0x00081EF9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D298..0x0008D303
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3143:00A8..3143:00DE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:05BB..31E0:05ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0AF8..31E0:0AFC
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at b0dd36dc; locked evidence Python and Capstone
environment: null
---

## Observation

This continues Q-ITEM-009's result-register reading after FND-ITEM-021/022.
It distinguishes direct register consumption from memory effects and child
input contracts. It assumes returning children and intact frames; Q-ITEM-011
still holds memory identity, aliasing and preservation obligations.

### Overlay 195

After wrapper +05BC, the caller tests local byte BP-1A mask 02. If clear,
it joins the continuation of wrapper +0615. That continuation tests local
byte BP-18 mask 08; if clear, it joins the continuation of wrapper +066E.
That continuation tests local byte BP-0D mask 80. None tests AL.

Each set branch calls resident 3143:00A8 with SI and an explicit code:
1, 19 or 111 respectively. The helper loads AL from its code-byte argument,
requests 3143:0061, and explicitly assigns AX the resulting row index or
-1 on both returning branches. Thus a caller's later store of this lookup
result does not save the preceding hit flags. FND-MAGIC-015 records the
inner lookup's byte-code contract; its argument's incidental high byte is
not evidence of a second code parameter.

If all these local tests are clear, +067C jumps to +076E, whose first
instruction overwrites AX for metadata segment setup. This removes the
remaining direct register path through the skipped lookup branches.

After wrapper +0733, DI is incremented and compared with a local bound.
The exit branch overwrites AX for metadata setup at +0747. The loop-back
branch reaches +06F4. A zero DI sets a local word and reaches +0723,
where AX is overwritten with its address before the next wrapper call.
A nonzero DI first calls the bounded random helper and stores that
helper's result. Subsequent argument preparation still overwrites AX at
+0723 before the next hit. The random helper's incoming-register contract
is a separate child-input qualification, not a direct AL mask test.

### Overlay 204

After wrapper +150F, the caller increments SI and compares it with a local
bound. Its loop branch requests overlay 173 +2A5D before preparing the next
hit. Its exit branch requests 31E0:05BB and overlay 173 +24C7. It then loads
its own local byte BP-19, clears AH, and uses negation, subtract-with-borrow
and increment to produce one for a zero local byte and zero otherwise.
That ending's explicit Boolean is therefore not a direct test of hit AL.

This does not yet remove the child's incoming-register question. Resident
31E0:05BB tests its index argument before assigning AX: negative or
out-of-range indices jump to +0AF8, a restore-and-return ending with no AX
assignment. Valid indices instead overwrite AX from the index at +05DB.
Consequently the invalid-index branch may carry an incidental AX into the
following +24C7 call. Treating every child call as an automatic overwrite
would lose that distinction. The remaining child-input contracts and native
index admission stay in Q-ITEM-009 and Q-ITEM-011 respectively.

## Interpretation

The direct local-field tests after the three overlay-195 calls are settled;
they must not be described as returned hit-mask tests. Their skipped branches
also have explicit register overwrites. The fourth call has an overwrite on
its exit and during next-call preparation, with the random child's implicit
input contract still qualified. Overlay 204 has a separate local-byte
Boolean but an unresolved intervening child path. No whole-child effects,
native admission, complete reading or rule confidence promotion follows.

## Alternatives

- Local masks 02, 08 and 80 are not tests of AL.
- A child call alone does not prove AX was replaced.
- A later normalized Boolean does not exclude an earlier child consuming
  an incidental register value.
- Register overwrites do not exclude hit-induced memory changes.

## How to reproduce

Use the locked evidence Python and installed DSUN.EXE resolved through
tools/game-dir.mjs. The readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. With the scripts named in Tool, run:

```text
overlay_listing.py <dsun> 195 81D31 81E64
overlay_listing.py <dsun> 195 81E64 81EF9
overlay_listing.py <dsun> 204 8D298 8D303
resident_listing.py <dsun> 3143:00A8..3143:00DE
resident_listing.py <dsun> 31E0:05BB..31E0:05ED 31E0:0AF8..31E0:0AFC
```

Reports are local in GAME_DIR/analysis/hit-incoming-20261104/:
remaining-195-branches.txt, remaining-195-loop-tail.txt,
remaining-204-loop-tail.txt, remaining-first-children.txt and
remaining-child-exits.txt. Some retained reports extend into the following
function; only the half-open ranges above support the observations.
No original listing, bytes or extracted content is committed.
