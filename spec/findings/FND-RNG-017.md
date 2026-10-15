---
id: FND-RNG-017
title: The measured startup clock uses the default environment offset and a one-hour daylight adjustment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession; tools/research/rust-dos-runs/clock_source.py; bounded resident_listing.py and fingerprinted table prerequisite reads
environment: Windows; Python 3.12; pinned rust-dos v1.7.2 SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4; deterministic 15000 instructions per millisecond; Sound Blaster 16 at 220 IRQ 5 DMA 1; no Ultrasound; default muted host audio
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0811..1000:0822
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0963..1000:09B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3B56..1000:3CD7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:386D..1000:3986
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3CD7..1000:3DB6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005097A..0x00050983
    kind: file-data
---

## Observation

FND-RNG-016 reads the calendar arithmetic but leaves its measured
initializer selection conditional. The bounded prerequisite initializer
reading identifies a lookup of the near name at DS:3980. The shipped
three-byte record is TZ followed by its terminator. A zero lookup return
branches to assignments of low offset word 18000, high offset word zero
and daylight-enable word one, followed by two default-label copies.

On 2026-10-15 the extended fixed clock_source.py ran the approved
DSUN.EXE -W0 -L using the manifest-verified private C: and licensed
game.ins on D:. The pinned session package used its normal machine lock
and muted configuration. The probe admitted the same zero optional-output
startup caller as FND-RNG-014, retained its near-buffer/caller checks,
and refused a known generator call. No input, memory write, environment
mutation or fixed-time state selection was used.

| Stage | CS:IP | DS and SS | Instruction count | Emulated ns |
|---|---|---|---|---|
| Environment lookup returned | 05BA:3B61 | 4D9A | 2,883,908 | 697,519,216 |
| Initializer returned | 05BA:387D | 4D9A | 2,883,975 | 697,524,083 |
| Daylight helper returned | 05BA:3933 | 4D9A | 2,884,171 | 697,537,150 |

At the lookup return, DS matched the relocated declared data segment.
The still-pushed name pointer was 3980 and its three bytes matched the
declared name. AX was zero. At initializer return the six-byte record
through the same declared DS consisted of low offset 18000, high offset
zero and enable word one. At daylight return AX was one. These are
separate actual return stops, rather than inferred results from the
later seed word alone.

The date and time buffers again held words [1995, 1035] and [3106, 14405].
The existing probe checks verified their pushed near pointers, matching
DS/SS and unchanged contents through conversion. The converter returned
797618096 and the subsequent validated seed entry received 44976,
its low word. The run stopped before that seed setter executed. No
known generator draw was admitted. The owned session closed afterward.

The report and package journal remain under
GAME_DIR/captures/rust-dos-clock-source/environment-default-20261015/.
No private drive, memory record or journal was committed.

## Interpretation

The measured startup selects the initializer's zero-lookup default
path, offset 18000 and enabled daylight helper. Its helper result one
selects the converter's one-hour subtraction. This supplies the missing
measured inputs of FND-RNG-016's conditional arithmetic cross-check and
accounts for this startup return and seed argument.

This answers the measured default-path obligation of Q-RNG-004, not
the full environment/service contract or other reseeding routes.

## Alternatives

- Lookup result zero establishes no returned value pointer in this call;
  it is not a census of every environment-table writer or possible TZ input.
- Only the unchanged admitted startup/default case was observed. Parsed
  environment branches, other dates, transition boundaries, nonzero output
  pointers and overlay-204 reachability remain open.
- The probe reads the declared global record at return. It does not
  establish all aliases, lifetime, intervening writers or other consumers.
- No native branch coverage, helper fixture, validated rebuild replay,
  complete-reading claim or rule-status promotion follows.

## How to reproduce

Set EVIDENCE_PYTHON to the locked project interpreter and run
clock_source.py with a new plain run name; this run used
environment-default-20261015. The fixed probe validates its startup
source and seed callers, the name pointer and declared data segment,
then waits for the three listed returns before its existing conversion
and seed-input stops. It records only named clock buffers, the offset/
enable record and return values. A nonzero lookup result is recorded
and refused rather than accepting another environment path.
The final guard ordering was repeated natively under
environment-default-20261015-verified with the same observed results.

check_clock_source.py supplies synthetic wrong-segment and incomplete-
record controls beside the existing caller/buffer controls. Prerequisite
static ranges and the shipped name record are the listed locations;
read them with the fingerprinted resident/table readers. Reports remain
local and emulator times are observations, never state-selection criteria.
