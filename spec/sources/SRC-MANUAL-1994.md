---
id: SRC-MANUAL-1994
title: "Dark Sun: Wake of the Ravager rule book, PDF shipped with the GOG release"
superseded_by: []
author: Strategic Simulations, Inc.
date: "1994"
location: Manual.pdf in the installation directory of BLD-GOG-EN-1.1
xxh3: 61febce4b0b6501be0253a3da1bed147
licence: All rights reserved by the publisher; only short quotations are used.
---

## Use

The designers' description of the controls, menus, character generation, the
origins and classes, combat commands, magic, psionics, camping, training and
advancement, and the credits. It is the best outside source for what a
mechanic is meant to do, and it says nothing about edge cases. The file is
1,455,599 bytes and holds 41 landscape PDF pages scanned from 77 numbered
manual pages; citations give the numbered manual page. The installation ships
a byte-identical copy named `ds_wakerave_manual_pdf.pdf`.

### Screen coverage

The manual's named views, panels and choice boxes map to these screen entries.
The page numbers are the manual's printed numbers. A description from the
manual alone leaves an entry `sourced`; it does not establish shipped pixels
or handlers.

| Manual page | View, panel or choice | Screen entry |
|---|---|---|
| 2 | Start window | SCR-UI-001 |
| 4-6 | Exploration view, Look options and conversation | SCR-EXPLORE-001, SCR-UI-011, SCR-UI-012 |
| 6 | Training selection and recent spell choice | SCR-UI-016, SCR-UI-017 |
| 7-10 | Character overview, generation, stored-character list, discipline and sphere lists, character-box choices | SCR-UI-002, SCR-UI-003, SCR-UI-004, SCR-UI-005, SCR-UI-015 |
| 11-13 | Inventory, item summary, store, spell selection and active effects | SCR-UI-008, SCR-UI-018, SCR-UI-019, SCR-UI-009, SCR-UI-010 |
| 14 | Game Menu, exit choice, load or save choice, Load Game and Save Game | SCR-UI-006, SCR-UI-020, SCR-UI-021, SCR-UI-013, SCR-UI-014 |
| 15 | Preferences, overhead map and About information | SCR-UI-007, SCR-UI-022, SCR-UI-023 |

## Known errors

- The origin descriptions on pages 17 and 18 and the class descriptions on
  pages 19 to 22 give different class lists for some origins: they disagree on
  half-giant ranger and thief, mul druid, and thri-kreen druid and thief.
  SRC-README-1.1's table 3 sides with the origin descriptions on the
  half-giant pairs and the thri-kreen thief, and with the class descriptions
  on the mul and thri-kreen druid. The executable's list has not been read
  (RULE-PARTY-007).
- Pages 8 and 9 name only clerics as choosing an elemental sphere, while the
  class descriptions on pages 20 and 21 give rangers and druids one as well
  (RULE-PARTY-003).
- It calls the four difficulty choices Easy, Balanced, Hard and Hideous, and
  then calls the default Average. The executable's table has no Average label.
- Other disagreements with the executable are recorded in the What the sources
  say section of each rule that relies on this source.
