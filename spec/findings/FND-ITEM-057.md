---
id: FND-ITEM-057
title: Canonical DATA properties narrow equipment dispatch while the getter retains separate cache and read admission
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0003..2C5F:0051
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04AB..38FF:04D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004DF4B..0x0004DF4D
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000120E8..0x00017C28
tool: Existing DarkSunWakeRedux.Inspect gff; tools/evidence/report.mjs table and tools/research/exec-census/resident_listing.py at 8dcb22f1; composition with FND-PARTY-254 and FND-ITEM-056
environment: null
---

## Observation

FND-ITEM-056 bounds the ordinary equipment-hook selectors to 0..318 but
leaves DATA-pointer results and property values open. This reads the complete
getter, verifies the canonical catalog/property domain and composes selector
frequencies. It does not infer successful native reads from archive presence.

### Complete getter and cache branches

2C5F:0003 takes a selector word N. It initializes a local far-pointer variable
to DS:4277. Exact N=FFFF returns DX:AX zero without a resource call or key
update. No other selector-range guard is present.

For other N it compares N with word DS:0F4B. Equality returns the initialized
DS:4277 pointer without reading or validating the buffer. Inequality calls
38FF:04AB with DATA, sign-extended N and an SS-based far pointer to its local
output-pointer variable. A returned AX of FFFF yields a null pointer without
updating the key. Every other AX value stores N at DS:0F4B and returns the
current local pointer variable. This low-word test is the getter's acceptance
condition, not independent proof of complete I/O or valid content.

The wrapper 38FF:04AB checks its runtime stack guard, then calls 38FF:07B1
with the supplied tag, number and output-pointer address, requested offset
zero and length -1. It returns that reader's result. FND-CONFIG-034/234 give
the reader's archive/search/allocation/read prerequisites and failure paths.
The stack-guard failure helper is also outside an ordinary returning-call
claim. No separate success-code normalization is introduced by this wrapper.

The installed image's initial key word is FFFF. With that initial value
retained, an ordinary canonical N=0..318 takes the miss branch; current
runtime or restored state is not assumed to retain the initial key. A failed
miss does not invalidate the previous key or restore the shared buffer in
this getter. Content/key coherence therefore needs its own preservation
contract, especially if a lower reader changes bytes before reporting failure.
No actual partial read or stale-buffer outcome was observed here.

On an accepted miss the lower reader can write the local output pointer;
its returned value must be admitted separately. On a hit this helper again
constructs DS:4277, rather than returning a separately cached far-pointer
variable. FND-RNG-091 independently identifies DS:4277 as this DATA request's
fixed destination. A nonnull pointer alone does not prove matching content.

### Canonical catalog and property census

The bounded Inspector catalog for RESOURCE.GFF, length 5724669 and
XXH3-128 92fe031616f36062f99484b0f728210e, contains 323 DATA resources.
Exactly IDs 0..319 have size 73, contiguous at file offset 120E8 hex plus
73 times the ID. The remaining three IDs are 1000,1001,1002; they are not
ordinary equipment selectors. The ID domain independently agrees with
FND-PARTY-096. Validate every ID, offset and size before treating source
array index as a resource identity.

A fingerprinted table read takes byte zero from each of those 320 records.
Its complete low-two-bit distribution is:

| Property | DATA records |
|---|---:|
| 0 | 97 |
| 1 | 99 |
| 2 | 83 |
| 3 | 41 |

All ordinary selectors 0..318 therefore have a source record and a first
property byte. The 73-byte record size describes the source and the intended
complete transfer span DS:4277..42C0; actual destination capacity, writes
and retained buffer identity remain native admission questions.

### Join with canonical item selectors

Join the 240 ordinary item-payload counts from FND-ITEM-056 by exact
N=W-1 to the unique cataloged DATA ID, not by arbitrary report order. The
weighted payload distribution is property zero 52, one 93, two 85 and three
10; the sum is 240. These are payload counts, not distinct DATA counts or
native invocation counts. The source W=0 and W=320..327 classes remain
FND-ITEM-056's separate early-return and rebuild paths.

With canonical W/bonus retained through deciding reads, status result zero,
returning children and DATA content matching that exact canonical record,
the 93 property-one payloads enter the after-hook's reset/dispatch path.
The other 147 do not: nonnegative bonus excludes the repeated-DATA/property-
zero alternative. Property one remains only a local dispatch eligibility
condition, not an observed effect or guaranteed successful dispatch.

These conditions include owner/index/record preservation, cache/content
coherence, admitted archive selection and I/O, DS/SS output-pointer/frame
identity, valid nonwrapping buffers and disjoint state. A different live
DATA property, changed selector/bonus or intervening cache write lies outside
this source-preserving composition.

## Interpretation

Q-PARTY-109 no longer needs an assumed missing ordinary DATA resource or an
unclassified canonical property byte. The remaining pointer obligation is
concrete: accepted miss/hit state, key/content coherence and the lower read's
actual result and destination. Q-ITEM-003 retains these caller/storage and
effect prerequisites; reset/dispatch and DS:6174 effects remain open.
No source catalog result promotes native parity, confidence or complete
readings, and no cache defect is claimed from a possible failure path alone.

## Alternatives

Treating a hit as a new resource acquisition adds validation absent from the
body. Treating a failed miss as invalidating the old key or undoing buffer
writes adds stores absent from this getter. Treating the 99 property-one DATA
records as 99 equipment invocations confuses source records with weighted
item payloads. A source record's existence does not choose its registered
archive or prove read completion.

## How to reproduce

Use the fingerprinted DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read 2C5F:0003..0051 and
38FF:04AB..04D5 with resident_listing.py. Retained placement-data-getter.txt
and placement-data-read-wrapper.txt are under
GAME_DIR/analysis/item-removal-20261104/.

Run the existing Inspect gff command on fingerprinted RESOURCE.GFF and
retain placement-data-catalog.json. Filter DATA, verify all IDs 0..319, sizes
73 and offsets 73960+73*ID, and retain the separate three high IDs. Run
report.mjs table with start 73960, count 320, stride 73, one byte at offset
zero and explicit limit 320 (the reporter default 256 rejects this count).
Read initial cache key at DSUN.EXE offset 4DF4B hex, count one, stride two,
one unsigned word at zero. Both tables require the exact source fingerprints.

Configurations/reports/repeats use placement-data-properties and
placement-data-initial-key stems in the same private directory. Repeated
reports are byte-identical; SHA-256 values are
f9fdd1123d3d4146ae761d3597c58ea4b20e55d8ea3862686adb60cf717769e8
and 5bf07056f9408c21e9daca6524c623bbe50e5a44e49b46e76e9a013b2fb636ed.
For each FND-ITEM-056 word10 histogram row with W in 1..319, add its count
to the group given by DATA ID W-1's property AND three. Verify total 240;
placement-data-source-summary.json retains the resulting counts. No original
code, DATA fixture, resource payload or image is committed or executed.
