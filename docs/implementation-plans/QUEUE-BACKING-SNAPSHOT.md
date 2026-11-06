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

## Saved-frame link observation

For Q-PARTY-137, extend only the admitted paused snapshot with the SS:BP
link walk documented by FND-EXE-234 and FND-PARTY-333. Start at the
reported BP; read each link word, stop on zero or one, clear the low bit
for the next offset, and observe return offset/segment only for a nonzero
even link. Follow at most 64 nodes, detect cycles, and report a specific
limit or crossing instead of guessing. No writes, guest continuation,
arbitrary starting address or frame repair. A snapshot is not admission
of the later interrupt frame or proof that saved returns are genuine.

Controls cover zero/one termination, odd links skipping comparison fields,
even links and return fields, cycles, the node cap, short reads and physical
or segment crossings. Keep outputs local; run controls and integration
before commit. Actual paused observations are a separate research batch.

## Loaded-overlay queue observation

For Q-PARTY-137, observe FND-EXE-566's loaded queue through FMT-EXE-003
header +1C. Reuse trampoline_census.layout on the fingerprint-checked
checkpoint executable to register all 49 headers and code sizes. Read only
registered headers, with cycle detection and a 49-node bound. Record load
segment, saved return offset, state/pass bytes and code size; preserve
unknown headers and size mismatches as incomplete results. Compare code
ranges and their preceding backlink words with queue and buffer bounds.
No trampolines, writes or continuation; this does not admit cache-list
chains, later graphs or native transfers. Controls cover empty/multi-node
graphs, cycles, foreign headers, size mismatch, ranges and source refusal.
Run controls and integration before commit; actual observations are separate.
