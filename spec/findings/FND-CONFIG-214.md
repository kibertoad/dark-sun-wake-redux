---
id: FND-CONFIG-214
title: Sound setup populates the nine-byte SOUND.CFG tail from a record and a constant
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:4897..1000:48CA
tool: raw displacement search and Capstone 5.0.7 16-bit disassembly of bounded MZ ranges
environment: null
---

## Observation

The installed `SOUND_DS.EXE` at `C:\GOG Games\Dark Sun 2\SOUND_DS.EXE`
has the 204,593-byte length recorded in FND-CONFIG-213. Its data segment
is `1E36`. The `SOUND.CFG` write in FND-CONFIG-213 takes 59 bytes from
`1E36:E01B`; offsets `0x32..0x3A` of that buffer are `E04D..E055`.

The far function at file offset `0x00005ABD` (`1429:042D`) begins with
`push bp; mov bp, sp` and does not change BP again before the `pop bp`
and `retf` that end it at `0x00005CC9`. Its last instructions, at file
offsets `0x00005C97..0x00005CCA`, assign those bytes from the far record
pointer that is the function's first argument (`[bp+6]`):

| Output offset | Writer | Value supplied |
|---|---|---|
| `0x32` | `0x00005C9A..0x00005CA1` | word at input record `+0x2A` |
| `0x34` | `0x00005CA1..0x00005CA7` | literal word 4 |
| `0x36` | `0x00005CAA..0x00005CB2` | word at input record `+0xD6` |
| `0x38` | `0x00005CB5..0x00005CBD` | word at input record `+0xD8` |
| `0x3A` | `0x00005CC0..0x00005CC8` | byte at input record `+0xDA` |

Two other bounded paths write the same output word at `0x36` from input
record `+0xD6` and the byte at `0x3A` from `+0xDA`: file offsets
`0x00005D36..0x00005D4C` and `0x00005DCE..0x00005DE4`. A raw search of
the whole file for the five output displacements finds ten byte pairs:
the nine stores above and a comparison of `E04D` with 4 at
`0x0000572A..0x0000572F`.

## Interpretation

The nine-byte tail is constructed by sound setup from four input-record
fields and one fixed word, rather than copied verbatim from a sound-card
record. The installed file's word at `0x34` is 4, agreeing with the
literal write (FND-CONFIG-003).

## Alternatives

This reading does not establish what the input fields at `+0x2A`, `+0xD6`,
`+0xD8` and `+0xDA` mean or which setup selection reaches each writer.
The raw displacement search bounds literal references to these output
addresses; computed writes or whole-buffer copies remain possible. The
FND-CONFIG-023 identifies bounded game-side uses of tail offsets
`0x34..0x3A`, but their full effects and other consumers remain unread.

This replaces FND-CONFIG-022, which called `0x00005C97..0x00005CC9` a
routine and ended its ranges on the last item they covered. `0x00005C97`
is not a function start: it follows an `add sp, 8` inside the function
at `0x00005ABD`, and `0x00005CC9` is that function's `retf`, which the
range left out. Its writer column and its Observation's two other paths
(`..0x00005D49`, `..0x00005DE1`) ended on the first byte of their last
store, while its How to reproduce gave `..0x00005D4C` and
`..0x00005DE4`. The documentation check found the location's end when a
valid SOUND_DS function inventory placed a function ending at
`1000:48CA`; decoding each range found the rest. The writes, values and
search result are unchanged.

## How to reproduce

Use the installed `SOUND_DS.EXE` whose size and XXH3-128 are recorded in
FND-CONFIG-213. Search for the little-endian displacements `4D E0`,
`4F E0`, `51 E0`, `53 E0` and `55 E0`, then disassemble with Capstone
5.0.7 in 16-bit mode `0x00005ABD..0x00005CCA`, `0x00005D36..0x00005D4C`,
`0x00005DCE..0x00005DE4` and `0x00005720..0x00005734`. Correlate the
displacements with the 59-byte write buffer at `1E36:E01B`
(FND-CONFIG-213).
