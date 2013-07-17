using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Neat;
using Neat.Mathematics;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;

namespace Lumi
{
    [Serializable]
    public class Light
    {
        [NonSerialized] public static Effect Effect;
        [NonSerialized] public static Vector2 ViewPortSize;
        public static Color GlobalAmbient = Color.Black;
        bool _enabled = true;
        float _depth = 1.0f;
        float _intensity = 1.0f;
        float _rangeInverse = 1.0f;

        public Vector2 Position { get; set; }
        public Vector2 Target { get; set; }
        public float Angle;// { get; set; }
        public float AngleDeg
        {
            get { return MathHelper.ToDegrees(Angle); }
            set { Angle = MathHelper.ToRadians(value); }
        }
        public float Range { get { return 1.0f / _rangeInverse; } set { _rangeInverse = 1.0f / value; } }
        public float RangeInverse { get { return _rangeInverse; } set { _rangeInverse = value; } }
        public Color Diffuse { get; set; }
        public float Falloff { get; set; }
        public float Intensity { get { return _intensity; } set { _intensity = value; } }
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }
        public float Depth { get { return _depth; } set { _depth = value; } }

        public static Vector2 Transform(Vector2 v)
        {
            return v / ViewPortSize;
        }

        #region Console
        [NonSerialized]
        Neat.Components.Console console;
        
        public void AttachToConsole(Neat.Components.Console _c)
        {
            console = _c;
            if (console == null) return;
            console.AddCommand("lt_ambient", o =>
                {
                    if (o.Count > 1) GlobalAmbient = console.ParseColor(console.Args2Str(o,1));
                    else console.WriteLine(GeometryHelper.Vector2String(GlobalAmbient.ToVector4()));
                }, console.AutocompleteColors);

            console.AddCommand("lt_position", lt_position);
            console.AddCommand("lt_target", lt_target);
            console.AddCommand("lt_angle", lt_angle);
            console.AddCommand("lt_angledeg", lt_angledeg);
            console.AddCommand("lt_range", o =>
                {
                    if (o.Count > 1) Range = float.Parse(o[1]);
                    else console.WriteLine(Range.ToString());
                });
            console.AddCommand("lt_rangeinverse", o =>
                {
                    if (o.Count > 1) RangeInverse = float.Parse(o[1]);
                    else console.WriteLine(RangeInverse.ToString());
                });
            console.AddCommand("lt_diffuse", lt_diffuse, console.AutocompleteColors);
            console.AddCommand("lt_falloff", lt_diffusecoef);
            console.AddCommand("lt_intensity", o =>
                {
                    if (o.Count > 1) Intensity = float.Parse(o[1]);
                    else console.WriteLine(Intensity.ToString());
                });
            console.AddCommand("lt_depth", lt_depth);
            console.AddCommand("lt_enabled", lt_enabled, console.AutocompleteBoolean);
        }

        void lt_position(IList<string> args)
        {
            if (args.Count > 1)
                Position = GeometryHelper.String2Vector(args[1]);
            else
                Console.WriteLine(GeometryHelper.Vector2String(Position));
        }

        void lt_target(IList<string> args)
        {
            if (args.Count > 1)
                Target = GeometryHelper.String2Vector(args[1]);
            else
                Console.WriteLine(GeometryHelper.Vector2String(Target));
        }

        void lt_angle(IList<string> args)
        {
            if (args.Count > 1)
                Angle = float.Parse(args[1]);
            else
                console.WriteLine(Angle.ToString());
        }

        void lt_angledeg(IList<string> args)
        {
            if (args.Count > 1)
                Angle = MathHelper.ToRadians(float.Parse(args[1]));
            else
                console.WriteLine(MathHelper.ToDegrees(Depth).ToString());
        }

        void lt_diffuse(IList<string> args)
        {
            if (args.Count > 1)
                Diffuse = console.ParseColor(console.Args2Str(args, 1));
            else
                console.WriteLine(GeometryHelper.Vector2String(Diffuse.ToVector4()));
        }

        void lt_diffusecoef(IList<string> args)
        {
            if (args.Count > 1)
                Falloff = float.Parse(args[1]);
            else
                console.WriteLine(Falloff.ToString());
        }

        void lt_depth(IList<string> args)
        {
            if (args.Count > 1)
                Depth = float.Parse(args[1]);
            else
                console.WriteLine(Depth.ToString());
        }

        void lt_enabled(IList<string> args)
        {
            if (args.Count > 1)
                Enabled = bool.Parse(args[1]);
            else
                console.WriteLine(Enabled.ToString());
        }
        #endregion
    }
}
