using System;
using Fix2Engine.Graphics;
using Fix2Engine.Input;

namespace Fix2Engine
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            InputManager.LoadInputMap();
            using var game = new Fix2Engine.Game();
            game.Run();


        }

    }
}
