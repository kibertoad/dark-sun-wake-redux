---
id: FND-EXE-572
title: From its established entries the sound utility reaches 44 of its 69 interrupt 21 sites, none selecting AH=4B, and the other 25 lie in routines nothing in the image references
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0000..1000:E360
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0652..1000:06A5
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:02C2..1000:02FF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:DD52..1000:E180
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:040E..1C08:04C0
tool: scientific-method-engine 19.2.0 (reach, Capstone 5.0.9) and xxhash 4.0.1 (tools/research/exec-census/sound_ds_reach_config.py, sound_ds_reached_ah.py); Capstone 5.0.9 for the reference searches
environment: null
---

## Observation

Addresses are load-image addresses with the image at segment `0x1000`, file offset
`0x1400 + (segment - 0x1000) * 16 + offset`, as FND-EXE-490 writes them. The program's data segment
is `1E36` (the startup code loads it into DS); the code is `1000:0000..1000:E360`.

**Regions.** Each paragraph a relocated far call names is a module's frame. A module starts inside
its frame's first 16 bytes, at the smallest offset a far call or a near call from inside the module
names: `1861:0008` follows the `retf` at `1861:0007` that ends the module of `17FB`, and near calls
name `1429:0006`, `14A2:0009`, `164C:0008`, `1783:0007` and `1DB2:0008`. With 25 such regions the
walk decodes no byte outside them and reports no gap.

**Entries.** The starts are:

1. The MZ entry point `1000:0000`.
2. The four startup records at `DS:DF1C..DF34`, read by the record walker that `1000:012F` calls
   with those bounds (six-byte records: a type, 0 for a near routine in segment `1000`, a priority
   and a pointer): `1000:0E0F`, `1000:1766`, `1000:190A` and `1000:2619`. The exit walker `1000:0231`
   is called once, at `1000:0164`, with `DF34..DF34`, so there are no exit records.
3. The interrupt 0 handler `1000:0174`, installed by the AH=25 request at `1000:01BC`. The other
   four AH=25 requests in reached code restore the vectors kept at `[005B]`, `[005F]`, `[0063]` and `[0067]`; the fifth, in
   `1DB2:0473..04A7`, is not reached.
4. The interrupt 8 handler `1C08:040E` and the routine `1C08:051B`, which `1C08:0540..0562` store
   into the vector at `0000:0020` and into `CS:0048`. These two stores are the only writes to
   the vector table that an instruction decoded at any byte offset of the code names.

**Computed transfers declared.** No instruction decoded at any byte offset of the code names
these operands as a displacement except those listed:

- `jmp [DF14]`, `[DF16]`, `[DF18]` and `[DF1A]` at `1000:2298..22A4`: the four words hold
  `18D0`, `18D5`, `18D5` and `18D5` in the image and nothing else names them.
- `call far [DB9C]`, `[DBA0]` and `[DBA4]` at `1000:0329..0345`: the three hooks hold the
  relocated pointer `1000:02FF`, a lone `retf`. `1000:38AC..38B7` stores `1000:3CCD` into
  `DB9C`, and `1000:2AD6..2AE1` stores `1000:3C90` into `DBA0`.
- The calls through `[bp+4]` and `[bp+6]` in `1000:072E` and `1000:0C4D`: `072E`'s only callers,
  `1000:39DF` and `1000:3A03`, push `3991` and `39B9`; `0C4D`'s only caller is `072E`, which passes
  its own; neither routine writes either slot.
- The call through `[bp+0E]` at `1000:0F6C`, in the helper `1000:0F55` that runs in the frame of
  `1000:0F25`: `0F25`'s only callers, `1000:1679`, `3353`, `396B` and `398C`, pass `1530`, `351D`,
  `390E` and `390E`, and `0F25` never writes the slot.

The callers named here were found by searching every byte offset of segment `1000`'s module for a
near call or jump to the routine, and the relocation table for far calls; no relocated far call
names `0F25`, `072E` or `0C4D`. Seven jump tables read to the bounds their `cmp` gives are also
declared.

**Result.** The engine's `reach` decodes 49,715 bytes in 345 ranges and 292 routines, with no gap
and no contested instruction. Both controls hold: it decodes FND-EXE-370's AH=43 site `1000:27A8`
and AH=3D site `1000:3316`, and reaches and resolves FND-EXE-353's call at `1BD4:0331` to
`1000:20DF`. It reaches 44 of the 69 `CD 21` sites. Read along the decoded range that holds each
one, the last instruction that writes AH loads an immediate, giving AH 07 (1), 0B (1), 25 (5),
30 (1), 35 (4), 3C (2), 3D (2), 3E (2), 3F (3), 40 (5), 41 (1), 42 (5), 43 (1), 44 (3), 48 (1),
49 (1), 4A (2), 4C (1), 58 (2) and 67 (1). Eight of FND-EXE-490's nine calls to `1000:20DF` lie
in decoded code; the ninth, at `1A7C:004C` (selector 33), does not.

Nine transfers stay unresolved:

| Site | Transfer | What it can reach |
|---|---|---|
| `1000:0222`, `0229` | the startup-record walker's far and near calls | the four startup records, which are starts |
| `1000:0263`, `026A` | the exit-record walker's calls | nothing: it is called only with `DF34..DF34` |
| `1000:0319` | `call far [bx-1294]`, the atexit table | nothing: its count `DS:DA98` is 0 in the image and only `1000:0295` raises it, which no far call names and no word equals |
| `1000:0B85` | `jmp cx` | three bytes past its caller's call site |
| `1000:216C` | FND-EXE-353's call into the stack-built interrupt | bytes on the stack, outside the image |
| `1C08:049A` | `call far cs:[si+8]` in the interrupt 8 handler | entries 0 to 15 come only from `1C08:0781`, called at `1C08:0BC1` with the pointer `1C08:03BE` returns for function `0x67` and at `1C08:1485` with `0000:0000`; entry 16 is `1C08:051B` |
| `1C08:051C` | `call far cs:[011C]` | the interrupt 8 vector saved at `1C08:0527..053B` |

**The 25 unreached sites.** Two lie in the far routines `1000:02C2` (AH=43, AL=0) and `1000:02E2`
(AH=43, AL=1). `02C2`'s one caller, `push cs; call 02C2` at `1000:0694`, is in `1000:0652`, which
no far or near call names and whose offset no word of the image equals apart from the bytes of
`push dx; push es` at `1000:0F57`. `02E2` has no caller. The other 23 lie in
`1000:DD52..1000:E180`, the part of module `1DB2` from `1DB2:0232` on. The relocated far calls
into `1DB2` name only `1DB2:0040` to `1DB2:01B5`; of the 19 routine starts that hold the 23 sites,
two are called only from inside the unreached part, and none is named by a far call, by a near call
from outside it, by a word in the module's reached part, or by a word followed by a word giving its
segment in any frame a relocation names.

## Interpretation

The kinds of established entry the census had are the MZ entry point, the startup records, the
installed interrupt handlers, a routine pointer the code stores, the hook values and callback
arguments the code passes, and jump tables to their checked bounds. It had no exit or atexit
records because the image has none that can run. It did not have code built at run time
(the stack interrupt FND-EXE-353 reads), code the timer handler reaches through a pointer that a
driver supplies at run time, or the earlier interrupt 8 handler. These lie outside the image.

No interrupt 21 site that the utility reaches from an established entry selects AH=4B, the DOS
service that loads and runs a program. The eight stack-interrupt calls it reaches pass selectors
2F, 33 and 21, the last with AX 3305 (FND-EXE-490), so none requests interrupt 2E or AH=4B. The 25 sites the walk does not reach
lie in library routines that nothing in the image references. This answers for the sound
utility's own code what Q-EXE-007 asks of the game editions. The drivers it loads are FND-EXE-496's
and Q-EXE-026's subject.

## Alternatives

- A write through a pointer, such as a string or block copy into `DS:DB9C..DBA7`, `DS:DF14..DF1B`
  or the timer table at `1C08:0008..004B`, is outside the displacement searches, and so is a store
  to the vector table through a register.
- The engine reports no negative result, since unresolved transfers remain; the reading that none
  of them reaches another site rests on the table above.
- The AH reading takes the writer in the same decoded range as the interrupt; FND-EXE-490's search
  for branches into the gap between a writer and its interrupt found none.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<sound>` the
installed `SOUND_DS.EXE`: in `tools/research/exec-census/`, run
`sound_ds_reach_config.py <sound> <config>`, then
`python -I -m scientific_method_engine reach <config> > <report>` and
`sound_ds_reached_ah.py <sound> <report>`. The reference searches decode an instruction at every
byte offset of `1000:0000..1000:E360` with Capstone and keep those that name the operands above,
and search the same range for `E8` and `E9` instructions and 16-bit words with the routine's
offset.
