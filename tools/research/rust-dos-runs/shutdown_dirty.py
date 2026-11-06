"""Fixed controlled pending-metadata setup; no address or data selection API."""
import hashlib
import struct

from dinorefurb_dosbox_session import FieldContract, WritableField
from shutdown_snapshot import capture


def seed_active(session, stop, delta):
    state = capture(session.client, stop, delta)
    record = state["active_record"]
    if (state["status"] != 0 or not state["active_in_head"] or record is None
            or state["archive_active"] != state["archive_head"]
            or record != state["archives"][0]):
        raise ValueError("dirty setup requires ordinary active head record")
    if record["dirty"] != 0 or not record["options"] & 2:
        raise ValueError("dirty setup requires clean writable-options record")
    seg, off = (int(x, 16) for x in record["pointer"].split(":"))
    field = WritableField("archive_pending_metadata", f"{seg:04X}:{off + 0x2C:04X}", 1)
    contract = FieldContract("dark-sun-shutdown-dirty/1", (field,))
    verified = session.write(contract, field.name, b"\x01", hashlib.sha256(b"\x00").hexdigest())
    return {"kind": "controlled-setup", "record": record, "old": 0, "new": 1,
            "verified": verified.to_json(),
            "limits": "seeded pending metadata; not ordinary dirty-state reachability"}


class Preparation:
    """Pair only the known close worker's preparation call and ordinary return."""
    def __init__(self, delta):
        self.delta = delta
        self.pending = None

    def entry(self, item, stop):
        if self.pending is not None:
            raise ValueError("nested preparation outside fixed contract")
        if item["return"] != f"{0x38FF + self.delta:04X}:0324":
            raise ValueError("preparation caller outside fixed close worker")
        self.pending = {"item": item, "ss": stop["registers"]["ss"],
                        "sp": int(stop["registers"]["sp"], 16)}

    def returned(self, client, stop):
        before = self.pending
        r = stop["registers"]
        if (before is None or stop.get("reason") != "breakpoint"
                or stop.get("cs_ip") != f"{0x38FF + self.delta:04X}:0324"
                or r.get("system", {}).get("mode") != "real"
                or r["ss"] != before["ss"] or r["ds"] != f"{0x57E0 + self.delta:04X}"
                or int(r["sp"], 16) != before["sp"] + 4):
            raise ValueError("preparation return lacks preserved fixed frame")
        argument = before["item"]["argument"]
        seg, off = argument >> 16, argument & 0xFFFF
        # Entry snapshot and per-entry reads have already bounded this record.
        handle = client.read_memory(f"{seg:04X}:{off + 4:04X}", 2).data
        dirty = client.read_memory(f"{seg:04X}:{off + 0x2C:04X}", 1).data
        if len(handle) != 2 or len(dirty) != 1:
            raise ValueError("short preparation result read")
        before["item"]["result"] = {"ax": r["ax"], "dirty": dirty[0],
                                     "io_handle": struct.unpack("<H", handle)[0]}
        self.pending = None
