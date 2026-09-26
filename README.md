# Dark Sun: Wake of the Ravager Redux

A clean-room, cross-platform MonoGame restoration of Strategic Simulations'
1994 party-based computer role-playing game *Dark Sun: Wake of the Ravager*.

This repository contains no copyrighted resources from the original game. The
standalone Asset Extractor requires and verifies a supported, legally owned GOG
copy, then will decode its proprietary resources into a local asset pack. The
reimplemented runtime consumes only that verified pack; it never runs or depends
on the original executable or DOSBox.

On Windows, run `play.bat` from the repository root to build the solution,
create or verify the local asset pack, and launch the current game build. Set
`DARK_SUN_WAKE_PATH` only when the owned installation is not at the documented
default `C:\GOG Games\Dark Sun 2` location.

## Terminology

The original game and its manuals use **race** for fantasy character origins
such as humans, elves, dwarves, and similar peoples. That historical fantasy
usage is not intended here in an offensive or real-world racial sense. To keep
the reimplementation's modern terminology respectful, new code, APIs, UI, and
project-authored prose use **origin** instead. When an evidence record quotes or
names an original manual heading or table that says **race** or **racial**, read
that term as **origin** or **origin-based** in our implementation.

## Current status

The repository is configured, foundations and startup-flow work are in place,
and Slice 3 is active. The supported GOG build is identified by exact
fingerprints and converted transactionally into a verified local-only pack.
Bounded readers cover the current GFF, image, palette, font, text, character,
region, object, and UI subsets. Core implements deterministic party creation,
human dual-class progression, snapshots/replay, exploration view state,
collision-aware pathfinding, occupancy, and actor movement.

The runtime enters the observed Tyr viewport, edge-scrolls or grab-drags the camera,
changes mouse modes, toggles native-resolution fullscreen with Alt+Enter, captures
the physical backbuffer with F12, expands
the world slice to fill that display, and previews the measured dialogue chrome
plus script-derived opening text over the expanded map with F9,
opens the authentic Game Menu, moves the exact opening leader through a modern
deterministic route, and renders character, inventory, Cast/Use, Current Effects,
and Preferences shells with shared navigation. The original Walk/Attack/Look
cursor family now renders with reachable/eligible target feedback. Dynamic destination content,
native in-world integration of the bounded opening-conversation preview, and
the rest of the campaign remain unfinished. The first hostile Look panel and
first conversation layouts are now
resource-mapped, and Core models their interaction action boundary, but they are
not rendered yet.

[`PARITY.md`](PARITY.md) gives the rebuild's status against every rule, format
and screen in the [spec](spec/README.md), with one file per area in `parity/`.
[`deviations/`](deviations/) records where the rebuild departs from the original
on purpose. The parts of the rebuild that have no spec entry stand as follows:

| Part | Status |
|---|---|
| Source recognition | Implemented. One English GOG build has an exact 233-file immutable inventory, and all 279 installed files have a game-data, mutable, wrapper or documentation disposition. Synthetic mismatch tests pass and the owned build verifies. |
| Asset pack | Implemented. Required revision 35 keeps all 233 source files and every one of the 16,168 GFF descriptors as 16,401 DSOP assets, plus 123 specialized derivatives (16,524 assets). The pack contract checks version, game and source identity, inventory, hashes, provenance, media type, conversion and unexpected files. |
| Error behavior | Implemented. A missing or invalid pack and a source mismatch return a diagnostic that says what to do. |
| Saves and replays | Partial. Start-flow snapshot schema 5 and replay format 3 with hash-verified replay are implemented in memory. Native save files, migration and whole-game coverage are not started. |
| Packaging | Identity configured. Packages are not release-ready while decoders are missing. |

Every launch opens on the rebuild's own options screen, before anything of the original's. It sets
the options that the deviations offer, today only Wide map view, and keeps them in `settings.json`
in the per-user data folder beside `UserContent` (`%LOCALAPPDATA%\DarkSunWakeRedux` on Windows).

## Developer quick start

Install the .NET SDK pinned by `global.json`. To verify the supported owned copy:

```powershell
dotnet run --project src/DarkSunWakeRedux.Extractor -- `
  verify-source --source "C:\GOG Games\Dark Sun 2"
```

Run the content-free smoke test and complete repository checks with:

```powershell
dotnet run --project src/DarkSunWakeRedux.Game -- --smoke-test
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Test.ps1
```

Normal runtime startup requires a fully verified extracted pack. `extract`
writes the evidenced startup/UI, character, text, Tyr region/object, and all-region structural
subsets as versioned derived assets, verifies their exact inventory and hashes,
and transactionally replaces the previous pack. It never copies the executable
or raw GFF payloads.

## Architecture

- `DarkSunWakeRedux.Extractor` verifies the licensed source and owns safe,
  transactional asset-pack creation.
- `DarkSunWakeRedux.Resources` owns bounded original-format readers and pack
  contracts.
- `DarkSunWakeRedux.Core` owns deterministic rules and serializable state.
- `DarkSunWakeRedux.Game` is the MonoGame presentation runtime and reads only a
  verified extracted pack.
- `DarkSunWakeRedux.Inspect` is read-only research tooling.

The roadmap and evidence gates are in
[docs/IMPLEMENTATION-PLAN.md](docs/IMPLEMENTATION-PLAN.md). The current
continuation notes are in [docs/HANDOVER.md](docs/HANDOVER.md).
For a concise map of the verified architecture, source contracts, static
analysis boundaries, timing policy, and evidence gates, see
[docs/TECHNICAL-REFERENCE.md](docs/TECHNICAL-REFERENCE.md).

## Controls

Start, Game Menu, Preferences navigation, and shared character/inventory/Cast/Effects controls accept
mouse clicks through the 320x200 logical canvas and semantic Core commands.
Exploration supports documented view hotkeys, Walk/Attack/Look cycling,
edge-scroll input, party display selection, click-to-walk, and resource-exact
valid/invalid cursor feedback at the documented upper-left hotspot. Broader target
eligibility, frame states, and many later actions still require controlled validation.

## Acknowledgements

First and foremost, thank you to Strategic Simulations, Inc. and the complete
original *Dark Sun: Wake of the Ravager* team. The rule book credits story and
game design to the **SSI Special Projects Team**; producers **Dan Cermak** and
**Nick Beliaeff**; associate producer **Rick White**; lead programmer **Robert
Calfee** and programmer **Mike Coustier**; additional programmers **Russ Brown**,
**Keith Brors**, **Doug Grounds**, and **John Miles**; data manager **Caron
White**; region designers and coders **Chris Carr**, **Don McClure**, **Adam
Isgreen**, and **Ken Eklund**, with support and testing by **Chris Lanka** and
**Steven Okano**; and lead artist **Maurie Manning** with **Ben Rush**, **Gennady
Krakousky**, **Diane Fong**, **Diane Duffey**, and **Dante Fuget**.

Audio and performance credits belong to **Ralph "Cooksey" Thomas**, **Ron
Calonje**, **Tim August**, **The Fat Man**, **Nada Lewis**, **Wally Fields**,
**Dianne Nola**, **Greg Walsh**, **Brian Session**, **David Hatch**, **Merle
Madlin**, **Doug Bushman**, **Michael London**, **Adam Gropman**, and **Ali
Katz**. Documentation, testing, and production-art credits include **Eileen
Matsumi**, **Al Brown**, **Jonathan Kromrey**, **Andre Vrignaud**, **Glen
Cureton**, **Cyrus G. Harris**, **Bill White**, **Ben Cooley**, **Mike
Klingler**, **Zane Wolters**, **Al Marenco**, **Kelly Calabro**, and **Louis
Saekow Design**, including **Leedara Zola** and **Dave Boudreau**. The original
manual gives special thanks to **James M. Ward** and **Roberta Gibbons**.

Special thanks to **kibbitz** for the exceptionally detailed
[*Dark Sun: Wake of the Ravager - Guide and Walkthrough*](https://gamefaqs.gamespot.com/pc/564927-dark-sun-wake-of-the-ravager/faqs/81038).
Its credited research contributors include **Seraphiel**, **@revcrussell**,
**UndeadHalfOrc**, **classiccola**, **GHostLPs**, and **rattus 128**. Their work
is treated as secondary evidence and checked against controlled observations
where possible.

Special thanks also to **John Glassmyer** for the MIT-licensed
[`dsun_music` project](https://github.com/JohnGlassmyer/dsun_music), including
its GFF, image, region, and XMI research tools for the Dark Sun games. This
restoration uses useful format and resource-identification results from that
project as credited secondary technical evidence, validates them against the
fingerprinted owned copy, and records any direct code reuse under its license.

These acknowledgements do not imply that any original creator, rights holder,
storefront, guide author, or contributor endorses this project.

This project copies no original source code and redistributes no copyrighted
resources from the original game. Players must extract those resources locally
from a supported legally owned copy, such as the
[GOG release](https://www.gog.com/en/game/dungeons_dragons_dark_sun_series).

## License

Copyright (C) 2026 kibertoad.

The original code in this repository is licensed under the [MIT License](LICENSE).
The license does not cover or grant rights to original-game assets.

Developer setup is documented in [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md), and
packaging and releases are documented in [docs/RELEASING.md](docs/RELEASING.md).
