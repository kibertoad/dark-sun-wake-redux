# VIDEO

Next ID: Q-VIDEO-004

## Static

- Q-VIDEO-001. FMT-VIDEO-001, RULE-VIDEO-002, RULE-VIDEO-003: How does the palette change while
  the player draws a record? Settles it: `2660:04F3` and `1000:1C32`, and when overlay 196 asks
  overlay 215 to write the colour chunk to the VGA palette. Tried: searches for the FLI header
  magic, the numbered cinematic names and their templates, and the static title image's resource
  number; the magic led to the player of overlay 196 (FND-VIDEO-008), which shows how records are
  read and paced. Tried: FND-EXE-613 reads the four chunk decoders and FND-VIDEO-011 decodes the
  installed files with them, which settles the chunk layouts and the overlong chunk of `5.FLI`.
  Blocks: slice 7.
- Q-VIDEO-002. RULE-VIDEO-001, RULE-VIDEO-003: When do cinematics 2 to 5 play, and what do the
  helper routines of the cinematic code do? Settles it: the scripts that run opcode `0x22` with
  6 as the first parameter, the other requests of overlay 204's routine at offset `0x169`,
  `44DE:04A1`, `4544:0000`, `1BF3:4723`, `1BF3:4C09`, `1BF3:4FEB`, `5787:005C`, `56BD:0057`,
  offsets `0x206E`, `0x21EA`, `0x2424` and `0x2A21` of overlay 187, the readers of
  `4E28:0005` and `DS:6298`, and what `DS:14E5` is for. Tried: the cinematic routine, its two
  callers and the region copier (FND-VIDEO-004, FND-VIDEO-010, FND-VIDEO-007), which show how a
  cinematic plays but not which scripts ask for one. Blocks: slice 7.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.
