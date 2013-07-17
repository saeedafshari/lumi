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
    [Serializable]
    public class Wall : Entity
    {
        public Wall(Polygon polygon)
        {
            EntityClass.Add("wall");
            Body = new Body(Physics, this, polygon, 1000.0f);
            Body.IsStatic = true;
            Body.AttachToGravity = false;
            DrawBorders = false;
            Body.Stop();
            Body.MaxSpeed = Vector2.Zero;
            DropShadow = true;
        }
    }

    [Serializable]
    public class Platform : Entity
    {
        public static float SnapDepth = -20;
        public Platform(Polygon polygon)
        {
            EntityClass.Add("platform");
            Body = new Body(Physics, this, polygon, 1000.0f);
            Body.IsStatic = true;
            Body.AttachToGravity = false;
            Body.IsFree = true;
            DrawBorders = false;
            Body.Stop();
            Body.MaxSpeed = Vector2.Zero;
            DropShadow = true;
        }

        #region Console Commands
        public override void AttachToConsole(Neat.Components.Console c = null)
        {
            base.AttachToConsole(c);
            if (c == null && console == null) return;
            else if (c != null) console = c;

            console.AddCommand("et_snapdepth", et_snapdepth);
        }

        void et_snapdepth(IList<string> args)
        {
            SnapDepth = float.Parse(args[1]);
        }
        #endregion
    }
}