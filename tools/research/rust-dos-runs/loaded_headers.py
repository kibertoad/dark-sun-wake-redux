"""Registered loaded-overlay queue metadata, without guest execution."""
import struct
import xxhash
from trampoline_census import layout, SIZE, XXH3


def registered(path, delta):
    if path.stat().st_size != SIZE:
        raise ValueError("overlay header source size differs")
    with path.open("rb") as source:
        body = source.read(SIZE + 1)
    if len(body) != SIZE or xxhash.xxh3_128_hexdigest(body) != XXH3:
        raise ValueError("overlay header source fingerprint differs")
    _, _, entries, overlays = layout(body)
    sizes = {d: end - start for start, end, d in overlays}
    result = {p["segment"] + delta: {"descriptor": p["descriptor"],
              "code_size": sizes[p["descriptor"]]} for p in entries}
    if len(result) != 49 or any(not 0 < seg <= 0xFFFF for seg in result):
        raise ValueError("overlay header source mapping differs")
    return result


def observe(read, head, headers, first, last, queue_linear):
    nodes, seen = [], set()
    for _ in range(49):
        if head == 0:
            return {"nodes": nodes, "end": "null"}
        if head not in headers:
            return {"nodes": nodes, "end": "unregistered-header", "next_header": head}
        if head in seen:
            return {"nodes": nodes, "end": "cycle", "next_header": head}
        seen.add(head)
        def word(off):
            return struct.unpack("<H", read(head, off, 2))[0]
        size = word(8)
        node = {"header": head, "descriptor": headers[head]["descriptor"], "code_size": size}
        nodes.append(node)
        if size != headers[head]["code_size"]:
            return {"nodes": nodes, "end": "code-size-mismatch", "next_header": head}
        load, following = word(0x10), word(0x1C)
        node.update(load_segment=load, next_header=following,
                    saved_return_offset=word(2), state_and_passes=list(read(head, 0x1A, 2)))
        start, end = load * 16, load * 16 + size
        valid = load > 0 and size > 0 and end <= 0x100000
        node["code_within_buffer"] = (first <= load and end <= last * 16) if valid and 0 < first < last else None
        node["code_linear"] = [start, end] if valid else None
        node["backlink_linear"] = [start - 2, start] if valid else None
        qstart, qend = queue_linear
        node["code_and_backlink_disjoint"] = (end <= qstart or qend <= start - 2) if valid else None
        node["header_fields_disjoint"] = head * 16 + 0x1E <= qstart or qend <= head * 16 + 2
        head = following
    return {"nodes": nodes, "end": "null" if head == 0 else "node-limit", "next_header": head}
