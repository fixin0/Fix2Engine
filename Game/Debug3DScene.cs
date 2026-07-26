using System;
using System.Numerics;
using Raylib_cs;
using ImGuiNET;
using Fix2Engine.Graphics;
using Fix2Engine.Components.Scene;
using static Raylib_cs.Raylib;

namespace Fix2Engine
{
    public class Debug3DScene : IFixScene
    {
        private Camera _camera;
        private Skybox _skybox;
        private Model3D _customModel;

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
            _customModel = new Model3D("assets/models/boss.glb")
            {
                Position = new Vector3(0.0f, 0.0f, 0.0f),
                Rotation = new Vector3(90.0f, 0.0f, 0.0f),
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
            ClearBackground(Color.Black);

            _camera.Begin();

            _skybox.Draw(_camera.Position);

            DrawSphere(_lightPosition, 0.3f, _lightColor);
            DrawSphereWires(_lightPosition, 0.35f, 10, 10, Color.Yellow);

            _customModel.Tint = new Color(
                (byte)(_modelColorNormalized.X * 255),
                (byte)(_modelColorNormalized.Y * 255),
                (byte)(_modelColorNormalized.Z * 255),
                (byte)255
            );

            _customModel.Draw();
            DrawGrid(20, 1.0f);

            _camera.End();
        }

        public void RenderUI()
        {
            ImGui.Begin("Fix2Engine Inspector");

            ImGui.Text("Camera & Mouse Control: F1 (Free/Lock)");
            ImGui.Separator();

            if (ImGui.CollapsingHeader("Camera Settings"))
            {
                Vector3 camPos = _camera.Position;
                if (ImGui.DragFloat3("Camera Position", ref camPos, 0.1f)) _camera.Position = camPos;

                float fov = _camera.FOV;
                if (ImGui.SliderFloat("FOV", ref fov, 30.0f, 120.0f)) _camera.FOV = fov;
            }

            if (ImGui.CollapsingHeader("Imported Model Settings", ImGuiTreeNodeFlags.DefaultOpen))
            {
                Vector3 pos = _customModel.Position;
                if (ImGui.DragFloat3("Model Position", ref pos, 0.1f)) _customModel.Position = pos;

                Vector3 rot = _customModel.Rotation;
                if (ImGui.DragFloat3("Rotation (X, Y, Z)", ref rot, 1.0f, -180.0f, 180.0f)) _customModel.Rotation = rot;

                Vector3 scale = _customModel.Scale;
                if (ImGui.DragFloat3("Scale", ref scale, 1.0f, 0.01f, 1000.0f)) _customModel.Scale = scale;

                ImGui.ColorEdit3("Model Color (Tint)", ref _modelColorNormalized);
            }

            if (ImGui.CollapsingHeader("Lighting Settings"))
            {
                ImGui.DragFloat3("Light Position", ref _lightPosition, 0.1f);
            }

            ImGui.Separator();
            ImGui.Checkbox("Show ImGui Demo Window", ref _showDemoWindow);

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