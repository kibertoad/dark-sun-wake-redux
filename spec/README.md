# Dark Sun: Wake of the Ravager specification

## Scope

This spec describes *Dark Sun: Wake of the Ravager*, the party-based role-playing game that
Strategic Simulations, Inc. released for DOS in 1994. It covers the one build studied so far, the
English GOG release listed in `builds/`: the `DSUN.EXE` executable with its overlay pack, the
installed data files, the data files read from the disc image, and the music tracks the GOG
release plays in place of the disc's audio.

In scope are the file formats the game reads and writes, the rules it applies (movement, the
random number generator, character creation and progression, magic, items, combat, dialogue and
its script interpreter, time and sound), and the screens, panels and dialogs the player sees. The
game's content is out of scope: names, texts, images, sounds, maps, scripts and the per-item or
per-creature statistics the designers filled in stay in the player's copy of the game, and the
spec refers to them by resource. The setup and helper programs that ship with the game are in
scope only where the game's own behaviour depends on them.

The original and its manual use "race" for the peoples a character can belong to. The spec uses
the term `origin` for that concept, and its glossary entry gives "race" as the name the game shows
the player.

The spec describes the original game and nothing else. It never names a class, file or setting
from the rebuild in this repository.

## Standard version

This spec follows version 1 of the
[documentation standard](https://dinorefurb.com/documentation-standard/).

## Areas

Areas are added to this list and never removed or renamed.

| Area | Covers |
|---|---|
| `EXE` | `DSUN.EXE` structure: the MZ image, the `FBOV` overlay pack, its trampolines and loader. |
| `GFF` | The GFF container: directory, tags, resource lookup. |
| `IMAGE` | `BMP `, `CBMP`, `ICON`, `PORT` images, `PLAN`/`PLNR` encodings, palettes. |
| `TEXT` | `FONT` glyphs, `TEXT` resources, the executable's string tables, text layout. |
| `UI` | `WIND`, `BUTN`, `APFM`, `EBOX` records and every screen, menu and panel built from them, including the layout and controls of the dialogue window. |
| `INPUT` | Mouse, keyboard and cursor handling, including the cursor family and hotspots. |
| `REGION` | Region maps: `RNME`, `MAP `, `GMAP`, `TILE`, `ETAB`, static composition. |
| `ACTOR` | Object and actor records: `OJFF`, `RDFF`, `MONR`, placement and footprints. |
| `EXPLORE` | Party movement, occupancy, camera, display modes and the travel view. |
| `SCRIPT` | `GPL `, `MAS `, `GPLI`, `SCMD` and the script interpreter: what each instruction does. |
| `TALK` | Conversations: what happens when a response is chosen, conditions, response selection, and the dialogue-specific procedures that call the script interpreter. |
| `PARTY` | Character creation and records (`CHAR`, `PSIN`), origins, classes, dual-class progression, the party roster. |
| `MAGIC` | Spells, psionic disciplines and clerical spheres. |
| `ITEM` | Items, `ITEMS.BIN` and inventory. |
| `COMBAT` | Combat entry, turns, commands, hit and damage arithmetic, the status panel. |
| `AI` | Decisions made for computer-controlled actors. |
| `RNG` | The random number generator and the functions that reduce its draws. |
| `TIME` | Clocks: BIOS ticks, the timer chip, display retrace, animation cadence. |
| `SOUND` | `VOC` files, the sound helper, music and CD audio. |
| `VIDEO` | `FLI` cinematics. |
| `CONFIG` | `SOUND.CFG`, `SOUND.INI`, `PREF` and the Preferences settings. |
| `SAVE` | Saved games and the state they hold, including `CHARSAVE.GFF` and the small `GREQ`/`CACT` families. |
| `QUEST` | Campaign progress, quest state and the route through the regions. |
