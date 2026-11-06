---
id: FND-SOUND-077
title: Shipped scripts contain 37 literal music-health override requests with 27 constant values and ten local-variable values
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00000000..0x00217249
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C19A..0x0008C1A3
tool: script_decode.py and tools/evidence/xxh3.mjs at 211ea53c; Python 3.12.10, xxhash 4.0.1; operand contracts from FND-SCRIPT-024
environment: null
---

## Observation

This census follows FND-SOUND-076's known script writer into the shipped
script resources. Installed GPLDATA.GFF is 2191945 bytes, XXH3-128
2bd1d2b989eef2dcb0545feb95975fca; OBJEX.GFF is 6816516 bytes,
XXH3-128 e08d3cac153c547f6804ca27b155915d. The latter supplies FNFO/2
for the existing decoder. The GPLDATA location above is the whole-file
resource census scope, not a native instruction range.

The committed script decoder visits 350 GPL and MAS resources. Its
sweep reports 349 decoded without a stop, 26003 checked literal code
offsets on decoded instruction boundaries, and 797 checked literal
cross-script calls on decoded instruction boundaries. GPL/37 retains
the known stop at 0D39 on expression byte B3 and resumes at the known
literal target 0D58 (FND-SCRIPT-024). This is a decoding boundary
census, not a reachability or runtime execution proof.

The decoded output contains 3577 opcode-22 occurrences, including nested
instruction expressions. Of those, 37 have literal function selector 52:
34 GPL sites and three MAS sites. FND-VIDEO-010 binds opcode 22's
four parameters to the dispatcher; FND-SOUND-076 binds request 52 to
the low byte of its third parameter stored at DS:14E7.

All 37 decoded sites supply the same kind-nine variable number 40 as
their second parameter and literal zero as their fourth. Neither is
used by the request-52 store itself. Twenty-seven third parameters are
literal constants, with this population:

| Health value | Number of literal sites |
|---|---|
| 1 | 5 |
| 2 | 1 |
| 5 | 4 |
| 6 | 4 |
| 7 | 5 |
| 8 | 4 |
| 9 | 3 |
| 10 | 1 |

The other ten read kind-two local number variables (RULE-SCRIPT-004):
index zero at three sites, one at three, two at one, seven at one and
nine at two. Their values are not bounded by this syntax census.
Concrete examples are GPL/5 at 0318 supplying two, GPL/126 at 06F1
supplying ten, and MAS/50 at 05D3 supplying one. MAS/51 at 05D5 and
MAS/67 at 05C3 are the other two MAS sites.

There is one decoded opcode-22 occurrence whose selector is not literal:
GPL/27 at 01C8 takes it from kind-seven variable eight, with its third
and fourth parameters from kind-seven variables five and six. This
matches FND-ACTOR-014's independent dynamic-selector observation. It
could select 52 only under appropriate runtime variable values, which
are not established here. The GPL/37 undecoded interval could also
contain an unclassified request. Changed scripts, other archives and
non-script callers remain outside this installed-resource census.

The independent known script control MAS/50 at 05DC decodes as request
37 with the recorded fade argument one (FND-UI-050). The dynamic-selector
control and the known GPL/37 gap are both retained; no negative result
is accepted by ignoring either one.

## Interpretation

The direct health override has concrete shipped script producers. For
the 27 literal-value sites, the requested low byte is now known and in
1..10. The ten local-variable values, the dynamic function selector,
the undecoded interval and actual script scheduling remain open. These
counts do not show that any request executes before a given music
selection; FND-SOUND-076's ordinary tick overwrites the field first.
No whole-entry complete reading or question closure follows.

## Alternatives

Treating request 52 as unused is contradicted by decoded literal sites.
Treating every script write as bounded to 1..10 ignores the ten variable
inputs, dynamic request and gap. Treating syntactic calls as all reachable
or all preceding the tick would require control-flow and state evidence
this census does not supply.

## How to reproduce

Use script_decode.py at 211ea53c on the installed GPLDATA.GFF and
OBJEX.GFF with --all and --sweep. Retain music-health-all-scripts.txt
and music-health-script-sweep.txt under
GAME_DIR/analysis/handler-census-20261106/. Verify both source hashes
with tools/evidence/xxh3.mjs before reusing the reports.

In the all-script report, match decoded instruction positions with
`[0-9A-F]{4}: 22 `, including nested positions, and classify the first
parameter as literal 52, another literal or nonliteral. For the 37
literal-52 records, count the third parameter's literal or kind-two
variable expression. Preserve both GPL/37 stop/error lines as one gap,
not two failed resources. Check the MAS/50 05DC and GPL/27 01C8 controls.
FND-SCRIPT-024 gives the decoder's operand derivation and limitations.
No native game run or emulated call supplied the result.
