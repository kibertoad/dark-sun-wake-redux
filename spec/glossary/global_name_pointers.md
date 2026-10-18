# global_name_pointers

Addresses of 15 values the game keeps, which scripts read as variables of kind 9 numbered `0x20` to `0x2E`. A `FARPTR<UINT8>[15]` the game keeps at `4C13:0000` in BLD-GOG-EN-1.1 [FND-SCRIPT-010]. Entries `0x20` to `0x24` point at words in segment `4C10`; `0x21` and `0x22` hold an object's two position words divided by 16 after script opcode `0x09` names it [FND-PARTY-241]. What the others hold is (unknown).
