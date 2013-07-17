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
    public partial class Entity //: ISerializable
    {
        string _sprite = "solid";
        bool _flip = false;
        bool _flipy = false;
        Color _tint = Color.White;
        string _tag = null;

        public string Name { get; set; }
        public string Sprite { get { return _sprite; } set { _sprite = value; } }
        public bool Flip { get { return _flip; } set { _flip = value; } }
        public bool FlipY { get { return _flipy; } set { _flipy = value; } }
        public Color Tint { get { return _tint; } set { _tint = value; } }
        public Body Body { get; set; }
        public string Tag { get { return _tag; } set { _tag = value; } }
        public float Depth { get; set; }

        [NonSerialized]
        public PlatformerMessages Messages;
        public bool CollectBody = false;

        [NonSerialized]
        public static NeatGame Game;
        [NonSerialized]
        public static PhysicsSimulator Physics;
        [NonSerialized]
        public static PlatformerWorld World;

        public HashSet<string> EntityClass = new HashSet<string> { "generic" };
        
        public bool DrawBorders = false;
        public bool DrawNoLight = false;
        public bool DropShadow = true;

        public TimeSpan DeathTimer = new TimeSpan(0, 0, 0, 0, 500);
        public bool Dead = false;

        [NonSerialized]
        protected Dictionary<Entity, Vector2> collisionItems;

        public Entity()
        {
            Messages = new PlatformerMessages();
            collisionItems = new Dictionary<Entity, Vector2>();
        }
        /*
        public Entity(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name");
            Sprite = info.GetString("Sprite");
            Flip = info.GetBoolean("Flip");
            Tint = (Color)info.GetValue("Tint", typeof(Color));
            Body = (Body)info.GetValue("Body", typeof(Body));
            Tag = info.GetString("Tag");
            CollectBody = info.GetBoolean("CollectBody");
            EntityClass = info.GetString("EntityClass");
            DrawBorders = info.GetBoolean("DrawBorders");
            DrawNoLight = info.GetBoolean("DrawNoLight");
            DropShadow = info.GetBoolean("DropShadow");
            DeathTimer = (TimeSpan)info.GetValue("DeathTimer", typeof(TimeSpan));
            Dead = info.GetBoolean("Dead");

            Messages = new PlatformerMessages();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("Sprite", Sprite);
            info.AddValue("Flip", Flip);
            info.AddValue("Tint", Tint);
            info.AddValue("Body", Body);
            info.AddValue("Tag", Tag);
            info.AddValue("CollectBody", CollectBody);
            info.AddValue("EntityClass", EntityClass);
            info.AddValue("DrawBorders", DrawBorders);
            info.AddValue("DrawNoLight", DrawNoLight);
            info.AddValue("DropShadow", DropShadow);
            info.AddValue("DeathTimer", DeathTimer);
            info.AddValue("Dead", Dead);
        }
        */
        public virtual void Initialize()
        {
            Body.Collide = Collide;
            Body.Simulator = Physics;
            Body.Mesh.Triangulate();
            if (collisionItems == null)
                collisionItems = new Dictionary<Entity, Vector2>();
        }

        public virtual void Move(Vector2 direction, bool linear = true)
        {
            if (linear)
                Body.ApplyImpact(direction);
            else
                Body.ApplyForce(direction);
        }

        public virtual void Update(GameTime gameTime)
        {
            if (collisionItems != null)
            {
                foreach (var collision in collisionItems)
                {
                    var item = collision.Key;
                    var mtd = collision.Value;
                    if (item.EntityClass.Contains("elevator"))
                    {
                        if (mtd.Y < 0 && mtd.Y > -3)
                            Body.Mesh.Offset((item as Elevator).Direction * item.Body.MaxSpeed * gameTime.ElapsedGameTime.Milliseconds);
                    }
                }

                collisionItems.Clear();
            }
            else collisionItems = new Dictionary<Entity, Vector2>();

            if (Dead)
            {
                DeathTimer -= gameTime.ElapsedGameTime;
                if (DeathTimer.TotalMilliseconds < 0) CollectBody = true;
            }
        }

        public virtual void Die()
        {
            Body.IsFree = true;
            Dead = true;
        }

        public virtual void DoDrawBorders(GameTime gameTime, Vector2 offset, Color? color = null)
        {
            if (!color.HasValue) color = Tint;

            Body.Mesh.Draw(Game.SpriteBatch, Game.BasicLine, offset, color.Value);
            
            if (!Body.Convex && Body.Mesh.Triangles != null)
            {
                foreach (var item in Body.Mesh.Triangles)
                {
                    if (GeometryHelper.Cross(item.A, item.B, item.C) >= 0) ;
                    //item.Draw(Game.SpriteBatch, Game.BasicLine, offset, Color.Yellow);
                    else
                        item.Draw(Game.SpriteBatch, Game.BasicLine, offset, Color.Red);
                }
            }
            return;
        }

        public virtual void Draw(GameTime gameTime, Vector2 offset)
        {
            var sprite = Game.GetSlice(Sprite);
            var rect = GeometryHelper.MoveRectangle(Body.Mesh.GetBoundingRect(), offset);
            Game.SpriteBatch.Draw(
                sprite.Texture, rect, sprite.Crop, Tint,
                0.0f, Vector2.Zero,
                (Flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None) |
                (FlipY ? SpriteEffects.FlipVertically : SpriteEffects.None), 0);
        }

        public virtual void Collide(object e, Vector2 mtd)
        {
            if (e == null)
            {
                Debug.WriteLine("Orphan Body Detected");
                Debug.WriteLine(e);
                return;
            }
            if (!collisionItems.ContainsKey((Entity)e))
                collisionItems.Add((Entity)e, mtd);

            if ((e as Entity).EntityClass.Contains("platform"))
            {
                CollidePlatform(e as Entity, mtd);
            }
        }

        public virtual void CollidePlatform(Entity e, Vector2 mtd)
        {
            if (Body.Velocity.Y > 0 && mtd.Y < 0 && mtd.Y > Platform.SnapDepth)
            {
                //Body.Mesh.Offset(mtd);
                StandOn(e.Body.Mesh.GetPosition());
            }
        }

        public virtual void Shot(Projectile p, Vector2 mtd)
        {
        }

        public virtual void StandOn(Vector2 floor)
        {
            Vector2 position;
            Vector2 size;
            Body.Mesh.GetPositionAndSize(out position, out size);
            Body.Mesh.Offset(new Vector2(0, floor.Y - size.Y - position.Y));
        }
    }
}