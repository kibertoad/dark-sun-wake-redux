---
id: FND-EXE-192
title: Shared-record query producers build a sixty-six-byte terminated name with a fixed pointer prefix
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006005CE..0x0060065F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006006E8..0x00600779
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00352050..0x00352072
tool: executable-reader 2.4.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The fingerprinted shipped PE maps `0x00754E50` to file offset 3481680.
The next 34 source bytes have their first zero at relative offset 33.
The preceding 33 bytes are nonzero. The suffix text is not retained here.
This is a shipped-file observation, not proof that runtime copies use
unchanged source bytes.

FND-EXE-190's first query producer copies eight successive dwords from
`0x00754E50..0x00754E70` and one word from `0x00754E70` into its
EBP-relative tail at `EBP - 0x48..EBP - 0x26`. It writes eight dwords
with repeated byte `0x41` to the adjacent prefix at
`EBP - 0x68..EBP - 0x48`. It passes the prefix address to
`0x00602540`. The local writes initialize 32 prefix bytes and 34 tail
bytes, occupying 66 bytes with no gap. If the copied suffix retains its
shipped contents, the first terminator is byte 65 relative to the query
start, after 65 nonzero name bytes.

The later producer writes a prefix at
`EBP - 0xB8..EBP - 0x98` and copies the same 34-byte suffix into
`EBP - 0x98..EBP - 0x76`. Its loop runs EDX from 31 down through zero,
writing one byte at each position. For each bit of the candidate pointer
in ESI, a set bit writes `0x41` and a clear bit writes `0x61`. Byte
position j represents bit `31 - j`. The mask doubles at dword width on
every iteration. On this normal path, the preceding fifteen-count REP
store has exhausted ECX and the local CL write sets its initial mask to
one; there is no intervening call before this loop. A fault or different
execution path is not admitted by this observation.

This registration buffer also occupies 66 initialized local bytes, with
its first terminator at byte 65 if the copied suffix is unchanged. Both
buffer intervals lie within the producer's reserved `0xAC` local bytes
below its three saved-register dwords. The prefix and tail intervals,
allocation-record extent and imported buffer capacity are distinct bounds.
The later buffer is passed to `0x00602550`; both returned atom values
are tested at low-word width as recorded in FND-EXE-190.

FND-EXE-191's recovery alphabet reverses this locally generated prefix:
it sets a recovered bit for `0x41` and leaves it clear for `0x61`.
Its capacity argument is 66, but the initialized extent of its returned
buffer depends on the admitted identifier, returned name and callee effects,
not just these producer layouts.

## Interpretation

This closes the producer-side prefix and terminator layout for the two
local queries. SRC-WIN32-ATOMS's external case-insensitive lookup and
first-registration case-preservation contract predicts that an unchanged
suffix can connect the all-`0x41` lookup with a generated pointer prefix.
That is an inference from the external contract, not native evidence of
which name or identifier was admitted. Q-EXE-001 and Q-EXE-010 retain
runtime suffix writers/aliases, existing atom provenance and mutation,
import argument/preservation effects, initialized retrieval extent and
record ownership/lifetime. No formal complete reading follows.

## Alternatives

A suffix-sized buffer alone would not bound the prefix. The explicit
32-byte producer and adjacent 34-byte copy account for the whole local
query. Conversely, a known generated name cannot be substituted for every
existing atom with a matching lookup: its identity, retained case pattern,
mutations and associated record lifetime must still be established.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
The pinned executable reader's PE32 mapping API maps virtual `0x00754E50`
to file offset 3481680. Bound the source query to exactly 34 bytes and
confirm first zero at index 33, excluding its text from durable output.
Use the source-derived section map, not a supplied VA-to-file delta, and
verify the fingerprint before reading. The file-data interval is half-open.

In the saved shipped-PE project, read-only with analysis disabled, run
ReportDataBytes at `0x00754E50`, count 34, as a bounded analyzer cross-check.
Run ReportInstructionWindow at `0x006005D6`, count 55, and at
`0x006006C6`, count 60; combine the former with FND-EXE-190's entry
window that records the first suffix load at `0x006005CE`. Restrict the
producer reading to the declared locations and the already-recorded normal
path. Follow every copy width and both EBP-relative interval ends.
ReportReferences at `0x00754E50` and `0x00754E70` supplies positive copy
leads only; an empty or partial list does not prove runtime immutability.
Rich reports and the suffix bytes stay in the local licensed-source store,
outside Git. No original-game execution is involved.
