---
id: FND-CONFIG-028
title: Start Game button branch delegates setup without direct settings writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 8BF3:0128..8BF3:0325
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 8BF3:0504..8BF3:0598
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV overlay 194 and selected callees; ReportFbovOverlayMap.ps1; FBOV descriptor and fixup inspection
environment: null
---

## Observation

The overlay 194 callback at `DSUN.EXE+0x00081130` dispatches one event value
through four consecutive button IDs starting at 19300. Its jump table sends
19300 (`RESOURCE.GFF#WIND/19500`'s Start Game button, FND-UI-024) to
`DSUN.EXE+0x0008126E`. That branch repeats two resident calls three times,
then invokes a local setup helper once with argument 1, updates a few
separate state fields and calls other routines before returning through the
callback's common exit.

The helper at `DSUN.EXE+0x00081634` takes an argument of 1 from this branch.
Its far-call segment operands are FBOV fixup encodings, not resident segment
numbers. Resolving them through the segment descriptor table gives these
targets in call order:

| Path | Resolved targets |
|---|---|
| Argument 1 | `2D72:0B84`, `0BF3:4FEB`, `344C:0008`, `2796:0514`, `344C:0092`, `2D72:0942` |
| Common exit | `1C5F:0182`, `46BD:0043` |

The `2796:0514` entry sends consecutive three-byte values to VGA ports
`0x3C8` and `0x3C9`. The `46BD:0043` entry is a trampoline into overlay
182 at `DSUN.EXE+0x00068978`, whose first branch tests a passed pointer
before calling further routines. The same setup helper also has an
argument-0 path. Neither the Start Game branch nor this helper directly
addresses the saved settings globals named in FMT-CONFIG-003, the runtime
voice gate `DS:14E4`, or the message-delay word `DS:26B7`.

After the helper returns, the Start Game branch passes `DS:140C` to overlay
187's entry at `DSUN.EXE+0x00072254` and passes its double-word return to
the entry at `DSUN.EXE+0x00071E9E`. It later calls overlay 182's entry at
`DSUN.EXE+0x000699B7` once with zero and the entry at
`DSUN.EXE+0x000693A2` with successive arguments 0 through 3. These are the
resolved targets of raw far-call operands `05D8:00DE`, `05D8:00E3`,
`05B0:00AC` and `05B0:0098`. FND-VIDEO-007 independently observes the two
overlay 187 entries in a region path, without establishing their effects.

## Interpretation

The button callback does not itself establish new-game settings values. Its
named callees, later game-start paths, and indirect writes remain possible
initialization points for Q-CONFIG-002 and Q-CONFIG-007.

## Alternatives

The callback's event parameter and the named callees' full effects were not
established here. The button ID identifies a branch, but this reading alone
does not show that an ordinary click always reaches it. An unexamined callee
may initialize settings or copy a larger block of state.

## How to reproduce

Use `ReportFbovOverlayMap.ps1` on the approved `DSUN.EXE` to locate overlay
194 at `0x00081130`. Disassemble its callback at `0x00081130..0x00081456`,
including the jump table at `0x00081457`, and the local setup helper at
`0x00081634..0x000816C8`. Follow the button-ID comparison at `0x00081258`
and the first jump-table target. Resolve the helper's far-call segment words
as FBOV descriptor indexes shifted left by three, then read each descriptor's
resident segment or overlay header. Inspect the bounded entry contexts at
`2796:0514` and overlay 182's trampoline `46BD:0043`. Compare the button ID
with FND-UI-024. Continue the first button branch through
`0x000812CF..0x00081322`, resolving four further FBOV far-call fixups to
overlays 187 and 182.
