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
    public partial class PlatformerWorld
    {
        [NonSerialized] Light selectedLight;

        public void AttachToConsole()
        {
            Console console = Game.Console;
            console.AddCommand("w_wall", o =>
                {
                    Entity e;
                    Entities.Add(e=new Wall(Polygon.BuildRectangle(GeometryHelper.Vectors2Rectangle(
                        GeometryHelper.String2Vector(o[1]), GeometryHelper.String2Vector(o[2])))));
                    e.AttachToConsole();
                }) ;
            console.AddCommand("w_light", o =>
                {
                    Lights.Add(selectedLight = new Light()
                    {
                        Position = Game.MousePosition - ViewOffset,
                        Target = Game.MousePosition - ViewOffset + Vector2.One,
                        Diffuse = Color.White,
                        Angle = MathHelper.Pi,
                        Falloff = 10f,
                        Intensity = 1.5f,
                        Enabled = true
                    });
                    selectedLight.AttachToConsole(console);
                });
            console.AddCommand("w_view", o =>
                {
                    if (o.Count > 1) ViewOffset = GeometryHelper.String2Vector(o[1]);
                    else console.WriteLine(GeometryHelper.Vector2String(ViewOffset));
                });
            console.AddCommand("w_viewspeed", o =>
            {
                if (o.Count > 1) ViewOffsetMoveSpeed = GeometryHelper.String2Vector(o[1]);
                else console.WriteLine(GeometryHelper.Vector2String(ViewOffsetMoveSpeed));
            });
            console.AddCommand("w_topleft", o =>
            {
                if (o.Count > 1) TopLeft = GeometryHelper.String2Vector(o[1]);
                else console.WriteLine(GeometryHelper.Vector2String(TopLeft));
            });
            console.AddCommand("w_bottomright", o =>
            {
                if (o.Count > 1) BottomRight = GeometryHelper.String2Vector(o[1]);
                else console.WriteLine(GeometryHelper.Vector2String(BottomRight));
            });
            console.AddCommand("w_followthreshold", o =>
            {
                if (o.Count > 1) FollowThreshold = GeometryHelper.String2Vector(o[1]);
                else console.WriteLine(GeometryHelper.Vector2String(FollowThreshold));
            });
            console.AddCommand("w_limitleft", o =>
            {
                if (o.Count > 1) LimitLeft = bool.Parse(o[1]);
                else console.WriteLine(LimitLeft.ToString());
            }, console.AutocompleteBoolean);
            console.AddCommand("w_limitright", o =>
            {
                if (o.Count > 1) LimitRight = bool.Parse(o[1]);
                else console.WriteLine(LimitRight.ToString());
            }, console.AutocompleteBoolean);
            console.AddCommand("w_limitup", o =>
            {
                if (o.Count > 1) LimitUp = bool.Parse(o[1]);
                else console.WriteLine(LimitUp.ToString());
            }, console.AutocompleteBoolean);
            console.AddCommand("w_limitdown", o =>
            {
                if (o.Count > 1) LimitDown = bool.Parse(o[1]);
                else console.WriteLine(LimitDown.ToString());
            }, console.AutocompleteBoolean);
            console.AddCommand("w_lights", o =>
            {
                if (o.Count > 1) UseLights = bool.Parse(o[1]);
                else console.WriteLine(UseLights.ToString());
            }, console.AutocompleteBoolean);
            console.AddCommand("w_sorttex", o => SortTextures());
            console.AddCommand("w_sortentities", o => SortEntities());
        }
    }
}