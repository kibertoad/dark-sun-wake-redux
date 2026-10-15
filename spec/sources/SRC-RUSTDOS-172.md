---
id: SRC-RUSTDOS-172
title: rust-dos v1.7.2 clock service source snapshot
superseded_by: []
author: dividebysandwich and rust-dos contributors
date: unknown
location: https://github.com/dividebysandwich/rust-dos/tree/746cc86dcf24157c48dc05c289f6bc0bb350d469
xxh3: null
licence: The repository LICENSE contains GNU GPL version 2; individual dependency licences are outside this reading.
---

## Use

Read on 2026-10-15 for Q-RNG-006. The GitHub tree API resolved tag v1.7.2
to commit 746cc86dcf24157c48dc05c289f6bc0bb350d469. This is the tag pinned
by the project's runtime installer. The reading covers public emulator
source, not original game source and not a verification that the installed
release executable was built from exactly this tree.

The bounded relevant source locations at that commit are:

| File | Lines | Reading |
|---|---|---|
| src/interrupts/int21.rs | 18..23, 941..957 | Time request and its clock stall. |
| src/interrupts/int21.rs | 1882..1888 | Date request's local result assignments. |
| src/interrupts/int21.rs | 1890..1911 | Separate date/time setters. |
| src/cmos.rs | 110..119 | Clock read and offset setter. |
| src/hosttime.rs | entire file | Host local-time and thread-local fixed-time branches. |
| src/bios.rs | 9..17, 143..170, 215..221, 272..282, 382..384 | Default vector table and installed service trap. |
| src/instructions/control.rs | 209..231 | Software interrupt entry. |
| src/cpu/fault.rs | 204..241 | Real-mode vector lookup and interrupt frame. |
| src/exec.rs | 1176..1187, 1288..1377 | Service recognition and common dispatch/return selection. |
| src/interrupts/mod.rs | 26..116, 243..264 | Service flags, real-mode return and DOS dispatch. |
| src/interrupts/int21.rs | 25..43, 293..325 | DOS common entry, caller save and exit. |
| src/dos_data.rs | 390..456 | DOS state entry, exit and paging-dependent reload. |
| src/timer.rs | 141..171, 218..229 | Clock construction, emulated time and stall charge. |
| src/deterministic.rs | 43..45, 100..114, 131..161 | Fixed start, boundary update and restored-state resynchronization. |
| src/cmos.rs | 41..58, 186..199 | Zero initial clock offset and persisted offset reload. |

The time handler selects AH 2C, requests a clock stall of 10000 nanoseconds,
then reads the CMOS clock once. It assigns hour to CH, minute to CL, second
to DH and nanoseconds divided by 10000000 to DL, narrowing each to a byte.
The date handler selects AH 2A and separately reads the CMOS clock once.
It assigns the year narrowed to a word to CX, month/day to DH/DL and
Sunday-based weekday to AL. Neither local handler body reads the other
incoming service registers or assigns SI, DS or SS. Common dispatch and
return paths can have effects beyond those local assignments.

The CMOS read obtains the host-time helper's local date/time plus a stored
offset. Its setter stores the difference between a requested local date/time
and a fresh host-time reading. The host-time helper normally calls the host's
local clock. Its separate thread-local fixed-time branch resolves the supplied
local time to the earliest matching instant, tries one hour later when
necessary, and falls back to the host clock if neither resolves. This is a
source branch, not admission of fixed-time controls in the session package.

The two service bodies do not share a cached date/time sample or compare
their readings. The date getter also has no local 1980..2099 clamp: that
filter occurs in the separate date setter. Inferring the getter's full
admitted year domain from the setter would therefore require additional
evidence. No generic DOS date-domain guarantee is imported from this source.

For FND-RNG-023's game wrapper, these bodies are a concrete external-source
lead for sequential independent readings and for the stored time fields.
Their output mapping agrees with the converter byte roles in FND-RNG-015.
The complete service path, clock-stall effects, installed interrupt routing,
guest hooks, source/binary correspondence and admitted clock construction
still need Q-RNG-006. No original-game rule status changes on this source.

### Connected real-mode service path

A further bounded reading on 2026-10-15 follows the default source route
through its common entry and return. The installed default vector for
INT 21 points to F000:1030, the thirteenth four-byte service slot. A
software interrupt follows the current vector table, rather than bypassing
it by dispatching on the INT instruction alone. The real-mode entry reads
the target, pushes flags, CS and the following IP, clears IF, TF and AC,
and loads that target. A null vector is skipped before this entry.
Guest replacement or chaining of the vector therefore remains relevant.

Recognition of the emulator service at the target reaches the common
service entry, DOS dispatcher and common return. For a nonpaged real-mode
call to the reviewed date/time branches, with no pending retry, disk wait,
stack fault or intervening guest hook, the connected source path has these
effects beyond the local handler assignments:

- Entry takes CF and ZF from the stacked flags. The DOS common path records
  incoming AX, BX and DS in its own data area. With a nonzero current PSP,
  it writes nine words, AX/BX/CX/DX/SI/DI/BP/DS/ES, below the interrupt
  frame and stores that span's SS:SP in the PSP. The save does not move
  the live SP. Its offsets wrap as words; safe separation from the caller's
  locals and other memory remains an admission condition.
- Date/time requests do not select the console-readiness branch or the
  file-error recording range. Without guest paging, the common DOS-state
  reload is a no-op. Exit writes the DOS process/DTA/error/drive state and
  clears its InDOS byte. The service therefore has memory effects outside
  the game's four-byte output buffer.
- The normal real-mode return pops IP, CS and flags from the interrupt
  frame, then clears DF and retains the service's CF/ZF. These date/time
  branches do not change CF/ZF after entry. Neither their common path nor
  their local register assignments changes DS, SS, SI or BP. This is a
  conditional source preservation result, not proof that the installed
  game actually used this unhooked route.

The normal return is distinct from the source's retry, disk-delay, paging
and virtual-8086 continuations. Those continuations are not substituted
for the ordinary startup contract. The source's default route does not
prove that the game's current vector table still contains that default.

### Deterministic clock update and the time-service stall

The time request's 10000-nanosecond stall increments instruction-clock
and stalled counters by floor(cycles_per_ms * 10000 / 1000000). At the
probe's configured 15000 cycles per millisecond, that is 150 charged
counts. The stall does not itself refresh the thread-local fixed date/time
or execute guest instructions before the following CMOS read.

Deterministic construction initializes the thread-local clock to its
start at emulated millisecond zero. Its default start is April 11, 1995,
12:34:56. A deterministic step runs toward the next whole millisecond;
only after its execution batch returns at or beyond that boundary does
it refresh the fixed date/time to start plus elapsed whole milliseconds.
Consequently a service stall that crosses a boundary can still read the
previous fixed clock value within that handler; the next boundary update
is not an instruction inside the handler. Save-state resynchronization
also refreshes the fixed clock from the restored emulated time.

CMOS construction initializes its independent offset to zero. A saved
CMOS offset is persisted in milliseconds and restored separately. Thus
the deterministic fixed-clock algorithm alone does not prove a zero
offset for an arbitrary restored state. Complete setter/caller coverage,
active frontend configuration and binary execution correspondence remain
separate from this reading of the algorithms.

These sections settle the connected ordinary source route's local
register, flag and common-memory effects, and its deterministic update/
stall algorithm. They remove those source-body obligations from Q-RNG-006.
Actual route, active clock state, memory separation and source-to-release
correspondence remain there. FND-RNG-014's observed buffers and DS/SS
identity corroborate one startup outcome without identifying its vector
target, hidden segment state or every intervening memory writer.

## Known errors

No error in the bounded source reading is known. The source-to-release-binary
correspondence has not been independently verified. Chrono's complete date
domain, host clock changes, all CMOS offset writers, active guest interrupt
route and exceptional return behavior are not covered. No original runtime was launched and no
midnight mismatch was observed. These contracts apply only conditionally to
this source revision, not every DOS environment or a future emulator release.
