using System;
using System.Numerics;
using Raylib_cs;
using ImGuiNET;
using Fix2Engine.Graphics;
using Fix2Engine.Components.Scene;

using Fix2Engine.Input;
using Fix2Engine.Input.InputBackend;
using static Raylib_cs.Raylib;

namespace Fix2Engine
{
    public class Debug3DScene : IFixScene
    {
        private Camera _camera;

        private bool _flashlightEnabled = false;

        private const float FlashlightRange = 14.0f;
        private const float FlashlightAngle = 32.0f;

        private float _bobTime = 0.0f;
        private float _bobAmount = 0.0f;

        private const float BobFrequency = 9.0f;
        private const float BobVertical = 0.055f;
        private const float BobHorizontal = 0.035f;

        private Vector3 _cameraPosition;

        private Vector3 _lightPosition =
            new Vector3(0.0f, 5.5f, 0.0f);

        private Color _lightColor =
            new Color(255, 220, 150, 255);

        private bool _showDemoWindow = false;

        private readonly Color _floorColor =
            new Color(16, 17, 20, 255);

        private readonly Color _wallColor =
            new Color(27, 23, 23, 255);

        private readonly Color _ceilingColor =
            new Color(9, 9, 11, 255);

        private readonly Color _pillarColor =
            new Color(32, 29, 28, 255);

        private readonly Color _detailColor =
            new Color(65, 52, 42, 255);

        public void Start()
        {
            _camera = new Camera(
                position: new Vector3(0.0f, 2.0f, 10.0f),
                target: new Vector3(0.0f, 2.0f, 0.0f),
                fov: 75.0f,
                type: CameraType.FirstPerson
            );

            _cameraPosition = _camera.Position;

            DisableCursor();
        }

        public void Update(float dt)
        {
            if (InputManager.Input.IsPressed(Keys.F1))
            {
                if (IsCursorHidden())
                    EnableCursor();
                else
                    DisableCursor();
            }

            if (InputManager.Input.IsPressed(Keys.Escape))
            {
                EnableCursor();
            }

            if (InputManager.Input.IsPressed(Keys.F))
            {
                _flashlightEnabled = !_flashlightEnabled;
            }

            if (IsCursorHidden())
            {
                UpdateCamera(dt);
            }
        }

        private void UpdateCamera(float dt)
        {
            _camera.Update();

            _cameraPosition = _camera.Position;

            bool moving =
                InputManager.Input.IsDown(Keys.W) ||
                InputManager.Input.IsDown(Keys.A) ||
                InputManager.Input.IsDown(Keys.S) ||
                InputManager.Input.IsDown(Keys.D);

            if (moving)
            {
                _bobTime += dt * BobFrequency;

                _bobAmount = MathF.Min(
                    _bobAmount + dt * 6.0f,
                    1.0f
                );
            }
            else
            {
                _bobAmount = MathF.Max(
                    _bobAmount - dt * 8.0f,
                    0.0f
                );
            }

            float verticalBob =
                MathF.Sin(_bobTime * 2.0f)
                * BobVertical
                * _bobAmount;

            float horizontalBob =
                MathF.Cos(_bobTime)
                * BobHorizontal
                * _bobAmount;

            Vector3 position = _cameraPosition;

            position.Y += verticalBob;
            position.X += horizontalBob;

            _camera.Position = position;
        }

        public void Render()
        {
            _camera.Begin();

            ClearBackground(
                new Color(3, 3, 5, 255)
            );

            DrawRoom();

            if (_flashlightEnabled)
            {
                DrawFlashlight();
            }

            _camera.End();
        }

        private void DrawRoom()
        {
            const float roomWidth = 16.0f;
            const float roomDepth = 24.0f;
            const float wallHeight = 7.0f;
            const float wallThickness = 0.5f;

            Vector3 floorPosition =
                new Vector3(0, -0.25f, 0);

            DrawCube(
                floorPosition,
                roomWidth,
                0.5f,
                roomDepth,
                GetLitColor(
                    floorPosition,
                    _floorColor
                )
            );

            for (int x = -8; x < 8; x += 2)
            {
                for (int z = -12; z < 12; z += 2)
                {
                    DrawCubeWires(
                        new Vector3(
                            x + 1,
                            0.01f,
                            z + 1
                        ),
                        2.0f,
                        0.02f,
                        2.0f,
                        new Color(
                            25,
                            25,
                            28,
                            255
                        )
                    );
                }
            }

            DrawCube(
                new Vector3(
                    0,
                    wallHeight,
                    0
                ),
                roomWidth,
                0.5f,
                roomDepth,
                _ceilingColor
            );

            Vector3 leftWall =
                new Vector3(
                    -roomWidth / 2.0f,
                    wallHeight / 2.0f,
                    0
                );

            DrawCube(
                leftWall,
                wallThickness,
                wallHeight,
                roomDepth,
                GetLitColor(
                    leftWall,
                    _wallColor
                )
            );

            Vector3 rightWall =
                new Vector3(
                    roomWidth / 2.0f,
                    wallHeight / 2.0f,
                    0
                );

            DrawCube(
                rightWall,
                wallThickness,
                wallHeight,
                roomDepth,
                GetLitColor(
                    rightWall,
                    _wallColor
                )
            );

            Vector3 frontWall =
                new Vector3(
                    0,
                    wallHeight / 2.0f,
                    -roomDepth / 2.0f
                );

            DrawCube(
                frontWall,
                roomWidth,
                wallHeight,
                wallThickness,
                GetLitColor(
                    frontWall,
                    _wallColor
                )
            );

            const float doorWidth = 4.0f;
            const float doorHeight = 4.5f;

            float sideWidth =
                (roomWidth - doorWidth) / 2.0f;

            Vector3 backLeft =
                new Vector3(
                    -(doorWidth / 2.0f +
                      sideWidth / 2.0f),
                    wallHeight / 2.0f,
                    roomDepth / 2.0f
                );

            DrawCube(
                backLeft,
                sideWidth,
                wallHeight,
                wallThickness,
                GetLitColor(
                    backLeft,
                    _wallColor
                )
            );

            Vector3 backRight =
                new Vector3(
                    doorWidth / 2.0f +
                    sideWidth / 2.0f,
                    wallHeight / 2.0f,
                    roomDepth / 2.0f
                );

            DrawCube(
                backRight,
                sideWidth,
                wallHeight,
                wallThickness,
                GetLitColor(
                    backRight,
                    _wallColor
                )
            );

            Vector3 aboveDoor =
                new Vector3(
                    0,
                    doorHeight +
                    (wallHeight - doorHeight) / 2.0f,
                    roomDepth / 2.0f
                );

            DrawCube(
                aboveDoor,
                doorWidth,
                wallHeight - doorHeight,
                wallThickness,
                GetLitColor(
                    aboveDoor,
                    _wallColor
                )
            );

            Vector3 doorLeft =
                new Vector3(
                    -doorWidth / 2.0f,
                    doorHeight / 2.0f,
                    roomDepth / 2.0f - 0.35f
                );

            Vector3 doorRight =
                new Vector3(
                    doorWidth / 2.0f,
                    doorHeight / 2.0f,
                    roomDepth / 2.0f - 0.35f
                );

            DrawCube(
                doorLeft,
                0.35f,
                doorHeight,
                0.7f,
                GetLitColor(
                    doorLeft,
                    _detailColor
                )
            );

            DrawCube(
                doorRight,
                0.35f,
                doorHeight,
                0.7f,
                GetLitColor(
                    doorRight,
                    _detailColor
                )
            );

            Vector3 doorTop =
                new Vector3(
                    0,
                    doorHeight,
                    roomDepth / 2.0f - 0.35f
                );

            DrawCube(
                doorTop,
                doorWidth + 0.7f,
                0.35f,
                0.7f,
                GetLitColor(
                    doorTop,
                    _detailColor
                )
            );

            DrawPillar(
                new Vector3(
                    -4.5f,
                    0,
                    -3.0f
                )
            );

            DrawPillar(
                new Vector3(
                    4.5f,
                    0,
                    -3.0f
                )
            );

            DrawPillar(
                new Vector3(
                    -5.5f,
                    0,
                    6.5f
                )
            );

            DrawPillar(
                new Vector3(
                    5.5f,
                    0,
                    6.5f
                )
            );

            Vector3 platform =
                new Vector3(
                    0,
                    0.4f,
                    1.0f
                );

            DrawCube(
                platform,
                5.0f,
                0.8f,
                3.0f,
                GetLitColor(
                    platform,
                    new Color(
                        35,
                        35,
                        38,
                        255
                    )
                )
            );

            DrawCubeWires(
                platform,
                5.0f,
                0.8f,
                3.0f,
                new Color(
                    75,
                    70,
                    65,
                    255
                )
            );

            DrawWallDecoration(
                new Vector3(
                    -7.7f,
                    2.0f,
                    -5.0f
                )
            );

            DrawWallDecoration(
                new Vector3(
                    -7.7f,
                    2.0f,
                    2.0f
                )
            );

            DrawWallDecoration(
                new Vector3(
                    7.7f,
                    2.0f,
                    -5.0f
                )
            );

            DrawWallDecoration(
                new Vector3(
                    7.7f,
                    2.0f,
                    2.0f
                )
            );

            Vector3 ceilingLight =
                new Vector3(
                    0,
                    wallHeight - 0.3f,
                    0
                );

            DrawCube(
                ceilingLight,
                2.5f,
                0.15f,
                0.6f,
                GetLitColor(
                    ceilingLight,
                    new Color(
                        100,
                        80,
                        50,
                        255
                    )
                )
            );

            DrawCube(
                new Vector3(
                    0,
                    wallHeight - 0.15f,
                    0
                ),
                2.0f,
                0.1f,
                0.25f,
                _lightColor
            );

            DrawSphere(
                _lightPosition,
                0.15f,
                _lightColor
            );
        }

        private void DrawPillar(Vector3 position)
        {
            Color main =
                GetLitColor(
                    position,
                    _pillarColor
                );

            Color detail =
                GetLitColor(
                    position,
                    _detailColor
                );

            DrawCube(
                position +
                new Vector3(
                    0,
                    2.5f,
                    0
                ),
                1.2f,
                5.0f,
                1.2f,
                main
            );

            DrawCube(
                position +
                new Vector3(
                    0,
                    0.25f,
                    0
                ),
                1.6f,
                0.5f,
                1.6f,
                detail
            );

            DrawCube(
                position +
                new Vector3(
                    0,
                    5.0f,
                    0
                ),
                1.6f,
                0.5f,
                1.6f,
                detail
            );

            DrawCube(
                position +
                new Vector3(
                    0,
                    2.5f,
                    0.63f
                ),
                0.25f,
                4.0f,
                0.15f,
                detail
            );
        }

        private void DrawWallDecoration(Vector3 position)
        {
            Color metal =
                GetLitColor(
                    position,
                    new Color(
                        45,
                        42,
                        40,
                        255
                    )
                );

            Color red =
                GetLitColor(
                    position,
                    new Color(
                        80,
                        25,
                        20,
                        255
                    )
                );

            DrawCube(
                position,
                0.25f,
                2.0f,
                1.5f,
                metal
            );

            DrawCube(
                position +
                new Vector3(
                    0,
                    0,
                    0.85f
                ),
                0.4f,
                0.8f,
                0.3f,
                red
            );
        }

        private void DrawFlashlight()
        {
            Vector3 position =
                _camera.Position;

            Vector3 forward =
                Vector3.Normalize(
                    _camera.Target -
                    _camera.Position
                );

            Vector3 end =
                position +
                forward *
                FlashlightRange;

            DrawCylinderEx(
                position,
                end,
                0.02f,
                2.5f,
                16,
                new Color(
                    255,
                    245,
                    200,
                    18
                )
            );

            DrawSphere(
                position +
                forward * 0.25f,
                0.07f,
                new Color(
                    255,
                    245,
                    200,
                    255
                )
            );
        }

        private Color GetLitColor(
            Vector3 objectPosition,
            Color baseColor)
        {
            if (!_flashlightEnabled)
                return baseColor;

            Vector3 toObject =
                objectPosition -
                _camera.Position;

            float distance =
                toObject.Length();

            if (distance <= 0.001f)
                return baseColor;

            if (distance > FlashlightRange)
                return baseColor;

            toObject =
                Vector3.Normalize(
                    toObject
                );

            Vector3 forward =
                Vector3.Normalize(
                    _camera.Target -
                    _camera.Position
                );

            float dot =
                Vector3.Dot(
                    forward,
                    toObject
                );

            float cone =
                MathF.Cos(
                    FlashlightAngle *
                    MathF.PI /
                    180.0f
                );

            if (dot < cone)
                return baseColor;

            float distanceFactor =
                1.0f -
                distance /
                FlashlightRange;

            float angleFactor =
                (dot - cone) /
                (1.0f - cone);

            float intensity =
                distanceFactor *
                angleFactor;

            intensity =
                Math.Clamp(
                    intensity,
                    0.0f,
                    1.0f
                );

            byte r =
                (byte)Math.Clamp(
                    (int)(
                        baseColor.R +
                        170.0f *
                        intensity
                    ),
                    0,
                    255
                );

            byte g =
                (byte)Math.Clamp(
                    (int)(
                        baseColor.G +
                        160.0f *
                        intensity
                    ),
                    0,
                    255
                );

            byte b =
                (byte)Math.Clamp(
                    (int)(
                        baseColor.B +
                        130.0f *
                        intensity
                    ),
                    0,
                    255
                );

            return new Color(
                r,
                g,
                b,
                baseColor.A
            );
        }

        public void RenderUI()
        {
            ImGui.SetNextWindowSize(
                new Vector2(
                    340,
                    0
                ),
                ImGuiCond.FirstUseEver
            );

            ImGui.Begin(
                "Fix2Engine Inspector"
            );

            ImGui.Text(
                "F1 - Cursor"
            );

            ImGui.Text(
                "ESC - Release Cursor"
            );

            ImGui.Text(
                "F - Flashlight"
            );

            ImGui.Separator();
            ImGui.Text("CAMERA");
            ImGui.Separator();

            Vector3 camPos =
                _camera.Position;

            if (ImGui.DragFloat3(
                "Position##cam",
                ref camPos,
                0.1f))
            {
                _camera.Position =
                    camPos;

                _cameraPosition =
                    camPos;
            }

            float fov =
                _camera.FOV;

            if (ImGui.SliderFloat(
                "FOV",
                ref fov,
                30.0f,
                120.0f))
            {
                _camera.FOV = fov;
            }

            float moveSpeed =
                _camera.MoveSpeed;

            if (ImGui.SliderFloat(
                "Move Speed",
                ref moveSpeed,
                1.0f,
                60.0f))
            {
                _camera.MoveSpeed =
                    moveSpeed;
            }

            float sensitivity =
                _camera.MouseSensitivity;

            if (ImGui.SliderFloat(
                "Mouse Sensitivity",
                ref sensitivity,
                0.0f,
                1.0f))
            {
                _camera.MouseSensitivity =
                    sensitivity;
            }

            ImGui.Separator();
            ImGui.Text("FLASHLIGHT");
            ImGui.Separator();

            ImGui.Text(
                _flashlightEnabled
                    ? "ON"
                    : "OFF"
            );

            ImGui.Separator();
            ImGui.Text("LIGHT");
            ImGui.Separator();

            ImGui.DragFloat3(
                "Light Position",
                ref _lightPosition,
                0.1f
            );

            ImGui.Separator();
            ImGui.Text("DEBUG");
            ImGui.Separator();

            ImGui.Checkbox(
                "ImGui Demo Window",
                ref _showDemoWindow
            );

            ImGui.End();

            if (_showDemoWindow)
            {
                ImGui.ShowDemoWindow(
                    ref _showDemoWindow
                );
            }
        }

        public void Unload()
        {
        }

        public void Dispose()
        {
        }
    }
}