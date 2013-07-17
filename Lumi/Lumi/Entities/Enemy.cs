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
    public class Enemy : Entity
    {
        public Enemy()
        {
        }

        public Enemy(Vector2 position)
        {
            EntityClass.Add("enemy");
            var size = new Vector2(73, 96);
            Sprite = "blue_monster";
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100.0f);
        }

        public override void Initialize()
        {
            Create();
            base.Initialize();
        }

        public virtual void Create()
        {
            Body.AttachToGravity = true;
            Body.IsStatic = false;
            Body.MaxSpeed = new Vector2(5);
            Body.MaxAcceleration = new Vector2(5.5f);
            DrawNoLight = true;
            DropShadow = false;
        }

        public virtual void PerformLocomotorLR(GameTime gameTime)
        {
            if (Body.Velocity.X != 0)
                Flip = Body.Velocity.X < 0;

            bool gravityFlip = Body.GravityNormal.Y > 0;
            //FlipY = gravityFlip;

            bool doflip = false;
            if (collisionItems.Count > 0)
            {
                var myaabb = Body.Mesh.GetBoundingRect();
                foreach (var item in collisionItems)
                {
                    var e = item.Key;
                    if (!e.EntityClass.Contains("wall") && !e.EntityClass.Contains("platform"))
                        continue;

                    var mtd = item.Value;
                    if (Math.Abs(mtd.Y) > Math.Abs(mtd.X))
                    {
                        var aabb = e.Body.Mesh.GetBoundingRect();
                        if (((!gravityFlip && aabb.Top > myaabb.Center.Y) ||
                            (gravityFlip && aabb.Bottom < myaabb.Center.Y)) &&
                            ((Body.Velocity.X > 0 && myaabb.Right > aabb.Right) ||
                            (Body.Velocity.X < 0 && myaabb.Left < aabb.Left)))
                        {
                            doflip = true;
                            //Body.Velocity.X = Body.MaxSpeed.X * Math.Sign(mtd.X);
                            //Body.Velocity.X = Body.MaxSpeed.X * (Flip ? -1 : 1);
                            break;
                        }
                    }
                    else if (e.EntityClass.Contains("wall"))
                    {
                        doflip = true;
                    }
                }
            }
            if (doflip)
                Body.Velocity.X *= -1;

            if (Body.Velocity.X == 0)
            {
                Body.Velocity.X = Body.MaxSpeed.X * (Flip ? 1 : -1);
            }
            else
                Body.Velocity.X = Math.Sign(Body.Velocity.X) * Body.MaxSpeed.X;
        }

        public virtual void PerformLocomotorUD(GameTime gameTime)
        {
            Flip = Body.GravityNormal.X < 0;
            
            bool doflip = false;
            if (collisionItems.Count > 0)
            {
                var myaabb = Body.Mesh.GetBoundingRect();
                foreach (var item in collisionItems)
                {
                    var e = item.Key;
                    if (!e.EntityClass.Contains("wall") && !e.EntityClass.Contains("platform"))
                        continue;

                    var mtd = item.Value;
                    if (Math.Abs(mtd.X) > Math.Abs(mtd.Y))
                    {
                        var aabb = e.Body.Mesh.GetBoundingRect();
                        if (((!Flip && aabb.Left < myaabb.Center.X) ||
                            (Flip && aabb.Right > myaabb.Center.X))
                            &&
                            ((Body.Velocity.Y > 0 && myaabb.Bottom > aabb.Bottom) ||
                            (Body.Velocity.Y < 0 && myaabb.Top < aabb.Top)))
                        {
                            doflip = true;
                            break;
                        }
                    }/*
                    else if (e.EntityClass.Contains("wall"))
                    {
                        doflip = true;
                    }*/
                }
            }
            if (doflip)
                Body.Velocity.Y *= -1;

            if (Body.Velocity.Y == 0)
            {
                Body.Velocity.Y = Body.MaxSpeed.Y * (doflip ? 1 : -1);
            }
            else
                Body.Velocity.Y = Math.Sign(Body.Velocity.Y) * Body.MaxSpeed.Y;
        }

        public override void Update(GameTime gameTime)
        {
            PerformLocomotorLR(gameTime);
            
            base.Update(gameTime);   
        }

        protected Action<GameTime> UpdateBase { get { return base.Update; } }

        public override void Shot(Projectile p, Vector2 mtd)
        {
            Body.ApplyForce(new Vector2(
                Game.RandomGenerator.Next((int)(-Body.MaxSpeed.X), (int)Body.MaxSpeed.X),
                Game.RandomGenerator.Next((int)-Body.MaxSpeed.Y, 0)));
            Die();
            Body.IsFree = true;
            base.Shot(p, mtd);
        }
    }

    [Serializable]
    public class YellowWorm : Enemy
    {
        public YellowWorm(Vector2 position)
        {
            var size = new Vector2(121, 82);
            Sprite = "monster_yellowworm";
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100.0f);
        }
    }

    [Serializable]
    public class WhiteBird : Enemy
    {
        public WhiteBird(Vector2 position)
        {
            var size = new Vector2(98, 90);
            Sprite = "white_bird";
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100.0f);
        }
    }

    [Serializable]
    public class EyeMonster : Enemy
    {
        public EyeMonster(Vector2 position)
        {
            var size = new Vector2(92, 80);
            Sprite = "eye_monster";
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100.0f);
        }
    }

    [Serializable]
    public class Roach : Enemy
    {
        public Roach(Vector2 position)
        {
            var size = new Vector2(101, 77);
            Sprite = "monster_roach";
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100.0f);
        }
    }

    [Serializable]
    public class Sticky : Enemy
    {
        public Sticky(Vector2 position)
        {
            var size = new Vector2(60, 78);
            Sprite = "monster_sticky";
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100.0f);
            Body.GravityNormal = new Vector2(1, 0);
        }

        public override void Update(GameTime gameTime)
        {
            PerformLocomotorUD(gameTime);

            UpdateBase(gameTime);
        }
    }

    [Serializable]
    public class StickyU : Enemy
    {
        public StickyU(Vector2 position)
        {
            var size = new Vector2(78, 60);
            Sprite = "monster_stickyu";
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100.0f);
            Body.GravityNormal *= -1;
        }
    }
}
