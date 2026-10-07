---
id: FND-EXE-122
title: Optional first-mode slot scan clears byte and masks before its selected call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2110..0x004F2237
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2310..0x004F233C
tool: Ghidra 12.1.3 PUBLIC bounded optional first-mode scan reading
environment: null
---

## Observation

FND-EXE-104 records the admission gates and optional call to
`0x004F2110`; FND-EXE-121 records two publishers for its mask and slot
bytes. The consumer saves four registers and reserves twelve bytes. Its
prefix rejects absent mask two in byte `0x0075B205`, zero full
`0x01D271C8`, or callback `0x0075B0D0` equal to `0x004A06D0`.
It zero-extends the word at `0x01D271C0`. Word 255 is replaced by
sixteen; every other word is retained as an index. It then zero-extends
the word at `0x0070DD00` plus twice that index into a retained scan
count K. The prefix supplies no other local bound on this table index.

The following observations concern only byte `0x01D292A0` equal to
zero. Nonzero takes a separate scan mode whose contract remains open.
K zero exits; otherwise the candidate position starts at zero. Each
candidate loads a full value N from `0x0070DCC0` plus four times its
position. It skips N if byte `0x01D291F0` plus eight times N is
nonzero or byte `0x01D291F1` plus eight times N is zero. Both slot
reads precede the unsigned comparison with seven. N at most seven passes
that comparison; a larger N passes only when byte `0x01D29200` is
zero. No local comparison bounds N to fifteen. Skipping increments the
position and continues only while the retained K is greater, unsigned.
K's word width bounds these local iterations, not N or the backing extent
of either table or slot storage.

For an admitted candidate it clears byte `0x01D291F1` plus eight
times N first. It forms a full clearing mask by rotating `0xFFFFFFFE`
left by N's low byte, with the thirty-two-bit rotation count reduced to
its low five bits. It reads full `0x0075B200`, then ANDs full
`0x01D271C8` and full `0x01D271C4` with the clearing mask, in
that order. Only then it reads the full slot field at `0x01D291F4`
plus eight times N. At `0x004F21E6` it calls `0x00411A30` with
that slot field, full zero, and the retained `0x0075B200` value as
its three outgoing inputs. All three local clearing stores precede the
call. The first mode does not locally require bit N of the admitted
nonzero mask to be set before selecting this candidate.

After an ordinary return it does not test the callee's EAX. It derives
G from bit three of N, so G is zero or one even without a bound on N,
and reads current byte `0x01D2927D` plus twenty times G. Zero
publishes full N to `0x01D271C0`, then byte one to
`0x01D291F2` plus eight times N, then returns full EAX one.
A nonzero first group byte instead reads current byte `0x01D2927E`
plus twenty times G. If this second byte is zero it returns full EAX one
without those two selection-publication stores. If both group bytes are
nonzero it supplies pointer `0x0072BC60` to the shared transfer at
`0x0058F890`, called at `0x004F233C`. This bounded reading stops
at that call; it does not claim an ordinary continuation beyond it.
Neither post-call group gate locally restores the cleared byte or masks.
Early admission exits and exhausted scans do not share the selected
path's explicit full EAX one assignment.

## Interpretation

The first mode is a table-ordered slot-byte scan with a pre-call clearing
sequence and fresh post-call group decisions. An initial nonzero mask admits
the scan but does not itself identify a locally tested candidate bit. The
word count bounds the loop, while candidate and initial table-index safety
still depend on unread producers. Q-EXE-009 in FMT-EXE-006 remains open for
the second scan mode, table and slot producers, selected callee effects,
shared transfer effects, lifetime and caller/native admission. This is not
a complete reading or an established description of actual PATH behavior.

## Alternatives

- A nonzero admitted mask is not a per-candidate bit test on this path;
  the two slot bytes and high-input gate decide local admission.
- The K bound does not prove N lies in zero through fifteen: slot bytes
  are already read before the comparison with seven.
- Returning one does not prove selection publication: the first group
  byte nonzero and second zero path omits both selection stores.
- Clearing occurs before the selected call, and no later local gate
  rolls it back. Unread callees may have effects beyond these local stores.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's six physical
controls. FND-EXE-104 independently supplies the direct consumer target.
Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x004F2110` limit 160. Restrict this
reading to the declared first-mode ranges, following its explicit branches
and excluding the second mode and following function. Track word extension,
unsigned comparisons, the alternate index-sixteen edge, candidate stride,
slot reads before bounds, rotation width, clearing order, outgoing inputs
and fresh group reads after the call. Follow the skip back-edge and both
post-call group exits. Stop the shared transfer path at its direct call;
the reported following instruction gap is not proof of its return contract.
No original-program run or emulated call is part of this observation.
