# script_menu_targets

The target offset of each entry the menu instruction has read, indexed by how many entries it had offered before. A `UINT16[12]` the game keeps at `4C13:027D` in BLD-GOG-EN-1.1, directly before `script_frame_offsets`, which indexes 12 and up overwrite [FND-TALK-001, FND-SCRIPT-006].
