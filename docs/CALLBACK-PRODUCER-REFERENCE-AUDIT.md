# Callback writer reference follow-up, 2026-10-05

Gap 32 remains open. Existing writer input and inventory-based incoming-call
reports were re-read before pursuing a new independent reference search.
The actual store consumes unknown stacked pointer bytes; its input-read
provenance does not establish the callback's offset, segment or admission.

An independent scan of the resident source bytes searched near-call encodings
whose signed displacement resolves to the candidate writer, and far-call
encodings whose resident segment/offset pair resolves to that writer. It also
checked every MZ relocation for the writer's canonical segment. None supplied
a candidate in these domains. These searches do not depend on function starts,
but they do not cover instruction-produced indirect targets, all segment
aliases, wrapping near calls or FBOV relocation semantics. No absence or dead
code claim follows. The existing inventory search remains partial.

The adjacent relocated-pair report likewise supplies no exact writer reference.
Do not substitute unknown stacked bytes with a synthetic callback or stitch
an isolated writer return into the consumer. A genuine incoming pointer or
registration-table producer is still required for the released engine's
instruction-produced indirect-call support to apply to this game case.

Toolkit issue 213's published response confirms that producer and alias
relations are consumer evidence. A new issue for unknown incoming state would
repeat that request without a demonstrated shared defect; none was opened.

Source-local search results: GAME_DIR/analysis/reporter-audit/
engine100-writer-inputs/independent-near-call-hits.json,
independent-far-call-hits.json and independent-segment-relocations.json.
No original instructions, bytes, assets or runtime observations are committed.
