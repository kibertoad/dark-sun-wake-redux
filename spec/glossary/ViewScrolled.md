# ViewScrolled

An event: the map view scrolls one step. It carries `dx` and `dy`, each -1, 0 or 1. RULE-EXPLORE-001 emits it. No rule handles it; the step and the clamping to the map are not specified [SRC-MANUAL-1994].
