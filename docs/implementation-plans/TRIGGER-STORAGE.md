# Prepared trigger destination observation

Tooling for Q-SCRIPT-019, following FND-SCRIPT-057's static attempt.
The existing fixed `saved-script-conversion` probe observes the live
trigger pointer and its 2600-byte block before conversion, before table
release and at reader return. It does not record SAVE/38's prepared slot.

Add one four-byte read of the named SAVE/38 slot, DS:1612, to the same
snapshot boundaries, using the existing executable identity, load mapping,
DS/SS, frame, table-size and provider controls. FND-SAVE-021 binds that
slot; FND-SCRIPT-057 distinguishes it from the live pointer DS:40C0.
Record its far-pointer words, whether they match the live pointer words,
and whether the slot stays unchanged between the sampled boundaries.
Do not dereference this additional pointer or use it to select a read.

No new entry point, input, memory write, breakpoint, resource transfer,
arbitrary address or process interface is admitted. Keep the package's
run lock, pinned rust-dos identity and muted host audio. Original-derived
reports remain in GAME_DIR. A mismatch or null slot is a recorded value,
not permission to follow another address. Existing trace success fields
retain their old meaning; review the new comparisons explicitly.

Synthetic fake-client controls must cover matching words, a changed slot,
an independently mismatched slot and zero, without reading through it.
The tooling gate must pass before the changed probe supplies evidence.
Then repeat the already admitted native saved-map scenario with a fresh
output name. Its native report must retain the existing provider/frame,
conversion prediction and reader-return controls, plus the new slot
comparisons. A matching result admits only those sampled boundaries;
it does not prove continuous lifetime or every restore/failure path.

Exit: the probe reports the fixed slot at the existing boundaries,
synthetic controls pass, and any native comparison is recorded with its
actual result and limits. Native capability remains unverified until that
attempt; a tooling or process exit alone is not an evidence result.

Native attempt recorded: FND-SCRIPT-058 admits matching prepared/live
pointer words at the three existing stops in one normal route, with the
existing conversion controls passing. Earlier transfer identity, continuous
lifetime and other restore/failure paths remain Q-SCRIPT-019.
