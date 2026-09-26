---
id: FND-COMBAT-006
title: The status panel image holds no text, and no GFF file holds the panel's words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x193EA..0x19C9B
tool: DarkSunWakeRedux.Inspect resource-pattern; hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#BMP/19003`, at `0x193EA..0x19C9B` of `RESOURCE.GFF`, is a 98 x 32 image. Drawn with
`RESOURCE.GFF#PAL/1000`, it is a grey stone plate with a raised border and nothing written on it.

None of the 26 GFF files of the installation holds the bytes `Moves`, `Move :` or `Okay`
anywhere.

## Interpretation

The words the status panel shows are not part of its image or of any GFF resource. They come from
`DSUN.EXE` (FND-COMBAT-022).

## Alternatives

The legacy record searched only for `Moves`, a misreading of the caption (FND-COMBAT-005), with a
tool that reports matches inside resources; the check here covers the whole of each file and the
caption's real text.

## How to reproduce

Decode `BMP/19003` with `PAL/1000` (FMT-IMAGE-001) and look at it. Search each of the 26 `*.GFF`
files of the installation directory for the three byte strings, for example with
`dotnet run --project tools/DarkSunWakeRedux.Inspect -- resource-pattern` or a plain byte search.
