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

The repository is configured and the first foundation slice is complete. Work
on the second slice includes bounded GFF directory, character identity,
indexed-image, palette,
indexed-font, short-text, and UI layout readers. The supported GOG build can be identified by exact
fingerprints,
asset-pack contracts and diagnostics are implemented, and assetless smoke
testing works. The startup pack now transactionally extracts the original
320x200 title and displays the observed black-backed start window with its two
stone/flame layers and four button images. The controls are placed from the
derived WIND/BUTN graph, and the pack carries the decoded interface font. Bounded
glyph-index run and multiline-block rasterizers can compose its original
palette indices with caller-selected spacing for later text rendering without
guessing the game's character encoding, authentic spacing, or colors.
Deterministic start/party menu semantics, commands, events, snapshots, and
hash-verified replay are implemented in Core, including the manual-documented
psionic-discipline and clerical-sphere creation constraints. The complete
manual origin ability-modifier table is queryable without guessing when the
original applies or caps it. Pre-adventure
party members can be edited atomically or dropped to recreation-native storage
and added back. Manual-evidenced human dual-class level gates, sequential
career state, DUAL selection commands, and deterministic snapshot/hash
persistence are implemented in Core; player-visible selection and native-save
integration are still pending.
The start buttons now accept scale-independent mouse clicks and route through
Core. A shared resolver materializes every extracted start-flow window's typed
control geometry, event mask, and optional image reference for subsequent
screens; their dynamic party content and playable gameplay are not implemented
yet.

| Area | Supported now | Current limitations |
|---|---|---|
| Legal source | Explicit verification of English GOG product `1432903719`, build `52095422060333615` | Other GOG revisions and storefronts are unsupported until separately fingerprinted |
| Asset extraction | Separate Extractor with versioned exact inventory, bounded readers, and transactional 41-asset Slice 2 pack | Title, evidenced start, party-overview, and ADD-list layers, start/generation/ADD controls and modal labels, shared window image, font, text, resolved seven-window UI graph, and bounded character metadata are extracted; dynamic list content, character selection mapping, and later-window shell draw semantics remain incomplete |
| Gameplay | Assetless startup smoke test, MonoGame shell, deterministic party invariants including discipline/sphere choices and origin modifier facts, occupied-slot edit/drop/add, human dual-class progression and selection, and command-driven start flow in Core | No player-visible game slice yet; modifier application/caps, DUAL presentation, disputed origin/class pairs, random generation, and shipped creation defaults await observation |
| Saves and compatibility | Start-flow snapshot schema 4 with class progression, dropped-character storage, and hash-verified deterministic replay | Native save-file I/O, whole-game replays, original saves, and Shattered Lands party transfer are not implemented |
| Presentation | Verified-pack start window composes two measured interface-palette layers and DSUI-derived controls over black; CREATE CHARACTERS reaches the two-layer original party-overview shell; the ADD state renders its measured list shell and controls; all seven extracted window graphs resolve to typed controls | Party portraits/fields, ADD list content/interactions, and other destination screens are incomplete; title sequencing, hit-edge parity, frame states, pixel aspect, animation, audio, and video await observation |
| Text resources | Bounded FONT decoding/DSFT extraction, verified owned-font identity map, deterministic indexed run/block rasterization, and all 62 printable-ASCII `TEXT` records decoded into DSTX | Generalized map semantics, authentic glyph/line spacing, palette, text-ID routing, alignment, and runtime rendering remain open |
| Region data | Read-only bounded catalogs validate all 20 owned regions, including Tyr's exact 128x98 terrain/geometry planes, local 16x16 tiles, and external object references | Region extraction, rendering, collision/geometry semantics, entities, opening placement, and gameplay are not implemented |

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
currently writes the evidenced title, two start-window shell layers, three
party-overview layers, and four start-window buttons as versioned derived
indexed-image assets, verifies their exact inventory and hashes, and
transactionally replaces the previous pack. It never copies the executable or
raw GFF payloads.

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

The four start-window buttons accept mouse clicks through the 320x200 logical
canvas and semantic Core commands. The original manual documents a broader
mouse-first interface plus keyboard shortcuts; exact edge behavior and all
later mappings remain subject to controlled validation.

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
