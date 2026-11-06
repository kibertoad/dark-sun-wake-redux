"""Fixed native-Save payload/frame guards; no guest writes."""
import struct
from regenerate_checkpoints import checked_read
from private_payload_lock import LIMIT


def pending_watch(client, pointer, expected):
    seg, off = (int(x,16) for x in pointer.split(":"))
    if checked_read(client,seg,off+0x2C,1) != bytes([expected]):
        raise ValueError("pending watch initial value differs")
    address=f"{seg:04X}:{off+0x2C:04X}"
    client.set_watchpoint(address,1)
    return seg*16+off+0x2C


def pending_change(stop, physical):
    watch=stop.get("watch",{})
    if (stop.get("reason")!="watchpoint" or int(watch.get("addr","-1"),16)!=physical
            or watch.get("len")!=1):
        raise ValueError("stop does not match the fixed pending-byte watch")
    return {"old":int(watch["old"],16),"new":int(watch["new"],16),
            "cs_ip":stop["cs_ip"],"seq":stop["seq"],"emu_ns":stop["emu_ns"]}


def record(client, pointer):
    seg, off = (int(x, 16) for x in pointer.split(":"))
    body = checked_read(client, seg, off, 0x2D)
    return {"pointer": pointer, "io_handle": struct.unpack_from("<H", body, 4)[0],
            "options": struct.unpack_from("<H", body, 6)[0],
            "flags": struct.unpack_from("<I", body, 0x24)[0],
            "handle": struct.unpack_from("<I", body, 0x28)[0], "dirty": body[0x2C]}


def payload(client, stop, delta, published, archive):
    from shutdown_run import write_request
    r = stop["registers"]
    data, core = 0x57E0 + delta, 0x37FC + delta
    if (stop.get("reason") != "breakpoint" or stop["cs_ip"] != f"{0x44DE + delta:04X}:0285"
            or r.get("system", {}).get("mode") != "real" or int(r["ss"], 16) != data):
        raise ValueError("payload requires the fixed real-mode write boundary")
    request = write_request(r)
    bp = int(r["bp"], 16)
    writer_bp, ret, caller = struct.unpack("<3H", checked_read(client, data, bp, 6))
    if (ret, caller) != (0x4E8, core):
        raise ValueError("write is not the resource payload caller")
    parent_bp, parent_ret, parent_seg = struct.unpack("<3H", checked_read(client, data, writer_bp, 6))
    source, length, tag, number = struct.unpack("<4I", checked_read(client, data, writer_bp + 6, 16))
    position = struct.unpack("<I", checked_read(client, data, writer_bp - 0x14, 4))[0]
    state = record(client, published["provider"])
    if (parent_ret != 0x9D8 or tag != int.from_bytes(b"SAVE", "little") or number != 1
            or not 0 < length <= 0x7FFF or request["count"] != length
            or position + length > LIMIT or state["options"] != 2 or state["flags"] & 8
            or request["handle"] != published["io_handle"] or state["io_handle"] != request["handle"]
            or state["handle"] != published["provider_handle"]
            or archive["working_handle"] != state["handle"]
            or archive["active"]["pointer"] != state["pointer"]):
        raise ValueError("payload does not match the fixed writable SAVE/1 record/span")
    if checked_read(client, parent_seg, 0x9D3, 5) != struct.pack("<BHH", 0x9A, 0xE8, core):
        raise ValueError("payload parent instruction differs from numbered writer")
    return {"request": request, "record": state, "position": position, "length": length,
            "resource_bp": writer_bp, "numbered_bp": parent_bp, "numbered_segment": parent_seg,
            "source_argument": source, "number": number}
