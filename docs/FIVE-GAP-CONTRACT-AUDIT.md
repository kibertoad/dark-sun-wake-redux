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
| 31 | Preserve aliased output writes in order and identify the actual predicate's register/field origin; separate deterministic copies from external result sequences. | Actual reached scratch-word checkpoints retain DX as last writer and interrupt BX as predicate origin. Wrong-writer, unread, unscoped and cap controls retain their expected outcomes. | Complete caller-frame and repeating-route coverage. External result sequences remain unknown rather than becoming invented inputs. |
| 32 | Check that guards precede and control accesses; retain rejected-path writes/calls, discarded results and checked-snapshot versus fresh-reload provenance. | Actual local metadata ordering, failure writes, normalized zero and both reloads are retained. Engine 10.0.0 follows an instruction-produced far pointer in the released regression and rejects unknown/undeclared targets. | Actual callback pointer/segment producers, intervening callee effects and both whole bracket controls. Synthetic pointer delivery does not establish these original inputs. |
| 33 | Separate leaf/external result origins from recursive propagation and re-encoding; retain finite/valid-state assumptions and do not infer native error or termination from an encoded edge. | Source controls distinguish zero leaves, a conditional recursive FFFF dependency and a fresh immediate, with success unestablished. Normalized/discarded-return and copy-bypass controls retain their qualifications. | Connected admitted graph/count/length evidence, complete caller/leaf routes and actual-copy storage/aliases. Stopped symbolic traversal does not prove termination or absence of an error origin. |
| 34 | Separate input bounds, generated cardinality and destination capacity; follow append/split gates and each copy/terminator base, retaining producer and alias assumptions. | Local append-capacity, conditional pair overflow, syntactic split bound and copy/terminator-base distinctions are recorded. Actual count-one caller roots retain setup/copy provenance into normalization. | Known nonzero connected append cardinality and whole pair/split/copy/terminator controls. Count-one input provenance alone does not establish output cardinality, native geometry, safety or rollback. |
| 37 | Classify common/conditional port I/O as hardware boundaries; distinguish RAM effects from pixels and state substituted-port/RAM assumptions separately from local completion/restoration. | Static transfer findings identify VGA and scratch boundaries. Synthetic harness controls cover named ports, missing services and limits. The actual resident initializer has separate known-answer and byte/register controls; it reaches no hardware boundary. | Actual connected transfer placement, caller/input and later-writer coverage required by issue 5. The initializer cannot stand in for a transfer-boundary positive, and RAM substitution cannot prove native rendered output. |

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
