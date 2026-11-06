---
id: FND-SOUND-052
title: Sound-state and stop wrappers test driver state two and do not verify stop completion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 49E9:00FD..49E9:018F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0C82..4842:0C94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:18B3..4842:1906
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:1045..4842:105F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0EB3..4842:0EEA
tool: resident_listing.py at f156ca09; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-007/008/011 and FND-EXE-585/589
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses and selectors below
are hex. FND-SOUND-007/008/011 identify callers that test the
low-byte result of 49E9:00FD before calling 49E9:0142.

Both no-argument far wrappers read configuration pointer DS:3411
and return AX zero when its word +8 equals 71. They also return
zero for mode word 4E71:0B51 equal to five or two. Neither locally
checks the configuration pointer for null. With those gates passed,
both request 4842:0C82 with DS:3436 and compare returned AX with two.

00FD returns AX one exactly for that comparison succeeding, zero
otherwise. 0142 returns zero when it fails; on success it reloads
DS:3436, requests 4842:0C8E, discards the argument and returns AX
one without inspecting the request result. Both restore BP and
RETF without callee argument cleanup. There is no local write to a
sound-state global. Their driver calls and configuration/mode reads
remain capable of depending on changed state and storage binding.

Thus the caller's query and the stop wrapper perform separate
queries. A successful first query does not imply a successful second
query, nor does it ensure that a reloaded DS:3436 names the same
handle. The stop wrapper's one means the request path returned;
it does not confirm device silence or a successful driver result.

The two controller stubs select AX 007C and 007E respectively,
then tail-jump to 4842:03F4. Reuse FND-EXE-585/589 for lookup and
far-return dispatch, including a missing target and the distinction
between loaded Copy-driver tables and the resident DIGPAK adapter.
A missing dispatch target can return without an actual device call;
this finding does not assume every configuration resolves a target.

For the unchanged DIGPAK table described by FND-EXE-585, selector
007C reaches 4842:18F7. That body saves DS/SI/DI, reads the word
at CS:0E5E into AX and restores those registers before returning.
It makes no interrupt request. Actual state writers and driver-table
identity are required before treating this as a hardware state test.

Selector 007E reaches 4842:18B3. It saves BP/DS/SI/DI, clears
CS:0E5E, constructs two zero word arguments on its stack and calls
4842:1045. That helper sets DS to CS, loads BX and DX from those
arguments and issues interrupt 66 with AX 068E, then restores its
saved DS/SI/DI/BP. After returning, 18B3 issues interrupt 66 with
AX 068F, writes three to CS:0E58 and CS:0E5A, and calls 0EB3.
The local ordering clears software state before the interrupt requests;
none of those requests is followed by a local success test.

0EB3, reused by that stop path, tests CS:0E2A bit three set,
CS:0002 nonzero and CS:0E7A zero. Only when all hold does it call
0A0F, pass its AX to 059C, call 0520 and publish CS:0E7A one.
Those children and the interrupt handler remain separate effects;
the helper's local guards alone do not establish their outcome.
All stated continuations require intact frames and child returns.

## Interpretation

Q-SOUND-002 can remove the unread local query/stop contracts and
replace them with explicit state/target/interrupt obligations. This
answers what the wrappers test and what their return values promise.
It does not establish driver-state word production, actual selected
driver/handle identity, effects of the timer/setup children, external
interrupt behavior or native playback/stop output. No complete reading,
queue closure, status promotion or runtime confirmation is claimed.

## Alternatives

- A nonzero query is a direct device measurement: the admitted
  DIGPAK path reads a software word without an interrupt.
- Stop returning one proves successful stop: the wrapper assigns one
  after the child returns and ignores the child's result.
- The outer query makes the stop decision stable: the stop wrapper
  repeats the gates and query and reloads the handle for its request.

## How to reproduce

Use resident_listing.py at the revision above for the five location
ranges. Retain sound-state-stop-contract.txt under
GAME_DIR/analysis/handler-census-20261106/. Reuse FND-EXE-585/589
for table binding and dispatch rather than treating a stub selector
as proof of a selected runtime target. Track the stack-built zero
arguments, ordering of software writes and interrupt requests, and
both wrappers' explicit AX assignments. No game run, emulator session
or emulated call supplied this evidence.
