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
    public class Player : Entity
    {
        public Player(Vector2 position)
        {
            EntityClass.Add("player");
            Vector2 size = new Vector2(212, 512) / 3.0f;
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100.0f);
            Body.AttachToGravity = true;
            Body.IsStatic = false;
            Body.MaxSpeed = new Vector2(60,40);
            Body.MaxAcceleration = new Vector2(5.5f);
            DrawNoLight = true;

            light = new Light
            {
                Position = position + new Vector2(20, 30),
                Target = position + new Vector2(10, 0),
                Falloff = 5,
                Intensity = 1f,
                Range = 0.5f,
                Angle = MathHelper.Pi / 6.0f,
                Enabled = true,
                Diffuse = Color.Orange
            };

            World.Lights.Add(light);
        }

        Light light;

        TimeSpan _maxJumpPower = new TimeSpan(0, 0, 0, 0, 350);
        bool _infiniteJump = false;

        public TimeSpan MaxJumpPower { get { return _maxJumpPower; } set { _maxJumpPower = value; } }
        public bool InfiniteJump { get { return _infiniteJump; } set { _infiniteJump = value; } }

        TimeSpan jumpPower = new TimeSpan(0);
        bool resetJump = false;
        bool lastJumping = false;
        bool lockJump = false;
        public override void Update(GameTime gameTime)
        {
            bool jumping = false;
            
            //////////////////
            //Process Messages
            //////////////////
            if (Messages.Contains(IdGenerator.MoveLeft))
                Move(Body.MaxSpeed * -Vector2.UnitX);
            if (Messages.Contains(IdGenerator.MoveRight))
                Move(Body.MaxSpeed * Vector2.UnitX);
            if (Messages.Contains(IdGenerator.Jump))
            {
                jumping = true;
            }
            if (Messages.Contains(IdGenerator.Shoot))
            {
                var aabb = Body.Mesh.GetBoundingRect();
                Projectile p;
                World.AddEntity(p = new Projectile(new Vector2(aabb.Center.X, aabb.Center.Y)));
                p.Body.NotCollideWith.Add(Body);
                p.Flip = this.Flip;
                p.Parent = this;
            }
            //////////////////
            //Handle Jumping
            //////////////////
            if (!jumping && !InfiniteJump)
            {
                if (lastJumping || (Body.Velocity.Y * Body.GravityNormal.Y > 0))
                    lockJump = true;
            }
            if (jumping)
            {
                if (!lockJump)
                {
                    jumpPower += gameTime.ElapsedGameTime;
                    if (jumpPower < MaxJumpPower || InfiniteJump)
                        Move(Body.MaxAcceleration * Body.Mass * Body.GravityNormal, false);
                }
            }
            else if (resetJump)
            {
                jumpPower = new TimeSpan(0);
                resetJump = false;
            }
            
            /////////////////////////////
            //Set Sprites and Direction
            /////////////////////////////
            if (Math.Abs(Body.Velocity.X) > 1)
                Flip = Body.Velocity.X < 0;

            if (Math.Abs(Body.Velocity.X) < 2.0f)
            {
                Sprite = "standing-blinking";
            }
            else
                Sprite = "walking";
            
            ///////////////////
            //Resolve Collisions
            ///////////////////
            if (collisionItems != null)
                foreach (var collision in collisionItems)
                {
                    var item = collision.Key;
                    var mtd = collision.Value;

                    //If on ground, recharge jump
                    if (mtd.Y < 0 && mtd.Y > -3 && !item.Body.IsFree)
                    {
                        resetJump = true;
                        lockJump = false;
                    }

                    if (item.EntityClass.Contains("item"))
                    {
                        item.Die();
                    }
                }
            else collisionItems = new Dictionary<Entity, Vector2>();
            lastJumping = jumping;
#if KINECT
            if (Game.Touch.Enabled)
            {
                var myPos = Body.Mesh.GetPosition() + new Vector2(20, 30);
                light.Position = myPos;
                foreach (var item in Game.Touch.TrackPoints)
                {
                    light.Target = (item.Position - World.ViewOffset);
                    break;
                }
            }
#else
            var myPos = Body.Mesh.GetPosition() + new Vector2(20, 30);
            light.Position = myPos;
            light.Target = Vector2.Normalize(Game.MousePosition - World.ViewOffset - myPos)*1000f + myPos;
#endif
            base.Update(gameTime);
        }

        public override void StandOn(Vector2 floor)
        {
            base.StandOn(floor);
            resetJump = true;
            lockJump = false;
        }

        #region Console Commands
        public override void AttachToConsole(Neat.Components.Console c = null)
        {
            base.AttachToConsole(c);
            if (c == null && console == null) return;
            else if (c != null) console = c;

            console.AddCommand("et_maxjumppower", et_maxjumppower);
            console.AddCommand("et_infinitejump", et_infinitejump,
                console.AutocompleteBoolean);
        }

        void et_maxjumppower(IList<string> args)
        {
            MaxJumpPower = new TimeSpan(0, 0, 0, 0, int.Parse(args[1]));
        }

        void et_infinitejump(IList<string> args)
        {
            InfiniteJump = bool.Parse(args[1]);
        }
        #endregion
    }
}