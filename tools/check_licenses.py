#!/usr/bin/env python3
"""Check reviewed package versions after restore; this is not legal clearance."""
from pathlib import Path
import json
import sys
import xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[1]
manifest = json.loads((root / "licenses/dependencies.json").read_text())
reviewed = {(p["id"].lower(), p["version"]) for p in manifest["packages"]}
errors = []
checked = set()

for project in root.rglob("*.csproj"):
    if any(part in {"bin", "obj", ".git"} for part in project.relative_to(root).parts):
        continue
    for ref in ET.parse(project).getroot().iter():
        if ref.tag.split("}")[-1] != "PackageReference":
            continue
        name = ref.get("Include") or ref.get("Update")
        version = ref.get("Version") or ref.findtext("Version")
        if name and version and (name.lower(), version) not in reviewed:
            errors.append(f"Unreviewed direct dependency: {name}/{version} in {project.relative_to(root)}")
    assets = project.parent / "obj/project.assets.json"
    if not assets.exists():
        errors.append(f"Restore/build first: missing {assets.relative_to(root)}")
        continue
    data = json.loads(assets.read_text())
    for library, details in data["libraries"].items():
        if details["type"] != "package":
            continue
        name, version = library.split("/", 1)
        checked.add((name.lower(), version))
        if (name.lower(), version) not in reviewed:
            errors.append(f"Unreviewed resolved dependency: {library} in {assets.relative_to(root)}")

notices = (root / "THIRD-PARTY-NOTICES.txt").read_text()
for package in manifest["packages"]:
    if f"  {package['id']} {package['version']} - {package['license']}" not in notices:
        errors.append(f"Missing notice inventory entry: {package['id']}/{package['version']}")

# Make edits to the copied upstream legal text explicit rather than silently dropping it.
import hashlib
for source in json.loads((root / "licenses/sources.json").read_text()):
    if "notice_sha256" not in source:
        continue
    marker = "=" * 78 + "\n" + source["component"] + "\nSource: " + source["source"] + "\n" + "=" * 78 + "\n\n"
    if marker not in notices:
        errors.append(f"Missing license section: {source['component']}")
        continue
    text = notices.split(marker, 1)[1].split("\n\n" + "=" * 78 + "\n", 1)[0].strip() + "\n"
    if hashlib.sha256(text.encode()).hexdigest() != source["notice_sha256"]:
        errors.append(f"Changed license text; review source and digest: {source['component']}")

if errors:
    print("License inventory check failed:")
    print("\n".join("- " + error for error in sorted(set(errors))))
    sys.exit(1)
print(f"PASS: {len(checked)} resolved package versions reviewed; upstream notice texts intact.")
print("Native binary provenance, new assets, and release-specific obligations still require review.")
