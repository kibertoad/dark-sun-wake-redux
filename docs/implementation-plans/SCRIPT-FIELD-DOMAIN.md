# Decoded script field-path domain

Research tooling for Q-PARTY-115 and shared field-accessor producer work.
FND-PARTY-234 maps the script setter wrappers to field-path words;
FND-PARTY-238 gives the fixed/type/list/transfer setter families;
FND-PARTY-239 supplies prior path counts and positive field controls.
FND-COMBAT-100 retains the GPL/37 decoder gap, which must stay visible.

Add a reusable bounded query over the existing indexed script listing,
not another archive parser or decoder. Require listing SHA-256, total
paths, empty/omitted list counts, unique final gap count, target field and
independent positive-field occurrence controls. Parse only canonical indexed
instructions and explicit decoder-error records; count nested field paths
within an instruction once, keep all selectors and gaps, and reject malformed
path forms or missed controls. Do not infer execution or analyze gap bytes.

Synthetic cases cover a target field after another selector, positive fields,
empty and omitted lists, malformed numbers/forms, wrong identity/counts,
duplicate/unsorted instruction records and paired gap diagnostics. Bound
input, lines, paths and configuration. Exit: selected tooling controls pass
and committed procedure, then a separate finding applying the result to
field eleven under the existing metadata/wrapper preservation conditions.
Computed/aliased targets, runtime list/stream mutation and gap paths remain
outside any unchanged decoded-domain exclusion.
