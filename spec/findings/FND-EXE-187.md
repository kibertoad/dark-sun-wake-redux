---
id: FND-EXE-187
title: Provider backing-base producer stores an allocation return before admission and reaches controlled CRT imports
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F7C80..0x004F7CBF
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F9940..0x005F99A1
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F7E10..0x005F7EAA
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601CF0..0x00601CF6
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

A positive backing-base writer lead at `0x004F7C80` loads a dword from
its current `ESP + 0x40`, shifts it left twenty at dword width, saves the
result at `ESP + 0x2C`, and writes it as an outgoing argument to
`0x005F9940` at `0x004F7C97`. For incoming value N, the request arithmetic
is `(N * 2^20) mod 2^32`. Its producer, range and units are not yet read.
The caller stores returned EAX at `0x01D4A380` before testing EAX for zero.
That global is FND-EXE-185's backing-memory base. Zero branches to the
unread failure path at `0x004F7FF1`; nonzero is passed with zero and the
saved request to `0x00601CE0` at `0x004F7CBA`.

The normal path of `0x005F9940` establishes EBP after saving it, saves
EDI, ESI and EBX, reserves sixty stack bytes, and constructs a local
record. It stores code target `0x005F99A1` into that record before a call
at `0x005F996E` to `0x006008F0`. The record's other target words, consumer
and nonlocal continuation contract remain unread. It then loads the dword
at current `EBP + 8` and passes it to `0x005F7E10` at `0x005F997F`.
After that call it saves EAX at `EBP - 0x44`, calls `0x00600990` with the
local record address, then reloads the saved slot into EAX. This is the
local store/reload sequence, not proof the intervening callee preserves
EBP or that slot. Cleanup resets ESP to `EBP - 0x0C`, restores EBX, ESI,
EDI and EBP, and returns near at `0x005F99A0` without extra argument cleanup.

The inner wrapper at `0x005F7E10` also constructs a local record and calls
`0x006008F0` before reading its dword argument at `EBP + 8`. A zero value
is replaced in that stack slot by one. It passes the current slot to
`0x00601CF0` at `0x005F7E62` and saves the result at `EBP - 0x44`.
For a zero result, it reads the dword at `0x0242BE90`. Nonzero calls that
stored target at `0x005F7E81`, then reloads the current request slot and
retries the allocation call. The target may change state or the request;
there is no local fixed retry bound. A zero target branches to the unread
path at `0x005F7EE0`. For a nonzero allocation result, the wrapper calls
`0x00600990`, reloads its saved result slot, restores its saved registers
through an EBP-relative stack reset, and returns at `0x005F7EA9`.
All intervening callee and local-record effects remain obligations.

The lower-level target at `0x00601CF0` jumps through slot `0x024319D0`.
The fingerprinted PE import lookup table identifies that slot as
`msvcrt.dll` import `malloc`, and slot `0x024319E4` as import `memset`.
Independent saved-project symbol references identify the same two slots;
both controlled import-report checks pass. Its positive memset trampoline
lead is `0x00601CE0`. These identities come from the source's slot mapping,
not neighboring functions or listing order. They do not establish which
CRT is loaded or its allocation, failure, initialization or lifetime effects.

The original-source normal-wrapper traversal covers 97 bytes at
`0x005F9940..0x005F99A1`, lists its three direct calls and near return,
and has no local decoding gap. Each call has an explicit return-continuation
assumption. The stored continuation target is not traversed by that query;
its complete flag is not a complete reading.

## Interpretation

This resolves a concrete producer of FND-EXE-185's backing base and
separates the caller's requested width, global-store order, initialization
arguments, wrapper retry behavior and imported targets. Initialized extent
cannot be equated with the saved request until argument preservation,
CRT effects, input range and failure continuations are established.
Q-EXE-001 and Q-EXE-010 retain those contracts, every base writer, lifetime,
output aliases and destination admission. No complete reading follows.

## Alternatives

A claim that the backing-base global is updated only after the result is
admitted is contradicted by its store-before-test order. A zero request is
not necessarily passed unchanged to the lower-level allocator: the inner
wrapper replaces it with one. A name-only allocator inference is insufficient;
the shipped import slots establish names while their loaded contracts stay open.
A saved return slot cannot be assumed preserved across an unread cleanup call.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved shipped-PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x004F7C80`, count 24; at `0x005F9940`, count
58; at `0x005F7E10`, count 26; at `0x005F7E67`, count 28; and at
`0x00601CF0`, count ten. Restrict each reading to the declared locations;
exclude neighboring routines. Run ReportInstructionContext at `0x004F7C9C`
and ReportSymbolReferences with fragments `malloc` and `memset`. Treat
symbol references as positive controls only, not exhaustive writer searches.

Run the committed wrapper's `x86-imports` command with sourceKind `pe32`
and two controls: slot `0x024319D0`, dll `MSVCRT.DLL`, name `malloc`;
slot `0x024319E4`, dll `MSVCRT.DLL`, name `memset`. Retain the source import
lookup and address-table correspondence and both matched controls.

Run `x86-bounds` with sourceKind `pe32`, entry file offset 2067776
(`0x001F8D40`), one named region start 2067776, exclusive end 2067873
(`0x001F8DA1`), entries `[2067776]`, and evidence naming the normal wrapper
path with stored continuation and callee effects excluded. Omit segment/ip,
seeds and callee summaries. Preserve its call-continuation assumptions and
exclude the stored-target route. Rich reports stay in the local licensed-source
store, outside Git; no original-game execution is involved.
