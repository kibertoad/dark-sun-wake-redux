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

## Connected conversion observation

Add a separate fixed saved-trigger-conversion stage combining the existing
SAVE/38 and script-conversion observations in one admitted saved-map run.
Keep the existing transfer-only stage's early stop. Cache the verified
2600-byte private transfer reference in the probe, then require the current
trigger pointer and all input bytes at the existing pre-conversion snapshot
to match it. At matched reader return compare SAVE/38's destination against
the converter prediction, not the serialized source bytes. Keep the earlier
transfer's private-span comparison separately recorded.

Reuse the established breakpoint, provider, mapping, frame, table, muted
session and owned-process controls. Add no guest write, input, arbitrary
address or breakpoint. Synthetic controls must reject duplicate/late source
binding, wrong lengths, changed pointers or changed input bytes. Run the
tooling gate before a native attempt. Exit requires one run to connect the
verified transfer, conversion input and predicted output at the sampled
boundaries. This still does not establish continuous lifetime, all writes
between stops, other inputs or failure behavior.

The first connected native attempt stopped at the existing overlap checker:
its arithmetic span allowlist omitted the newly admitted 2600-byte target.
Admit that fixed extent for overlap arithmetic only; the map reader's
12544/1568-byte input limits stay unchanged. Add overlapping, disjoint,
wrapping and unsupported-size controls before retrying with fresh output.
The stopped attempt remains a tooling diagnostic, not a conversion result.

Accepted connected result: FND-SCRIPT-065 records one run with source-equal
transfer, matching pointer/input at the later conversion snapshot, and
predicted output before release and at reader return. Its limits and the
historical prediction-hash field label are recorded in that finding.
