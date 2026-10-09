---
id: FND-EXE-263
title: Wider scalar query finds CS-relative candidates and misses an independently known undisassembled link consumer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0647..4842:064E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0669..4842:066E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:067D..4842:0682
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:068C..4842:0691
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:031B..4AE5:031F
tool: scientific-method-engine 13.6.0, Ghidra 12.1.3 PUBLIC and Capstone 5.0.7
environment: null
---

## Observation

A memory-operand scalar query for `0x0122` across the saved installed
resident analysis snapshot returns four operands, all associated by the
analyzer with entry `4842:063A`. The result reaches its end without its
300-match cap. These are positive decoded-listing hits, not every original
access to an effective state slot.

Independent shipped-source decoding confirms that `4842:0647` writes
`0xFFFF` to CS-relative word `0x0122`, `4842:0669` compares DX against
that word, `4842:067D` writes DX to it and `4842:068C` loads it into DX.
The explicit segment override is CS, not DS. Their matching displacement
does not admit them as writers of FND-EXE-262's state-record link slot.
Native segment identity and effective storage would have to be established
before connecting these candidates to that consumer.

The query does not return FND-EXE-246's independently source-decoded
DS-relative link consumer at `4AE5:031B`. A bounded listing request at
that exact address reports no starting instruction: it is undisassembled
in memory block CODE_80, with the next defined instruction at
`4AE5:04F4`, and prints no window. This is an analyzer-coverage gap,
not missing original source bytes or evidence that the consumer is data.
The scalar search's advertised coverage is every disassembled instruction,
which does not include this known source consumer.

## Interpretation

This supplies positive wider candidates and an independently located
control demonstrating why their finite result cannot establish absence
of other link writers or readers. It also prevents matching numerical
offsets from being treated as segment-qualified storage identity.
The existing source consumer remains recorded; the missing analyzer
instruction does not invalidate FND-EXE-246's direct source reading.

Q-EXE-001 and Q-EXE-010 retain actual state-link producers, native segment
bindings, indexed/bulk writes, unrecognized and undisassembled code and
lifecycle ordering. The query excludes immediate operands by choice and
cannot cover accesses with no matching decoded scalar, other program
images or native memory effects. No absence-of-writer conclusion,
complete_reading or replacement inventory is established.

## Alternatives

Treating the four listing hits as exhaustive original accesses is
contradicted by the independently known consumer outside analyzer coverage.
Treating their CS-relative word as the state DS-relative word solely from
`0x0122` ignores the explicit segment override. Treating an unprinted
instruction window as an empty original code range ignores the diagnostic
and source decode.

## How to reproduce

At revision `1b53bf1`, open the installed resident Ghidra snapshot
INST-resident/DSUN.EXE read-only with analysis disabled and the packaged
script path. Run ReportScalarConstants with arguments memory and
`0x122`. Check the four sites above, memory operand kinds and the
end-of-search message, with the tool's maximum 300 matches.

Against the installed source identity in FND-EXE-236, independently decode
in sixteen-bit mode with Capstone from shipped offset `0x0003DC5A`
through exclusive `0x0003DCD0`, initial IP `0x063A`. The model segment
`0x4842` has shipped base `0x0003D620` under MZ load segment `0x1000`
and header size `0x5200`. Restrict the stored/accessed-word claims to
the four location intervals, not the unread rest of the routine.

Run ReportInstructionWindow at `4AE5:031B`, count two, in the same
read-only snapshot and retain its undisassembled-start diagnostic.
Compare FND-EXE-246's independent source instruction as the control.
These are positive search and coverage observations, not a negative
reference proof. Source, configurations and reports remain in GAME_DIR.
