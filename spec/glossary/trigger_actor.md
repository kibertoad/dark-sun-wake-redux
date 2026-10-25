# trigger_actor

The object that set off the trigger being run: an `INT16` the game keeps at `4C13:0367` in BLD-GOG-EN-1.1, set by the trigger walkers before they run a trigger's script, which reads it as variable `0x26` of kind 9 [FND-SCRIPT-028]; `plan_walk` also stores in it the slot it plans for when it is not replanning [FND-EXPLORE-014]. No other build has been described.
