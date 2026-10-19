# Pinned-runtime checkpoint regeneration

Regenerate the retained start, added, dropped, afterdrop, talk and play
states using rust-dos v1.7.5 through dinorefurb-dosbox-session 0.11.0.
Preserve old states; never alter recorded emulator identities to admit a
restore. The owner requested regeneration on 2026-10-19.

Evidence: docs/RUNTIME.md, FND-RNG-010, FND-UI-035, FND-UI-040,
FND-UI-045, FMT-UI-001, EXP-PARTY-001 and the retained input plans.
The installed package's RustDosSession checks a saved state's emulator
SHA-256 before starting a restore. That restriction does not by itself
establish native save-format incompatibility between releases.

Existing fixed startup_window.py and restore_opener.py provide the
state-gated opener and opened foundations. A regeneration driver may
read the supported active-window pointer and window number, record
keyboard/mouse delivery, inspect named party fields and save strict,
file-checked checkpoints. It must derive segment relocation from the
observed load state and refuse unexpected identity, input or RNG paths.
Polling may advance a bounded quantum, but readiness is a read window/
party state rather than elapsed time. No original memory writes are needed.
Original images, state files, journals and profiles remain in GAME_DIR.
The box table is the far pointer stored at DS:19C5, not inline storage;
each 66-byte record's word at +0x0E admits the occupied/empty box gate
(FND-PARTY-046). The selected-control pointer at DS:A125 is bounded to
its tag/number header (FND-CONFIG-215). Diagnostic stages may stop at
the mouse callback or the declared pointer/press-match branches of
3D72:0009; no diagnostic stop admits continued gameplay implicitly.
Split pointer movement and button operations, checking delivery separately.
The menu driver is bounded; conversation-entry discovery and Esc on the
stable main window are capability attempts, not gameplay parity evidence.
The RNG recorder admits only RULE-SOUND-003 and RULE-SCRIPT-006 callers.

Acceptance: every regenerated state records the pinned emulator and actual
sound hardware; input is delivered before saving; each state restores in a
new owned process with strict matching and file checks; state fields and
frames agree at restore. Known gameplay RNG draws retain rule, bound and
result; unknown draw sites stop the run. Window/party state predicates must
pass before input and checkpoint creation. Screenshot availability proves
frame capture only. Sound and patched native saves are separate capabilities.

Synthetic controls cover stale identity, failed state predicates, pending
operation reuse, unsupported actions and bounded pointer/state reads.
Run Test.ps1 before the tooling batch commit. Record actual capability
attempts separately in spec and docs/RUNTIME.md; retain any unavailable
state and the precise failed predicate instead of claiming all six passed.

Exit: six fresh checked states, six independent native restore checks,
current runtime capability documentation and a clean validated handover.
