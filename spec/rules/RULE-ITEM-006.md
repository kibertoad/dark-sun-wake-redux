---
id: RULE-ITEM-006
title: The transfer utility's translation of a Dark Sun 1 item
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-ITEM-001, FND-ITEM-006]
conflicting: []
split_with: []
related: [FMT-ITEM-001]
---

## Summary

When the character transfer utility moves characters from a Dark Sun 1 saved game, it replaces
the number of each Dark Sun 1 item they carry by the number of a Dark Sun 2 item, found in
`ITEMS.BIN`. A number of 0 stays 0, and a number the table does not hold becomes 900.

## When it runs

In the transfer utility, once for each of the up to 400 Dark Sun 1 items it has read, after it
has loaded `ITEMS.BIN` at start-up (FND-ITEM-006).

## Parameters

`old_number`, the Dark Sun 1 item's number, a signed 16-bit value.

## Inputs

None beyond the parameters.

## Procedure

```text
define translate_item(old_number):
    if old_number == 0:
        return 0
    let key = abs(old_number)
    for i in 0..234:
        let pair = read_file("ITEMS.BIN", FMT-ITEM-001, i * 4)
        if pair.old_item == key:
            return pair.new_item
    return 900
```

## Outputs

The `RDFF` number of the Dark Sun 2 item in `OBJEX.GFF`, or 0 for no item. The utility counts a
0 as an item not translated and anything else as translated. It then reads the `RDFF` resource
of that number, and when `OBJEX.GFF` has none, uses 900 in its place (FND-ITEM-006).

## Edge cases

The original loads the whole file into memory first, and stops with a message when it cannot
allocate the memory or open the file (FND-ITEM-006).

The original's loop runs for as many steps as the file has 16-bit words, 468, reading a pair at
each, so for a number the table does not hold it compares 234 more pairs of whatever memory
follows the table before giving 900. The procedure stops at the end of the file; a match in that
memory would make the original return a word from outside the table.

`abs` of -32,768 stays -32,768 in 16 bits, whose bits are those of 32,768, larger than any
`old_item`, so the number becomes 900 after the same over-read.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- Which Dark Sun 1 records the numbers come from, and whether `DSUN.EXE` uses `ITEMS.BIN` at all
  (FND-ITEM-004, Q-ITEM-001).
