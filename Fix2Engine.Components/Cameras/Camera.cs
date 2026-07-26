using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Fix2Engine.Graphics
{
    public enum CameraType
    {
        FirstPerson,
        ThirdPerson,
        Free,
        Custom
    }

    public enum ProjectionType
    {
        Perspective,
        Orthographic
    }

    public class Camera
    {
        // Uzay & Yönelim Özellikleri
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; } = new Vector3(0.0f, 1.0f, 0.0f);

        // Görüş & Perspektif Özellikleri
        public float FOV { get; set; } = 60.0f;
        public CameraType Type { get; set; } = CameraType.FirstPerson;
        public ProjectionType Projection { get; set; } = ProjectionType.Perspective;

        // ThirdPerson için takip mesafesi
        public Vector3 TargetOffset { get; set; } = new Vector3(0.0f, 3.0f, 5.0f);

        // --- CONSTRUCTORS (YAPICI METOTLAR) ---
        
        /// <summary>
        /// Boş Kamera Oluşturur
        /// </summary>
        public Camera()
        {
            Position = new Vector3(0.0f, 5.0f, 10.0f);
            Target = Vector3.Zero;
        }

        /// <summary>
        /// Belirli Pozisyon ve Hedef ile Kamera Oluşturur
        /// </summary>
        public Camera(Vector3 position, Vector3 target, float fov = 60.0f, CameraType type = CameraType.FirstPerson)
        {
            Position = position;
            Target = target;
            FOV = fov;
            Type = type;
        }

        // --- İŞLEVSEL METOTLAR ---

        /// <summary>
        /// Kamerayı bir hedef noktaya veya karaktere kilitler (ThirdPerson için idealdir)
        /// </summary>
        public void Follow(Vector3 targetPosition)
        {
            Target = targetPosition;
            if (Type == CameraType.ThirdPerson)
            {
                Position = targetPosition + TargetOffset;
            }
        }

        /// <summary>
        /// Raylib'in yerleşik kamera kontrolleriyle kamerayı günceller.
        /// </summary>
        public void Update()
        {
            if (Type == CameraType.Free || Type == CameraType.FirstPerson)
            {
                Camera3D rCam = GetRaylibCamera();
                CameraMode mode = Type == CameraType.FirstPerson ? CameraMode.FirstPerson : CameraMode.Free;
                
                UpdateCamera(ref rCam, mode);

                // Raylib'in güncellediği pozisyonu kendi sınıfımıza aktarırız
                Position = rCam.Position;
                Target = rCam.Target;
            }
        }

        /// <summary>
        /// Verileri Raylib'in anlayacağı Camera3D struct yapısına çevirir.
        /// </summary>
        public Camera3D GetRaylibCamera()
        {
            return new Camera3D
            {
                Position = Position,
                Target = Target,
                Up = Up,
                FovY = FOV,
                Projection = Projection == ProjectionType.Perspective 
                    ? CameraProjection.Perspective 
                    : CameraProjection.Orthographic
            };
        }

        /// <summary>
        /// 3D Çizim modunu başlatır.
        /// </summary>
        public void Begin()
        {
            BeginMode3D(GetRaylibCamera());
        }

        /// <summary>
        /// 3D Çizim modunu bitirir.
        /// </summary>
        public void End()
        {
            EndMode3D();
        }
    }
}