# Getting Started

This creates a ready-to-run .NET 10 project with a game class, a scene, an input
map, and references to Fix2Engine. You need the .NET 10 SDK and a local clone of
Fix2Engine.

```bash
dotnet build /path/to/Fix2Engine/Fix2Engine.sln -m:1
cd /path/where/you/keep/games
dotnet run --project /path/to/Fix2Engine/Fix2Console -- --new-project MyGame
cd MyGame
dotnet run
```

The generated `Program.cs` loads `InputMap.toml` and starts your game:

```csharp
using Fix2Engine;
using Fix2Engine.Input;

namespace MyGame;

internal static class Program
{
    private static void Main()
    {
        InputManager.LoadInputMap();
        Fix2.Run<Game>();
    }
}
```

The other examples use the generated `MyGame` namespace. Replace it with your own
project name.
