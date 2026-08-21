using System;
using System.Collections.Generic;
using System.Numerics;
using Fix2Engine.Components.Scene;
using Fix2Engine.Input;
using Fix2Engine.Input.InputBackend;
using ImGuiNET;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Fix2Engine;

public class Debug2DPixelScene : IFixScene
{
    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;

    private Vector2 _playerPosition;
    private float _playerRotation;

    private float _playerSpeed = 180.0f;

    private int _score;
    private int _health;

    private bool _gameOver;

    private float _shootCooldown;

    private const float PlayerFireRate = 0.25f;

    private readonly List<Bullet> _bullets = new();
    private readonly List<EnemyTank> _enemies = new();
    private readonly List<Wall> _walls = new();

    private readonly Random _random = new();

    private Vector2 _mouseWorldPosition;

    private struct Bullet
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public bool FromPlayer;
    }

    private struct EnemyTank
    {
        public Vector2 Position;
        public float Rotation;
        public float Speed;
        public float ShootCooldown;
        public bool Alive;
    }

    private struct Wall
    {
        public Rectangle Rectangle;
    }

    public void Start()
    {
        _playerPosition =
            new Vector2(
                ScreenWidth / 2.0f,
                ScreenHeight / 2.0f
            );

        _playerRotation = 0.0f;

        _score = 0;
        _health = 100;

        _gameOver = false;

        _shootCooldown = 0.0f;

        _bullets.Clear();
        _enemies.Clear();
        _walls.Clear();

        CreateMap();

        for (int i = 0; i < 8; i++)
        {
            SpawnEnemy();
        }

        DisableCursor();
    }

    public void Update(float dt)
    {
        // F1:
        // Cursor'u kilitle / serbest bırak.
        if (InputManager.Input.IsPressed(Keys.F1))
        {
            if (IsCursorHidden())
            {
                EnableCursor();
            }
            else
            {
                DisableCursor();
            }
        }

        // ESC:
        // Cursor'u serbest bırak.
        if (InputManager.Input.IsPressed(Keys.Escape))
        {
            EnableCursor();
        }

        // R:
        // Oyunu yeniden başlat.
        if (InputManager.Input.IsPressed(Keys.R))
        {
            Start();
            return;
        }

        if (_gameOver)
            return;

        // Keyboard input cursor durumundan bağımsız.
        UpdatePlayer(dt);

        // Mouse ile ilgili işlemler sadece
        // cursor kilitliyken yapılır.
        if (IsCursorHidden())
        {
            UpdateMouseAim();
            UpdatePlayerShooting(dt);
        }

        UpdateBullets(dt);
        UpdateEnemies(dt);

        CheckEnemySpawn();
    }

    private void UpdateMouseAim()
    {
        _mouseWorldPosition =
            GetMousePosition();

        Vector2 direction =
            _mouseWorldPosition -
            _playerPosition;

        if (direction.LengthSquared() > 0.001f)
        {
            _playerRotation =
                MathF.Atan2(
                    direction.Y,
                    direction.X
                );
        }
    }

    private void UpdatePlayer(float dt)
    {
        Vector2 forward =
            new Vector2(
                MathF.Cos(_playerRotation),
                MathF.Sin(_playerRotation)
            );

        Vector2 movement =
            Vector2.Zero;

        // W = ileri
        if (InputManager.Input.IsDown(Keys.W))
        {
            movement += forward;
        }

        // S = geri
        if (InputManager.Input.IsDown(Keys.S))
        {
            movement -= forward;
        }

        // Sağ vektör
        Vector2 right =
            new Vector2(
                -forward.Y,
                forward.X
            );

        // A = sola
        if (InputManager.Input.IsDown(Keys.A))
        {
            movement -= right;
        }

        // D = sağa
        if (InputManager.Input.IsDown(Keys.D))
        {
            movement += right;
        }

        if (movement.LengthSquared() <= 0.0f)
            return;

        movement =
            Vector2.Normalize(
                movement
            );

        Vector2 newPosition =
            _playerPosition +
            movement *
            _playerSpeed *
            dt;

        if (!IsInsideWall(
            newPosition,
            20.0f))
        {
            _playerPosition =
                newPosition;
        }

        _playerPosition.X =
            Math.Clamp(
                _playerPosition.X,
                30.0f,
                ScreenWidth - 30.0f
            );

        _playerPosition.Y =
            Math.Clamp(
                _playerPosition.Y,
                30.0f,
                ScreenHeight - 30.0f
            );
    }

    private void UpdatePlayerShooting(float dt)
    {
        _shootCooldown -= dt;

        if (!IsCursorHidden())
            return;

        if (!IsMouseButtonDown(
                MouseButton.Left))
        {
            return;
        }

        if (_shootCooldown > 0.0f)
            return;

        ShootPlayer();

        _shootCooldown =
            PlayerFireRate;
    }

    private void ShootPlayer()
    {
        Vector2 direction =
            new Vector2(
                MathF.Cos(_playerRotation),
                MathF.Sin(_playerRotation)
            );

        Bullet bullet =
            new Bullet
            {
                Position =
                    _playerPosition +
                    direction * 35.0f,

                Velocity =
                    direction * 500.0f,

                Life = 2.0f,

                FromPlayer = true
            };

        _bullets.Add(bullet);
    }

    private void UpdateEnemies(float dt)
    {
        for (int i = 0;
             i < _enemies.Count;
             i++)
        {
            EnemyTank enemy =
                _enemies[i];

            if (!enemy.Alive)
                continue;

            Vector2 toPlayer =
                _playerPosition -
                enemy.Position;

            float distance =
                toPlayer.Length();

            Vector2 direction =
                Vector2.Zero;

            if (distance > 0.001f)
            {
                direction =
                    Vector2.Normalize(
                        toPlayer
                    );

                enemy.Rotation =
                    MathF.Atan2(
                        direction.Y,
                        direction.X
                    );
            }

            if (distance > 140.0f)
            {
                Vector2 newPosition =
                    enemy.Position +
                    direction *
                    enemy.Speed *
                    dt;

                if (!IsInsideWall(
                    newPosition,
                    18.0f))
                {
                    enemy.Position =
                        newPosition;
                }
            }

            enemy.ShootCooldown -= dt;

            if (enemy.ShootCooldown <= 0.0f &&
                distance < 600.0f &&
                HasLineOfSight(
                    enemy.Position,
                    _playerPosition))
            {
                ShootEnemy(enemy);

                enemy.ShootCooldown =
                    1.0f +
                    _random.NextSingle() * 1.5f;
            }

            _enemies[i] =
                enemy;
        }
    }

    private void ShootEnemy(
        EnemyTank enemy)
    {
        Vector2 direction =
            new Vector2(
                MathF.Cos(enemy.Rotation),
                MathF.Sin(enemy.Rotation)
            );

        Bullet bullet =
            new Bullet
            {
                Position =
                    enemy.Position +
                    direction * 30.0f,

                Velocity =
                    direction * 280.0f,

                Life = 3.0f,

                FromPlayer = false
            };

        _bullets.Add(bullet);
    }

    private void UpdateBullets(float dt)
    {
        for (int i =
             _bullets.Count - 1;
             i >= 0;
             i--)
        {
            Bullet bullet =
                _bullets[i];

            bullet.Position +=
                bullet.Velocity * dt;

            bullet.Life -= dt;

            bool remove = false;

            if (bullet.Life <= 0.0f)
            {
                remove = true;
            }

            if (bullet.Position.X < 0 ||
                bullet.Position.X > ScreenWidth ||
                bullet.Position.Y < 0 ||
                bullet.Position.Y > ScreenHeight)
            {
                remove = true;
            }

            if (!remove)
            {
                foreach (Wall wall in _walls)
                {
                    if (PointInRectangle(
                        bullet.Position,
                        wall.Rectangle))
                    {
                        remove = true;
                        break;
                    }
                }
            }

            if (!remove)
            {
                if (bullet.FromPlayer)
                {
                    for (int e = 0;
                         e < _enemies.Count;
                         e++)
                    {
                        EnemyTank enemy =
                            _enemies[e];

                        if (!enemy.Alive)
                            continue;

                        float distance =
                            Vector2.Distance(
                                bullet.Position,
                                enemy.Position
                            );

                        if (distance < 22.0f)
                        {
                            enemy.Alive = false;

                            _enemies[e] =
                                enemy;

                            _score += 5;

                            remove = true;

                            break;
                        }
                    }
                }
                else
                {
                    float distance =
                        Vector2.Distance(
                            bullet.Position,
                            _playerPosition
                        );

                    if (distance < 20.0f)
                    {
                        _health -= 10;

                        if (_health <= 0)
                        {
                            _health = 0;
                            _gameOver = true;
                        }

                        remove = true;
                    }
                }
            }

            if (remove)
            {
                _bullets.RemoveAt(i);
            }
            else
            {
                _bullets[i] =
                    bullet;
            }
        }
    }

    private bool HasLineOfSight(
        Vector2 start,
        Vector2 end)
    {
        Vector2 direction =
            end - start;

        float distance =
            direction.Length();

        if (distance <= 0.001f)
            return true;

        direction =
            Vector2.Normalize(
                direction
            );

        const float step = 10.0f;

        for (float current = 0.0f;
             current < distance;
             current += step)
        {
            Vector2 point =
                start +
                direction *
                current;

            foreach (Wall wall in _walls)
            {
                if (PointInRectangle(
                    point,
                    wall.Rectangle))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void CheckEnemySpawn()
    {
        int aliveCount = 0;

        foreach (EnemyTank enemy in _enemies)
        {
            if (enemy.Alive)
                aliveCount++;
        }

        if (aliveCount < 8)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector2 position;

        int attempts = 0;

        do
        {
            position =
                new Vector2(
                    _random.Next(
                        50,
                        ScreenWidth - 50
                    ),
                    _random.Next(
                        50,
                        ScreenHeight - 50
                    )
                );

            attempts++;

            if (attempts > 100)
                return;
        }
        while (
            Vector2.Distance(
                position,
                _playerPosition
            ) < 250.0f
            ||
            IsInsideWall(
                position,
                25.0f)
        );

        EnemyTank enemy =
            new EnemyTank
            {
                Position = position,
                Rotation = 0.0f,
                Speed =
                    45.0f +
                    _random.NextSingle() * 25.0f,
                ShootCooldown =
                    0.5f +
                    _random.NextSingle(),
                Alive = true
            };

        _enemies.Add(enemy);
    }

    private void CreateMap()
    {
        _walls.Add(
            new Wall
            {
                Rectangle =
                    new Rectangle(
                        180,
                        120,
                        250,
                        35
                    )
            }
        );

        _walls.Add(
            new Wall
            {
                Rectangle =
                    new Rectangle(
                        700,
                        120,
                        350,
                        35
                    )
            }
        );

        _walls.Add(
            new Wall
            {
                Rectangle =
                    new Rectangle(
                        150,
                        520,
                        300,
                        35
                    )
            }
        );

        _walls.Add(
            new Wall
            {
                Rectangle =
                    new Rectangle(
                        700,
                        520,
                        350,
                        35
                    )
            }
        );

        _walls.Add(
            new Wall
            {
                Rectangle =
                    new Rectangle(
                        560,
                        200,
                        45,
                        280
                    )
            }
        );

        _walls.Add(
            new Wall
            {
                Rectangle =
                    new Rectangle(
                        250,
                        280,
                        180,
                        40
                    )
            }
        );

        _walls.Add(
            new Wall
            {
                Rectangle =
                    new Rectangle(
                        820,
                        280,
                        180,
                        40
                    )
            }
        );
    }

    private bool IsInsideWall(
        Vector2 position,
        float radius)
    {
        foreach (Wall wall in _walls)
        {
            Rectangle rect =
                wall.Rectangle;

            rect.X -= radius;
            rect.Y -= radius;
            rect.Width += radius * 2.0f;
            rect.Height += radius * 2.0f;

            if (PointInRectangle(
                position,
                rect))
            {
                return true;
            }
        }

        return false;
    }

    private bool PointInRectangle(
        Vector2 point,
        Rectangle rectangle)
    {
        return
            point.X >= rectangle.X &&
            point.X <=
            rectangle.X +
            rectangle.Width &&
            point.Y >= rectangle.Y &&
            point.Y <=
            rectangle.Y +
            rectangle.Height;
    }

    public void Render()
    {
        ClearBackground(
            new Color(
                18,
                20,
                22,
                255
            )
        );

        DrawArena();
        DrawWalls();
        DrawBullets();
        DrawEnemies();
        DrawPlayer();
        DrawCrosshair();
        DrawHUD();

        if (_gameOver)
        {
            DrawGameOver();
        }
    }

    private void DrawArena()
    {
        DrawRectangle(
            0,
            0,
            ScreenWidth,
            ScreenHeight,
            new Color(
                22,
                24,
                27,
                255
            )
        );

        const int gridSize = 40;

        for (int x = 0;
             x < ScreenWidth;
             x += gridSize)
        {
            DrawLine(
                x,
                0,
                x,
                ScreenHeight,
                new Color(
                    30,
                    33,
                    36,
                    255
                )
            );
        }

        for (int y = 0;
             y < ScreenHeight;
             y += gridSize)
        {
            DrawLine(
                0,
                y,
                ScreenWidth,
                y,
                new Color(
                    30,
                    33,
                    36,
                    255
                )
            );
        }

        DrawRectangleLines(
            10,
            10,
            ScreenWidth - 20,
            ScreenHeight - 20,
            Color.Gray
        );
    }

    private void DrawWalls()
    {
        foreach (Wall wall in _walls)
        {
            Rectangle rect =
                wall.Rectangle;

            DrawRectangle(
                (int)rect.X,
                (int)rect.Y,
                (int)rect.Width,
                (int)rect.Height,
                new Color(
                    70,
                    74,
                    78,
                    255
                )
            );

            DrawRectangleLines(
                (int)rect.X,
                (int)rect.Y,
                (int)rect.Width,
                (int)rect.Height,
                new Color(
                    110,
                    115,
                    120,
                    255
                )
            );
        }
    }

    private void DrawPlayer()
    {
        Vector2 forward =
            new Vector2(
                MathF.Cos(_playerRotation),
                MathF.Sin(_playerRotation)
            );

        Vector2 body =
            _playerPosition;

        Vector2 turret =
            body +
            forward * 4.0f;

        Vector2 barrelEnd =
            body +
            forward * 32.0f;

        float rotationDegrees =
            _playerRotation *
            180.0f /
            MathF.PI;

        DrawRectanglePro(
            new Rectangle(
                body.X,
                body.Y,
                42,
                30
            ),
            new Vector2(
                21,
                15
            ),
            rotationDegrees,
            new Color(
                45,
                130,
                65,
                255
            )
        );

        DrawRectanglePro(
            new Rectangle(
                turret.X,
                turret.Y,
                25,
                12
            ),
            new Vector2(
                12,
                6
            ),
            rotationDegrees,
            new Color(
                70,
                165,
                80,
                255
            )
        );

        DrawLineEx(
            turret,
            barrelEnd,
            8.0f,
            new Color(
                80,
                180,
                90,
                255
            )
        );

        DrawCircleV(
            turret,
            8.0f,
            new Color(
                100,
                200,
                100,
                255
            )
        );

        DrawCircleLinesV(
            body,
            23.0f,
            Color.White
        );
    }

    private void DrawEnemies()
    {
        foreach (EnemyTank enemy in _enemies)
        {
            if (!enemy.Alive)
                continue;

            Vector2 forward =
                new Vector2(
                    MathF.Cos(enemy.Rotation),
                    MathF.Sin(enemy.Rotation)
                );

            Vector2 turret =
                enemy.Position +
                forward * 3.0f;

            Vector2 barrel =
                enemy.Position +
                forward * 29.0f;

            float rotationDegrees =
                enemy.Rotation *
                180.0f /
                MathF.PI;

            DrawRectanglePro(
                new Rectangle(
                    enemy.Position.X,
                    enemy.Position.Y,
                    40,
                    28
                ),
                new Vector2(
                    20,
                    14
                ),
                rotationDegrees,
                new Color(
                    145,
                    45,
                    45,
                    255
                )
            );

            DrawRectanglePro(
                new Rectangle(
                    turret.X,
                    turret.Y,
                    24,
                    11
                ),
                new Vector2(
                    12,
                    5.5f
                ),
                rotationDegrees,
                new Color(
                    190,
                    60,
                    60,
                    255
                )
            );

            DrawLineEx(
                turret,
                barrel,
                7.0f,
                new Color(
                    200,
                    70,
                    70,
                    255
                )
            );

            DrawCircleV(
                turret,
                7.0f,
                new Color(
                    220,
                    80,
                    80,
                    255
                )
            );
        }
    }

    private void DrawBullets()
    {
        foreach (Bullet bullet in _bullets)
        {
            if (bullet.Velocity.LengthSquared() <= 0.001f)
                continue;

            Vector2 direction =
                Vector2.Normalize(
                    bullet.Velocity
                );

            Vector2 start =
                bullet.Position -
                direction * 6.0f;

            Color color =
                bullet.FromPlayer
                    ? Color.Yellow
                    : Color.Red;

            DrawLineEx(
                start,
                bullet.Position,
                5.0f,
                color
            );

            DrawCircleV(
                bullet.Position,
                4.0f,
                color
            );
        }
    }

    private void DrawCrosshair()
    {
        if (!IsCursorHidden())
            return;

        Vector2 mouse =
            GetMousePosition();

        DrawCircleLinesV(
            mouse,
            10.0f,
            Color.White
        );

        DrawLine(
            (int)mouse.X - 15,
            (int)mouse.Y,
            (int)mouse.X - 5,
            (int)mouse.Y,
            Color.White
        );

        DrawLine(
            (int)mouse.X + 5,
            (int)mouse.Y,
            (int)mouse.X + 15,
            (int)mouse.Y,
            Color.White
        );

        DrawLine(
            (int)mouse.X,
            (int)mouse.Y - 15,
            (int)mouse.X,
            (int)mouse.Y - 5,
            Color.White
        );

        DrawLine(
            (int)mouse.X,
            (int)mouse.Y + 5,
            (int)mouse.X,
            (int)mouse.Y + 15,
            Color.White
        );
    }

    private void DrawHUD()
    {
        DrawRectangle(
            20,
            20,
            250,
            80,
            new Color(
                0,
                0,
                0,
                160
            )
        );

        DrawText(
            $"SCORE: {_score}",
            35,
            30,
            25,
            Color.White
        );

        DrawText(
            $"HEALTH: {_health}",
            35,
            62,
            20,
            Color.White
        );

        DrawText(
            "WASD: MOVE",
            20,
            ScreenHeight - 60,
            18,
            Color.LightGray
        );

        DrawText(
            "LEFT CLICK: FIRE",
            20,
            ScreenHeight - 35,
            18,
            Color.LightGray
        );
    }

    private void DrawGameOver()
    {
        DrawRectangle(
            0,
            0,
            ScreenWidth,
            ScreenHeight,
            new Color(
                0,
                0,
                0,
                170
            )
        );

        string title =
            "GAME OVER";

        string score =
            $"SCORE: {_score}";

        string restart =
            "PRESS R TO RESTART";

        int titleWidth =
            MeasureText(
                title,
                60
            );

        int scoreWidth =
            MeasureText(
                score,
                30
            );

        int restartWidth =
            MeasureText(
                restart,
                20
            );

        DrawText(
            title,
            ScreenWidth / 2 -
            titleWidth / 2,
            260,
            60,
            Color.Red
        );

        DrawText(
            score,
            ScreenWidth / 2 -
            scoreWidth / 2,
            335,
            30,
            Color.White
        );

        DrawText(
            restart,
            ScreenWidth / 2 -
            restartWidth / 2,
            390,
            20,
            Color.LightGray
        );
    }

    public void RenderUI()
    {
        ImGui.SetNextWindowSize(
            new Vector2(
                300,
                220
            ),
            ImGuiCond.FirstUseEver
        );

        ImGui.Begin(
            "Tank Game"
        );

        ImGui.Text(
            $"Score: {_score}"
        );

        ImGui.Text(
            $"Health: {_health}"
        );

        ImGui.Text(
            $"Enemies: {_enemies.Count}"
        );

        ImGui.Separator();

        ImGui.Text(
            "WASD - Move"
        );

        ImGui.Text(
            "Mouse - Aim"
        );

        ImGui.Text(
            "Left Click - Fire"
        );

        ImGui.Text(
            "F1 - Cursor"
        );

        ImGui.Text(
            "R - Restart"
        );

        ImGui.End();
    }

    public void Unload()
    {
    }

    public void Dispose()
    {
    }
}