"""Fixed canonical item-type domain; FND-PARTY-252/253, FND-ACTOR-017."""
import json
from pathlib import Path
import sys

import xxhash


def candidates(table):
    if len(table) != 1530:
        raise ValueError("expected exactly 102 shared fifteen-byte types")
    return [index for index in range(102)
            if table[15 * index + 4] & 0x20
            and not table[15 * index] & 0x13
            and not table[15 * index + 11] & 0x89]


def main():
    if len(sys.argv) != 2:
        raise SystemExit("usage: look_property_domain.py PATH_TO_OBJEX.GFF")
    with Path(sys.argv[1]).open("rb") as stream:
        source = stream.read(6816517)
    if len(source) != 6816516 or xxhash.xxh3_128_hexdigest(source) != "e08d3cac153c547f6804ca27b155915d":
        raise SystemExit("source differs from the approved OBJEX archive")
    selected = candidates(source[0x1F291:0x1F88B])
    print(json.dumps({"rows": 102, "candidate_indices": selected,
                      "type_forced_nonzero_rows": 102 - len(selected)}))


if __name__ == "__main__":
    main()
