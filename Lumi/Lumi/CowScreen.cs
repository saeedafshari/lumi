#if FALSE
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
using Platformer.Entities;
using Neat.GUI;

namespace Lumi
{
    public partial class CowScreen : Screen
    {
        public PlatformerWorld World;
        public CowScreen(NeatGame game)
            : base(game)
        {
        }
        Label scoreLabel;

        public override void Initialize()
        {
            base.Initialize();
            scoreLabel = Form.NewControl("score", new Label()).ToLabel();
            scoreLabel.Position = new Vector2(10);
            scoreLabel.DrawShadow = true;
            scoreLabel.ForeColor = Color.Red;
            //CreateWorld();
        }

        public override void Activate()
        {
            base.Activate();
            game.GameBackgroundColor = Color.White;
            CreateWorld();
        }
        void CreateWorld()
        {
            World = new PlatformerWorld(game);
            World.Entities.Add(new Cow(new Vector2(100)));
            World.Entities.Add(new Wall(Polygon.BuildRectangle(0, 500, 1000, 300)));
            World.Entities.Add(new Wall(new Polygon(
                new Vector2(1000, 500), new Vector2(1200, 500), new Vector2(1500, 400),
                new Vector2(1700, 500), new Vector2(1700, 800), new Vector2(1000, 800))));
            World.LimitUp = false;

            List<Entity> RopeNodes = new List<Entity>();

            for (int i = 0; i < 10; i++)
            {
                var e = new Entity
                {
                    DrawBorders = true,
                    Body = new Body(Entity.Physics, Entity.Game,
                        //new Polygon(Polygon.BuildCircle(10, new Vector2(300, 150), 32).GetVerticesClockwise()), 10.0f)
                        Polygon.BuildRectangle((i*50) + 550, 0, 32, 32), 10.0f)
                    {
                        IsStatic = false,
                        AttachToGravity = true,
                        MaxSpeed = new Vector2(30)
                    }
                };
                World.Entities.Add(e);
                RopeNodes.Add(e);
                if (i > 0)
                {
                    Rope r = new Rope();
                    r.A = RopeNodes[i - 1].Body;
                    r.B = RopeNodes[i].Body;
                    r.Length = 50;
                    World.Physics.Ropes.Add(r);
                }
            }

            RopeNodes[0].Body.IsStatic = true;
            RopeNodes[0].Body.AttachToGravity = false;
            RopeNodes[0].Body.MaxSpeed = Vector2.Zero;
            RopeNodes[0].Body.Stop();
            RopeNodes[9].Body.IsStatic = true;
            RopeNodes[9].Body.AttachToGravity = false;
            RopeNodes[9].Body.MaxSpeed = Vector2.Zero;
            RopeNodes[9].Body.Stop();

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
            }*/
            //World.Entities.Add(new Elevator(new Vector2(300, 10)));
            /*World.Entities.Add(new ElevatorO(new Vector2(100, 400), 200)
            {
                FlipTime = new TimeSpan(0,0,10)
            });*/
            World.Entities.Add(new Platform(Polygon.BuildRectangle(100, 400, 200, 200)));
            World.Entities.Add(new Item(new Vector2(800, 200))
            {
                Sprite = "checkedbox"
            });

            World.Textures.Add(new PlatformerTexture
            {
                Position = Vector2.Zero,
                Size = new Vector2(1280, 768),
                WrapX = true,
                WrapY = false,
                Depth = -10,
                ScrollRate = Vector2.Zero,
                Sprite = "blue_gradient"
            });

            World.Textures.Add(new PlatformerTexture
            {
                Position = new Vector2(100),
                ScrollRate = new Vector2(0.2f),
                Depth = -5,
                Tint = Color.WhiteSmoke,
                Sprite = "cloud1",
                WrapX = true,
                WrapY = true
            });

            World.Textures.Add(new PlatformerTexture
            {
                Position = new Vector2(400, 150),
                ScrollRate = new Vector2(0.3f),
                Depth = -5,
                Tint = Color.White,
                Sprite = "cloud2",
                WrapX = true,
                WrapY = true
            });

            World.Textures.Add(new PlatformerTexture
            {
                Position = new Vector2(600,300),
                ScrollRate = new Vector2(0.05f),
                Depth = -5,
                Tint = Color.WhiteSmoke,
                Sprite = "cloud3",
                WrapX = true,
                WrapY = true
            });

            World.Textures.Add(new PlatformerTexture
            {
                Position = new Vector2(800,70),
                ScrollRate = new Vector2(0.2f),
                Depth = -5,
                Tint = Color.WhiteSmoke,
                Sprite = "cloud4",
                WrapX = true,
                WrapY = true
            });

            World.Textures.Add(new PlatformerTexture
            {
                Position = new Vector2(300, 100),
                ScrollRate = Vector2.One,
                Depth = -1,
                Tint = Color.White,
                Sprite = "tree"
            });

            World.Physics.Gravity *= 0.5f;
            World.InitializeEntities();
            World.SortTextures();
        }

        public override void Update(GameTime gameTime, bool forceInput=true)
        {
            World.Update(gameTime);
            foreach (var item in World.Entities)
            {
                if (-World.ViewOffset.Y + game.GameWidth < item.Body.Mesh.GetPosition().Y)
                    item.CollectBody = true;
            }
            scoreLabel.Caption = "Score: " + (long)(World.ViewOffset.Y*0.1) + " " + World.Entities.Count;
            base.Update(gameTime, forceInput);
        }

        public override void HandleInput(GameTime gameTime)
        {
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
            if (game.IsPressed(Keys.Up))
            {
                World.Messages.Add(IdGenerator.Jump);
            }
            if (game.IsPressed(Keys.Down))
            {
             
            }
            if (game.IsPressed(Keys.A))
                World.Messages.Add(IdGenerator.OffsetLeft);
            if (game.IsPressed(Keys.D))
                World.Messages.Add(IdGenerator.OffsetRight);
            if (game.IsPressed(Keys.W))
                World.Messages.Add(IdGenerator.OffsetUp);
            if (game.IsPressed(Keys.S))
                World.Messages.Add(IdGenerator.OffsetDown);
            /*if (game.IsTapped(Keys.R))
                CreateWorld();*/

            base.HandleInput(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            World.Draw(gameTime);
            base.Render(gameTime);
        }
    }
}
#endif