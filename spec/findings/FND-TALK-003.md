---
id: FND-TALK-003
title: About 709 menus in 219 scripts have up to 24 entries and almost all are titled by global string 4; global flag 357 appears only in GPL 135
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00..0x217249
tool: hex inspection with Python 3.14.7, expressions read as RULE-SCRIPT-004 describes
environment: null
---

## Observation

Every byte `0x48` in the 330 `GPL ` and 20 `MAS ` resources of `GPLDATA.GFF` was read as the
start of a menu instruction (FND-TALK-001): one expression, then three per entry, until byte
`0x4A`. A byte was counted as a menu when its title expression is a single global string variable
and the reading reached `0x4A` within 60 entries without an unknown expression byte. The scan
does not follow the instruction stream, so it can miss menus whose title has another form and
could count a `0x48` byte inside other data that happens to parse.

- 709 menus in 219 resources, all `GPL `. Their title is global string 4 in 687 and global string
  1 in 22.
- Entries per menu: 0 in one, 2 to 12 in 633, and 13 to 24 in 75. None has more than 24.
- Entry conditions: a single variable in 3,646 entries, a literal in 1,241, a variable compared
  with a byte inside parentheses in 323, and other shapes in 5.

A byte search of the same resources for `CD 01 65`, the extended global flag 357, finds it only in
`GPL/135`, at offsets 611, 711, 1798 and 1821 (FND-TALK-002).

## Interpretation

Menus are the scripts' way of offering responses. No menu has more entries than the 25 the
instruction can offer, so its limit is never reached by entry count alone. Global string 4 is
the usual title (FND-TALK-002 shows `MAS/99` setting it). Most conditions are flags; the literal
1 marks entries that are always offered. Global flag 357 is private to the first conversation.

## Alternatives

The counts are those of a scan and not of a reading that follows each script's instructions from
its entry points, so they are approximate in both directions.

## How to reproduce

Run the scan above over every `GPL ` and `MAS ` resource, with the expression reader of
RULE-SCRIPT-004 skipping strings of kind 5 as FMT-SCRIPT-002 describes.
