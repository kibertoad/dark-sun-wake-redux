---
id: FND-SOUND-092
title: Installed sound flags clear the local music-enable gate before driver installation and sequence allocation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND.CFG
    offset: 0x00000014..0x00000016
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:00B0..4734:0190
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:0293..4734:03D0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0005043E..0x00050446
tool: resident_listing.py, window_operands.py and direct_callers.py at e3c6e842; tools/evidence/xxh3.mjs; pinned evidence Python and Capstone 5.0.9; Ghidra 12.1.3 focused decompiler/data reports; reused FND-CONFIG-257/020/021/155, FND-PARTY-320, FND-SOUND-011/016/082 and FND-RNG-084
environment: null
---

## Observation

Q-CONFIG-014 retains actual driver/profile admission. Existing findings
already read separate portions of startup; this assessment connects the
installed source flag with its enable setter, driver gates and allocation
bypass rather than extending driver internals. Addresses are hexadecimal
unless a size/count is explicitly decimal.

The current installed SOUND.CFG is 59 bytes and has XXH3-128
64d376205212cb8a6b8a782b88576327, exactly FND-CONFIG-257's source identity.
Its word +14 is decimal 11, so bits 08, 02 and 01 are set. Identity
agreement permits reuse of that field reading; it does not prove the loaded
buffer remains unchanged. DSUN.EXE is the same approved 634416-byte image
used by the resident reader. Source-initialized words DS:343E, 3440 and
3442 are FFFF, while word 3444 is one, with data segment 57E0.

### Earlier startup choice is subject to the profile override

FND-SOUND-011/064 locate the alternate overlay-180 startup calls to
4734:0128: DS:14E3 selects argument zero or one. The setter's entire
body loads SS:[BP+6] and stores it to DS:3444, then returns far.

Inside library initializer 4734:0133, the test at 0153 reads bit 08 of
the configuration reached through DS:3411. If set, the local call at
015E invokes that same setter with zero. Thus an admitted unchanged
installed configuration overrides either earlier startup choice to zero;
the file's music-driver name and chunk number do not establish that a
driver is installed. The later +32 rewrite in FND-CONFIG-020 is separate
from this +14 test.

### Three later gates select the local no-music-driver route

| Gate | Zero-word continuation |
| --- | --- |
| 0293 tests current DS:3444 | Skips music-resource acquisition 47E5:0074 and joins reset 4842:06CF. |
| 02C7 tests it again after reset | Jumps to 039B, bypassing installer 4842:0A23, returned-slot store DS:343E at 02DE and the subsequent music-driver setup requests. |
| 03C6 tests it after the digital-setup branch and its error test | Jumps to 04D9, bypassing the music requests and sequence/control-block allocation component in FND-SOUND-082. |

These are separate reads across child calls, not one cached decision.
Nonzero at a later read selects its corresponding music path even if an
earlier read was zero. Digital setup can also select an earlier error exit;
a zero music word does not prove all initialization succeeds.

FND-PARTY-320 already reads the shared reset and outer ready-byte wrapper.
FND-CONFIG-155 supplies the reset's bounded CS-relative write spans and
saved-state return; its local body does not directly assign DS:3444 or
DS:343E. The intermediate 4734:083B body initializes a 127-byte DS-relative
table starting at A3A4 and has no child. With the admitted same DS binding,
that explicit range is disjoint from these gate/slot words. Remaining
callback, release, digital-setup and asynchronous effects still need their
actual binding/backing admission before assuming the gate survives all calls.

### Literal writers and callers are finite components

The controlled window 3444..3446 reports six operands in current inventory
ranges: the setter's one write and five reads in initialization/teardown.
Its all-offset candidate pass has zero additional literal matches. The
setter write and the known initializer reads are positive controls.
The direct-call matcher reports the two established overlay-180 calls
at +0091/+00A4 and the initializer's near call at 015E after PUSH CS.
Indirect calls, jumps, changed streams and aliases remain outside that
call census; zero raw literal matches excludes no computed or block writer.

The neighboring 343E..3440 window reports 72 listed operands and retains
27 raw candidates. Its listed direct writes are installer result 02DE
and teardown store zero at 07DA. A teardown does not restore the
source-initialized FFFF value. This assessment therefore distinguishes
first initialization from repeated initialization and retained state.

## Interpretation

For the faithful installed source, stable loaded flag and zero enable
word at the three tested points, the known ordinary initializer does not
install the music driver or allocate its sequence blocks. If its source
slot was FFFF and no intervening writer changes it, it remains FFFF on
that route. This gives a static route consistent with FND-RNG-084's older
observed no-driver profile; it does not generalize those runs to other
configurations or prove the current native history.

Q-CONFIG-014 can remove unread local profile/driver/block-selection gates.
Retain actual loaded configuration, caller/ready-byte admission, gate/slot
preservation across the named children, repeat-initialization history and
other write mechanisms. Prefer that admission work before more conditional
driver internals. The getter's separate handle and sentinel branches remain
FND-CONFIG-236 and FND-SOUND-085; driver absence alone is not a complete
Preferences readback proof.

No complete reading, answered queue deletion, observed device failure or
audible outcome is claimed. No original game or emulated call was run.

## Alternatives

- A configured driver name proves installation: word 3444 can bypass it.
- DS:14E3 alone decides music-driver enable: the later bit-08 setter can
  override its earlier choice.
- The first zero gate fixes the whole route: later tests reload the word.
- Reset or teardown always restores the initial slot sentinel: the known
  teardown store is zero, distinct from source-initialized FFFF.

## How to reproduce

Hash current SOUND.CFG with node tools/evidence/xxh3.mjs and check its
59-byte size against FND-CONFIG-257. Reuse that exact source's field reading.
Run window_operands.py on approved DSUN.EXE and its current inventory for
343E..3440 and 3444..3446; retain every candidate. Run direct_callers.py
for 4734:0128 and verify the three sites against the existing caller streams.

Use resident_listing.py for the entry/setter/profile, driver-gate and
allocation-bypass ranges in the locations and observation. In the existing
read-only resident Ghidra project inspect 4734:0133's full local decompilation
and 57E0:343E, eight bytes, retaining ordinary initialized-data limitations.
Reports under GAME_DIR/analysis/startup-profile-contract-20261107 include
source-config-identity.txt, entry-and-profile.txt, driver-gates.txt,
allocation-bypass.txt, enable-and-slot-census.txt, enable-setter-callers.txt,
startup-profile.log and initial-and-wrapper.log. Reuse the existing reset,
wrapper and allocation findings; inferred Ghidra parameters/symbol names are
not segment or argument evidence. No original-derived code or data is committed.
