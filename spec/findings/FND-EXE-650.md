---
id: FND-EXE-650
title: Free-list header address confinement needs visited-link and extent bounds rather than a complete heap-validity certificate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:13CA..1000:149B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1582..1000:1622
tool: Compositional static reading of retained FND-CONFIG-211/225/227/230/231, FND-EXE-294/296/649 and FND-COMBAT-166; no new original execution or source extraction
environment: null
---

## Observation

FND-EXE-649 supplies a conditional fresh-allocation lower segment Ls,
with modeled Ls at least 632F. FND-CONFIG-227/230 retain exact free-list
and merge interfaces. This composition distinguishes address confinement
needed by Q-EXE-033 from stronger claims of a valid circular heap.
No previously unread helper is assigned a speculative effect.

Let D be the nonzero segment words at or above Ls. The resident overlay
header/trampoline envelope is below modeled linear 57EF9, whereas D's
linear bases start at least at 632F0. An actual metadata access to a
small offset zero through nine in a D segment is therefore above that
resident envelope. This is a linear address relation on the stated
bindings, not proof of backing, object identity or arbitrary physical
mapping. The proof must cover every actually used segment; checking only
the free root does not do so.

### Search, exact allocation and tail selection

FND-EXE-294 connects the selected DS and DX. Allocation starts from the
nonzero free root, reads each selected word-zero extent and follows its
word-six link while the count is inadequate. For these reads, the needed
input is that the root and every link actually followed belong to D.
Reciprocity or eventual return to the root is not necessary to keep those
particular addresses above Ls. It remains necessary for other claims
about progress, membership and ordinary completion.

On an exact-size selection, 143B reads the selected block's words four
and six. Its self-link case clears the free root. Otherwise A=word four
and B=word six select the own writes B to A:6 and A to B:4, and A is
published as the free root. With selected S, A and B in D, those accesses
and the returned S:4 remain in D; ordinary selected-DS restoration and
stable inputs are still conditions. No full proof that A/B are reciprocal
neighbors is needed merely to classify these addresses. Such a proof
cannot be inferred from this address property either.

For a tail selection, let selected free segment be S, available count H
and requested count p. The caller supplies unsigned H>p and nonzero
admitted p. Define the mathematical end E=S+H before word truncation.
If S is in D and E<=FFFF, then

- T=S+(H-p) is between S and E, strictly above S and below E;
- U=T+p equals E without word wrap;
- the own stores to S:0, T:0, T:2 and U:2, and returned T:4, are in D.

This follows directly from FND-CONFIG-227's word-sized operations; no
new allocator experiment was performed. The following U header must
still have its actual state and storage for a valid predecessor update.
In particular, the unconditional U:2 store can overwrite a free-state
zero. That state/adjacency question is not needed to show that U's
numeric segment is above Ls when the stated extent bound holds.

### Concrete escape conditions

A root in D with a foreign word-six link can leave D during search.
For example, selected 7000 has count one, request p is two and its
word-six link is 57DD. The next selection differs from starting root
7000 and reads word zero at segment 57DD, a resident overlay header.
This is the consequence of the named DS-from-link path for that input,
not evidence that the shipped game supplies that link.

A high selected segment alone also does not prevent tail wrapping.
With S=F000, H=67DE and p=1, H>p holds. The word calculation gives
T=57DD and U=57DE, because the mathematical end S+H=157DE exceeds
FFFF. The tail's own header stores then address the resident header
domain despite S being high. These are conditional integer calculations,
not seeded memory, an executed original call or an observed game defect.
They identify the needed extent/count producer admission rather than
prove an actual corrupted heap.

### What the retained mutations preserve conditionally

The direct free-root writers are already assigned in FND-CONFIG-225.
Insertion, unlink and terminal clearing do not require a new root census:

| Retained transition | Address/domain inputs that matter here |
| --- | --- |
| Empty insertion 1464 | Released P in D; own head and self-link values are P |
| Nonempty insertion 1464 | P, head H and its actually used next N in D; writes only those segments and copies those segment values |
| Unlink 143B | Selected segment and actually dereferenced A/B in D; head becomes zero or A |
| Tail allocation 1582 | S in D and H>p>0 with untruncated S+H<=FFFF |
| Nonterminal release 13CA | Released P, any dereferenced predecessor R and link neighbors in D; actual positive counts and nonwrapping ends for each computed following segment |
| Terminal root clearing/unlink | Clearing a head preserves zero-or-D; an unlink still needs its actual A/B inputs in D |

For nonterminal predecessor merging, the initial following address P+p
and the later M+count(M) need not be equated without the physical
adjacency relation retained in FND-CONFIG-230. Each computed address
needs its own bound, or that relation plus a bound that implies both.
For successor merging, the actual following segment plus its count and
the resulting merged extent likewise need bounds. No independent carry
rejection supplies them inside the merge. Count/root publications are
not transactional on later failure.

The known count sources are fresh header initialization, tail subtraction
and initialization, resize writes and predecessor/successor additions.
FND-CONFIG-211/227/230 and FND-EXE-296 retain their exact operations.
Their identities are not proof that the inputs preserve extents: caller
writes into freed metadata, partial publication, different entries or
bad prior counts can invalidate the required bounds. No completeness
claim about every possible writer is made by listing those sources.

## Interpretation

For FMT-EXE-003's header-access question, the concrete remaining
obligations are actual root/visited-link/release/predecessor segment
domains and nonwrapping count/end producers, with the recorded frame and
segment bindings. A complete reciprocal-list, checker or backing proof
is not substituted for those address conditions, and is not separately
added unless it can change the header access under study.

Under the stated domain inputs the own mechanisms are classified;
actual free-block histories, excluded writers and later target effects
remain unresolved. No complete-reading declaration, queue answer,
implementation status or parity result changes.

## Alternatives

- A high free head is sufficient. The foreign-link example rules that
  inference out even before an allocation returns.
- A high selected segment is sufficient for a tail result. The wrapped
  count example rules that inference out; H>p supplies no segment bound.
- Every heap property must be settled to classify these addresses. The
  sufficient domain conditions above do not require full reciprocity,
  closure or successful backing, while making no claim about them.
- All actual states satisfy the sufficient conditions. No such admission
  follows from this composition. FND-CONFIG-227/230/231 and FND-EXE-649
  retain their source, native, alias, frame and history limitations.

## How to reproduce

Reuse the installed DSUN.EXE readings named by the retained findings:
size 634416, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Their committed
resident_listing.py procedures cover 1000:13CA..1000:149B and
1000:1582..1000:1622, including selection, link stores, subtraction,
segment additions and returned offset four. Keep the byte-range and
entry limitations of each source reading; this finding adds composition,
not a fresh all-writer scan or an original-game run.

For each actual read/store, enumerate the segment source from the table
and compare its numeric base with FND-EXE-649's admitted lower domain.
Use mathematical S+H and H>p before applying 16-bit truncation to check
the tail relation and its conditional counterexample. Keep actual inputs,
following-header state, termination and physical storage distinct.
