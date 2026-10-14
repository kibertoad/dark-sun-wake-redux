"""Synthetic continuation/pause controls; no original or emulator is started."""
from pathlib import Path
import sys
import struct
from types import SimpleNamespace
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from startup_window import wait_for_stop, seed_caller, checkpoint_summary


class Session:
    def __init__(self, observations):
        self.observations = iter(observations)
        self.operations = []
        self.pauses = 0

    def observe(self, operation, timeout):
        assert timeout == 30
        self.operations.append(operation)
        return next(self.observations)

    def pause(self):
        self.pauses += 1
        return {"reply": "paused"}


class StopChecks(unittest.TestCase):
    def test_checkpoint_boolean_is_not_reported_as_its_directory(self):
        directory = Path("synthetic/states/opener")
        checkpoint = SimpleNamespace(name="opener", checkpoint=True, directory=directory)
        self.assertEqual(checkpoint_summary(checkpoint),
                         {"name": "opener", "checked": True, "directory": str(directory)})

    def test_seed_caller_requires_declared_overlay_return_and_call(self):
        frame = struct.pack("<HHH", 0x0663, 0x6000, 123)
        trampoline = struct.pack("<BHH", 0xEA, 0x0141, 0x6000)
        call = struct.pack("<BHH", 0x9A, 0x0811, 0x05BA)
        self.assertEqual(seed_caller(frame, trampoline, call, 0x05BA), ("6000:0663", 123))
        for bad_frame, bad_trampoline, bad_call in (
            (frame[:4], trampoline, call),
            (struct.pack("<HHH", 0x0023, 0x6000, 123), trampoline, call),
            (struct.pack("<HHH", 0x0663, 0x6001, 123), trampoline, call),
            (frame, struct.pack("<BHH", 0xCD, 0x0141, 0x6000), call),
            (frame, struct.pack("<BHH", 0xEA, 0x0000, 0x6000), call),
            (frame, trampoline, struct.pack("<BHH", 0x9A, 0x0822, 0x05BA)),
            (frame, trampoline, struct.pack("<BHH", 0x9A, 0x0811, 0x05BB)),
        ):
            with self.subTest(frame=bad_frame, trampoline=bad_trampoline, call=bad_call):
                with self.assertRaises(ValueError):
                    seed_caller(bad_frame, bad_trampoline, bad_call, 0x05BA)

    def test_destination_or_guard_stop_is_returned_without_resume(self):
        for reason in ("destination", "generator"):
            final = SimpleNamespace(pending=False, stop={"reason": reason})
            session = Session([final])
            operation = SimpleNamespace(op=7)
            observed, exhausted = wait_for_stop(session, operation)
            self.assertIs(observed, final)
            self.assertFalse(exhausted)
            self.assertEqual(session.pauses, 0)
            self.assertEqual(session.operations, [operation])

    def test_pending_keeps_handle_and_pause_observes_original_operation(self):
        pending = SimpleNamespace(pending=True)
        final = SimpleNamespace(pending=False, stop={"reason": "pause"})
        session = Session([pending, pending, pending, final])
        operation = SimpleNamespace(op=7)
        observed, exhausted = wait_for_stop(session, operation, attempts=2)
        self.assertIs(observed, final)
        self.assertTrue(exhausted)
        self.assertEqual(session.pauses, 1)
        self.assertEqual(session.operations, [operation] * 4)


if __name__ == "__main__":
    unittest.main()
