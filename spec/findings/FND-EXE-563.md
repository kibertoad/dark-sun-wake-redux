---
id: FND-EXE-563
title: The overlay manager can copy unloaded overlays into an EMS or extended-memory cache, tracked in header words 0x0E to 0x18
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:08EB..4AE5:0D27
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 and xxhash 4.0.1, MZ relocations applied for a load image at segment 0x1000 (tools/research/exec-census/overlay_cache.py)
environment: null
---

## Observation

In the installed `DSUN.EXE`, with DS `55CE` (FND-EXE-560):

- EMS setup, far routine `4AE5:08EB`, with word arguments at `BP+6` (an EMS handle), `BP+8`
  (a first page) and `BP+0x0A` (a page count). It returns `0xFFFF` when bit 0 of byte
  `[0x10]` is set. With a handle of 0, it calls `4AE5:0D8B`, which opens the device
  `EMMXXXX0` (the string at `4AE5:0D82`) and uses `INT 67h` services `40h`, `46h` (version
  at least `0x32`), `41h` (page frame, kept in `[0x30]`) and `42h` (free pages). The page
  count becomes the smaller of the request (all free pages after the first when it is 0) and
  the pack's `payload_size` from `57E0:356C` rounded up to 16 KiB pages. Below 4 pages it fails
  unless the pages times `0x400` reach `[0x11A]`, the largest overlay in paragraphs plus 2
  (FND-EXE-561). After `4AE5:0E3D` gets the pages (service `43h` when the handle was 0), it
  writes a cache record at `55CE:0140`: the doublewords first page times `0x4000` (start),
  start plus the count times `0x4000` (end) and start again (next free), the word `0x0A4B` and
  the word 0. It sets bit 1 of `[0x10]`, `[0x84]` to `0x0EA2` and `[0x80]` to `0x0D11`.
- Extended-memory setup, far routine `4AE5:0AB5`, with a doubleword start at `BP+6` and a
  doubleword size at `BP+0x0A`. It returns 0 when bit 1 of `[0x10]` is set. `4AE5:0ECD`
  finds the extended memory through `INT 2Fh` `4300h`/`4310h` (an XMS driver) or `INT 15h`
  `88h`. The start is raised to the lowest free address, the size is capped at the free
  space and at `payload_size`. It fails when the size is below 64 KiB and its paragraphs are
fewer than `[0x11A]`. It writes the same record at
  `55CE:0130` with the word `0x0BFE`, sets bit 0 of `[0x10]`, `[0x82]` to `0x1155` and `[0x80]`
  to `0x0D11`.
- Unload hook. `4AE5:061F` calls the near routine in `[0x80]` when header `+0x18` is `0x04C6`
  (FND-EXE-562). Without a cache that word is `0x1256`, a `ret`. `4AE5:0D11` calls
  `4AE5:0BCF` when bit 0 of `[0x10]` is set and `4AE5:09CB` when bit 1 is.
- Copy into the cache, `4AE5:09CB` (EMS, record `55E2:0000`) and `4AE5:0BCF` (extended,
  record `55E1:0000`). Each calls `4AE5:0C2E` and returns if it sets carry. It then calls the
  far pointer at `[0x86]` with AX 1 and copies the code from the load segment in header
  `+0x10` to the cached position. EMS copies `code_size` bytes through the page frame
  (`INT 67h` `44h`); extended memory copies `(code_size + 1) & ~1` bytes through `4AE5:11A9`
  (XMS move, function `0Bh`, or `INT 15h` `87h`).
- Cache allocation, `4AE5:0C2E`, with DS the cache record and ES the overlay header. It takes
  `(code_size + 1) & ~1` bytes at the record's next-free position. When the space before the
  end is smaller, the next-free position goes back to the start, after a step through the
  chain at `4AE5:0C51..4AE5:0C7A` that this reading does not follow in detail. `4AE5:0CD5`
  walks the chain of headers from the record's word `+0x0E` through each header's word
  `+0x0E`, and sets `+0x18` to `0x04C6` in every header whose cached position lies in the
  bytes being taken. The record's chain start moves to the header after the last of them. The
  routine then stores the old next-free position in header words `+0x14` and `+0x16`, the
  record's word `+0x0C` in header `+0x18`, advances the next-free position, and appends the
  header to the end of the chain: the previous last header's `+0x0E` gets its segment and its
  own `+0x0E` gets 0.
- Loading from the cache: `4AE5:0A4B` copies `code_size` bytes from the EMS position in
  `+0x14`/`+0x16` to the load segment. `4AE5:0BFE` copies `(code_size + 1) & ~1` bytes from the
  extended-memory position. Neither calls the fixup pass `4AE5:0421`.

## Interpretation

The manager can keep a second copy of overlays in expanded or extended memory. When an overlay
loaded from the file is unloaded with a cache active, its code, with fixups already applied, is
copied into the cache as a ring. Header `+0x18` then names the cache's loader (`0x0A4B` for
EMS, `0x0BFE` for extended memory) in place of the file loader, so the next load copies the
code back without reading the file. Header words `+0x14`/`+0x16` hold the copy's position.
Header `+0x0E` links the cached overlays in the order they were copied. When the ring wraps,
the oldest copies are overwritten, and their headers go back to loading from the file.

## Alternatives

What `[0x112]`, the chain step on wrap-around, the routines in `[0x82]` and `[0x84]`, and the
far routine at `[0x86]` (`1000:02FC`) do was not read. Neither were the details of the
extended-memory detection and allocation (`4AE5:0ECD`, `4AE5:107D`, with an `INT 19h`-vector
hook at `4AE5:1103..4AE5:112D`). Whether the game enables either cache is FND-EXE-564. Whether
one is active while the game runs depends on the machine's EMS and XMS drivers.

## How to reproduce

Run `python -I tools/research/exec-census/overlay_cache.py <install dir>/DSUN.EXE` from the
commit that adds this finding, with the locked evidence Python. It checks the file's size and
XXH3-128, applies the MZ relocations, prints the device name at `4AE5:0D82` and disassembles
`4AE5:08EB..4AE5:0D27` in the seven ranges its docstring lists.
