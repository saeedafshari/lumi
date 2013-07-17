using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neat.Mathematics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;
using Neat.Graphics;
using Console = Neat.Components.Console;

namespace Lumi
{
    public partial class PlatformerScreen : Screen
    {
        public PlatformerWorld World;
        public PlatformerScreen(NeatGame game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            Form.HasMouse = false;
            //CreateWorld();

            game.Console.AddCommand("w_load", o =>
                {
                    World = PlatformerWorld.Load(game.Console.Args2Str(o, 1));
                    World.Game = game;
                }, game.Console.AutocompleteFiles);

            game.Console.AddCommand("w_save", o =>
                {
                    World.Save(game.Console.Args2Str(o, 1));
                }, game.Console.AutocompleteFiles);

            game.Console.AddCommand("w_new", o =>
                {
                    CreateWorld();
                });
        }

        public override void GraphicsReinitialized()
        {
            base.GraphicsReinitialized();
            if (World != null) World.InitRender();
        }

        bool lastMouse;
        public override void Activate()
        {
            base.Activate();
            CreateWorld();
            lastMouse = game.ShowMouse;
            game.ShowMouse = false;
        }

        public override void Deactivate(string nextScreen)
        {
            game.ShowMouse = lastMouse;
            base.Deactivate(nextScreen);
        }
        Light lt;
        void CreateWorld()
        {
            World = new PlatformerWorld(game);
            //Light.GlobalAmbient = Color.Gray;
            /*World.LimitDown = false;
            World.LimitLeft = false;
            World.LimitUp = false;
            World.LimitRight = false;*/
            World.LimitDown = true;

            World.Entities.Add(new Player(new Vector2(100, 1800))
            {
                InfiniteJump = false,
                DrawNoLight = true,
                DropShadow = false
            });

            World.Entities.Add(new Wall(Polygon.BuildRectangle(0, 0, 25, 2025)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(0, 2000, 7000, 50)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(7000-25, 0, 25, 2025)));

            World.Lights.Add(lt = new Light
            {
                Position = new Vector2(500, 1500),
                Target = new Vector2(500, 2000),
                Diffuse = Color.White,
                Angle = MathHelper.ToRadians(30),
                Falloff = 10f,
                Intensity = 1.5f,
                Enabled = true
            });
            /*
            World.Entities.Add(new Wall(Polygon.BuildRectangle(2000, 2000 - 100, 1000, 100)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(2200, 2000 - 200, 800, 100)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(2400, 2000 - 300, 600, 100)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(2600, 2000 - 400, 400, 100)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(2800, 2000 - 500, 200, 100)));
            */
            World.Lights.Add(lt = new Light
            {
                Position = new Vector2(2100, 1000),
                Target = new Vector2(2100, 2000),
                Diffuse = Color.White,
                Angle = MathHelper.ToRadians(80),
                Falloff = 1.2f,
                Intensity = 1f,
                Range = 2f,
                Enabled = true
            });
            /*
            World.Entities.Add(new Wall(Polygon.BuildRectangle(3000, 2000 - 600, 1000, 100)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(3200, 2000 - 300, 1000, 50)));
            */
            /*World.Entities.Add(new Wall(Polygon.BuildRectangle(0, 500, 600, 300)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(800, 500, 100, 50)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(1000, 500, 100, 50)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(1200, 500, 100, 50)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(1400, 500, 300, 50)));
            
            World.Entities.Add(new Wall(Polygon.BuildRectangle(800, 700, 600, 50)));*/
            //World.Entities.Add(new Wall(Polygon.BuildCircle(32, new Vector2(1000,300), 50)));
            //World.Entities.Add(new Wall(Polygon.BuildRectangle(1400, 500, 300, 50)));
            /*World.Entities.Add(new Wall(new Polygon(
                new Vector2(1000, 500), new Vector2(1200, 500), new Vector2(1500, 400),
                new Vector2(1700, 500), new Vector2(1700, 800), new Vector2(1000, 800))));
            */
            /*for (int i = 0; i < 20; i++)
            {
                World.Entities.Add(new Entity
                {
                    DrawBorders = true,
                    Body = new Body(Entity.Physics, Entity.Game,
                        //new Polygon(Polygon.BuildCircle(10, new Vector2(300, 150), 32).GetVerticesClockwise()), 10.0f)
                        Polygon.BuildRectangle((i%5) * 48 + 500, (i/5) * 48 + 150, 64, 64), 10.0f)
                    {
                        IsStatic = false,
                        AttachToGravity = true,
                        MaxSpeed = new Vector2(10)
                    }
                });
                World.Entities[i].Body.Entity = World.Entities[i];
            }
            World.Entities.Add(new ElevatorH(new Vector2(300, 400)) { FlipTime = new TimeSpan(0, 0, 15) });
            World.Entities.Add(new ElevatorO(new Vector2(200, 400), 200)
            {
                FlipTime = new TimeSpan(0,0,10)
            });
            World.Entities.Add(new Platform(Polygon.BuildRectangle(100, 400, 200, 200)));
            */
            /*World.Textures.Add(new PlatformerTexture
            {
                Sprite = "solid",
                Tint = Color.White,
                Position = Vector2.Zero,
                Size = new Vector2(4096),
                WrapX = true,
                WrapY = true,
                Depth = -10,
                ScrollRate = Vector2.One
            });*/
            
            World.InitializeEntities();
            World.SortTextures();
        }

        public override void Update(GameTime gameTime, bool forceInput=true)
        {
            World.Update(gameTime);
            base.Update(gameTime, forceInput);
        }

        public override void HandleInput(GameTime gameTime)
        {
            /*if (game.IsPressed(Keys.B)) lt.Falloff += 0.1f;
            if (game.IsPressed(Keys.V)) lt.Falloff -= 0.1f;
            if (game.IsPressed(Keys.D6)) lt.Range += 0.1f;
            if (game.IsPressed(Keys.D5)) lt.Range -= 0.1f;
            if (game.IsPressed(Keys.Y)) lt.Intensity += 0.1f;
            if (game.IsPressed(Keys.T)) lt.Intensity -= 0.1f;
            if (game.IsPressed(Keys.H)) lt.AngleDeg += 1f;
            if (game.IsPressed(Keys.G)) lt.AngleDeg -= 1f;*/
            if (game.IsMousePressed()) World.Messages.Add(IdGenerator.CaptureMouseLeft);
            if (game.IsRightMousePressed()) World.Messages.Add(IdGenerator.CaptureMouseRight);
            if (game.IsPressed(Keys.Left))
            {
                World.Messages.Add(IdGenerator.MoveLeft);
            }
            if (game.IsPressed(Keys.Right))
            {
                World.Messages.Add(IdGenerator.MoveRight);
            }
            if (game.IsPressed(Keys.LeftControl, Keys.RightControl))
            {
                World.Messages.Add(IdGenerator.Jump);
            }
            if (game.IsPressed(Keys.Down))
            {
             
            }
            if (game.IsTapped(Keys.F12))
            {
                World.Editor.Enabled = !World.Editor.Enabled;
            }
            if (game.IsTapped(Keys.Space))
            {
                World.Messages.Add(IdGenerator.Shoot);
            }
            //if (game.IsPressed(Keys.LeftAlt, Keys.RightAlt) && game.IsTapped(Keys.R))
                //CreateWorld();
            
            base.HandleInput(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            World.Draw(gameTime);
            //game.Write((gameTime.ElapsedGameTime.Milliseconds).ToString() + " fps", new Vector2(game.GameWidth - 100, 10));
            base.Render(gameTime);
        }
    }
}
