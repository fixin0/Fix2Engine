using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Fix2Engine.Graphics
{

    public enum MaterialMaps
    {
        Albedo,
        Normal,
        Metalness,
        Roughness,
        Emission,
        Irradiance,
        Height
    }
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
        
        

        public void SetTexture(string texturePath, MaterialMaps  materialMap = MaterialMaps.Albedo)
        {
            _customTexture = LoadTexture(texturePath);

            unsafe
            {
                if (_model.MaterialCount <= 0)
                {
                    Console.WriteLine("Model has no materials, cannot assign texture");
                    return;
                }

                switch (materialMap)
                {
                    case MaterialMaps.Albedo:
                        _model.Materials[0].Maps[(int)MaterialMapIndex.Albedo].Texture = _customTexture.Value;
                        break;
                    case MaterialMaps.Normal:
                        _model.Materials[0].Maps[(int)MaterialMapIndex.Normal].Texture = _customTexture.Value;
                        break;
                    case MaterialMaps.Metalness:
                        _model.Materials[0].Maps[(int)MaterialMapIndex.Metalness].Texture = _customTexture.Value;
                        break;
                    case  MaterialMaps.Roughness:
                        _model.Materials[0].Maps[(int)MaterialMapIndex.Roughness].Texture = _customTexture.Value;
                        break;
                    case MaterialMaps.Emission:
                        _model.Materials[0].Maps[(int)MaterialMapIndex.Emission].Texture = _customTexture.Value;
                        break;
                    case MaterialMaps.Height:
                        _model.Materials[0].Maps[(int)MaterialMapIndex.Height].Texture = _customTexture.Value;
                        break;
                    case MaterialMaps.Irradiance:
                        _model.Materials[0].Maps[(int)MaterialMapIndex.Irradiance].Texture = _customTexture.Value;
                        break;
                    default:
                        Console.WriteLine("NOT TEXTURE FOR MODEL");
                        break;

                }
               
            }
        }

        public List<string> GetMaterialMaps()
        {
            List<string> _maps = new List<string>();
            for (int i = 0; i < _model.MaterialCount; i++)
            {
                unsafe
                {
                    _maps.Add(_model.Materials[i].Maps[i].ToString());
                }
            }

            return _maps;
        }

        public int GetMaterialCount()
        {
            return _model.MaterialCount;
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