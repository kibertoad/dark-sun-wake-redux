---
id: FND-EXE-250
title: Header-dispatched transfer wrapper propagates read carry but clears optional rewrite results
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:04C6..4AE5:04F4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:05A4..4AE5:05BE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:05BF..4AE5:061F
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

The candidate at `4AE5:04C6` covers fourteen instructions and forty-six
bytes. Its two near calls target FND-EXE-248's transfer helper at
`4AE5:03E8` and FND-EXE-249's rewrite helper at `4AE5:0421`. It returns
near without additional argument cleanup.

It loads SI from current ES-relative word eight, clears DI, then adds
word `0x000A` to SI with carry into DI. Thus the request count is the
full unsigned sum of two sixteen-bit words, up to 131070, not a wrapped
low-word sum alone. Words four and six supply DX and CX for the seek
position; word `0x0010` supplies AX for the destination segment. It calls
the transfer helper, immediately branches to return on carry set and does
not explicitly undo writes already requested there.

On carry clear it reloads CX from current ES-relative word `0x000A`.
Zero skips the rewrite helper; nonzero calls it. Both continuations
explicitly clear carry before returning. The rewrite helper's returned
flags therefore do not serve as this wrapper's error status, and the
reloaded count is not the earlier retained sum. ES identity, header
preservation through DOS effects, count writers and stack aliases remain
native admission obligations. In particular a reloaded count of one
retains FND-EXE-249's shifted-zero edge case.

FND-EXE-226's direct callee at `4AE5:05A4` covers forty-two instructions
and 122 bytes across the two intervals above, excluding the one-byte gap
at `4AE5:05BE`. It first increments DS-relative word `0x011C` modulo
65536 and tests ES-relative word `0x0010` for zero. The nonzero arm sets
header byte `0x001B` to one, sets bit two of byte `0x001A` and proceeds
to the outer trampoline writer at `4AE5:0672`.

The zero arm sets bit three of header byte `0x001A`, calls
FND-EXE-228's publisher at `4AE5:055A`, saves DS, decrements returned AX,
loads that segment into DS and writes current ES to its word `0x000E`,
then restores DS. It next makes a computed near call through current
ES-relative word `0x0018` at `4AE5:05D1`. This carries an offset in the
current code segment, not a stored far pointer. FND-EXE-245 records one
writer of literal `0x04C6` to that header field, but neither that writer
nor this conditional CFG proves the field's actual value at this call.

Carry set after the computed call selects a far tail transfer at
`4AE5:061A`; carry clear calls FND-EXE-231's helper at `4AE5:0735` and
then the outer trampoline writer. Earlier counter, header-flag and
preceding-segment publications are not explicitly rolled back on this
carry-error branch. If the computed call actually targets the wrapper
under the admitted current code segment, its transfer carry determines
this branch, while its optional rewrite return flags are discarded.

After the writer, the handler samples header byte `0x001A` masked to two
bits and adds it to byte `0x001B` at byte width. It saves ES, calls
FND-EXE-229's difference helper and loads ES from DS-relative word
`0x012C`. Each following loop test reads word `0x001C` into CX, exits
on zero, and otherwise compares AX unsigned against DS-relative word
`0x0118`, exiting when AX is at least that threshold. The loop's other
arm saves CX and AX, tests current header byte `0x001B`, and either
clears AX or calls FND-EXE-235's rewrite helper followed by
FND-EXE-230's size helper. It pops the retained AX into CX and the saved
header link into ES, adds CX to current AX modulo 65536 and repeats.
The final pop restores the initially saved ES if saved storage remains
intact. The link, threshold and helper results have their own provenance
and termination obligations; arithmetic growth is not assumed.

## Interpretation

This connects the candidate wrapper to an explicit computed-call consumer
and distinguishes three results: transfer carry, ignored optional rewrite
flags, and the handler's later post-call publication/loop behavior. It does
not equate a historical field writer with a resolved live dispatch target,
or treat the earlier publications as transactional on failure.

Q-EXE-001 and Q-EXE-010 retain live header-field and segment writers,
computed target admission, DOS effects, independent extents, saved-stack
aliases and caller/loop bounds. No complete_reading or replacement
inventory is established.

## Alternatives

A wrapped sixteen-bit request total is contradicted by carry propagation
into DI. Propagating optional rewrite flags is contradicted by explicit
carry clearing. Treating the header dispatch as a far call ignores its
near encoding. Treating the tail error as undoing prior state ignores
the publications before the call.

## How to reproduce

At revision `d1ceea5`, use FND-EXE-236's source identity, original-source
region and default x86-bounds limits, with no seeds or summaries. Set
entry and sole entries independently to `0x00040516` and `0x000405F4`.
Check the first interval `0x00040516..0x00040544`, fourteen instructions
and two direct calls. Check the second body's intervals
`0x000405F4..0x0004060E` and `0x0004060F..0x0004066F`, forty-two
instructions, six direct calls, computed call and near/far exits. Calls
are assumed to return; complete fields do not resolve the computed call
or establish Standard complete readings.

Independently decode both bodies from the shipped source in sixteen-bit
mode with Capstone. Follow request widths, carry consumers, segment writes,
ordered publications and retained values through loop re-entry. Compare
FND-EXE-245's field writer without admitting its value at a later call.
Keep source, configurations and reports in GAME_DIR.
