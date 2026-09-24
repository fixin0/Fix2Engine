# Başlangıç (Getting Started)

Bu adımlar; oyun sınıfı, sahne, giriş haritası ve Fix2Engine referansları hazır
olan bir .NET 10 projesi oluşturur. .NET 10 SDK ve bilgisayarınıza klonlanmış
Fix2Engine gereklidir.

```bash
dotnet build /Fix2Engine/yolu/Fix2Engine.sln -m:1
cd /oyun/projelerinin/bulundugu/yol
dotnet run --project /Fix2Engine/yolu/Fix2Console -- --new-project MyGame
cd MyGame
dotnet run
```

Oluşturulan `Program.cs`, `InputMap.toml` dosyasını yükler ve oyunu başlatır:

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

Diğer örnekler oluşturulan `MyGame` namespace'ini kullanır. Bunu kendi proje
adınızla değiştirin.
