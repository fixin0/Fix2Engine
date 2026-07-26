# Fix2Engine - Kapsamlı Dokümantasyon

## 📚 İçindekiler

1. [Giriş](#giriş)
2. [Mimari Genel Bakış](#mimari-genel-bakış)
3. [Program.cs ve Oyun Başlatma](#programcs-ve-oyun-başlatma)
4. [Windowing - Pencere Yönetimi](#windowing---pencere-yönetimi)
5. [Scene Management - Sahne Yönetimi](#scene-management---sahne-yönetimi)
6. [Camera - Kamera Sistemi](#camera---kamera-sistemi)
7. [Model3D - 3D Model Yönetimi](#model3d---3d-model-yönetimi)
8. [Skybox - Gökyüzü Sistemi](#skybox---gökyüzü-sistemi)
9. [Sprite2D - 2D Sprite Sistemi](#sprite2d---2d-sprite-sistemi)
10. [Character3D - Karakter Sistemi](#character3d---karakter-sistemi)
11. [PineObject2D - 2D Nesneler](#pineobject2d---2d-nesneler)
12. [Tam Örnek Projeler](#tam-örnek-projeler)

---

## Giriş

**Fix2Engine**, Raylib-cs kütüphanesine dayalı, hafif ancak güçlü bir C# oyun motorudur. 

### Önemli Özellikler:
- ✅ **3D Grafikleri**: Model yükleme, dönüştürme, ışıklandırma
- ✅ **2D Grafikleri**: Sprite sistemi, desen oluşturma
- ✅ **Sahne Sistemi**: Dinamik sahne geçişleri
- ✅ **Kamera Sistemi**: FirstPerson, ThirdPerson, Free kamera modları
- ✅ **ImGui Entegrasyonu**: Gerçek zamanlı UI ve debug araçları
- ✅ **Gökyüzü Sistemi**: Cubemap tabanlı skybox

### Desteklenen Formatlar:
- **3D Modeller**: .glb, .gltf, .iqm, .obj
- **Textürler**: .png, .jpg, .bmp, .tga
- **Ses**: .ogg, .flac, .mp3, .wav (Raylib aracılığıyla)

---

## Mimari Genel Bakış

```
Fix2Engine/
├── Fix2Engine.Graphics/
│   ├── Windowing.cs          (Pencere ve oyun döngüsü)
│   ├── Camera.cs             (Kamera yönetimi)
│   ├── Model3D.cs            (3D model işleme)
│   ├── Skybox.cs             (Gökyüzü)
│   └── Sprite2D.cs           (2D görüntüler)
└── Fix2Engine.Components/
    ├── Scene/
    │   ├── IFixScene.cs      (Sahne arayüzü)
    │   └── SceneManager.cs   (Sahne yöneticisi)
    ├── Character3D.cs        (Karakter sistemi)
    └── PineObject2D.cs       (2D nesneler)
```

---

## Program.cs ve Oyun Başlatma

Oyun başlatma süreci 3 adımda gerçekleşir:

### Adım 1: Program.cs

```csharp
using System;
using Fix2Engine.Graphics;

namespace Fix2Engine
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            // Oyun sınıfını oluştur ve çalıştır
            using var game = new Fix2Engine.Game();
            game.Run();
        }
    }
}
```

**İşlem Akışı:**
1. `new Fix2Engine.Game()` - Oyun sınıfı örneklendirilir
2. Yapıcı metot çalışır ve pencere oluşturulur
3. `game.Run()` - Ana oyun döngüsü başlatılır

### Adım 2: Game.cs (Oyun Sınıfı)

```csharp
using Raylib_cs;
using rlImGui_cs;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;

namespace Fix2Engine
{
    public class Game : Windowing
    {
        // Yapıcı metot - pencereyi 1024x768 boyutunda oluşturur
        public Game() : base(1024, 768, "Fix2Engine - DEBUGGING") { }

        // Adım 1: İlk kurulum
        protected override void Start()
        {
            rlImGui.Setup(true);
            SceneManager.LoadScene<Debug3DScene>();
        }

        // Adım 2: Sahnedeki nesneleri güncelle
        protected override void Update(float dt)
        {
            SceneManager.Update(dt);
        }

        // Adım 3: Ekrana çiz
        protected override void Render()
        {
            // 3D sahne çizimi
            SceneManager.Render();

            // ImGui UI çizimi
            rlImGui.Begin();
            SceneManager.RenderUI();
            rlImGui.End();

            // FPS göster
            Raylib.DrawFPS(Width - 90, 10);
        }
    }
}
```

**Yaşam Döngüsü:**

```
Program.cs çalışır
    ↓
Game() yapıcısı
    ↓
Init() çalışır
    ↓
Start() çalışır
    ↓
┌─────────────────────────────┐
│ Oyun Döngüsü (Loop)         │
│  1. FixedUpdate() → 60Hz   │
│  2. Update() → 240Hz       │
│  3. Render() → 240Hz       │
│  4. EndDrawing()           │
└─────────────────────────────┘
    ↓ (pencere kapatılırsa)
Dispose() çalışır
    ↓
Program biter
```

---

## Windowing - Pencere Yönetimi

`Windowing` sınıfı oyun penceresini ve ana döngüyü yönetir.

### Sınıf Tanımı

```csharp
public class Windowing : IDisposable
{
    public int Width { get; set; }          // Pencere genişliği
    public int Height { get; set; }         // Pencere yüksekliği
    public string Title { get; set; }       // Pencere başlığı
}
```

### Yapıcı Metot (Constructor)

```csharp
public Windowing(int width, int height, string title)
{
    Width = width;
    Height = height;
    Title = title;
    
    Init();                      // İlk setup
    InitWindow(Width, Height, Title);  // Raylib pencere oluştur
    SetTargetFPS(240);          // 240 FPS hedef
}
```

### Geçersiz Metotlar (Virtual Methods)

```csharp
// Pencere oluşturulmadan ÖNCE çalışır (Raylib başlatma için)
protected virtual void Init()
{
    // Override et ve burada Raylib ek setup'ları yap
}

// Döngü başladıktan SONRA, bir kez çalışır
protected virtual void Start()
{
    // Sahneleri, kaynakları ve nesneleri burada başlat
}

// Her karede çalışır (framerate'e bağlı)
protected virtual void Update(float dt)
{
    // Mantık, giriş, fizik hesaplamaları
    // dt = Delta Time (geçen kare süresi saniye cinsinden)
}

// 60 Hz'de (sabit) çalışır (Fizik için ideal)
protected virtual void FixedUpdate(float fixedDt)
{
    // Çarpışma, fizik simülasyonları
}

// Her karede çalışır (Render=çizim yapılır)
protected virtual void Render()
{
    // BeginDrawing() ve EndDrawing() Windowing tarafından yapılır
    // Sen sadece çizim komutları yaz
}
```

### Tam Örnek

```csharp
public class MyGame : Windowing
{
    private Camera _camera;
    
    public MyGame() : base(1280, 720, "Benim Oyunum") { }

    protected override void Init()
    {
        // İsteğe bağlı: Raylib config
        Raylib.SetConfigFlags(ConfigFlags.VsyncHint);
    }

    protected override void Start()
    {
        _camera = new Camera(
            position: new Vector3(0, 5, 10),
            target: Vector3.Zero,
            fov: 75
        );
    }

    protected override void Update(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            System.Environment.Exit(0);
        
        _camera.Update();
    }

    protected override void Render()
    {
        Raylib.ClearBackground(Color.Black);
        
        _camera.Begin();
        Raylib.DrawSphere(Vector3.Zero, 1.0f, Color.Red);
        _camera.End();
    }
}
```

---

## Scene Management - Sahne Yönetimi

Sahne yönetimi, oyunun farklı bölümlerini (menü, seviye, inventory vb.) organize etmek için kullanılır.

### IFixScene Arayüzü

Tüm sahneler bu arayüzü uygulamalıdır:

```csharp
public interface IFixScene : IDisposable
{
    void Start();          // Sahne yüklendikten sonra çalışır
    void Update(float dt); // Güncelleme mantığı
    void Render();         // 3D çizim
    void RenderUI();       // ImGui UI çizimi
    void Unload();         // Sahne kaldırıldığında çalışır
}
```

### SceneManager Kullanımı

```csharp
// Sahneyi türüne göre yükle (yeni örnek oluştur)
SceneManager.LoadScene<GameplayScene>();

// Veya mevcut örneği yükle
var customScene = new GameplayScene();
customScene.Initialize(); // eğer gerekli ise
SceneManager.LoadScene(customScene);

// Mevcut sahneyi al
if (SceneManager.CurrentScene != null)
{
    // Sahneye erişim
}

// Döngüde çağırılmalı (Windowing tarafından otomatik)
SceneManager.Update(dt);
SceneManager.Render();
SceneManager.RenderUI();

// Oyun bittiğinde temizle
SceneManager.Unload();
```

### Tam Sahne Örneği

```csharp
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;
using System.Numerics;

namespace Fix2Engine
{
    public class MainMenuScene : IFixScene
    {
        private Sprite2D _backgroundImage;
        private float _buttonY = 300;

        public void Start()
        {
            _backgroundImage = new Sprite2D("assets/menu_bg.png");
        }

        public void Update(float dt)
        {
            // Menü interaktifliği
            if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            {
                // Oyun sahnesine geç
                SceneManager.LoadScene<GameplayScene>();
            }
        }

        public void Render()
        {
            Raylib.ClearBackground(Color.Black);
            _backgroundImage.Draw();
        }

        public void RenderUI()
        {
            ImGui.SetNextWindowPos(new Vector2(100, 100));
            ImGui.SetNextWindowSize(new Vector2(400, 300));
            
            if (ImGui.Begin("Main Menu"))
            {
                if (ImGui.Button("Oyunu Başlat", new Vector2(200, 50)))
                    SceneManager.LoadScene<GameplayScene>();

                if (ImGui.Button("Çıkış", new Vector2(200, 50)))
                    Environment.Exit(0);

                ImGui.End();
            }
        }

        public void Unload()
        {
            _backgroundImage?.Dispose();
        }

        public void Dispose()
        {
            Unload();
        }
    }
}
```

---

## Camera - Kamera Sistemi

Kamera sistemi, 3D sahne görünümünü kontrol eder.

### Kamera Türleri

```csharp
public enum CameraType
{
    FirstPerson,   // Birinci şahıs (oyuncu gözünden)
    ThirdPerson,   // Üçüncü kişi (karakteri takip et)
    Free,          // Serbest kamera
    Custom         // Özel kamera davranışı
}

public enum ProjectionType
{
    Perspective,    // Normalişik (derinlik hissi)
    Orthographic   // Isometrik (2D benzeri)
}
```

### Yapıcı Metotlar

```csharp
// Varsayılan kamera
var camera = new Camera();
// Position: (0, 5, 10), Target: (0, 0, 0), FOV: 60

// Özel pozisyon ve hedef ile
var camera = new Camera(
    position: new Vector3(0, 5, 10),
    target: Vector3.Zero,
    fov: 75,
    type: CameraType.FirstPerson
);
```

### Kamera Özellikleri

```csharp
// Pozisyon ve yön
camera.Position = new Vector3(0, 2, 5);  // Kamera konumu
camera.Target = new Vector3(0, 0, 0);   // Nereye bakıyor
camera.Up = Vector3.UnitY;               // Yukarı yönü (genelde Y)

// Görüş özellikleri
camera.FOV = 75.0f;                      // Görüş açısı (derece)
camera.Type = CameraType.FirstPerson;   // Kamera tipi
camera.Projection = ProjectionType.Perspective; // Projeksiyon

// ThirdPerson için
camera.TargetOffset = new Vector3(0, 3, 5); // Karakterden uzaklık
```

### Kamera Metotları

```csharp
// Kamerayı hedef noktaya kilitler ve takip eder
camera.Follow(new Vector3(10, 0, 5));

// Kamerayı güncelle (FirstPerson/Free modlarında)
// İçinde fare ve klavye inputları işlenir
camera.Update();

// Raylib'in anlayacağı Camera3D yapısını döndür
Camera3D raylibCam = camera.GetRaylibCamera();

// 3D çizim başla (çizim komutları arasında)
camera.Begin();
Raylib.DrawSphere(Vector3.Zero, 1.0f, Color.Red);
camera.End();
```

### Tam Kamera Örneği

```csharp
public class GameplayScene : IFixScene
{
    private Camera _camera;
    private Model3D _player;

    public void Start()
    {
        // FirstPerson kamera
        _camera = new Camera(
            position: new Vector3(0, 2, 10),
            target: Vector3.Zero,
            fov: 75,
            type: CameraType.FirstPerson
        );

        _player = new Model3D("assets/models/player.glb");
    }

    public void Update(float dt)
    {
        // Fare gizli/göster (F1)
        if (Raylib.IsKeyPressed(KeyboardKey.F1))
        {
            if (Raylib.IsCursorHidden())
                Raylib.EnableCursor();
            else
                Raylib.DisableCursor();
        }

        // Fare kilitliyse kamerayı kontrol et
        if (Raylib.IsCursorHidden())
            _camera.Update();
    }

    public void Render()
    {
        Raylib.ClearBackground(Color.Black);

        _camera.Begin();
        _player.Draw();
        Raylib.DrawGrid(20, 1.0f);
        _camera.End();
    }

    public void RenderUI() { }

    public void Unload() => _player.Unload();

    public void Dispose() => Unload();
}
```

---

## Model3D - 3D Model Yönetimi

3D modelleri yüklemek, dönüştürmek ve çizmek için kullanılır.

### Desteklenen Formatlar
- ✅ .glb, .gltf (Gltf binary/text)
- ✅ .iqm (Inter-Quake Model)
- ✅ .obj (OBJ format)

### Sınıf Özellikleri

```csharp
public class Model3D
{
    // Dönüştürme
    public Vector3 Position { get; set; } = Vector3.Zero;    // Konum
    public Vector3 Rotation { get; set; } = Vector3.Zero;    // Dönüş (derece)
    public Vector3 Scale { get; set; } = Vector3.One;        // Boyut

    // Renk
    public Color Tint { get; set; } = Color.White;           // Renkli ton
}
```

### Model Yükleme ve Kullanım

```csharp
// 1. Model yükle
var model = new Model3D("assets/models/boss.glb");

// 2. Özellikleri ayarla
model.Position = new Vector3(0, 0, 0);
model.Rotation = new Vector3(90, 0, 0);  // X ekseni etrafında 90°
model.Scale = new Vector3(100, 100, 100);
model.Tint = Color.Red;

// 3. Çiz
model.Draw();

// 4. Temizle
model.Unload();
```

### Metotlar

```csharp
// Özel doku yükle
model.SetTexture("assets/textures/diffuse.png");

// Modeli çiz (dönüştürme matrisi uygulanır)
model.Draw();

// Kaynakları serbest bırak
model.Unload();
```

### Dönüş Sistemi

Dönüş Euler açıları ile yapılır (X, Y, Z eksenleri):

```csharp
model.Rotation = new Vector3(
    0,    // X ekseni (eğim - pitch)
    45,   // Y ekseni (sapma - yaw)
    0     // Z ekseni (ayak - roll)
);
```

### Tam Örnek

```csharp
public class Scene3D : IFixScene
{
    private Model3D _tree;
    private Model3D _rock;
    private Model3D _castle;

    public void Start()
    {
        // Ağaç
        _tree = new Model3D("assets/models/tree.glb");
        _tree.Position = new Vector3(-5, 0, 0);
        _tree.Scale = new Vector3(1, 1, 1);

        // Kaya
        _rock = new Model3D("assets/models/rock.glb");
        _rock.Position = new Vector3(5, 0, 0);
        _rock.Rotation = new Vector3(0, 45, 0);

        // Kale
        _castle = new Model3D("assets/models/castle.glb");
        _castle.Position = new Vector3(0, 0, 10);
        _castle.Scale = new Vector3(2, 2, 2);
    }

    public void Update(float dt)
    {
        // Ağacı döndür
        _tree.Rotation = new Vector3(
            0,
            (float)DateTime.Now.TotalSeconds * 30, // Saniyede 30° dön
            0
        );
    }

    public void Render()
    {
        Raylib.ClearBackground(Color.SkyBlue);
        _tree.Draw();
        _rock.Draw();
        _castle.Draw();
    }

    public void RenderUI() { }

    public void Unload()
    {
        _tree.Unload();
        _rock.Unload();
        _castle.Unload();
    }

    public void Dispose() => Unload();
}
```

---

## Skybox - Gökyüzü Sistemi

Ortamı çevreleyen gökyüzü (cubemap) oluşturur.

### Cubemap Nedir?

Cubemap, 6 yüzü olan (yukarı, aşağı, ön, arka, sol, sağ) bir 3D tekstürür. Oyuncu kamerasını neye çevirirse çevirse, görüş tamamıyla ortamla kaplanır.

### Desteklenen Formatlar
- Derlenmiş cubemap dosyaları
- HDR .hdr dosyaları
- Panorama tekstürleri

### Skybox Kullanımı

```csharp
// Skybox oluştur
var skybox = new Skybox("assets/skybox.png");

// Kameranın pozisyonunu merkeze alarak çiz
skybox.Draw(cameraPosition);

// Temizle
skybox.Unload();
```

### Sınıf Özellikleri

```csharp
public class Skybox
{
    // Özel shader ve mesh içerir
    // Derinlik testi devre dışı bırakılır
    // Ters yüz culling yapmaz
}
```

### Tam Sahne Örneği

```csharp
public class OutdoorScene : IFixScene
{
    private Skybox _skybox;
    private Camera _camera;
    private Model3D _terrain;

    public void Start()
    {
        // Skybox yükle
        _skybox = new Skybox("assets/skybox.png");

        // Kamera
        _camera = new Camera(
            position: new Vector3(0, 5, 0),
            target: Vector3.Zero,
            fov: 75
        );

        // Arazi modeli
        _terrain = new Model3D("assets/models/terrain.glb");
    }

    public void Update(float dt)
    {
        _camera.Update();
    }

    public void Render()
    {
        Raylib.ClearBackground(Color.Black);

        _camera.Begin();
        
        // Skybox'ı en arkada çiz
        _skybox.Draw(_camera.Position);

        // Diğer nesneleri çiz
        _terrain.Draw();
        Raylib.DrawGrid(100, 1.0f);

        _camera.End();
    }

    public void RenderUI() { }

    public void Unload()
    {
        _skybox.Unload();
        _terrain.Unload();
    }

    public void Dispose() => Unload();
}
```

---

## Sprite2D - 2D Sprite Sistemi

2D görüntüleri, UI'ları ve 2D oyun nesnelerini çizmek için.

### Sınıf Özellikleri

```csharp
public class Sprite2D : IDisposable
{
    // Pozisyon ve dönüş
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0.0f;
    
    // Boyut ve merkez noktası
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    
    // Renk tonlaması
    public Color Tint { get; set; } = Color.White;
    
    // Kaynak alan (spritesheet'ler için)
    public Rectangle SourceRect { get; set; }
    
    // İç doku
    public Texture2D Texture { get; private set; }
}
```

### Sprite Oluşturma

```csharp
// Dosyadan yükle
var sprite = new Sprite2D("assets/images/player.png");

// Raylib Texture2D nesnesinden oluştur
Texture2D texture = Raylib.LoadTexture("assets/images/sprite.png");
var sprite = new Sprite2D(texture);
```

### Metotlar

```csharp
// Pivot noktasını ortaya ayarla
sprite.CenterOrigin();

// Sprite'ı çiz
sprite.Draw();

// Temizle
sprite.Dispose();
```

### Tam Sprite Örneği

```csharp
public class UIScene : IFixScene
{
    private Sprite2D _background;
    private Sprite2D _player;
    private Sprite2D _button;

    public void Start()
    {
        // Arka plan
        _background = new Sprite2D("assets/images/bg.png");
        _background.Position = Vector2.Zero;

        // Oyuncu karakteri
        _player = new Sprite2D("assets/images/hero.png");
        _player.Position = new Vector2(400, 300);
        _player.CenterOrigin();
        _player.Scale = new Vector2(2, 2);

        // Buton
        _button = new Sprite2D("assets/images/button.png");
        _button.Position = new Vector2(600, 500);
        _button.Tint = Color.Green;
    }

    public void Update(float dt)
    {
        // Oyuncuyu kontrol et
        if (Raylib.IsKeyDown(KeyboardKey.Left))
            _player.Position.X -= 200 * dt;
        if (Raylib.IsKeyDown(KeyboardKey.Right))
            _player.Position.X += 200 * dt;

        // Oyuncuyu döndür
        _player.Rotation += 90 * dt;
    }

    public void Render()
    {
        Raylib.ClearBackground(Color.Black);
        _background.Draw();
        _player.Draw();
        _button.Draw();
    }

    public void RenderUI() { }

    public void Unload()
    {
        _background.Dispose();
        _player.Dispose();
        _button.Dispose();
    }

    public void Dispose() => Unload();
}
```

### Spritesheet Animasyonu

```csharp
public class AnimatedSprite : IDisposable
{
    private Sprite2D _sprite;
    private float _frameTimer;
    private int _currentFrame;
    private int _framesPerSecond;

    public AnimatedSprite(string path, int rows, int cols, int fps)
    {
        _sprite = new Sprite2D(path);
        _framesPerSecond = fps;
        
        int frameWidth = _sprite.Texture.Width / cols;
        int frameHeight = _sprite.Texture.Height / rows;
        _sprite.SourceRect = new Rectangle(0, 0, frameWidth, frameHeight);
        _sprite.CenterOrigin();
    }

    public void Update(float dt)
    {
        _frameTimer += dt;
        float frameDuration = 1f / _framesPerSecond;

        if (_frameTimer >= frameDuration)
        {
            _currentFrame++;
            _frameTimer = 0;

            int cols = _sprite.Texture.Width / (int)_sprite.SourceRect.Width;
            if (_currentFrame >= cols)
                _currentFrame = 0;

            _sprite.SourceRect = new Rectangle(
                _currentFrame * (int)_sprite.SourceRect.Width,
                0,
                _sprite.SourceRect.Width,
                _sprite.SourceRect.Height
            );
        }
    }

    public void Draw(Vector2 position)
    {
        _sprite.Position = position;
        _sprite.Draw();
    }

    public void Dispose() => _sprite.Dispose();
}
```

---

## Character3D - Karakter Sistemi

Oynanabilir veya NPC karakterleri yönetmek için temel sistem.

### Sınıf Özellikleri

```csharp
public class Character3D
{
    public Guid _guid { get; private set; }  // Benzersiz kimlik
    public string Name { get; set; }         // Karakter adı
}
```

### Genişletilmiş Karakter Örneği

```csharp
public class Player : Character3D
{
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    
    private Model3D _model;
    private float _speed = 5.0f;
    private float _jumpForce = 10.0f;

    public Player(string name, string modelPath)
    {
        Name = name;
        _model = new Model3D(modelPath);
        Position = Vector3.Zero;
    }

    public void HandleInput()
    {
        if (Raylib.IsKeyDown(KeyboardKey.W))
            Velocity.Z += _speed * Raylib.GetFrameTime();
        if (Raylib.IsKeyDown(KeyboardKey.A))
            Velocity.X -= _speed * Raylib.GetFrameTime();
        if (Raylib.IsKeyDown(KeyboardKey.S))
            Velocity.Z -= _speed * Raylib.GetFrameTime();
        if (Raylib.IsKeyDown(KeyboardKey.D))
            Velocity.X += _speed * Raylib.GetFrameTime();

        if (Raylib.IsKeyPressed(KeyboardKey.Space))
            Velocity.Y = _jumpForce;
    }

    public void Update(float dt)
    {
        HandleInput();

        // Yer çekimi
        Velocity.Y -= 9.81f * dt;

        // Pozisyon güncelle
        Position += Velocity * dt;

        // Modeli güncelle
        _model.Position = Position;
    }

    public void Draw() => _model.Draw();

    public void Unload() => _model.Unload();
}
```

---

## PineObject2D - 2D Nesneler

Sahne grafları (parent-child ilişkileri) ile 2D oyun nesneleri.

### Sınıf Özellikleri

```csharp
public class PineObject2D
{
    public Guid Guid { get; private set; }              // Kimlik
    public string Name { get; set; }                    // Adı
    public bool IsActive { get; set; } = true;          // Aktif mi?

    // Dönüştürme
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f;
    public Vector2 Scale { get; set; } = Vector2.One;

    // Render
    public string TexturePath { get; set; }
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public Color Tint { get; set; } = Color.White;

    // Parent-Child sistemi
    public PineObject2D? Parent { get; private set; }
    public IReadOnlyList<PineObject2D> Children { get; }
    
    // Dünya koordinatları
    public Vector2 GlobalPosition { get; }
}
```

### Parent-Child Sistemi

```csharp
// Nesne oluştur
var parent = new PineObject2D("Parent");
var child1 = new PineObject2D("Child1");
var child2 = new PineObject2D("Child2");

// Hiyerarşi oluştur
parent.AddChild(child1);
parent.AddChild(child2);

// Parent hareket ettiğinde, çocuklar da hareket eder
parent.Position = new Vector2(100, 100);
Console.WriteLine(child1.GlobalPosition); // (100 + child1.Position)

// Çocuğu kaldır
parent.RemoveChild(child1);
```

### Tam Örnek - UI Sistemi

```csharp
public class UIPanel : PineObject2D
{
    private List<PineObject2D> _buttons = new();

    public UIPanel(string name) : base(name) { }

    public void AddButton(string buttonName, Vector2 position)
    {
        var button = new PineObject2D(buttonName);
        button.Position = position;
        button.TexturePath = "assets/images/button.png";
        AddChild(button);
        _buttons.Add(button);
    }

    public override void Update(float deltaTime)
    {
        // Paneli hareket ettir
        if (Raylib.IsKeyDown(KeyboardKey.Left))
            Position.X -= 100 * deltaTime;

        base.Update(deltaTime);

        // Butonları kontrol et
        foreach (var button in _buttons)
        {
            var mousePos = Raylib.GetMousePosition();
            var buttonRect = new Rectangle(
                button.GlobalPosition.X,
                button.GlobalPosition.Y,
                50, 50
            );

            if (Raylib.CheckCollisionPointRec(mousePos, buttonRect))
            {
                button.Tint = Color.Yellow;
                
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    // Buton tıklandı
                }
            }
            else
            {
                button.Tint = Color.White;
            }
        }
    }
}
```

---

## Tam Örnek Projeler

### 1. Basit 3D Sahne

```csharp
using Fix2Engine;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;
using System.Numerics;
using ImGuiNET;
using Raylib_cs;

public class Simple3DScene : IFixScene
{
    private Camera _camera;
    private Model3D _cube;
    private Model3D _sphere;
    private Skybox _skybox;
    private float _cubeRotation = 0;

    public void Start()
    {
        _camera = new Camera(
            position: new Vector3(0, 5, 10),
            target: Vector3.Zero,
            fov: 75,
            type: CameraType.FirstPerson
        );

        _cube = new Model3D("assets/models/cube.glb");
        _cube.Position = new Vector3(-3, 0, 0);

        _sphere = new Model3D("assets/models/sphere.glb");
        _sphere.Position = new Vector3(3, 0, 0);

        _skybox = new Skybox("assets/skybox.png");
    }

    public void Update(float dt)
    {
        _camera.Update();

        // Küpü döndür
        _cubeRotation += 45 * dt;
        _cube.Rotation = new Vector3(0, _cubeRotation, 0);

        // ESC ile çık
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            Environment.Exit(0);
    }

    public void Render()
    {
        Raylib.ClearBackground(Color.Black);
        _camera.Begin();

        _skybox.Draw(_camera.Position);
        _cube.Draw();
        _sphere.Draw();
        Raylib.DrawGrid(50, 1.0f);

        _camera.End();
    }

    public void RenderUI()
    {
        ImGui.SetNextWindowPos(new Vector2(10, 10));
        if (ImGui.Begin("Info"))
        {
            ImGui.Text($"FPS: {Raylib.GetFPS()}");
            ImGui.Text($"Camera Pos: {_camera.Position}");
            ImGui.Text($"Cube Rot: {_cubeRotation}");
            ImGui.End();
        }
    }

    public void Unload()
    {
        _cube.Unload();
        _sphere.Unload();
        _skybox.Unload();
    }

    public void Dispose() => Unload();
}
```

### 2. İnteraktif Karakter

```csharp
using Fix2Engine;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;
using System.Numerics;

public class CharacterScene : IFixScene
{
    private Camera _camera;
    private Player _player;
    private Model3D _ground;

    public void Start()
    {
        _player = new Player("Hero", "assets/models/hero.glb");
        _player.Position = new Vector3(0, 1, 0);

        _ground = new Model3D("assets/models/ground.glb");
        _ground.Scale = new Vector3(10, 0.1f, 10);

        _camera = new Camera(
            position: new Vector3(0, 3, 5),
            target: Vector3.Zero,
            fov: 75,
            type: CameraType.ThirdPerson
        );
    }

    public void Update(float dt)
    {
        _player.Update(dt);
        
        // Kamerayı oyuncuyu takip ettir
        _camera.Follow(_player.Position);
    }

    public void Render()
    {
        Raylib.ClearBackground(Color.SkyBlue);
        _camera.Begin();

        _ground.Draw();
        _player.Draw();
        Raylib.DrawGrid(20, 1.0f);

        _camera.End();
    }

    public void RenderUI() { }

    public void Unload()
    {
        _player.Unload();
        _ground.Unload();
    }

    public void Dispose() => Unload();
}
```

### 3. Sahne Geçişi Sistemi

```csharp
public class MenuScene : IFixScene
{
    public void Start() { }

    public void Update(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
            SceneManager.LoadScene<GameScene>();
    }

    public void Render()
    {
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawText("ANA MENÜ - Başlamak için ENTER tuşuna basın", 200, 300, 30, Color.White);
    }

    public void RenderUI() { }

    public void Unload() { }

    public void Dispose() { }
}

public class GameScene : IFixScene
{
    private Model3D _world;

    public void Start()
    {
        _world = new Model3D("assets/models/level1.glb");
    }

    public void Update(float dt)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
            SceneManager.LoadScene<MenuScene>();
    }

    public void Render()
    {
        _world.Draw();
    }

    public void RenderUI() { }

    public void Unload()
    {
        _world.Unload();
    }

    public void Dispose() => Unload();
}

// Program.cs
public class MyGame : Windowing
{
    public MyGame() : base(1024, 768, "Game") { }

    protected override void Start()
    {
        rlImGui.Setup(true);
        SceneManager.LoadScene<MenuScene>();
    }

    protected override void Update(float dt)
    {
        SceneManager.Update(dt);
    }

    protected override void Render()
    {
        Raylib.ClearBackground(Color.Black);
        SceneManager.Render();

        rlImGui.Begin();
        SceneManager.RenderUI();
        rlImGui.End();
    }
}
```

---

## İpuçları ve En İyi Uygulamalar

### ✅ Yapılması Gerekenler

1. **Resource Yönetimi**: Kullandığınız modelleri, textürleri `Unload()` ile temizleyin
2. **Sahne Yönetimi**: Büyük projeler için sahne geçişleri kullanın
3. **Kamera Ayarları**: Her sahne için uygun kamera tipi seçin
4. **ImGui Debugging**: Oyunun içinden debug bilgilerini gösterin

### ❌ Yapılmaması Gerekenler

1. **Sonsuz Döngüler**: `Update()` içinde bekleme (blocking) yapmayın
2. **Dinamik Bellek Sızıntıları**: Dispose() çağrılmayan kaynakları bırakmayın
3. **Ana Döngüyü Engelleme**: `Run()` çağrıldıktan sonra kontrol dökmez

### 📊 Performans Tipsı

```csharp
// Çok sayıda nesnesi çiziyorsanız
for (int i = 0; i < 1000; i++)
{
    _models[i].Draw(); // Batch rendering kullanın
}

// Raylib'in batch sistemi otomatik optimize eder
// Düzeltilmiş dönüştürme matrisleri kullanın
```

---

## Kaynakça

- **Raylib-cs**: https://github.com/chrisdill/raylib-cs
- **Raylib**: https://www.raylib.com/
- **ImGui.NET**: https://github.com/ImGuiNET/ImGui.NET
- **.NET 10.0**: https://dotnet.microsoft.com/

---

## Lisans

Fix2Engine - MIT License

**Son Güncelleme**: 2026-07-26

