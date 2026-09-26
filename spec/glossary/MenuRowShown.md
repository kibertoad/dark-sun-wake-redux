# MenuRowShown

An event: a script menu shows a row. It carries `row: UINT8`, 0 for the title and 1 upward for each offered entry, and `label: FARPTR<UINT8>`, the address of the row's NUL-terminated text. RULE-TALK-001 emits it. No rule handles it yet; the original passes it at once to the far routine `5702:0048` [FND-TALK-001].
