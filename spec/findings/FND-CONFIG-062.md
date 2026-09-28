---
id: FND-CONFIG-062
title: A failed startup resource-archive open reaches the runtime termination request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0020
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0034
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4448:002C
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV and resident windows; installed-file and build-manifest inspection
environment: null
---

## Observation

The startup routine in overlay 180 selects `RESOURCE.GFF` when
`DS:14E3` is nonzero and `RESFLOP.GFF` when it is zero, then calls the
archive-open wrapper (FND-CONFIG-039). The installed
`BLD-GOG-EN-1.1` directory and its build manifest contain
`RESOURCE.GFF` but no `RESFLOP.GFF`.

At physical file offset `0x00067625`, a zero wrapper return enters a
branch that formats a missing-file message with the selected archive
name and game's directory. It passes the result to the local routine
at `0x00067A47` before the later graphics-initializer call at
`0x000676D9`. That local routine passes the message pointer to
resident `4448:002C`. The resident routine calls `1000:3603` with the
pointer and then calls `1000:03DF` with status 1. The latter reaches
the runtime cleanup and DOS `INT 21h` terminate-process request
(FND-SAVE-011). If the termination request returns, the static code
after the helper call remains reachable; the operating-system outcome
was not observed.

## Interpretation

In the unmodified installed GOG files, selecting the alternate
`RESFLOP.GFF` path cannot open that file from the game directory and
reaches a termination request rather than a normal resource-backed
message state. This is a property of the checked installation and
startup path, not a claim about an edition that supplies that archive.

## Alternatives

A user-supplied `RESFLOP.GFF` or different source installation could
change the open result. The digital-sound flag's value in every launch,
the wrapper's complete file-search behavior, and the operating system's
response to the termination request remain outside this reading.

## How to reproduce

Compare the approved installed directory and
`spec/builds/BLD-GOG-EN-1.1.files.yaml` for the two archive names.
Disassemble `0x000675FD..0x0006765C` for selection and the failure
branch, `0x00067A47..0x00067A83` for the message helper, and
`0x000396AC..0x000396C8` for resident `4448:002C`. Follow its
`1000:03DF` call through the runtime exit path in FND-SAVE-011.
