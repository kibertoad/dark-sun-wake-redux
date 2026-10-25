# flush_input

The far routine at `2C5F:0182` in BLD-GOG-EN-1.1, which empties the queue the keyboard and mouse handlers send their packets to, so that every key and mouse event still waiting is dropped [FND-CONFIG-171, FND-INPUT-016]. No other build has been described.
