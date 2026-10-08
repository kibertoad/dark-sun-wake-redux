---
id: FND-EXE-247
title: Post-bound caller enters trampoline writer after its outer guards and checks its own count
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:03AA..4AE5:03B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0693..4AE5:06B1
tool: executable-reader 2.5.0, Capstone 5.0.7 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-246's caller tests current ES-relative word `0x000C` for zero at
`4AE5:03AA`. Equality skips the call; otherwise the near call at
`4AE5:03B2` enters `4AE5:0693`. The caller's optional call to `4AE5:0421`
occurs before this test, not between it and the interior entry. The interior
body contains thirteen instructions and thirty bytes, ending exclusively
at `4AE5:06B1`, with no calls and a near return at `4AE5:06B0` without
extra argument cleanup.

This entry bypasses FND-EXE-227's outer count guard, first-slot opcode guard,
word-two test and optional call to `4AE5:0753`. It begins by loading BX from
current ES-relative word `0x0010`, then CX from word `0x000C`, sets DI to
`0x0020` and clears direction. The caller's test and this reload are separate
reads. The near call's return-address stack store occurs between them;
native header/SS aliases and interrupt-time changes remain admission
obligations. No helper or explicit header store intervenes inside that gap.

Each iteration reads the old target word at ES-relative DI plus two into
DX before overlapping stores, writes a far-jump opcode byte, writes retained
DX as the offset word and BX as the segment word, then uses sixteen-bit
LOOP. The net DI advance is five modulo 65536. Neither the count nor segment
is reloaded within the loop. A loaded count n other than zero gives n
iterations if accesses complete; zero at the interior entry gives 65536.
These are conditional instruction semantics, not an observed native count
or proof of safe output storage.

On an explicit completed loop, CX is zero, BX retains the initially loaded
segment, AX holds that same segment from the last store, DX holds the last
read target and DI is the final advanced offset. The body has no explicit
DS or ES register writes. It does not preserve AX, CX, DX or DI. FND-EXE-246's
caller immediately reloads AX from ES-relative word `0x0010` after return,
so its following preceding-segment calculation uses that memory reload,
not the writer's returned AX. Its later far-callback setup overwrites AX
and BX, while the callback's remaining input contract is unread.

## Interpretation

This supplies an independently measured interior-entry body and the actual
caller's guard and result consumption. It narrows the new entry's contract
without treating it as identical to the outer helper. The outer path's
optional-callee count dependency is absent here, but native header/count,
stack aliases, interrupt changes and independent output bounds still require
admission. No preserved-register or safe-buffer conclusion follows solely
from the fresh caller guard or the absence of calls inside the loop.

Q-EXE-001 and Q-EXE-010 retain actual header and segment producers, count and
target writers, near-call frame and saved-storage aliases, output extent,
interrupt-enabled changes, other incoming transfers and callback arguments.
No complete_reading or replacement inventory is established.

## Alternatives

Applying the outer opcode guard or optional stack-word helper to this caller
is contradicted by its interior target. Treating returned AX as the source of
the caller's next segment calculation ignores the post-return memory reload.
Preserving all input registers ignores the count, offset and target outputs.
A fresh nonzero guard alone cannot establish alias-free safe destinations.

## How to reproduce

At revision `f8121bd`, use FND-EXE-236's original-source region, source hash
and default x86-bounds settings, setting entry and sole entries value to
`0x000406E3` (`4AE5:0693`), with no seeds or summaries. Check thirty covered
bytes, thirteen instructions, sole near return and absence of calls.

Use FND-EXE-246's bounded source decoding for the caller test, call and
subsequent reload. In the resident Ghidra snapshot, read-only with analysis
disabled, run ReportInstructionWindow at `4AE5:0672`, count 36, restricting
the interior body to the interval above. Compare FND-EXE-227's outer entry
separately; do not add its skipped guards or callee to this entry. Keep
native storage and successful-access assumptions conditional. Sources,
configurations and listings remain in GAME_DIR.
