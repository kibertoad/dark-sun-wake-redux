---
id: FND-PARTY-298
title: A failed reflection getter tests vector-four offset bit fifteen, with different default and requested-handler flag results
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0003..2C5F:0051
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084C1C..0x00084C59
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2C77..1000:2DA2
tool: FND-ITEM-057, FND-PARTY-293/297 and retained resident_listing.py/overlay_listing.py reports; SRC-RUSTDOS-181 bios.rs lines 9..17, 102..109 and 145..183 at 3bdb0eff305cf827c07414ce30b9616ef450562c
environment: null
---

## Observation

FND-PARTY-297 excludes the mask-10 reflection flag for an intact DATA
1001 payload but retains the getter's failure result. This follows that
ordinary result through the already read caller; offsets are hexadecimal.

2C5F:0003 compares the reader result with AX=FFFF at 003F. Equality
branches to 001A/001C, which explicitly clear DX and AX before the
normal restoration/return. Thus an ordinary failed miss returns
0000:0000, regardless of the local output pointer's preceding value.
It leaves the prior cache key unchanged. Other accepted returns and
frame/continuation admission remain FND-ITEM-057's separate conditions.

Overlay 197 +182D/+182F copies returned AX/DX to BX/ES. No null
test intervenes before +1831 tests word ES:[BX+11] against 0080.
For the zero result, this is a word read at real-mode 0000:0011.
The interrupt-vector table has four bytes per vector, with its offset
word first. Vector four's offset is at 0000:0010..0011; the following
segment word begins at 0012. Therefore the tested low-word mask bit
is bit seven of the byte at 0011: bit fifteen of vector four's offset.
The segment byte also read by the word operand cannot affect mask 0080.
This identifies the field used, not an observed low-memory value.

### Two known vector representations give opposite flag results

| Admitted vector-four state | Offset | Mask-10 flag result after zero getter return |
|---|---|---|
| Exact pinned runtime source's default table | FF53 | Set; the next caster test is reachable by this flag condition |
| Game's known requested replacement from FND-PARTY-293 | 2B6A | Clear; this test falls into the mask-20 path |
| Restored, replaced or otherwise modified vector | Not admitted here | Determined by offset bit fifteen, not by DATA 1001 |

SRC-RUSTDOS-181's BIOS source initializes vectors to a common IRET
pointer and leaves vector four at F000:FF53: it is absent from the
service-vector list and later overrides. That source table is a
conditional external-runtime contract. It does not prove the installed
release's live table or the game's execution state. The game's setter
argument likewise proves a request, not successful installation or the
active vector. Release correspondence and vector writers remain open.

### Passing the flag is not successful reflection

Even with the default bit set, the caller still checks the caster and
calls overlay 208 +0885 before its result can reach the forwarded-number
queue branch. Current DI, the helper's result, queue publication and
later client preservation remain separate conditions. A null-result
flag pass does not prove DATA 1001 is ever queued in a native run.
Likewise the clear replacement bit does not bypass the separately gated
mask-20 continuation of FND-PARTY-288.

## Interpretation

The failed-miss flag is now a precise vector-four field obligation.
It cannot be dismissed as the intact payload's zero flag or universally
excluded on the pinned runtime's initial table. Actual vector-four
admission, other getter content/key histories and later caller conditions
remain Q-PARTY-134. Q-PARTY-124 retains the getter access itself.

This is a bounded static failure-path description, without a run,
failure injection or claim that an invalid runtime state occurs.
FMT-PARTY-007 stays supported. No complete reading or queue item closes.

## Alternatives

- Treating a failed getter as skipping the test is contradicted by the
  caller's direct use of its returned pointer without a null branch.
- Treating the failure as reading DATA 1001 is contradicted by the
  getter's explicit zero result and the real-mode effective address.
- Both a set default flag and a clear requested-handler flag can fit
  the known code; resolving the actual vector selects between them.

## How to reproduce

Reuse di-preservation-data-getter.txt and reflected-cast-producers.txt
under GAME_DIR/analysis/class-table-closure-20261106/. Check getter
003F..0042 and 001A..001E, then reflection +1827..+1859. Derive
the vector-table field from the zero pointer, four-byte stride and
little-endian mask. Compare FND-PARTY-293's vector-four setter argument.
Read SRC-RUSTDOS-181's immutable bios.rs constant, vector list and
default_ivt construction, retaining source/release and active-state limits.
No original game, emulator session or emulated call ran.
