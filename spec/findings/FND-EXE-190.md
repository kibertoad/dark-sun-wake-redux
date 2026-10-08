---
id: FND-EXE-190
title: Shared record producer separates new allocation fields from adopted-pointer publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006005B0..0x006007F3
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-188's shared-state initializer at `0x006005B0` saves EBP, EDI,
ESI and EBX and reserves `0xAC` stack bytes. It reads the pointer at
`0x0242F640` into EBX. Nonzero restores its saved registers and returns
at `0x006005CD`. Zero builds a bounded local query and calls
`0x00602540`; it tests only the returned AX. Nonzero masks EAX to its low
word and calls `0x00600530`, then takes that EAX as the pointer to publish.
The helper's pointer admission and input contract remain unread.

For zero AX, the initializer calls `0x00601CF0` with request `0x3C`,
stores returned EAX in ESI and tests EAX at dword width. Zero calls
`0x006020C0` on an unresolved failure path. Nonzero clears the direction
flag and performs fifteen dword stores through ES:EDI using EBX, then
writes fields through ESI. EBX originated from the zero entry-pointer
test, but preservation through intervening calls and equality of the ES
write domain with later field accesses still need admission. This is a
60-byte requested count and a separate 60-byte repeated-store count,
not proof of admitted allocation capacity or initialized storage.

The newly allocated path explicitly writes these fields:

| Record offset | Width | Local producer |
|---|---|---|
| `0x00` | dword | `0x3C` |
| `0x04` | dword | stored target `0x006020C0` |
| `0x08` | dword | stored target `0x00600520` |
| `0x28` | dword | zero |
| `0x2C` | dword | dword read at `0x0242C930` |
| `0x30` | dword | `0xFFFFFFFF` |
| `0x34` | dword | dword read at `0x0071B208` |
| `0x38` | dword | dword read at `0x0071B20C` |

Other intervening field stores do not admit those source globals or the
stored targets. The initializer encodes the candidate pointer into a local
query and calls `0x00602550`, again testing only AX. A nonzero low word
is masked and passed to `0x00600530`. The returned pointer is compared
with the candidate in ESI. Equality proceeds to publication. A different
pointer, or zero AX from the preceding call, takes the local candidate-
abandonment path: it calls `0x00601D00` with ESI, queries `0x00602540`
again, masks the low word, calls `0x00600530` and adopts the latter return
as ESI. The imported and helper effects, preservation and pointer lifetime
are not established by these local branches.

At common publication, the initializer writes ESI to `0x0242F640`,
ESI plus four to `0x0242F630`, and ESI plus eight to `0x0242F650`, with
dword address arithmetic. It restores saved registers and returns at
`0x006007D6`, without extra argument cleanup. This publishes either a
new candidate or an adopted helper return. There is no local repeat of
the new-record field initialization on the adopted-pointer paths.

The controlled source import table identifies `0x00602540` through slot
`0x02431804` as KERNEL32.dll `FindAtomA`; `0x00602550` through
`0x024317D8` as `AddAtomA`; and `0x00601D00` through `0x024319A0`
as msvcrt.dll `free`. FND-EXE-187 identifies the allocation import.
Names establish source slot identities, not the loaded contracts or
validity of the pointer encoded or recovered by the local helper.

The source traversal reaches 579 bytes at `0x006005B0..0x006007F3`,
including its two near returns, and remains incomplete: the assumed
continuation after the failure call reaches the exclusive region end.
Every reached call has a return-continuation assumption. A CFG traversal
alone cannot establish external publication, lifetime or allocation failure.

## Interpretation

This supplies a concrete producer of FND-EXE-189's shared pointer and
separates the new path's field writes from adopted-record admission.
Q-EXE-001 and Q-EXE-010 retain query producers and extents, the low-word
helper's pointer decoding, all source-global writers, imported argument
and preservation contracts, segment domains, abandoned allocations and
failure/lifetime effects. The adopted pointer must not inherit the new
record's field claims without evidence. No formal complete reading follows.

## Alternatives

Assuming every published record is newly allocated is contradicted by the
direct and fallback adoption paths. Assuming all published selectors start
negative confuses the new-path explicit store with adopted storage. The
allocation request, repeated-store width and publication offsets are distinct
quantities; none alone proves the initialized extent of an adopted pointer.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
Run the committed wrapper's `x86-bounds` with sourceKind `pe32`, entry
2095536, one named region `[2095536,2096115)` entries `[2095536]`,
and evidence naming the bounded producer with adopted-record, external and
failure contracts unresolved. Omit segment/ip, seeds and callee summaries.
Preserve its two returns, call-continuation assumptions and incomplete
exclusive-end edge.

In the saved shipped-PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x006005B0`, count 18; `0x006005D6`, count
55; `0x006006C6`, count 60; and `0x006007AB`, count 28. Restrict the
reading to the declared location. Query the three import trampolines above
with count one. Run `x86-imports` with FND-EXE-187's independent positive
controls, slot `0x024319D0`/MSVCRT.DLL/`malloc` and
`0x024319E4`/MSVCRT.DLL/`memset`; select the named slots by address.
The low-word helper and query bytes remain separate reading obligations.
Rich reports stay in the local licensed-source store, outside Git;
no original-game execution is involved.
