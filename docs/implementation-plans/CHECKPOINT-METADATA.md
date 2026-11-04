# Paused checkpoint metadata observation

Research tooling for Q-CONFIG-008 and Q-COMBAT-067. Resolve their remaining
native getter-input question at the already verified saved-map boundary;
this does not admit later cleanup or replace its storage/lifetime obligations.

Evidence: FND-CONFIG-140/145/150/153/154 name the getter tables, scratch
index and canonical FNFO spans; FND-PARTY-119 and FND-ITEM-015 name the
object table, free-list heads and counter fields. FND-COMBAT-140 and
docs/RUNTIME.md admit strict current-pin saved-map restoration.

Extend native_save.py's existing `verify` branch only, after its source,
instruction/time, map, native-save and frame checks pass. Capture fixed
resident scratch/selected words, heads/counters, FNFO buffers, width bytes,
selector positions, record bases/strides and the 524 three-byte object rows.
Reject segment/offset/physical wrapping and short reads; chunk reads through
the existing checked reader. Retain original-derived bytes only in the
owned run's native-save.json. Summarize each pool with bounded visited-index
tracking, preserving cycle, invalid-link and overlapping-pool diagnostics.
Those diagnostics describe the snapshot and do not execute a game function.

No gameplay input, continuation, guest memory write or new checkpoint is
required for this observation. Keep the existing strict roundtrip check,
muted pinned owned-session settings, private drive and run lock. The normal
native-save path is unchanged. A new run name preserves earlier evidence.

Synthetic acceptance covers empty/valid chains, cycles, invalid links,
nonzero free kinds, cross-pool sharing, short metadata reads and wrapping
addresses. Test.ps1 selects the affected evidence controls. Exit: committed
bounded capture and passing controls, followed by a separate research batch
recording actual values and canonical-source comparisons. A snapshot alone
establishes neither initial-resource provider identity nor continuous
preservation, a populated temporary save, or cleanup correctness.

## Record allocation headers

Extend the same paused verification with the five captured kind-indexed
record bases. FND-CONFIG-209/210 and FND-COMBAT-166 identify P:0004's
header paragraph count at P:0000, predecessor word at P:0002 and marker
at P:(16*count-1). Admit only nonnull P:0004 pointers, counts 1..4095,
nonwrapping physical extents and marker 77 hex. Read four header bytes
and one marker byte per base; never traverse heap links or dump a block.
Record described payload extents and pairwise overlap diagnostics. Repeat
the comparison after the existing roundtrip. Actual allocation history,
later lifetime and unknown kind-four resource length remain open.
Synthetic controls cover bad pointer offsets, null bases, zero/oversized
counts, physical wrap, bad markers, short reads and overlapping blocks.
Exit: passing selected tooling gate, then a separate native finding.

## Shared item-type payload

FND-ACTOR-017/018 and FMT-ITEM-002 already establish canonical ALL/4's
1530-byte, 102-row span at OBJEX.GFF 1F291..1F88B. After the five named
header checks, read exactly those 1530 bytes from the captured kind-four
base only when its described payload admits the span. Retain address,
size, digest and bytes locally, and compare after the existing roundtrip.
A separate bounded comparator verifies the canonical archive's full
fingerprint before exact equality. Synthetic controls cover extent refusal,
base mismatch, short reads and one-byte payload mismatch. This admits a
source-equal endpoint, not provider identity, later lifetime or all consumers.
