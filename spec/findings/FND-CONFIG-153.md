---
id: FND-CONFIG-153
title: The supported object archive holds FNFO resources at the initializer's exact length limits
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x0001FF48..0x0002037E
tool: DarkSunWakeRedux.Inspect gff at repository revision 2276305; Python 3.14.7 and xxhash 4.0.1 bounded reads and XXH3-128 verification
environment: null
---

## Observation

The installed source at C:\GOG Games\Dark Sun 2\OBJEX.GFF
is 6,816,516 bytes with XXH3-128
e08d3cac153c547f6804ca27b155915d, matching the
BLD-GOG-EN-1.1 manifest. The installed RESOURCE.GFF also
matches its recorded length 5,724,669 and XXH3-128
92fe031616f36062f99484b0f728210e. The installed
GPLDATA.GFF matches length 2,191,945 and XXH3-128
2bd1d2b989eef2dcb0545feb95975fca.

The existing bounded archive inspector, using FMT-GFF-001
through FMT-GFF-003, reports these requested FNFO entries
in OBJEX.GFF:

| Resource | File offset | Length |
|---|---|---|
| FNFO/1 | 0x0001FF48 | 980 |
| FNFO/2 | 0x0002031C | 98 |

These equal, rather than merely fall below, the initializer's
signed maximum checks in FND-CONFIG-150. The first entry
ends where the second begins. The RESOURCE.GFF and
GPLDATA.GFF catalogs contain no FNFO tag; that catalog
result does not establish which registered archive is selected
at a runtime request. FND-CONFIG-040's fallback traversal
can search other registered archives.

The read destinations in FND-CONFIG-150 are DS:5AF7
and DS:60ED. A full 980-byte first transfer would end at
DS:5ECB, just before the selector-table region. A full 98-byte
second transfer would end at DS:614F, just before the
far-base-pointer region read by FND-CONFIG-140. These are
half-open spans; they describe the supplied destinations and
source lengths, not an observed successful transfer.

Under the existing getter's indexing in FND-CONFIG-140,
slot bytes one through five address five 196-byte regions in
the first buffer. Their selector words zero through 97 occupy
98 words each. This matches the first resource's length and
the second resource's 98 selector metadata bytes. Only the
bounded selector cases in FND-CONFIG-154 are interpreted
here; no complete FNFO field identity or general input bound
is established from the matching lengths alone.

## Interpretation

There is a fingerprint-matching supported source for both
requests, with lengths exactly matching their local limits.
The archive names must remain distinct: RESOURCE.GFF's lack
of FNFO does not make these resources missing from the supported
installation. The concrete source narrows the metadata producer
question beyond an offered pointer, while runtime registration,
record selection, stability and I/O outcomes remain required
for actual population of the buffers.

## Alternatives

FND-CONFIG-157 subsequently locates an earlier OBJEX.GFF
registration attempt in the same caller. This supplies a
concrete possible archive-list producer, without establishing
the open's outcome or retention through intervening calls.
FND-CONFIG-156 shows why the following initializer's nonzero
check cannot substitute for those conditions.

Q-CONFIG-008 retains archive registration and selection at
these calls, record stability between the size and read requests,
successful transfer, later writes and reachable selectors. One
path selects these installed records and fills both buffers;
another bypasses initialization or fails before or during it.
The static catalog and matching lengths do not choose between
those states or identify the archive used by every request.

## How to reproduce

Check OBJEX.GFF and RESOURCE.GFF against the named build
manifest, then run the existing Inspect gff command and filter
its parsed catalog to tag FNFO before emitting results. Inspect
the same tag catalog in GPLDATA.GFF without treating a missing
tag there as a missing installation-wide resource. Read only
the named FNFO spans, use the initializer's destinations and
checks from FND-CONFIG-150, and compare the getter's
196-byte type stride and two-byte selector stride in
FND-CONFIG-140. Keep runtime state and successful transfer
separate from these installed-file observations.
