# Nested script instruction census

Research tooling for Q-ITEM-006 and the nested-call limits in FND-ITEM-045.
Read the retained indexed script listing, never execute a script or decode a
gap speculatively. Reuse script_selector_census.py for source hash, top-level
instruction ownership and independent selector/error controls. Parse balanced
nested instruction braces separately, retaining parent, offset, opcode and
depth. Do not conflate nested nodes with top-level starts.

Require explicit total-node and selected-opcode counts plus one or more
positive resource/offset/opcode identities from an independent marker query.
The current source controls are 3643 markers, including 374 opcode-22 markers;
positive controls include GPL/1 offset 0071 and GPL/2 offsets 010B and 020B.
These values are controls, not yet an accepted research result.

Bound source to 32 MiB, config to 64 KiB, nesting to 32 and nodes to 262144;
bound output to 16 MiB. Reject unbalanced/malformed braces, invalid opcodes,
duplicate or non-forward child identities, nodes crossing the next top-level
start, wrong selected arity, missed counts or positive identities. Classify
each selected operand as full signed-32-bit literal or explicit nonliteral,
retaining low words/bytes only for literals. Reports stay in GAME_DIR.

Synthetic acceptance covers recursive nesting, parent/depth identity, signed
narrowing, unknown operands, a selected opcode absent with an unrelated
positive control, malformed ownership/arity/braces, count/hash/control misses
and depth limits. Use the existing script-census synthetic test domain and
run its selected gate. Exit: committed bounded reporter and passing controls,
then a separate source-domain finding. Native entry, mutations, aliases and
undecoded ranges remain outside any source absence result.
