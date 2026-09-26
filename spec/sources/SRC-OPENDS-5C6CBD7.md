---
id: SRC-OPENDS-5C6CBD7
title: OpenDS, a Dark Sun reverse-engineering project with a GPL disassembler, commit 5c6cbd7
superseded_by: []
author: Brandon LaRocque
date: "2026-09-13"
location: https://github.com/VirInvictus/opends/tree/5c6cbd7d23f89f090d2e0005ad603d75ffb14433
xxh3: null
licence: MIT
---

## Use

A reverse-engineering project for Dark Sun: Wake of the Ravager. Its `gpl-disasm` tool, version
0.8.0 at this commit, disassembles `GPL ` and `MAS ` resources, and its `docs/gpl-bytecode.md`,
`docs/gpl-opcodes.md`, `docs/dispatch-table-ds2.md` and `docs/engine-quirks.md` describe the
instruction encoding, the parameter shapes of the 129 instructions `0x00` to `0x80`, the variable
kinds and the `GPLI` index. Its opcode names and parameter shapes are taken from
SRC-LIBGFF-839B11D, so the two count as one line of evidence. It supports a claim only as a
secondary source next to a reading of this build's data or executable.

The disassembly the project's research used was made with this tool from a checkout at this
commit; the research record did not name the commit, and this is the checkout that was on disk
when it was written.

## Known errors

Its names for instructions `0x26`, `0x4A`, `0x4C` to `0x4E`, `0x53`, `0x55` to `0x57`, `0x60` and
`0x71` to `0x75` are placeholders; in this build all fifteen stop the script (FND-SCRIPT-005).
