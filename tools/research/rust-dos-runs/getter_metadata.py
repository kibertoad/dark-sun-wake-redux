"""Fixed paused getter inputs; FND-CONFIG-140/150, FND-PARTY-119."""
import hashlib
import struct
import xxhash

from regenerate_checkpoints import checked_read


def compare_buffers(captured, reference):
    if len(reference) != 1078:
        raise ValueError("FNFO reference extent differs")
    result = {}
    for name, start, size in (("field_offsets", 0, 980), ("field_metadata", 980, 98)):
        span = captured["spans"][name]
        if span["size"] != size or not isinstance(span["hex"], str) or len(span["hex"]) != size * 2:
            raise ValueError("FNFO captured extent differs")
        body = bytes.fromhex(span["hex"])
        result[name] = {"size": size, "equal": body == reference[start:start + size]}
    return result


def compare_archive(captured, path):
    with open(path, "rb") as stream:
        body = stream.read(6816517)
    if len(body) != 6816516 or xxhash.xxh3_128_hexdigest(body) != "e08d3cac153c547f6804ca27b155915d":
        raise ValueError("FNFO source differs from the approved OBJEX archive")
    return compare_buffers(captured, body[0x1FF48:0x2037E])


def fixed_read(client, segment, offset, size):
    if (type(segment) is not int or type(offset) is not int or type(size) is not int
            or not 0 <= segment <= 0xFFFF or not 0 <= offset <= 0xFFFF
            or not 0 < size <= 1572 or offset + size > 0x10000
            or segment * 16 + offset + size > 0x100000):
        raise ValueError("getter observation crosses real-mode bounds")
    return b"".join(checked_read(client, segment, offset + at, min(261, size - at))
                    for at in range(0, size, 261))


def pools(table, heads):
    if len(table) != 1572 or len(heads) != 3 or any(type(x) is not int or not 0 <= x <= 0xFFFF for x in heads):
        raise ValueError("getter pool snapshot differs from declared tables")
    result, owners, sharing = {}, {}, set()
    for name, head in zip(("56ED", "56EF", "56F1"), heads):
        seen, indices, nonzero = set(), [], []
        current = head
        while current != 9999 and 0 <= current < 524 and current not in seen:
            seen.add(current)
            indices.append(current)
            if current in owners:
                sharing.add(current)
            owners[current] = name
            if table[3 * current]:
                nonzero.append(current)
            current = struct.unpack_from("<H", table, 3 * current + 1)[0]
        result[name] = {"head": head, "indices": indices, "terminated": current == 9999,
                        "cycle_at": current if current in seen else None,
                        "invalid_index": current if current != 9999 and not 0 <= current < 524 else None,
                        "nonzero_kind_indices": nonzero}
    return {"lists": result, "shared_indices": sorted(sharing)}


def record_allocations(client, captured):
    span = captured["spans"]["record_bases"]
    if span["size"] != 24 or len(span["hex"]) != 48:
        raise ValueError("record-base snapshot extent differs")
    bases = bytes.fromhex(span["hex"])
    result, intervals = {}, {}
    for kind in range(1, 6):
        offset, segment = struct.unpack_from("<HH", bases, kind * 4)
        if offset != 4 or not segment:
            raise ValueError("record base is outside the declared P:0004 contract")
        paragraphs, predecessor = struct.unpack("<HH", fixed_read(client, segment, 0, 4))
        if not 1 <= paragraphs <= 4095:
            raise ValueError("record allocation header extent differs")
        end = paragraphs * 16
        if segment * 16 + end > 0x100000:
            raise ValueError("record allocation extent crosses physical memory")
        marker = fixed_read(client, segment, end - 1, 1)[0]
        if marker != 0x77:
            raise ValueError("record allocation marker differs")
        intervals[kind] = (segment * 16, segment * 16 + end)
        result[kind] = {"base": f"{segment:04X}:0004", "paragraphs": paragraphs,
                        "predecessor": predecessor, "marker": marker,
                        "payload_bytes_before_marker": end - 5}
    overlaps = [[a, b] for a in intervals for b in intervals if a < b
                and max(intervals[a][0], intervals[b][0]) < min(intervals[a][1], intervals[b][1])]
    return {"blocks": result, "overlaps": overlaps}


def capture(client, delta):
    if type(delta) is not int or not 0 <= 0x4F49 + delta <= 0xFFFF or not 0 <= 0x57E0 + delta <= 0xFFFF:
        raise ValueError("getter observation lacks a valid resident mapping")
    resident = 0x57E0 + delta
    spans, raw = {}, {}
    for name, segment, offset, size in (
            ("scratch_index", resident, 0x60EB, 2),
            ("selected_index", resident, 0x1A32, 2),
            ("pool_heads", resident, 0x56ED, 6),
            ("pool_counters", resident, 0x55C6, 4),
            ("field_offsets", resident, 0x5AF7, 980),
            ("field_metadata", resident, 0x60ED, 98),
            ("widths", resident, 0x040C, 23),
            ("selectors", resident, 0x5EED, 170),
            ("record_bases", resident, 0x614F, 24),
            ("record_strides", resident, 0x6167, 12),
            ("objects", 0x4F49 + delta, 0x0C33, 1572)):
        body = fixed_read(client, segment, offset, size)
        raw[name] = body
        spans[name] = {"address": f"{segment:04X}:{offset:04X}", "size": size,
                       "hex": body.hex(), "sha256": hashlib.sha256(body).hexdigest()}
    return {"spans": spans, "scratch_index": int.from_bytes(raw["scratch_index"], "little"),
            "selected_index": int.from_bytes(raw["selected_index"], "little"),
            "pool_counters": list(struct.unpack("<2H", raw["pool_counters"])),
            "pool_analysis": pools(raw["objects"], struct.unpack("<3H", raw["pool_heads"]))}
