"""Fixed naturally produced post-Save source for Q-CONFIG-014."""
from preferences_key import KeyTrace
from regenerate_checkpoints import checked_read
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
