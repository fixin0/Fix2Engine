using Fix2Engine.Input;
using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Fix2Engine.Graphics
{
    public class Windowing : IDisposable
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Title { get; set; }
        
        private const float FixedDeltaTime = 1.0f / 60.0f; // ~0.01666s
        private float _accumulator = 0.0f;

        public Windowing(int width, int height, string title)
        {
            Width = width;
            Height = height;
            Title = title;
            
            Init();
            InitWindow(Width, Height, Title);
            SetTargetFPS(240);
        }

        /// <summary>
        /// Called before the window is created. Perform initial configuration here.
        /// </summary>
        protected virtual void Init()
        {
           
        }

        /// <summary>
        /// Called once after the window is opened, before the game loop starts.
        /// </summary>
        protected virtual void Start()
        {
            
        }

        /// <summary>
        /// Runs synchronized with the frame rate (FPS). Called once per frame.
        /// </summary>
        /// <param name="dt">Elapsed time since last frame (Delta Time)</param>
        protected virtual void Update(float dt)
        {
            
        }

        /// <summary>
        /// Runs at fixed time intervals, independent of frame rate.
        /// Ideal for physics and collision calculations.
        /// </summary>
        /// <param name="fixedDt">Fixed time step (default 1/60 sec)</param>
        protected virtual void FixedUpdate(float fixedDt)
        {
            
        }

        /// <summary>
        /// Handles rendering. Called every frame.
        /// </summary>
        protected virtual void Render()
        {
            ClearBackground(Color.Black);

            // Example default drawing
            
        }

        /// <summary>
        /// Starts the main lifecycle and runs the loop until the window is closed.
        /// </summary>
        public void Run()
        {
            Start();

            while (!WindowShouldClose())
            {
                float dt = GetFrameTime();

             
                _accumulator += dt;
                while (_accumulator >= FixedDeltaTime)
                {
                    FixedUpdate(FixedDeltaTime);
                    _accumulator -= FixedDeltaTime;
                }

                
                Fix2Engine.Input.InputManager.Update();
                Update(dt);

              
                BeginDrawing();
                Render();
                EndDrawing();
            }

            Dispose();
        }

        public void Dispose()
        {
            CloseWindow();
        }
    }
}