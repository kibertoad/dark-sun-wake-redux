# Script selector census

Research-side tooling for Q-COMBAT-057. Classify the second operand of every
indexed top-level opcode 0x5E before spending research on whether a nonliteral
first operand can equal the sentinel. FND-COMBAT-099 supplies the full-width
sentinel and low-word selector contract; FND-COMBAT-100 supplies the retained
index and independently recorded category controls. No game code changes.

Acceptance:

- Use the existing committed listing grammar, operand splitter and bounded
  reader. Verify the explicit raw index SHA-256. Reject malformed, duplicate
  or unsorted indexed instructions, wrong operand counts, malformed error
  records, unknown config fields and oversized inputs/output.
- Group first operands as exact sentinel literal, other integer literal or
  nonliteral. Within each group retain literal selectors by zero/nonzero low
  word and nonliteral selectors. Preserve resource/offset and expression
  identity so remaining sites are finite, without treating unknown as zero.
- Require explicit positive category counts. The original controls are the
  912 total starts, 218 sentinel literals, 588 other literals and 106 nonliteral
  first operands, and the sentinel group's 207 literal and 11 nonliteral
  selectors of FND-COMBAT-100, plus its known decoder-error count. A missed
  control rejects the report.
- Synthetic controls cover nested operand commas, simple and expression
  operands, explicit zero, high-word-only values, negative word narrowing,
  variables, control mismatches, rejected spans and malformed/oversized input.
- Reports identify config, listing, tool and parser hashes, retain decoder
  error records, and state that nested instructions, runtime admission,
  parameter preservation and native effects remain outside this census.
- Keep actual configs/reports in GAME_DIR. Use the unchanged retained index;
  no rescan of licensed assets or speculative decoder recovery is needed.

Exit: synthetic controls and Test.ps1 pass, and the tooling is committed
separately from findings. Research may exclude a site's encoded zero selector
only under FND-COMBAT-099's argument-preservation conditions. It must retain
remaining first-operand and context domains, not declare native unreachability.

## Count-controlled operand queries

For Q-SCRIPT-017, reuse the same fingerprinted index parser and positive
selector/error controls to classify the fourth operand of opcodes 1B and 1C.
FND-SCRIPT-038/FND-PARTY-261 give the low-byte writer; the independently
recorded top-level counts are 347 and 35. Add optional operandQueries with
opcode, arity, zero-based operand index and positive expected count. Bound
queries to sixteen unique opcodes and arity to eight. Each matching record
must have exactly that arity. Retain literal/nonliteral distinctions, full
signed 32-bit values, low words/bytes and count of literal bytes with bit
seven set. Preserve decoder gaps and the original selector report.

Synthetic controls cover negative and high-word values, bit-seven boundaries,
nonliteral/nested operands, exact counts, malformed arity, duplicate/vacuous
queries and unsupported keys. Require the selected tooling gate before a
separate source-domain finding. This query never proves runtime admission,
operand immutability or nonliteral values, and adds no game implementation.
