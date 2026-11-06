"""Synthetic loaded-header queue and source refusal controls."""
from pathlib import Path
import struct
import sys
import tempfile
import unittest
sys.path.insert(0, str(Path(__file__).resolve().parent))
from loaded_headers import observe, registered, SIZE


class Controls(unittest.TestCase):
    def run_graph(self, links, loads=None, sizes=None, bounds=(0x6000, 0x7000), queue=(0x40000, 0x40100)):
        calls = []
        loads, sizes = loads or {}, sizes or {}
        headers = {0x5000: {"descriptor": 1, "code_size": 32},
                   0x5010: {"descriptor": 2, "code_size": 32}}
        def read(seg, off, count):
            calls.append((seg, off, count))
            value = {8: sizes.get(seg, 32), 0x10: loads.get(seg, 0x6000),
                     0x1C: links.get(seg, 0)}.get(off, 0)
            return struct.pack("<H", value)
        return observe(read, links.get("head", 0x5000), headers, *bounds, queue), calls

    def test_empty(self):
        result, calls = self.run_graph({"head": 0})
        self.assertEqual(result["end"], "null")
        self.assertEqual(calls, [])

    def test_registered_chain(self):
        result, calls = self.run_graph({0x5000: 0x5010}, {0x5010: 0x6003})
        self.assertEqual(result["end"], "null")
        self.assertEqual(len(result["nodes"]), 2)
        self.assertTrue(all(n["code_within_buffer"] and n["code_and_backlink_disjoint"] for n in result["nodes"]))
        self.assertTrue(all(size == 2 for _, _, size in calls))

    def test_cycle(self):
        result, _ = self.run_graph({0x5000: 0x5010, 0x5010: 0x5000})
        self.assertEqual((result["end"], len(result["nodes"])), ("cycle", 2))

    def test_unregistered(self):
        result, calls = self.run_graph({"head": 0x1234})
        self.assertEqual(result["end"], "unregistered-header")
        self.assertEqual(calls, [])

    def test_size_mismatch(self):
        result, calls = self.run_graph({}, sizes={0x5000: 33})
        self.assertEqual(result["end"], "code-size-mismatch")
        self.assertEqual(calls, [(0x5000, 8, 2)])

    def test_ranges(self):
        result, _ = self.run_graph({}, loads={0x5000: 0x4000})
        self.assertFalse(result["nodes"][0]["code_within_buffer"])
        self.assertFalse(result["nodes"][0]["code_and_backlink_disjoint"])
        result, _ = self.run_graph({}, loads={0x5000: 0})
        self.assertIsNone(result["nodes"][0]["code_linear"])

    def test_source_refusals(self):
        with tempfile.TemporaryDirectory() as root:
            path = Path(root) / "synthetic.bin"
            path.write_bytes(b"x")
            with self.assertRaisesRegex(ValueError, "size"):
                registered(path, 0)
            path.write_bytes(bytes(SIZE))
            with self.assertRaisesRegex(ValueError, "fingerprint"):
                registered(path, 0)

    def test_node_bound(self):
        headers = {0x5000 + i: {"descriptor": i, "code_size": 32} for i in range(50)}
        def read(seg, off, size):
            return struct.pack("<H", {8: 32, 0x10: 0x6000, 0x1C: seg + 1}.get(off, 0))
        result = observe(read, 0x5000, headers, 0x6000, 0x7000, [0x40000, 0x40100])
        self.assertEqual((result["end"], len(result["nodes"])), ("node-limit", 49))


if __name__ == "__main__":
    unittest.main()
