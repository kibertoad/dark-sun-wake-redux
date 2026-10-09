---
id: FND-EXE-491
title: Sound utility indirect far calls stay in its image except driver code loaded from named files
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0128..1000:0272
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0295..1000:02C2
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0300..1000:0349
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:4127..1000:4296
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:03BE..1C08:040E
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:0781..1C08:07D1
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1C08:0A25..1C08:0B08
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D2FC..0x0001D308
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D67C..0x0001D694
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

This reads the far-pointer values behind the indirect far calls
FND-EXE-490 lists. DGROUP is load-image paragraph 0E36: startup at
1000:0000 loads that relocated immediate into DX and CS:0291, so DS
offset `o` lies at file offset 0xF760 + `o`.

Startup at 1000:012A sets ES from CS:0291 and calls the record walker at
1000:01ED with SI=DF1C and DI=DF34. The exit pass at 1000:0157 calls the
second walker at 1000:0231 with SI=DI=DF34, so it visits no record. Each
walker picks the unvisited six-byte record with the lowest (startup) or
highest (exit) priority byte at +1, marks byte +0 with FF, and calls
through +2: a near call when byte +0 was 00, a far call otherwise. The
shipped records, at file 0x1D67C..0x1D694, are:

| Record | Byte +0 | Priority | Target |
|---|---|---|---|
| DS:DF1C | 00 | 02 | near 1000:0E0F |
| DS:DF22 | 00 | 10 | near 1000:1766 |
| DS:DF28 | 01 | 10 | far 1000:190A; its segment word is relocated |
| DS:DF2E | 00 | 10 | near 1000:2619 |

The loop at 1000:030D calls far through DS:[ED6C + 4n] while the word at
DS:DA98 is nonzero, decrementing it first. The shipped DA98 word is 0000.
Its other accesses are in 1000:0295..1000:02C2, which compares it with
0020, stores the incoming far pointer at DS:[ED6C + 4n] and increments
it. 1000:0295 has no near or far caller, and no word in the image equals
0295. No other instruction in the image addresses DS:DA98 or the table
by displacement.

The exit hooks DS:DB9C, DS:DBA0 and DS:DBA4 ship as 0000:02FF with
relocated segment words, at file 0x1D2FC..0x1D308. 1000:02FF is a
single `retf`. The only other stores to them are at 1000:2AD6..2AE2,
which writes 3C90 and a relocated 0000 to DBA0/DBA2, and at
1000:38AC..38B8, which writes 3CCD and a relocated 0000 to DB9C/DB9E.

The timer dispatch at 1C08:049A calls far through CS:[SI*4 + 0008]. Those
slots are written only at 1C08:07B9 and 1C08:07BE, from the far pointer
argument of the registration routine at 1C08:0781. That routine has two
callers, both `push cs / call` from the same segment, and no far caller
or stored pointer. The call at 1C08:0BC1 passes the far pointer returned
by 1C08:03BE for AX=0067. The call at 1C08:1485 passes 0000:0000.

1C08:03BE takes a driver slot below 16, loads the far pointer at
CS:[slot*4 + 0128] and scans four-byte pairs there for a word equal to AX,
stopping at FFFF. On a match it returns that pair's second word as the
offset and the table's segment; otherwise it returns 0000:0000. The slots
at CS:0128 are written by the install routine at 1C08:0A25 and cleared by
1C08:0B08. 1C08:0A25 takes a far pointer to an image in memory. When the
words at +3 spell `DIGPAK`, it stores ES-10 and DI+0100 at CS:0E2A/0E28,
which 1C08:13EA..13FD and 1C08:14D9..14EC load into CS:0E20/0E1E before
calling far through CS:0E1E, and it puts 1C08:0D80 in the slot. When the
words at +2 and +4 spell `Copy`, it puts the image address plus its first
word in the slot.

1C08:0A25 has two callers, at file 0x5285 and 0x53F6. Each passes the
far pointer that 1000:4127 returns for a name at offset 16 or 24 of the
record DS:[E017]. 1000:4127 copies the string at DS:EB7E into a 100-byte
stack buffer when its length is nonzero and passes the buffer and the
name to 1000:3A0B; otherwise it copies the name alone. It opens that path
through 0DB2:008C, the wrapper holding the AH=3D request at linear
0xDBF3, takes a size from 0DB2:012D, allocates through 0D98:0008, rounds
the address up to a paragraph, reads the file into it through 0DB2:01B5
and closes it through 0DB2:0040. Open, allocation or read failure returns
0000:0000.

Besides interrupt 21, the image's aligned interrupt instructions are
interrupt 10 (9 sites), 1A (1) and 66 (7, all in segment 1C08).

## Interpretation

The startup records, the exit hooks and their two replacement values
point inside the load image, and the atexit-style table is never filled
or called. FND-EXE-490's census already covers every interrupt 21
instruction those targets can reach. The code outside the image that the
utility enters is: the saved interrupt 08 handler, the interrupt 66
handler, driver code reached through the CS:0E1E calls and the timer
callback from the driver table lookup, and a timer slot registered with
0000:0000. The driver code is read at run time from a file named by the
device record; the utility's image holds no such name. Whether any of
that code requests program execution depends on which driver files are
loaded, which this reading does not establish.

The `DIGPAK` and `Copy` tests identify image formats by signature; they
are not an identification of any particular shipped file.

## Alternatives

Treating the startup and exit tables as possible jump-outs ignores their
shipped targets, relocated segments and empty exit range. Treating the
DS:ED6C table as live ignores the uncalled routine that is the only
writer of its count. Reading the CS:[SI*4+8] slots as fixed handlers
ignores that both registrations come from run-time values: a driver
lookup and a null pointer. Reading the loaded drivers as embedded data
ignores the open and read through the path built from the device record.

## How to reproduce

Require FND-EXE-350's SOUND_DS.EXE identity: length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. With locked Capstone 5.0.7 in
sixteen-bit mode, decode load-image offsets 0x0000..0x0349,
0x2AD6..0x2AE2, 0x38AC..0x38B8, 0x4127..0x4296 and 0xC43E..0xCBB0,
0xCC20..0xCC46, 0xD4E8..0xD50A and 0xD45C..0xD56C (file offset is load
offset plus 0x1400; segment 1C08 starts at load offset 0xC080). Read the
DGROUP words at file 0x1D67C..0x1D694, 0x1D2FC..0x1D308 and 0x1D1F8 and
check the relocation table for each segment word. Search the image for
far immediates and near rel16 calls to linear 0x0295, 0xC801 and 0xCAA5,
for words equal to their segment offsets, and for instructions with
displacements DA98, DB9C..DBA7 and ED6C..EE6C. Original bytes stay
outside Git; no original process, DOSBox or emulated call runs.
