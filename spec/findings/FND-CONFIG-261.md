---
id: FND-CONFIG-261
title: Setup source conversion accepts a signed decimal prefix and returns wrapped DX AX while its card caller retains AX
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2713..1000:2788
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D1FB..0x0001D2FB
tool: Ghidra 12.1.3 ReportFunctionSummary.java; resident_listing.py; retained report.mjs table from FND-SOUND-020
environment: null
---

## Observation

FND-CONFIG-260 leaves1000:2713's conversion grammar open. Its complete
117-byte body has no child call, interrupt or port access. This reads that
body and reuses the exact classification table from FND-SOUND-020.
SOUND_DS.EXE is204593 bytes, XXH3-128236c2dc23c071eca421eb5b427caee57.
Addresses and register words below are hexadecimal; ordinary arithmetic
values and character codes are decimal.

### Prefix recognition and character classification

The routine takes one far string pointer, saves SI/DI/ES/BP and explicitly
clears direction. It zeroes AX and DX, keeps BH zero and reads characters
into BL through ES:SI, incrementing only the offset. Leading characters
are skipped while bit1 of the byte at DS:DA9B+unsigned-character is set.
The verified256-byte table has that bit set exactly for codes9..13 and32,
not for NUL or high bytes. Its data are reused unchanged; no new table
contents are inferred from conventional C whitespace rules.

After leading whitespace, one plus or minus is consumed if present.
Whitespace after that sign is not skipped. The accepted digit loop is
explicit ASCII0 through9, independent of the classification table. It
stops at the first other character; that character has already been read
and the internal cursor advanced. The saved SI/ES are restored on return,
and there is no end-pointer output or full-token validation.
An empty prefix, sign without digits, or initial nondigit therefore leaves
the accumulator zero. There is no hexadecimal-prefix or octal-base arm.

### Two-word accumulation and overflow

The first digit path multiplies AX by10 into DX:AX, adds the digit to AX
and incorporates its carry into the small high word. It stays on that
path only while the high part is zero. Once a nonzero high part occurs,
the second path multiplies the low word by10, combines its high product
with ten times the retained high word, and adds the next digit with carry.
Only the low two result words are retained. Repeated digits thus accumulate
modulo2^32; no range rejection, saturation, error assignment or child result
is present in this body. The small-product byte operation in the first
path is safe as that path's product high word is at most9 before digit carry.

For a minus sign, the exit negates DX and AX and subtracts the low-word
borrow from DX, producing the two-word two's complement. It restores the
saved registers and returns far with the converted result in DX:AX.
The inferred Ghidra return type is only uint and hides DX's significance;
the multiplication, exchange, carry and paired-negation instructions decide
this result width. No return-type inference is used to discard the high word.

FND-CONFIG-260's four caller sites store only AX after return. Their actual
retained value is therefore the signed-prefix result modulo65536, followed
by the first-field remaps already recorded there. These are derived examples
of the instruction arithmetic, not native-run or synthetic-execution results:

| Source string | Returned DX:AX | Card caller's retained word |
|---|---|---|
| 65535 | 0000:FFFF | FFFF |
| 65536 | 0001:0000 | 0000 |
| -1 | FFFF:FFFF | FFFF |
| 4294967296 | 0000:0000 | 0000 |
| 0x220 | 0000:0000 | 0000 |
| 220suffix | 0000:00DC | 00DC |

The last example stops at the first letter; in card7C's first field its
retained word then meets the existing00DC-to0220 remap. No suffix rejection
is added by the converter. The caller still needs its comma/NUL scans,
local-buffer bounds and source/frame identity admitted independently.

## Interpretation

This removes the local conversion-spelling, prefix, sign and overflow
algorithm gap for card7C's four2713 calls from Q-CONFIG-005. It does not
admit arbitrary source lengths, changed classifications or all other
numeric routines in setup. The word passed to a driver or persisted later
cannot be inferred from source spelling alone without the caller's low-word
storage and remap. FMT-CONFIG-006 remains supported, with no whole-entry
complete reading or native device result claimed.

## Alternatives

Treating the decompiler's uint as the whole result loses the high-word
path. Treating it as a checked standard integer parser adds behavior absent
from the body. Treating numeric zero as a failure sentinel conflates empty,
nondigit, valid zero and wrapped results. Changed table/source/segment
bindings or a nonterminating string are outside this admitted reading.

## How to reproduce

The verified SETUP-blocks project under
GAME_DIR/analysis/config-block-construction-20261108/ retains
source-word-conversion-ghidra.log from packaged ReportFunctionSummary.java
for1000:2713. Regenerate source-word-conversion-instructions.txt with
resident_listing.py against the approved executable,1000:2713..1000:2788.
Read the two arithmetic paths, carry/negation instructions and saved-register
restoration rather than relying on the inferred decompiler signature.

Reuse FND-SOUND-020's256 stride-one classification rows from file1D1FB,
bounded by BH zero and the input byte. The retained setup-case-repeat.json
under GAME_DIR/analysis/disc-adv-survey-20261104/ has SHA-256
1ceb2e4574f31a486f808054091bc4c1afcbea6772b1da72ee8850a2c5e4d1fc;
selecting rows with bit1 yields exactly the codes above. All original-derived
data and code remain local. No original executable or emulated call was run.
