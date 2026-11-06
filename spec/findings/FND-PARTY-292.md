---
id: FND-PARTY-292
title: The pinned built-in file-service source preserves live DI under explicit routing and ordinary-file conditions
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0064..44DE:0086
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:01AF..44DE:02A9
tool: SRC-RUSTDOS-181 public source at 3bdb0eff305cf827c07414ce30b9616ef450562c; reuse original wrapper readings FND-PARTY-289/290/291 and tools/Install-RustDos.ps1 at 47e35d46
environment: null
---

## Observation

Q-PARTY-134 retains DI across the reached INT 21h AH=3F/40/42
requests in FND-PARTY-291. The original wrappers do not save DI.
This follows the public source of the installed-runtime tag, recorded
separately in SRC-RUSTDOS-181; it is not original-game execution.

### Ordinary-file service result

For the built-in implementation at the exact source revision, the
three ordinary-file branches and their reached CPU-facing helpers
do not assign DI. Read and write return through AX/carry; seek
also assigns DX. Underlying handle-table operations, disk reads/writes/seeks and drive
accounting access the bus, without receiving the CPU register object.
Common DOS entry/exit and ordinary HLE return likewise leave DI
unchanged. This answers the source implementation's live-register
question for an ordinary fault-free file call.

The result is conditioned on the interrupt actually reaching that
built-in service, ordinary file-handle identity, admitted normal return
and the source corresponding to the release binary. Neither the
installer's tag nor the original wrapper's AH value proves those
conditions. The built-in default vector exists, but real-mode interrupt
entry consults the current vector. Guest hooks and separately booted
DOS are different routes.

### Memory and interleaving remain separate

The DOS entry's caller-save routine writes eighteen bytes below
the current SP and two words into the process PSP when its real-mode
process condition holds. It does not move SP or later restore DI from
that copy. Read services write their destination, and DOS bookkeeping
writes its state area. Therefore unchanged live DI does not establish
unchanged caller saved words or frames; those specific memory effects
still require admitted disjoint backing and preserved identities.

The common dispatcher can delay an ordinary disk-service return.
Its wait code leaves DI untouched but enables interrupts. A guest
handler running meanwhile can have independent register/memory effects.
Paging faults, retry paths and console/device behavior are outside
the ordinary-file condition. They are not silently included in the
no-DI-assignment result.

## Interpretation

The exact source's ordinary-file live-DI behavior is answered.
Q-PARTY-134 retains source/release correspondence, actual vector and
file-handle admission, saved-memory/PSP/DOS-area separation and
intervening-handler preservation. It no longer needs an unspecified
guess about whether this built-in handler's ordinary file branch
assigns DI. The original-platform and all-runtime claims remain open.

No actual original caller domain or whole getter site is closed by
this external contract. The eighty other sites, three word clients,
repeated-hook preservation and indirect/bypass scope remain unchanged.
FMT-PARTY-007 stays supported; no queue closure, parity promotion
or complete reading follows from public emulator source.

## Alternatives

- A source function with no DI assignment is not proof the active
  guest interrupt vector reaches it.
- An interrupt's own unchanged register does not exclude memory
  aliasing or an intervening handler's effects during delayed return.
- A matching release tag is not an independently verified reproducible
  build; the source and executable identities are recorded separately.

## How to reproduce

Use SRC-RUSTDOS-181's immutable commit links and bounded source
table. Follow the ordinary-file branches through handle/result helpers,
DOS bookkeeping, HLE dispatch/return and disk-delay alternatives.
Keep console/device branches and fault/retry behavior excluded.
Compare the release tag/hash with tools/Install-RustDos.ps1, without
treating that comparison as source-to-binary verification. Reuse
FND-PARTY-291 for the original AH values and unsaved-DI boundaries.
No original game or emulator ran.
