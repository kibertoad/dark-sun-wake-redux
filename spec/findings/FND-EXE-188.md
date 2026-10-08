---
id: FND-EXE-188
title: Allocation record helpers select shared or imported thread-local storage and retain distinct cleanup paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006008F0..0x00600988
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600990..0x006009FD
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-187's setup helper at `0x006008F0` establishes EBP and saves
EDI, ESI and EBX, then reserves twelve stack bytes. It reads its record
pointer from `EBP + 8` and the shared-state pointer at `0x0242F640`.
A zero shared pointer calls `0x006005B0` and reloads it. A negative signed
dword at shared offset `0x30` calls `0x00600860` and reloads the shared
pointer and selector. These callees' writes and admission remain unread.

When the selected dword at offset `0x30` is zero, setup reads the dword
at shared offset `0x28`, writes it into the record's first dword, then
writes the record pointer into that shared field. It resets ESP to
`EBP - 0x0C`, restores EBX, ESI, EDI and EBP, and returns near at
`0x00600923` without extra argument cleanup.

For a nonzero selector, setup loads the dword at shared offset `0x2C`
into EBX, calls `0x00602490` and saves its EAX return in ESI. It pushes
EBX and calls `0x00602580`, saves that EAX in EBX, then pushes ESI and
calls `0x00602440`. It writes EBX into the record's first dword, writes
the record pointer at current ESP, reloads the shared pointer and its
`0x2C` field, pushes that value and calls `0x00602590`. The return is
then tested at full dword width. Nonzero takes the earlier register-restore
return; zero restores those same saved registers and tail-jumps to
`0x00602490` at `0x00600983`. This describes explicit stores and pushes;
argument grouping and stack effects across imported calls remain unproved.

The cleanup helper at `0x00600990` establishes EBP, saves EBX and ECX,
and reads the record's first dword into EBX before choosing its shared
or imported-storage path. Its shared pointer and selector gates invoke
the same two initializer callees, with reloads after each. A zero selector
writes EBX to shared offset `0x28`, restores saved EBX from `EBP - 4`,
executes LEAVE and returns near at `0x006009B8` without extra argument
cleanup. Its nonzero-selector path pushes the saved first dword and the
shared `0x2C` value before calling `0x00602590`. It tests EAX at full
dword width. Nonzero branches to the saved-EBX/LEAVE return; zero restores
EBX, executes LEAVE and tail-jumps to `0x00602490` at `0x006009F8`.

The controlled shipped-PE import report identifies the trampoline slots:

| Trampoline | Import slot | DLL | Import |
|---|---|---|---|
| `0x00602490` | `0x02431838` | KERNEL32.dll | GetLastError |
| `0x00602580` | `0x024318A0` | KERNEL32.dll | TlsGetValue |
| `0x00602440` | `0x02431888` | KERNEL32.dll | SetLastError |
| `0x00602590` | `0x024318A4` | KERNEL32.dll | TlsSetValue |

These are names from the source's import lookup table, not established
loaded-function effects. The shared-field and import paths cannot be merged
into one assumed storage identity. Callee effects, register preservation,
import argument widths/cleanup and aliases of the record remain unread.

Separate original-source traversals reach 152 setup bytes at
`0x006008F0..0x00600988` and 102 cleanup bytes at
`0x00600990..0x006009B9` and `0x006009C0..0x006009FD`. Both list their
local near return and an unresolved final jump beyond the declared region;
the saved listing identifies those jumps as the trampoline above. Both
reports remain incomplete, with return-continuation assumptions at calls.
They do not establish imported tail-call completion or nonlocal transfers.

## Interpretation

This resolves local record-link stores and cleanup ordering for FND-EXE-187's
wrapper-record dependency, including the separate imported-storage path and
its failure return route. It does not yet prove that the wrapper's saved
return slot, request or frame survives those helpers: admitted record/state
storage and every intervening callee must exclude aliases and establish
preservation. Q-EXE-001 and Q-EXE-010 retain those obligations, shared-state
and selector producers, initializer and imported contracts, nonlocal stored
targets and lifetime. No formal complete reading follows.

## Alternatives

Treating the record's first dword as unchanged across setup is contradicted
by both local paths' stores. A near return on one path does not establish
the zero-result path, which restores registers then tail-transfers. A
thread-local import name does not prove that its state is the same storage
as shared offset `0x28` or that an imported call preserves all saved locals.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
Run the committed wrapper's `x86-bounds` twice, sourceKind `pe32`: setup
entry 2096368, one named region `[2096368,2096520)` entries `[2096368]`;
cleanup entry 2096528, one named region `[2096528,2096637)` entries
`[2096528]`. Name the bounded record helper and unresolved external/shared
contracts in each region's evidence. Omit segment/ip, seeds and callee
summaries. Preserve both incomplete results, each final unresolved jump,
cleanup's seven-byte hole and every call-continuation assumption.

In the saved shipped-PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x006008F0`, count 36; `0x00600948`, count 22;
`0x0060097C`, count eight; `0x00600990`, count 24; and `0x006009D1`,
count 19. Restrict observations to the declared intervals. Query each of
the four trampolines in the table with count one, excluding its neighbors.

Run `x86-imports` with sourceKind `pe32` and FND-EXE-187's independent
positive controls: slot `0x024319D0`, dll `MSVCRT.DLL`, name `malloc`;
slot `0x024319E4`, dll `MSVCRT.DLL`, name `memset`. Select the four named
slots from the controlled source-table result by slot address, not list
order. Keep rich reports in the local licensed-source store, outside Git.
No original-game execution is involved.
