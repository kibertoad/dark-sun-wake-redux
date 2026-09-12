# Dark Sun: Wake of the Ravager Redux

A clean-room, cross-platform MonoGame restoration of Strategic Simulations'
1994 party-based computer role-playing game *Dark Sun: Wake of the Ravager*.

This repository contains no copyrighted resources from the original game. The
standalone Asset Extractor requires and verifies a supported, legally owned GOG
copy, then will decode its proprietary resources into a local asset pack. The
reimplemented runtime consumes only that verified pack; it never runs or depends
on the original executable or DOSBox.

## Current status

The repository is configured and the first foundation slice is complete. The
next slice will implement bounded decoders for the verified original formats.
The supported GOG build can be identified by exact fingerprints, asset-pack
contracts and diagnostics are implemented, and assetless smoke testing works.
Game-specific resource decoders and playable gameplay are not implemented yet,
so the Extractor deliberately writes no output.

| Area | Supported now | Current limitations |
|---|---|---|
| Legal source | Explicit verification of English GOG product `1432903719`, build `52095422060333615` | Other GOG revisions and storefronts are unsupported until separately fingerprinted |
| Asset extraction | Separate `DarkSunWakeRedux.Extractor` executable; versioned exact-inventory pack contract and actionable diagnostics | Dark Sun resource decoders and transactional pack creation are planned, not implemented |
| Gameplay | Assetless startup smoke test and MonoGame shell | No player-visible game slice yet |
| Saves and compatibility | Deterministic Core seed/state scaffold | Native saves, replays, original saves, and Shattered Lands party transfer are not implemented |
| Presentation | Window and startup-failure reporting scaffold | Original resolution, graphics, animation, audio, video, and controls await observation and extraction |

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

Normal runtime startup requires a fully verified extracted pack. Until the
first decoders land, `extract` verifies the licensed source, reports that the
decoders are unavailable, leaves any existing pack unchanged, and exits with a
failure code rather than copying raw original files.

## Architecture

- `DarkSunWakeRedux.Extractor` verifies the licensed source and owns safe,
  transactional asset-pack creation.
- `DarkSunWakeRedux.Resources` owns bounded original-format readers and pack
  contracts.
- `DarkSunWakeRedux.Core` owns deterministic rules and serializable state.
- `DarkSunWakeRedux.Game` is the MonoGame presentation runtime and reads only a
  verified extracted pack.
- `DarkSunWakeRedux.Inspect` is read-only research tooling.

The approved roadmap and evidence gates are in
[docs/IMPLEMENTATION-PLAN.md](docs/IMPLEMENTATION-PLAN.md).

## Controls

No playable controls are implemented yet. The original manual documents a
mouse-first interface plus keyboard shortcuts; mappings will be added only as
their screens become playable and validated.

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
