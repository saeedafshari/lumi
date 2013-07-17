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

namespace Lumi
{
    public partial class PlatformerWorld
    {
        bool prevWrap = false;
        
        [NonSerialized] Effect 
            wrapEffect,
            shadeEffect,
            lightEffect;

        [NonSerialized] RenderTarget2D 
            stencilTarget, 
            texTarget,
            lightMap;

        public bool UseLights = true;

        public void InitRender()
        {
            Debug.WriteLine("World.InitRender()");
            lightMap = new RenderTarget2D(Game.GraphicsDevice,
                Game.GameWidth,
                Game.GameHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                1,
                RenderTargetUsage.PreserveContents);
            texTarget = new RenderTarget2D(
                Game.GraphicsDevice,
                Game.GameWidth,
                Game.GameHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                1,
                RenderTargetUsage.DiscardContents);
            stencilTarget = new RenderTarget2D(Game.GraphicsDevice,
                Game.GameWidth,
                Game.GameHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8,
                1,
                RenderTargetUsage.DiscardContents);
            
            Light.ViewPortSize = Game.GameSize;
        }

        public void DrawLight(Light light)
        {
            //Before calling, textures are drawn normally on texTarget
            if (!light.Enabled) return;
            Vector2 gameSize = Game.GameSize;
            
            //Light Visible Test:
            ///See if light circle intersects with camera
            ///Circle-Rect collision test
            ///if not colliding, exit func.
            Rectangle viewPort = GeometryHelper.Vectors2Rectangle(ViewOffset, gameSize);
            float lightRadius = Vector2.Distance(light.Position, light.Target) * light.Range;

            //if (!GeometryHelper.RectCollidesWithCircle(light.Position, lightRadius, viewPort))
                //return;
            
            Vector2 lightPos = light.Position + ViewOffset;
            Vector2 targetPos = light.Target + ViewOffset;
            
            Vector2 lightPosNormal = lightPos / gameSize;
            Vector2 targetPosNormal = targetPos / gameSize;
            
            //Push Target --> stencilTarget
            Game.PushTarget(stencilTarget);

            //Clear Stencil
            Game.GraphicsDevice.Clear(Color.Black);
            //Game.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0, 0);

            //Set Stencil To 0, Increment, Begin
            Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp,
                new DepthStencilState
                {
                    StencilEnable = true,
                    ReferenceStencil = 0,
                    StencilFunction = CompareFunction.LessEqual,
                    StencilPass = StencilOperation.IncrementSaturation
                },
                RasterizerState.CullNone,
                shadeEffect = Game.GetEffect("Shadow"));
            shadeEffect.Parameters["lightPos"].SetValue(lightPosNormal);

            //Draw Items As Shadows On Stencil
            foreach (var item in Entities)
            {
                if (!item.DropShadow) continue;
                if (item.Body.Mesh.IsRectangle())
                {
                    Rectangle aabb = item.Body.Mesh.GetBoundingRect();
                    //If light source is inside the entity, don't shade
                    if (aabb.Contains((int)light.Position.X, (int)light.Position.Y)) continue;

                    //Is inside the light?
                    LineSegment ab = new LineSegment(aabb.Left, aabb.Top, aabb.Right, aabb.Top);
                    LineSegment cd = new LineSegment(aabb.Left, aabb.Bottom, aabb.Right, aabb.Bottom);
                    LineSegment ac = new LineSegment(aabb.Left, aabb.Top, aabb.Left, aabb.Bottom);
                    LineSegment bd = new LineSegment(aabb.Right, aabb.Top, aabb.Right, aabb.Bottom);
                    float rab = ab.Distance(light.Position);
                    float rcd = cd.Distance(light.Position);
                    float rac = ac.Distance(light.Position);
                    float rbd = bd.Distance(light.Position);

                    if (rab <= lightRadius || rcd <= lightRadius || rac <= lightRadius || rbd <= lightRadius)
                    {
                        //Find lit edges
                        var edges = new Dictionary<LineSegment, float>{
                            { ab, rab },
                            { cd, rcd },
                            { ac, rac },
                            { bd, rbd }
                        }.OrderBy(o => o.Value).ToArray();

                        Vector2[] vertices = new Vector2[]
                        {
                            (edges[0].Key.StartPos + ViewOffset) / gameSize,
                            (edges[0].Key.EndPos + ViewOffset) / gameSize,
                            (edges[1].Key.StartPos + ViewOffset) / gameSize,
                            (edges[1].Key.EndPos + ViewOffset) / gameSize
                        };
                        //shadeEffect.Parameters["strip"].SetValue(false);
                        shadeEffect.Parameters["vertices"].SetValue(vertices);
                        //shadeEffect.Parameters["strip"].SetValue(true);
                        //shadeEffect.Parameters["vertices"].SetValue(vertices);
                        //shadeEffect.Parameters["vCount"].SetValue(3);
                        Game.SpriteBatch.Draw(texTarget, Vector2.Zero, Color.Black);
                    }
                }
                else //if not rectangle
                {
                    if (item.Body.Mesh.IsInside(light.Position)) continue;

                    var edges = item.Body.Mesh.GetEdges();
                    Vector2[] vertices = new Vector2[4];
                    int c = 0;
                    int b = 0;
                    while (c <= edges.Count)
                    {
                        if (c == edges.Count)
                        {
                            if (b == 2)
                            {
                                vertices[b] = vertices[b - 2];
                                vertices[b + 1] = vertices[b - 1];
                                shadeEffect.Parameters["vertices"].SetValue(vertices);
                                Game.SpriteBatch.Draw(texTarget, Vector2.Zero, Color.Black);
                            }
                            break;

                        }
                        else
                        {
                            vertices[b++] = (edges[c].StartPos + ViewOffset) / gameSize;
                            vertices[b++] = (edges[c].EndPos + ViewOffset) / gameSize;
                        }

                        if (b == 4)
                        {
                            b = 0;
                            shadeEffect.Parameters["vertices"].SetValue(vertices);
                            Game.SpriteBatch.Draw(texTarget, Vector2.Zero, Color.Black);
                        }
                        c++;
                    }
                }
            }

            Game.SpriteBatch.End();

            //Set Stencil To 1, Equal, Begin
            Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.AnisotropicClamp,
                new DepthStencilState
                {
                    StencilEnable = true,
                    ReferenceStencil = 0,
                    StencilFunction = CompareFunction.GreaterEqual,
                    StencilPass = StencilOperation.Keep
                },
                RasterizerState.CullNone,
                lightEffect = Game.GetEffect("Lighting"));
            lightEffect.Parameters["lightPos"].SetValue(lightPosNormal);
            lightEffect.Parameters["targetPos"].SetValue(targetPosNormal);
            lightEffect.Parameters["lv"].SetValue(targetPosNormal - lightPosNormal);
            lightEffect.Parameters["maxAngle"].SetValue(light.Angle);
            lightEffect.Parameters["falloff"].SetValue(light.Falloff);
            lightEffect.Parameters["intensity"].SetValue(light.Intensity);
            lightEffect.Parameters["rangeInverse"].SetValue(light.RangeInverse);

            //Draw texTarget w light
            Game.SpriteBatch.Draw(texTarget, Vector2.Zero, light.Diffuse);
            
            //Pop Target
            Game.SpriteBatch.End();
            Game.PopTarget();

            //Draw light additive
            Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            Game.SpriteBatch.Draw(stencilTarget, Vector2.Zero, Color.White);
            Game.SpriteBatch.End();
        }

        public void DrawGame(GameTime gameTime)
        {
            ////////////////////
            // Render Background (-inf, -1)
            ////////////////////
            int texI = 0;
            prevWrap = false;
            wrapEffect = Game.GetEffect("texturewrapper");
            if (UseLights)
            {
                Game.SpriteBatch.End();
#if USE_LIGHTMAP
                if (Game.Frame % 10 == 0) //REDRAW LIGHTS PERIOD
                {
                    Game.PushTarget(lightMap, false);
                    {
#endif
                        Game.PushTarget(texTarget);
                        {
                            Game.GraphicsDevice.Clear(Color.White);

                            Game.SpriteBatch.Begin();
                            if (!string.IsNullOrWhiteSpace(BackgroundTexture))
                            {
                            }

                            for (; texI < Textures.Count && Textures[texI].Depth < -1; texI++)
                            {
                                DrawTexture(Textures[texI], gameTime);
                            }
                            prevWrap = false;

                            Game.SpriteBatch.End();
                        }
                        Game.PopTarget();

                        Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                        Game.SpriteBatch.Draw(texTarget, Vector2.Zero, Light.GlobalAmbient);
                        Game.SpriteBatch.End();

                        ////////////////////
                        // Draw Lights
                        ////////////////////
                        foreach (var item in Lights)
                        {
                            DrawLight(item);
                        }
#if USE_LIGHTMAP
                    }
                    Game.PopTarget();

                }

                Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                Game.SpriteBatch.Draw(lightMap, Vector2.Zero, Color.White);
                Game.SpriteBatch.End();
#endif
                Game.SpriteBatch.Begin();
            }

            ////////////////////
            // Render Background [-1, 0]
            ////////////////////
            for (; texI < Textures.Count && Textures[texI].Depth <= 0; texI++)
            {
                DrawTexture(Textures[texI], gameTime);
            }
            prevWrap = false;

            Game.RestartBatch();
            ////////////////////
            // Render Entities
            ////////////////////
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                var item = Entities[i];
                if (item.DrawNoLight)
                    item.Draw(gameTime, ViewOffset);
                if (item.DrawBorders)
                    item.DoDrawBorders(gameTime, ViewOffset);
            }

            ////////////////////
            // Render Foreground
            ////////////////////
            for (; texI < Textures.Count; texI++)
            {
                DrawTexture(Textures[texI], gameTime);
            }
            prevWrap = false;
            Game.RestartBatch();
        }

        public void DrawTexture(PlatformerTexture item, GameTime gameTime)
        {
            var slice = Game.GetSprite(item.Sprite).GetSlice();
            var texSize = new Vector2(slice.Texture.Width, slice.Texture.Height);
            var sliceSize = (slice.Crop.HasValue ?
                    new Vector2(slice.Crop.Value.Width, slice.Crop.Value.Height) :
                    texSize);
            var itemSize = item.Size.X < 0 ? sliceSize : item.Size;
            if (item.TextureWrap)
            {
                if (!prevWrap)
                {
                    //wrapEffect = Game.GetEffect("texturewrapper");
                    Game.SpriteBatch.End();
                    Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, wrapEffect);
                    prevWrap = true;
                }
            }
            else if (!item.TextureWrap && prevWrap)
            {
                Game.RestartBatch();
            }

            var texPos = (slice.Crop.HasValue ?
                new Vector2(slice.Crop.Value.Left, slice.Crop.Value.Top) :
                Vector2.Zero);

            wrapEffect.Parameters["texRelativeSize"].SetValue(sliceSize * item.WrapScale / itemSize);
            wrapEffect.Parameters["sliceRelativeSize"].SetValue(sliceSize * item.Scale / texSize);
            wrapEffect.Parameters["texPos"].SetValue(texPos / texSize);

            wrapEffect.Parameters["wrapX"].SetValue(item.WrapX);
            wrapEffect.Parameters["wrapY"].SetValue(item.WrapY);

            wrapEffect.Parameters["uv"].SetValue(item.Uv);

            Game.SpriteBatch.Draw(
                slice.Texture,
                GeometryHelper.Vectors2Rectangle(
                    item.Position + (ViewOffset * item.ScrollRate),
                    itemSize),
                slice.Crop,
                item.Tint,
                item.Rotation,
                item.Origin, 
                item.SpriteEffect, 
                item.BatchDepth);
        }
    }
}