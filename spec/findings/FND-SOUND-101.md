---
id: FND-SOUND-101
title: Fresh post-label display and vector samples agree while a shared-handler guard prevents the chooser-use attempt
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/setup_parser.py at d6773d8d and 1540f032; dinorefurb-dosbox-session 1.0.0; Python 3.12.10, xxhash 4.0.1
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output; private two-file C drive
---

## Observation

Q-SOUND-010 retains the actual vector/mode conditions in FND-SOUND-022
and098/099, beyond100's verified post-label storage endpoint. The static
readings supply the fixed named fields and desired chooser-use boundary.
On2026-11-09 one fresh `--chooser-rows` attempt used d6773d8d; two
independent fresh `--labels` attempts used1540f032's added endpoint reads.
All used the same fixed source identities as100, pinned muted session,
private two-file drive, early-error/driver guards and no guest input/writes.
Numbers below are hexadecimal except stated counts and geometry dimensions.

### The use-point attempt stopped at its guard

The chooser-use attempt first passed the parser and post-label storage
comparisons. It then armed the shared190F:0000 handler guard and
continued toward the fixed19CF:009A chooser entry. The next stop was
that guard, at relocated0EC9:0000, with declared DS13F0. The observer
refused the unexpected target instead of reading chooser fields or
continuing. The owned session closed. No notice was dismissed, no input
was sent, and neither chooser entry nor the later025B row-adjustment
endpoint was observed.

This identifies the guarded shared handler, not the particular caller,
caption or action needed to continue it. Its incoming arguments were
not read by this profile. The host-side declared-stop refusal is not
evidence of an original-game crash, failed parse or backend incompatibility.
This attempt remains a retained diagnostic, not successful chooser-use
capability evidence. A supported caller/input/frame contract is needed
before another continuation past this handler.

### Two independent post-label state observations

The subsequent `--labels` runs independently repeated100's successful
record/label/header comparisons and stopped at canonical1ABD:0092,
relocated1077:0092. At that paused stop1540f032 read only the display
window bytes at DS:DEB8..DEBC, mode/geometry/gate fields at DEBE..DEC8,
and the four-byte interrupt-zero pointer. It took each fixed span twice
and rechecked the named label-endpoint bindings. No pointer was followed
into handler or video memory.

Both runs give exactly the same decoded state:

| Field | Observed value |
| --- | --- |
| DEBE mode byte |03 |
| DEBF row extent |25 decimal |
| DEC0 column extent |80 decimal |
| DEC1 transfer classification |00 |
| DEC2 direct-copy synchronization |00 |
| DEC3 base word |0000 |
| DEC5 video-location segment |B800 |
| DEC7 direct-transfer enabling word |0001 |
| DEB8/DEB9 origins |00/00 |
| DEBA/DEBB bounds |4F/18 |
| DEBC attribute byte |07 |
| Interrupt-zero offset/segment |0174/05BA |

05BA is the declared loaded code segment. The observed vector therefore
matches022's requested handler; its initial bytes74,01 are that finding's
nonprintable-prefix condition at this endpoint. This does not establish
the subsequent comparison string's entire storage or persistence until
the chooser uses it.

Under the observed retained fields,098's transfer dispatcher would select
direct copying, and140D's copy loop would choose its no-port-polling branch.
An admitted80-column scroll scratch would need160 bytes. These are
conditional consumer consequences of the sampled values, not execution
of those consumers at this stop. Delay ports, BIOS register preservation,
later backing/frame extents and eventual chooser return remain unverified.

The complete post-label display objects and label-comparison objects match
across the two fresh runs. No whole-session, timing, pixel or sound
equivalence is claimed. Private copies of both fixed inputs were reverified
after all three attempts. All session records report muted host sound,
empty guest writes, null run_failure and null event_log. Both successful
contexts and the guarded context closed; process inspection found no owned
emulator or Python probe. No absent input/RNG trace is reconstructed.

## Interpretation

Q-SOUND-010 now has actual mode, geometry, transfer gates and declared
vector evidence at the verified post-label endpoint. Those values are no
longer merely assumed loaded defaults there. Retention, excluded writers,
far-zero comparison storage and all intervening state until the chooser
remain distinct. The desired use-point check is specifically limited by
the shared-handler guard, whose caller/action and setup input capability
are not admitted. It must not be reported as a successful row-count check.

No complete-reading declaration, format-status promotion, whole queue
closure or parity validation is claimed. Other profiles, transient aliases,
heap/break history and native BIOS effects remain open.

## Alternatives

A matching vector at the label endpoint does not prove it remained installed
through every later call. A video segment value is not a pixel observation.
The guard's presence does not identify a particular visible notice or
authorize arbitrary input. An unobserved chooser stop is not a missing or
terminal process that can be restarted while still live; this attempt
ended only after its owned session closed.

## How to reproduce

The d6773d8d fixed `--chooser-rows` profile and1540f032 fixed `--labels`
profile are planned in ADV-SURVEY; their synthetic controls reject wrong
callers/stops/frame bindings, malformed text, short fields and paused drift,
while retaining unexpected sampled values as evidence. Both use only the
fingerprinted utility/source pair and declared addresses.

Retained attempts under GAME_DIR/captures/rust-dos-setup-parser/ are
v181-setup-rows-20261109-a/ (guarded diagnostic),
v181-setup-display-20261109-a/ and v181-setup-display-20261109-b/.
Their setup-parser.json SHA256 values respectively are:

- dc0fc1cfaacbeccdbce4b4978729b9c4c58d137ba921eab6c695510a5453e384
- 69fb3d894ff400a9164e7f89ac280af2d08fa88f76a3987c73b65911369f8569
- eb96dcf90b289b7be7a88bbaecf70d711bd3168cf43e27b684051b96b6440b2e

The diagnostic has no chooser_entry/chooser_rows result; only the two
endpoint runs have post_label_display. Compare semantic objects rather than
whole-report bytes. Keep all original-derived bytes and diagnostics in the
approved local store.
