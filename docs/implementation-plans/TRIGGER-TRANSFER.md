# Fixed SAVE/38 transfer observation

Tooling for Q-SCRIPT-025, reusing FND-SCRIPT-057/059's static pointer
bindings, FND-COMBAT-035's reader-local in/out slot and FND-CONFIG-151's
complete-resource service. Existing provider observations accept SAVE/24
and SAVE/26 only; FND-SCRIPT-058 samples the trigger binding after GPLI
transfer, too late to observe SAVE/38's own transfer destination.

Add the fixed saved-trigger-transfer stage to the existing native saved-map
route. Preserve its checkpoint identity, copy/open/provider/directory guards,
session lock, pinned muted rust-dos and existing navigation. Admit SAVE/38
as a third fixed target only for this stage, after 24 and 26. Require its
2600-byte selected payload to match the verified private working-file span.
At request entry, selection and matched return, record the reader-local
destination slot, prepared DS:1612 slot and live DS:40C0 pointer, comparing
both pointer words. Read these fixed supported fields without following
their values except through the existing selected-payload reader.

Stop after the matched successful SAVE/38 return and payload comparison,
before conversion. No new guest write, input, arbitrary address or process
interface is added. A null/mismatched binding rejects this fixed normal-route
observation; it does not authorize following another destination. Existing
stages retain their two-target behavior. Outputs remain in GAME_DIR.

Synthetic controls cover opt-in target admission, wrong target/frame/count,
matching/mismatched slot words, bounded 2600-byte payload and rejection of
unsupported lengths. Run the tooling gate before using the new stage.
Exit: controls pass and the fixed stage records these comparisons or a
specific refusal. A native result requires its own finding and establishes
only the observed route/boundaries, not continuous lifetime or failure paths.

Native result: FND-SCRIPT-061 records the accepted fixed route, matching
three pointer fields at entry/selection/return and all 2600 selected bytes.
The session stopped before conversion; later preservation and other
inputs/failures remain Q-SCRIPT-025.
