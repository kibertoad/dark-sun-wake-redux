---
id: FND-EXE-207
title: Physical gate-overlap start search excludes neighboring contiguous address encodings within an eight-byte write envelope
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0060086B..0x00600870
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FCD56..0x005FCD5B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FCE58..0x005FCE5E
tool: ReportPhysicalBytePattern.ps1 and executable-reader 2.4.0 sourceXxh3
environment: null
---

## Observation

The four-byte gate occupies `0x0242C910..0x0242C914`. For a contiguous
write of width W, overlap requires its start to be no earlier than
G minus W plus one and no later than G plus three, where G is the
gate start. The union for widths one through eight is consequently
the eleven integer starts `0x0242C909..0x0242C913`, inclusive.
This is an explicit eight-byte search envelope, not a bound on every
write the original can perform.

After verifying source length 3802624 and XXH3-128
`09861838aa3018346f9f15c9a4f5925c`, independent whole-file physical
searches at every four-byte encoding start found no contiguous
little-endian address value for ten of those eleven candidates.
Only `0x0242C910` matched, at seventeen physical offsets. Comparing
the complete offset set with FND-EXE-076's recorded table gives no
difference. No query exceeded its 128-result cap.

The independently located bitmap control `0x02427E40` and counter
address-taking control `0x02427E50` each matched five times, as in
FND-EXE-076. Those controls exercise the full four-byte address-value
representation being searched. They do not validate a write width,
operand direction, instruction boundary or execution path. FND-EXE-206's
separate decoded numeric query recovers the seventeen known gate loads,
including the recovered reading in FND-EXE-077, but it is not used to
exclude undisassembled file bytes from this physical scan.

## Interpretation

For this unchanged shipped file, a partial gate write within the
stated width envelope cannot be explained by an additional neighboring
start encoded as one contiguous absolute 32-bit value in the file.
This narrows a representation, not all gate writers. Q-EXE-009 retains
computed or indexed effective addresses, aliases through other pointers,
relative or split address forms, wider and variable-length writes,
loaded or generated code/state, stack overlap, external writers and
actual gate initialization. The seventeen exact-address occurrences
remain the same candidates; an address occurrence itself is not a store.
No complete writer set, pointer validity or complete reading follows.

## Alternatives

An exact-start search alone would omit partial overlaps beginning before
or inside the gate. Conversely, an eight-byte envelope does not cover
a wider write beginning earlier, and absence of an absolute address
encoding does not exclude an address formed from a base and displacement.
The physical scan is independent of decoded function boundaries but
cannot classify its matches as code or admitted accesses.

## How to reproduce

Verify the source with executable-reader's sourceXxh3 applied to its
complete bytes and compare with the identity above. Run
ReportPhysicalBytePattern.ps1 on that source once per integer value
in `0x0242C909..0x0242C913`, inclusive, plus independent controls
`0x02427E40` and `0x02427E50`, with MaximumMatches 128. Derive each
four-byte Pattern by little-endian encoding of the named value.
Search every physical position with four remaining bytes, including
unaligned positions, headers, section contents and trailing file bytes.

Require the two control counts to be five, the gate count seventeen,
and the other candidate counts zero. Sort the gate's offsets and
compare them with FND-EXE-076's full table; the difference is empty.
Keep encoding, write width and decoded access classification separate.
Use FND-EXE-206 for the independent numeric listing result. Keep rich
reports local and execute no original program.
