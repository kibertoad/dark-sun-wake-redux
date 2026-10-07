---
id: FND-EXE-120
title: Post-record nonzero-byte priority path publishes local flags before the shared counter suffix
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A461E..0x005A468B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A46BA..0x005A46F4
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A472A..0x005A4731
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4767..0x005A4782
tool: Ghidra 12.1.3 PUBLIC bounded post-record priority and local-flag publication reading
environment: null
---

## Observation

FND-EXE-116 records a fresh current-byte test after the second-record write;
nonzero selects `0x005A461E` with retained object O. This path zero-extends
O's bytes `0x0118` and `0x011C`, ORs mask four into the former, computes
full intersection M and publishes the modified `0x0118` byte. It publishes
`0x011E` by the first matching condition: mask four in M gives six;
otherwise mask sixteen gives twelve; otherwise mask one gives four;
otherwise mask two gives two; otherwise mask eight absent gives one and
present gives zero. Every arm joins at `0x005A4646` for a full M zero
test. Untested upper mask bits still affect that test.

M nonzero checks byte `0x011D`; already nonzero goes to `0x005A4680`.
Otherwise it tests `0x0123`, publishes one to `0x011D`, and only for
a nonzero tested gate freshly reads full `0x0110` and calls
`0x004F2060`. M zero instead checks `0x011D`; already zero joins
directly, otherwise tests `0x0123`, publishes zero to `0x011D`, and
only for a nonzero gate freshly reads full `0x0110` and calls
`0x004F20E0`. Each call receives that full field as its first argument.
The gate-test flags survive the state stores. A zero gate suppresses the
call without cancelling the direct state-byte update.

Both calls return normally to `0x005A4680` without a result predicate.
That join freshly reads the local-F byte at current ESP plus fifteen,
ORs it into byte O plus `0x0125`, and then jumps to `0x005A43DB`,
the shared counter suffix read in FND-EXE-118. In contrast, the original
current-byte-zero arm in FND-EXE-116 jumps directly to that suffix and
bypasses this `0x0125` publication. The publication uses local F, not
the merged local byte at plus thirteen from FND-EXE-117. No direct F
modification or second-record write occurs in this priority path; aliases
and callee effects remain unresolved. Nonreturning or exceptional calls do
not establish that publication or counter processing happens.

## Interpretation

Post-record nonzero byte selects priority/state processing followed by
fresh local-F publication before the counters; zero byte bypasses those
steps. Similar priority paths in FND-EXE-119 have a different entry but
normal joins with comparable publication order, without proving identical
field meanings or admission. Q-EXE-009 in FMT-EXE-006 still requires
record/object producers and aliases, callback slot/virtual targets, storage
bounds/lifetime and state-helper effects. No complete first-callee reading
or actual PATH outcome follows.

## Alternatives

- The initial byte branch distinguishes whether priority and F publication
  occur; it is not only an optimization of one common result.
- Selector zero can coexist with M nonzero when mask eight is selected.
- A suppressed state call still leaves the state-byte publication in place.
- The merged record byte does not replace F in this publication.
- Callee results do not control the normal join or provide a rollback test.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-116 independently supplies current-byte admission and the
`0x005A45FC` limit-35 window covering the main path through its join.
Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x005A46BA` limit 11,
`0x005A46E3` limit 5, `0x005A46F4` limit 1, `0x005A472A` limit 2 and
`0x005A4767` limit 8. Restrict observations to the declared ranges.
FND-EXE-118 supplies the independently read suffix at `0x005A43DB`.
Follow every priority destination, flags across state stores, fresh call
arguments, post-call local-F read and the differing zero-byte join. Keep
reports in GAME_DIR/analysis/exe-batches; commit no original listings or
bytes and execute neither interpreter nor game.