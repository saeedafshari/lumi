using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Neat;
using Neat.MenuSystem;
using Neat.EasyMenus;
using Neat.GUI;
using Neat.Mathematics;
using Neat.Graphics;

namespace Lumi
{
    public class StencilTestScreen : Screen
    {
        RenderTarget2D target;

        public StencilTestScreen(NeatGame Game)
            : base(Game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            target = new RenderTarget2D(game.GraphicsDevice, game.GameWidth, game.GameHeight,
                false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }

        public override void Activate()
        {
            base.Activate();
        }

        public override void Behave(GameTime gameTime)
        {
            base.Behave(gameTime);
        }

        public override void HandleInput(GameTime gameTime)
        {
            base.HandleInput(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            game.SpriteBatch.End();
            game.PushTarget(target);
            game.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0, 0);

            DepthStencilState dss = new DepthStencilState();
            dss.StencilEnable = true;
            dss.ReferenceStencil = 0;
            dss.StencilFunction = CompareFunction.Equal;
            dss.StencilPass = StencilOperation.Increment;
            game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
                SamplerState.AnisotropicClamp, dss,
                RasterizerState.CullNone);

            game.SpriteBatch.Draw(game.GetTexture("solid"), new Rectangle(100, 100, 100, 100), Color.Black);

            game.SpriteBatch.End();

            dss = new DepthStencilState()
            {
                StencilEnable = true,
                ReferenceStencil = 0,
                StencilFunction = CompareFunction.Equal,
                StencilPass = StencilOperation.Keep
            };
            game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.AnisotropicClamp, dss,
                RasterizerState.CullNone);

            game.SpriteBatch.Draw(game.GetTexture("solid"), new Rectangle(50, 50, 200, 200), Color.Red);
            game.SpriteBatch.Draw(game.GetTexture("solid"), new Rectangle(100, 25, 100, 250), Color.Cyan);
            game.SpriteBatch.End();
            game.PopTarget();

            game.SpriteBatch.Begin();
            game.SpriteBatch.Draw(target, Vector2.Zero, Color.White);

            base.Render(gameTime);
        }
    }
}
