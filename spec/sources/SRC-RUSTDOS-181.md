---
id: SRC-RUSTDOS-181
title: rust-dos v1.8.1 file-service, vector and XMS source snapshot
superseded_by: []
author: dividebysandwich and rust-dos contributors
date: unknown
location: https://github.com/dividebysandwich/rust-dos/tree/3bdb0eff305cf827c07414ce30b9616ef450562c
xxh3: null
licence: The repository LICENSE contains GNU GPL version 2; individual dependency licences are outside this reading.
---

## Use

Read on 2026-11-06 for Q-PARTY-134. GitHub's commit API resolves
tag v1.8.1 to 3bdb0eff305cf827c07414ce30b9616ef450562c.
tools/Install-RustDos.ps1 pins that tag and the release executable's
SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0.
This identifies the chosen source and installer release separately;
it does not independently verify their build correspondence.
The older SRC-RUSTDOS-172 remains evidence for its historical revision.

| File at the pinned source commit | Relevant lines | Scope |
|---|---|---|
| src/interrupts/int21.rs | 32..81, 99..164, 284..295 | Caller-memory save, handle lookup, result assignment, ordinary file operations and accounting. |
| src/interrupts/int21.rs | 463..497, 1466..1598 | Common DOS entry and AH=3F/40/42 dispatch branches. |
| src/dos_data.rs | 413..473 | DOS entry/exit and process-state synchronization. |
| src/interrupts/mod.rs | 26..118, 244..264 | HLE flags/return and vector-21 dispatch. |
| src/exec.rs | 1390..1465 | Recognized service dispatch, page-fault/retry and disk-delay alternatives. |
| src/diskio.rs | 169..218 | Delayed service return and wait loop. |
| src/instructions/control.rs | 209..234 | Software-interrupt dispatch. |
| src/cpu/fault.rs | 217..241 | Real-mode vector lookup and interrupt frame. |
| src/bios.rs | 9..17, 102..109, 145..183 | Default HLE and IRET vector mapping versus separately booted DOS. |
| src/interrupts/int2f.rs | 30..37 | XMS installation result and entry pointer query. |
| src/dos_data.rs | 19, 75..78, 171..173 | Fixed DOS segment, XMS entry offset and physical address calculation. |
| src/xms.rs | 21..26, 299..309, 324..327, 406..417 | Entry installation, result convention, AH selection and free-handle branch. |

The XMS files were additionally read at the same exact source commit on
2026-11-06 for Q-SAVE-007. Installation query4300 returns AL80; query4310
returns ES:BX=0160:1F00. Function0A takes DX as its memory-block handle.
An invalid or locked block reports AX0 and an error in BL; a valid unlocked
block is removed from the allocation and resident-handle records, then
reports AX1. The bounded branch makes no file-service request. This is
the source's own interface, not proof of every active guest hook or of
source/release build correspondence. FND-SAVE-040 separately records a
current native query, entered service and reported result.

The ordinary-file branches for read/write/seek publish AX and carry,
with seek also publishing DX. Neither those branches nor their
ordinary-file/accounting helpers assign DI. Handle and disk operations
receive the bus rather than the CPU register object. Console and other
character-device branches are not included in this contract.

Common DOS bookkeeping changes DOS/process metadata and guest memory,
not DI. In real mode with a process PSP, caller saving writes nine
register words below the current stack pointer and SS:SP into PSP
fields. This is a memory effect, not a later restoration of DI.
Read services also write the requested destination. Fault-free, valid
and separate backing remains necessary for those writes.

Ordinary HLE return updates control state, stack and flags without
assigning DI. Disk-delay entry/wait likewise leaves DI untouched,
but enables interrupts during the wait. Intervening guest handlers
are not covered by the service's own register result. The dispatcher
has separate paging-fault/retry and device-wait behavior.

Default real-mode vector setup can route INT 21h to the built-in
service. A separately booted DOS or a guest-replaced vector follows
another route. The source therefore supplies a conditional interface
contract for this exact built-in implementation, not an all-DOS ABI
claim or proof of the active guest vector.

For FND-PARTY-298, the same source's default vector four remains
F000:FF53: it starts at the common IRET pointer and is absent from
the service list and later overrides. Offset bit fifteen is set.
Vectors two, three and five likewise retain that default pointer;
FND-PARTY-299 uses only their identified bytes for a conditional copy.
This describes the source's initial table, not a live original-game
state or a verified source-to-release correspondence.

## Known errors

None identified in the bounded reading. The release binary has not
been independently matched to this source tree. Actual guest routing,
device handles, intervening handlers, invalid/paged memory, retries,
stack/PSP aliases and original-platform behavior remain outside this
contract. The source reading itself ran no emulator or original game; no source-only result
raises an original evidence status or declares a complete reading.
