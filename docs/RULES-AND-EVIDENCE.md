# Rules and evidence

## Sources

| ID | Source | Type | Use | Confidence |
|---|---|---|---|---|
| `MANUAL-1994` | `C:\GOG Games\Dark Sun 2\ds_wakerave_manual_pdf.pdf` (local only) | original rule book | Intended controls, menus, party and character rules, combat commands, magic, psionics, advancement, credits | high for documented intent; not proof of shipped edge cases |
| `GOG-1432903719` | English GOG build `52095422060333615` | owned release | Exact source fingerprints and future runtime/data observations | verified for recorded hashes and metadata |
| `FAQ-81038` | kibbitz, GameFAQs guide v1.13 | community research | Mechanics, route/branch index, reported bugs and manual conflicts | medium; confirm with controlled observations |
| `DSUN-MUSIC` | John Glassmyer, [`dsun_music`](https://github.com/JohnGlassmyer/dsun_music), MIT licensed | community technical research | GFF, image, region, and XMI structure and resource identification | medium; confirm each applicable result against the fingerprinted owned build |
| `OBS-GOG-*` | Future controlled runs | runtime observation | Player-visible state transitions, coordinates, timing, outcomes | unknown until recorded per finding |
| `DATA-GOG-*` | Bounded inspection of the owned build | data fact | Format fields and resource relationships | recorded per finding |

### DATA-GOG-GFF-001 - GFF container directory

- **Question:** How are resource identities, offsets, and lengths represented in
  the owned build's `.GFF` containers?
- **Method:** Fingerprint the supported edition, inspect bounded header/index
  windows, compare the DSUN-MUSIC GFF results, implement an independently
  bounded reader, and enumerate metadata from every installed `.GFF` without
  retaining payload bytes.
- **Finding:** Version `0x00030000` uses a 28-byte header and little-endian
  primary/secondary tag tables as specified in `docs/ORIGINAL-FORMATS.md`.
  All 26 installed GFF files parse, producing 16,168 bounded descriptors.
- **Confidence:** verified for directory structure and counts in
  GOG-1432903719; tag payload semantics remain unknown unless separately
  recorded.
- **Implementation:** `DarkSunWakeRedux.Resources.GffArchive` and the read-only
  `DarkSunWakeRedux.Inspect gff` command.
- **Tests:** synthetic primary, primary-only, segmented-secondary, truncation,
  out-of-range, and partial-overlap cases.

## Initial rules

### RULE-INPUT-001 - Mouse-first interaction

- **Behavior:** The original requires a mouse and uses cursor modes for walking,
  looking, attacking, and targeted actions. Escape exits the active menu.
- **Preconditions:** Relevant exploration or menu state is active.
- **Evidence:** MANUAL-1994, introduction and "How to Play".
- **Confidence:** high for intended behavior; exact hit regions and shipped edge
  behavior are unknown.
- **Implementation:** not implemented.
- **Tests:** planned semantic-input and menu-routing tests.
- **Uncertainty:** Cursor art, complete mode transitions, right-click behavior,
  and coordinate boundaries require OBS-GOG evidence.

### RULE-PARTY-001 - Four-character party

- **Behavior:** The start flow supports a pre-generated party or creation and
  selection of four characters.
- **Evidence:** MANUAL-1994, quick-start and party-creation sections.
- **Confidence:** high for intended party size and flow.
- **Implementation:** not implemented.
- **Tests:** planned party-size and selection invariant tests.
- **Uncertainty:** Exact defaults, random generation, cancellation, and invalid
  combinations require observation.

### RULE-COMBAT-001 - Party expansion on combat entry

- **Behavior:** Exploration may show only the leader; combat makes all four party
  members visible. The manual also documents wait, guard, target cycling, and
  end-turn commands.
- **Evidence:** MANUAL-1994, "How to Play" and hotkeys.
- **Confidence:** high for intended commands; resolution order and formulas are
  unknown.
- **Implementation:** not implemented.
- **Tests:** planned deterministic combat command traces.
- **Uncertainty:** Activation order, RNG, THAC0/AC details, timing, and difficulty
  effects require OBS-GOG and possibly targeted Ghidra evidence.

## Conflict handling

FAQ-81038 reports discrepancies between documentation and shipped behavior.
Each conflict receives its own rule ID, both claims, reproduction procedure, and
owner decision. No compatibility behavior is selected from plausibility alone.
