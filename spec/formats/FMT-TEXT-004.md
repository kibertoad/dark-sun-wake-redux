---
id: FMT-TEXT-004
title: Preferences text block in DSUN.EXE
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DSUN.EXE"]
byte_order: little
size: 529
text: false
definition: fmt_text_004.ksy
evidence: [FND-TEXT-005]
conflicting: []
split_with: []
related: []
---

## Layout

A block of the resident data of `DSUN.EXE`, at `5000:A4B9` (file offset `0x4F6B9`) in
BLD-GOG-EN-1.1 [FND-TEXT-005]. It holds the strings of the Preferences screen: the four
difficulty labels, ten descriptions of the screen's settings, and the nine lines of its About
text. The strings are ASCII, printable, and each ends with a NUL that the field includes. The
far pointers hold the segment as it is stored in the file; the MZ loader adds the load segment
to it through the relocation table, so `0x47E0` becomes `0x57E0` with the image at `0x1000`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x000` | 16 | `FARPTR<char[]>[4]` | `difficulty_labels` | Pointers to `difficulty_label_0` to `difficulty_label_3`, in that order. | supported | FND-TEXT-005 |
| `0x010` | 8 | `UINT16LE[4]` | `unk_010` | Purpose unknown. 205, 143, 231 and 24. | supported | FND-TEXT-005 |
| `0x018` | 36 | `FARPTR<char[]>[9]` | `about_lines` | Pointers to `about_line_0` to `about_line_8`, in that order. | supported | FND-TEXT-005 |
| `0x03C` | 5 | `char[5]` | `difficulty_label_0` | First difficulty label, `EASY`. | supported | FND-TEXT-005 |
| `0x041` | 9 | `char[9]` | `difficulty_label_1` | Second difficulty label, `BALANCED`. | supported | FND-TEXT-005 |
| `0x04A` | 5 | `char[5]` | `difficulty_label_2` | Third difficulty label, `HARD`. | supported | FND-TEXT-005 |
| `0x04F` | 8 | `char[8]` | `difficulty_label_3` | Fourth difficulty label, `HIDEOUS`. | supported | FND-TEXT-005 |
| `0x057` | 17 | `char[17]` | `resource_path_pattern` | `%c:\RESOURCE.GFF`, a `sprintf` pattern for the path of `RESOURCE.GFF` on a drive. | supported | FND-TEXT-005 |
| `0x068` | 9 | `char[9]` | `about_pattern` | `%C%C%C%s`. | supported | FND-TEXT-005 |
| `0x071` | 20 | `char[20]` | `description_0` | First setting description, `MUSIC TOGGLE ON/OFF`. | supported | FND-TEXT-005 |
| `0x085` | 14 | `char[14]` | `description_1` | Second setting description, `MESSAGE DELAY`. | supported | FND-TEXT-005 |
| `0x093` | 21 | `char[21]` | `description_2` | Third setting description, `SOUND EFFECTS ON/OFF`. | supported | FND-TEXT-005 |
| `0x0A8` | 20 | `char[20]` | `description_3` | Fourth setting description, `SOUND EFFECT VOLUME`. | supported | FND-TEXT-005 |
| `0x0BC` | 16 | `char[16]` | `description_4` | Fifth setting description, `GAME DIFFICULTY`. | supported | FND-TEXT-005 |
| `0x0CC` | 18 | `char[18]` | `description_5` | Sixth setting description, `ANIMATIONS ON/OFF`. | supported | FND-TEXT-005 |
| `0x0DE` | 6 | `char[6]` | `description_6` | Seventh setting description, `ABOUT`. | supported | FND-TEXT-005 |
| `0x0E4` | 22 | `char[22]` | `description_7` | Eighth setting description, `SPEECH EFFECTS ON/OFF`. | supported | FND-TEXT-005 |
| `0x0FA` | 10 | `char[10]` | `description_8` | Ninth setting description, `GAME MENU`. | supported | FND-TEXT-005 |
| `0x104` | 15 | `char[15]` | `description_9` | Tenth setting description, `RETURN TO GAME`. | supported | FND-TEXT-005 |
| `0x113` | 30 | `char[30]` | `about_line_0` | First About line, starting with `%C%C%C`. | supported | FND-TEXT-005 |
| `0x131` | 27 | `char[27]` | `about_line_1` | Second About line, starting with `%C%C%C`. | supported | FND-TEXT-005 |
| `0x14C` | 27 | `char[27]` | `about_line_2` | Third About line, starting with `%C%C%C`. | supported | FND-TEXT-005 |
| `0x167` | 34 | `char[34]` | `about_line_3` | Fourth About line, starting with `%C%C%C`. | supported | FND-TEXT-005 |
| `0x189` | 29 | `char[29]` | `about_line_4` | Fifth About line, starting with `%C%C%C`. | supported | FND-TEXT-005 |
| `0x1A6` | 30 | `char[30]` | `about_line_5` | Sixth About line, starting with `%C%C%C`. | supported | FND-TEXT-005 |
| `0x1C4` | 25 | `char[25]` | `about_line_6` | Seventh About line, starting with `%C%C%C`. | supported | FND-TEXT-005 |
| `0x1DD` | 24 | `char[24]` | `about_line_7` | Eighth About line, starting with `%C%C%C`. | supported | FND-TEXT-005 |
| `0x1F5` | 28 | `char[28]` | `about_line_8` | Ninth About line, starting with `%C%C%C`. | supported | FND-TEXT-005 |
| `0x211` | | | | Total size 529 | | |

## Enumerations and flags

None.

## Differences between builds

Only BLD-GOG-EN-1.1 has been read.

## Coverage

The block in the installed `DSUN.EXE` of BLD-GOG-EN-1.1 [FND-TEXT-005].

## Open questions

- What reads the block. Ghidra finds no direct reference to the tables or strings
  (FND-TEXT-006), so which code draws the Preferences screen, which description belongs to which
  control, and what `unk_010` holds are not known (Q-TEXT-003).
- What `%C` does in `about_pattern` and the About lines (Q-TEXT-003).
- Which difficulty the game starts with. SRC-MANUAL-1994 (page 15) lists the settings as Easy,
  Balanced, Hard and Hideous and names the default as Average, which is none of the four labels
  (Q-TEXT-003).
