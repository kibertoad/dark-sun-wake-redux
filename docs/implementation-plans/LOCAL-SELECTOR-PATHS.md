# Local selector assignment paths

Research-side tooling for Q-COMBAT-056. The preservation question is whether
the declared entry controls of FND-COMBAT-101 reach their first selector
instruction after a direct assignment to the selected local number. This
supports measured region/load research; it adds no gameplay behavior.

Inputs are the retained script_decode.py --all index with an explicit SHA-256,
one resource, its declared entry controls, selector offset and local-number
index. RULE-SCRIPT-002 and RULE-SCRIPT-003 describe local calls and returns;
FND-SCRIPT-024 describes the index and its limits. Do not decode a rejected
span by assuming the next listed instruction is contiguous with its predecessor.

Acceptance:

- Keep local call continuations on a bounded stack and reach them only on a
  modeled local return. RULE-TALK-001's menu choices also push a local frame;
  there is no no-choice fall-through. Keep selection admission and possible
  menu/frame-array overlap explicit. Branches and menu choices remain conservative candidate
  alternatives; cross-script calls, missing/computed targets, rejected spans,
  frame limits and state limits produce explicit unresolved boundaries.
- Report the last direct assignment before the first target on each candidate
  route. Distinguish no known assignment, a literal zero, a nonzero literal and
  an expression. Track intervening other effects conservatively; never call
  a direct assignment history a proof of preserved local storage or an admitted
  runtime value. Native effects and opcode callbacks remain outside the graph.
- Parse a bounded UTF-8 listing, reject duplicate/unsorted instructions, invalid
  roots/targets/limits, hash mismatches and empty resource selections. Identify
  the listing, config and tool revisions in the report. Output stays local for
  original-derived inputs; synthetic fixtures contain no licensed data.
- Synthetic controls cover assignment inside a returning callee, a callee that
  stops instead of returning, a conditional assignment bypass, explicit zero
  versus no assignment, effect tracking and reset by a later assignment,
  expression assignments, cross-script boundaries, missing and computed
  transfers, rejected spans, menus, recursion/caps and malformed inputs.
- Run the existing declared roots and assignment controls only after these
  synthetic tests pass. Preserve partial results and actual-source limitations;
  research findings and queue changes belong in a later research batch.

Exit: synthetic controls and Test.ps1 pass; the tool and its documented command
are committed as tooling. No spec/parity status or original-content contract
changes. The resulting reports alone do not close Q-COMBAT-056.
