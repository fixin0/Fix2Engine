# Graphics

Sources: `Graphics/*` — namespace `Fix2Engine.Graphics`

## Dependencies

```xml
<PackageReference Include="Raylib-cs" Version="8.0.0" />
<PackageReference Include="rlImgui-cs" Version="3.2.0" />
<AllowUnsafeBlocks>true</AllowUnsafeBlocks>
```

All drawing goes through Raylib (`using static Raylib_cs.Raylib`).

---

## Model3D

`Model3D.cs` — loads external models and draws them with a composed transform.

```csharp
public enum MaterialMaps { Albedo, Normal, Metalness, Roughness, Emission, Irradiance, Height }

public class Model3D
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero; // degrees
    public Vector3 Scale { get; set; } = Vector3.One;
    public Color Tint { get; set; } = Color.White;

    public Model3D(string modelPath); // LoadModel(path) — supports .obj, .glb, .gltf, etc.
    public void SetTexture(string texturePath, MaterialMaps map = MaterialMaps.Albedo);
    public int GetMaterialCount();
    public List<string> GetMaterialMaps();
    public void Draw();   // builds Matrix4x4 Scale*RotX*RotY*RotZ*Translation, assigns _model.Transform, DrawModel
    public void Unload(); // UnloadTexture (if any) + UnloadModel
}
```

Usage:

```csharp
var ship = new Model3D("assets/models/ship.glb")
{
    Position = new Vector3(0, 0, 0),
    Scale = Vector3.One * 2.0f,
    Rotation = new Vector3(0, 45, 0)
};
ship.SetTexture("assets/textures/ship_albedo.png", MaterialMaps.Albedo);

// inside Render() / between Camera.Begin() and Camera.End():
ship.Draw();

// on unload:
ship.Unload();
```

Notes:
- `SetTexture` uses an `unsafe` block to assign `model.Materials[0].Maps[(int)MaterialMapIndex.X].Texture`.
- Rotation is in **degrees** (converted via `DEG2RAD` internally).

---

## Sprite2D

`Sprite2D.cs` — 2D textured quad, `IDisposable`.

```csharp
public class Sprite2D : IDisposable
{
    public Texture2D Texture { get; }
    public Vector2 Position { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; } = Vector2.Zero; // pivot
    public float Rotation { get; set; }
    public Color Tint { get; set; } = Color.White;
    public Rectangle SourceRect { get; set; } // defaults to full texture

    public Sprite2D(string filePath);
    public Sprite2D(Texture2D texture);
    public void CenterOrigin();
    public void Draw();    // DrawTexturePro(Texture, SourceRect, destRect, Origin, Rotation, Tint)
    public void Dispose(); // UnloadTexture
}
```

Usage:

```csharp
using var player = new Sprite2D("assets/sprites/player.png");
player.Position = new Vector2(100, 100);
player.Scale = new Vector2(2, 2);
player.CenterOrigin();
player.Draw();
```

---

## Skybox

`Skybox.cs` — cubemap skybox with embedded GLSL 330 shaders.

```csharp
public class Skybox
{
    public Skybox(string texturePath); // GenMeshCube(1,1,1) → LoadTextureCubemap → LoadShaderFromMemory
    public void Draw(Vector3 cameraPosition, float radius = 5000.0f);
    public void Unload();
}
```

Usage:

```csharp
var skybox = new Skybox("assets/skybox.png");

// inside Camera.Begin()/End():
skybox.Draw(camera.Position);

// on exit:
skybox.Unload();
```

`Draw` disables depth test and back-face culling, draws the cube at `cameraPosition` with the given radius, then restores state.

---

## Primitives (Raylib Direct)

When you do not need external assets, use Raylib primitives directly inside a `Camera.Begin()`/`End()` block:

```csharp
camera.Begin();
ClearBackground(new Color(40, 44, 52, 255));

DrawPlane(new Vector3(0, 0, 0), new Vector2(100, 100), new Color(60, 64, 72, 255));
DrawGrid(50, 2.0f);
DrawCube(position, 1, 1, 1, Color.Red);
DrawSphere(position, 0.8f, Color.Blue);
DrawCubeWires(position, 1, 1, 1, Color.Black);

camera.End();
```

This is how `Debug3DScene` renders without model files. See [Camera](Camera.md) for 3D context setup.
