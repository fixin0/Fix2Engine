# Export Your Game

Publishing creates a release folder that can run outside the source project. Run
the command on the same operating-system family as the target, especially when
Native AOT is enabled by the generated project.

```bash
# Windows 64-bit
dotnet publish -c Release -r win-x64 --self-contained true

# Linux 64-bit
dotnet publish -c Release -r linux-x64 --self-contained true

# Apple Silicon
dotnet publish -c Release -r osx-arm64 --self-contained true
```

The result is under `bin/Release/net10.0/<runtime>/publish/`. Distribute the whole
`publish` directory, including your `assets` folder,
`Fix2Engine-LICENSE.txt`, and `Fix2Engine-THIRD-PARTY-NOTICES.txt`. Test that copy
on a clean target machine before releasing it.
