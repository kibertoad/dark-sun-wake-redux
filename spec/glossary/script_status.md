# script_status

Set to 1 when a script run or a script call starts and to `0xFFFF` when an unknown opcode stops the interpreter; the post-setup initializer also tests it against one after its script call [FND-CONFIG-160]. Other consumers remain open. A `UINT16` the game keeps as a global at `4C0E:0009` in BLD-GOG-EN-1.1 [FND-SCRIPT-005, FND-SCRIPT-007].
