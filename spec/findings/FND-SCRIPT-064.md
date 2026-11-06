---
id: FND-SCRIPT-064
title: Short selected trigger resources and short I/O returns leave different restore flags but can both supply mixed records to reverse conversion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070964
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:09D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:01AF..44DE:022C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071659..0x00071869
tool: connected existing-evidence assessment at c0f79581; FND-COMBAT-035/082/111/151, FND-CONFIG-151/152, FND-SAVE-015/021 and FMT-SCRIPT-004; integer prefix/record-boundary calculation
environment: null
---

## Observation

This assesses SAVE/38's short-transfer outcomes for Q-SCRIPT-025. It
uses existing direct readings of the numbered reader, complete-resource
service, transfer helper and converter rather than reopening each helper.
No changed save file or native failure is executed or asserted.

### Selected length and completed count are different inputs

FND-SAVE-015 gives the writer's nominal SAVE/38 length as 2600, for
200 thirteen-byte records. FND-COMBAT-035 and FND-CONFIG-151 establish
that restoration supplies a nonnull destination through a local in/out
pointer and requests the selected resource's complete recorded length.
The caller does not supply 2600 as a capacity or required-length argument.
The service requires a signed-positive selected length, with the other
record/archive/positioning conditions retained in those findings.

Let L be a selected length from 1 through 2600. Its high word is zero
and its low word below 32768, so FND-CONFIG-152/FND-COMBAT-082's read
helper branch issues one remaining DOS read after pointer normalization.
With a carry-clear actual count K, it returns that count without retrying
or filling an unread tail. The resource core compares K with L, not with
the writer's nominal length. Under the recorded caller/storage conditions:

| Local outcome | Resource core | Numbered-reader continuation |
| --- | --- | --- |
| L = 2600, K = L | AX zero | Continue to later resources |
| 0 < L < 2600, K = L | AX zero | Continue despite a prefix-sized resource |
| 0 <= K < L | Error 21, AX FFFF | Clear transfer-success byte; stop later resources |
| Carry-set DOS return | Error return, then core mismatch | Stop later resources; produced bytes are not inferred from the error |

The table assumes earlier selection/positioning gates admit the read.
Zero selected length is rejected before that transfer; larger lengths
are outside this finite assessment and are not bounded to 2600 by the
restore caller. Successful shorter source data and a short I/O return
are therefore different cases, even when both leave an unreplaced tail.

### A prefix can leave mixed words and links

Assume the external read supplies a contiguous prefix of K bytes to the
existing trigger buffer, with no independent clearing, aliases or later
interference. This prefix premise is explicit: an error status alone
does not identify the bytes actually written. For 0 <= K <= 2600, let
j = floor(K/13) and r = K modulo 13. Records below j are fully replaced.
For j below 200, record j receives r prefix bytes and later bytes retain
their preceding values. Records after j retain their bytes.

The thirteen-byte layout then gives these partial-record cases:

| Remainder r | Entry/script words in the partial record | Link at bytes 11/12 |
| --- | --- | --- |
| 0 | Both preceding words retained | Retained |
| 1 | New entry low byte, old entry high byte; old script word | Retained |
| 2 | New entry word; old script word | Retained |
| 3 | New entry word; new script low byte and old high byte | Retained |
| 4 through 11 | Both words replaced | Retained |
| 12 | Both words replaced | New link low byte, old link high byte |

At K=2600 there is no partial record. At smaller K, no local truncation
of the later converter's record count follows from either L or K.
The record's keys and other fields can likewise mix new and retained
bytes; a complete entry/script pair does not make the whole record new.

### Later head transfers and conversion remain distinct

SAVE/51 supplies the free-head word and SAVE/52 through 61 supply the
ten trigger-list heads (FND-SAVE-015). These resource requests follow
SAVE/38 in the selector-zero loop. A failure at38 prevents their later
requests in that invocation. A successful short selected SAVE/38 does
not prevent them; they can be attempted if the intervening requests
continue. Their actual selected lengths, results and aliases remain
conditions, not implicit successful head restoration.

FND-COMBAT-111 establishes the later reverse-converter call after either
numbered-loop outcome, subject to the actual intervening continuations.
FND-COMBAT-151 establishes that its trigger pass visits all 200 records,
gates on each current nonzero script word and matches the current entry
word against GPLI. It does not use L, K, the transfer-success byte or
list membership as its trigger-pass bound. A retained or mixed pair can
therefore select, skip or miss a remap under those conditions. FND-SCRIPT-062
supplies concrete retained-pair remaps with the shipped table.

## Interpretation

Q-SCRIPT-025 can distinguish successful short-resource restoration from
short-I/O failure and can reuse the exact prefix/record postconditions.
A returned success does not prove all200 records were replaced, and
a failed transfer does not prove none were changed. Later list-head
requests can differ between these two cases independently of the
converter's fixed traversal.

Actual selected lengths and returned bytes, destination/live-buffer
identity, optional placed-buffer fill, capacity, aliases and preservation
through intervening children remain unresolved. No partial-record state,
native failed load or player-visible defect is claimed observed.
No queue closure or complete-reading declaration follows.

## Alternatives

Comparing K with2600 would invent a check absent from this reader.
Treating a complete short resource as an I/O error confuses selected
length with completed count. Applying a prefix model to an unspecified
carry-set outcome would invent bytes not supplied by evidence. Bounding
conversion by the prefix or by linked-list heads contradicts its fixed pass.

## How to reproduce

Reuse the complete-resource and numbered-loop readings cited above,
following the selected-count sentinel, nonnull destination, returned-count
comparison, failure flag and resource order separately. Use FND-SAVE-015
for nominal lengths and head destinations and FND-COMBAT-151 for the
converter's independent bound and script-word gate.

For each K from zero through2600, partition the half-open prefix [0,K)
into thirteen-byte records; compare its overlap with entry [0,2), script
[2,4) and link [11,13) within a record. K values0,1,2,3,4,11,12,13 and2600
provide the boundary controls in the table. These are calculations from
the recorded layout and read interface, not an original-code execution
or a new parser, fixture or runtime experiment.
