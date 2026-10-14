"""From a checkpoint, stop at the generator 1000:0822 (05BA:0822 here) and list its callers."""
import os, sys, struct, collections
from pathlib import Path
from dinorefurb_dosbox_session import (Media, RustDosConfig, RustDosSession, RustDosSettings, Target,
                                       RUST_DOS_PINNED_SHA256, read_saved_state)
game = Path(os.environ["GAME_DIR"])  # the installed BLD-GOG-EN-1.1
d, name = sys.argv[2].rsplit(":", 1)
until = int(sys.argv[3]); click = sys.argv[4:] and sys.argv[4] == "start"
settings = RustDosSettings(emulator=Path("artifacts/rust-dos/rust-dos.exe"), emulator_sha256=RUST_DOS_PINNED_SHA256,
    run_directory=Path(sys.argv[1]), target=Target("DSUN.EXE", ("-W0", "-L")),
    emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),), deterministic=True, cycles="15000"),
    start_state=read_saved_state(Path(d), name))
def wait(s, op):
    ob = s.observe(op, timeout=600)
    while ob.pending: ob = s.observe(op, timeout=600)
    return ob
callers = collections.Counter(); n = 0
with RustDosSession(settings) as s:
    if click:
        s.send_input("mouse", {"action": "click", "x": 157, "y": 76, "coords": "virtual"})
    s.client.set_breakpoint("05BA:0822")
    while True:
        ob = wait(s, s.continue_(until_ms=until))
        if ob.stop.get("reason") != "breakpoint":
            print("end", ob.stop.get("reason"), ob.stop.get("emu_ns")); break
        r = ob.stop["registers"]
        st = s.client.read_memory(f"{r['ss']}:{r['sp']}", 4).data
        off, seg = struct.unpack("<HH", st)
        callers[f"{seg:04X}:{off:04X}"] += 1; n += 1
        if n == 1: print("first", ob.stop.get("emu_ns"), r)
        if n >= 3000: print("cap"); break
print("draws", n)
for c, k in callers.most_common(20): print(" ", c, k)
