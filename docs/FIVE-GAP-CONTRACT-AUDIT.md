# Five-gap contract and connected-evidence audit

Audited on 2026-10-05 against the requests in `gaps.md`, the acceptance records,
the active goal, and the current body of project issue 5. This is a tooling
acceptance audit, not a complete reading or a change to any game claim.

The shared dependencies are adopted. This does not complete the active goal:
the goal and issue 5 additionally require connected caller, producer and callee
evidence. Preserve that work and its controls. Distinguish an unmet consumer
exit from an undelivered toolkit feature and from an original-game unknown.

## Original requests and current evidence

| Gap | Original reporting obligation | Evidence already recorded | Connected acceptance still unverified |
| --- | --- | --- | --- |
| 31 | Preserve aliased output writes in order and identify the actual predicate's register/field origin; separate deterministic copies from external result sequences. | Actual reached scratch-word checkpoints retain DX as last writer and interrupt BX as predicate origin. Both documented callers now have complete local CFG and independently checked repeating pointer/predicate structure. Wrong-writer, unread, unscoped and cap controls retain their expected outcomes. | Connected caller-frame/callee preservation and whole controls across repeating routes. Static back-edge coverage does not prove a native finite driver sequence; external results remain unknown. |
| 32 | Check that guards precede and control accesses; retain rejected-path writes/calls, discarded results and checked-snapshot versus fresh-reload provenance. | Actual local metadata ordering, failure writes, normalized zero and both reloads are retained. Engine 10.0.0 follows an instruction-produced far pointer in the released regression and rejects unknown/undeclared targets. | Actual callback pointer/segment producers, intervening callee effects and both whole bracket controls. Synthetic pointer delivery does not establish these original inputs. |
| 33 | Separate leaf/external result origins from recursive propagation and re-encoding; retain finite/valid-state assumptions and do not infer native error or termination from an encoded edge. | Source controls distinguish zero leaves, a conditional recursive FFFF dependency and a fresh immediate, with success unestablished. Normalized/discarded-return and copy-bypass controls retain their qualifications. | Connected admitted graph/count/length evidence, complete caller/leaf routes and actual-copy storage/aliases. Stopped symbolic traversal does not prove termination or absence of an error origin. |
| 34 | Separate input bounds, generated cardinality and destination capacity; follow append/split gates and each copy/terminator base, retaining producer and alias assumptions. | Local append-capacity, conditional pair overflow, syntactic split bound and copy/terminator-base distinctions are recorded. Actual count-one caller roots retain setup/copy provenance into normalization. | Known nonzero connected append cardinality and whole pair/split/copy/terminator controls. Count-one input provenance alone does not establish output cardinality, native geometry, safety or rollback. |
| 37 | Classify common/conditional port I/O as hardware boundaries; distinguish RAM effects from pixels and state substituted-port/RAM assumptions separately from local completion/restoration. | Published actual-primitive bounds and an independent CFG now agree on common/conditional hardware-site placement. The actual wrapper/validator/getter/primitive call graph resolves, with omitted-body and limit negatives. Synthetic harness controls and the actual no-port initializer remain separate. | Actual connected transfer placement, caller/input and later-writer coverage required by issue 5. The initializer cannot stand in for a transfer-boundary positive, and RAM substitution cannot prove native rendered output. |

These evidence descriptions come from `TOOLKIT-RESPONSE-ACCEPTANCE.md`,
`LATEST-RELEASE-GAP-AUDIT.md`, `TRANSFER-CALLER-PRODUCER-AUDIT.md` and their named
local reports. They preserve the reports' incomplete/conditional verdicts;
this audit has not rerun every case or promoted a local hold to a whole hold.

## Delivery and migration are separate checks

Current GitHub issue state confirms toolkit requests 143, 190, 198, 200, 213
and 274 are closed. Their delivered work is recorded in the release acceptance
documents. Closure of those issues is evidence of disposition, not evidence
that all original-game consumer exits pass. No new missing capability is
demonstrated by this audit.

The original requests explicitly preserve unknown external results, finite
traversal assumptions, input invariants and hardware output. They do not ask
the library to establish every game's entire startup or resource history.
Later acceptance records and issue 5 added the stronger connected-evidence
work. That work remains part of the active goal; this distinction does not
delete it or lower its exit.

A game unknown blocks migration only where the migrated consumer relies on
the corresponding unproved claim. The release/adoption checks already pass,
but a complete audit of such reliance has not been supplied here. Therefore
neither "all five are undelivered library gaps" nor "all five are irrelevant
and complete" is justified. Track delivered features, pending connected
acceptance and original-game uncertainty separately.

## Next evidence to obtain

For each row, compare the named source report and its executable controls with
the original reporting obligation and the stronger connected exit. Record a
whole pass only at the scope it actually covers. An explicit unknown may
satisfy a reporting qualification; it cannot satisfy a required connected
positive or a native-behavior claim.

For Gap 37, start with the actual primitive's hardware-site placement and
caller arguments. The existing initializer control reaches no ports. Require
a specific missing producer dependency before expanding the archive/startup
chain further; reading unrelated resource history is not itself a transfer
acceptance result. Keep previous partial reports, unresolved scopes and
negative controls intact. No new owner-run request or toolkit issue follows
from this audit alone.

## Connected Gap 37 caller graph

The published callee query now covers the actual FND-CONFIG-186 caller's
request, transfer and cleanup children, including the hardware-bearing release.
Its declared graph is complete with no unresolved edges or unchecked entries;
request/release omissions and an instruction cap lose graph completion.
This removes missing direct-callee coverage within that scope. Dynamic whole
controls, admitted slot/count inputs and preservation against storage/stack
aliases remain incomplete. See TRANSFER-CALLER-PRODUCER-AUDIT.md; no full exit
is closed by this source-local graph result.

## Subsequent focused control

The actual Gap 37 mask prefix now retains port-number, selector-byte and
mask-read producer occurrences with omission/cap controls. Its numeric masked
index bound was undecided in engine 10.0.0. PR 293 delivers toolkit issue 290
in engine 10.1.0: the complete returning synthetic case now holds, and the
actual masked-index occurrence holds. Omission and cap controls retain their
negative outcomes; the stopped actual query's whole verdict stays undecided.
This removes that specific demonstrated capability limitation. See the latest
section of TRANSFER-CALLER-PRODUCER-AUDIT. It does not complete any of the five
whole connected exits or establish native mask contents/hardware output.

## Gap 32 independent callback reference forms, 2026-10-06

The known callback-field writer already has stacked-input and literal-candidate
controls; the earlier caller census found no confirmed direct caller with its
exclusions retained. A fresh function-boundary-independent resident-byte search
now checks relative near call/jump encodings, including word-IP wrap candidates,
and raw far call/jump representations for that writer. Known resident near and
far calls are positive controls. No target candidate is retained in those
specific resident encoding domains.

A separate whole-file word census retains bare writer-offset candidates. Their
bounded linear contexts do not establish a pointer-producing instruction or a
native reference to this writer. Linear decoding cannot classify embedded data
or prove entry-path ownership, and an offset without its segment does not name
this target. No candidate is promoted to a native pointer producer.

The negative remains unusable as an absence-of-callers claim. Computed or
indirect transfers, synthesized pointer parts, overlay/native mapping and
unclassified data references remain outside the verified domains. This adds an
independent check to the earlier boundary-based census; it neither proves the
writer dead nor admits its unknown stacked callback pointer.

Private source/identity reports and candidate contexts:
GAME_DIR/analysis/reporter-audit/issue5-callback-reference-forms100. Ignored
drivers: artifacts/engine100/callback-reference-forms.mjs and
callback-reference-word-contexts.py. The unchanged capped consumer/writer
queries were not repeated. Next work needs a verified constructed/computed
reference or the actual bracket's producer/effect dependency; another identical
literal or direct-caller census cannot complete the connected callback exit.
All five complete contracts and native uncertainty qualifications remain intact.
