---
id: FMT-SOUND-002
title: Music table DJ.DAT
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["DJ.DAT", "CD:DJ.DAT"]
byte_order: little
size: 231
text: false
definition: fmt_sound_002.ksy
evidence: [FND-SOUND-011, FND-SOUND-012, FND-SOUND-013]
conflicting: []
split_with: []
related: []
---

## Layout

The whole of `DJ.DAT`, which the game reads at startup [FND-SOUND-011] and its music selector
reads each time it chooses a song [FND-SOUND-012].

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `UINT8` | `count` | The number of records, 38. | supported | FND-SOUND-011 |
| `0x01` | 2 | `UINT16LE` | `retry_passes` | After a failed chance, the number of selector runs to pass before choosing again: 1,000. | supported | FND-SOUND-011, FND-SOUND-012 |
| `0x03` | 6 × `count` | `music_record[count]` | `records` | The songs. | supported | FND-SOUND-011 |
| `0xE7` | | | | Total size 231 | | |

`music_record`, 6 bytes:

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 1 | `INT8` | `region` | The region number the song belongs to, or -1 (255) for any region. | supported | FND-SOUND-012 |
| `0x01` | 1 | `UINT8` | `chance` | Passed to `chance_in_ten` before a mode-3 song is played; 10, 8, 5 or 3. | supported | FND-SOUND-012 |
| `0x02` | 1 | `UINT8` | `health_band` | For mode-3 songs, the band of party health it is played in: 1, 2 or 3. | supported | FND-SOUND-012 |
| `0x03` | 2 | `UINT16LE` | `mode` | The music mode the song is chosen in: 1, 2 or 3. | supported | FND-SOUND-012 |
| `0x05` | 1 | `UINT8` | `song` | The song number, played as disc track `song + 1`; 1 to 35. | supported | FND-SOUND-012, FND-SOUND-013 |

## Enumerations and flags

`mode`: 1 at startup, 3 in combat after a spoken line, 2 not chosen by the selector
[FND-SOUND-012].

`health_band`: 1 when the party-health measure is below 4, 2 from 4 to 7, 3 above 8
[FND-SOUND-012].

## Differences between builds

None known.

## Coverage

The one installed file [FND-SOUND-011].

## Open questions

- What chooses the 18 records of mode 2, which the selector never takes (FND-SOUND-012,
  Q-SOUND-004).
