---
id: FND-EXE-245
title: Intervening descriptor scan computes the range-check threshold from wrapped header sizes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:029B..4AE5:02C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:02C8..4AE5:02E3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:02E4..4AE5:031B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:07AD..4AE5:07C5
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-244's intervening callee at `4AE5:029B` covers 126 instruction
bytes in three intervals, with 42 instructions and a near return at
`4AE5:031A` without extra argument cleanup. The two one-byte holes in
Locations are not reached by its declared traversal. Its only direct call
is at `4AE5:02FD` to `4AE5:07AD`, assumed to return.

The scan loads ES from a relocated immediate resolving to `57E0:0000`,
reads ES-relative word `0x0090`, adds sixteen at sixteen-bit width and
stores that value at CS-relative word `0x000E`. It then loads ES from a
second relocated immediate resolving to `55DF:0000`, clears BX and DI and
sets SI to `0x01A0`. Their segment-immediate sites are file `0x000402EC`
and `0x000402FC`; the MZ reader maps them to shipped-file bases
`0x0004D000` and `0x0004AFF0` under load segment `0x1000`.

At each position it requires bit one set in DS-relative word SI plus four
and a nonzero word SI plus two before further processing. Otherwise it
advances. For an eligible position, it reads word SI into AX, saves ES,
stores AX at the current ES-relative word `0x0012` and loads ES from AX.
If that header's byte `0x001A` equals `0xFF`, it restores saved ES, clears
that ES-relative word `0x0012` and advances. Otherwise it pops the saved
segment into AX rather than ES, writes literal `0x04C6` to current
ES-relative word `0x0018`, and adds current DS-relative words
`0x0114`/`0x0116` to ES-relative words four/six using ADD followed by ADC.

It calls the size helper and retains the unsigned maximum of BX and returned
DX by comparison and conditional exchange. It advances SI by eight and
repeats while SI is below `0x08C8`. Under the explicit returning-call flow,
this visits 229 positions; the size helper has no explicit SI or BX writes.
That count does not establish valid descriptor/header storage or exclude
aliases that change instructions or saved slots. At completion it clears
AX, adds two to BX at sixteen-bit width and stores BX into current
DS-relative word `0x011A`, the threshold later read by FND-EXE-244.

The size helper has nine instructions and 24 bytes, no calls or explicit
memory stores, and returns near at `4AE5:07C4` without extra argument cleanup.
For current ES-relative words w8 and wA at offsets eight and ten, it computes
AX as `((w8 + 17) modulo 65536) >> 4` and an initial DX as
`((wA + 15) modulo 65536) >> 4`, then adds AX to DX at sixteen-bit width.
Each component is at most 4095, so this explicit total is at most 8190
and the scan's maximum-plus-two is between two and 8192, absent other
changes. These are arithmetic bounds, not native input observations or an
allocation contract. The helper does not explicitly write DS or ES.

Neither routine explicitly targets DS-relative words `0x0120`, `0x0124`
or `0x0126`. This closes their direct fixed-displacement writer dependency,
not preservation against header/state aliases, stack writes or interrupts.
The scan changes ES as described and does not restore incoming ES on return.

## Interpretation

This identifies the intervening call's threshold producer and its complete
explicit size-callee arithmetic. It distinguishes initialized bounds from
the newly produced comparison threshold. The caller still reloads its bound
words; native inputs, storage aliases and preservation must be admitted before
concluding that those reloads necessarily equal their earlier stores.

Q-EXE-001 and Q-EXE-010 retain entry-frame inputs, native DS/header/descriptor
admission, every field writer, effective aliases, saved-stack integrity,
invocation order and interrupt-enabled changes. No complete_reading or
replacement inventory is established.

## Alternatives

A direct bound rewrite by this call is not supplied by its explicit stores.
An immutable comparison threshold ignores the final word-`0x011A` store.
Unbounded size rounding ignores the component additions' sixteen-bit wrap.
Restoring ES on every eligible path ignores the non-rejected path's pop into
AX. A fixed iteration count alone cannot prove safe indirect destinations.

## How to reproduce

At revision `10aa956`, use FND-EXE-236's source hash, original-source region
and default x86-bounds settings. Separately set entry and sole entries value
to `0x000402EB` and `0x000407FD`, with no seeds or summaries. Check the
126/42 and 24/9 byte/instruction results, both holes and the single call.

Decode only file intervals `0x000402EB..0x00040317`,
`0x00040318..0x00040333`, `0x00040334..0x0004036B` and
`0x000407FD..0x00040815` with Capstone 5.0.7 in x86 sixteen-bit mode,
initial IPs `0x029B`, `0x02C8`, `0x02E4` and `0x07AD`, after hash checking.
Inspect all paths, stores and register effects above. Use the committed
operand command with sourceKind mz, sites `0x000402EC` and `0x000402FC`,
targetOffset zero, checking both MZ relocations and source mappings.
Keep native storage and return assumptions conditional. Sources,
configurations and listings remain in GAME_DIR.
