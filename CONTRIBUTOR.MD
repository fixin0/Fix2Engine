# Contributing to Fix2Engine

Thank you for helping improve Fix2Engine. This guide applies to code, documentation,
examples, tests, and assets submitted to this repository.

## Development setup

Install the .NET 10 SDK. The demo requires the native graphics/windowing support
used by Raylib on your platform.

From the repository root:

```bash
dotnet build Fix2Engine.sln -m:1
dotnet build Tests/InputSystem/InputSystem.Tests.csproj -m:1
dotnet run --project Tests/InputSystem/InputSystem.Tests.csproj --no-build
python3 tools/check_licenses.py
dotnet run --project Game/Game.csproj --no-build
```

The input regression checks do not open a graphics window. Run the demo for changes
to rendering, windowing, or device input; describe the platform and behavior tested.
If Python 3 is not installed, install it before running the license inventory check.

## Making a contribution

1. Keep the change focused. Explain the problem and the resulting behavior.
2. Follow the surrounding C# style and the existing module boundaries. Project
   names such as `Graphics` are short; public namespaces remain `Fix2Engine.*`.
3. Add meaningful regression checks for changed behavior. Update the examples and
   relevant pages in `Documents/` when the public API or setup changes.
4. Build the solution and run the relevant checks. Report any failures or checks
   you could not run in the pull request.
5. Include reproduction steps or before/after screenshots for visual defects.

Do not commit `bin/`, `obj/`, machine-specific paths, credentials, access tokens,
or private customer/project data. Keep unrelated formatting and generated output
out of a contribution.

## Your rights and the project license

By submitting a contribution, you confirm that you have the right to submit it
and to license your original contributions under this repository's MIT license.
You retain ownership of your contributions; this is not a copyright assignment.
Check any employer, client, or collaborator permissions that apply to your work.

Third-party material keeps its original license and copyright notices. The root
`LICENSE` covers Fix2Engine's own code; it does not relicense its dependencies.
Do not remove attribution from copied or adapted code. Identify the source,
version or commit, changes made, and the applicable license in the contribution.
AI-assisted contributions have the same review, provenance, and licensing
requirements as other contributions; generated code is not a license clearance.

## Adding or updating dependencies

- Check both managed packages and any native libraries or embedded components.
- Confirm that the intended use and distribution satisfy their licenses. A
  binding's license does not automatically cover the native library it loads.
- Update `licenses/dependencies.json`, the complete texts in
  `THIRD-PARTY-NOTICES.txt`, and `Documents/Licensing.md` when applicable.
- Use the license from the actual version/commit, retain copyright statements,
  and record its source. A URL or an SPDX identifier alone is not a substitute
  for a required license text.
- Restore/build all affected projects and run `python3 tools/check_licenses.py`.
  That check detects package/version drift; it does not replace license review.
- Check a published sample to ensure `Fix2Engine-LICENSE.txt` and
  `Fix2Engine-THIRD-PARTY-NOTICES.txt` accompany the executable.

Review additional obligations before adding copyleft components or bundling a
native OpenAL implementation. The Silk.NET bindings and OpenAL Soft have different
licenses. Changes to target frameworks or publish runtime identifiers may introduce
additional runtime packages and notices.

## Assets, screenshots, and branding

For each external image, model, texture, font, audio file, or substantial example,
record its author, source, license/permission, and modifications in
`Documents/AssetProvenance.md`. Include the required license text or permission
record; do not commit private correspondence without permission.

Do not assume that an asset is reusable because it is downloadable or visible on
GitHub. Verify redistribution and commercial-use rights separately where relevant.
Do not claim third-party work, trademarks, or logos as Fix2Engine's own or imply
endorsement by their owners. Do not publish another person's personal information
in issues, screenshots, or sample data.

## Release review

Before publishing a release, follow `Documents/Licensing.md`, review the actual
files being shipped, and resolve outstanding asset provenance entries. Keep the
license notices in installers, archives, and store packages as well as ordinary
publish directories. Projects using Fix2Engine remain responsible for their own
code, assets, additional dependencies, and distribution conditions.
