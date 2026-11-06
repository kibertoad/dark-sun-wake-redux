"""Fixed F1/Enter Save with one private native payload-write lock failure."""
import hashlib
import json
import os
from pathlib import Path
import re
import struct
import sys

from dinorefurb_dosbox_session import (
    Media, RUST_DOS_PINNED_SHA256, RustDosConfig, RustDosSession,
    RustDosSettings, SoundBlaster, Target, read_saved_state,
)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from probe_drive import ROOT
from restore_opener import verify_checkpoint_executable
from regenerate_checkpoints import admit_state, mapping, snapshot, checked_read
from load_entry import fields, input_map_ready, observe_delivery, archive_fields, terminated_path, returned_frame
from native_save import save_ready, save_root, save_return
from startup_window import wait_for_stop, checkpoint_summary
from checkpoint_rng import Recorder
from shutdown_run import file_states, write_request
from private_payload_lock import PrivatePayloadLock, LIMIT
from save_failure_fields import payload, record as archive_record, pending_watch, pending_change
from request_provider import frame
import archive_origin


def arm_save_return(client, registers):
    destination = save_return(client, registers)
    ss, sp = int(registers["ss"],16), int(registers["sp"],16)
    checked_read(client,ss,sp+2,2)
    watch = f"{ss:04X}:{sp+2:04X}"
    client.set_watchpoint(watch,2)
    client.set_breakpoint(destination,once=True)
    return destination,watch


def main():
    if len(sys.argv) != 3 or any(not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", x) for x in sys.argv[1:]):
        raise SystemExit("usage: failed_save.py <play-ready-source-run> <new-run-name>")
    config = json.loads((ROOT / "tools/project-config.json").read_text(encoding="utf-8-sig"))
    game = Path(os.environ.get("DARK_SUN_WAKE_REDUX_GAME_DIR",
                Path(config["original"]["analysisExecutable"]["path"]).parent)).resolve()
    source = game / "captures/rust-dos-regenerated" / sys.argv[1]
    meta = json.loads((source / "regeneration.json").read_text(encoding="utf-8"))
    controls = [c for c in meta["checkpoints"] if c["name"] == "play-ready" and c["checked"]]
    if len(controls) != 1:
        raise ValueError("source lacks unique verified play-ready controls")
    control = controls[0]
    state = read_saved_state(source, "play-ready")
    verify_checkpoint_executable(state, config["original"]["analysisExecutable"])
    admit_state(state)
    delta = mapping(meta["load_mapping_stop"])
    rng = Recorder(delta)
    run = game / "captures/rust-dos-save-failure" / sys.argv[2]
    if run.exists():
        raise ValueError("failed-Save run already exists")
    settings = RustDosSettings(
        emulator=ROOT / "artifacts/rust-dos/rust-dos.exe", emulator_sha256=RUST_DOS_PINNED_SHA256,
        run_directory=run, target=Target("DSUN.EXE", ("-W0", "-L")), start_state=state,
        emulator_config=RustDosConfig(media=(Media("D", game / "game.ins", "iso"),),
            deterministic=True, cycles="15000", sound_blaster=SoundBlaster(base=0x220, irq=5, dma=1),
            ultrasound=None), readiness_timeout=60.0)
    report = {"source": sys.argv[1], "load_mapping_stop": meta["load_mapping_stop"],
              "inputs": [], "writes": [], "draws": [], "preparations": [], "native_writes": [], "pending_changes": [],
              "passed": False, "limits": "one private file-lock input; no guest setup write"}
    lock = PrivatePayloadLock(run)
    with RustDosSession(settings) as session:
        def retain():
            (run / "failure.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
        try:
            stop = session.stop
            if (stop["reason"] != "load" or stop["cs_ip"] != state.stop["cs_ip"]
                    or stop["emu_ns"] != state.stop["emu_ns"]):
                raise ValueError("failed-Save restore instruction/time differs")
            current = snapshot(session.client, delta)
            if (current != control["state"]
                    or hashlib.sha256(session.client.screenshot()).hexdigest() != control["frame_sha256"]
                    or not input_map_ready(session.client, delta, current, fields(session.client, delta))):
                raise ValueError("failed-Save source differs from verified input map")
            report["restored_stop"], report["initial_seed"] = stop, current["rng_state"]
            report["files_before"] = file_states(run)
            if report["files_before"]["SAVE02.SAV"]["exists"]:
                raise ValueError("fixed failed-Save slot already exists")
            generator, setter = rng.address(0x1000, 0x822), rng.address(0x1000, 0x811)
            open_entry, open_io, open_return = (rng.address(0x38FF, o) for o in (0x66, 0x96, 0x2B1))
            write_at, write_after = (rng.address(0x44DE, o) for o in (0x285, 0x287))
            resource_return, prep = rng.address(0x37FC, 0x5C2), rng.address(0x37FC, 0x8FB)
            map_gate = rng.address(0x39D1, 0x97D)
            for address in (generator, setter):
                session.client.set_breakpoint(address)
            key_name = "f1"
            key = session.send_input("key", {"key": key_name, "hold_ms": 100})
            delivered = None
            phase, previous = "window", None
            root = destination = watch = opening = published = pending_write = suffix = None
            dirty_watch = None
            builder_armed = False
            for index in range(2048):
                # Bounded sampling only: input/state transitions require the predicates below.
                target = int(session.stop["emu_ns"]) // 1000000 + 100
                observed, exhausted = wait_for_stop(session, session.continue_(until_ms=target))
                stop = observed.stop
                report["last_stop"], report["observations"] = stop, index + 1
                if exhausted:
                    raise RuntimeError("failed-Save operation observation exhausted")
                address = stop["cs_ip"]
                delivered = observe_delivery(session, key, delivered)
                if stop["reason"] == "breakpoint" and address == generator:
                    report["draws"].append(rng.record(session, stop))
                elif stop["reason"] == "breakpoint" and address == setter:
                    raise ValueError("unadmitted seed setter during failed Save")
                elif (stop["reason"] == "watchpoint" and dirty_watch is not None
                        and int(stop.get("watch",{}).get("addr","-1"),16)==dirty_watch):
                    report["pending_changes"].append(pending_change(stop,dirty_watch))
                elif stop["reason"] == "watchpoint" and watch is not None:
                    if int(stop.get("watch",{}).get("addr","-1"),16) != int(watch.split(":")[0],16)*16+int(watch.split(":")[1],16):
                        raise ValueError("unknown failed-Save watchpoint")
                    changed = save_return(session.client, report["save_entry"]["registers"])
                    if changed != destination:
                        session.client.remove_breakpoints(destination)
                        destination = changed
                        session.client.set_breakpoint(destination, once=True)
                elif stop["reason"] == "breakpoint" and root is not None and address == root:
                    ui = fields(session.client, delta)
                    if ui["load_mode"] or ui["selected_row"] != 1 or ui["list_top"]:
                        raise ValueError("failed Save reached another row/mode")
                    report["save_entry"] = stop
                    session.client.remove_breakpoints(root)
                    destination,watch = arm_save_return(session.client,stop["registers"])
                    for site in (open_entry, write_at, resource_return, prep):
                        session.client.set_breakpoint(site)
                    phase = "saving"
                elif stop["reason"] == "breakpoint" and address == open_entry:
                    item = archive_origin.opening(session.client, stop["registers"], delta, terminated_path)
                    if item is not None:
                        if opening is not None:
                            raise ValueError("duplicate working open")
                        opening = item
                        report["working_open"] = {"entry": stop, "arguments": opening}
                        session.client.set_breakpoint(open_io, once=True)
                        session.client.set_breakpoint(open_return, once=True)
                elif stop["reason"] == "breakpoint" and address == open_io:
                    frame(stop["registers"], opening)
                    opening["io_handle"] = int(stop["registers"]["ax"],16)
                    if opening["io_handle"] == 0xFFFF:
                        raise ValueError("working native open failed before fault input")
                elif stop["reason"] == "breakpoint" and address == open_return:
                    published = archive_origin.publication(session.client, stop["registers"], opening,
                                                           archive_fields(session.client, delta))
                    report["working_open"]["publication"] = published
                elif stop["reason"] == "breakpoint" and address == write_at:
                    if pending_write is not None:
                        raise ValueError("unpaired native write")
                    item = {"stop": stop, "request": write_request(stop["registers"])}
                    if "fault" not in report:
                        if published is None:
                            raise ValueError("payload precedes observed working publication")
                        report["fault"] = payload(session.client, stop, delta, published,
                                                  archive_fields(session.client, delta))
                        dirty_watch=pending_watch(session.client,published["provider"],report["fault"]["record"]["dirty"])
                        report["files_before_fault"] = file_states(run)
                        lock.acquire()
                        report["fault"]["lock"] = {"file": "drive-c/DARKRUN.GFF", "start": 0,
                                                     "length": LIMIT, "acquired": True, "released": False}
                    report["native_writes"].append(item)
                    pending_write = item
                    session.client.set_breakpoint(write_after, once=True)
                elif stop["reason"] == "breakpoint" and address == write_after:
                    if pending_write is None:
                        raise ValueError("write result lacks request")
                    result = {"ax": stop["registers"]["ax"],
                              "carry": bool(int(stop["registers"]["flags"],16)&1)}
                    pending_write["result"] = result
                    if lock.locked:
                        lock.release()
                        report["fault"]["lock"]["released"] = True
                        report["fault"]["write_result"] = result
                        report["files_after_fault"] = file_states(run)
                        if not result["carry"]:
                            raise ValueError("private lock did not cause native write failure")
                        if report["files_before_fault"]["DARKRUN.GFF"] != report["files_after_fault"]["DARKRUN.GFF"]:
                            raise ValueError("locked payload changed the private working file")
                        f = report["fault"]
                        suffix = f'{f["numbered_segment"]:04X}:0A65'
                        session.client.set_breakpoint(suffix, once=True)
                    pending_write = None
                elif stop["reason"] == "breakpoint" and address == resource_return:
                    if "fault" in report and "resource_result" not in report:
                        r, f = stop["registers"], report["fault"]
                        if int(r["bp"],16) != f["resource_bp"] or int(r["ss"],16) != 0x57E0+delta:
                            raise ValueError("resource result frame differs")
                        state_now = archive_record(session.client, published["provider"])
                        error = int.from_bytes(checked_read(session.client,0x57E0+delta,0x9D99,2),"little")
                        report["resource_result"] = {"ax": r["ax"], "error": error, "record": state_now}
                        if int(r["ax"],16)!=0xFFFF or not state_now["dirty"]:
                            raise ValueError("native error did not reach dirty resource failure")
                elif stop["reason"] == "breakpoint" and suffix is not None and address == suffix:
                    r, f = stop["registers"], report["fault"]
                    if int(r["bp"],16) != f["numbered_bp"] or int(r["ss"],16) != 0x57E0+delta:
                        raise ValueError("numbered return frame differs")
                    flag = checked_read(session.client,0x57E0+delta,f["numbered_bp"]-1,1)[0]
                    report["numbered_result"] = {"success_byte":flag,"record":archive_record(session.client,published["provider"])}
                    if flag:
                        raise ValueError("failed payload did not retain numbered failure byte")
                elif stop["reason"] == "breakpoint" and address == prep:
                    r=stop["registers"]
                    _,_,arg=struct.unpack("<HHI",checked_read(session.client,int(r["ss"],16),int(r["sp"],16),8))
                    report["preparations"].append(archive_record(session.client,f"{arg>>16:04X}:{arg&0xFFFF:04X}"))
                elif stop["reason"] == "breakpoint" and destination is not None and address == destination:
                    if not returned_frame(report["save_entry"]["registers"],stop["registers"]):
                        raise ValueError("Save return frame differs")
                    if "numbered_result" not in report:
                        raise ValueError("Save returned without the recorded failure suffix")
                    report["save_return"] = stop
                    session.client.remove_watchpoints(watch)
                    watch = None
                    for site in (open_entry, write_at, resource_return, prep):
                        session.client.remove_breakpoints(site)
                    phase = "map"
                elif phase == "map" and stop["reason"] == "breakpoint" and address == map_gate:
                    pass
                elif stop["reason"] != "time":
                    raise ValueError("unadmitted failed-Save stop")
                current = snapshot(session.client, delta)
                ui = fields(session.client, delta)
                if (phase == "window" and delivered.state == "done" and current == previous
                        and save_ready(current,ui)):
                    report["inputs"].append({"key":key_name,"delivery":delivered.detail})
                    root=save_root(session.client,rng.address(0x5736,0x2F))
                    session.client.set_breakpoint(root)
                    key_name="enter"
                    key=session.send_input("key",{"key":key_name,"hold_ms":100})
                    delivered=None
                    phase="action"
                map_ready_now = phase=="map" and delivered.state=="done" and input_map_ready(session.client,delta,current,ui)
                if map_ready_now and not builder_armed:
                    session.client.set_breakpoint(map_gate,once=True)
                    builder_armed = True
                if map_ready_now and stop["reason"]=="breakpoint" and address==map_gate:
                    if pending_write is not None or lock.locked:
                        raise ValueError("map reached with unfinished fault input")
                    report["inputs"].append({"key":key_name,"delivery":delivered.detail})
                    report["saved_state"],report["frame_sha256"]=current,hashlib.sha256(session.client.screenshot()).hexdigest()
                    report["post_failed_map"]={"archive":archive_fields(session.client,delta),
                        "record":archive_record(session.client,published["provider"])}
                    report["files_after"]=file_states(run)
                    if report["files_after"]["SAVE02.SAV"]["exists"]:
                        raise ValueError("failed writer unexpectedly copied a numbered save")
                    report["checkpoint"]=checkpoint_summary(session.save_state("failed-map",check_files=True))
                    report["passed"]=True
                    break
                previous=current
                retain()
            else:
                raise RuntimeError("failed-Save state budget exhausted")
        except Exception as error:
            report["error"]=str(error)
            raise
        finally:
            try:
                lock.release()
            finally:
                if "fault" in report and "lock" in report["fault"]:
                    report["fault"]["lock"]["released"]=not lock.locked
                retain()
    print(json.dumps({"passed":report["passed"],"draws":len(report["draws"])}),flush=True)


if __name__=="__main__":
    main()
