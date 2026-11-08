"""Fixed naturally produced post-Save source for Q-CONFIG-014."""
import struct
from preferences_key import KeyTrace
from regenerate_checkpoints import checked_read, ready
from load_entry import named_file


def source_control(meta):
    checkpoint = meta.get("checkpoint", {})
    inputs = meta.get("inputs", [])
    if (meta.get("passed") is not True or meta.get("writes") != []
            or checkpoint.get("name") != "saved-map" or checkpoint.get("checked") is not True
            or len(inputs) != 2 or [x.get("key") for x in inputs] != ["f1", "enter"]
            or any(x.get("delivery", {}).get("state") != "done" for x in inputs)
            or meta.get("save_after", {}).get("exists") is not True):
        raise ValueError("source lacks a completed checked native Save")
    return {"state": meta["saved_state"], "frame_sha256": meta["frame_sha256"]}


def verify_save(run, meta):
    actual = named_file(run, "SAVE02.SAV")
    if actual != meta["save_after"]:
        raise ValueError("restored native save differs")
    return actual


class SavedKeyTrace(KeyTrace):
    WAIT_FOR_DELIVERY = True

    def after_store(self, stop):
        if self.phase != "done" or len(self.report["events"]) != 10:
            raise ValueError("saved keyboard delivery requires paired store")
        self.report["store_stop"] = {k: stop[k] for k in ("cs_ip", "seq", "emu_ns")}
        self.report["endpoint"] = None
        self.phase = "delivery"

    def endpoint(self, stop, current, delivered):
        if (self.phase != "delivery" or stop.get("reason") != "time"
                or stop.get("registers", {}).get("system", {}).get("mode") != "real"
                or type(stop.get("seq")) is not int or stop["seq"] <= self.sequence
                or type(stop.get("emu_ns")) is not int or stop["emu_ns"] < self.time):
            raise ValueError("saved keyboard endpoint requires ordered post-store time stop")
        self.sequence, self.time = stop["seq"], stop["emu_ns"]
        self.report["last_ui"] = current
        if not (delivered and ready(current, 16500) and current.get("mode") == 2
                and current.get("handler") == "5782:0043"):
            return False
        self.report["endpoint"] = {**{k: stop[k] for k in ("cs_ip", "seq", "emu_ns")},
                                   "ui_state": current, "input_delivery": "done"}
        self.phase = "done"
        return True

    def complete(self):
        if self.report.get("endpoint") is None:
            raise ValueError("saved keyboard completion needs delivered ready Preferences")
        super().complete()

    def outer(self, client, item):
        off, seg = struct.unpack("<HH", checked_read(client, item["ss"], self.root_sp, 4))
        item["preferences_return"] = f"{seg:04X}:{off:04X}"
        if (off, seg) == (0x1EC9, self.segment):
            item["return_representation"] = "loaded"
            return
        header = 0x571F + self.delta
        if (off, seg) != (0, header):
            raise ValueError("saved keyboard outer pair is neither declared loaded nor header return")
        prefix = checked_read(client, header, 0, 4)
        trap = checked_read(client, header, 0x20, 5)
        item["saved_return_offset"] = int.from_bytes(prefix[2:], "little")
        if prefix != bytes((0xCD, 0x3F, 0xC9, 0x1E)) or trap != bytes((0xCD, 0x3F, 0x9B, 0x13, 0)):
            raise ValueError("saved keyboard header return representation differs")
        item["return_representation"] = "unloaded-header"

    def baseline(self, client, stop):
        if self.phase != "unarmed" or "baseline" in self.report or stop.get("reason") != "load":
            raise ValueError("saved keyboard baseline requires strict restore")
        if stop.get("registers", {}).get("system", {}).get("mode") != "real":
            raise ValueError("saved keyboard baseline requires real mode")
        values = {name: int.from_bytes(checked_read(client, segment + self.delta, offset, 2),
                                      "little", signed=name == "turn")
                  for name, segment, offset in (("mode", 0x57E0, 0x0DAB),
                      ("turn", 0x57E0, 0x426D), ("leader", 0x4C13, 0x0369),
                      ("combat", 0x4C10, 0x0019))}
        self.report["baseline"] = {**values, **{k: stop[k] for k in ("cs_ip", "seq", "emu_ns")}}
        if not (values["mode"] == 1 and values["combat"] == 0
                and 0 <= values["turn"] < 4 and values["turn"] == values["leader"]):
            raise ValueError("saved keyboard baseline lacks natural leader turn")

    def arm(self, client):
        if "baseline" not in self.report:
            raise ValueError("saved keyboard lacks baseline")
        super().arm(client)
