# Manifest-bounded native probe drive

Tooling batch supporting Q-PARTY-081. Outcome: a fresh native probe starts
with a private C: containing only fingerprint-verified installed-build manifest
files, without recursively copying captures, analysis or user saves. This
enables reproducible research; it introduces no player behavior or new claim.

Evidence: BLD-GOG-EN-1.1 and the configured extractor fingerprint manifest;
the existing rust-dos launcher and FND-PARTY-219 supply the runtime context.
The CD image remains separately mounted as D:. Media files and storefront
music are excluded from C:, not altered. No arbitrary executable is launched.

Acceptance: validate canonical relative paths, unique case-insensitive names,
sizes and XXH3 hashes; reject symlinks and changed sources; bound source reads;
verify all selected files before publishing them into an empty destination.
Copy only selected files, including unchanged SOUND.CFG. Reject a nonempty
destination and leave no copied files when source validation fails.

Automated checks use synthetic manifests to cover successful copying, ignored
unlisted captures, corrupt/missing input, traversal, duplicate names and links.
An installed-source check prepares a fresh local drive without starting the
game. Outputs remain in GAME_DIR/captures/. Exit: controls and Test.ps1 pass,
tooling committed, separate goal handover updated. State-driven navigation and
RNG interpretation remain subsequent work; no queue item closes here.

## State-driven start-window handoff

Research probe supporting Q-PARTY-081. Outcome: determine whether an unchanged
fresh licensed drive reaches main's call to the start-window opener without
input, and read its actual argument and DS/SS. Static prerequisites are
FND-UI-035's opener and the bounded main transfer at 277B:02E2..277B:02E9.
Stop before the call at 277B:02E4; do not equate this with a drawn window.
Acceptance: dynamically relocate the stop from program entry, preserve pinned
session/audio/lock contracts, retain the RNG-entry guard and refuse any draw,
read only registers, seed and the argument at SS:SP, and record observations
under GAME_DIR/captures/. No inputs or memory writes. Wait for the breakpoint,
with bounded wall-clock observation windows and a diagnostic pause on exhaustion;
elapsed time never establishes a game state. Exit: actual attempt and bounded
finding, with explicit remaining gameplay and draw gaps, then canonical gate.

## Startup seed observation and reusable checkpoint

Research extension for Q-RNG-003 and Q-PARTY-081. Outcome: identify the actual
startup seed caller, argument and resulting state on the unchanged private
drive; save a checked local checkpoint at the established opener call site.
Evidence: FND-RNG-009's startup caller at overlay 180 +065E and return +0663,
seed setter 1000:0811, FMT-EXE-004's loaded trampoline, and FND-UI-039's
opener stop. This adds no game behavior or complete clock-conversion claim.

Acceptance: before continuing the seed setter, validate its six-byte far-return
and argument frame, the loaded startup trampoline's target +0141/segment,
and the preceding far-call instruction's target against the relocated setter.
Reject another caller, short frame, wrapping stack read or changed trampoline.
Read DS/SS, argument and seed before/after this declared call; require the
result to equal the argument with its high word zero. Retain guards on both
the known generator and any further seed call; no draw is admitted.
Save state only after the opener stop passes and seed remains unchanged.
Reports and checkpoint files stay under GAME_DIR/captures/ and out of Git.
Synthetic caller checks exercise wrong segment, offset, opcode and frame;
native run verifies the observed path and checked checkpoint creation.
Exit: bounded finding, queue obligations updated, full gate, separate handover.

## Native opener-checkpoint restore and return

Research probe for Q-PARTY-081's state route. Outcome: actually restore the
checked opener checkpoint and run its declared window-opening call to its
return, avoiding a repeated intro. Evidence: FND-RNG-010's checkpoint,
FND-UI-039's call site, FND-UI-035's opener and the bounded static return
at 277B:02E9. It does not establish pixels, input readiness or gameplay.
Acceptance: checkpoint source/run names are plain names under the fixed
licensed captures root; package verifies all hashes, hardware/media and saved
stop. Require the checkpoint's copied DSUN.EXE to match the configured build
fingerprint, then the restored load stop and declared far-call instruction before
continuing. Reinstall generator and seed guards after the strict load resets
the debugger. No input or writes; stop by address, require unchanged RNG state,
save a checked returned-opener checkpoint only after arrival. Retain reports
locally. Synthetic controls reject changed call targets or stop offsets;
native attempt verifies load and return separately; synthetic executable controls
reject changed or missing copied sources. Exit: bounded finding,
runtime capability record, full gate and separate handover; no queue closure
or presentation/parity promotion based on a call returning.

## Startup clock-buffer identity probe

Research probe for Q-RNG-003. Outcome: measure the admitted startup clock
helper's near-buffer DS/SS identity and the result passed to the seed setter.
FND-RNG-009 identifies the zero optional-output argument, source caller at
overlay 180 +0657/return +065C, local buffers BP-4/BP-8, service return
sites 1000:0976/0980 and converter return 1000:098E. FND-RNG-010 identifies
the subsequent seed call. No complete calendar or reseeding claim follows.
Acceptance: validate the loaded startup trampoline and source/seed callers;
read only those named frames, buffer offsets and four-byte CX/DX output
records. Require nonwrapping offsets and matching DS/SS before treating the
near pointers as the stack locals; verify pushed pointers and their unchanged
contents through conversion. Stop at the seed entry, compare its argument
with the converter's low return word and retain the known RNG guard.
No inputs, writes, patched saves or emulated-time waits. Reports stay under
GAME_DIR/captures/rust-dos-clock-source/. Synthetic controls reject wrong
caller and segment identity or impossible frame bounds. Exit: actual bounded
observation, remaining Q-RNG-003 obligations updated and full gate.

## Startup environment and daylight observation

Research extension for Q-RNG-004. Outcome: identify the actual initializer
branch and daylight adjustment of the unchanged startup clock case.
FND-RNG-016 supplies the converter globals and daylight return; the bounded
initializer reading supplies lookup return 1000:3B61 and initializer return
1000:387D. The shipped lookup name is the three-byte TZ terminator record
at DS:3980. Acceptance: retain all existing caller, buffer, RNG and session
guards; require DS to match the relocated declared data segment before
reading the lookup pointer/name or six-byte offset/enable record at DS:397A.
Record lookup AX and admit only the zero-result default path. Read the
offset/enable words after initializer return, then AX at daylight return
1000:3933. No input, writes, environment mutation or arbitrary reads.
Synthetic controls reject a wrong data segment or incomplete global record;
an actual native run identifies the measured path, with outputs local only.
Exit: bounded finding and capability record, remaining conversion obligations
updated, full repository gate; no complete reading or rule-status promotion.

## Environment constructor and standalone startup initializer

Research confirmation of FND-RNG-019 for Q-RNG-005. Outcome: observe the
record-backed constructor's actual caller, requested sizes, returned near
offsets and terminated pointer-array metadata, then the distinct startup
timezone call before the existing clock-source observation. No environment
string contents are collected. Evidence supplies allocator return stops
1000:29B7/29DA, constructor return 1000:2A06, startup records DS:39B2/39B8,
size/source words DS:008A..0090 and root/mirror words DS:3920/0088.
Acceptance: require the declared relocated data/stack segments before reads;
validate near return 0260 and saved ES against the selected constructor record.
Check nonzero returned offsets and nonwrapping, independent requested spans
before reading only pointer words, stopping at their first zero within the
requested table and at most 1024 words. Reject out-of-payload, nonincreasing
or unterminated members. Validate the restored dispatcher bounds and root
mirror at constructor return. The standalone timezone call must return through
the declared far frame at 0259 and match its relocated initialization record;
observe its zero lookup result and offset/enable record before continuing to
the existing source/caller/buffer/seed stops. Keep all RNG/session/audio guards,
no inputs, writes or environment mutation. Synthetic controls reject wrong
frames, segments, widths, wrapping/overlapping spans and invalid array members
before unsupported reads. Exit: actual bounded finding, capability record,
remaining admission/writer gaps, full gate and separate handover.
