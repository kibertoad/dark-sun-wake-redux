---
id: FND-EXE-225
title: Descriptor-198 trampoline names the candidate entry while analyzer ownership omits and adds different bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004C8A0..0x0004C8B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087459..0x000874F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000874F5..0x0008753C
tool: Bounded source table reader and repeated Ghidra 12.1.3 PUBLIC address-set export
environment: null
---

## Observation

Descriptor 198's five resident trampolines start at shipped offsets
`0x0004C8A0`, `0x0004C8A5`, `0x0004C8AA`, `0x0004C8AF` and
`0x0004C8B4`. Their stored overlay-relative target words are respectively
`0x0149`, `0x02B2`, zero, `0x041D` and `0x0549`. Adding the declared
code base `0x00087310` places the first target at `0x00087459`,
FND-EXE-224's outer procedure candidate. This is a stored entry target,
not an observation of the overlay manager transferring control there.

The corrected snapshot's function entry `9211:0149` has a 955-byte
analyzer-owned body. Converting its complete exported ranges to shipped
offsets gives:

| Half-open interval | Bytes |
|---|---:|
| `0x00087459..0x00087478` | 31 |
| `0x000874B2..0x000874C6` | 20 |
| `0x000874D3..0x000874E7` | 20 |
| `0x00087504..0x0008751A` | 22 |
| `0x00087527..0x0008753C` | 21 |
| `0x00087F40..0x00088218` | 728 |
| `0x000950CC..0x00095131` | 101 |
| `0x00095133..0x0009513F` | 12 |

The first five intervals are precisely the 114-byte intersection with
FND-EXE-224's conditional source body. The other three contain 841 bytes
outside descriptor 198's code extent `0x00087310..0x000878CC`.
Their analyzer association does not prove this procedure reaches them.

The 225-byte conditional source body additionally contains 111 bytes absent
from that saved ownership set: `0x00087478..0x000874B2`,
`0x000874C6..0x000874D3`, `0x000874E7..0x000874F3`,
`0x000874F5..0x00087504` and `0x0008751A..0x00087527`.
Thus the comparison finds both extra ownership and missing candidate code,
not merely one incorrect scalar body size.

## Interpretation

The resident target provides concrete entry provenance beyond selecting an
arbitrary source offset. It does not establish native caller or loader
admission. The saved 955-byte body is not equivalent to the independently
decoded conditional 225-byte candidate. Inventory reconciliation must
review both missing chunks and assigned chunks beyond the descriptor,
retaining source, segment and ownership assumptions. Q-EXE-010 remains open;
no database or inventory replacement or status promotion follows.

## Alternatives

Respelling the entry without reviewing its body would retain the mismatch.
Clipping only physically non-code fragments would not address the complete
ownership comparison. Conversely, replacing the saved set with the candidate
without admitting its native binding would silently adopt an unresolved
reading. The stored trampoline target alone does not settle that binding.

## How to reproduce

At revision `ff84f66`, run the bounded table report against the installed
DSUN.EXE with XXH3 `e296af55ba2ecde7e77f555c90f33d0b`: start
`0x0004C8A0`, count five, stride five, limit five, field targetOffset at
offset two, width two. The count evidence is descriptor 198's source-derived
trampoline count from ReportFbovOverlayMap. Add each word to code base
`0x00087310`, retaining the stored-target limitation.

Use FND-EXE-174's corrected installed snapshot and the repeated address-set
exports produced by tools/ghidra/ExportResearchBaseline.java (SHA256
`28ffd9e2f7a195e9d2d6db7f522b4301f736c48cec3a5f9221dd73a1b77f4725`).
Read its row at `9211:0149`, size 955, and convert each range endpoint by
segment times sixteen plus offset, minus `0x10000`, plus the original MZ
header size `0x5200`. Require matching repeated exports and completion
markers. Compare the full interval set above with FND-EXE-224's two code
intervals using intersection and set difference, summing exclusive lengths.
All operands needed for the arithmetic are recorded above; no local comparison
script is required. Source, exports and reports stay in GAME_DIR.
