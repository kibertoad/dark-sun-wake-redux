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
