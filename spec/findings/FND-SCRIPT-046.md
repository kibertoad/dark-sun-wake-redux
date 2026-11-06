---
id: FND-SCRIPT-046
title: Object99 has one shipped ETAB placement in Limbo and does not itself establish the region1 TALK key
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RGN0FF.GFF
    offset: 0x00006A2F..0x00006A37
  - build: BLD-GOG-EN-1.1
    file: RGN001.GFF
    offset: 0x00000000..0x00019941
tool: etab_objects.py and tools/evidence/report.mjs table/xxh3 at 3cf8bd98; Python 3.12.10; FND-SCRIPT-034 key-normalization interface reuse
environment: null
---

## Observation

FND-SCRIPT-045 identifies MAS/1's TALK registration for normalized key
-99 and direct pair GPL/37 offset 0A41. This assessment tests a candidate
placed-object input for that key. It does not assume that every normalized
key names a source object resource or that registering a key places an object.

The installed-archive ETAB query reads 26 GFF archives, 20 ETAB resources
and 13559 eight-byte records. It finds one record with positive object
word 99: RGN0FF.GFF, ETAB/255, row 128 (zero-based), coordinates
1462 and 159, flags six. No other ETAB row in this queried installed set
has object word 99, including RGN001.GFF's table. FMT-REGION-006 and
FND-REGION-004 define this positive object word and the source-table scope.
This is an initial source placement census, not a live placement census.

RGN0FF.GFF is 33987 bytes, XXH3-128
ce488ce6806ad6a214569a318f408c91; RGN001.GFF is 104769 bytes,
XXH3-128 063a33c7cfa3fb234f519af2d1182290. Source hashes for all
queried region files are retained with the report. RGN0FF is region 255,
Limbo (FND-REGION-001 and FND-PARTY-153). The latter finding's
independent control object 430 is recovered at ETAB/255 row 909,
coordinates 31 and 1383, flags six.

A separate bounded eight-byte table read at RGN0FF.GFF file
6A2F..6A37 agrees with the object-99 record. Its location follows
from the established control record 909 at file 8297 (FND-PARTY-153),
which gives ETAB base 662F and row 128 at 6A2F. This checks the
specific location against the archive directory walk.

In the retained script decode, MAS/1's only operand occurrence of literal
-99 is its registration at 0135. It is not an explicit object-creation
or placement operation at that location. This textual observation does
not exclude computed values, other scripts or native placement mechanisms.

### A source object and a trigger key are different bindings

FND-SCRIPT-034 already establishes that the key wrapper copies any signed-
negative incoming word straight to AX without calling the field getter.
Thus an admitted raw input -99 returns -99 on that intact local path;
no kind-specific metadata is needed for that branch. It remains necessary
to establish whether an actual TALK caller supplies such an input.

For a nonnegative caller argument, the wrapper instead asks for field
zero through the kind-specific getter. The recorded kind-one field is
the negated source object word, while kind two uses the character archive
identifier, kind three a details key, and other kinds have their separate
meanings and validity conditions. The selected TALK argument therefore
cannot be assigned a unique live identity merely by reading registration
key -99 or the ETAB object-99 row. FND-SCRIPT-033/034 and Q-SCRIPT-022
retain the actual handle/kind/metadata admission question.

For the specific hypothesis that a region-1 TALK event uses an initially
placed source object 99, the census supplies no region-1 placement. That
candidate needs a later transfer, creation, restored placement or another
producer before it can establish such a live input. It does not follow
that the trigger is unused, that the player cannot meet the object, or
that every route yielding normalized key -99 requires object 99.

## Interpretation

Q-SCRIPT-024's input-admission work has a concrete source constraint:
MAS/1 supplies the registration, while the initially placed object-99
candidate is in Limbo. Keep that candidate's runtime placement history
separate from the known negative-input fast path and nonnegative field-zero
mapping. The local key conversion need not be reread as unknown arithmetic.
Actual event input, registration retention, selected trigger and the other
entry/state conditions remain open. No native TALK event or whole-entry
complete reading is established.

## Alternatives

Equating the registration with a region-1 object placement contradicts its
opcode and omits the source census. Inferring that the trigger is unreachable
from the absence of a region-1 ETAB row omits runtime transfers and other key
bindings. Treating a raw negative -99 as requiring a metadata lookup ignores
the already-read direct-return branch. No full absence claim about live or
computed producers follows from this installed-resource query.

## How to reproduce

Run tools/research/exec-census/etab_objects.py at 3cf8bd98 over the licensed
installation with object arguments 99 and 430. Retain
GAME_DIR/analysis/handler-census-20261106/gpl37-object99-placement-controls.txt;
the earlier 99-only result is gpl37-object99-placements.txt. Require the
independent object-430 control and the archive/resource/record totals.
Use tools/evidence/xxh3.mjs on the region sources, retaining
gpl37-region-source-hashes.txt. Scope remains this installed archive set.

Run the bounded table reporter with gpl37-object99-record-config.json:
source RGN0FF.GFF and its hash above, start 6A2F, count one, stride eight;
unsigned fields x/y at offsets zero/two width two, flags at five width one,
and object at six width two. Output gpl37-object99-record-report.json
must agree with the archive census. All observed values are nonnegative;
this query does not redefine the format's signed coordinate fields.
Reuse FND-SOUND-077's retained script report for the MAS/1 operand check.
No new reader, original-game run or emulated call was used.
