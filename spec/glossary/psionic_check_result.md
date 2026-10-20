# psionic_check_result

An `INT8` the game keeps at `DS:4224` in BLD-GOG-EN-1.1: the result of the last psionic power check, the die from 1 to 19 on a success, 0 on a failure and -1 when disrupted. It is 0 in the file, and overlay 176 `+029A` is the only instruction that writes it [FND-RNG-046]. No other build has been described.
