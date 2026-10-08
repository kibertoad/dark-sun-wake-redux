---
id: FND-EXE-228
title: Resident publisher writes the live trampoline segment from a post-call state-word reload
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:055A..4AE5:05A4
tool: Ghidra 12.1.3 PUBLIC, executable-reader 2.5.0 and engine 13.6.0
environment: null
---

## Observation

The resident procedure at `4AE5:055A`, called by the shared handler callee
before FND-EXE-227's trampoline writer, supplies an explicit store to the
writer's segment-source field. Its conditional traversal has 29 instructions
and 74 bytes, ending at `4AE5:05A4` exclusively. Its near return at
`4AE5:05A3` adds no argument cleanup. All seven direct calls are followed
under explicit callee-return assumptions:

| Site | Target |
|---|---|
| `4AE5:055F` | `4AE5:07AD` |
| `4AE5:0568` | `4AE5:0637` |
| `4AE5:057E` | `4AE5:061F` |
| `4AE5:0581` | `4AE5:07A1` |
| `4AE5:058B` | `4AE5:06E4` |
| `4AE5:058E` | `4AE5:0735` |
| `4AE5:0592` | `4AE5:0785` |

The procedure saves ES at entry. Its local paths include a repeated
comparison between a retained word and a callee result, with
different work selected by flags and an ES-relative byte at offset `0x001B`.
That word and flags are saved and restored around the loop's work.
This is a local control-flow reading, not an admitted allocation contract
or termination proof.

At the common completion suffix, `4AE5:059A` restores flags and
`4AE5:059B` restores ES from the saved stack word. Then `4AE5:059C`
loads the current DS-relative word at offset `0x0120`, and `4AE5:059F`
stores that word into the current ES-relative word at offset `0x0010`.
There is no intervening call between that reload and store. Thus the
outgoing segment word has a concrete last read and writer rather than
being inferred from an earlier register result.

The traversal has no CFG gaps and reports complete discovery, conditional
on all seven calls returning. It does not read their effects, prove they
preserve the saved ES slot or DS binding, or establish the state word's
writers and lifetime.

## Interpretation

This locates the producer store for FND-EXE-227's live segment source.
To identify it as the loaded overlay's native code segment still requires
the current DS state-word provenance, saved-header ES admission, called
routine effects, computed sizes/storage bounds and actual loader entry. These
remain Q-EXE-001 and Q-EXE-010. Numeric segment values alone do not prove
which allocation or source bytes they name. No complete_reading declaration
or inventory replacement follows.

## Alternatives

An earlier callee's register result is not the final segment store's
immediate source: the completion suffix reloads the state word afterward.
Saving ES at entry does not prove its stack slot survives every callee
or that restored ES identifies the intended live header. A complete local
CFG with returning-call assumptions does not establish loop termination
or the allocation meaning of the returned words.

## How to reproduce

At revision `d7c6fb8`, use FND-EXE-226's original-source x86-bounds region,
source hash and default limits, setting entry and sole region entries value
to `0x000405AA` (`4AE5:055A`). Supply no seeds or callee summaries and
retain every assumedContinuation. In the original resident Ghidra snapshot,
read-only with automatic analysis disabled, run engine 13.6.0
ReportInstructionWindow at `4AE5:055A`, count 28. It prints through the
segment-field store; the source bounds report includes the final near return.
Read the saved ES, loop saves/restores and common completion in execution
order, but do not infer callee preservation from the listing. Source, configs,
listings and reports remain in GAME_DIR and are not committed.
