using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Fix2Engine.Graphics
{
    public class Model3D
    {
        private Model _model;
        private Texture2D? _customTexture;

        public Vector3 Position { get; set; } = Vector3.Zero;
        
        public Vector3 Rotation { get; set; } = Vector3.Zero; 
        
        public Vector3 Scale { get; set; } = Vector3.One;
        public Color Tint { get; set; } = Color.White;

        public Model3D(string modelPath)
        {
            _model = LoadModel(modelPath);
        }

        public void SetTexture(string texturePath)
        {
            _customTexture = LoadTexture(texturePath);

            unsafe
            {
                _model.Materials[0].Maps[(int)MaterialMapIndex.Albedo].Texture = _customTexture.Value;
            }
        }

        public void Draw()
        {
           
            Matrix4x4 transform = Matrix4x4.CreateScale(Scale) *
                                  Matrix4x4.CreateRotationX(Rotation.X * DEG2RAD) *
                                  Matrix4x4.CreateRotationY(Rotation.Y * DEG2RAD) *
                                  Matrix4x4.CreateRotationZ(Rotation.Z * DEG2RAD) *
                                  Matrix4x4.CreateTranslation(Position);

            _model.Transform = transform;

            DrawModel(_model, Vector3.Zero, 1.0f, Tint);
        }

        public void Unload()
        {
            if (_customTexture.HasValue)
            {
                UnloadTexture(_customTexture.Value);
            }

            UnloadModel(_model);
        }
    }
}