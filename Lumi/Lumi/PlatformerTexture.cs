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
    [Serializable]
    public class PlatformerTexture
    {
        float _depth = 0.0f;
        Vector2 _scrollRate = Vector2.One;
        bool _wrapX = true;
        bool _wrapY = true;
        Vector2 _size = new Vector2(-1);
        Color _tint = Color.White;
        Vector2 _origin = Vector2.Zero;
        Vector2 _wrapscale = Vector2.One;
        Vector2 _scale = Vector2.One;
        Vector2 _uv = Vector2.Zero;
        SpriteEffects _se = SpriteEffects.None;

        public float Depth { get { return _depth; } set { _depth = value; } }
        public Vector2 ScrollRate { get { return _scrollRate; } set { _scrollRate = value; } }
        public string Sprite { get; set; }
        public bool WrapX { get { return _wrapX; } set { _wrapX = value; } }
        public bool WrapY { get { return _wrapY; } set { _wrapY = value; } }
        public bool TextureWrap { get { return WrapX || WrapY; } }
        public Vector2 Size { get { return _size; } set { _size = value; } }
        public Vector2 Position { get; set; }
        public Color Tint { get { return _tint; } set { _tint = value; } }
        public float Rotation { get; set; }
        public Vector2 Origin { get { return _origin; } set { _origin = value; } }
        public Vector2 WrapScale { get { return _wrapscale; } set { _wrapscale = value; } }
        public Vector2 Scale { get { return _scale; } set { _scale = value; } }
        public Vector2 Uv { get { return _uv; } set { _uv = value; } }
        public SpriteEffects SpriteEffect { get { return _se; } set { _se = value; } }
        public float BatchDepth { get; set; }

        public void AttachToConsole(Console console)
        {
            if (console == null) return;

            console.AddCommand("tx_depth", o =>
                {
                    if (o.Count > 1) Depth = float.Parse(o[1]);
                    else console.WriteLine(Depth.ToString());
                    console.Run("w_sorttex");
                });
            console.AddCommand("tx_scrollrate", o =>
                {
                    if (o.Count > 1) ScrollRate = GeometryHelper.String2Vector(o[1]);
                    else console.WriteLine(GeometryHelper.Vector2String(ScrollRate));
                });
            console.AddCommand("tx_sprite", o =>
                {
                    if (o.Count > 1) Sprite = o[1];
                    else console.WriteLine(Sprite);
                }, console.AutocompleteSprites);
            console.AddCommand("tx_wrapx", o =>
                {
                    if (o.Count > 1) WrapX = bool.Parse(o[1]);
                    else console.Write(WrapX.ToString());
                }, console.AutocompleteBoolean);
            console.AddCommand("tx_wrapy", o =>
                {
                    if (o.Count > 1) WrapY = bool.Parse(o[1]);
                    else console.Write(WrapY.ToString());
                }, console.AutocompleteBoolean);
            console.AddCommand("tx_size", o =>
             {
                 if (o.Count > 1)
                 {
                     if (o[1].ToLower() == "auto")
                     {
                         var sl = ((NeatGame)console.Game).GetSlice(Sprite);
                         Size = sl.Size;
                     }
                     else Size = GeometryHelper.String2Vector(o[1]);
                 }
                 else console.WriteLine(GeometryHelper.Vector2String(Size));
             }, o => { return new string[] { "auto" };} );
            console.AddCommand("tx_position", o =>
            {
                if (o.Count > 1) Position = GeometryHelper.String2Vector(o[1]);
                else console.WriteLine(GeometryHelper.Vector2String(Position));
            });
            console.AddCommand("tx_tint", o =>
            {
                if (o.Count > 1) Tint = console.ParseColor(console.Args2Str(o,1));
                else console.WriteLine(GeometryHelper.Vector2String(Tint.ToVector4()));
            }, console.AutocompleteColors);
            console.AddCommand("tx_rotate", o =>
                console.WriteLine((o.Count > 1 ? Rotation = float.Parse(o[1]) : Rotation).ToString()));
            console.AddCommand("tx_origin", o =>
                console.WriteLine(GeometryHelper.Vector2String(o.Count > 1 ?
                Origin = GeometryHelper.String2Vector(o[1]) : Origin)));
            console.AddCommand("tx_uvscale", o =>
                console.WriteLine(GeometryHelper.Vector2String(o.Count > 1 ?
                WrapScale = GeometryHelper.String2Vector(o[1]) : WrapScale)));
            console.AddCommand("tx_scale", o =>
                console.WriteLine(GeometryHelper.Vector2String(o.Count > 1 ?
                Scale = GeometryHelper.String2Vector(o[1]) : Scale)));
            console.AddCommand("tx_batchdepth", o =>
                console.WriteLine((o.Count > 1 ? BatchDepth = float.Parse(o[1]) : BatchDepth).ToString()));
            console.AddCommand("tx_uv", o =>
                console.WriteLine(GeometryHelper.Vector2String(o.Count > 1 ?
                Uv = GeometryHelper.String2Vector(o[1]) : Uv)));
        }
    }
}