# DEV-EXPLORE-002

- Departs from: RULE-EXPLORE-005
- Reason: A walk to a clicked cell follows a route the rebuild plans itself, since how the original plans it is not known.
- Setting: None
- Default: mandatory
- Justification: The original's route planner is not known (Q-EXPLORE-006), so its routes cannot be reproduced, and there is no original behaviour a setting could switch back to. The rebuild's routes obey the same blocked and occupied cells as the original's movement (RULE-EXPLORE-003).
- Dropped: no

The owner approved a planner of the rebuild's own on 2026-09-13, on the condition that it reaches
every reachable cell and keeps the movement constraints the spec records.

The planner searches the eight directions by A* with octile costs and a fixed tie-break, never cuts
a blocked corner diagonally, and reports a cell it cannot reach. A walk advances one cell per step
and stops before a step into a cell that has become blocked. A step takes 125 ms and a frame
catches up at most four steps; the original's cadence is an open question of RULE-EXPLORE-005, so
both values are the rebuild's own until it is answered.
