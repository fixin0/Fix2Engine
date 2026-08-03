using System;
using System.Numerics;
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;
using Fix2Engine.IMGUI;
using ImGuiNET;
using Raylib_cs;

namespace Fix2Engine;

public class Debug2DPixelScene : IFixScene
{
    private Sprite2D _sprite2D;

    public void Start()
    {
        _sprite2D = new Sprite2D("C:\\Users\\fixinlab\\RiderProjects\\Fix2Engine\\Game\\bin\\Debug\\net10.0\\assets\\sprites\\ayran.png")
        {
            Position = new Vector2(25.0f, 25.0f),
            Rotation = 0.0f,
            Origin = new Vector2(5.0f, 5.0f),
            Scale = new Vector2(25.0f, 25.0f)
            
            
            
        };
        
        
    }

    public void Update(float dt)
    {
        
    }
    
    public void Render()
    {
        Raylib.ClearBackground(Color.Black);

        _sprite2D.Draw();
        
    }


    public void RenderUI()
    {
        ImGui.SetNextWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);
    }

    public void Unload(){}
    public void Dispose(){}

}