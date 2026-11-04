---
id: FND-RNG-093
title: Startup places the modeled far-heap base above resident data using paragraph rounding, with later heap validity still required
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:00AB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:14C4..1000:1622
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:177C..1000:18BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:00FA..444C:0162
tool: tools/research/exec-census/resident_listing.py at 3406d564; existing lower-heap readings FND-CONFIG-210/211/212 and startup reading FND-PARTY-215
environment: null
---

## Observation

This replaces FND-RNG-083's heap-base interpretation. Its claimed minimum
included a full paragraph after the unrounded byte sum. The actual startup
shifts the sum right four before adding one paragraph. FND-PARTY-215
already records that instruction order. Q-COMBAT-065's restore-buffer
comparison prompted this explicit arithmetic correction; it does not
establish new native heap state.

### Exact startup arithmetic

At 1000:0059 startup copies current DS to DX and subtracts that segment
from its available-memory word BP. It reads DS:384E, raises values below
0200 to 0200 and stores that minimum back. Let S be this admitted stable
word and P the unsigned word DS:37EC. It adds A4EC and P with separate
16-bit carry rejections. For a path passing both tests, define

```text
H = A4EC + S + P, with S >= 0200 and H <= FFFF
C = floor(H / 16) + 1
```

The SHR DI,CL at 007C uses CL four; INC DI at 007E forms C.
Unsigned available paragraphs B below C reject. If the following reads
of DS:384E and DS:37EC are both nonzero, the selected paragraph count
R is C. Otherwise the body selects the lesser of B and 1000.
Under stable fields and the earlier B >= C gate, that alternative
also has R >= C, since C <= 1000.

It stores current DS plus R as the segment words DS:00A0 and
DS:00A4. DS:009E is the base offset, initially zero in the image;
the stored segment alone does not prove that offset remained zero.
For the modeled data segment 57E0 and admitted R <= 1000 the
segment addition does not wrap. Preserving the zero offset gives
the base's linear distance 16*R from DS:0000.

Since H >= A6EC, the resulting minimum is A6F0, which is 0204
bytes past A4EC. It is not necessarily A6FC, the older finding's
A4EC + 0200 + 0010. For example, named fields S=0200 and P=0001
give H=A6ED and C=0A6F, hence a base distance A6F0 when that
computed-count branch is admitted. This is an arithmetic case,
not evidence that those field values occur in a native run.

The shipped words S=1000 and P=zero instead select min(B,1000)
after checking B against 0B4F. With ample admitted B, R=1000.
FND-RNG-084's historical run observes that separate default case;
its runtime identity and observed values are not changed here.

### Allocation and lifetime qualifications

The known noncallback 4AB9:001C path reaches 444C:00FA, which
requests its count plus one through 1000:15A5 and places its marker.
An admitted nonnull callback at DS:3471 is a separate route, not
automatically the same heap contract; FND-EXE-583 records that field's
bounded census. The nominal 444C:0008 route reaches 15A5 through
1000:18D0 as FND-CONFIG-209/210 record.

FND-CONFIG-211 accounts for first acquisition, growth, exact removal
and tail splitting. It identifies the paragraph headers, predecessor
and link fields and returned offset four. FND-CONFIG-212 qualifies
1831's arithmetic: the known nonnegative allocation increments pass
the preliminary gate only within the modeled linear range, whereas
that signed gate is not a universal bound for arbitrary callers.
The normalized candidate is then compared with current lower/upper
bounds before 177C, whose runtime and interrupt effects are recorded
in FND-CONFIG-167. Accepted requests return the previous top pointer.

Thus valid preserved bounds, roots, free-list links, block extents,
release inputs and nonwrapping segment arithmetic are conditions for
placing later returned blocks above this startup base. The base's
initial placement alone is not proof that every later pointer, free
header, indirect allocation or alias has that provenance. It also
does not establish payload capacity or successful clearing/transfer.

## Interpretation

Under the named startup and valid-heap conditions, the far-heap
segments remain above resident data ending at DS:A4EC. The corrected
minimum still supplies that separation. An access within such a
segment cannot reach the earlier RNG/date windows solely by choosing
another unsigned offset, absent segment wrap or changed mapping.
This is a conditional storage argument, not a global no-other-writers
or all-allocation-lifetimes proof.

Q-RNG-005/029 retain alias and other-writer coverage. Q-CONFIG-008
and Q-SCRIPT-003 retain the shared allocator/bounds/metadata admission;
Q-COMBAT-065 needs that admission for its concrete runtime bindings.
The previous numerical overstatement is withdrawn through supersession.
No RNG algorithm, native outcome or parity status changes.

## Alternatives

- Adding one paragraph after truncation does not add sixteen bytes to
  every unrounded input sum.
- The ample-memory default case is not the minimum over all admitted fields.
- An initial heap base does not prove valid later free-list contents.
- A callback or changed base offset requires its own pointer provenance.

## How to reproduce

Resolve licensed DSUN.EXE through tools/game-dir.mjs. With locked evidence
Python, resident_listing.py verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read 1000:0000..1000:00AB,
tracking the two carry rejections, SHR before INC, available-memory gate,
substituted count and final segment stores. Reuse FND-PARTY-215 and
FND-CONFIG-209/210/211/212/167 for the connected runtime bodies and
their explicit limits; do not reinterpret their conditional readings as
an admitted global heap invariant. Retain the correction listing only in
GAME_DIR/analysis/hit-incoming-20261104/heap-base-correction.txt.
No original bytes or listing are committed.
