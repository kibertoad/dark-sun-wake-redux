---
id: FND-SOUND-049
title: Resource sound wrapper gates card and library mode, conditionally remaps low indices, and forwards three words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4654:04FE..4654:056A
tool: resident_listing.py at 656417ff; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with ReportDecompileMatches.java; FND-SOUND-007 and FND-PARTY-317
environment: null
---

## Observation

Q-SOUND-002 explicitly retains 4654:04FE and the meaning of its
6000/6001 arguments. Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets below are hexadecimal.
This reads its complete local body and outgoing interface, not the
playback implementation of its child.

### Gates precede any child invocation

The wrapper saves BP, SI and DI. Its first word argument at SS:BP+6
becomes SI and its second at BP+8 becomes DI. It loads the far
configuration pointer from current DS:3411 and reads word +8.
Value 0071 skips to its common return, without invoking the child.
There is no preceding local null test of the configuration pointer.

It next reads current word 4E71:0B51. Values two and five each
skip the child. Other values continue; there is no independent bound
or normalization of that mode word. The wrapper then reloads the
configuration pointer and reads word +8 again. This is a separate
access epoch; the local body makes no child call between those reads,
but it does not itself establish interrupt or backing preservation.

### One unsigned index branch changes only the first forwarded word

When the second card-word read is 0068 and unsigned SI is below
00F1, it computes six times SI at word width, selects segment
50C3, and loads the far pointer at offset 0D2F plus that product.
It reads word +4 of the pointed record and uses that word as the
first argument to 4654:056A. For this branch, SI is at most
00F0: the product and table-field addition do not wrap. The four-byte
pointer field for that last admitted index starts at 12CF. Its actual
storage, pointer target and record extent are not locally validated.

Every other continuing case forwards original SI as the first word:
card values other than 0068, and SI at or above 00F1 including
high-bit values. The branch is unsigned JAE, not a signed-negative
or byte-only test. This wrapper's table-remap path therefore does not
select entry 00F1, which FND-PARTY-317 identifies in the separate
timer callback. That does not exclude another index/binding under a
different incoming route or child effect.

Both branches forward original DI as the second word and current
SS:BP+0A as the third. They construct the child far frame with
PUSH CS followed by a near call at 0560, then remove six argument
bytes. On its ordinary continuation the wrapper restores DI from
SS:BP-4, SI from SS:BP-2 and BP, then returns far without removing
its incoming arguments. Actual saved words/frame integrity remain
conditions; the child need not preserve live DI for that restoration.

FND-SOUND-007 binds its resource-effect call as first word n,
second 6000 and third 6001, decimal. This wrapper neither interprets
the last two words as a rate or volume nor transforms them. That
meaning remains with 056A and the services it reaches. There is
no local success-result assignment common to all exits; no-child
and child paths must not be assigned an invented AX success flag.

The wrapper has no own interrupt, indirect call or record/global store
outside stack operations. Its only child is 056A on the continuing
paths. Its own gate/remap behavior is therefore classified; child
effects, actual card/mode correspondence and storage admission remain.
The bounded decompiler match recovers the card/index remap branch;
instructions establish unsigned width, far-pointer segments and push order.

## Interpretation

Q-SOUND-002 no longer needs the local checks, index comparison or
argument forwarding of 04FE read as unknown. It needs actual inputs,
card/mode/record/alias admission and the playback/parameter contract of
056A and its reached services. The gates are local evidence, not
proof of a reachable state contradicting RULE-SOUND-001's caller
model: correspondence with the caller's settings remains an obligation.
No native playback, sample rate, volume, complete rule reading or
queue closure is established.

## Alternatives

- The helper always invokes playback after resource lookup: it has card
  and mode branches that return without its child.
- The special path accepts any signed-small index: the comparison is
  unsigned and excludes every word at or above 00F1.
- 6000/6001 are established rate/volume units: the wrapper only forwards
  those words; it does not decide their meaning.
- A restoring wrapper requires unchanged live child DI: it reloads its
  saved word, whose backing still needs admission.

## How to reproduce

Reuse FND-SOUND-007's caller binding. Read resident_listing.py range
4654:04FE..4654:056A, following both no-child gates, the unsigned
index boundary, ES changes, record-word selection, three pushes and
the constructed far call. Check saved-slot offsets from the prologue.
Use a bounded Ghidra match for 4654:04FE and 0xf1; do not infer
runtime pointer identity from its flattened pointer expressions.

Retain sound-entry-lookup-contract.txt and sound-entry-lookup-ghidra
reports under GAME_DIR/analysis/handler-census-20261106/. No original
game, emulator session or emulated call supplied evidence here.
