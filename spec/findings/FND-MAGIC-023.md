---
id: FND-MAGIC-023
title: DATA 306 is the clerics' turn undead; the only code that casts it is the spell window's TURN UNDEAD button, control 0x2BEF, which overlay 211 +1CBD arms for a member with a cleric bit in class_flags and which casts at the member's greatest cleric level; overlay 208 +0F07 lets 306 through only to a target of details kind 9, and no cast list, item or other push holds 306
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097ACD..0x00097BBC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00097BBC..0x00097BCC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009756E..0x0009772F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0009772F..0x00097743
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000960C5..0x00096150
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000962B2..0x000962DA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068A3F..0x00068A7D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A788..0x0007A843
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A843..0x0007AA38
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092EA2..0x00092F2E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092797..0x0009299E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0009299E..0x000929DA
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x0001782A..0x00017873
tool: tools/research/exec-census/immediate_search.py, overlay_listing.py, direct_callers.py, trampoline_target.py, store_values.py, item_words.py, data_bytes.py, enemy_spell_lists.py, cast_list_bounds.py and gff_tag_numbers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7; a short scan of 32-bit immediates and a short read of DATA records, both described under How to reproduce
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x95E10 + x` for overlay 211, `0x78340 + x` for 190,
`0x68850 + x` for 182 and `0x91890 + x` for 208. `DS` is `57E0`. In the spell window (window
11500, overlay 211 `+02B5`, FND-INPUT-012) `4E71:0B44` is the current member `m` and `DS:1429`
the far pointer to its 66-byte details record: `+02B5` stores the records it is given, or in a
fight (`4C10:0019` not 0) those of the slot `DS:426D`, at `DS:1429` and `DS:142D`, and the slot at
`4E71:0B44` (`+02B5..+0340`).

**Every push of 306.** `immediate_search.py <dsun> 132` lists 15 instructions with 0x132 as an
immediate or displacement. Eleven are `jmp`/`call` targets inside other code. The other four are
overlay 190 `+251D` (`cmp word [DS:6273], 0x132`), overlay 211 `+18A1` and `+18B9` (`push 0x132`)
and overlay 211 `+201A` (`cmp word [bp+8], 0x132`, in `+1FB5`, FND-MAGIC-017). A scan of every
instruction with a 32-bit immediate whose high or low word is 0x132 (How to reproduce) adds one,
overlay 211 `+1889` (`push 0x132FFFF`); the same scan for 0x4E4F finds the control
`push 0x4E4F4349` at `+1D4F` among its 25 hits. All three pushes lie in overlay 211 `+175E`.

**The handler.** `+175E` (`+175E..+191F`, trampoline `57C1:0066` by `trampoline_target.py`) takes
a word, a control `id` and a message and looks the message up in the five words at `+191F` (2, 4,
0x20, 0x40, 0x80; targets `+178D`, `+17D2`, `+1813`, `+1916`, `+18D2`). Message 2 writes `TURN
UNDEAD` (`DS:2D08`, a 12-byte string) to control 0x2C06 through overlay 190 `+06B4`. Message 0x40
sets the byte `DS:9D76`. Message 0x20, when `DS:9D76` is set (`+1813..+18CF`), closes the window
(`+073D`), stores `DS:9D77` in `DS:445A`, and with `m` calls overlay 190 `+2448(m, 0xFFFF,
0x132)`, then overlay 211 `+1FB5(m, 0x132)`, then overlay 208 `+1612(m, DS:9D77, 0x132, 0, r)`
with `r` the result of `+1FB5`. FND-EXE-634 reads the rest of the handler.

**Where the handler is registered.** `direct_callers.py <dsun> 211+175E` finds no direct call,
only the trampoline. Overlay 182 `+01EF` (`+01EF..+022D`) takes a window, a control, a far callback
and a mask: it stores the callback through `3F96:02AB` and ORs the mask in through `3F96:02F8`.
`+02B5` registers trampoline `57C1:0061` (overlay 211 `+10F4`) with mask 0x66 for each control
0x2BCD to 0x2BEE (`+04A2..+04CA`). The only push of `57C1:0066` is in overlay 211 `+1CBD`
(`+1CBD..+1DAC`), which registers it for control 0x2BEF with mask 0x66 (`+1D1D..+1D38`).

`+1CBD` takes the word `+0x10` of the record at `DS:1429`, ANDed with 0xF, into `DS:9C1C`, and 0
instead when the record's word `+0x0E` is 0 or the word `DS:140C` is 0x44 (`+1CC0..+1CE4`). With 0
it registers a null callback and mask 0 for control 0x2BEF and returns (`+1D8E..+1DAA`).
Otherwise it jumps through the eight words at `+1DAC` by the value less 1 (`+1CEE..+1CFA`): 1
stores 3044 (0xBE4) in `DS:9D77`, 2 stores 3045, 4 stores 3046, 8 stores 3047, and 3, 5, 6, 7
and 9 to 15 leave `DS:9D77` as it was. All then register `+175E` on control 0x2BEF, load `ICON
DS:9D77` into `DS:2B9A` through `38FF:04AB` when nothing is there, and draw it through `+1933`
(`+1D1D..+1D8C`). `gff_tag_numbers.py <install> ICON` lists `ICON` 3044, 3045, 3046 and 3047 in
`RESOURCE.GFF`. `direct_callers.py <dsun> 211+1CBD` finds near calls at `+0210`, `+04DB` and
`+0B6F`.

**The level.** `+1FB5(m, 0x132)` returns the greatest of `27E5:00C1` over class codes 1 to 4
(FND-MAGIC-017). The fifth argument of overlay 208 `+1612` (`+1612..+169E`) is stored in
`DS:9BE5` (`+1669..+166C`) and reaches overlay 193 `+2290` as its level byte (FND-MAGIC-016); the
second, `DS:9D77`, is stored in `DS:9BD4` (`+164F..+1652`). `+1612` also copies the 73 bytes of
`DATA 0x132` into `DS:9B86` (`+161D..+1634`).

**The target gate.** Overlay 208 `+0F07(t, c, i)` (`+0F07..+110E`) looks `i` up in the 15 words at
`+110E` (2, 29, 30, 31, 34, 36, 60, 82, 115, 116, 127, 131, 136, 165, 306) and returns 1 for any
other number. For 306 it jumps to `+0FBE`, which returns 1 when byte `+0x12` of `t`'s details
record is 9 and 0 otherwise. 60 and 165 go to `+0F8A`, which returns 1 only for byte `+0x12` 14.
`direct_callers.py <dsun> 208+0F07` finds overlay 197 `+0EEC`, in `+0E8F`, which ends the hit when
`+0F07` returns 0 (FND-PARTY-112), and a near call at overlay 208 `+031D`.

**The pending selection.** Overlay 190 `+2448(m, kind, n)` (`+2448..+2503`) stores `m` in
`DS:6276`, the kind byte in `DS:6275` and `n` in `DS:6273`, and for kind 0xFF stores 0 as the
`ICON` number in `DS:445A`. `+2503` (`+2503..+26F8`), for kind 0xFF, returns unless the word
`4C10:0019` is not 0, `DS:6273` is 0x132 and its byte argument is not 0; then it calls overlay 173
`+306B(DS:426D, 0)` and clears the selection (`+250D..+2531`, `+264D..+266E`). Kind 0xFF skips both
the recent-cast list at `DS:445E` and overlay 197 `+3060` (FND-PARTY-243).

**Lists and items.** `enemy_spell_lists.py <dsun>` prints the 13 enemy lists; none holds a number
above 230. `cast_list_bounds.py <dsun>` gives the party list bounds, all at most 235, and the
psionic lists 235 to 268. `item_words.py` with the list `306,307` finds no chunk whose word at 0x10
is 306 or 307 in `OBJEX.GFF` (708 chunks, largest word 326), `CHARSAVE.GFF` (172 chunks) or any
other `.GFF` of the installation (no item chunks).

**The record.** `DATA 306` in `RESOURCE.GFF` (`0x1782A`, 73 bytes) has the 32-byte name field at
`+0x20` holding `MONS306`, as `DATA` 300 to 310 hold `MONS300` to `MONS310`. Its byte `+0x0C` is
1, byte `+0x16` 60, byte `+0x17` 42, word `+0x11` 0x4000 and word `+0x1A` 0.

## Interpretation

`DATA` 306 is the turn undead ability of the four cleric classes. The spell window shows a
`TURN UNDEAD` button, control 0x2BEF, only for a member whose `class_flags` (FMT-PARTY-001, copied
to details word `+0x10`) has one of the four cleric bits; its icon is `ICON` 3044 to 3047, one per
sphere bit. Pressing and releasing it casts 306 from that member at the member's greatest cleric
level, which is what RULE-MAGIC-006's `level` receives. The hit reaches the effect switch only for
a target whose details byte `+0x12` is 9, the undead kind (FND-MAGIC-022), and there RULE-MAGIC-006
rolls the d20 against the level table. A member without a cleric bit, or anyone while `DS:140C` is
0x44, has no button.

Nothing else casts 306: it is in no spell list, psionic list, enemy list or item, and its `DATA`
name is a placeholder. Using it does not enter the recent-cast list and does not pay through overlay
197 `+3060`; in a fight overlay 190 `+2503` then calls overlay 173 `+306B` for the slot at
`DS:426D`, which ends that slot's turn state as other completed actions do (FND-COMBAT-047).

FND-EXE-608 said `+02B5` registers `+175E` for controls 0x2BCD to 0x2BEE. The pushes at
`+04A7..+04AF` put the callback `57C1:0061`, which is `+10F4`, and the mask 0x66 on those
controls; `+175E` is registered on 0x2BEF only, by `+1CBD` (FND-EXE-634).

## Alternatives

- A number formed by arithmetic (for example a table entry plus an offset, or a script argument
  `n mod 1000` as script function 46 of opcode 0x22 forms, FND-MAGIC-020) would not show in an
  immediate search. Script programs that cast by number were not searched for 306.
- Which bit of `class_flags` is which sphere rests on FMT-PARTY-001 listing the bits 1, 2, 4 and 8
  in the order of cleric codes 1 to 4; the code here only needs one of the four to be set. A value
  with two cleric bits keeps the `ICON` number left in `DS:9D77` from an earlier member.
- The mask 0x66 holds 2, 4, 0x20 and 0x40; the handler's case for 0x80 is reached only if another
  registration adds that bit, which was not found.
- Whether window 11500 has a child numbered 0x2BEF was not read from `RESOURCE.GFF`.
- What `DS:140C` 0x44 is as a region was not checked.
- The overlays are outside the resident harness; no emulated call or run was made.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python, `<dsun>` the installed `DSUN.EXE` and
`<install>` its folder: `immediate_search.py <dsun> 132` and `9D77`; `overlay_listing.py <dsun>
211 9756E 9772F`, `211 960C5 96150`, `211 96280 962F0`, `211 97ACD 97BBC`, `190 7A788 7AA38`,
`182 68A3F 68A7D`, `208 92EA2 92F2E` and `208 92797 9299E`; `trampoline_target.py <dsun>
57C1:0061 57C1:0066`; `direct_callers.py <dsun> 211+175E 211+1CBD 208+0F07 190+2503`;
`enemy_spell_lists.py <dsun>`; `cast_list_bounds.py <dsun>`; `item_words.py <gff> 306,307` for
each `.GFF`; `gff_tag_numbers.py <install> ICON`; `data_bytes.py <install>/RESOURCE.GFF 0C`.
Read the 8 words at file `0x97BBC`, the 10 at `0x9772F`, the 30 at `0x9299E` and the bytes at
`DS:2D08` (file `0x4FD08`). The 32-bit scan decodes each overlay's code and the load image (as
`immediate_search.py` does) at every offset up to 8 bytes before each occurrence of the word, and
keeps instructions of 6 bytes or more whose immediate is above 0xFFFF with the word as its high or
low half; run it for 0x132 and, as the control, 0x4E4F. The record read takes the `DATA` directory
as `data_bytes.py` does and prints, for 300 to 310, the text at `+0x20` up to its first zero and
the bytes and words named above.
