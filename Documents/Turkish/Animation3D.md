# 3D Animasyonlar

3D sistemi, 2D animasyon sisteminin yanında çalışan kendi animasyon katmanına
sahiptir. Her şey motora ait ve arka uçtan bağımsızdır: transform klipleri,
zaman çizelgeleri, kamera ve materyal animasyonu yalnızca Fix2Engine türlerini
kullanır; iskelet animasyonu ise GLB/GLTF dosyalarındaki klipleri aktif render
arka ucu üzerinden okur.

Bir 3D sahneyi üç şekilde canlandırabilirsin:

1. **Transform animasyonu** bir `Object3D`'yi hareket ettirir, döndürür ve ölçekler.
2. **Zaman çizelgeleri** kamerayı, materyali veya özel bir değeri zamanla sürer.
3. **İskelet animasyonu** bir GLB/GLTF modeline gömülü kemik kliplerini oynatır.

## Transform animasyonu

`TransformAnimator3D`, bağlı olduğu `Object3D` üzerinde `TransformClip3D`
anahtar karelerini oynatır. Konum, dönüş ve ölçek izlerini içeren bir klip
oluştur, animatörü bağla ve klibi adıyla oynat:

```csharp
using System.Numerics;
using Fix2Engine;
using Fix2Engine.Components;
using Fix2Engine.Components.Animation3D;
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;

namespace MyGame;

public sealed class Game : FixGame
{
    private Scene3D? _world;

    protected override void Start()
    {
        _world = new Scene3D();
        _world.Camera.Position = new Vector3(7, 5, 7);
        _world.Camera.Target = Vector3.Zero;
        Scene3DManager.Load(_world);

        var beacon = new Object3D("Beacon");
        beacon.AddComponent(new ModelRenderer3D(
            Model3D.Cube(new Vector3(1)), Material3D.FromPreset(ShaderSet.Toon),
            ownsModel: true, ownsMaterial: true));
        _world.Add(beacon);

        var clip = new TransformClip3D("orbit")
            .AddPosition(0f, new Vector3(-3, 0, 0))
            .AddPosition(2f, new Vector3(3, 0, 0), EasingKind.SineInOut)
            .AddRotation(0f, Vector3.Zero)
            .AddRotation(2f, new Vector3(0, 360, 0))
            .AddScale(0f, Vector3.One)
            .AddScale(2f, new Vector3(1.5f), EasingKind.BackOut);

        beacon.AddComponent(new TransformAnimator3D()).Add(clip).Play("orbit");
    }
}
```

Dönüş anahtarları X pitch, Y yaw, Z roll sırasıyla Euler derece kullanır. Eksik
izler kimlik değerini korur; bu yüzden bir klip örneğin yalnızca konumu
canlandırabilir.

Animatör, sahneye ait bir nesneye bağlandığında oynatma otomatiktir; `Update`'i
kendin çağırma. Klipler varsayılan olarak döngülüdür. Tek seferlik bir klip için
`clip.Loop = false` yap.

### Oynatma kontrolü

`TransformAnimator3D`; `Play`, `Pause`, `Resume`, `Stop`, `Seek` (saniye cinsinden)
ve `Speed` ile birlikte `Started`, `Finished` ve `Looped` olaylarını sunar. `Play`
tekrar çağrıldığında ilerlemeyi korur; baştan almak için `restart: true` geç:

```csharp
var animator = new TransformAnimator3D();
animator.Add(clip);
animator.Started += c => Console.WriteLine($"Başladı {c.Name}");
animator.Finished += c => Console.WriteLine($"Bitti {c.Name}");
animator.Play("orbit");
```

Aynı klip birden çok nesneye bağlanıp bağımsız oynatılabilir.

## Easing

Her anahtar kare bir `EasingKind` kabul eder. `Easing.Evaluate` aynı eğrileri özel
kullanım için de sunar; giriş ve çıkış 0..1 aralığına sınırlanır:

```csharp
float eased = Easing.Evaluate(EasingKind.BounceOut, 0.75f);
```

## Zaman çizelgeleri

`Timeline3D` bağımsız izleri paralel olarak çalıştırır. Her iz, yumuşatılmış 0..1
ilerleme değeri alır; bu yüzden yalnızca transformları değil, her şeyi sürebilir:

```csharp
var timeline = new Timeline3D { Loop = true };
timeline.Add(0f, 2f, t => Console.WriteLine($"ilerleme {t}"), EasingKind.CubicInOut);
timeline.Play();
```

Zaman çizelgesinin sahneyle otomatik ilerlemesi için bir `Object3D`'ye
`TimelineComponent3D` bağla ya da `Timeline.Update(dt)`'yi kendin çağır.

## Kamera animasyonu

`CameraAnimator3D`, bağlı olduğu nesnenin ait olduğu `Scene3D`'nin kamerasını
canlandırır:

```csharp
var camera = new CameraAnimator3D()
    .AnimatePosition(0f, 3f, new Vector3(10, 6, 10), new Vector3(0, 2, 8), EasingKind.SineInOut)
    .AnimateTarget(0f, 3f, Vector3.Zero, new Vector3(0, 1, 0))
    .AnimateFieldOfView(0f, 3f, 70f, 50f);

_world.Add(new Object3D("CameraDriver")).AddComponent(camera);
camera.Play();
```

## Materyal animasyonu

`MaterialAnimator3D`, adlandırılmış shader parametrelerini canlandırır. Kurucuda
verilen materyali ya da aynı nesnedeki ilk `ModelRenderer3D`'yi hedefler:

```csharp
var material = Material3D.FromPreset(ShaderSet.Toon).SetFloat("pulse", 0f);
var node = new Object3D();
node.AddComponent(new ModelRenderer3D(Model3D.Cube(Vector3.One), material,
    ownsModel: true, ownsMaterial: true));
var animator = new MaterialAnimator3D(material)
    .AnimateFloat("pulse", 0f, 1f, 0f, 10f);
node.AddComponent(animator);
animator.Play();
_world.Add(node);
```

## İskelet animasyonu

GLB/GLTF dosyalarından yüklenen modeller kemik animasyonları taşıyabilir. Modeli ve
animasyon setini yükle, ardından `ModelAnimator3D`'nin bir klibi adıyla oynatmasına
izin ver:

```csharp
using var model = Model3D.Load("Assets/robot.glb");
using var animations = model.LoadAnimations();

var node = new Object3D("Robot");
node.AddComponent(new ModelRenderer3D(model, material,
    ownsModel: true, ownsMaterial: true));
var animator = new ModelAnimator3D(model, animations);
node.AddComponent(animator);
animator.Play("walk");
_world.Add(node);
```

`ModelAnimator3D`, transform animatörünün API'sini yansıtır: `Play`, `Pause`,
`Resume`, `Stop`, `Seek`, `Speed`, `Loop` ve `Started`, `Finished`, `Looped`
olayları. `Play`, klibin iskeletinin modelle uyuştuğunu doğrular. Animasyon seti
varsayılan olarak ödünç alınır; bileşenin onu serbest bırakmasını istersen
`ownsAnimations: true` geç.

Desteklenen biçimler arka uca bağlıdır. Dahil edilen Raylib arka ucu, model
yükleyicisinin kabul ettiği biçimleri yükler (ör. `.glb`, `.gltf`, `.iqm`, `.m3d`);
`.fbx` yüklemez. Üretilen ilkeller animasyon verisi içermediği için bunlarda
`LoadAnimations` hata fırlatır.

Materyaller için [3D Shader Setleri](Shaders3D.md) ile devam et ya da
[İlk 3D Sahnen](Hello3D.md) sayfasına dön.
