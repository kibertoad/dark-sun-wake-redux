---
id: FND-COMBAT-040
title: The installed effect window contains all 35 registered application frames with 18-pixel slots
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A776..0x0008A7C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A815..0x0008A837
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068914..0x00068946
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000689BB..0x00068A3F
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000294EC..0x0002A005
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00027144..0x00028120
tool: DarkSunWakeRedux.Inspect gff; tools/evidence/report.mjs table; tools/research/exec-census/overlay_listing.py; installed-build fingerprints and relocation/fixup checks
environment: null
---

## Observation

FND-COMBAT-039 identifies the setup's registration interval
2BCD..2BEF hexadecimal and handler 577D:0034. This finding connects
that interval to the installed window and frame resources. Resource
metadata is read from the supported RESOURCE.GFF, size 5724669 and
XXH3 92fe031616f36062f99484b0f728210e; each bounded table query
checks that complete-file identity before reading fields.

### The setup requests WIND/11500 on its creation branch

When DS:2690 is null, overlay 202 +0287 passes first word 2CEC
(11500), two additional words and callback 577D:002F to overlay 182
+016B. That entry forwards its first word to local +00C4. The latter
zero-extends the word to a resource number, supplies tag WIND and a
null-initialized local pointer slot to 38FF:04AB, then returns that
slot's far pointer. It does not convert the reader's AX status into a
separate successful-acquisition result.

The +016B continuation tests the returned pointer and then calls
placement/activation/refresh helpers before its normal pointer return.
On its local error path it requests a diagnostic and returns zero if
that call returns. No successful native window or child allocation is
inferred from these requests. On a nonzero DS:2690 reuse branch, the
setup instead forwards the existing pointer to a callback-update helper;
this finding does not admit that existing pointer's resource identity.

The creation branch stores the returned pointer in DS:2690 and
DS:1431. The registration loop subsequently passes DS:2690, whereas
the icon/list handler uses DS:1431. They have the same local producer
on this creation path, not an established lifetime invariant across
every later call or reused window.

### Installed graph and exact reference membership

Inspect's resource directory locates WIND/11500 at decimal offset
169196, length 2841. Its embedded tag, number and length agree with
that directory record. Under FMT-UI-001 the header states child_count
86, width 320, height 189 and image number zero. The explicit count
gives 261 + 86*30 = 2841, exactly the resource length.

Reading all 86 FMT-UI-002 children finds exactly one APFM reference
for each number 11213..11247, with no missing or duplicated reference
in that interval. These are child indices 43..77, counted from zero,
in increasing resource-number order. All 35 listed child flags are
zero. The registration's 35 numeric targets therefore have matching
references in this installed window, not merely matching entries
somewhere in the archive.

The directory also individually locates all 35 corresponding APFM
resources. Each is 116 bytes, has matching embedded tag/number/size,
width 18 and height 18 under FMT-UI-004. They occupy contiguous
source spans beginning at decimal 160068 and ending at 164128
exclusive. This contiguous layout was checked against the individual
directory offsets before using a fixed-stride bounded query.

All 35 shipped callback double words at offset 62 hexadecimal are
zero. Masks are 01E6 for numbers 11213..11218 and 00E6 for the other
29. Every mask already contains all bits of the setup's OR request
0066. Thus that request need not add mask bits to unchanged shipped
records, although successful callback installation and later mask/state
writers remain runtime dependencies. The extra mask bits are not
assigned native input meanings here (Q-UI-001).

### Relative placement of the 35 slots

Let i be resource number minus 11213. The stored child coordinates
can be described by these six groups; indices and coordinates are
decimal, and each frame is 18 by 18:

| Slot indices | X coordinate | Y coordinate |
|---|---|---|
| 0..14 | 167 + 19*(i modulo 5). | 42 + 20*floor(i/5). |
| 15..17 | 262. | 42 + 20*(i-15). |
| 18..20 | 148. | 42 + 20*(i-18). |
| 21..30 | 167 + 19*((i-21) modulo 5). | 109 + 20*floor((i-21)/5). |
| 31..32 | 262. | 109 + 20*(i-31). |
| 33..34 | 148. | 109 + 20*(i-33). |

This describes resource order as well as positions: it is not a
single left-to-right row-major sequence. All these child rectangles
fit within the stored 320-by-189 window extent. The table describes
window-relative file positions, not absolute native screen coordinates.
Loaded origin/placement and successful dispatch remain distinct.

FND-COMBAT-030 found 16-by-16 frames for the 97 referenced installed
ICON resources. FND-COMBAT-038 reads the caller's requested ICON
position one pixel beyond its background-image position in each axis.
For a frame whose loaded rectangle and window origin agree with these
admitted child coordinates, a 16-by-16 image at that +1 offset fits
inside an 18-by-18 slot with one pixel on each side. This is a
conditional metadata/caller geometry match, not a screenshot comparison
or proof that the other image request supplies a particular border.

## Interpretation

This answers the shipped graph-existence, reference membership,
dimensions, masks and relative slot placement part of Q-COMBAT-010.
It replaces a merely attempted registration count with a bounded check
of the actual installed source graph, while keeping native acquisition
and handler dispatch separate. Shipped null callback fields cannot
establish an installed live callback without the code path succeeding.

Q-COMBAT-010 retains active archive/window identity, successful resource
and child acquisition, callback installation, state synchronization and
backend/palette/display outcomes. Q-UI-001 retains event interpretation
and loaded graph behavior. FMT-COMBAT-003 is not raised to established;
no original game was started and no new rendering behavior was built.

## Alternatives

- Finding 35 APFM directory entries alone would not prove that this
  window references them. The complete 86-child query supplies that link.
- Dividing resource length by an expected stride is not the count
  evidence: the WIND header's own child_count is read first.
- The archive's callback fields are zero, even though the setup names a
  nonzero handler. File bytes and successful installed runtime state
  must not be conflated.
- A two-dimensional geometric fit does not prove palette, image-buffer
  validity, loaded window origin or the pixels drawn by a backend.

## How to reproduce

Use the locked evidence environment and supported DSUN.EXE/RESOURCE.GFF.
Read overlay_listing.py descriptor 202, 8A776..8A7C5 and
8A815..8A837, and descriptor 182, 68914..68946 and 689BB..68A3F.
Bounds are hexadecimal and half-open. Retain the null-create/nonzero-
reuse distinction and the supplied/returned window pointer route.

Run the project Inspect gff command on the owned RESOURCE.GFF to
locate WIND/11500 and each APFM/11213..11247. Use report.mjs table
with the RESOURCE.GFF XXH3 above and these decimal query settings:

- Header: start 169196, count/limit one, stride 261; fields tag/size/
  number at offsets 0/4/8, width/height/image at 190/192/194 and
  child_count at 243, with widths 4/4/4/2/2/2/2.
- Children: start 169457, count/limit 86, stride 30; fields tag,
  number, x, y and flags at 4/8/12/14/28, widths 4/4/2/2/2.
- Frames: start 160068, count/limit 35, stride 116; fields tag,
  size, number, width, height, event_mask and callback at
  0/4/8/40/42/88/98, widths 4/4/4/2/2/2/4.

Compare the registration interval with child and directory numbers,
check uniqueness and source record bounds, then group dimensions/masks
and compare coordinates with the six groups above. Configurations and
reports panel-window-header, panel-window-children and panel-effect-frames
remain only in GAME_DIR/analysis/combat-effects/. Existing approved
readers supply the checks; no new format reader or original-derived
resource file is committed.
