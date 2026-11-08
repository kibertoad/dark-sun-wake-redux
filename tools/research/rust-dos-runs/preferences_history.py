"""Fixed named-field value watches for Q-CONFIG-014; no guest writes.

CONFIG-236/238 and SOUND-085/092 provide the fields and widths. A changed
configuration pointer stops this observer; it cannot select new watch targets.
"""
import re
import struct

from regenerate_checkpoints import checked_read


class ValueHistory:
    FIELDS = (
        ("master_sound", 0x57E0, 0x13F7, 1),
        ("music_on", 0x57E0, 0x1436, 1),
        ("music_level", 0x57E0, 0x26B4, 1),
        ("configuration_pointer", 0x57E0, 0x3411, 4),
        ("music_slot", 0x57E0, 0x343E, 2),
        ("source_handle", 0x57E0, 0x3440, 2),
        ("getter_handle", 0x57E0, 0x3464, 2),
        ("library_mode", 0x4E71, 0x0B51, 2),
    )

    def __init__(self, delta):
        if type(delta) is not int or not -0x1000 <= delta <= 0:
            raise ValueError("unsupported history relocation")
        self.delta, self.fields, self.armed = delta, {}, []
        self.previous, self.sequence, self.time = None, -1, -1
        self.report = {"fields": self.fields, "initial": None, "changes": [],
                       "final": None, "complete": False,
                       "limits": "changed instruction-boundary values only; no unchanged writes, intra-instruction access or fresh-startup history"}

    def _stop(self, stop):
        if (stop.get("registers", {}).get("system", {}).get("mode") != "real"
                or not re.fullmatch(r"[0-9A-Fa-f]{4}:[0-9A-Fa-f]{4}", stop.get("cs_ip", ""))
                or type(stop.get("seq")) is not int or stop["seq"] <= self.sequence
                or type(stop.get("emu_ns")) is not int or stop["emu_ns"] < self.time):
            raise ValueError("history requires an ordered real-mode stop")

    def _values(self, client):
        return {name: int.from_bytes(checked_read(client, field["segment"], field["offset"], field["length"]), "little")
                for name, field in self.fields.items()}

    def arm(self, client, stop):
        if self.previous is not None or self.fields or stop.get("reason") != "load":
            raise ValueError("history must arm once at the admitted restore")
        self._stop(stop)
        ds = 0x57E0 + self.delta
        off, seg = struct.unpack("<HH", checked_read(client, ds, 0x3411, 4))
        if seg == off == 0:
            raise ValueError("history configuration pointer is null")
        checked_read(client, seg, off, 59)
        spans = [(name, s + self.delta, o, length) for name, s, o, length in self.FIELDS]
        spans.append(("card_id", seg, off + 8, 2))
        occupied = []
        for name, segment, offset, length in spans:
            checked_read(client, segment, offset, length)
            low, high = segment * 16 + offset, segment * 16 + offset + length
            if any(low < end and start < high for start, end in occupied):
                raise ValueError("history fields overlap physically")
            occupied.append((low, high))
            self.fields[name] = {"segment": segment, "offset": offset, "physical": low, "length": length}
        self.previous = self._values(client)
        self.report["initial"] = dict(self.previous)
        self.sequence, self.time = stop["seq"], stop["emu_ns"]
        for field in self.fields.values():
            address = f'{field["segment"]:04X}:{field["offset"]:04X}'
            self.armed.append(address)  # Cleanup also covers an ambiguous failed arm response.
            client.set_watchpoint(address, field["length"])
        self.check(client)

    def check(self, client):
        if self.previous is None or self.report["complete"]:
            raise ValueError("history is not active")
        if self._values(client) != self.previous:
            raise ValueError("history value changed without a recorded watch")

    def changed(self, client, stop):
        if self.previous is None or self.report["complete"]:
            raise ValueError("history is not active")
        self._stop(stop)
        watch = stop.get("watch", {})
        try:
            physical = int(watch["addr"], 16)
            old, new = int(watch["old"], 16), int(watch["new"], 16)
        except (KeyError, TypeError, ValueError) as error:
            raise ValueError("malformed history watch") from error
        matches = [name for name, f in self.fields.items()
                   if f["physical"] == physical and type(watch.get("len")) is int and f["length"] == watch["len"]]
        if stop.get("reason") != "watchpoint" or len(matches) != 1:
            raise ValueError("unknown history watch")
        name = matches[0]
        current = self._values(client)
        if old != self.previous[name] or new != current[name] or old == new:
            raise ValueError("history watch values differ from named storage")
        changes = {n: {"old": self.previous[n], "new": value}
                   for n, value in current.items() if value != self.previous[n]}
        self.report["changes"].append({"primary": name, "cs_ip": stop["cs_ip"],
                                      "seq": stop["seq"], "emu_ns": stop["emu_ns"], "values": changes})
        if "configuration_pointer" in changes:
            raise ValueError("history configuration pointer changed; no target following")
        self.previous = current
        self.sequence, self.time = stop["seq"], stop["emu_ns"]

    def finish(self, client):
        self.check(client)
        if not any(event["primary"] == "music_level" for event in self.report["changes"]):
            raise ValueError("history lacks a positive music-byte watch")
        self.report["final"] = dict(self.previous)

    def close(self, client):
        for address in tuple(self.armed):
            client.remove_watchpoints(address)
            self.armed.remove(address)

    def complete(self):
        if self.armed or self.report["final"] is None:
            raise ValueError("history completion needs final values and watch cleanup")
        self.report["complete"] = True
