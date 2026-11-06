---
id: FND-PARTY-301
title: Three encoded range-wrapper callers reduce to two byte-plus-235 producers and one forwarded word with known producer families
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005EE00..0x0005EF2F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005EF81..0x0005F3BD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F404..0x0005F5ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005F5ED..0x0005F724
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000875C2..0x0008772D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000920F6..0x00092115
tool: direct_callers.py and overlay_listing.py at 6477f1c8, Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with packaged ReportMemoryBlockForFileOffset.java and ReportDecompileMatches.java; FND-CONFIG-125, FND-PARTY-121/158/273/280 and FND-EXE-521
environment: null
---

## Observation

Q-PARTY-134 retains the original-number domains of three word-input
clients after FND-PARTY-286. This takes the complete encoded caller
set of the range wrapper 208+0866 as one connected producer objective.
Offsets below are hex; number ranges and the constant 235 are decimal.
Installed DSUN.EXE remains 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b.

### One reusable caller census

The direct-call reporter for 208+05B2, 208+0866 and 197+1A94 finds
two, three and five encoded candidates respectively. It retains its
raw-pattern, indirect/jump/bypass and runtime-binding exclusions.
This reading classifies the three range-wrapper callers; it does not
count the other seven as newly understood. FND-CONFIG-125's 174+0749
and FND-PARTY-158's 198+03EB are independent positive call controls.

| Caller of 208+0866 | Actual pushed second word, the number |
|---|---|
| 174+03D3, in +0181 | Own word argument BP+8, pushed at +03CD |
| 174+0749, in +0604 | Current SS:BP-2 plus 00EB, at +0741..+0747 |
| 198+03EB, in +02B2 | Current DI plus 00EB, at +03E4..+03E9 |

The range wrapper retains that number in SI across 177+07AF and
forwards it to 208+07F2 at +087C. The other returned argument is
the level/range input; it does not replace the number (FND-PARTY-273).

### Two intrinsic byte domains exclude 1001

174+0604 reads a selected list byte at +06B2 and uses unprefixed
98 at +06B5 to sign-extend AL to AX, storing it at SS:BP-2.
198+02B2 performs the corresponding byte read at +0359, sign
extension at +035C and DI assignment at +035D. These are word
sign extensions, despite the decoder's generic CWDE label.

Every possible source byte therefore produces a word representing
-128..127. Adding 235 produces 107..362, without word wrap, and
cannot produce 1001. This domain does not require the random choice
to select a valid shipped list member: even another readable byte
has the same intrinsic width bound. FND-PARTY-158's shipped power
lists give a narrower ordinary source set, but that is unnecessary
for this arithmetic exclusion.

Coordinate/line-of-sight/distance calls intervene before the pushes.
Actual use of this domain requires retained SS:BP-2 in 174 or DI in
198, and correct outgoing frames; these are Q-PARTY-136 obligations.
Neither the domain nor an existing list establishes those effects.

### The formal-word caller has two encoded producers

A second census for 174+0181 finds near calls at 174+0039 and
+087D, both after PUSH CS. The first is FND-CONFIG-125's +0000
route; it passes the nonnegative result retained from +00B6. The
second is its +084D route; it passes the result retained from +07ED,
except exact FFFF exits first. Both known calls are controls; unknown
indirect and bypass inputs to +0181 remain outside the census.

FND-PARTY-121 reads +00B6 as choosing a word from its incoming list,
with FFFF as its initial no-choice result. Scoring decides which word
is selected; it does not compute a replacement number. Its known
+0000 caller gets that list from 173+1595 and 177+0400 with exact
level -1, whose intrinsic emitted selectors are 0..234
(FND-PARTY-280). Under retained list/count/selected-word storage, this
producer family cannot yield 1001. Actual list admission and forwarding
still need the preserved-state contract, not another score arithmetic
reading merely because scoring calls children.

The +07ED helper reads the object's byte +0E. Zero returns FFFF;
otherwise it calls 198+0149 with that byte and mode zero, returning
AX unchanged on its ordinary continuation. FND-PARTY-121 reads the
mode-zero results as fixed immediates no greater than 319 or FFFF.
FND-EXE-521 independently resolves the mode-zero twenty-slot table,
its unsigned bound and its instruction targets. This producer also
cannot intrinsically return 1001. Object lookup identity can change
which input byte is read but not the finite return set of that mode.
Return/frame and retained DI/argument storage remain Q-PARTY-136.

## Interpretation

The three known range-wrapper bindings now have a finite producer map.
Two have an intrinsic signed-byte-plus-235 exclusion; the formal-word
route has known list-choice and fixed-return families. Preserve their
stated storage and transfer admission rather than treating an unbounded
formal word as an unexamined numeric producer. This narrows the +087C
client's producer-domain component, not every execution into that client.

Q-PARTY-134 retains other producers, changed lists/inputs and excluded
incoming transfers. Q-PARTY-136 owns retaining these produced words,
including the two named byte-source intermediates and formal-word caller
frames. The other two word clients and Q-PARTY-124's eighty sites remain.
No whole getter site is excluded; FMT-PARTY-007 stays supported without
a complete reading or answered queue deletion.

## Alternatives

- A word argument can be unbounded at an interface while its known
  producer families are bounded. Unknown alternate entry routes survive.
- A correct byte-width bound at production does not prove an intervening
  call leaves the later word or register unchanged.
- The 0..234 list family depends on actual list/count retention; arbitrary
  changed list words are not admitted by the producer's intended use.

## How to reproduce

Run direct_callers.py for 208+05B2, 208+0866 and 197+1A94; require
the two independent range-call controls. Run it for 174+0181 and
check both near-call frames. Read the stated source ranges with
overlay_listing.py, verifying each byte read, unprefixed sign extension,
word add and push, plus the formal argument's actual identity.
The exploratory first-174 report extends through the following helper;
its relevant +0181 body ends at file 5F3BD. Do not count the next helper.

In the read-only Ghidra project, the two byte-producer entries map
file 5F404 to fbov174::06a204 and 875C2 to fbov198::0923c2.
The decimal-literal 235 query misses its controls; 0xeb recovers bounded
matches in both. Extra inferred far-call arguments are not trusted;
verify the actual two pushed arguments and widths against instructions.
Reuse FND-PARTY-121/280 and FND-EXE-521 for the known formal producers.
Reports word-selector-producer-callers, word-selector174-formal-callers,
word-selector174-range-first/second, word-selector198-range,
word-selector174-formal-producer/second-producer and range-producer
Ghidra/control logs remain in GAME_DIR/analysis/handler-census-20261106/.
No original game, emulator session or emulated call ran.
