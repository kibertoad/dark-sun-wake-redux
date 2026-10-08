---
id: FND-EXE-175
title: A resident overlay-loader candidate has a bounded body beyond the saved analyzer listing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0010..4AE5:0140
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

A bounded traversal starting at resident address `4AE5:0010` reaches
`4AE5:0010..4AE5:0134` and `4AE5:0135..4AE5:0140`: 303 instruction bytes,
with a one-byte hole. Its listed exit is the far return at `4AE5:013D`,
which additionally removes eight argument bytes. This is a traversal of an
explicitly selected candidate, not proof that any original caller enters it.

The traversal lists these direct call sites and targets:

| Site | Target |
|---|---|
| `4AE5:0029` | `4AE5:01C1` |
| `4AE5:0040` | `4AE5:01B5` |
| `4AE5:0045` | `4AE5:0206` |
| `4AE5:0063` | `4AE5:028B` |
| `4AE5:00A2` | `4AE5:028B` |
| `4AE5:00F3` | `4AE5:0140` |
| `4AE5:0107` | `4AE5:029B` |
| `4AE5:0126` | `4AE5:031B` |

The local instruction window in the saved resident analyzer snapshot cannot
start at `4AE5:0009`; its next defined instruction is `4AE5:0140`.
The bounded original-source decoder therefore supplies a candidate body that
the earlier saved listing does not supply. The bytes before the selected entry
are not included as instructions or treated as a preceding function.

The effect trace from the selected entry stops its nonzero-state arm at the
DOS interrupt in the first callee, `4AE5:01C3`, because its handler is not
modeled. The other arm reaches the far return but rejects its return width
against the unspecified root call frame. Neither arm establishes effects
after those stops or native reachability.

## Interpretation

This gives concrete resident targets to read for the overlay-loader question
Q-EXE-001 and the segment-admission question Q-EXE-010. The candidate's
association with overlay loading remains a lead, not an established loader
contract. Its input storage, caller admission, DOS results, segment producers
and callee effects must still be read. The bounds reporter's complete CFG
result assumes every call and interrupt returns; it is not a complete reading.
No inventory replacement or status promotion follows.

## Alternatives

The saved analyzer listing alone is insufficient to exclude resident code
in this interval. Conversely, a selected entry and well-formed return do not
prove a native procedure boundary. A different entry or data interpretation
remains possible until incoming transfers and input admission are established.
FND-EXE-007's earlier limited DOS-wrapper search remains historical evidence;
this observation does not retroactively make that search exhaustive.

## How to reproduce

Use the installed original whose XXH3-128 is
`e296af55ba2ecde7e77f555c90f33d0b`. Run the committed wrapper
`node tools/evidence/report.mjs x86-bounds <local-config.json>` and separately
`x86-trace` with sourceKind `mz`, entry file offset `262240` (`0x00040060`),
and one resident region: start `262224` (`0x00040050`), exclusive end
`266320` (`0x00041050`), segment `19173` (`0x4AE5`), ip `0`, entries
`[262240]`. Name the region candidate-prefix and record its evidence as an
exploratory original-source prefix with unresolved caller admission. Supply
no register seeds, callee summaries or interrupt models. Defaults are 512
steps, 64 paths and depth eight; continuation defaults are 64 paths,
20,000 total steps, 512 per path, visit limit four and 4,096 string iterations.
The source reader derives relocations and format tables; do not supply them.

In the saved original resident project, with analysis disabled and read-only
mode, run ReportInstructionWindow at `4AE5:0009` with count 75. Its refusal
and next-instruction address are part of the observation, not a decoded window.
The two reported stops and every assumed continuation must remain visible.
Reports and configurations stay outside Git in the licensed local store.
