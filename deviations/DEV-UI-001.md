# DEV-UI-001

- Departs from: SCR-UI-001, RULE-VIDEO-001, RULE-VIDEO-003
- Reason: A launch options screen of the rebuild's own opens at every launch before anything of the original's is shown, lists the settings of the other deviations with their values, and starts the original's flow when the player confirms.
- Setting: None
- Default: mandatory
- Justification: The screen adds to the interface and removes nothing: after Confirm the original's flow runs unchanged, with the opening cinematic of RULE-VIDEO-001 and RULE-VIDEO-003 once the rebuild plays it, and then the start window of SCR-UI-001. It is the only place the rebuild's settings are changed, so a setting that skipped it would also hide the way back to it.
- Dropped: no

The owner approved the screen on 2026-09-26.

The screen is drawn on the fixed 320x200 canvas, centred and letterboxed, with the game's
interface font, and with colours and a layout of the rebuild's own that follow no screen of the
original. It lists each option with its current value, then Confirm and Exit. Up, Down and Tab
move the highlight, Left and Right change the highlighted option, Enter or Space changes it or
presses the highlighted button, and Escape quits. The pointer highlights the item under it, and a
left click changes an option or presses a button. Exit quits without showing anything of the
original's.

The values are kept in `settings.json` in the per-user data folder that also holds `UserContent`
(`%LOCALAPPDATA%\DarkSunWakeRedux` on Windows). The file has a version and a size limit of 4,096
bytes, is written through a temporary file that replaces it in one rename, and the last copy that
read back is kept as `settings.json.bak`. Each value is read from the file, then from the backup,
then from its default, so a missing, damaged, oversized or unknown-version file never stops the
game from starting. Confirm overwrites a file with a version this build does not know, such as
one a later build wrote, and keeps no copy of it. The only option today is Wide map view (DEV-EXPLORE-001).
