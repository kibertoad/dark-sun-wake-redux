"""Fixed paused overlay/queue metadata; caller verifies the source and restore."""
import struct
from frame_links import observe


def capture(client, stop, delta):
    if type(delta) is not int or not all(0 <= s + delta <= 0xFFFF
                                       for s in (0x4DA1, 0x55CE, 0x57E0)):
        raise ValueError("invalid overlay snapshot mapping")
    r = stop.get("registers", {})
    if stop.get("reason") != "load" or r.get("system", {}).get("mode") != "real":
        raise ValueError("overlay snapshot requires paused real-mode restore")
    try:
        regs = {k: int(r[k], 16) for k in ("ds", "ss", "bp", "sp")}
    except (KeyError, TypeError, ValueError) as error:
        raise ValueError("missing overlay snapshot registers") from error
    if any(not 0 <= n <= 0xFFFF for n in regs.values()) or regs["ds"] != 0x57E0 + delta:
        raise ValueError("overlay snapshot register binding differs")
    manager, queue, data = 0x55CE + delta, 0x4DA1 + delta, 0x57E0 + delta

    def read(seg, off, size):
        if off + size > 0x10000 or seg * 16 + off + size > 0x100000:
            raise ValueError("overlay named field crosses physical bounds")
        body = client.read_memory(f"{seg:04X}:{off:04X}", size).data
        if len(body) != size:
            raise ValueError("short overlay named-field read")
        return body

    def word(off):
        return struct.unpack("<H", read(manager, off, 2))[0]

    def pointer(off):
        offset, segment = struct.unpack("<HH", read(manager, off, 4))
        return f"{segment:04X}:{offset:04X}"

    flags = read(manager, 0x10, 1)[0]
    first, last = word(0x124), word(0x126)
    qstart, qend = queue * 16, queue * 16 + 0x60
    ssstart, ssend = regs["ss"] * 16, regs["ss"] * 16 + 0x10000
    result = {"registers": regs, "mode_byte": flags,
              "selected_cache": "extended" if flags & 1 else "ems" if flags & 2 else "none",
              "ems_window_segment": word(0x30), "ems_handle": word(0x32),
              "move_target": pointer(0x43), "move_handle": word(0x47),
              "move_base": struct.unpack("<I", read(manager, 0x49, 4))[0],
              "cache_hook": word(0x80), "notification": pointer(0x86),
              "buffer_first": first, "buffer_end": last, "buffer_next": word(0x120),
              "loaded_head": word(0x12C),
              "queue_index": struct.unpack("<b", read(data, 0x915, 1))[0],
              "queue_kinds": list(struct.unpack("<6H", read(queue, 0x54, 12))),
              "queue_linear": [qstart, qend],
              "ss_window_disjoint": (ssend <= qstart or qend <= ssstart)
                  if ssend <= 0x100000 else None,
              "buffer_disjoint": (last * 16 <= qstart or qend <= first * 16)
                  if 0 < first < last else None,
              "frame_links": observe(read, regs["ss"], regs["bp"]),
              "limits": "paused checkpoint only; observed links do not prove frame validity or later-state preservation"}
    return result
