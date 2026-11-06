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
| `0x03` | 6 Ãƒâ€” `count` | `music_record[count]` | `records` | The songs. | supported | FND-SOUND-011 |
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

### Closure assessment and finite dependency map

The existing-evidence assessment keeps this entry supported. FND-SOUND-011
records the shipped size and loader branches; FND-SOUND-012 records the
selector's record fields and local branches, including its special health-8
route; FND-SOUND-013 records conditional song dispatch. These findings do
not yet establish every access to the file and loaded structure. No finding
qualifies for a whole-entry complete_reading declaration on that evidence.

Q-SOUND-011 owns the remaining file/loaded-structure access census: count
at DS:4252, retry word at DS:4253 and record pointer at DS:424E. FND-SOUND-061 now classifies the known literal field accesses and loader
failure writes. FND-SOUND-062 classifies the eight field-window candidates
as loader interiors or combat-table data; FND-SOUND-063 classifies the
four exact-name interiors. Excluded computed/aliased/indirect mechanisms
and actual bindings remain. FND-SOUND-064 narrows the known selector
computed-access component to its four DS list stores, mixed DS/SS reads,
word-offset calculation and explicit capacity/population conditions.
FND-SOUND-065 supplies nonempty shipped populations of nine mode-one
fallbacks and four/three/three mode-three records, retaining actual
loaded-byte and frame/binding conditions. FND-SOUND-066 supplies the
known RNG children's direct segment/write interface; actual state/list/
table/frame aliasing remains Q-SOUND-011. A missing
access could change a field's extent, signedness or interpretation. Unknown
computed/indirect accesses remain obligations. Successful generic I/O and
unrelated playback output are not themselves layout claims of this entry.
Q-SOUND-004 retains mode-2 use and the selected song's dispatch/track meaning;
physical playback success is not inferred from the encoded song field.
FND-SOUND-067 records local song-plus-one forwarding and disc-request
publication/status contracts; external handler/output and actual child
heap/frame aliases remain unresolved. FND-SOUND-068 bounds the related
busy/stop interface, including error-sentinel classification and conditional
cleanup; it does not establish external success or table lifetime.

The known inventory roots in coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv are
2834:043B (acquisition), 2834:000C (selection), 2834:0508 (retry state),
2834:0665 (song forwarding), 4A32:0011 (dispatch) and 28C9:389A (health
input). They are shared with RULE-SOUND-003; 2834:0519 and 2834:061C
are shared RNG helpers whose relevant interface contracts may be reused
without absorbing all their callers into this format. The unknown accesses
and downstream driver bodies have no complete-reading ownership assigned
by this assessment. Other functions and inventories remain outside this
partial map; the independent playback plan's counts are not added to it.
