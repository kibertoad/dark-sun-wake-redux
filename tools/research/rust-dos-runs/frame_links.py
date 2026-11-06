"""Bounded observation of the documented SS:BP overlay frame-link walk."""
import struct


def observe(read, ss, bp):
    """read(seg, off, size) supplies exact bytes; never writes or resumes."""
    nodes, seen = [], set()
    for _ in range(64):
        if bp in seen:
            return {"nodes": nodes, "end": "cycle", "next_bp": bp}
        seen.add(bp)
        if bp + 2 > 0x10000 or ss * 16 + bp + 2 > 0x100000:
            return {"nodes": nodes, "end": "link-crossing", "next_bp": bp}
        link = struct.unpack("<H", read(ss, bp, 2))[0]
        node = {"bp": bp, "link": link}
        nodes.append(node)
        if link >> 1 == 0:
            return {"nodes": nodes, "end": "sentinel"}
        node["compares_return_segment"] = not bool(link & 1)
        if not link & 1:
            if bp + 6 > 0x10000 or ss * 16 + bp + 6 > 0x100000:
                return {"nodes": nodes, "end": "return-crossing", "next_bp": bp}
            offset, segment = struct.unpack("<HH", read(ss, bp + 2, 4))
            node["return_offset"], node["return_segment"] = offset, segment
        bp = link & 0xFFFE
    return {"nodes": nodes, "end": "node-limit", "next_bp": bp}
