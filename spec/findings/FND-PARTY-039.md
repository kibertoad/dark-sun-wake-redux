---
id: FND-PARTY-039
title: Most pointers behind the indirect calls before the party-loader gate hold fixed routine lists, two are never set, and the record fields stay unread
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:018F..172C:01C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:612C..1BF3:61AE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0418..39D1:0460
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:08E1..3D72:08FB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AB9:0007..4AB9:001C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4BD8:0010..4BD8:0029
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000794E1..0x000794FC
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/store_values.py, segment_references.py, immediate_search.py, direct_callers.py, trampoline_target.py, resident_listing.py, overlay_listing.py)
environment: null
---

## Observation

FND-PARTY-037's `reach` leaves 61 indirect calls unresolved (60 from FND-PARTY-031 and the
Ctrl-Break call). For each, the pointer it reads was looked up as follows: its start value in
the load image's data group (`DS` = `57E0`, file offset `0x4D000` + offset) or code segment,
every instruction that writes it through a displacement with no base or index register (with
`--indexed` for tables), at every byte of the load image and of every overlay's code, and, for a
write of an argument, the direct callers of the writing routine and the values they push. A
resident segment word counts only with an MZ relocation, an overlay one only with an FBOV
fixup. Values are given as resident addresses, or as overlay `descriptor+offset` after
resolving trampolines.

| Calls (file offsets) | Pointer | What it holds |
| --- | --- | --- |
| `0x5455`, `0x545C`, `0x5496`, `0x549D` | start-up and exit records at `DS:3994..39C4`, read by `1000:0220` and `1000:0264` | the record targets `4AE5:0D27`, `1000:0886`, `1000:11C1`, `1000:12D9`, `1000:2877`, `1000:29AA`, `1000:3B56`, and at exit `4AE5:0193` |
| `0x5546` | `DS:A4C6` | overlay 180 `+0848`, the Ctrl-Break handler (FND-PARTY-037) |
| `0x55A1`, `0x55B1`, `0x55C9`, `0x55CD` | `DS:A446` table, `DS:3676`, `DS:367A`, `DS:367E` | read only as the program ends (FND-PARTY-032); the three words start as `1000:0387`, a lone `retf` |
| `0x5C37` | the putter argument of `1000:09F4` | `1000:0FB5`, `1000:345B` or `1000:3726`, pushed by its five callers |
| `0x7110`, `0x71BC` | a stub built on the stack | interrupt `0x21` or `0x2F` through `1000:1EB4`; the game sets vectors `0x23`, `0x24`, 0, 4, 5, 6, 9 and `0x3F` and no other through interrupt `0x21` function `0x25` |
| `0x772C` (`jmp`) | `DS:398C` | `1000:129F` from the start value; nothing writes it |
| `0x9A3C`, `0x9B83`, `0xA04A`, `0xA09A`, `0xA219`, `0xAAA1`, `0xAAFB`, `0xAEF1`, `0xB0A8` | 14-byte records from `DS:3F42` | written only at file `0x95D3..0x9944`: `1425:00DB`, `1425:00AF`, `15F3:0360`; `1425:0009`, `1425:0044`, `1425:007F`; `1425:02E1`, `1425:02FE`, `1425:031B`; `1425:041A`, `1425:0437`, `1425:0454` |
| `0xB704`, `0xB79C`, `0xB7C9` | `15F3:00B8`, read with DS set to CS | the address interrupt `0x2F` function `0x4310` returns, stored at `15F3:00DA` with DS set to CS, the XMS driver's entry |
| `0xC667` | `DS:02F6` | 0 at start, tested before the call; written only by overlay 188 `+0D1A..0D26` with `2D40:37F3` |
| `0xC675` | the 129-word table at `DS:030A` | 115 distinct routines in segment `172C`; no store has the displacement `0x030A`, direct or indexed; the index is the byte argument, at most `0x80` |
| `0x16E3E` | field `+0x0C` of the record at `ES:SI` | not enumerated |
| `0x172C9` | the 10 words at `1BF3:61B8`, read with DS set to CS | `1BF3:624E`, `6227`, `626E`, `628E`, `61CC`, `61DB`, `61F8`, `6207`, `6216`, `62C2`, chosen by the format letter at `1BF3:61AE` |
| `0x172D0` | `1BF3:1154`, read with DS set to CS | written at `1BF3:615B` on the same path with CS and `0x59C1` or `0x5E12` |
| `0x2F696`, `0x2F6DC`, `0x2F74B`, `0x2F7A9`, `0x2FA65` | `DS:A0F1`, `DS:A0F5` | 0 at start and set to 0 by `39D1:0009` at `39D1:02E4` and `39D1:02ED`; set by `39D1:0418` (A0F1) and `39D1:0430` (A0F5) to their far argument; see below |
| `0x2F712`, `0x301E7`, `0x30397`, `0x32A4A`, `0x32EB0`, `0x332E7`, `0x334F7` | fields `+0xF5`, `+0xFD`, `+0xF9`, `+0x70`, `+0x62`, `+0x5E`, `+0x5A` of the record at `ES:BX` | not enumerated |
| `0x336C1`, `0x336F0` | `DS:A119` | 0 at start, set to 0 by `39D1:0009` at `39D1:0272`; each call is skipped while it is 0; its only other writer, `3D72:08E1`, has no direct call and no reference to its segment or offset |
| `0x3C6D9`, `0x3CB07`, `0x3F57D`, `0x3F6ED` | `DS:3486`, `348A`, `348E`, `3492` | written only at file `0x1B9D9..0x1BA09` with `2660:052D`, `2660:05CA`, `2660:0250`, `2660:0447` |
| `0x3F007` | `DS:3475` | written only by `49DE:00A7` with its argument; its only direct call, at overlay 180 `+007B`, passes trampoline `56B2:0048`, overlay 180 `+0000` |
| `0x3F071` | `DS:3479` | written only by `49D2:0006` with `49D2:0017`; called from overlay 180 `+0070` |
| `0x3FDBB`, `0x3FDE2` | `DS:3471`, `DS:346D` | 0 at start; each call is skipped while it is 0; the only writer, `4AB9:0007`, has no direct call and no reference to its segment |
| `0x40FA5` to `0x41270` (9 calls) | `DS:0043` | the XMS driver's entry, from interrupt `0x2F` function `0x4310` at `4BD8:0016..0023` |

**The A0F1 and A0F5 writers.** `39D1:0418` is called with 0 at overlay 184 `+1235`, and with a
trampoline at overlay 184 `+08E3` (`56DD:0043`) and overlay 199 `+06B7`, `+07D6`, `+0855` and
`+0B31` (`576C:003E`, `576C:004D` twice, `576C:0052`). Overlay 190 `+11A1` stores its far
argument at `DS:61A2` and passes it to `39D1:0418`. It is called with trampolines from overlay
190 `+378B` (`5713:004D`), 171 `+01B0`, 172 `+03B7`, 175 `+01D8`, 181 `+04FF`, 192 `+02CE`, 194
`+04D1`, 201 `+00BE`, 202 `+036D`, 203 `+00BA`, 209 `+12C3`, 211 `+03F0` and 212 `+06D3`, with
`28C9:0CFF` from overlay 182 `+11EF`, `+1751` and `+1AE1` and overlay 199 `+0BEE`, with 0 from
overlay 182 `+1596`, `+167B` and `+19B9`, and at overlay 172 `+0479` with the value overlay 190 `+11C1` (trampoline `571F:0039`)
returned earlier, the word pair at `DS:61A2`, which only overlay 190 `+11A1` writes. The trampolines lead to overlay 184 `+0DA7`;
199 `+0053`, `+0D75` and `+102C`; 189 `+0559`; 171 `+03C5`; 172 `+04C5`; 175 `+0B61`; 181
`+0000`; 192 `+0785`; 194 `+0000`; 201 `+0000`; 202 `+05CA`; 203 `+00EE`; 209 `+169C`; 211
`+081B`; 212 `+0967`. `39D1:0430` is called with 0 at twelve sites and with trampoline `571F:0057`,
overlay 190 `+0092`, at six. `39D1:0448` stores `DS:A0F9`, which none of these calls reads,
with 0 or `28C9:152C`.

**The table at `DS:030A`.** `172C:018F` calls `DS:02F6` when it is not 0, with the byte
argument, then calls the table word the byte selects, when the byte is at most `0x80`. Its
direct callers are `172C:00CC` and `172C:3358`, both with a byte that `172C:20F5` reads. FND-CONFIG-136
and FND-SCRIPT-007 read `172C:018F` as the script interpreter's opcode dispatch.

## Interpretation

Of the 61 calls, 49 read a pointer that holds one of the listed routines or the XMS driver's
entry, or is 0 when read, and four run only at exit. The table at `DS:030A` holds the script
opcodes' handlers, so which of them run depends on the script being run. The 8 calls through
record fields at `ES:BX` and `ES:SI` are not settled by this reading. The A0F1 values come from
calls in many overlays; whether each runs before the gate is a question of reach, read in
FND-PARTY-040.

## Alternatives

- A pointer is written through a computed address (`rep movs`, a pointer register, or a segment
  other than the one named): not searched for; the search covers displacement stores only.
- The game hooks interrupt `0x21` or `0x2F` without function `0x25`: the search for direct
  stores to `0000:0084`, `0000:0086`, `0000:00BC` and `0000:00BE` with an `es:` prefix found
  none; other forms were not searched for.
- A setter with no direct call is reached through a pointer: ruled out for `3D72:08E1` and
  `4AB9:0007` by the absence of any non-call reference to their segments and of the offset
  `0x08E1` as an immediate.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `store_values.py <dsun> 398C 3F42
3F44 3F46 3F48 3F4A 3F4C 02F6 02F8 1154 1156 A119 A11B 030A 030C 61B8 61BA A0F1 A0F3 A0F5 A0F7
0043 0045 346D 3471 3475 3479 0111 61A2 61A4`, the same with `--indexed` and `3F42 3F44 3F46 3F48 3F4A
3F4C 398C 398E 61B8 00B8 00BA 030A A446 A448 3486 3488 348A 348C 348E 3490 3492 3494`;
`direct_callers.py <dsun> 3A10:0028 3A10:0040 3A10:0058 49E7:0017 4AB8:0017 49D2:0006
3DFF:0011 190+11A1 172C:018F 2200:005C`; `segment_references.py <dsun> 3D72 4AB9 3DFF 4AB8
3E01`; `immediate_search.py <dsun> 08E1`; `trampoline_target.py` on each trampoline named; and
`overlay_listing.py` on 15 bytes before each call named. Start values are the words at file
offset `0x4D000` plus the offset, and for `1BF3` at `0x11130` plus the offset.
