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
    public class Item : Entity
    {
        Vector2 size = new Vector2(64);
        Vector2 position = Vector2.Zero;
        public Item(Vector2 pos, Vector2 size)
        {
            position = pos;
            this.size = size;
            Create();
        }

        public Item(Vector2 pos)
        {
            position = pos;
            Create();
        }

        void Create()
        {
            EntityClass.Add("item");
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 1000.0f);
            Body.IsFree = true;
            Body.IsStatic = true;
            Body.MaxSpeed = Vector2.Zero;
            Body.Stop();
            DrawNoLight = true;
            DropShadow = false;
            DrawBorders = false;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var item in collisionItems)
            {
                if (item.Key.EntityClass.Contains("player"))
                    Die();
            }
            if (Dead) Body.Mesh.Offset(-Vector2.UnitY);
            base.Update(gameTime);
        }

        [NonSerialized]
        Effect e;

        public override void Draw(GameTime gameTime, Vector2 offset)
        {
            if (Dead)
            {
                e = Game.GetEffect("ColorFilter");
                e.Parameters["color"].SetValue(new Vector4(Color.Yellow.ToVector3()*10.0f, 1));
                e.CurrentTechnique = e.Techniques["ColorBalance"];
                Game.UseEffect(e);
            }
            base.Draw(gameTime, offset);
            if (Dead)
            {
                Game.RestartBatch();
            }
        }
    }

    [Serializable]
    public class Banana : Item
    {
        public Banana(Vector2 pos)
            : base(pos, new Vector2(80, 98))
        {
            Sprite = "banana";
        }
    }

    [Serializable]
    public class Strawberry : Item
    {
        public Strawberry(Vector2 pos)
            : base(pos, new Vector2(78, 98))
        {
            Sprite = "strawberry";
        }
    }

    [Serializable]
    public class Cherry : Item
    {
        public Cherry(Vector2 pos)
            : base(pos, new Vector2(74, 92))
        {
            Sprite = "cherry";
        }
    }

    [Serializable]
    public class Lollipop : Item
    {
        public Lollipop(Vector2 pos)
            : base(pos, new Vector2(49, 73))
        {
            Sprite = "lollipop";
        }
    }

    [Serializable]
    public class Stick : Item
    {
        public Stick(Vector2 pos)
            : base(pos, new Vector2(56, 76))
        {
            Sprite = "stick";
        }
    }

    [Serializable]
    public class Flash : Item
    {
        public Flash(Vector2 pos)
            : base(pos, new Vector2(35, 76))
        {
            Sprite = "flash";
        }
    }

    [Serializable]
    public class Tablet : Item
    {
        public Tablet(Vector2 pos)
            : base(pos, new Vector2(115, 76))
        {
            Sprite = "tablet";
        }
    }

    [Serializable]
    public class Battery : Item
    {
        public Battery(Vector2 pos)
            : base(pos, new Vector2(50, 83))
        {
            Sprite = "battery";
        }
    }
}
