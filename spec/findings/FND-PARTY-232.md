---
id: FND-PARTY-232
title: DUAL class selection requests WIND 17502 and maps its eight buttons to the eligibility flags
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009439E..0x000947E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000947FC..0x000948C9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000948DB..0x0009493C
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00062B08..0x00062D1B
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0006200C..0x0006237C
tool: tools/research/exec-census/overlay_listing.py; DarkSunWakeRedux.Inspect gff; tools/evidence/report.mjs table; Python 3.12.10, Capstone wheel 5.0.9 (module version 5.0.7), xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-161 records overlay 209 +123E's class-test loop,
eight availability flags, button-closure requests and +169C handler.
This reading connects that logic to the requested window resource
and its shipped child/button metadata. It is the class-selection
window, not SCR-UI-015's character-box context menu.

The opener tests original class codes 1 through 17 in order with
+19A7 and maps them to flags k=0..7 as FND-PARTY-161 describes.
Each test writes its group's flag zero or one; a separate byte is
set to one whenever any test returns nonzero and is not cleared by
later failures. If that byte stays zero, the opener requests the
no-other-classes message and skips its class-window creation branch.
RULE-PARTY-004 records the supported eligibility test, including
origin, level, abilities, class group and alignment conditions.

The creation branch registers callback 57B0:0048, then calls
56BD:0048 with window ID 17502 (445E hexadecimal), position words
73 and 41, and that same callback. It stores returned DX:AX at
mapped 55BD:0004. This is a requested placement and stored result,
not proof that resource acquisition or registration succeeded.
FND-CONFIG-018/030 retain the window helper's failure gates.

For each resulting zero flag k, the opener requests button code
one for ID 17303+k through 3EBE:071F. FND-PARTY-077 maps that
code to setting bit two of the loaded button's mask and bit 4000
of its word at +0C. A nonzero flag skips that request; this loop
does not explicitly send code zero to enable a reused closed button.
Actual loaded-control state and helper outcomes remain conditions.

The installed RESOURCE.GFF directory locates WIND/17502 at decimal
404232, length 531. A fresh numeric query confirms width 172,
height 117, image word 17002 and nine children. Its first child is
BUTN/17302 at window-relative (126,99); the other eight are:

| k | Button | Logical class family | Child origin | ICON reference |
|---|---|---|---|---|
| 0 | 17303 | Cleric, original codes 1..4 | (89,25) | 17102 |
| 1 | 17304 | Druid, original codes 5..8 | (89,34) | 17101 |
| 2 | 17305 | Fighter, original code 9 | (89,43) | 17105 |
| 3 | 17306 | Gladiator, original code 10 | (89,52) | 17107 |
| 4 | 17307 | Preserver, original code 11 | (89,61) | 17103 |
| 5 | 17308 | Psionicist, original code 12 | (89,70) | 17108 |
| 6 | 17309 | Ranger, original codes 13..16 | (89,79) | 17106 |
| 7 | 17310 | Thief, original code 17 | (89,88) | 17104 |

All eight class buttons have source width 79, height 8, mask zero
and no tail bytes. These are source headers, not native icon-frame
or enabled-state observations. FND-PARTY-154 separately records
the shared exit button BUTN/17302's 42-by-12 dimensions and label.

FND-PARTY-161's event-two handler maps these IDs to the listed
class families. Cleric, druid and ranger take the sphere-menu route;
returned values 1..3 adjust their base class code and other returned
values retain the base. This does not establish what every native
menu cancellation returns. The handler calls +17DC and then the
close helper +177B; it does not itself rerun +19A7 on receiving the
button event. That is not a claim about child validation or delivery
of events to a closed control. ID 17302 and event kind six/code
011B take the close route without that local class-change call.
Event kind one calls the close helper, clears current DS:1462 and
returns FFFF; other ordinary returning paths call 2C5F:0182 and
return zero. The shared byte's complete role is not assigned here.

The opener's event loop polls 39D1:097D, dispatches a returned
24-byte event through 39D1:071F when AX was zero, and repeats
while current 55BD:0004 is nonnull. The close helper performs
several state/cleanup calls and clears that field after them, with
no test of their returned status. It requests window cleanup only
on its current DS:1431-null branch. Normal child returns and
preservation are required to connect that clear to loop exit.
Native input delivery and the children remain unconfirmed here.

## Interpretation

Q-PARTY-007's class-offer/closed-button mapping belongs to
SCR-PARTY-003. The rule's previous SCR-UI-015 citation identified
a different menu. The source geometry, logical choices and exact
closure requests can be documented without claiming native paint,
successful loaded state, input delivery or the remaining child effects.

## Alternatives

- A class-choice list and a character-box context menu have different
  source controls and roles; they are not one screen entry.
- Treating flags as OR-accumulated omits their per-code overwrites;
  only the separate any-success byte is sticky in the local loop.
- Treating a nonzero flag as an explicit enable call adds work this
  loop skips. Actual source/reused control state still matters.
- Treating direct handler delivery as an eligibility recheck adds a
  call absent from that handler's local body.

## How to reproduce

Verify installed DSUN.EXE size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read descriptor 209
9439E..947E2, 947FC..948C9 and 948DB..9493C, hexadecimal
and half-open. Follow the jump tables qualified in FND-PARTY-161.
The inventory omits reached instructions within the opener, including
part of the call at 945E1; do not restart decoding at those fragment
boundaries or call the current inventory a complete reading.

Run the existing Inspect gff command on RESOURCE.GFF, then use
report.mjs table with size 5724669 and XXH3-128
92fe031616f36062f99484b0f728210e. Query WIND/17502 at
404232: header offsets BE/C0/C2/F3 and children at +261,
nine records of stride 30 with tag/number/x/y offsets 4/8/12/14.
Query BUTN/17303..17310 at 401420, eight records of stride
110, using FMT-UI-003's width/height/mask/icon/tail-length fields.
The fresh directory verifies each record's number, offset and size.
Reports and numeric query definitions remain under
GAME_DIR/analysis/party-dual-window-20261018/.

Use FND-PARTY-161 for the eligibility and handler tables and
FND-PARTY-077 for code-one semantics. Treat coordinates and ICON
numbers as source/request mappings; no image was viewed and no
original game or function was executed in this batch.
