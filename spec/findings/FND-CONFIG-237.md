---
id: FND-CONFIG-237
title: Copy-driver level queries read current sequence state and the immediate setter writes current and target before update
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0006DC03..0x0006DC29
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0006DDBE..0x0006DDE4
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0006DE58..0x0006DED5
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000705D6..0x000705FC
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00070791..0x000707B7
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007082B..0x000708A8
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00075269..0x0007528F
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00075424..0x0007544A
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000754BE..0x0007553B
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007735B..0x00077381
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00077516..0x0007753C
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000775B0..0x0007762D
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007BF7F..0x0007BFA5
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007C13A..0x0007C160
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0007C1D4..0x0007C251
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00080E54..0x00080E7A
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008100F..0x00081035
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000810A9..0x00081126
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000858D9..0x000858FF
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00085A94..0x00085ABA
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00085B2E..0x00085BAB
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008985A..0x00089880
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00089A15..0x00089A3B
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00089AAF..0x00089B2C
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008D920..0x0008D946
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008DADB..0x0008DB01
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0008DB75..0x0008DBF2
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00090FEE..0x00091014
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000911A7..0x000911CD
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00091241..0x000912BE
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00085631..0x00085777
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00084952..0x000849FC
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00084C68..0x00084CC7
tool: installed_adv_resources.py and adv_function_stores.py at 32ed354b; Ghidra 12.1.3 BinaryLoader, RecoverCitedFunctions, ReportInstructionWindow, ReportDataBytes and ReportDecompileWindow; reused FND-CONFIG-236, FND-EXE-646, FND-EXE-594 and FND-RNG-082
environment: null
---

## Observation

Installed RESOURCE.GFF is 5724669 bytes, XXH3-128
92fe031616f36062f99484b0f728210e. Its directory reader independently accounts
for seventeen ADV resources and compares them with the manifested disc files.
ADV 11 is 16357 bytes at file 0x00081DDF, XXH3-128
0faa31676d7d993a393da11bde9e7885, equal to CD:SBP2FM.ADV.
This source is the configured music image identified by FND-RNG-082, not
proof that every live driver slot still names it.

The source-bound function-table review finds the following entries. Numbers
AE, AF and B1 and entry offsets below are hexadecimal; resource IDs are decimal.
Each query occupies 38 contiguous source bytes, including its overlapping
flag-restore entry. Each listed setter's local body occupies 125 bytes.
Reading the tables in source order finds exactly one occurrence of each of
the three selectors in each of the ten resources: thirty single-occurrence
controls, so first-match lookup and the candidate reader's dictionary agree
for this set. This does not exclude runtime table changes.

| ADV resource | Status query AE | Current-level query AF | Target setter B1 |
|---|---|---|---|
| 1 | `34CC` | `3687` | `3721` |
| 2 | `24E8` | `26A3` | `273D` |
| 4 | `369F` | `385A` | `38F4` |
| 5 | `1C07` | `1DC2` | `1E5C` |
| 7 | `34FE` | `36B9` | `3753` |
| 9 | `3698` | `3853` | `38ED` |
| 11 | `3AFA` | `3CB5` | `3D4F` |
| 12 | `3A96` | `3C51` | `3CEB` |
| 17 | `2551` | `270C` | `27A6` |
| 19 | `1D6F` | `1F28` | `1FC2` |

Resources 3, 6, 8, 13, 18 and 20 have none of these table entries. ADV 10
has no terminating table in this Copy-table reader and is excluded, not
classified as absent. Reuse FND-EXE-646 for the distinct DIGPAK adapter.
Runtime table replacement and other entry mechanisms are not excluded.

### Query result and frame contract

In all ten listed drivers, the two query procedures save their caller's
DS, SI, DI, BP and FLAGS, then disable maskable interrupts. They read the
sequence handle word through SS:[BP+8], after establishing a far-call frame;
the preceding argument at BP+6 is the driver slot that dispatch has already
used. A handle equal to FFFF returns AX=FFFF without dereferencing a sequence.
Other values are used directly as a byte displacement into a CS-resident
far-pointer array. No local alignment, bounds, null-segment or sequence-state
check protects that dereference. This is not an admission of arbitrary handles.

The selected far pointer becomes DS:SI. AE returns word [SI+1A]; AF returns
word [SI+24]. AF does not read target word +26 or measure audible output.
Neither query makes a non-stack memory store on its decoded paths. The
pointer-array base differs by resource; in ADV 11 it is CS:293B. Reuse
FND-RNG-082's producer and ownership interface rather than treating caller
DS and the sequence segment as identical.

ADV 11's AE instructions are 3AFA..3B20, and AF's are 3CB5..3CDB.
Both include a separately entered one-byte IRET inside an otherwise executed
instruction: 3B15 and 3CD0. The preceding saved FLAGS, pushed CS and near
call supply the IRET's FLAGS, CS and return offset; the restored path then
pops saved registers and far-returns. FND-EXE-594 records the same mechanism
in resident sound code. Source-byte controls and instruction windows verify
this in the ten queries. Ghidra's one-decode listing and inferred decompilation
leave the interior call as an offcut; those inferred signatures do not prove
the frame or flag behavior. No original function was executed.

### Caller, setter and initializer

FND-CONFIG-236 already connects AE and AF dispatch to the game's getter,
including its zero/two status rejection and byte store by Preferences.
The driver results identify the values tested: a sequence-state word at +1A,
then current-level word +24. They do not establish the current pointer,
state or result in a particular game sequence. AE=FFFF is not one of the
zero/two rejection values; the handle can also be reloaded between calls.
Thus a local status result cannot certify all later binding and storage.

The B1 bodies use that same handle representation. A non-FFFF handle loads
the sequence pointer and writes the supplied level word to target +26.
When the duration word at SS:[BP+0C] is zero, they also write the level to
current +24 before requesting a downstream update. The nonzero-duration
arm compares target and current and updates interpolation state where they
differ; it is not an immediate assignment to current. No local level clamp
is present in these B1 bodies. This does not rule out later child writes.

The game's setter pushes zero for duration and the already clamped level
(FND-CONFIG-236), selecting the immediate arm under an unchanged, valid
binding and frame. The duration test follows the target store; retaining
that zero therefore requires its frame bytes not to alias the record write
or change before the test. In ADV 11, writes are at 3D67 (+26) and 3DB4 (+24),
followed by the update request at 3DBA. Their effects precede that child;
there is no local rollback based on a returned success flag. ADV 11's update helper
2E89..2EE8 has a sixteen-channel loop, reads current +24, publishes per-channel
values through CS and calls 2635 for enabled, nonempty channels. Effects
beyond that child and interval-wide state preservation remain unestablished;
the generic candidate walk does not prove absence of other effects.
This finding does not prove physical playback, unrestricted preservation or
that the child returns in every admitted state.

In ADV 11, registration 3852..3998 checks eight four-byte pointer slots;
the selected displacement is retained as the handle. On the successful
publication/setup branch it calls reset 2B73..2C1D. That reset loads the
selected pointer and writes 100 to both current +24 and target +26 at
2BD2 and 2BD7. Registration first stores zero to state +1A at 396E;
the default level does not bypass the game's zero-state getter rejection.
The reset's four-count and descending byte loops are locally bounded,
and it has no child call. These are conditional initialization writes, not
new-game defaults for the saved PREF byte. Registration still depends on
its input payload, output storage and earlier child; those are not settled
by finding the reset stores.

## Interpretation

This closes the local uncertainty about which words these known driver queries
return and distinguishes target from current level. The selected immediate
setter and ADV 11 initializer supply connected writers for those fields.
The readback is code-decided sequence state, not a hardware loudness measurement.
The conditional values and ordering do not establish the whole rule or any
universal absence of music-volume controls.

## Alternatives

- Actual configuration, driver/sequence slot admission, registration success,
  storage identity and lifetime, earlier/later writers, table mutation and
  reachable callback effects remain Q-CONFIG-002. Query instructions without
  non-stack stores do not prove their stack cannot alias another object.
- ADV 10 and arbitrary replacement tables are outside this Copy-table reading.
  Missing functions in the six other resources say nothing about dynamically
  changed tables or unrelated driver interfaces.
- A setter's duration-zero assignment does not establish audible response or
  successful downstream return. The broader recursive candidate reports retain
  other writes, segment changes and transfers; they are not negative proofs.
- No complete_reading, whole queue closure, status promotion or actual new-game
  level follows. Neither query performs a local range clamp on its returned word.

## How to reproduce

Use the pinned evidence Python. Run installed_adv_resources.py with GAME_DIR
and spec/builds/BLD-GOG-EN-1.1.files.yaml; require the manifest identities and
all seventeen resource dimensions. Then run adv_function_stores.py with the
installed RESOURCE.GFF and hexadecimal selectors AE AF B1. The latter is a
candidate classifier, not an identity guard; verify the archive identity first.
Check selector multiplicity in source order: read the little-endian word at
resource offset zero as the table offset, then four-byte selector/entry pairs
up to selector FFFF within that resource's bounds. Require exactly one pair
for each selected selector and agreement with the candidate entry. The
retained table-order-check.json records the thirty metadata controls.

Retain outputs under GAME_DIR/analysis/music-driver-contract-20261107/.
Extract only each listed resource's reported offset/size into that local
folder; ADV 11, for example, is archive bytes 0x81DDF..0x85DC4. Verify its
resource fingerprint above. Import each slice with BinaryLoader at base zero,
processor "x86:LE:16:Real Mode", no automatic analysis. Supply its three table
entries as eight-digit offsets to RecoverCitedFunctions.java. Require three
recovered entries, zero failures and zero outside-executable-memory entries.
ReportInstructionWindow.java uses 22 instructions per query and 65 per setter;
its output can continue across a gap, so use the spans ending at the far return,
not the requested instruction count as a body boundary. ReportDataBytes.java
uses 38 bytes per query to check the interior IRET stream.

For ADV 11, decompile entries 3AFA, 3CB5 and 3D4F with windows 1..80,
1..80 and 1..120. Inspect source controls at 3B14, 3CCF and 3DC0, seven
bytes each. Recover 3852 and 2B73 in a read-only follow-up; decompile with
windows 1..120 and 1..100, and use instruction windows to verify the
publication, handle, reset stores and loop bounds. The initialization report
also contains the already decoded update helper; its own span is 2E89..2EE8.
Retained per-driver reading logs, ADV11-registration.log,
ADV11-level-initialization.log and ADV11-overlap-and-update.log record the
outputs. The initial ADV11-reading.log rejected an incorrectly spelled
language ID; ADV11-reading-corrected.log is the successful import. No canonical
DSUN project was changed and no game or driver code was executed.
