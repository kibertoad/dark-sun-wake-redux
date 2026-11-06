---
id: FND-SOUND-019
title: Corrected disc ADV envelope and inline loader-name binding
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CD:ADLIB.ADV
    offset: 0x00000000..0x000000C7
  - build: BLD-GOG-EN-1.1
    file: CD:ALGDIG.ADV
    offset: 0x00000000..0x0000007B
  - build: BLD-GOG-EN-1.1
    file: CD:SBPDIG.ADV
    offset: 0x00000000..0x00000073
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 13D0:00FB..13D0:0427
tool: tools/research/exec-census/disc_adv_envelopes.py at 82c2e588 and resident_listing.py at 05d904c6; Python 3.12.10; xxhash 4.0.1; existing disc_adv_exec_census ISO collection
environment: null
---

## Observation

This supersedes FND-SOUND-018: its directory observations hold, but its
loader summary wrongly spelled hexadecimal offsets as decimal and called
the inline name fields pointers. The instructions forming the two actual
name arguments settle those mistakes; no driver-envelope result changes.

FND-EXE-491/492 identify the setup loader's accepted Copy signature and
function-directory pointer. FND-EXE-496 already uses each directory to
start its bounded code census. This checks the complete directory envelopes
against all nineteen manifest files and brings that common structure into
FMT-SOUND-004, without claiming that a directory describes every code entry.

The read-only inspector collected the nineteen root ADV files from the
licensed MODE2/2352 disc image. Their individual lengths and XXH3-128
digests matched the committed CD: manifest entries. They total 227209 bytes.
No driver was installed, called or executed; the reused collector's
unrelated interrupt-census text is suppressed and supplies no evidence
for the new envelope checks.

### Common envelope and exact directory extent

Every file begins with a little-endian word equal to 45, pointing to its
directory at file offset 2D hex. Bytes two through five are Copy. Bytes
six through 44 are outside this described signature/pointer contract and
remain opaque. At the directory, each nonterminal entry has a two-byte
function number followed by a two-byte target offset. A function number
FFFF ends the directory; no target word after that number is required.
FND-EXE-491/646's lookup tests the number and stops at that sentinel before
using an entry's target.

| Entries per directory | Files | Sentinel offset, hex | Directory end, hex |
| --- | ---: | --- | --- |
| 38 | 13 | C5 | C7 |
| 17 | 5 | 71 | 73 |
| 19 | 1 | 79 | 7B |

There are 598 nonterminal entries in total. ALGDIG.ADV is the nineteen-entry
file. ARIADIG, GF1DIGI, PASDIG, SBDIG and SBPDIG are the seventeen-entry
files. The remaining thirteen have 38 entries, agreeing with FND-EXE-496.
Within every file all function numbers are distinct and all target offsets
are distinct, at or beyond that file's directory end and below its length.
This checks offsets as file locations, not instruction identity, reachability
or the complete native segment mapping; FND-EXE-498 retains SBAWE32's
separate code-segment issue. The inspector would accept aliased targets,
but none occurs in this set.

### Loader evidence already recorded

FND-EXE-491 reads the setup utility's two install callers: a driver name
is formed at inline offset 0x16 or 0x24 of its current settings record,
the named file is read, and the signature/table installer receives it.
FND-EXE-492 and FND-CONFIG-213 record that the shipped setup batch copies
the disc ADV files into the working directory before running that utility.
They do not establish the complete parser/selection path that populates
the selected names and the current settings-record pointer. Fresh complete
local listings of 13D0:00FB..02B3 and 13D0:02B3..0427 show the first
name formed by adding 0x16 to the low word of DS:E017 at +0145 and the
second by adding 0x24 at +02C9; both retain the segment word DS:E019.
Neither site loads a far pointer from a name field. The current record
pointer producer and preservation remain separate prerequisites.

The game-side image is requested as an ADV resource rather than a loose
file in FND-EXE-646. FND-CONFIG-023/024 bind the resource selectors to
SOUND.CFG words +38 and +36. FND-EXE-590 compares the resources byte for
byte: the installed configuration selects ADV 11, equal to SBP2FM.ADV,
and ADV 8, equal to SBPDIG.ADV. Other resource/configuration choices and
setup selections are not inferred from these two shipped matches.

## Interpretation

The common on-disc envelope is supported for all nineteen files. Q-SOUND-007
no longer asks what their shared pointer/signature/directory structure is;
it retains the complete actual setup record/name selection and remaining
loader admission. Successful resource acquisition, driver initialization,
device behavior, all driver function semantics and complete code mapping
do not follow from a well-formed directory. No complete-reading or parity
validation promotion is made.

## Alternatives

Treating Copy as a DOS MZ header would skip the first-word directory
contract. Requiring a target word after FFFF reads beyond the sentinel
the lookup tests. Treating the nineteen-entry count as a format-wide bound
confuses one observed directory with the contract; the inspector's 256-entry
cap is also only a tool limit. Treating the default two game resources as
every possible setup selection would omit the current-record producers.

## How to reproduce

Run twice with the locked evidence Python:

```text
python -I tools/research/exec-census/disc_adv_envelopes.py <licensed-install-dir> spec/builds/BLD-GOG-EN-1.1.files.yaml
```

The tool reuses the existing ISO collector, requires precisely the nineteen
manifest ADV identities, caps each input at 65536 bytes and its directory
at 256 entries, checks pair/target/number bounds and emits metadata only.
The complete reports are adv-envelopes.json and adv-envelopes-repeat.json
under GAME_DIR/analysis/disc-adv-survey-20261104/. They compare byte for byte,
SHA-256 3c93cf0325c9ab635e824d42548bf60ebe7591a6e3a3627c5efd9f0b551770f6.
Reconcile all file counts and target spans; reuse the named loader findings
instead of treating a directory scan as a fresh loader reading. Original
driver bytes and code remain outside Git.

The corrected inline-name binding is reproduced with resident_listing.py
for 13D0:00FB..13D0:0427 in the approved SOUND_DS.EXE, length 204593,
XXH3-128 236c2dc23c071eca421eb5b427caee57. setup-name-consumers.txt in
the same private directory retains that complete local listing. Check
the two low-word additions and unchanged source segment before each
call of 13D0:0427; keep current-pointer lifetime separate.
