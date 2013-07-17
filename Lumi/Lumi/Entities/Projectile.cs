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
using System.Runtime.Serialization;

namespace Lumi
{
    [Serializable]
    public class Projectile : Entity
    {
        public Entity Parent;
        public Projectile(Vector2 position)
        {
            EntityClass.Add("projectile");
            Sprite = "projectile";
            Vector2 size = new Vector2(44, 19);
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100.0f);
            Body.AttachToGravity = false;
            Body.IsStatic = false;
            Body.MaxSpeed = new Vector2(40,0);
            Body.MaxAcceleration = new Vector2(5.5f);
            DrawNoLight = true;
            DropShadow = false;
            DeathTimer = TimeSpan.Zero;
        }

        public override void Update(GameTime gameTime)
        {
            Body.Velocity = Body.MaxSpeed;
            if (Flip) Body.Velocity.X *= -1;

            if (collisionItems.Count > 0)
            {
                foreach (var item in collisionItems)
                {
                    if (!item.Key.EntityClass.Contains("projectile"))
                    {
                        if (Parent != null && item.Key == Parent)
                            continue;
                        Die();
                        item.Key.Shot(this, item.Value);
                        break;
                    }
                }
            }

            base.Update(gameTime);
        }
    }
}
