"""Named read-only snapshot at the ordinary DSUN exit worker; no runner API."""
import struct


def capture(client, stop, delta):
    """Caller must verify the licensed source/checkpoint before invoking this helper."""
    if type(delta) is not int:
        raise ValueError("invalid shutdown relocation")
    code, data = 0x1000 + delta, 0x57E0 + delta
    if not (0 <= code <= 0xFFFF and 0 <= data <= 0xFFFF):
        raise ValueError("shutdown relocation outside real mode")
    registers = stop.get("registers", {})
    if (stop.get("reason") != "breakpoint"
            or stop.get("cs_ip", "").upper() != f"{code:04X}:0388"
            or registers.get("system", {}).get("mode") != "real"):
        raise ValueError("snapshot requires ordinary exit-entry breakpoint in real mode")
    try:
        ds, ss, sp = (int(registers[key], 16) for key in ("ds", "ss", "sp"))
    except (KeyError, TypeError, ValueError) as error:
        raise ValueError("missing shutdown segment/frame registers") from error
    if ds != data or ss != data:
        raise ValueError("shutdown DS/SS differs from admitted data segment")

    def read(segment, offset, size):
        if (not 0 <= segment <= 0xFFFF or not 0 <= offset <= 0xFFFF
                or not 0 < size <= 128 or offset + size > 0x10000
                or segment * 16 + offset + size > 0x100000):
            raise ValueError("shutdown named field crosses supported bounds")
        body = client.read_memory(f"{segment:04X}:{offset:04X}", size).data
        if len(body) != size:
            raise ValueError("short shutdown named-field read")
        return body

    def word(segment, offset):
        return struct.unpack("<H", read(segment, offset, 2))[0]

    def pointer(segment, offset):
        off, seg = struct.unpack("<HH", read(segment, offset, 4))
        return seg, off

    def address(pair):
        return f"{pair[0]:04X}:{pair[1]:04X}"

    returned, status, second, third = struct.unpack("<4H", read(ss, sp, 8))
    if returned != 0x03EC or second != 0 or third != 0:
        raise ValueError("shutdown frame differs from ordinary 03DF caller")
    count = word(ds, 0x3572)
    if count > 32:
        raise ValueError("shutdown callback count exceeds recorded capacity32")
    callbacks = [address(pointer(ds, 0xA446 + 4 * i)) for i in range(count)]
    head, active = pointer(ds, 0x9DA3), pointer(ds, 0x9D9F)
    nodes, seen = [], set()

    def record(pair):
        seg, off = pair
        return {"pointer": address(pair), "io_handle": word(seg, off + 4),
                "options": word(seg, off + 6),
                "handle": struct.unpack("<I", read(seg, off + 0x28, 4))[0],
                "dirty": read(seg, off + 0x2C, 1)[0]}

    current = head
    while current != (0, 0):
        if current in seen:
            raise ValueError("shutdown archive list cycle")
        if len(nodes) == 64:
            raise ValueError("shutdown archive list exceeds tooling limit64")
        seen.add(current)
        nodes.append(record(current))
        current = pointer(current[0], current[1] + 0x0C)
    active_record = next((r for r in nodes if r["pointer"] == address(active)), None)
    if active != (0, 0) and active_record is None:
        active_record = record(active)
    slots = [{"index": i, "argument": word(ds, 0x3F4E + 14 * i),
              "target": address(pointer(ds, 0x3F4A + 14 * i))} for i in range(16)]
    streams = []
    for i in range(4):
        off = 0x3682 + 16 * i
        raw = word(ds, off)
        streams.append({"offset": off, "count": raw - 0x10000 if raw & 0x8000 else raw,
                        "flags": word(ds, off + 2), "handle_byte": read(ds, off + 4, 1)[0],
                        "capacity": word(ds, off + 6), "buffer": word(ds, off + 8),
                        "cursor": word(ds, off + 10), "self": word(ds, off + 14)})
    return {"phase": "ordinary-exit-entry", "status": status, "callbacks": callbacks,
            "stream_hook": address(pointer(ds, 0x3676)), "streams": streams,
            "slot_gate": word(ds, 0x00C4), "slots": slots,
            "archive_count": word(ds, 0x9D9D), "archive_head": address(head),
            "archive_active": address(active), "active_in_head": active in seen,
            "active_record": active_record, "archives": nodes,
            "limits": "entry snapshot only; no later-state, native-result or file-change proof"}
