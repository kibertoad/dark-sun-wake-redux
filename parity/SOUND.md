# SOUND

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-SOUND-001` | Voice file with one block of 8-bit samples | supported | missing | None | None | supported | The source manifest lists the installed `.VOC` files as game data; nothing parses them, and the `BVOC` resources are not read. |
| `FMT-SOUND-002` | Music table DJ.DAT | supported | missing | None | None | supported | The source manifest lists `DJ.DAT` as game data; nothing parses it. |
| `FMT-SOUND-003` | STDPATCH.AD files | unknown | missing | None | None | unknown | The two manifest paths have no verified format or reader. |
| `FMT-SOUND-004` | ADV files on the disc | unknown | missing | None | None | unknown | The nineteen disc files have no verified format or reader. |
| `RULE-SOUND-001` | A sound effect plays the BVOC resource of its number, or the installed SOUND file when there is no such resource | supported | missing | None | None | supported | The rebuild plays no sound. |
| `RULE-SOUND-002` | A spoken line plays INTR files from the disc below 50 and SPCH files from the installation or the disc from 50 up | supported | missing | None | None | supported | The rebuild plays no speech. |
| `RULE-SOUND-003` | Music is chosen from DJ.DAT by region at startup and by party health in combat, and plays as a disc audio track | supported | missing | None | None | supported | The rebuild plays no music. The source manifest lists the Ogg files as game data. |
