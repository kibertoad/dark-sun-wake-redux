---
id: FND-EXE-189
title: Record selector publication follows a constant-zero local stub and keeps failure and wait paths separate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600800..0x00600857
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600860..0x006008E9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00602630..0x00602637
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-188's selector initializer at `0x00600860` saves EBP, ESI and
EBX and reads the shared pointer at `0x0242F640` into EBX. It reads the
dword gate at `0x0242C910` and forms ESI as EBX plus `0x34` at dword
width. A zero gate or zero formed ESI writes dword zero at shared offset
`0x30` and returns. These checks do not admit the shared pointer's storage.

Otherwise it reads shared offset `0x34`. When already nonzero, it reads
the selector at offset `0x30`: a signed negative selector goes to the
zero-selector write, while a nonnegative selector returns without that
local write. When offset `0x34` is zero, it calls `0x00602570` at
`0x006008AC`, pushing the address of shared offset `0x38`. A zero EAX
return calls `0x00600800`, then writes dword one at shared offset `0x34`.
It reloads the shared pointer and follows the signed-selector test again.
Preservation of EBX across these callees remains an obligation.

For a nonzero return from `0x00602570`, the initializer rereads offset
`0x34`. While zero, it repeatedly calls `0x006023B0`, pushing zero, and
rereads the dword through ESI. Once nonzero, it reloads the shared pointer
and follows the selector test. No local bound on iterations or proof of
the writer that ends this wait follows from this reading. Both local
returns restore EBX, ESI and EBP through an EBP-relative stack reset and
return near at `0x0060088D` or `0x006008A4`, without extra argument cleanup.

The publisher at `0x00600800` saves EBP and EBX, reads the shared pointer
into EBX, and calls `0x00602560`. It compares EAX with dword `0xFFFFFFFF`.
For any other value it stores EAX at shared offset `0x2C`, pushes zero
and that stored value, and calls `0x00602630`. The actual local callee is
a seven-byte constant-return stub: it saves and restores EBP and returns
zero in EAX, with no extra argument cleanup and no calls or writes beyond the saved-EBP stack slot.
It does not read the outgoing arguments. The publisher releases sixteen
stack bytes, converts that zero into dword one, and stores it at shared
offset `0x30`. This resolves the callee-return dependency on this path;
shared-state admission and preservation across the preceding import remain open.

For return `0xFFFFFFFF` from `0x00602560`, the publisher instead calls
`0x00602490`, converts its returned EAX to one if zero and zero otherwise,
and stores that result at shared offset `0x30`. It performs no local
write to offset `0x2C` on this branch. Both publisher returns restore EBX
and EBP and return near at `0x00600839` or `0x00600856`, without extra
argument cleanup. A selector value alone does not identify whether allocation
succeeded; the failure branch also derives it from another returned value.

The fingerprinted, controlled PE import report identifies `0x00602560`'s
slot `0x02431898` as KERNEL32.dll import `TlsAlloc`; `0x00602570`'s slot
`0x02431858` as `InterlockedIncrement`; `0x006023B0`'s slot `0x02431894`
as `Sleep`; and FND-EXE-188's `0x00602490` as `GetLastError` through
`0x02431838`. Those imported effects are not inferred from their names.

Separate source traversals cover 81 publisher bytes, 135 initializer bytes
and seven stub bytes. They list the two publisher returns, two initializer
returns and stub return, with no decoding gaps. The publisher's and
initializer's complete flags retain their call-return continuation assumptions;
they establish neither wait termination nor external or nonlocal behavior.

## Interpretation

This closes FND-EXE-188's local selector-publication and constant-callee
return dependencies. It distinguishes initial fallback, already-published
state, waiting and imported allocation failure. Q-EXE-001 and Q-EXE-010
retain shared/gate/flag producers, initialized record extent, imported
argument and preservation contracts, storage aliases, concurrency and native
admission. No formal complete reading follows.

## Alternatives

Inferring that the post-allocation call is another import or initializes
a thread-local value is contradicted by its actual constant-return body.
Treating the selector as an unambiguous allocation-success flag is contradicted
by its separate error-return conversion. The wait's code has no local
iteration limit; no timing or writer behavior can be established from a
bounded CFG flag or import name.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
Run the committed wrapper's `x86-bounds` separately with sourceKind `pe32`:
publisher entry 2096128, region `[2096128,2096215)` entries `[2096128]`;
initializer entry 2096224, region `[2096224,2096361)` entries `[2096224]`;
stub entry 2103856, region `[2103856,2103863)` entries `[2103856]`.
Give each a unique region name and evidence identifying the bounded dependency
and unresolved external/state admission. Omit segment/ip, seeds and callee
summaries. Preserve the publisher's six-byte and initializer's two-byte holes
and all call-continuation assumptions.

In the saved shipped-PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x00600860`, count 38; `0x006008B8`, count 24;
`0x00600800`, count 25; `0x00600845`, count 12; and `0x00602630`, count
18. Restrict observations to the declared locations. Query each named import
trampoline with count one. Use `x86-imports` with FND-EXE-187's independent
controls, slot `0x024319D0`/MSVCRT.DLL/`malloc` and
`0x024319E4`/MSVCRT.DLL/`memset`, selecting the four slots by address.
Rich reports remain in the local licensed-source store, outside Git;
no original-game execution is involved.
