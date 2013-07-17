using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Neat;
using System.Diagnostics;

namespace Lumi
{
    public class PlatformerEngine : NeatGame
    {
        public TimeSpan Time;

        public PlatformerEngine(string[] args = null, GraphicsDevice _device = null, ContentManager _content = null)
            : base(args, _device, _content)
        {
            GameWidth = 1920;
            GameHeight = 1080;
            GameBackgroundColor = Color.Black;
            FullScreen = false;
            Debug.WriteLine("PlatformerEngine Created.");

            if (args.Contains("-editor"))
            {
                Console.BackColor.A = 127;
            }

            IsFixedTimeStep = false;
        }

        public void Draw()
        {
            Draw(gamesTime);
        }

        protected override void Update(GameTime gameTime)
        {
            gamesTime = gameTime;
            Time = gameTime.TotalGameTime;
            base.Update(gameTime);
        }

        public PlatformerEngine()
        {
            GameWidth = 1920;
            GameHeight = 1080;
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);
            Freezed = false;
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);
            Freezed = true;
        }

        protected override void LoadContent()
        {
            LoadEffect(@"Effects\Shadow", "shadow");
            LoadEffect(@"Effects\TexLighting", "lighting");

            LoadTexture(@"Sprites\standing-walking");
            CreateSprite("standing", "standing-walking", new Rectangle(0, 0, 212, 512));
            CreateSprite("standing-blinking", "standing-walking", new Vector2(212, 512), 10,
                new Vector2(0, 0),
                new Vector2(224, 0),
                new Vector2(451, 0),
                new Vector2(676, 0));
            CreateSprite("walking", "standing-walking", new Vector2(212, 512), 8,
                new Vector2(0, 512),
                new Vector2(224, 512),
                new Vector2(451, 512),
                new Vector2(676, 512));

            LoadTexture(@"Sprites\editor");
            CreateSprite("cross16x1", "editor", new Rectangle(0, 0, 16, 16));
            CreateSprite("cross16x2", "editor", new Rectangle(0, 17, 16, 16));
            CreateSprite("cross32", "editor", new Rectangle(17, 0, 32, 32));
            CreateSprite("crossx", "editor", new Rectangle(0, 34, 16, 16));
            CreateSprite("crosso", "editor", new Rectangle(17, 34, 16, 16));

            LoadTexture(@"Sprites\sheet1");
            CreateSprite("elevator_orange", "sheet1", new Vector2(100, 24), 30, Vector2.Zero, new Vector2(100,0));
            CreateSprite("elevator_yellow", "sheet1", new Vector2(100, 24), 30, new Vector2(0, 24), new Vector2(100, 24));
            CreateSprite("elevator_blue", "sheet1", new Vector2(100, 24), 30, new Vector2(0, 24*2), new Vector2(100, 24 * 2));

            CreateSprite("switch_ud_off", "sheet1", new Rectangle(613, 546, 73, 99));
            CreateSprite("switch_ud_on", "sheet1", new Rectangle(691, 547, 73, 99));

            CreateSprite("strawberry", "sheet1", new Rectangle(1, 76, 78, 98));
            CreateSprite("banana", "sheet1", new Rectangle(100, 76, 80, 98));
            CreateSprite("cherry", "sheet1", new Rectangle(5, 183, 74, 92));
            CreateSprite("lollipop", "sheet1", new Rectangle(792, 551, 49, 73));
            CreateSprite("stick", "sheet1", new Rectangle(885, 574, 56, 76));
            CreateSprite("flash", "sheet1", new Rectangle(973, 574, 35, 76));
            CreateSprite("tablet", "sheet1", new Rectangle(8, 757, 115, 76));
            CreateSprite("battery", "sheet1", new Rectangle(135, 754, 50, 83));

            CreateSprite("sign_goup", "sheet1", new Rectangle(226, 2, 117, 81));
            CreateSprite("sign_danger", "sheet1", new Rectangle(226, 85, 173, 89));
            CreateSprite("sign_exit", "sheet1", new Rectangle(226, 178, 137, 41));
            CreateSprite("sign_exit2", "sheet1", new Rectangle(245, 223, 103, 54));
            CreateSprite("sign_arrowup", "sheet1", new Rectangle(852, 303, 64, 65));

            CreateSprite("door_exit", "sheet1", new Rectangle(413, 9, 207, 257));


            CreateSprite("blue_monster", "sheet1", new Vector2(73, 96), 5,
                new Vector2(626, 1),
                new Vector2(703, 1),
                new Vector2(780, 1),
                new Vector2(857, 1),
                new Vector2(934, 1),
                new Vector2(626, 99),
                new Vector2(703, 99),
                new Vector2(780, 99),
                new Vector2(857, 99),
                new Vector2(934, 99));

            CreateSprite("white_bird", "sheet1", new Vector2(98, 90), 5,
                new Vector2(626, 199),
                new Vector2(731, 199),
                new Vector2(835, 200),
                new Vector2(627, 293),
                new Vector2(733, 293));

            CreateSprite("eye_monster", "sheet1", new Vector2(92, 80), 5,
                new Vector2(2, 282),
                new Vector2(97, 282),
                new Vector2(192, 282),
                new Vector2(288, 284),
                new Vector2(383, 284));

            CreateSprite("monster_yellowworm", "sheet1", new Vector2(124, 82), 5,
                new Vector2(8,484),
                new Vector2(151,484),
                new Vector2(286, 486),
                new Vector2(425, 486));
            SetAnimationMode("monster_yellowworm", Sprite.AnimationModes.PingPong);

            CreateSprite("monster_roach", "sheet1", new Vector2(101, 78), 5,
                new Vector2(8, 583),
                new Vector2(120, 583),
                new Vector2(232, 583),
                new Vector2(342, 583),
                new Vector2(455, 583));
            SetAnimationMode("monster_roach", Sprite.AnimationModes.PingPong);

            CreateSprite("monster_sticky", "sheet1", new Vector2(60, 78), 5,
                new Vector2(384, 667),
                new Vector2(455, 668),
                new Vector2(527, 667),
                new Vector2(600, 667));
            //SetAnimationMode("monster_sticky", Sprite.AnimationModes.PingPong);

            CreateSprite("monster_stickyu", "sheet1", new Vector2(78, 60), 5,
                new Vector2(673, 671),
                new Vector2(758, 671),
                new Vector2(839, 671),
                new Vector2(921, 671));

            CreateSprite("hazard_fencecolor", "sheet1", new Rectangle(9, 678, 159, 67));
            CreateSprite("hazard_fencegray", "sheet1", new Rectangle(198, 679, 159, 67));

            CreateSprite("projectile", "sheet1", new Vector2(44, 19), 5,
                new Vector2(490, 284),
                new Vector2(490, 306),
                new Vector2(491, 328),
                new Vector2(490, 348));

            CreateSprite("shadow_brick", "sheet1", new Rectangle(949, 358, 64, 64));
            CreateSprite("shadow_brick2", "sheet1", new Rectangle(949, 213, 64, 64));
            CreateSprite("brick", "sheet1", new Rectangle(949, 284, 64, 64));
            CreateSprite("round_brick", "sheet1", new Rectangle(556, 297, 64, 64));

            CreateSprite("lamp_down", "sheet1", new Rectangle(5, 383, 86, 55));
            CreateSprite("lamp_up", "sheet1", new Rectangle(256, 384, 86, 55));
            CreateSprite("lamp_right", "sheet1", new Rectangle(107, 384, 55, 86));
            CreateSprite("lamp_left", "sheet1", new Rectangle(185, 384, 55, 86));

            CreateSprite("platform_orange", "sheet1", new Rectangle(373, 379, 183, 44));
            CreateSprite("platform_gray", "sheet1", new Rectangle(375, 422, 181, 40));

            CreateSprite("tile_stripes", "sheet1", new Rectangle(585, 398, 128, 128));
            CreateSprite("tile_star_white", "sheet1", new Rectangle(743, 398, 128, 128));

            base.LoadContent();
        }

        protected override void Initialize()
        {
            base.Initialize();
            Console.AddCommand("g_start", g_start);
            ShowMouse = true;

            ElegantTextEngine.Position.X -= 250;
            ElegantTextEngine.MaxLines = 5;
        }

        public override void InitializeGraphics()
        {
            base.InitializeGraphics();
        }
        public override void FirstTime()
        {
            base.FirstTime();
            Console.Run("c_texture transparent");
            Console.Run("c_backcolor 0.3,0.3,0.3,0.7");
            Console.Run("g_start");
        }

        void g_start(IList<string> args)
        {
            Console.Run("sh game");
        }

        public override void AddScreens()
        {
            Screens["game"] = new PlatformerScreen(this);
            Screens["stencil"] = new StencilTestScreen(this);
            base.AddScreens();
        }

        TimeSpan secondCounter = TimeSpan.Zero;
        int fpsCounter = 0;
        int lastFps = 0;

        protected override void Draw(GameTime gameTime)
        {
            secondCounter += gameTime.ElapsedGameTime;
            fpsCounter++;
            if (secondCounter.TotalSeconds > 1)
            {
                secondCounter = TimeSpan.Zero;
                lastFps = fpsCounter;
                fpsCounter = 0;
                Window.Title = "FPS: " + lastFps;
            }

            base.Draw(gameTime);
        }

        protected override void Render(GameTime gameTime)
        {
            base.Render(gameTime);

            if (FullScreen)
                Write(Window.Title, new Vector2(GameWidth / 2.0f, 10));
        }
    }
}
