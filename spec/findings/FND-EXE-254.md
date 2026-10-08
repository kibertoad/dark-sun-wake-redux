---
id: FND-EXE-254
title: First cleanup callback writer publishes only after a zero helper result and retains earlier flag changes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:08EB..4AE5:09CB
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-178's writer entry at `4AE5:08EB` contains ninety instructions
and 224 bytes, with two near calls and a far return without additional
argument cleanup. It saves BP and DS, forms its BP frame and loads DS
through CS-relative word five. BP-relative accesses use SS by default;
they are not automatically fields in that loaded DS segment.

A set bit zero of DS-relative byte `0x0010` selects AX `0xFFFF` and the
return path. Otherwise the SS-relative word at BP plus six selects two
routes. Nonzero reads CX from BP plus ten, rejects zero or unsigned CX
at most the word at BP plus eight, then sets bit zero of DS-relative byte
`0x0038`. That publication precedes later limits and helper calls, and
no explicit rollback appears on their rejection paths.

The zero-word route pushes CS and calls FND-EXE-179's `4AE5:0D8B`.
Returned AX zero rejects. Otherwise it loads BX from SS:BP plus eight,
rejects unsigned BX at least AX, subtracts BX from AX and loads CX from
SS:BP plus ten. CX zero or at least the remaining AX is replaced with
AX; a smaller nonzero CX is retained. These decisions use returned values
and current argument storage, not independently admitted native capacities.

The common path loads ES through CS-relative word seven and reads its
words `0x356C` and `0x356E` into AX and DX. It adds `0x3FFF` at component
widths with carry and divides unsigned DX:AX by `0x4000`. There is no
explicit quotient-overflow guard here. If that instruction completes,
CX is reduced to the quotient when larger. For CX below four it computes
AX as `0x0400` times CX and rejects if AX is unsigned below DS-relative
word `0x011A`; CX at least four skips this comparison. Rejection selects
AX `0xFFFF`, not a rollback of earlier flag changes.

It pushes SS:BP plus six and plus eight, stores the selected CX back to
SS:BP plus ten, then pushes CX and CS before the near call at
`4AE5:0972` to `4AE5:0E3D`. FND-EXE-179 records that helper's far return
with six bytes of additional cleanup. Returned AX nonzero goes directly
to restoration with that result, before callback publication. The earlier
argument-word store and any helper effects remain. DS identity and saved
storage across the helper are not established by its restoration encoding.

Only returned AX zero enters the final publication sequence. It saves DI,
sets DI to `0x0140`, copies current DS to ES through a push/pop and clears
direction. It multiplies current SS:BP plus eight by `0x4000`, retaining
its full DX:AX product in CX:BX while writing its low and high words through
ES. It multiplies current SS:BP plus ten by `0x4000`, adds that retained
product with carry and writes the resulting low and high words. It then
writes the retained first product again. Thus six consecutive words cover
ES-relative `0x0140..0x014C`, in first-product, sum, first-product order.
The argument values are fresh post-helper reads, not the previously pushed
values assumed preserved.

It writes `0x0A4B` to DS-relative word `0x014C`, zero to word `0x014E`,
one to word `0x0112`, sets bit one of byte `0x0010`, stores callback offset
`0x0EA2` to word `0x0084` and stores `0x0D11` to word `0x0080` in that
order. It clears AX, restores saved DI, DS and BP and returns far. No call
intervenes between the DS-to-ES copy and these publications, but stack,
source/destination and argument aliases remain native obligations. This
body does not explicitly set bit two of byte `0x0038`, which is the bit
FND-EXE-253's first replacement cleanup target tests.

## Interpretation

This supplies the writer's branches, actual helper-result gate, ordered
state publications and fresh argument reads. A zero setup result is a
local consumer condition, not proof of successful native allocation,
external operations or alias-free storage. The early flag and modified
argument can survive rejected setup independently of callback publication.

Q-EXE-001 and Q-EXE-010 retain incoming frames, segment bindings and
preservation through both callees, every flag/word writer, arithmetic and
output bounds, argument last writers and the connection to live cleanup
slots. FND-EXE-253's cleanup gate still requires its own producer reading.
No complete_reading or replacement inventory is established.

## Alternatives

Unconditional callback publication is contradicted by the helper-result
test. Transactional rejection is contradicted by the earlier flag and
argument stores. Retaining the originally pushed values for final products
ignores their post-call memory reloads. The setup flag set at byte `0x0038`
is not the cleanup target's tested bit.

## How to reproduce

At revision `678e8b8`, use FND-EXE-236's original-source identity, region
and default x86-bounds limits. Set entry and sole entries to `0x0004093B`,
with no seeds or summaries. Check interval `0x0004093B..0x00040A1B`,
ninety instructions, far return and near call sites `4AE5:091A` and
`4AE5:0972`. The traversal assumes both callees return; its complete field
is not a Standard complete reading or proof that division succeeds.

Decode that interval directly from the shipped source in sixteen-bit
mode with Capstone. Track SS-relative argument accesses, unsigned branches,
component-width arithmetic, saved call frames, fresh post-call reads and
ordered DS/ES publications. Compare FND-EXE-179's return cleanup and
FND-EXE-253's distinct flag gate separately. Keep source, configurations
and reports in GAME_DIR.
