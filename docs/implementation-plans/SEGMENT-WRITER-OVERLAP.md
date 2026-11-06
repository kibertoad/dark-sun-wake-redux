# Segment-slot writer overlap census

Tooling for Q-EXE-033's concrete 4DA0:0000 producer/lifetime review.
The existing reporter matches an exact encoded displacement despite prose
claiming that every covering word/dword store is listed. Its positive
control at 178+0411 writes pointer bytes zero and one, but querying byte
one omits it. Preserve legacy default results as exact-start matches,
correct that description, and add explicit --overlap byte coverage.

Keep all existing fingerprint, source, ES override, absolute-displacement,
12-byte segment-load candidate and all-byte-decode limitations. Overlap
means a nonwrapping half-open operand byte interval; do not add aliases,
string writes, reached-instruction claims or native segment behavior.
Synthetic controls must distinguish word/dword/byte extents, ES/DS,
indexed operands, read-only operations and the 16-bit end boundary.
Exit: legacy source reports unchanged; controls pass; the known pointer
word store appears for both covered bytes in overlap mode; full gate passes.
This fixes a local research reporter, not the shared engine or protocol.
No research closure or evidence status follows from the tooling change.
