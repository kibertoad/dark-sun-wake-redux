---
id: FND-EXE-408
title: Controlled link-field search exposes a writer that retries mutable records before publishing three outputs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:08E4..1425:09AD
tool: scientific-method-engine 15.0.0, executable-reader 2.5.0, Capstone 5.0.7
environment: null
---

## Observation

A literal-39CE search over FND-EXE-402's two resident regions returns
28 entry-path memory candidates, three other operands and seven rejected
overlapping decodes. All memory candidates use DS. It scans 9579 byte
starts without truncation or unsearched starts and retains eighteen interrupt
boundaries and 25 unresolved computed transfers. These model classifications are
not a complete native writer census or proof of segment identity.

The memory candidate offsets in modeled segment 1425 are 0857,
08A0, 08A9, 08B8, 08C2, 0928, 0980, 0A41,
0AA5, 0AF4, 0B0B, 0B4B, 0B70, 0D1A, 0D45,
0E62, 0E95, 0F3E, 0F5E, 122B, 1296, 12B6,
1332, 156B, 1A25, 1AE6, 1BDC and 1C9D. Writes
are reported at 0857, 08B8, 08C2, 0980, 0AA5,
0B0B, 1296 and 156B; the others are reads. FND-EXE-407
independently supplies read/write controls at 0857, 08A0 and 08B8.

The first additional writer, 0980, belongs to the local body starting
08E4. It saves BP and SI, allocates four local bytes and checks
the word through incoming far pointer SS:BP+6. Nonzero returns AX
nine without calling its helper or publishing outputs. Zero takes current
DS:00CA into SI and calls 060B with SI and far addresses
SS:BP-2 and SS:BP-4. It removes ten outgoing bytes and copies
returned AX into DX. Nonzero DX selects final return without later
publication. The helper's output and preservation contracts remain unread here.

After zero, the two current local words form a word-width indexed
address: BP-2 times 0108 plus BP-4 times four, added to
39CE in current DS. Word one there selects publication. Other words
increment SI and compare it unsigned with current DS:00C6. Above
returns AX ten; otherwise the body writes FFFF to DS:39CA
indexed by current BP-2 times 0108 and repeats the 060B request.
It returns that new nonzero result or repeats the indexed test after
zero. The FFFF store precedes a possibly failing request and is not
locally undone on that failure.

No local SI or DS save surrounds either request, and local output
words are re-read. The initial candidate is tested before the first
upper-bound comparison. SI increments at word width and 00C6 is mutable;
the comparison alone is not an unconditional retry bound. With stable
00C6=FFFF, word increment cannot produce an unsigned value above the
limit; termination then requires a matching link or a failing helper.

Publication first writes one to DS:39C4 indexed by current BP-2,
then zero to the 39CE word formed from both current local outputs.
It writes SI through incoming far pointer +6, current BP-2 through
incoming far pointer +0A and current BP-4 through incoming far pointer
+0E, in that order. Each pointer and local word is reloaded for
its store. Writable aliases can therefore change later outputs and indexed
destinations; this is not atomic publication or an admitted disjoint-output contract.

It then writes SI plus one at word width to current DS:00CA,
increments current DS:00CC and returns DX in AX. On the locally
admitted publication path DX remains the tested zero helper result; no
additional call intervenes. All exits restore saved SI and discard the
frame. The complete body covers 201 bytes in 79 instructions, ending
with the far return at 09AC.

## Interpretation

This supplies a further writer and concrete retries/output producers for
the link/table dependencies of FND-EXE-407. Q-EXE-007 retains 060B's
full behavior, input/caller completeness, other candidate writers, computed/aliased
accesses, DS/storage admission and preservation. No complete reading, invariant
link shape or unconditional termination is established.

## Alternatives

Treating all raw matches as instruction-owned accesses would retain rejected
overlaps. Treating the limit test as a precondition on the initial candidate
changes its order. Assuming preserved SI or stable 00C6 invents missing
contracts. Treating failure as rollback removes the earlier FFFF store.
Treating the three outputs as unchanged local snapshots ignores their reloads
and possible aliases.

## How to reproduce

At revision fd0b9113 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Use FND-EXE-402's
exact two regions, entry seeds, instructionLimit 20000, scanLimit 10000
and result limit 100 for reader-backed x86-operand-candidates. Change query
offset to 14798 and controls to shipped-file offsets 40103, 40176
and 40200. Keep all classifications, caps, exclusions and gaps separate.

Decode file base 0x9450 plus 0x08E4 through exclusive 0x09AD
in sixteen-bit mode. Require full 201-byte coverage and 79 instructions.
Follow both 060B calls, current local outputs, unsigned retry test,
pre-failure store, ordered publication, counter writes and each returned word.
Use FND-EXE-407 for independent controls. No negative whole-program writer
claim is made; licensed reports and bytes remain outside Git. No original
game, DOSBox or emulated call runs.
