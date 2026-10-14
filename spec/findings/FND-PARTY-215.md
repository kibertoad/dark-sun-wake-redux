---
id: FND-PARTY-215
title: Startup replaces the MZ stack segment from a saved data-segment value before dispatching initialization records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
tool: Python 3.12 with Capstone 5.0.9, tools/research/exec-census/resident_listing.py and startup_layout.py
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0163
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:01B0..1000:01F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0220..1000:0264
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0000000E..0x00000012
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000507C2..0x000507C4
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000507EC..0x000507EE
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005084E..0x00050850
    kind: file-data
---

## Observation

The resident listing uses a relocated MZ load segment of 1000. The shipped
MZ initial SS is 522F relative to the image, hence 622F in that mapping;
initial SP is 0080. The header occupies 5200 bytes. The words loaded at
57E0:384E, 37EC and 37C2 are respectively 4096, zero and 20.

FND-PARTY-038 already describes entry's data-segment assignment and the call
of game main. The intervening stack setup is read here.

1. The near helper 1000:01B0 saves DS, makes four INT 21 requests with AX
   3500, 3504, 3505 and 3506, and saves returned BX/ES pairs in words
   DS:0074..0082. It then sets DS from CS, makes an AX 2500 request with
   DX 01A7, restores the saved DS, and returns. It writes no SS itself.
2. At 0059 startup explicitly copies current DS into DX. It subtracts that
   value from BP (the earlier word read at offset 2), and takes DI from
   DS:384E, raising it to at least 512 and writing that minimum back.
3. It adds A4EC and DS:37EC to DI, rejecting either 16-bit carry through
   009C to 02AD. It converts the sum to paragraphs by shifting right four
   and adding one, and rejects an unsigned available BP below that count.
   If DS:384E or DS:37EC is zero it substitutes the lesser of BP and 4096;
   otherwise it retains the computed count. These are reads at the time
   of setup; the shipped word values do not alone establish their runtime
   values after the earlier helper.
4. It stores DS plus the selected paragraph count in DS:00A0 and DS:00A4,
   computes a resize count relative to the segment in DS:0090, sets ES
   from that segment, and requests INT 21 with AH 4A at 00B5. DI is saved
   across this request. No carry test follows the request in this path.
5. At 00B8 it shifts restored DI left by CL, disables maskable interrupts,
   assigns SS from DX at 00BB and SP from DI at 00BD, then reenables
   interrupts. The last own instruction assigning DX before that service
   was the copy from DS at 0059. There is no own DS assignment between
   0059 and 00BB, but the intervening DOS service is not read here.
6. It clears bytes 39C4..A4EC through ES loaded from CS:02C4. The own
   instructions through main's call assign ES but make no further SS
   assignment. Additional INT 21 requests are conditional on the word
   DS:37C2 and version bytes; INT 1A occurs at 0122 before dispatch.
7. At 0149 it calls 0220 with the initialization-record span
   3994..39BE. This six-byte-record dispatcher marks a chosen record FF,
   calls its target as near when the record's first byte was zero and far
   otherwise, saves/restores ES around the call and repeats. It does not
   save/restore DS or SS around a target. Only after the dispatcher
   returns does 0158 call game main 277B:0024 (FND-PARTY-038).

The dispatch scan selects the smallest unsigned priority byte among records
not marked FF, using the first record for equal priorities. The near path
uses the word target at record +2; the far path uses both target words.
This describes that local dispatch, not the targets' effects or termination.

## Interpretation

The MZ header's initial SS cannot be carried forward as the cleanup call's
stack segment: startup contains an explicit replacement. The copy at 0059
and assignment at 00BB supply a concrete producer route for the stack/data
relationship needed by FND-PARTY-214's near map pointer.

This route is conditional on service effects and subsequent segment
preservation. The dispatcher restores ES only, so its own wrapper cannot
prove DS/SS identity at main, still less at the later deletion call.

## Alternatives

- DOS-service changes to DX, CL, DS or SS, initial segment values and the
  initialized memory remain external dependencies. No native outcome is
  inferred from these instructions alone.
- Shipped values 4096, zero and 20 are not proof those words remain unchanged
  after the preceding interrupts/helper. They are fixed source observations.
- The startup-record targets or later game calls may alter DS/SS; complete
  target and segment-writer coverage and the native service comparison are
  separated into Q-PARTY-081, a dependency of Q-PARTY-078.
- The state-stop startup probe is unverified: the 2026-10-15 attempt was
  refused by the machine run lock before emulator launch (docs/RUNTIME.md).
  No dynamic comparison or complete-reading declaration is claimed here.

## How to reproduce

Use the hash-locked evidence Python and installed DSUN.EXE. Run
startup_layout.py with that file to read only the initial SS/SP and the
three named startup words at their shipped offsets. Run resident_listing.py
with ranges 1000:0000..1000:0163, 1000:01B0..1000:01F3 and
1000:0220..1000:0264. Inspect the assignments, intervening services and
initialization dispatch in execution order; do not substitute the header's
SS for the value startup explicitly assigns later.
