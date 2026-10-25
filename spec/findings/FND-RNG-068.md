---
id: FND-RNG-068
title: 2D40:2F34 rejects only slots below 48, so the slot 9999 that 28C9:0353 passes when DS:426D holds no slot makes it copy the text at DS:0CE6 into a new record and store the record's number at DS:819C and DS:3956
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0353..28C9:04DD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0683..28C9:06C8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:10AE..2D40:10DC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2F34..2D40:31F6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C3F..2D40:0C73
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004DCE6..0x0004DD0B
tool: Capstone 5.0.9 over the load image; Python 3.14.7
environment: null
---

## Observation

Addresses are static, with the load image at segment `1000` (segment immediates in its code are
relative to 0) and `DS` = `57E0`. Records are the 37-byte records at `DS:67BB`; record `n` is at
`0x67BB + 37 × n` computed in 16 bits.

**`28C9:0353(slot, x, y)`.** `28C9:0683..06C5` calls it with `DS:426D` and the point of a view
event: directly when the combat word `4C10:0019` is 0, and in combat when `28C9:0568` returns a
slot other than `DS:426D`. It converts the point to a 16-pixel cell `[bp-2]`, `[bp-4]` with the
view origin `DS:1408` and `DS:140A`, sets `[bp-6]` to `0x0F` and `[bp-7]` to the byte `25AF:00E1`
returns for the cell. For a slot of 4 or more it skips to `+0461`. There, when `[bp-7]` is 0 and
`[bp-6]` is still `0x0F`, it stores the cell and `[bp-6]` in the three words at
`4F49:08A3 + 19 × slot` (`28C9:0473..04A4`). Then it calls `2D40:10AE` with the slot and 1:
between calls to `25AF:0648` and `25AF:0692` when `4C10:0019` is 0 and the slot is below 4
(`28C9:04BB..04CB`), and directly otherwise (`28C9:04D2..04DA`). Nothing on this path compares the
slot with 9999.

**`2D40:10AE(slot, flag)`** first calls `2D40:2F34` with the slot (`2D40:10D3..10D9`).

**`2D40:2F34(n)`.** It returns `n` unchanged when `n` is below `0x30` (signed). Otherwise it reads
the word `w` at `+0x1B` of record `n` and returns unchanged unless `|w|` is 1 to `0x1F3F` or above
`0x36AE`. It takes a record `r` from the list at `DS:56ED` through `2D40:0C3F`, which returns the
list head unless it is 9999, and returns unchanged when `r` is 9999. Then, in order, it:

1. copies the 37 bytes of record `n` over record `r` through `1000:3F5A` (`2D40:2F96..2FB3`);
2. copies the 3 bytes at `4F49:0C33 + 3 × n` to `4F49:0C33 + 3 × r`;
3. stores `r` at offset 6 of the 8-byte entry, numbered by record `r`'s word `+0x01`, of the far
   table at `DS:67B7`;
4. fills record `n` with `0xFF` through `1000:3FA2`;
5. sets bit `n` of the bitmap at `52A2:0000` to bit `r`'s value and bit `r` to bit `n`'s old
   value;
6. when record `r`'s word `+0x21` is not -1, stores `r` at `+0x23` of the record it numbers, and
   when its word `+0x23` is not -1, stores `r` at `+0x21` of the record it numbers
   (`2D40:30C3..310B`);
7. replaces `n` with `r` in each of the words `4C13:0361`, `4C13:0363`, `4C13:0365` and
   `4C13:0367` that holds `n`;
8. calls `2D40:3D4C(n, r)` when record `r`'s `|+0x1B|` passes the same test, the overlay 193
   trampoline `573B:00AC` with `n` and `r` when the byte at `4F49:0C33 + 3 × r` is 2, and
   `2D40:03C1(n)`, and returns `r`.

**Slot 9999.** `37 × 9999` is `0xA52B` in 16 bits, so record 9999 is at `DS:0CE6`. In the file
those 37 bytes (`0x4DCE6..0x4DD0B`) are the start of a help message. Its word `+0x1B` is `0x726F`,
above `0x36AE`, so the routine goes on. Copied into record `r`, its word `+0x21` is `0x6F66` and
its word `+0x23` is `0x6D72`, neither -1. Step 6 then stores `r` at
`0x67BB + 37 × 0x6F66 + 0x23` = `DS:819C` and at `0x67BB + 37 × 0x6D72 + 0x21` = `DS:3956`, the
July and August bytes of the month lengths at `DS:3950` (FND-RNG-016). Step 3 uses record `r`'s
word `+0x01`, `0x4309`; step 2 reads `4F49:8160`; step 4 overwrites the text at `DS:0CE6..0D0B`
with `0xFF`.

## Interpretation

`2D40:2F34` treats any number from 48 up as a record, including 9999, the value `DS:426D` holds
when no slot is current. With `DS:426D` at 9999, a view event that reaches `28C9:0353` therefore
writes the record's number, a small value, into the month-length table, where the clock conversion
sums it for the months after June, and makes the other stray stores above, among them the three
words at `4F49:08A3 + 19 × 9999`. Step 4 leaves `0xFF` at `DS:0CE6`, so a second call with 9999
copies `0xFF` bytes, whose links are -1, and stores nothing through them. FND-RNG-067 observes the
first call from the two-character play checkpoint, with combat off.

## Alternatives

The routines of step 8 and `25AF:00E1` were not read; they may make further stores with 9999.
Which states hold `DS:426D` at 9999, and which other callers pass `2D40:10AE` or `2D40:2F34` a
slot from `DS:426D`, were not searched.

## How to reproduce

Disassemble the address ranges above from the load image and read the 37 bytes at `DS:0CE6` in
the file (`0x4DCE6`); compute `0x67BB + 37 × n` modulo `0x10000` for `n` = 9999, `0x6F66` and
`0x6D72`.
