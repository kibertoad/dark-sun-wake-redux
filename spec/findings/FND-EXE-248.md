---
id: FND-EXE-248
title: Post-bound content transfer ignores seek carry and rejects short reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0371..4AE5:039A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:03E8..4AE5:0421
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-246's content-transfer target has twenty-eight instructions and
fifty-seven bytes, with two DOS interrupts, no calls and a near return
without argument cleanup. It saves incoming AX, loads BX from current
DS-relative word `0x0128`, selects service `0x4200` and interrupts with
incoming CX:DX. It then restores AX and saves DS without testing the seek's
carry result. The saved AX becomes the destination segment in DS.

At each read request, CX is `0xFFF0` when DI is nonzero, otherwise SI;
DX is zero and AH selects service `0x3F`. There is no initial zero-count
exit: even DI:SI equal to zero reaches the read interrupt. Read carry set
branches directly to restoration and return. Otherwise the unsigned
comparison of returned AX against current CX rejects a smaller count;
that comparison sets carry on the rejected path. Counts equal to or larger
than current CX proceed, rather than requiring exact equality.

The accepted returned AX is subtracted from SI with borrow into DI at
sixteen-bit component widths. The remaining components are combined into
AX by an OR. A zero result exits with carry clear and AX zero. Otherwise
the next iteration advances DS by `0x0FFF` paragraphs modulo 65536 before
requesting another read. This fixed segment step does not depend on the
returned count. With conventional preserved count registers and exact
successful counts, it advances by the requested `0xFFF0` bytes. Excess
returned counts instead participate in wrapping subtraction; their absence
has not been established from the instructions alone.

On the explicit error exits AX retains either the interrupt's value or the
short count used by the comparison. Restoring DS does not itself change
flags. Restoration of saved storage is conditional on stack integrity;
no absence of buffer, stack or interrupt-time aliases is claimed. The
interrupts' register effects and actual file writes are not supplied by
this static CFG. Statements about requests use the registers immediately
before each interrupt, and subsequent arithmetic uses their actual values
when it returns.

The caller loads AX from DS-relative word `0x0124`, subtracts that lower
segment from SI modulo 65536 and skips downstream work if zero. Otherwise
it rotates the difference left four and splits it into DI:SI, yielding the
unsigned sixteen-bit segment difference multiplied by sixteen. AX still
holds the lower segment. ES comes from DS-relative word `0x012C`; words
four and six supply DX and CX respectively. After the near call at
`4AE5:0395`, carry set branches to `4AE5:03DA`; carry clear continues by
reloading the state header. It therefore consumes the helper's carry
result, not a returned byte total or a separately retained seek status.

## Interpretation

This identifies the seek/read boundary and actual caller consumption for
one downstream dependency. A successful conditional instruction traversal
is not evidence that a seek succeeded, all requested bytes were written,
or native buffers and register contracts are valid. Seek failure is not
immediately propagated by a dedicated test. The reading does not replace
FND-EXE-007's separate-wrapper caller search with a universal service search.

Q-EXE-001 and Q-EXE-010 retain handle and offset writers, native segment and
count admission, DOS register effects, saved-storage aliases, destination
extent and other incoming transfers. No complete_reading is established.

## Alternatives

An immediate seek-error return is contradicted by the uninterrupted path
to the first read. Requiring exact read counts is contradicted by the
unsigned smaller-than test. A no-op zero request is contradicted by the
absence of an entry zero-count guard. Safe segment progression requires
more than the fixed step and conditional CFG alone.

## How to reproduce

At revision `75b41f4`, use FND-EXE-236's original-source identity, region and
default x86-bounds limits. Set entry and sole entries value to `0x00040438`
(`4AE5:03E8`), with no seeds or summaries. Check the covered interval
`0x00040438..0x00040471`, twenty-eight instructions, near return and DOS
interrupt boundaries at `4AE5:03F0` and `4AE5:040C`. The report assumes both
interrupts return; its complete field is not a Standard complete reading.

Independently decode that shipped-file interval in sixteen-bit mode with
Capstone, and the caller interval `0x000403C1..0x000403EA`, starting at
`4AE5:0371`. Check operand widths, unsigned carry branches, segment writes,
request setup and the carry consumer without executing the game. Keep the
source, configurations and listings in GAME_DIR and native assumptions open.
