# message_delay_value

The 16-bit word that the Preferences `MESSAGE DELAY` arrows change in steps of
eight [FND-CONFIG-010, FND-UI-034]. An overlay passes 100 times this word,
with 16-bit arithmetic, to a millisecond wait [FND-TIME-004].
