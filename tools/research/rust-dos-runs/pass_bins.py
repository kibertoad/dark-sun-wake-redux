"""Bins a pass_counts.py log.json by pass of 173 +059A and prints the counts per site."""
import json, sys
from collections import Counter, defaultdict

d = json.load(open(sys.argv[1]))
print("window", d["config"]["count_from"], "to", d["config"]["duration"], "hits", d["hits"])
print("overlay placements:", [(e["t"], e["segment"]) for e in d["overlay_segments"] if e["label"] == "173+0C65"])
passes = [[]]
for e in d["events"]:
    if e["kind"] == "hit" and e["site"] == "173+059A" and e.get("stub", "").startswith("cd"):
        passes.append([])
    if e["kind"] == "hit" and e["site"] == "173+059A" and e.get("stub", "").startswith("ea") \
            and passes[-1] and passes[-1][-1].get("site") == "173+059A":
        continue  # the re-entry after the overlay load
    if e["kind"] == "hit" and e.get("stub", "").startswith("ea") and e["ret"].startswith("574E:"):
        continue  # 197 entry re-entered after the overlay manager loaded it
    passes[-1].append(e)
for i, p in enumerate(passes):
    if not p:
        continue
    c = Counter()
    det = defaultdict(list)
    for e in p:
        if e["kind"] == "write":
            c["write " + e["changed"]["4503:0A32"] + " by " + (e["static"] or e["run"])] += 1
            det["writes"].append(e["t"])
        else:
            c[e["site"]] += 1
            if e["site"] in ("1000:0822",):
                det["rng"].append((e["t"], e["ret"]))
            if e["site"] in ("2D40:03C1", "31E0:05BB", "2D40:0BA4", "197+29E0", "2D40:2001", "2D40:197A"):
                det[e["site"]].append(int(e["args"][0], 16))
            if e["site"] == "173+0C65":
                det["0C65"].append((e["t"], e["at"]))
    head = "pass %d %s..%s" % (i, p[0]["t"], p[-1]["t"])
    print(head, dict(c))
    for k, v in det.items():
        print("   ", k, v)
