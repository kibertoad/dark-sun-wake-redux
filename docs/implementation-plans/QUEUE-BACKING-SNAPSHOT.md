# Paused queue-backing snapshot

Tooling for Q-PARTY-137 and FMT-PARTY-007, following FND-PARTY-332/333/334.
Extend the existing strict checkpoint verifier for its admitted play/play-ready
states with fixed read-only overlay fields: mode, EMS window/handle, native
move target/handle/base, hook targets and buffer bounds. Also record the
queue index/kind words and SS/BP/SP. Read only the named fields from the
fingerprint-checked, strictly restored source; expose no arbitrary target,
input, continuation or guest write. Other verifier stages remain unchanged.

Compare the snapshot across the existing same-process roundtrip. Reports
stay in GAME_DIR alongside the restore verification. Quantitative ranges
are point observations, not reflection-time invariants, valid BP chains,
successful cache operations or complete-reading declarations. Existing
hardware mismatch, instruction/time/frame/file and executable checks stay.

Synthetic controls require exact named fields and bounded reads, relocation,
wrong mode/DS rejection before reads, short-read rejection and invalid
register/mapping rejection. Run those controls and Test.ps1 -Integration
before commit. Then verify the existing play-ready checkpoint in a new owned,
muted pinned session without resuming the guest, and record the actual result
in a separate research batch; a failed capability remains a specific gap.
