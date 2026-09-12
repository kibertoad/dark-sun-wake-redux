# Original file formats

Confidence values are `unknown`, `low`, `medium`, `high`, and `verified`.
Observed filenames and sizes establish only inventory facts, not internal
semantics.

| Format/family | Observed role | Current knowledge | Confidence | Required failure behavior |
|---|---|---|---|---|
| `*.GFF` | Large shared data (`RESOURCE`, `GPLDATA`, `OBJEX`) and numbered regions | Container semantics, endianness, directory structure, compression, and cross-references are not yet established | unknown | Bound file size and every discovered count/offset/length; reject overlap, overflow, truncation, unsafe names, and dangling references with source context |
| `*.FLI` | Four observed cinematics | Likely animation resources by extension only; dimensions, palette changes, frame timing, and exact variant are unverified | low | Bound frames, chunks, dimensions, decoded bytes, and timing; unsupported chunk types fail explicitly |
| `*.VOC` | Speech and sound-effect files | Creative Labs VOC is suggested by extension; headers, codecs, sample rates, and block use must be verified per file | low | Bound blocks and decoded samples; reject unsupported codecs and malformed terminators |
| `MUSIC/*.ogg` | 39 GOG-supplied music files | Ogg container presence is observed; mapping, loop points, provenance, and relationship to original media are unknown | low | Validate stream metadata and decode limits; unknown track mapping remains data, not a guessed rule |
| `ITEMS.BIN` | Small game data file | Size is 936 bytes in the supported build; record layout and meaning are unknown | unknown | Require exact supported source fingerprint before any reader; bound all table dimensions |
| `game.gog` / `game.ins` | GOG disc-image payload and descriptor | Presence and exact sizes observed; extractor relevance is unknown | low | Never mount/execute automatically; parse only if a later slice documents a bounded need |
| `DSUN.EXE` | Original DOS executable | Fingerprinted research oracle; never an extraction output or runtime dependency | verified inventory only | Never execute, load, copy into the pack, or commit; Ghidra findings remain independent evidence |

The first format reader is still an open question (`Q4` in the implementation
plan). Synthetic fixtures must precede production extraction. No byte layout or
resource mapping is currently claimed.
