# Licensing and release notices

Fix2Engine's own code is MIT licensed under the root `LICENSE`. Dependencies retain
their own licenses. The engine's copyright notice does not claim ownership of
Raylib, ImGui, Tomlyn, Silk.NET, or other third-party code.

## What is included

`THIRD-PARTY-NOTICES.txt` contains full upstream license texts for the reviewed
managed dependencies, native Raylib/cimgui/ImGui components, embedded code notices,
and notices supplied by the reviewed .NET packages and installed .NET 10 SDK.
Some upstream notices cover optional components not present in every binary.

- `licenses/dependencies.json` records reviewed package IDs, versions, and licenses.
- `licenses/sources.json` records the provenance and digest of each copied legal text.
- Native Raylib notices use the 6.0 sources identified by Raylib-cs 8.0.0.
- ImGui/cimgui notices use the 1.91.6 upstream sources corresponding to
  ImGui.NET 1.91.6.1. The NuGet package identifies its managed-wrapper commit but
  does not establish a complete native-binary build manifest. Verify that provenance
  when replacing or rebuilding the native library.
- .NET notices include SDK 10.0.112 and the packages in the inventory. A different
  SDK, runtime identifier, self-contained publish, or NativeAOT toolchain can change
  the material included in a release; inspect that release's resolved files.

These records document license handling; they are not a legal opinion or a claim
that all code/asset provenance, patent rights, or trademarks have been cleared.

## Automatic distribution

`Directory.Build.targets` imports `build/Fix2Engine.Licenses.targets` for projects
in this repository. Fix2Console-generated projects explicitly import the same
file. Running `Fix2Console --init` upgrades an existing project's import while
preserving its TOML files. The import adds these files to build and publish output:

- `Fix2Engine-LICENSE.txt` — the engine's MIT license.
- `Fix2Engine-THIRD-PARTY-NOTICES.txt` — third-party notices.

The prefixed names avoid overwriting a game's own `LICENSE` or notice file. Keep
both with installers, archives, and store uploads. The game's own code, additional
libraries, assets, and license choice remain the game author's responsibility.
If distributing individual assemblies or creating a NuGet package, verify the
license files are included in that distribution too; `dotnet pack` is not a
validated packaging workflow in this repository.

## Maintaining the inventory

1. Restore/build all projects after changing dependencies.
2. Review the actual package, exact upstream source, and embedded/native libraries.
   Preserve required copyright, permission, and disclaimer text. URLs and package
   metadata supplement the texts; they do not replace them.
3. Update the package inventory and notice sections. Record source URLs and notice
   SHA-256 values in `licenses/sources.json` when updating a section.
4. Run `python3 tools/check_licenses.py`. It detects new package versions and missing
   or changed recorded legal text. It cannot establish the legal suitability of a
   license or infer undocumented native dependencies.
5. Publish and inspect the actual distribution, including platform runtime notices.

## Release review

- The maintainer confirmed authorship and absence of unauthorized third-party
  content for the current README screenshots on 2026-09-21; see
  `AssetProvenance.md`. Review replacement images and newly added content separately.
- Confirm rights to any externally copied code, contributor submissions, or local
  assets added outside the tracked repository. Keep existing source attributions.
- Check the Fix2Engine name/logo against the intended markets before relying on it
  as a cleared brand. No trademark search or registration is performed by these changes.
- If bundling OpenAL Soft or another native OpenAL implementation, review its own
  license and distribution requirements. Silk.NET.OpenAL's MIT license covers the
  binding, not every native library that it can load. No OpenAL Soft binary is
  included in the reviewed tracked files.

Do not mark these items cleared without evidence. For a commercial release with
unresolved rights questions, have the relevant material reviewed by qualified
legal counsel.
