using System;
using System.Numerics;
using Raylib_cs;
using ImGuiNET;
using Fix2Engine.Graphics;
using Fix2Engine.Components.Scene;
using Fix2Engine.IMGUI;
using static Raylib_cs.Raylib;

namespace Fix2Engine
{
    public class Debug3DScene : IFixScene
    {
        private Camera _camera;
        private Skybox _skybox;
        private Model3D _sponzaModel;

        private Vector3 _lightPosition = new Vector3(3.0f, 6.0f, -2.0f);
        private Color _lightColor = new Color(255, 240, 200, 255);

        private bool _showDemoWindow = false;
        private Vector3 _modelColorNormalized = new Vector3(1.0f, 1.0f, 1.0f);

        public void Start()
        {
            _camera = new Camera(
                position: new Vector3(0.0f, 2.0f, 15.0f),
                target: Vector3.Zero,
                fov: 75.0f,
                type: CameraType.FirstPerson
                
            );

            DisableCursor();

            _skybox = new Skybox("assets/skybox.png");
            _sponzaModel = new Model3D("assets/models/boss.glb")
            {
                Position = new Vector3(0.0f, 0.0f, 0.0f),
                Rotation = new Vector3(0.0f, 0.0f, 0.0f),
                Scale = new Vector3(100.0f, 100.0f, 100.0f)
            };

        }

        public void Update(float dt)
        {
            if (IsKeyPressed(KeyboardKey.F1))
            {
                if (IsCursorHidden()) EnableCursor();
                else DisableCursor();
            }

            if (IsCursorHidden())
            {
                _camera.Update();
            }

            if (IsKeyPressed(KeyboardKey.Escape)) EnableCursor();
        }

        public void Render()
        {
            

            _camera.Begin();

            _skybox.Draw(_camera.Position);

            DrawSphere(_lightPosition, 0.3f, _lightColor);
            DrawSphereWires(_lightPosition, 0.35f, 10, 10, Color.Yellow);

            _sponzaModel.Tint = new Color(
                (byte)(_modelColorNormalized.X * 255),
                (byte)(_modelColorNormalized.Y * 255),
                (byte)(_modelColorNormalized.Z * 255),
                (byte)255
            );

            _sponzaModel.Draw();
            //DrawGrid(20, 1.0f);

            _camera.End();
        }

        public void RenderUI()
        {
            ImGui.SetNextWindowSize(new Vector2(340, 0), ImGuiCond.FirstUseEver);
            ImGui.Begin("Fix2Engine Inspector");

            ImGui.TextColored(Palette.TextMuted, "Kamera & Fare Kontrolü: F1 (Serbest/Kilit)");

            Widgets.SeparatorWithLabel("KAMERA");
            Vector3 camPos = _camera.Position;
            if (ImGui.DragFloat3("Konum##cam", ref camPos, 0.1f)) _camera.Position = camPos;

            float fov = _camera.FOV;
            if (Widgets.SliderFloat("FOV", ref fov, 30.0f, 120.0f)) _camera.FOV = fov;

            float moveSpeed = _camera.MoveSpeed;
            if (Widgets.SliderFloat("Hareket Hızı", ref moveSpeed, 20.0f, 60.0f)) _camera.MoveSpeed = moveSpeed;

            float mouseSensitivity = _camera.MouseSensitivity;
            if (Widgets.SliderFloat("Fare Hassasiyeti", ref mouseSensitivity, 0.0f, 1.0f)) _camera.MouseSensitivity = mouseSensitivity;

            Widgets.SeparatorWithLabel("MODEL");
            Vector3 pos = _sponzaModel.Position;
            if (ImGui.DragFloat3("Konum##model", ref pos, 0.1f)) _sponzaModel.Position = pos;

            Vector3 rot = _sponzaModel.Rotation;
            if (ImGui.DragFloat3("Rotasyon (X, Y, Z)", ref rot, 1.0f, -180.0f, 180.0f)) _sponzaModel.Rotation = rot;

            Vector3 scale = _sponzaModel.Scale;
            if (ImGui.DragFloat3("Ölçek", ref scale, 1.0f, 0.01f, 1000.0f)) _sponzaModel.Scale = scale;

            ImGui.ColorEdit3("Renk (Tint)", ref _modelColorNormalized);

            Widgets.SeparatorWithLabel("IŞIKLANDIRMA");
            ImGui.DragFloat3("Işık Konumu", ref _lightPosition, 0.1f);

            Widgets.SeparatorWithLabel("DEBUG");
            Widgets.Toggle("ImGui Demo Penceresi", ref _showDemoWindow);

            ImGui.End();

            if (_showDemoWindow)
            {
                ImGui.ShowDemoWindow(ref _showDemoWindow);
            }
        }

        public void Unload()
        {
            
        }

        public void Dispose()
        {
            // VRAM Temizliği
        }
    }
}