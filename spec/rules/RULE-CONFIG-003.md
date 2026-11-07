---
id: RULE-CONFIG-003
title: Music-level readback on the Preferences screen
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-235, FND-CONFIG-010, FND-CONFIG-236, FND-CONFIG-237, FND-SOUND-084, FND-SOUND-085, FND-SOUND-086, FND-SOUND-087, FND-CONFIG-029, FND-UI-034, FND-UI-065, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-CONFIG-004, RULE-CONFIG-005, SCR-UI-007]
---

## Summary

On entry with sound and music enabled, Preferences stores the sound-library
getter's returned byte as `music_level`. The inspected button branches contain
no direct store to that byte. The arrows beside the top bar adjust message delay
(RULE-CONFIG-005), and those beside the second bar adjust effects volume
(RULE-CONFIG-004). This does not exclude changes through callees, copied state
or controls elsewhere [FND-CONFIG-235, FND-CONFIG-029].

## When it runs

When the Preferences screen opens (SCR-UI-007) [FND-CONFIG-235].

## Parameters

None.

## Inputs

`master_sound_on` and `music_on`.

## Procedure

```text
if master_sound_on and music_on:
    music_level = fn_4A6E_011F()
```

## Outputs

`music_level` receives the getter result only when both gates pass; otherwise
this entry path makes no such direct store. A music-toggle redraw passes this byte
when music is enabled, or zero otherwise, to the top bar. Full redraw uses
that position for message delay instead (FND-UI-065). Effects through the
getter and subsequent callees remain unresolved.

In the ten inspected Copy drivers with the query interface, the getter's
driver query returns the selected sequence's current level word, distinct
from its target level. Preferences keeps the low byte. The game's duration-zero
setter request writes both words before downstream update. ADV 11's successful
registration/reset path initializes both words to 100; that conditional write
does not establish the saved setting's new-game default or bypass the getter's
state rejection. Frame/record aliasing and later effects remain unresolved
(FND-CONFIG-237).

## Edge cases

- With either entry gate off, this entry path does not store a getter result.
  A music-toggle redraw with music off uses zero (FND-CONFIG-235, FND-UI-065).
- The driver's answer can be 255 for card ID 113 and for some unavailable drivers
  (FND-CONFIG-236).

## What the sources say

SRC-MANUAL-1994, page 15: music volume is a slide bar the player can adjust to control music
volume by clicking the buttons on either end of the bar; sound effects volume is a slide bar that
works the same way. The first half does not hold for this build: the top arrows are the
message-delay buttons (FND-CONFIG-235).

## Differences between builds

None known.

## Open questions

- Q-CONFIG-014: FND-SOUND-087 bounds quiet restart with empty note slots
  and inactive signed cleanup flags. Active channel remapping can select
  other registered blocks; retain actual count/slot, mapping and backing
  admission and descendant effects, rather than treating only the caller's
  block as the cleanup write domain. The local selection and scan semantics
  no longer require another reading.
- Q-CONFIG-014: FND-SOUND-086 bounds ADV 11's sentinel/fresh-start effects
  with stable admitted slot/sequence/frame bindings and disjoint storage.
  Fresh start resets current/target level to 100 and sets state one; later
  setters prevent inferring an actual Preferences value from that write alone.
  Retain actual state admission, already-playing cleanup effects and other
  driver targets. The local fresh-start/reset bundle is no longer unread.
- Q-CONFIG-014: FND-SOUND-085 classifies the inventoried literal handle
  writers and initialized source values. The known start publication reloads
  its source after the child without testing success; failed acquisition does
  not clear that source. Establish actual initial/prior handle histories and
  intervening child effects, nonliteral/block/alias writes and segment/storage
  admission. Initial FFFF plus unchanged source yields the getter's sentinel
  exit; this conditional case does not establish a universal runtime default.
- Q-CONFIG-014: FND-SOUND-084 classifies both alternate known direct setup
  calls. They reuse the shared payload, explicitly store a different handle
  from the getter's, and cannot reach setup with unchanged configuration
  through their normal gates. Changed bindings, entry after those gates,
  child effects and indirect/copied propagation remain unresolved; those
  local argument/result contracts no longer require another reading.
- Which admitted driver/sequence bindings and writer histories reach `fn_4A6E_011F`,
  including effects through the button paths' indirect or copied writes.
  The direct-store census does not exclude these mechanisms.
  The local Copy-driver query fields and ADV 11 reset stores are already read;
  retain actual registration/payload/storage admission, later field writers,
  changed or missing function tables and reached update effects. The selected
  setup path's patch loop requires cache-membership progress: ADV 11 rescans
  missing keys and its install request can retry unchanged. FND-SOUND-083
  excludes a matching payload in the validated installed set for the four
  ordinary song-request arms. Preserve other media, runtime records and
  bypass/independent setup entries. TIMB, cache and arena admission remain
  required for paths that actually supply setup data
  (FND-CONFIG-236, FND-CONFIG-237, FND-CONFIG-029, FND-CONFIG-235,
  FND-SOUND-082, FND-SOUND-083, Q-CONFIG-014).
  - Dependency ownership: CONFIG owns this readback contract and resident
    getter/setter rows `4A6E:011F` and `4A6E:0064`; UI shares the Preferences
    entry/dispatcher rows `0x0008B240` and `0x0008B32E`. SOUND/RNG share
    lookup/dispatch rows `4842:03BC` and `4842:03F4` and sequence lifecycle.
    These six rows are present in the installed DSUN.EXE inventory; ownership
    here does not declare a complete reading or reassign the separate JSON plans.
  - Q-CONFIG-014 retains caller/frame admission, sequence publication/reset
    success and later current/target writers, and actual indirect slot/table
    targets. The thirty native driver entries read in FND-CONFIG-237 have no
    committed function inventory for the embedded ADV code; do not count them
    as inventoried DSUN functions. Shared helpers and other unassigned functions
    remain visible in the independent closure assessments.
  - Native frames remain Q-CONFIG-001. Audible output is not established by
    the stored-level readback; retain Q-CONFIG-002 for that separate question
    without expanding this rule into the whole playback subsystem.
- Whether another control changes saved or audible music level remains a
  separate discovery/output question (FND-CONFIG-029, FND-CONFIG-235,
  Q-CONFIG-002). Any unresolved effect on readback storage also remains
  Q-CONFIG-014; separation alone excludes no writer or indirect mechanism.
