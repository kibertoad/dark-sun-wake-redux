# exit_control_byte

A `UINT8` at `57E0:1462` in BLD-GOG-EN-1.1. The `F3` exit choice's Quit
branch and the Start Game window's Exit to DOS branch set it to 0. It
starts at 1 and controls whether the resident main loop continues or
returns. After the return, the startup path performs cleanup and calls
the DOS terminate service; indirect cleanup and interrupt results are
not established [FND-SAVE-010, FND-SAVE-011, FND-UI-035].
