---
id: FND-SOUND-072
title: Exact music-mode and region census has three instruction interiors and one unresolved gap candidate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:0406..2834:041A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:4723..1BF3:4C09
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071B1E..0x00071B60
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DAEE..0x0008DB22
tool: committed exec-census readers at dfb2b5dc; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

This classifies the four raw candidates retained by FND-SOUND-071, using
the same installed DSUN.EXE (634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b). It does not change its fifteen listed
mode operands, twenty-seven listed region operands or five region stores.

The mode candidate 2875:0000 aliases 2834:0410. On the selector's
recorded stream it is the final displacement byte of the near call at
2834:040E, preceded by PUSH CS. That call ends at 0411, where the next
instruction reads DS:08BC. The raw decoding is not an additional mode
write along this path.

The region candidates overlay 187 +1D1E and overlay 204 +1D5D skip the
operand-size prefix of instructions at +1D1D and +1D5C respectively.
Both containing instructions zero-extend the word at DS:140C into EAX.
The connected streams start at +1CEE and +1D2E. FND-EXE-639 records the
former's entry and region-resource path. These are duplicate interior
decodings of known reads, not additional accesses along those streams.

The remaining region candidate 207B:000B aliases 1BF3:488B, file offset
0x159BB. The inventory function beginning 1BF3:4723 ends with RETF at
4888; its recorded range ends at 4889. The next inventory root is 4C09.
Thus 488B lies in an ownership gap, not in that preceding function's
established instruction stream. The return alone does not prove data.

A provisional sequential decoding beginning at 490D reaches a return at
4C08 and includes byte lookups at 4ABE and following sites with bases
4889, 48AA, 48CB and 48EC, spaced 33 bytes apart. This supports a table
interpretation of 488B, but no established incoming binding for 490D was
found in the tested direct calls or nearby relocated offset/segment pairs.
The 4723 target supplies positive binding controls. These searches omit
computed targets, distant pairs, near entries and changed code, and do not
establish that the provisional decoding is reached.

## Interpretation

Three candidates leave the unclassified set for the recorded instruction
streams. One remains: determine the incoming ownership and table/entry
interpretation of the 4889..4C09 gap. This is part of Q-SOUND-004's
remaining mode/region census, not a new established region reader or writer.

## Alternatives

Treating every raw candidate as another executed access is contradicted
by the three containing instructions. An alternative incoming transfer to
an interior byte is not excluded by this classification.

The gap may contain lookup data followed by a reader at 490D; the four
spaced bases support that reading. An actual incoming binding and its
segment/index domains would test it. An executable path entering 488B
would instead support a real region-word access. Neither reading is settled.

## How to reproduce

Use tools/research/exec-census/ readers at dfb2b5dc with the installed
DSUN.EXE and coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv. Retained reports are
under GAME_DIR/analysis/handler-census-20261106/:

- Reuse music-exact-mode-region-window.txt from FND-SOUND-071.
- resident_listing.py range 2834:0406..2834:041A writes
  music-mode-interior-stream.txt.
- overlay_listing.py descriptor 187 file 71B1E..71B60 and descriptor 204
  file 8DAEE..8DB22 write music-region-prefix187-stream.txt and
  music-region-prefix204-stream.txt.
- resident_listing.py ranges 1BF3:4723..1BF3:48D0 and
  1BF3:490D..1BF3:4C09 write music-region-candidate-owner-stream.txt and
  music-region-lookup-reader-stream.txt. Decodings after the former's
  RETF and the entire provisional latter stream are diagnostic.
- direct_callers.py target 1BF3:490D writes
  music-region-lookup-owner-callers.txt. alias_far_words.py targets
  1BF3:490D and 1BF3:4723 writes music-region-lookup-owner-bindings.txt;
  preserve its exclusions and positive controls.

No original-game run or emulated call supplied this evidence. No whole
entry complete reading or question closure follows from these classifications.
