---
id: FND-EXE-259
title: Buffer helper selects external transfer requests and truncates the direct word count to sixteen bits
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:11A9..4AE5:11E0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:11E1..4AE5:1208
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1209..4AE5:1256
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-258's direct callee at `4AE5:11A9` contains sixty instructions
and 171 bytes across the three intervals above. Two one-byte gaps are
excluded. It saves DS and ES, adds one to SI with carry into DI modulo
the two-word width, clears SI's low bit and clears direction. It then
loads DS through CS-relative word five and tests state word `0x0047`
against zero. A zero transfer length does not have a separate entry exit;
wrapping length plus one can also produce zero.

The nonzero-word path stores rounded SI/DI to state words `0x004D`/
`0x004F`. It tests only DL against `0x10`, not all of DX. At least that
byte value selects state word `0x0047` for word `0x0051` and subtracts
state pair `0x0049`/`0x004B` from AX/DX at component widths with borrow.
The other arm stores zero to word `0x0051` and rotates the entire DX
word right four. Both store AX/DX to words `0x0053`/`0x0055`.

It independently applies the same low-byte comparison to CL. At least
`0x10` selects state word `0x0047` for word `0x0057` and subtracts
pair `0x0049`/`0x004B` from BX/CX. Otherwise it writes zero to word
`0x0057` and rotates all of CX right four. It stores BX/CX to words
`0x0059`/`0x005B`, sets AH `0x0B`, SI to `0x004D` and makes a far
call through current DS-relative pointer `0x0043`. No independent pointer
nonzero or validity check occurs on this path. Returned AX nonzero
selects the carry-clear success exit; zero selects the carry-set failure
exit. Request storage, pointer identity and external effects remain
unadmitted native inputs/contracts.

The zero-word path instead shifts DI right one and rotates SI right
through that carry, halving the rounded two-word length. It stores AX to
state word `0x005F` and only DL to byte `0x0061`, BX to word `0x0067`
and only CL to byte `0x0069`. These stores do not initialize adjacent
high bytes or the rest of the request structure. It moves only SI to CX,
sets SI to `0x004D`, copies DS to ES through a push/pop, selects AH
`0x87` and interrupts through vector `0x15` at `4AE5:1248`. The upper
half of the halved count in DI is not passed in CX. Thus the explicit
request count is the low sixteen bits of the word count, not an
independently admitted full-length transfer. Direction stays clear in
the explicit body, while external effects remain separate.

Interrupt carry clear selects the success exit; carry set selects failure.
Success clears AX, thereby clearing carry. Failure forms AX `0xFFFF`
from set carry and explicitly sets carry again. It restores saved ES
and DS and returns near without additional argument cleanup. Restoration
depends on saved-storage integrity and does not prove either external
operation preserved registers or completed the requested writes.

FND-EXE-258 sets SI/DI to `0x0020`/zero before its call. Conditional
rounding retains 32 bytes; the direct request therefore carries sixteen
words. The caller's source AX/DX is zero/`0x0010` and destination BX/CX
is formed from SS times sixteen plus its local-buffer offset. The
nonzero-state path can classify those pairs by their low high-word bytes;
native pointer and handle admission is not supplied solely by this setup.
On carry clear the caller reads local bytes and words; those reads still
need initialized-extent and alias evidence from the actual external
contract, not the helper's accepted-request status alone.

## Interpretation

This closes the helper's immediate request construction and return flags,
while separating two external output mechanisms. Count rounding, word
conversion and low-word truncation are distinct operations. Partial-byte
request stores require their own prior-byte writers and consumers. The
caller remains below complete reading until its local buffer's actual
initialized output and every effective binding are established.

Q-EXE-001 and Q-EXE-010 retain all request-field and pointer writers,
external output/register contracts, input and independent output bounds,
argument/address classification and saved-stack/storage aliases. No
complete_reading or replacement inventory is established.

## Alternatives

A full-width word count passed to the interrupt is contradicted by moving
only SI into CX. Testing the whole high word for address classification
ignores the byte operands DL and CL. Shifting those high words instead
of rotating changes the explicit conventional-address arm. Treating a
carry-clear result as proof of initialized local bytes confuses external
acceptance with output admission.

## How to reproduce

At revision `013a2be`, use FND-EXE-236's original-source identity, region
and default x86-bounds limits. Set entry and sole entries to `0x000411F9`,
with no seeds or summaries. Check intervals `0x000411F9..0x00041230`,
`0x00041231..0x00041258` and `0x00041259..0x000412A6`, sixty
instructions, computed far call `4AE5:1220`, interrupt `4AE5:1248`
and near return without cleanup. The traversal assumes the call and
interrupt return; complete CFG is not a Standard complete reading or
external output proof.

Decode the intervals directly from the shipped source in sixteen-bit
mode with Capstone. Follow rounded count widths, byte classification,
word rotation, partial stores and both flag/result exits. Compare
FND-EXE-258's request and local consumers separately, retaining native
output admission. Keep source, configurations and reports in GAME_DIR.
