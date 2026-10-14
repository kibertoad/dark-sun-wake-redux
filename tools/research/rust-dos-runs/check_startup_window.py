"""Synthetic continuation/pause controls; no original or emulator is started."""
from pathlib import Path
import sys
from types import SimpleNamespace
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parent))
from startup_window import wait_for_stop


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
