using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Fix2Engine.Components.Cameras
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
        // Space & Orientation Properties
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; } = new Vector3(0.0f, 1.0f, 0.0f);

        // View & Perspective Properties
        public float FOV { get; set; } = 60.0f;
        public CameraType Type { get; set; } = CameraType.FirstPerson;
        public ProjectionType Projection { get; set; } = ProjectionType.Perspective;

        // Follow distance for ThirdPerson
        public Vector3 TargetOffset { get; set; } = new Vector3(0.0f, 3.0f, 5.0f);

        // Movement & Look Sensitivity
        public float MoveSpeed { get; set; } = 6.0f;
        public float MouseSensitivity { get; set; } = 0.003f;

        // Clip Planes - prevents z-fighting/clipping in large-scale scenes
        public double NearPlane { get; set; } = 0.05;
        public double FarPlane { get; set; } = 10000.0;

        // --- CONSTRUCTORS ---
        
        /// <summary>
        /// Creates an empty camera.
        /// </summary>
        public Camera()
        {
            Position = new Vector3(0.0f, 5.0f, 10.0f);
            Target = Vector3.Zero;
        }

        /// <summary>
        /// Creates a camera with specified position and target.
        /// </summary>
        public Camera(Vector3 position, Vector3 target, float fov = 60.0f, CameraType type = CameraType.FirstPerson)
        {
            Position = position;
            Target = target;
            FOV = fov;
            Type = type;
        }

        // --- FUNCTIONAL METHODS ---

        /// <summary>
        /// Locks the camera to a target point or character (ideal for ThirdPerson).
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
        /// Updates the camera using Raylib's built-in camera controls.
        /// </summary>
        public void Update()
        {
            if (Type == CameraType.Free || Type == CameraType.FirstPerson)
            {
                Camera3D rCam = GetRaylibCamera();
                float dt = GetFrameTime();

                Vector3 movement = Vector3.Zero;
                movement.X = (IsKeyDown(KeyboardKey.W) ? 1.0f : 0.0f) - (IsKeyDown(KeyboardKey.S) ? 1.0f : 0.0f);
                movement.Y = (IsKeyDown(KeyboardKey.D) ? 1.0f : 0.0f) - (IsKeyDown(KeyboardKey.A) ? 1.0f : 0.0f);
                movement.Z = (IsKeyDown(KeyboardKey.Space) ? 1.0f : 0.0f) - (IsKeyDown(KeyboardKey.LeftControl) ? 1.0f : 0.0f);
                movement *= MoveSpeed * dt * 10.0f;

                Vector2 mouseDelta = GetMouseDelta();
                Vector3 rotation = new Vector3(
                    mouseDelta.X * MouseSensitivity,
                    mouseDelta.Y * MouseSensitivity,
                    0.0f
                );

                UpdateCameraPro(ref rCam, movement, rotation, 0.0f);

                // Transfer the position updated by Raylib to our class
                Position = rCam.Position;
                Target = rCam.Target;
            }
        }

        /// <summary>
        /// Converts data to Raylib's Camera3D struct.
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
        /// Begins 3D rendering mode.
        /// </summary>
        public void Begin()
        {
            Rlgl.SetClipPlanes(NearPlane, FarPlane);
            BeginMode3D(GetRaylibCamera());
        }

        /// <summary>
        /// Ends 3D rendering mode.
        /// </summary>
        public void End()
        {
            EndMode3D();
        }
    }
}