---
id: FND-CONFIG-179
title: A mode helper passes callback 28C9:0061 and mask 0166 before ungated resource results
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:00BB
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared relocation/fixup mapping
environment: null
---

## Observation

FND-CONFIG-161 calls overlay entry 56BD:00BB after its
pointer consumers. Descriptor 182 maps this entry to
code 19F8, file span `0x0006A248..0x0006A40A`, ending
with far return at `0x0006A409`. Its local body first
clears current DS bytes 0DA4 and 0DA3, then tests word
DS:0DAB. Fields below mean DS at each instruction; the
external callees' preservation is not assumed.

An initial value two enters a four-record loop with no
calls inside it. The record base is the far pointer at
DS:19C9; record offsets add 49 times indices zero through
three in word arithmetic. Each record with nonzero word
at +6 and zero byte at +14 has that byte set to one.
The body then calls local far 0840 and resident
362C:06B9, passes word DS:140C to local far 0297, passes
that word and word zero to 362C:01FA, then passes double
word 000092E0 to 5773:0025. These callees' full effects
remain open. The word 0DAB is read again afterward;
its initial value alone does not guarantee the next gate.

A value two or three at that later gate continues;
other values return without the remaining local work.
The continuing branch passes the far pointer stored at 4E28:00D7,
word identifier 4B01, far callback 28C9:0061 and word mask
0166 to local far 01EF, in the callee's argument layout.
FND-CONFIG-180 resolves those widths through both resident
setters; the earlier grouping of 0166 as a callback offset
in FND-CONFIG-169 was wrong. It then passes double-word zero to
39D1:0430, sets byte DS:143F to one, calls 2C5F:0139
then 28C9:250A, and clears byte 143F. Next it passes
28C9:152C to 39D1:0448, writes word DS:0D9C to one,
and passes 28C9:0CFF to 571F:0034. FND-CONFIG-085
identified this setter site; FND-CONFIG-083 and
FND-CONFIG-163 bound its stores and prior guard route.
These local calls do not establish that registration
completes or the callees return.

Word 4C10:0019 equal to zero causes a copy of word
4C13:0369 into current DS:426D. Other values skip it.
The body then assigns word DS:0DAB to one. Nonzero
word DS:445A enters the remaining resource branch;
zero returns. That branch sets word DS:0FCC to FFFF
and makes up to three requests through 38FF:04AB:

| Destination pointer field | Required local condition | Type | Resource number |
|---|---|---|---|
| Current DS:9BE8 | That field is zero. | BMP followed by a space | 19002 (4A3A hexadecimal). |
| 4E71:0A59 | That field is zero at its later test. | ICON | 19109 (4AA5 hexadecimal). |
| 4E71:0A5D | That field is zero at its later test. | ICON | Zero-extended current word DS:445A. |

Each call supplies the far address of the destination
pointer field, not the pointed-to buffer. Type and number
arguments are double words. FND-CONFIG-151 reads this
wrapper's complete-count path: it can allocate and assign
a destination pointer before a later failed archive or
I/O operation. These calls provide no own capacity argument,
record-validity check or restoration of the destination
field on failure.

After each resource call, an OR of AX with itself sets
flags, but no conditional branch consumes those flags.
The next actual conditional branch uses a new comparison
of a pointer field or word DS:1440. There is no local
success-result gate on any of the three calls. The later
requests therefore are not locally contingent on earlier
AX being zero, provided the calls return and their later
state permits the respective field test.

Only the third field's zero branch reaches the final
mode test. After its resource call returns, word
DS:1440 equal to five continues. If 4E71:0A59 is then
nonzero, it is passed with word zero to 3D72:12ED.
Whether that call was made or skipped, this mode-five
continuation clears byte current DS:9BE7. A nonzero
third field bypasses the entire third request, mode
comparison, optional 12ED call and byte clear. No
success test on the third request protects those steps.

All overlay segment operands in this body were resolved
using its declared FBOV fixups, including the final
38FF:04AB and 3D72:12ED calls. The prefixed immediate
pushes were checked for double-word argument width;
the record index and offset additions remain words.

## Interpretation

The following helper has mode-dependent state writes,
callback requests and conditional resource requests. Its
resource continuation depends on pointer and mode state,
not returned success. A populated field can remain after
a failed resource operation, and can then bypass a later
request. This local contract does not establish actual
failure, accepted content, an archive switch or successful
restoration of any native screen.

## Alternatives

This entry supersedes FND-CONFIG-169, whose caller-push grouping assigned
the wrong callback offset. FND-CONFIG-180 resolves the local wrapper and
its resident argument widths. The remaining local observations are retained
with that correction; actual registration, resources and native outcomes
are still conditional. FND-CONFIG-181 subsequently bounds 0297's archive
handle, filename and near/far segment conditions.

Q-CONFIG-008 and Q-SCRIPT-003 retain word/byte and record
producers, valid pointer fields, selected resources and
archives, complete local/external callees, later writers,
DS preservation, aliases and I/O outcomes. One reading
supplies valid records and successful requests; another
returns errors after different state or assigned outputs.
Complete producer/callee evidence and native I/O results
would distinguish their reachable outcomes. An AX flags
instruction without a dependent branch does not resolve
that choice.

The initial two branch can change state through callees
before the later two/three gate. The latter gate is not
proved by the first comparison alone. Existing nonzero
outputs can come from accepted resources or other
producers; their mere presence proves neither identity
nor valid loaded bytes. No native or emulated observation
is claimed, and Q-SCRIPT-007 cannot execute this overlay.

## How to reproduce

Resolve descriptor 182's 00BB trampoline to code 19F8.
Read 19F8 through 1BB9 from its entry, verifying each
declared fixup. Follow initial mode two, the four-record
loop, intervening calls and the separate later two/three
gate. Retain callback argument widths/order and the setter
guard dependency. Follow all three destination-field
checks and each resource call through the next actual
conditional branch; check which instruction last sets
its flags. Inspect prefixed double-word pushes and the
zero-extension of DS:445A. Compare FND-CONFIG-151's
output-pointer assignment before I/O and the existing
setter findings. Keep unread callees and actual outcomes
separate from this local ordering.
