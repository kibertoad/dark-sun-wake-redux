---
id: FND-ITEM-046
title: The dynamic GPL twenty-seven request loop has sixteen inspected literal caller leads selecting functions ten through thirteen
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
tool: Retained script_decode.py indexed source of FND-COMBAT-100; bounded source reading at 6136ea1a; composition with FND-ITEM-045 and RULE-SCRIPT-003/004
environment: null
---

## Observation

FND-ITEM-045 leaves one decoded top-level nonliteral function request,
GPL/27 at 01C8 hex. This reading identifies its complete small loop and
the function inputs of sixteen inspected exact literal cross-script caller
leads. These are positive source routes, not an exhaustive incoming-call
census or proof that they execute natively.

The indexed source SHA-256 is
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Offsets below are hexadecimal; function numbers and counts are decimal.

### Loop at GPL/27 entry 01BF

The source compares global number four with zero at 01BF and uses a while
branch at 01C5 to return at 01EA when the signed value is not positive.
Its body at 01C8 requests opcode 22 with global number eight as function,
the named kind-nine variable 40 as target, global number five as field
and global number six as value. These are evaluated afresh each iteration.

After the request returns, it reads whether global number seven equals
one. Equality increments global number five at 01DA; otherwise it
increments global number six at 01E0. Both branches join at 01E3, then
the body decrements global number four at 01E4 and jumps back to 01BF
at 01E7. The source does not test the request's result as success or
failure; its subsequent comparison supplies the branch state instead.

All three increments/decrements are word operations. If the current
global bindings, function/target/mode words and the other loop inputs
survive the request and control instructions except for these named
writes, a positive initial count C produces C requests and ends at zero.
Initial zero or negative count produces none. Mode one advances the
field word modulo 65536; another retained mode advances the value word.
The words are interpreted through their signed variable reads on later
iterations. Callee writes to those globals, aliases or changed control
state can invalidate this bounded-loop deduction.

The loop has no own assignment to global number eight. Its source being
dynamic therefore does not imply that function 49 is selected on a route
whose caller and callees retain another function value.

### Inspected exact literal callers

An exact printed top-level source call with operands offset 447 decimal
(01BF) and script 27 gives sixteen positive leads. Their nearby source
setups assign global number eight as follows:

| Caller resource | Call offsets | Function |
|---|---|---:|
| GPL/1 | 037D,03A5,0C0F,100D | 11 |
| GPL/1 | 0387,03AF,0C19 | 13 |
| GPL/1 | 03CD,03F5,0C29 | 10 |
| GPL/1 | 03D7,03FF,0C33 | 12 |
| GPL/163 | 12AA | 12 |
| GPL/164 | 04C2 | 13 |
| GPL/185 | 0BA2 | 12 |

For GPL/1's first eight calls, the function assignments are at 0378,
0382,03A0,03AA,03C8,03D2,03F0,03FA respectively. The next four
are at 0C0A,0C14,0C24,0C2E; the final one at 0FF9. Other callers
assign at GPL/163 1288, GPL/164 04A9 and GPL/185 0B80.
GPL/163 and GPL/185 additionally bracket the cross-script call with
function-29/30 requests; their state effects must not be silently ignored.

Require each inspected setup to reach its call without replacing its
function word, and retain that word, global-pointer identity and unchanged
opcode-22 dispatch through the helper's requests. Then every actual
request in these sixteen route classes selects 10,11,12 or 13, never
the scalar setter's function 49. This conclusion does not require finding
the field word's numeric range on those same preserved routes.

The count is also caller state. Some GPL/1 calls immediately follow a
previous helper call without a new count assignment. A prior positive
loop that retains and consumes its counter leaves zero, so a later
call need not make any request. Sixteen source call sites are not sixteen
observed requests or sixteen independent fresh counter initializations.

## Interpretation

This removes the loop's own operation and the inspected callers' function
inputs from Q-ITEM-006's unknown scalar-position/word producers. The dynamic
source cannot select function 49 within those preserved route classes.
Remaining alternatives include entry after the setup or directly into the
loop, other local/cross-script/computed/native entries, replaced operand or
global storage, changed dispatch and callee/alias effects on the function
word. FND-ITEM-045's nested/gap/other-source limits still apply.

No complete-reading, evidence-status or parity promotion follows, and no
original script or game function was executed.

## Alternatives

A dynamic expression alone admits an unknown function, but the named
positive caller setups supply specific non-49 values. Conversely, those
sixteen values do not prove every incoming route has been found or that
the values survive arbitrary callees. A source call after counter exhaustion
is not a new bounded loop unless its count is replenished.

## How to reproduce

Reuse GAME_DIR/analysis/reader-admission-20261029/scripts.txt with the hash
above. Source regeneration and archive fingerprints are FND-COMBAT-100
and FND-ITEM-045. Read only GPL/27 01BF..01EA and these caller blocks:
GPL/1 0364..03FF,0C0A..0C33,0FF4..100D; GPL/163 1288..12AF;
GPL/164 04A9..04C2; GPL/185 0B80..0BA7.

For positive lead discovery, match only canonical indexed lines with
resource GPL/NUMBER, a four-digit hexadecimal offset, opcode 14 and
exact printed operands `447, 27`. The sixteen identities are retained in
GAME_DIR/analysis/item-removal-20261104/dynamic-scalar-literal-leads.json.
That metadata explicitly labels this a non-exhaustive positive query.
Check every function assignment above from its bounded source block,
not by assuming the lead search resolves branch feasibility or native state.
No original listing or payload fixture is committed.
