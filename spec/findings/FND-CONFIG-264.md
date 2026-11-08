---
id: FND-CONFIG-264
title: ARIA source fields condition card 7D paired records with four address spellings and ignored extraction returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1783:0007..1783:0262
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1783:02F0..1783:057B
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3AA3..1000:3AE4
tool: Ghidra 12.1.3 ReportFunctionSummary.java; resident_listing.py; tools/evidence/report.mjs table; retained EC/EE cohorts
environment: null
---

## Observation

Q-CONFIG-005 retains three paired families after FND-CONFIG-263. This
reads1783:02F0 and its connected presence/address/IRQ/DMA helpers.
SOUND_DS.EXE remains204593 bytes, XXH3-128236c2dc23c071eca421eb5b427caee57.
Addresses, offsets, register words and card IDs below are hexadecimal;
sizes and character counts are decimal. Data segment is1E36.

### Source binding and per-call values

The caller initializes three word locals to zero, then calls1783:0007.
That helper passes its fixed name to1000:307B and tests both words of
the actual returned DX:AX. The source name and each of the three field
helpers' separate names are the same NUL-terminated ASCII ARIA string,
verified in four adjacent five-byte literal records.
Reuse FND-CONFIG-262's case-sensitive source-vector lookup contract.

A null presence result takes this caller's AX0 return without its own
record stores. A nonnull presence result requests address via015F,
IRQ via00C5 and DMA via0040, in that order. Each helper initializes its
own five-byte buffer from a verified zero template through1000:03A9,
then looks up ARIA again. The caller ignores all three helper return words.
These are new local values on every invocation, unlike the retained
BLASTER globals in FND-CONFIG-262.

The field searches advance the source's low offset word and use the
existing1000:0EC9 case-fold contract. They look for the first matching
character without a token-boundary or terminal-NUL branch. The requested
marker and subsequent copied bytes must exist within admitted readable
source storage; no malformed or missing-marker execution was performed.

| Helper | Marker | Value construction after that marker |
|---|---|---|
| 015F | A | Copies exactly three characters, explicitly appends NUL, then performs four independent string comparisons below. |
| 00C5 | I | Copies one character, and a second unless that second byte is space; converts with1000:2713 and stores AX. |
| 0040 | D | Copies one character; converts with1000:2713 and stores AX. |

The copied numeric fields remain terminated by their zero templates.
Reuse FND-CONFIG-261's decimal-prefix/sign/overflow and low-word return
contract; these are not checked whole-token conversions.
Every field helper returns0 without its own destination store for a null
source. On admitted nonnull execution, each returns1 even if the address
comparisons made no assignment. The address spellings and results are:

| Three copied characters | Stored address word |
|---|---|
| 290 | 0290 |
| 280 | 0280 |
| 2a0 | 02A0 |
| 2b0 | 02B0 |

The complete1000:3AA3 body clears direction and compares through NUL,
folding ASCII a..z to uppercase on unequal bytes before deciding equality.
Thus either case of the letters above matches. It restores DS/SI/DI/BP
and has no child. These are exact three-character string matches, not a
general address-number parser. If no comparison matches, the caller's
address local can remain zero. Repeated source lookups likewise require
actual preservation before treating their results as one source snapshot.

### Conditional paired record application

After those three requests, the caller invokes190F:0000 with its fixed
notice arguments and discards the result. It tests MainGroup membership
for card7D and independently tests DigitalGroup membership for the same ID.
On each match, it repeats that group's first-index lookup and254-byte
record-address calculation for every word store. Reuse FND-CONFIG-259's
group/count/pointer contracts. Under valid disjoint retained source/group/
record/frame/segment storage and completed child calls, each matched group
receives this same sequence:

| Record field | Stored value |
|---|---|
| EE | Address local |
| F0 | IRQ local |
| F2,F4 | DMA local |
| F6 | Address local |
| F8 | IRQ local |
| FA,FC | DMA local |
| EC, after the eight stores | 0001 |

There is no local MusicOnlyGroup update or ECCE assignment. The local
nonnull-presence continuation returns AX1 even if both groups were absent;
neither extraction nor notice result is a later success predicate.
It does not establish a device operation, valid persisted settings or
successful file write. Ghidra and the instructions agree on actual far-
return bindings, marker searches, literal arguments, comparisons, repeated
lookups, output widths and final marker ordering.

## Interpretation

This assigns two remaining EC and two EE census sites to a complete local
ARIA producer contract. Q-CONFIG-005 retains four EC/four EE sites in the
two file-based families, excluded implicit/block/alias access, actual source
and group/frame lifetime, and downstream behavior. A found name or helper
AX1 alone does not prove that all resulting settings were populated.
No original executable or emulated call was run; no complete reading,
format-status promotion or parity validation is claimed.

## Alternatives

The three-character address spellings do not imply acceptance of arbitrary
hexadecimal strings. Case-insensitive field search does not make the
source-vector name lookup case-insensitive. Initial local zero is not an
error status checked by the caller. Treating repeated group/index reads
as one target or the three field lookups as one source requires preservation
that the local control graph does not itself establish.

## How to reproduce

Use the verified SETUP-blocks project under
GAME_DIR/analysis/config-block-construction-20261108/.
remaining-extended-producers-ghidra.log contains1783:02F0;
card7d-source-fields-ghidra.log covers0007/0040/00C5/015F.
Regenerate card7d-source-instructions.txt with resident_listing.py for
1783:0007..0262,1783:02F0..057B and1000:3AA3..3AE4.

The card7d-names, card7d-addresses and card7d-templates query/result/repeat
sets use report.mjs table at fileFFB2(count4,stride5),FFC6(count4,stride4)
andFFA0(count3,stride5), reading one-byte fields. Counts come from adjacent
literal arguments and explicit five-byte initialization copies. All three
reports match their repeats. Reuse the unchanged EC/EE controlled cohorts:
this body's two marker sites are file9085/919A and EE sites8FA0/90B5.
Keep original-derived code, data and reports local.
