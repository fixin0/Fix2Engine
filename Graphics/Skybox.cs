using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Rlgl;

namespace Fix2Engine.Graphics
{
    public class Skybox
    {
        private Model _skyboxModel;
        private Texture2D _cubemapTexture;
        private Shader _skyboxShader;

        // ==========================================
        // (EMBEDDED SHADERS)
        // ==========================================
        private const string VertexShaderCode = @"#version 330
        in vec3 vertexPosition;
        out vec3 fragPosition;
        uniform mat4 mvp;
        void main() {
            fragPosition = vertexPosition;
            gl_Position = mvp * vec4(vertexPosition, 1.0);
        }";

        private const string FragmentShaderCode = @"#version 330
        in vec3 fragPosition;
        out vec4 finalColor;
        uniform samplerCube environmentMap;
        void main() {
            finalColor = texture(environmentMap, fragPosition);
        }";
        // ==========================================

        public Skybox(string texturePath)
        {
            Mesh cubeMesh = GenMeshCube(1.0f, 1.0f, 1.0f);
            _skyboxModel = LoadModelFromMesh(cubeMesh);

            Image img = LoadImage(texturePath);
            _cubemapTexture = LoadTextureCubemap(img, CubemapLayout.AutoDetect);
            UnloadImage(img);

            
            _skyboxShader = LoadShaderFromMemory(VertexShaderCode, FragmentShaderCode);

            int envMapLoc = GetShaderLocation(_skyboxShader, "environmentMap");
            int param = (int)MaterialMapIndex.Cubemap;
            SetShaderValue(_skyboxShader, envMapLoc, param, ShaderUniformDataType.Int);

            unsafe
            {
                _skyboxModel.Materials[0].Shader = _skyboxShader;
                _skyboxModel.Materials[0].Maps[(int)MaterialMapIndex.Cubemap].Texture = _cubemapTexture;
            }
        }

        public void Draw(Vector3 cameraPosition, float radius = 5000.0f)
        {
            Rlgl.DisableDepthTest();
            Rlgl.DisableBackfaceCulling();

            DrawModel(_skyboxModel, cameraPosition, radius, Color.White);

            Rlgl.EnableBackfaceCulling();
            Rlgl.EnableDepthTest();
        }

        public void Unload()
        {
            UnloadShader(_skyboxShader);
            UnloadTexture(_cubemapTexture);
            UnloadModel(_skyboxModel);
        }
    }
}