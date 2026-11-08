---
id: RULE-CONFIG-003
title: Music-level readback on the Preferences screen
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-235, FND-CONFIG-010, FND-CONFIG-236, FND-CONFIG-237, FND-CONFIG-238, FND-CONFIG-239, FND-CONFIG-240, FND-CONFIG-241, FND-CONFIG-242, FND-CONFIG-243, FND-CONFIG-244, FND-CONFIG-245, FND-CONFIG-246, FND-CONFIG-149, FND-CONFIG-155, FND-CONFIG-159, FND-PARTY-320, FND-PARTY-321, FND-PARTY-322, FND-PARTY-323, FND-PARTY-324, FND-PARTY-325, FND-PARTY-326, FND-PARTY-327, FND-SOUND-084, FND-SOUND-085, FND-SOUND-086, FND-SOUND-087, FND-SOUND-088, FND-SOUND-089, FND-SOUND-090, FND-SOUND-091, FND-SOUND-092, FND-SOUND-093, FND-SOUND-094, FND-SOUND-095, FND-SOUND-096, FND-CONFIG-029, FND-UI-034, FND-UI-065, SRC-MANUAL-1994]
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
- Two strict restores of the fixed play-ready route observed entry store 255
  over a previously held 100, with card ID 122, mode word 4 and getter handle
  FFFF at the paired snapshots. This is not a bounded 0..100 setting or a
  new-game default; later display and changes were not observed (FND-CONFIG-238).
- A reached status query on the unchanged, admitted DIGPAK adapter table
  returns zero and selects getter result 255 before a current-level query.
  Installed ADV 10 takes that distinct interface on the normal installer route;
  it needs no Copy query table. Actual runtime bindings remain open (FND-CONFIG-239).

## What the sources say

SRC-MANUAL-1994, page 15: music volume is a slide bar the player can adjust to control music
volume by clicking the buttons on either end of the bar; sound effects volume is a slide bar that
works the same way. The first half does not hold for this build: the top arrows are the
message-delay buttons (FND-CONFIG-235).

## Differences between builds

None known.

## Open questions

- Q-CONFIG-014: FND-CONFIG-246 supplies the fixed fresh program-entry-to-
  first-music-gate value history and positive word/pointer mutations. Retain
  the later seed/opener/map-to-Preferences interval as one unobserved history;
  CONFIG-245's separate restored interval does not bridge it. Other paths,
  unmonitored data, unchanged/intra-instruction effects, actual storage and
  the remaining entry/reader/writer mechanisms stay open.

- Q-CONFIG-014: FND-CONFIG-245 supplies a repeated fixed watch interval
  from strict play-ready restore through the paired byte store. Its positive
  music-byte change and named-value reports narrow that restored route only.
  Retain history outside that restored interval, other callers/non-sentinel paths, later and
  unmonitored state, same-value/intra-instruction effects and the existing
  alias, writer and reentrant-entry obligations. Untriggered watches are not
  independent mutation-delivery controls for every width.

- Q-CONFIG-014: FND-CONFIG-244 resolves the named cross-row pair for
  the admitted ordinary descriptor-walk/fixup consumers. Retain other or
  computed/copied readers, transformed/changed fields, different bindings
  and alternate entries, plus actual caller/reentrant and writer histories.
  A numerical pair is not an established additional opener invocation.

- Q-CONFIG-014: FND-CONFIG-243 connects both ordinary opener calls to
  their menu/button or keyboard gates, post-child key reload and readback
  gate/store order. Retain actual parent/event/frame and child-state histories,
  exit-time reentrancy and other entry mechanisms. Its additional numerical
  pair spans descriptor fields at file 4B6D2/4B6D8; actual combination or
  transformation as an incoming pointer remains unestablished. Those fields
  are not the already-excluded PARTY-323 shutdown pair.

- Q-CONFIG-014: FND-CONFIG-242 audits the modeled installed getter calls
  and nearby declared-segment/raw-offset pairs; the known Preferences call
  is their sole target. Retain actual Preferences root/event/frame admission
  and exit-time reentrancy, unqueried jump/indirect/table/return mechanisms,
  dynamic/copied/distant-pair pointers, relocated-offset reuse and changed
  streams. This finite census is not a whole-caller or writer-history proof.

- Q-CONFIG-014: reuse FND-CONFIG-149/155/159, FND-SOUND-092/093
  and FND-PARTY-320/321/322 for the already-read alternate lifecycle
  paths. The sound initializer's exact-one ready gate, teardown's nonzero
  gate and the exit caller's exact-one mode test are distinct. Under the
  recorded intact-zero/frame model, the known registered shutdown route
  has no finite ordinary return through its outer exit call. A returning
  getter needs no blanket preservation proof across that whole route as a
  returning child. Retain reentrant exit-time readers, actual ready/mode
  and callback-table history, changed frames/nonlocal rejoining, other
  incoming mechanisms. FND-PARTY-323 excludes PARTY-322's specific unchanged
  separated alias-word pair: both words are relocated, so the loaded pair
  does not name shutdown. Later mutation, arithmetic or different pairing
  remains outside that exclusion. The ordinary first initializer still needs
  actual child/state admission; the exit result excludes none of its calls.
  This reuse review supplies no complete_reading declaration: none of those
  findings closes the remaining caller, writer/lifetime and indirect-target
  obligations for the whole rule.

- Q-CONFIG-014: reuse FND-PARTY-324/325/326/327 for mode-source and
  modeled alternate-entry classification. The shipped mode and its two known
  switch stores restrict unchanged state to zero or one; they do not prove
  preservation between separate reads. Three raw write-like candidates are
  operands on their established owner streams. The recorded near-pattern and
  declared-segment/raw-offset-pair exclusions retain their exact regions,
  controls and excluded mechanisms. Actual mode history, block/string and
  adjusted-pointer writers, physical aliases, changed or differently entered
  code and reentrant readers remain; these are not unread literal stores or
  an unclassified unchanged shutdown pair.

- Q-CONFIG-014: FND-CONFIG-241 supplies the local whole-array reset and
  ordinary reset/shutdown caller pairs. Reuse shutdown's conditional selector
  68 dispatch and the controlled direct-removal census, retaining its excluded
  mechanisms. Actual backing and writer history, alternate overlay helper
  effects, driver/timer children and scratch/frame preservation remain;
  shutdown's local instructions do not admit table preservation.

- Q-CONFIG-014: FND-CONFIG-240 connects local slot publication, all sixteen
  installed Copy descriptor providers and the two caller-result stores.
  Reuse its publication-before-check ordering, conditional version acceptance
  and distinct game/library teardown state. Retain actual slot/table/frame
  and ADV 20 environment admission, reset/removal callers and reached cleanup
  effects, relevant writer/lifetime histories and sequence state. These local
  contracts do not qualify the whole entry for complete_reading.

- Whole-entry review FND-CONFIG-239 keeps RULE-CONFIG-003 supported, with
  no qualifying complete-reading declaration. Q-CONFIG-014 still holds every
  caller, relevant writer/lifetime and indirect-binding condition not settled
  by the local readings or FND-CONFIG-238's one native route. The installed
  source-interface partition, including ADV 10's adapter, is now read; repeating
  its Copy-reader rejection is not a remaining query-interface obligation.

- Q-CONFIG-014: FND-SOUND-094 resolves the known configuration callback's
  ordinary return-result propagation. Its zero failure byte can be a nonzero
  word, so continuation is not evidence of readiness. Retain actual target,
  table/backing, nonreturn and state-write admission; the known result-width
  fork is no longer unread.
- Q-CONFIG-014: FND-SOUND-093 separates no-release retained-buffer cases
  from mixed cleanup and reads the size-only pre-setup producer. Retain
  actual pointer/reuse-byte histories and reached release effects; a helper
  bypassed by the admitted path is not its gate-preservation dependency.
- Q-CONFIG-014: FND-SOUND-092 reads the installed profile's local
  driver-enable override and installation/block-allocation bypass gates.
  Retain actual loaded configuration, caller/ready-byte admission, intervening
  child effects and repeat-initialization histories. A configured driver
  name does not prove installation, and driver absence alone does not settle
  the getter's separate handle or the saved byte.
- Q-CONFIG-014: FND-SOUND-091 reads the known callback binding and its
  selected ADV 11 branch child. Retain actual registration/slot/target/DS
  histories, other targets and branch-resource count/backing/offset admission;
  the known target is no longer an unread arbitrary function. The older
  no-driver runtime profile does not prove loaded-driver admission.
- Q-CONFIG-014: FND-SOUND-090 reads initialization's reached count/slot
  reset prefix, including implicit stores outside the direct writer list.
  Retain actual entry timing, later initialization effects and other
  computed/block/alias or callback writers. The controlled literal census
  retains candidates and unresolved transfers; it proves no global absence.
- Q-CONFIG-014: FND-SOUND-089 reads the known count-lifecycle transitions
  and conditional preservation relation. Retain actual completed/serialized
  histories, nonzero block segments, valid slot handles, selected event and
  cleanup admission, and other writers. Pending release alone proves neither
  prompt removal nor a later getter result.
- Q-CONFIG-014: FND-SOUND-088 completes the finite active voice-effect
  bundle through its fixed-count output tail. The remaining conditions are
  actual voice/channel/assignment indices, valid driver-data write domains,
  code/frame/handle separation and relevant external effects; these helpers
  are no longer unread. Port output alone proves no audible outcome.
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
    SOUND also owns installer row `4842:0A23`, shared here as the table-producer
    dependency in FND-CONFIG-239. Its lookup consumers are the two shared rows
    above. These seven known nodes are a partial map, not the whole reached graph.
  - Q-CONFIG-014 retains caller/frame admission, sequence publication/reset
    success and later current/target writers, and actual indirect slot/table
    targets. The thirty native driver entries read in FND-CONFIG-237 have no
    committed function inventory for the embedded ADV code; do not count them
    as inventoried DSUN functions. Shared helpers and other unassigned functions
    remain visible in the independent closure assessments.
    ADV 10 has no committed native-code inventory either. Its ordinary query
    adapter classification does not remove its other code or native writer/
    callback effects from the remaining obligations (FND-CONFIG-239).
  - FND-SOUND-095 reads the configuration callback's local metadata/table
    write contract. Q-CONFIG-014 retains actual response freshness, stable
    first/last bytes, current table capacity and nonwrapping base, request/
    allocation effects and disjointness from readback state. The final table
    write has no local null guard even after no iterations or failed acquisition;
    a reused table has no local size revalidation. Those local branches no
    longer require another reading; native extent/alias admission remains unresolved.
  - FND-SOUND-096 supplies two matching native observations through the first
    music gate, including actual installed configuration, known disc callback,
    metadata, allocation and final table-write admission. Q-CONFIG-014 retains
    unobserved writer/alias/extent conditions and the later startup-to-Preferences
    getter/store path. Equal captured values establish no continuous preservation.
  - FND-CONFIG-238 supplies actual caller/frame, getter return and entry-byte
    store on the fixed restored-map route. Q-CONFIG-014 retains other caller/
    gate and non-sentinel driver admission, later or aliased writes, and the
    uninterrupted startup history. Do not repeat this native pair as if unread.
  - Native frames remain Q-CONFIG-001. Audible output is not established by
    the stored-level readback; retain Q-CONFIG-002 for that separate question
    without expanding this rule into the whole playback subsystem.
- Whether another control changes saved or audible music level remains a
  separate discovery/output question (FND-CONFIG-029, FND-CONFIG-235,
  Q-CONFIG-002). Any unresolved effect on readback storage also remains
  Q-CONFIG-014; separation alone excludes no writer or indirect mechanism.
