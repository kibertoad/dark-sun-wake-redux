# EXE

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-EXE-001` | FBOV overlay pack at the end of DSUN.EXE | supported | complete | None | None | implemented | `FbovOverlayProfileReader` checks the marker and reads all four header fields, and requires the payload to end at the end of the file. The rebuild never runs `DSUN.EXE`; it reads the pack only to check the source file. Its tests use synthetic files. |
| `FMT-EXE-002` | FBOV segment-table descriptor | supported | complete | None | None | implemented | `FbovOverlayProfileReader` reads all four words and tallies `flags`. It names `unk_02` and `unk_06` as maximum and minimum offsets, which the spec does not establish; nothing depends on the names beyond a count. |
| `FMT-EXE-003` | FBOV overlay header in the resident image | supported | partial | None | None | supported | `FbovOverlayProfileReader` reads the first 16 bytes of each overlay header (`trap`, `payload_offset`, `code_size`, `fixup_size`) and not `trampoline_count` or the trampolines. |
| `FMT-EXE-004` | FBOV overlay trampoline | supported | missing | None | None | supported | Only the local research tool `tools/ghidra/New-FbovMappedImage.ps1` reads trampolines; the rebuild has no need to. |
| `FMT-EXE-005` | FBOV overlay code block with its fixup list | supported | missing | None | None | supported | Only the local research tool `tools/ghidra/New-FbovMappedImage.ps1` reads the fixup lists; the rebuild has no need to. |
| `FMT-EXE-006` | BAT launch files | supported | missing | None | None | supported | Complete command-text reads and GOG wrapper branches are recorded; bounded compiled dispatch, lookup and cleanup paths are recorded too. FND-EXE-161 through FND-EXE-163 add bounded conditional writer/callback/transfer contracts. Complete caller coverage, encoding and shell outcomes remain Q-EXE-005 through Q-EXE-009. No format-specific reader exists. |
