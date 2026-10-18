# shift_string_byte

The function each byte of a script string of kind 2 passes through, at `172C:325F` in BLD-GOG-EN-1.1 [FND-SCRIPT-010]. It returns `(b << 2) & 0xFF` for its byte `b`, which is never 3 [FND-SCRIPT-024]. No other build has been described.
