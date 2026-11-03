# Typed offset-word candidate classification

Research tooling for Q-SCRIPT-020. The existing far_pointer_words.py helper
can label a memory displacement as an immediate when rendered instruction text
contains the queried value. The retained candidate at file 0x2FE44 is a lead,
not an accepted conversion-entry instruction. Fix the diagnostic classifier
before using its immediate candidates as evidence.

Outcome: researchers can distinguish a queried word inside a MOV/PUSH immediate
from the same number in a displacement or another operand. This adds no game
behavior, original-content contract or runtime capability.

Use Capstone's typed operands and encoded immediate offset/size, with detail
enabled. Require both queried bytes to belong to the immediate and match the
queried word. Preserve the existing six-byte holder search, source fingerprint,
source ranges, CLI and far-call/relocated-pair classifications. All results
remain candidates until instruction ownership and use are independently read.
Make the module import-safe so synthetic controls need no licensed source.

Acceptance:

- A memory displacement equal to the query is not an immediate, including a
  MOV with a different immediate and one with equal displacement/immediate.
- Word PUSH/MOV immediates and words within a four-byte immediate are
  classified at their encoded positions. A one-byte immediate or a word
  crossing its boundary is not enough. Relative branches remain outside
  MOV/PUSH classification. Nonzero instruction addresses are preserved.
- Synthetic x86 inputs live under tests/fixtures/synthetic and are decoded,
  never executed. Their test imports the helper without source access.
- The CLI still verifies the installed DSUN.EXE identity and writes no source.
  Original-derived reports remain in GAME_DIR. Existing far-pointer evidence
  does not gain a global literal-census or caller-completeness claim.
- Run the focused synthetic control and Test.ps1 before the tooling commit.
  A later research batch reruns the retained offset-word query with the new
  tool identity, qualifies its candidates and records any game observations.

Exit: typed classification and synthetic controls pass, the tool/documentation
and gate integration are committed as tooling, and no game spec, queue or
parity claim changes in that commit.
