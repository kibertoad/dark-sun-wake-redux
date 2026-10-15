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

## Known errors

No error in the bounded source reading is known. The source-to-release-binary
correspondence has not been independently verified. Chrono's complete date
domain, host clock changes, all CMOS offset writers, interrupt dispatch and
return behavior are not covered. No original runtime was launched and no
midnight mismatch was observed. These contracts apply only conditionally to
this source revision, not every DOS environment or a future emulator release.
