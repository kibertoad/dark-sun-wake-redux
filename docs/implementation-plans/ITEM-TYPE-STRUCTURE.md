# Shared item-type structural specification

Outcome: preservation tooling and reviewers have a bounded fifteen-byte
record definition for installed ALL/4. This is a specification artifact;
it introduces no player-visible behavior, importer or asset-pack contract.

Evidence: FND-ITEM-011's complete native descriptor/metadata partition and
102-row source check; FND-ACTOR-017's native base/stride/resource binding;
FND-ACTOR-018's inline/shared relationship; FND-ITEM-012's directly resolved
material labels and guarded presentation bindings; FND-PARTY-248/252
for the separately guarded completion, classification and cache branches. The pinned standard's
Formats section permits supported structural claims while field purposes
remain open. It does not require inventing meanings for raw fields.

Acceptance:

- The layout gives fourteen fields covering exactly fifteen bytes, one row
  per native descriptor interval. Bytes retain neutral offset-based names
  and raw unsigned storage; the word at twelve is little-endian. Signed
  uses in consumers are explicit transformations, not guessed universal types.
- A matching Kaitai definition describes only that partition. Material names
  are a derived low-nibble enum with the six directly evidenced values;
  no validity constraint rejects the remaining native nibble states.
- Reconcile the existing completion/classification/cache flag uses without
  assigning universal item labels or admitting unsupported input indices.
  Preserve the classifier's replacement operations and sentinel checks.
- Every supported table claim cites direct findings. Unknown purposes,
  other flag meanings and all-reader/writer completeness remain Q-ITEM-005;
  admission/lifetime stay with their existing questions. No complete-reading
  declaration follows from coverage or schema creation.
- The schema's widths and order agree with the independent descriptor and
  complete source-row reports. The existing documentation checker validates
  the linked definition/table contract, queue references and citations; the
  required full Test.ps1 gate passes. Report a missing Kaitai compiler as a
  compilation gap, never as a successful compile or executed parser test.

Exit: linked structural schema and supported format/parity row, all remaining
questions retained, no licensed values or gameplay code in Git, documentation
and full batch gate passing, compilation limitations explicit in the handover.
