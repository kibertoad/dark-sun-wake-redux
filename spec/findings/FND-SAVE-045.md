---
id: FND-SAVE-045
title: Character generation allocates a full-word identifier against four party slots and 39 stored identifiers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FC72..0x0006FCDD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E171..0x0006E17F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006ED7D..0x0006EE5E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E181..0x0006E1A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074AB9..0x00074B2E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065C22..0x00065C52
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2D1C..2D40:2D78
tool: Connected reread with Python 3.12.10/Capstone distribution 5.0.9; retained observations with Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, overlay_listing.py, resident_listing.py, details_chunks.py; scratch scripts reading the CHAR resources of CHARSAVE.GFF and the details chunks of OBJEX.GFF as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies a CHAR record's word at `0x53` to the word at `+0x0E` of the
character's FMT-COMBAT-002 record (FND-PARTY-259), and the word at `0x10` to the FMT-COMBAT-001
record's word at `+6` (FND-PARTY-049, FND-PARTY-050).

**Generation.** Overlay 184 `+1CED`, which the generation finish calls (`+10E4`), counts a word
up from 0x8015 to below 0x8300 until it finds one that no combatant record of slots 0 to 3 holds at
`+6` and that none of up to 39 calls of `38FF:04AB` with the tag `CACT` leaves in its output word; it stores that
word at `4E4F:0037`, at `4E4F:0071`, at `+0x0E` of the slot's details record and at `+6` of its
combatant record (`+1D12..+1DAB`). When `DS:112E` is -1, overlay 184 `+1102..+1113` stores 0 in the
slot's state byte at `4F49:0C33` and at `+0x0E` of its details record.

**Reads.** `field_reads.py <dsun> <inventory> --es --table 19c5 e` finds 20 reads in the
inventory, all `cmp word ptr es:[bx + 0xe], 0` after a load of `DS:19C5` and a multiplication
by 0x42, in overlays 176, 189, 190, 191, 207, 210 and 213. At overlay 189 `+0109..+0137` the
slot in `4E71:0B44` is printed by the name of the record at `DS:142D` when the word is not 0 and as
`INACTIVE CHARACTER` (`DS:1AB2`) when it is 0. Outside the inventory, `2D40:2D38` and
`2D40:2D3F` are the search FND-PARTY-259 reads in `2D40:2C87`: for a slot above 4, the details
records 5 to 16 are searched for one whose word at `+0x0E` equals the word at `+0x0E` of the data
being loaded, and else the last one whose word is 0 is taken (`2D40:2D1C..2D40:2D78`). Overlay 179
`+1952` reads the word for a struck combatant's details record and compares it with the six words
at `cs:28A9`, 7020, 7045, 7046, 7047, 7050 and 7051, jumping to one of three cases
(`+1942..+196E`).

**The shipped records.** In 14 of the 19 `CHAR` resources of the installed `CHARSAVE.GFF` the
word at `0x53` equals the word at `0x10`, which runs from 0x8001 to 0x801B in them. In the other
five the word at `0x10` is 0x82FD to 0x8300 and the word at `0x53`, read as signed, is -21
(resource 31), -14 (33), -7039 (50), -7036 (51) and -7038 (53).

**OBJEX.** Of the 375 details chunk headers in the installed `OBJEX.GFF` that `details_chunks.py`
finds, 353 in `RDFF` resources, the signed word at `+0x0E` of the data is between -7111 and -7007
in all, with 93 distinct values; -7007 occurs 35 times and -7033 29 times. No resource of any
installed GFF archive is numbered 7000 to 7120.

## Interpretation

The word at `+0x0E` of a details record, CHAR word `0x53`, is the record's key. A details record
with key 0 is free, and a party slot whose details record has key 0 holds no character. A
character made in generation gets the new combatant identifier as its key, so the two words
agree. Creatures loaded beyond the party share a details record when their keys agree, and the
creature records of `OBJEX` use keys from -7111 to -7007, many shared, as a key of the creature's
kind would be. The five shipped characters whose keys differ from their identifiers are of the
kind that was not made by generation: their identifiers are at the top of the range generation
leaves out.

### Connected allocation boundary

The reread of overlay 184 `+1CED..+1DCE` corrects the earlier call count.
This body initializes its candidate to `0x8015` only when `DS:112E` is -1;
otherwise it skips allocation. For each candidate it first compares the full
16-bit word against combatant slots 0 through 3. On a match it advances the
candidate without reading CACT. Otherwise the resource loop starts at 1,
continues while the number is below 40, and stops early on a full-word match.
There are at most 39 reads per candidate, not 40.

The local comparison word is initialized to zero for each candidate, but not
again between resource reads. The caller ignores the read routine's return
value and compares the resulting local word anyway. Consequently a failed read
cannot simply be counted as an absent identifier: its effect depends on the
reader's output-buffer behavior and any earlier value retained in that word.
No external read-success or stable-buffer admission is asserted here.

If neither party nor stored-word comparisons match, this body publishes the
candidate to the two temporary words and both selected runtime records described
above. On rejection it increments the 16-bit candidate and uses a signed
comparison against `0x8300`; all candidates `0x8015..0x82FF` have the same signed
ordering in this interval. If all 747 candidates are rejected, it reaches
`0x8300` and exits without executing those publication stores. It does not
publish `0x8300` as an exhaustion sentinel. The final comparison's flags are
overwritten before return; no useful success return is established.

The normal caller at file `0x6E171..0x6E17F` passes the slot and, after return,
calls `27E5:000F` without testing allocation success. Which generation UI routes
admit that caller remains Q-PARTY-130. This is not a proof that exhaustion is
reachable in an ordinary installation.

The preparatory call is overlay 186 `+0572..+05AD`: it returns AL0 if the
archive handle at `DS:144A` already exists; otherwise it invokes the
CHARSAVE.GFF opener, calls the error path if its low-byte result is zero,
and on an ordinary continuation returns AL1. The allocator remembers this
byte and calls `+05AD..+05DD` only when it is nonzero. That helper conditionally
closes the handle, reports a nonzero close result, and clears the handle on
ordinary continuation. These are conditional helper interfaces, not evidence
that failed open/error paths return normally.

No instruction in the allocator separately masks, sets or clears bit 15:
it starts with an already high-bit-set constant, increments whole words and
compares whole words. This establishes a high-range allocation convention,
not a global interpretation of that bit in every identifier consumer.

## Alternatives

- This supersedes FND-PARTY-147, whose generation paragraph incorrectly said
  40 CACT calls. Its details-key, shipped-record and OBJEX observations above
  are retained; this reread does not independently repeat those data censuses.

- What the -7000 range numbers, and why overlay 179 compares the key with positive values in that
  range, was not read; a creature loaded in play may carry a positive key.
- Which routine reaches overlay 184 `+1102` with `DS:112E` at -1, and so when a slot is emptied,
  was not traced.
- The reads in overlays 176, 190, 191, 207, 210 and 213 were not each read for what they skip.

## How to reproduce

For the connected allocation reread, use the locked evidence Python and
`tools/research/exec-census/overlay_listing.py <dsun> 184 6ED7D 6EE5E`,
`overlay_listing.py <dsun> 184 6E150 6E1AA`, and
`overlay_listing.py <dsun> 186 6FC72 6FCDD`. The reporter checks the licensed
installed source length and fingerprint and resolves overlay fixups. Interpret
only the complete caller instructions at `0x6E171..0x6E17F` in the caller
window. Python 3.12.10, executable-reader 4.3.0 / engine 19.10.0 and Capstone
distribution 5.0.9 were installed for the reread; the script uses Capstone
directly. Original-derived listings remain local. Additional locations:
DSUN.EXE file `0x6FC72..0x6FCDD` and `0x6E171..0x6E17F`.

The retained observations from the superseded finding reproduce as follows.

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `overlay_listing.py <dsun> 184 6ED7D 6EE5E`, `overlay_listing.py
<dsun> 184 6E160 6E1B0`, `field_reads.py <dsun> <inventory> --es --table 19c5 e`,
`field_stores.py <dsun> <inventory> --es e`, `overlay_listing.py <dsun> 189 74AB0 74B30`,
`resident_listing.py <dsun> 2D40:2C87..2D40:2E10` and `overlay_listing.py <dsun> 179 65C10 65C52`,
and read the twelve words at file `0x66B89`. Read the words at `0x10` and `0x53` of each `CHAR`
resource of `CHARSAVE.GFF`, and the word at `+0x0E` of the data of each details chunk that
`details_chunks.py` finds in `OBJEX.GFF`.
