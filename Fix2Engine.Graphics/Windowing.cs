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
        /// Pencere açılmadan hemen önce çalışır. Ilk yapılandırmalar burada yapılır.
        /// </summary>
        protected virtual void Init()
        {
           
        }

        /// <summary>
        /// Oyun döngüsü başlamadan hemen önce, pencere açıldıktan sonra 1 kez çalışır.
        /// </summary>
        protected virtual void Start()
        {
            
        }

        /// <summary>
        /// Kare hızıyla (FPS) senkronize çalışır. Her karede bir kez çağrılır.
        /// </summary>
        /// <param name="dt">Geçen kare süresi (Delta Time)</param>
        protected virtual void Update(float dt)
        {
            
        }

        /// <summary>
        /// Kare hızından bağımsız, SABİT zaman aralıklarında çalışır.
        /// Fizik ve çarpışma (Collision) hesaplamaları için idealdir.
        /// </summary>
        /// <param name="fixedDt">Sabit zaman adımı (Varsayılan 1/60 sn)</param>
        protected virtual void FixedUpdate(float fixedDt)
        {
            
        }

        /// <summary>
        /// Çizimlerin yapıldığı metottur. Her karede çalışır.
        /// </summary>
        protected virtual void Render()
        {
            ClearBackground(Color.Black);

            // Örnek varsayılan çizim
            
        }

        /// <summary>
        /// Tüm yaşam döngüsünü başlatır ve pencere kapanana kadar döngüyü sürdürür.
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