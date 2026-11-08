---
id: FND-CONFIG-262
title: BLASTER fields and profile selection supply twelve extended-record writer sites including installed card 7A
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 164C:0008..164C:1377
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:03A9..1000:03C5
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:307B..1000:30EB
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:02CD..17FB:0346
tool: Ghidra 12.1.3 ReportFunctionSummary.java; resident_listing.py; tools/evidence/report.mjs table; retained controlled EC/EE reports in FND-CONFIG-259/260
environment: null
---

## Observation

This follows the installed card7A's extended-record writer together with
its shared source/selection path and related writers, rather than assigning
meaning to one isolated equal-offset store. SOUND_DS.EXE remains204593
bytes, XXH3-128236c2dc23c071eca421eb5b427caee57. Addresses, fields, masks,
card IDs and explicit register values below are hexadecimal; counts and
type values are decimal. Data segment is1E36.

### Source binding and retained values

The entry164C:044C calls00F1, which requests a fixed source name through
1000:307B and tests the actual returned DX:AX for zero. Its name and the
five field helpers' separately stored names all resolve to ASCII BLASTER
followed by NUL. A null presence result takes the local zero-return path.
A found empty value still has a nonnull pointer; that is not field admission.

The complete307B body reads the far string-vector pointer DS:DF0A. It
returns zero for a null/empty requested name, null vector, null entry or
empty entry terminating the walk, or an exhausted search. For each entry
it requires the same first character, an equals sign exactly after the
requested name length, and an exact case-sensitive prefix comparison.
The first match returns the far pointer immediately after the equals sign.
The vector advances by four bytes with low-word offset arithmetic. Its
backing, terminators, initialization and lifetime remain admitted state;
no host environment lookup or original executable is run by this research.

Each field helper requests BLASTER again. The entry neither caches that
pointer nor checks their return words before later consuming the stored
values. Four source globals have these loaded-image values, verified with
a bounded table read:

| Global | Loaded word | Entry's destination use |
|---|---|---|
| DS:070A | 0000 | A-derived second-block address |
| DS:070C | 0000 | First-block address, A or P path |
| DS:070E | 0000 | I-derived IRQ-related word |
| DS:0710 | 0000 | D-derived DMA-related word |

These are initial image values, not a reset by044C. A helper returning
without its own output store can retain an earlier value. Calls separated
by source lookup or children do not prove one immutable BLASTER value.

### Exact local field extraction

The helpers initialize stack-local buffers from zero-valued templates:
five bytes for0140/01C5/025F/0319 and three for03BE. The complete
1000:03A9 helper clears direction, copies CX bytes with word and odd-byte
operations, restores DS/SI/DI/BP and returns far while removing its two far
arguments. The fixed templates and disjoint valid stack spans supply the
terminators after each limited copy; capacity is not inferred from a name.

Character searches use1000:0EC9's existing ASCII case-fold contract from
FND-SOUND-020. Each search increments only its source offset. Except for
the P helper's explicit T alternative, these loops have no NUL-exit arm;
the requested marker must occur within admitted readable source storage.
Numeric conversion is1000:2713's decimal-prefix routine from FND-CONFIG-261;
these callers retain AX only.

| Helper | Marker | Copied source after marker | Local outcome |
|---|---|---|---|
| 03BE | T | One character | Stores its converted word through the argument; returns1 only when that word is6, otherwise0. |
| 0319 | A | Three characters | Converts, applies0042's remap and stores the word; nonnull source route returns1. |
| 025F | P | Three characters | A T encountered before P returns0 without storing; otherwise converts/remaps/stores and returns1. |
| 01C5 | I | First character and a second unless the second is space | Converts/stores and returns1. |
| 0140 | D | One character | Converts/stores and returns1. |

All return0 without their own destination assignment for a null source.
The A/P remapper0042 recognizes exactly these conversion-result words;
every other word becomes FFFF, without changing the caller's success1:

| Input word | Output word | Input word | Output word |
|---|---|---|---|
| 00DC | 0220 | 00D2 | 0210 |
| 00E6 | 0230 | 00F0 | 0240 |
| 00FA | 0250 | 0104 | 0260 |
| 014A | 0330 | 012C | 0300 |
| 014C | 0332 | 014E | 0334 |
| 0150 | 0336 | 0140 | 0320 |

Thus familiar address spelling is handled by explicit decimal-result
remaps, not by a general hexadecimal parser.

### Caller order and profile selection

044C initializes its local type word to FFFF and calls03BE with it.
It uses the stored type, not that helper's return word. For type6 it calls
P extraction into070C, then A extraction into070A. Other types call A
extraction into070A and again into070C. All paths then request I into070E
and D into0710, with independent source lookups and ignored helper returns.

If17FB:02CD(7A) returns zero while the stored type is6, it replaces the
type with4. That helper's complete local graph searches Main, Digital and
MusicOnly groups in order for record word+28, under signed counts and the
254-byte stride; it returns1 at a first match and0 when none exists.
It is group membership, not driver installation or hardware detection.

The type next indexes a far-caption table through012A using low-word
type*4, and the caller passes the returned DX:AX through its formatter,
copy and notice calls. There is no local type-bound validation there;
caption/table/frame and child completion remain independent conditions.
After those calls, the ordinary stored-type branches are:

| Retained type | Card whose records are written |
|---|---|
| 1 | 66 |
| 2 or3 | 67 |
| 4 or5 | 73 |
| 6 | Selection among7A,85,79 below |
| Other | No record stores in this dispatch graph |

For retained membership state on the type6 path, these are the alternatives.
The implementation repeats the membership calls across branches; this table
does not replace them with cached flags under changed state:

| 79 present | 85 present | Supplied prompt result and chosen writer |
|---|---|---|
| no | no | 7A directly |
| yes | no | 0 selects7A; nonzero selects79 |
| no | yes | 0 selects7A; nonzero selects85 |
| yes | yes | 0 selects7A;1 selects85; other selects79 |

No actual prompt result or device state is observed. On admitted completion
after nonnull presence, the local final return is1, even when no corresponding
group record was found; it is not a device-success result. The absent-source
branch returns0. Formatter/prompt effects are not established by those returns.

### Paired extended-record writers

The type1..5 branches and the three complete helpers0CDA(card85),
0F0F(card7A) and1144(card79) each test MainGroup and DigitalGroup separately.
Within each admitted group they repeat its index lookup for each word
store and set record EC to1 after the eight extended stores. Reuse
FND-CONFIG-259's membership/index contracts; no single immutable target is
inferred from the repeated calls. There is no local MusicOnlyGroup write.

Under valid disjoint retained group/record/global/frame storage, both groups
receive these word mappings. Card66/67/73 use the ordinary column:

| Record field | Ordinary and7A | Card85 | Card79 |
|---|---|---|---|
| EE | DS:070C | DS:070A | DS:070C |
| F0 | DS:070E | DS:070E | FFFF |
| F2,F4 | DS:0710 | DS:0710 | FFFF |
| F6 | DS:070A | DS:070A | DS:070A |
| F8 | DS:070E | DS:070E | DS:070E |
| FA,FC | DS:0710 | DS:0710 | DS:0710 |

Each of the three helpers assigns its literal card ID to DS:ECCE after
both group tests, even when both miss. The ordinary type1..5 branches
have no corresponding own ECCE assignment. Card79's helper first calls
0008 and ignores its result. That child's local graph obtains an A-derived
base into a local word, and on extraction success writes83 to base+4 and
0B to base+5 through OUT instructions. Map value FFFF does not itself
clear the extraction-success flag. No port/device outcome is established;
the remaining record statements require that child to return with the
declared bindings. No such original call was executed.

The Ghidra bodies and retained instruction reads agree on all deciding
far-return bindings, widths, field searches, remaps, repeated membership/
index calls, branch order and record stores. The discontiguous044C body
ranges match its current inventory; linear bytes between them are not
silently added as supported alternative entries.

## Interpretation

This supplies the shared source/selection contract for twelve EC and twelve
EE write sites in FND-CONFIG-259/260's controlled cohorts. Q-CONFIG-005
retains eight other EC and six other EE sites, excluded computed/block/
alias mechanisms, actual source-vector/table/frame/group lifetime and
downstream device/file behavior. The installed SOUND.CFG's card ID7A can
reuse this conditional producer; its equal installed blocks do not prove
which native source or prompt path produced them. No complete reading,
rule/format promotion, native execution or parity validation is claimed.

## Alternatives

A found BLASTER name does not establish complete fields. Loaded zero words
are not a later reset. A remap of FFFF is not extraction failure. Group
membership is not hardware support. The table of stable memberships does
not cover changes between the repeated calls. The first-block forced FFFF
values for79 do not generalize to7A or85. These distinctions preserve the
caller, lifetime and external-evidence questions rather than assuming them.

## How to reproduce

Use the verified SETUP-blocks project under
GAME_DIR/analysis/config-block-construction-20261108/. Packaged
ReportFunctionSummary.java results are installed-card-producer-selection.log,
blaster-source-dispatch-ghidra.log, blaster-source-fields-ghidra.log,
blaster-source-binding-ghidra.log and blaster-profile-membership-ghidra.log.
They name all functions above. Regenerate blaster-producers-instructions.txt
with resident_listing.py for164C:0008..044C,044C..0CDA,0CDA..1377;
blaster-source-binding-instructions.txt covers1000:03A9..03C5 and307B..30EB.
blaster-profile-membership-instructions.txt covers17FB:02CD..0346; compare
its membership arithmetic with FND-CONFIG-259's groups.

Five local table query/result/repeat sets use report.mjs table:
blaster-initial-globals at fileFE6A, four stride-two words;
blaster-buffer-templates at FE8A, four stride-five byte records;
blaster-type-template at FE9E, one three-byte record;
blaster-presence-name at FEA1, one eight-byte record; and
blaster-source-names at FF2A, five stride-eight byte records.
Counts are bounded by the explicit global destinations, CX copy counts
and adjacent literal arguments. Every result matches its repeat. Reuse
the unchanged EC/EE controlled reports; their164C:044C..1377 cohort has
twelve verified writes of each queried displacement. All original-derived
output stays local. No original program or emulated call was run.
