---
id: FND-RNG-014
title: The admitted startup clock helper uses its SS-local date and time buffers through matching DS near pointers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
tool: dinorefurb-dosbox-session 0.10.0 RustDosSession; tools/research/rust-dos-runs/clock_source.py; bounded resident_listing.py prerequisite reads
environment: Windows; Python 3.12; pinned rust-dos v1.7.2 with executable SHA-256 310de9a912a4874cfceccb2aead71b8da39315aa059dbb5a108f86feb3f73ee4; deterministic 15000 instructions per millisecond; Sound Blaster 16 at 220, IRQ 5, DMA 1; no Ultrasound; default muted host audio
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0963..1000:09B0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:046E..1000:0494
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:386D..1000:3986
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067835..0x00067844
---

## Observation

FND-RNG-009 identifies the clock source helper's zero optional-output
argument, its two near-buffer producers and its conversion call.
The bounded prerequisite reread confirms that the date output pointer
is BP-4 and the time output pointer is BP-8. The service wrappers store
CX and DX as two words through DS, while the caller constructed those
offsets relative to its SS frame. Before converter entry the caller pushes
time then date, making date the first near-pointer argument. The callers'
pushed words remain on the stack at their respective return stops.

On 2026-10-15 clock_source.py ran fresh approved DSUN.EXE -W0 -L on
the manifest-verified private C:, with licensed game.ins on D:, the
normal machine lock and the pinned muted configuration. No input,
memory write, screenshot, saved-state patch or fixed-time advancement
was used. A known-generator guard refused a draw. The probe stopped at
the clock source, validated its overlay-180 caller against the loaded
trampoline and far-call target, then used only declared return stops.

| Stage | CS:IP | DS | SS | BP | Instruction count | Emulated ns |
|---|---|---|---|---|---|---|
| Source helper entry | 05BA:0963 | 4D9A | 4D9A | FF4C | 2,883,782 | 697,500,683 |
| Date service returned | 05BA:0976 | 4D9A | 4D9A | FE52 | 2,883,804 | 697,502,150 |
| Time service returned | 05BA:0980 | 4D9A | 4D9A | FE52 | 2,883,822 | 697,513,350 |
| Conversion returned | 05BA:098E | 4D9A | 4D9A | FE52 | 2,884,238 | 697,541,616 |
| Seed setter entry | 05BA:0811 | 4D9A | 4D9A | FF4C | 2,884,253 | 697,542,616 |

At the service and converter return stops, BP-4 was FE4E and BP-8 was
FE4A. Their pushed near pointers matched those offsets, DS equaled SS,
and neither four-byte range wrapped its offset. Therefore these writers
and the converter addressed the measured stack locals in this admitted
case, rather than a different segment's storage at the same offset.

| Buffer | First stored word (CX) | Second stored word (DX) |
|---|---|---|
| Date output | 1995 | 1035 |
| Time output | 3106 | 14405 |

The date bytes remained unchanged after the time service. Both buffers
and their pointers remained unchanged through conversion. The converter
returned DX:AX value 797618096. At the subsequent declared seed entry,
the word argument was 44976, equal to that value's low word. The probe
stopped before the seed setter executed; FND-RNG-010 separately observes
its execution and resulting state. No known generator call was admitted.

The report and package journal remain beneath
GAME_DIR/captures/rust-dos-clock-source/buffer-identity-20261015/.
The owned session closed and released the normal lock. No original-derived
private drive, memory record or journal was committed.

## Interpretation

This settles the earlier near-buffer identity for the measured startup
call in Q-RNG-003 and connects its actual converter return to the seed
argument. It supplies direct memory/argument evidence for FND-RNG-009's
source path, beyond observing only the later seed change.

## Alternatives

- Only this startup path with a zero optional-output argument and this
  environment was observed. Other admitted paths, nonzero optional outputs
  and other reseeding calls still require their own reading or evidence.
- The full clock conversion, timezone/environment initializer, service
  input domain and branch-dependent semantics remain Q-RNG-003. One numeric
  return does not define a general epoch or a complete seed formula.
- DS/SS equality at these stops does not establish every intervening state
  or the later deletion-map identity in Q-PARTY-081.
- The known-generator guard is not a complete randomness or state-writer
  census; Q-RNG-001 retains those obligations.
- No native gameplay state, emulated helper fixture or validated rebuild
  replay is established here. The reported times are observations only.

## How to reproduce

Use the locked evidence Python and run clock_source.py with a new plain
run name. The recorded name was buffer-identity-20261015. The tool verifies
the licensed source files, starts only the declared target via the pinned
muted session, validates the source caller and waits by address for each
documented return before reading the named buffers and pointers.

Its output records the measured words and converter/seed values locally.
Wrong callers, segment identity, pointer offsets, changed contents and
wrapping reads are refused. check_clock_source.py exercises bounded caller
and frame controls with synthetic segments and buffers. No raw argument
seeding or broader executable runner is introduced.
