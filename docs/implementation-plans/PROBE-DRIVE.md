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
