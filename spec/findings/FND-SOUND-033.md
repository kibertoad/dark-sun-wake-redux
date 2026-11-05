---
id: FND-SOUND-033
title: Setup's binary open and startup stream initializer produce a conditional buffered path without text translation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0E0F..1000:0EC9
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2A3A..1000:2AF4
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2AF6..1000:2BC5
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2BC7..1000:2C46
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:37F1..1000:390E
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0519..1000:052A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:317F..1000:333D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:21A9..1000:21D2
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2799..1000:27B6
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1AF6:0001..1AF6:0037
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D06E..0x0001D071
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D308..0x0001D49A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D67C..0x0001D694
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table at 210f26ab; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

FND-SOUND-030/031/032 leave actual stream producers conditional. This
reads the connected binary-open route and startup slot initializer.
Addresses, field offsets and masks below are hexadecimal; sizes and
slot counts are decimal. FND-EXE-491 establishes DS's file mapping and
the startup dispatch; FND-EXE-572 retains its wider census limits.

### Startup slot production

The four startup records match FND-EXE-491. Their first target,
1000:0E0F at priority 02, precedes the three priority-10 records.
The initial handle-count word at DS:DD38 is twenty. All twenty stream
records at DS:DBA8 have zero count, size, base/current pointer and
auxiliary word at 10. The first five have handles zero through four
and self-offset identity words at 12. Slots five through nineteen ship
with zero flags, handle and identity: file-image zero alone does not
make one of these a free stream with a valid identity.

0E0F loops from index five while unsigned index is below DS:DD38.
For each index it clears the corresponding handle-table word at
DS:DD3A+2*index, writes FF to the stream's handle byte at
DS:DBA8+14*index+4, and writes that stream's own offset to its word at
12. With the retained initial count this covers exactly fifteen slots,
ending at index nineteen. It does not initialize their remaining
count or buffer fields; those remain the measured initial zeros unless
something else writes them. Its later calls initialize the first two
standard streams, not these fifteen slots. Later startup/application
effects and aliases still need a preservation reading.

### Actual open request and slot choice

1AF6:0001 passes DS:D90E as the mode and DS:D904 as the filename to
1000:2C11, storing its DX:AX result at DS:1888/188A. The three mode
bytes are lowercase r, lowercase b and NUL. 2C11 calls slot finder
2BC7 before initializing a stream. The finder tests each signed handle
byte and returns a slot with a negative byte; under the initialized,
otherwise unchanged table the first such slot is index five.

The finder increments its pointer before comparing the previous slot
offset with its computed end. Thus its general exhausted-table path
can inspect a slot beyond the nominal table. That path is not needed
for the admitted first-free-slot condition and is not a bounded-table
guarantee. All slot identities and allocation bounds remain explicit
conditions outside that first-slot case.

2C11 passes the selected slot, filename, mode and zero extra flags to
near helper 2AF6, whose RET 000E removes all fourteen argument bytes.
Its mode child 2A3A receives far output pointers to two local words and
the supplied mode; RET 000C removes those twelve bytes. For this exact
rb input it produces stream flags 0041, lower-open flags 8001 and
permission word zero. Both locals are assigned before 2AF6 reads them.
It also installs the in-image close-all exit hook of FND-EXE-491.
Other mode strings are outside this bounded producer claim.

2AF6 stores 0041 into the stream flags before checking its handle.
The initialized FF byte takes the lower-open call, with filename,
8001 and zero permission. Its returned AL replaces the handle byte;
a negative AL makes the wrapper set handle FF, clear stream flags and
return a zero far pointer. This is a byte test, not a full-word handle
range guarantee. A returned handle in zero through nineteen satisfies
the separate lower-reader handle bound under retained DD38.

On this read-only binary route, lower opener 317F makes its attribute
query through 2799, skips the create/truncate alternatives, and opens
through 32EE with DOS request AX=3D00. 32EE records an initial binary
handle-table word on success. 317F's later device query through 21A9
can alter device state; on the conditional non-device result it skips
the device-mode and append branches. Its final handle word retains
8001 and may add 0100 according to the saved attribute result's low
bit. It has neither 4000 text translation nor 0002 read rejection.
DOS results and preservation of tables through interrupts are not
observed here. Attribute-query failure does not by itself stop this
read-only route before its open request.

### Buffer construction and first read

After the handle test, 2AF6 calls 0519, which issues AX=4400 and returns
only returned DX bit 0080, without testing carry. A zero result leaves
stream flags 0041; a nonzero result adds 0200. For the zero-result
case the wrapper calls 37F1 with the selected stream, null buffer,
mode zero and size 512. The explicit null pointer and size are written
by this caller, not inferred defaults.

37F1 requires the identity word at 12 to match the supplied offset.
The initialized index-five identity satisfies this if retained. Under
count zero and flags 0041 it skips both the reposition child and old
owned-buffer release. It clears flags 000C, makes size zero, and sets
base/current pointers to the in-record byte at offset five. It then
requests 512 bytes through allocator 1BD6. A zero allocation returns
FFFF after those earlier mutations. A nonzero result sets owned-buffer
flag 0004, assigns both base and current pointers to the allocation,
and writes size 512. Mode zero does not set line-buffer flag 0008.
FND-SOUND-026 supplies the allocator's conditional offset-four result
and preserves its native heap/service obligations.

Under that successful, disjoint allocation and the stated retained
count/flags/identity conditions, the wrapper receives zero, clears
auxiliary word 10 and returns the slot pointer. Stream flags are 0045,
count zero and buffer size 512. The first FND-SOUND-030 byte request
therefore uses buffered refill, with no 0200 flush scan. Under the
retained binary handle word, FND-SOUND-031's lower reader bypasses
CR/1A translation. A successful count no greater than 512 fits the
normal offset-four allocation without offset wrap. Later refills
continue to depend on the fields and handle word remaining retained.

## Interpretation

The actual mode constant and startup handle/identity producers are no
longer guesses. This removes the installed-mode and local normal
stream-construction uncertainty from the stream portion split into
Q-CONFIG-011. The binary route excludes text translation and read-side
flush scanning under the explicit successful regular-file/storage
conditions. It does not prove those native conditions hold, every
startup child preserves the table, or setup completes.

## Alternatives

Treating the zero-filled free slots as ready before 0E0F omits their
handle and identity writes. Assuming the rb path strips CR contradicts
the binary table producer. A device/error result, high returned handle,
changed table/count/slot, invalid identity, failed heap request or alias
can defeat the normal continuation. The full exhausted-slot route,
cleanup after construction failure, other modes/callers/writers and
actual DOS/heap outcomes remain Q-CONFIG-011. Numeric/token and field
consumer obligations remain Q-CONFIG-006 and Q-CONFIG-012. No complete
reading, original runtime result or parity promotion is claimed.

## How to reproduce

Use approved installed SOUND_DS.EXE, length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Retain reports only under
GAME_DIR/analysis/disc-adv-survey-20261104/. Run resident_listing.py
with -I and the ten code ranges above in order; setup-stream-construction.txt
and its -repeat pair have SHA-256
4ed4003183d0782a8f155fec379e99fa37c0af393da760dcb772837a0e82e9b2.

Run report.mjs table twice with each named setup controls file. The
open-mode query reads three one-byte rows at file 1D06E; stream-initial
reads twenty stride-20 records at 1D308 with the fields named above;
handle-count reads one word at 1D498; startup-records reads four
stride-six records at 1D67C (kind byte, priority byte, offset word,
segment word). Controls and output remain local. Their report/repeat
SHA-256 values, in that order, are:

- e5b0d3a91c6496e1cfd1159d7480b2cfa60b8a3326f5981561c447cee4be15cd
- 6cddf948a78e9e1e46c028488b9e0426ab0efc2e285907780c0afe14e6debdf4
- 969e6df2fa3e735e20d7528b1d333bf4dfba28af8e15d3bbfe5e557d0fd54001
- dfcadd8b9491ec3d9d7a2dcbebeec7117a3ad24bf865bdcf32bd315af19b5771

Use FND-EXE-491's DS-to-file mapping, the mode caller, initializer
stores, known handle count and startup dispatch as positive controls.
Separate emitted byte values from the native state after startup.
