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
not rendered yet; status claims below and in the parity matrix intentionally keep
those boundaries explicit.

| Area | Supported now | Current limitations |
|---|---|---|
| Legal source | Explicit verification of English GOG product `1432903719`, build `52095422060333615` | Other GOG revisions and storefronts are unsupported until separately fingerprinted |
| Asset extraction | Separate Extractor with an exact 233-file immutable inventory, bounded readers, and transactional full-corpus pack | All 233 immutable source files and 16,168 GFF resource descriptors are preserved locally as 16,401 verified DSOP assets, alongside 123 specialized DSIX/DSGP/DSTX/DSUI/DSCH/DSRG/DSOB derivatives (16,524 assets total). The additional combat-status-panel DSIX preserves only confirmed BMP #19003 artwork and placement; it does not define an overlay or combat behavior. Twenty source-derived DSRG catalogs validate every owned `RGN*.GFF` archive but do not enable travel or rendering. The runtime never loads opaque bytes. A one-way required extraction revision rejects stale local output and has no backwards-compatibility or migration role. Existing specialized readers project the evidenced opening dialogue without committing original text; dynamic destination content, broader actor animation, later dialogue branches, and semantics for remaining resources are incomplete |
| Gameplay | Assetless startup smoke test, deterministic party/start flow, the observed Tyr viewport with bounded original edge scrolling plus owner-tuned 1.3x right-button grab-drag panning, native-resolution Alt+Enter fullscreen with an aspect-expanded world slice, mode/display controls, documented hotkeys, an authentic clickable Game Menu including clean Exit, Center on Leader, Collapse Party, and Preferences, shared navigation across character/inventory/Cast/Effects screens, original Walk/Attack/Look valid/invalid cursors, evidenced `GMAP` terrain collision, and runtime click-to-walk using stable A*, atomic occupancy, the evidenced leader anchor, bounded fixed-step advancement, and fixed-point visual interpolation | F9 provides a temporary resource-backed dialogue preview with the first literal speech and the observed five-response opening page rendered in the shipped font over the live world. The observed state selects original choices 0, 1, 2, 3, and 7; menu exit labels resolve from extracted MAS #99 global strings #5/#6. Clicking a response records its original choice index and branch offset in deterministic Core state and consumes the world click. Choices 0-4 execute bounded returned effects and the exact opening continuation: choices 0/1 advance to the conditionally filtered second menu, choices 2/3 increment the counter and collectively reveal choice 4 on the opening menu, and choice 4 resets the counter, sets local flag 9, and advances. Choice 1 also conditionally sets local flags 6/7 from fresh-opening global flag 357 and then sets that global flag. Returned branches replace the speech with their projected transcript; the shared identity target stays on its calling page. The second-menu trouble branch clears local flag 6 and conditionally enables the king question from fresh-opening local flag 16; the king branch then sets flag 16, clears its own flag 10, and returns. Target 1996 selects its transcript from `GNUM22` and `GNUM84 & 2`: the equals-one path sets flag 11, while the alternate path applies `GNUM84 |= 1`; both clear flag 7 and return. The resulting Acar branch clears flag 11 and returns. Target 2415 sets local flags 12/13, conditionally derives flag 10 from flag 16, and advances to the filtered third menu; on the implemented owned path it displays choices 0, 1, and 6. Targets 2921, 3089, and 3257 successively consume flags 12/13/15 and contract the page to source choice 6. Alternate targets 3686/3786 consume flags 17/18 and are also target-dispatched. Target 3976 clears flag 8, derives flag 14 from the exact post-assignment six-flag condition, and completes the third page; the first two menus' shared exit also completes the session. Unknown variables still fail closed, and visible unimplemented targets remain inert; broader GPL consequences remain pending. Load/Save, Preferences setting changes, remaining destination screens, and destination content/actions remain pending; target eligibility is bounded to reachable Walk cells, displayed Look entities/leader pixels, and the first observed melee entity, while broader interaction semantics remain open; the opening leader currently uses an explicit provisional single-cell footprint and 125 ms semantic step, while native footprint/cadence, sprite-frame animation semantics, NPCs, remaining party/interface rendering, modifier application/caps, DUAL presentation, disputed origin/class pairs, random generation, and shipped creation defaults remain open |
| Saves and compatibility | Start-flow snapshot schema 5/replay format 3 with an explicit unresolved-shipped-party boundary, class progression, dropped-character storage, and hash-verified deterministic replay | Native save-file I/O, whole-game replays, original saves, Shattered Lands party transfer, and the complete shipped-party membership are not implemented |
| Presentation | Verified-pack start, party-overview, ADD-list, 210x116 Game Menu/Preferences, character, inventory, Cast/Use, and Current Effects shells compose original indexed assets through typed DSUI controls; travel fills the display by exposing more map at wide/tall aspect ratios while fixed screens remain centered and letterboxed; the F9 preview draws the measured portrait/dialogue chrome and bounded script-derived text on that fixed canvas over the expanded world | Dialogue wrapping remains provisional. Response rows use the captured opening state, are selected deterministically in source order, retain original choice/branch identities, route clicks into Core, switch among the projected first, second, and third menus, and present choices 0-4, the shared identity target, and targets 1825/1996/2352/2415/2921/3089/3257/3479/3686/3786 in the speech area with explicit newlines retained. Target 3976 completes and closes the preview after its output path is validated; generic native variable initialization and selected/hover frame feedback remain unknown. The centered menu origin is provisional; dynamic destination fields and most controls, party portraits, ADD content/actions, remaining destinations, title sequencing, frame states, pixel aspect, animation, audio, and video await observation |
| Text resources | Bounded FONT decoding/DSFT extraction, verified owned-font identity map, deterministic indexed run/block rasterization, and all 62 printable-ASCII `TEXT` records decoded into DSTX | Generalized map semantics, authentic glyph/line spacing, palette, text-ID routing, alignment, and runtime rendering remain open |
| Region data | Read-only bounded catalogs validate all 20 owned regions and all 4,479 object-frame definitions; Tyr's DSRG/DSOB graph feeds a clipped compositor, controlled observation validates the opening `(1024,1368)` background, uniquely identifies and displays its exact leader sprite, and executable evidence identifies `GMAP` bit `0x40` plus opening anchor cell `(74,91)` | Other geometry bits, concrete actor footprints, animation, entity behavior, remaining party/interface rendering, scroll timing, and later camera anchors remain open |

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

The approved roadmap and evidence gates are in
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
