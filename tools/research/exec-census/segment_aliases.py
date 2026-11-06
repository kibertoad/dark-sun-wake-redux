"""Select all descriptor aliases of a loaded resident segment.

Overlay operands name rows, not unique paragraph bases. Matching metadata
does not establish instruction ownership or live segment preservation.
"""


def descriptor_words(rows, segment, flags=None):
    return frozenset(index * 8 for index, row in enumerate(rows)
                     if row[0] + 0x1000 == segment
                     and (flags is None or row[2] in flags))
