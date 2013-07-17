using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Neat.Mathematics;

namespace Lumi
{
    [Serializable]
    public class Elevator : Entity
    {
        Vector2 _direction = new Vector2(0, 1);
        TimeSpan _flipTime = new TimeSpan(0, 0, 5);
        bool _enabled = true;
        bool _platform = true;
        public Vector2 Direction { get { return _direction; } set { _direction = value; } }
        public TimeSpan FlipTime { get { return _flipTime; } set { _flipTime = value; } }
        public bool Enabled { get { return _enabled; } set { _enabled = value; } }
        public bool Platform
        {
            get
            {
                return _platform;
            }
            set
            {
                if (value)
                {
                    Body.IsFree = true;
                    EntityClass.Add("platform");
                }
                else
                {
                    Body.IsFree = false;
                    if (EntityClass.Contains("platform")) EntityClass.Remove("platform");
                }
                _platform = value;
            }
        }
        public Elevator(Vector2 position)
        {
            EntityClass.Add("elevator");
            EntityClass.Add("platform");
            Vector2 size = new Vector2(100, 24);
            Polygon p = Polygon.BuildRectangle(position.X, position.Y, size.X, size.Y);
            Body = new Body(Physics, this, p, 100);
            Body.IsStatic = true;
            Body.IsFree = true; //For Platform
            Body.AttachToGravity = false;
            Body.MaxSpeed = new Vector2(0.1f);
            Body.MaxAcceleration = new Vector2(5);
            DrawBorders = false;
        }

        TimeSpan time = new TimeSpan(0);
        public override void Update(GameTime gameTime)
        {
            MoveElevator(gameTime);
            base.Update(gameTime);
        }

        public virtual void MoveElevator(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime;
            if (time > FlipTime)
            {
                time = new TimeSpan(0);
                Direction *= new Vector2(-1);
            }

            Body.Mesh.Offset(Direction * Body.MaxSpeed * gameTime.ElapsedGameTime.Milliseconds);
        }

        #region Console Commands
        public override void AttachToConsole(Neat.Components.Console c = null)
        {
            base.AttachToConsole(c);
            if (c == null && console == null) return;
            else if (c != null) console = c;

            console.AddCommand("et_direction", et_direction);
            console.AddCommand("et_fliptime", et_fliptime);
            console.AddCommand("et_platform", o =>
                console.WriteLine((o.Count == 1 ? Platform : Platform = bool.Parse(o[1])).ToString()),
                console.AutocompleteBoolean);
            console.AddCommand("et_speedup", o =>
                {
                    var oldTime = FlipTime.TotalMilliseconds;
                    var scale = float.Parse(o[1]);

                    if (scale == 0 || float.IsNaN(scale)) return;

                    oldTime *= 1 / scale;
                    Body.MaxSpeed *= scale;
                    time = TimeSpan.FromMilliseconds(time.TotalMilliseconds / scale);
                    FlipTime = TimeSpan.FromMilliseconds(oldTime);
                });
        }

        void et_direction(IList<string> args)
        {
            Direction = GeometryHelper.String2Vector(args[1]);
        }

        void et_fliptime(IList<string> args)
        {
            FlipTime = new TimeSpan(0,0, int.Parse(args[1]));
        }
        #endregion
    }

    [Serializable]
    public class ElevatorH : Elevator
    {
        public ElevatorH(Vector2 position)
            : base(position)
        {
            Direction = new Vector2(1, 0);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }

    [Serializable]
    public class ElevatorV : Elevator
    {
        public ElevatorV(Vector2 position)
            : base(position)
        {
            Direction = new Vector2(0, 1);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }

    [Serializable]
    public class ElevatorO : Elevator
    {
        public float Radius { get; set; }
        public Vector2 Center { get; set; }
        public float Alpha { get; set; }
        public float Offset;
        Vector2 lastPos;
        public Vector2 FirstPos { get; set; }
        public float Angle;
        public ElevatorO(Vector2 position, float radius)
            : base(position)
        {
            Radius = radius;
            lastPos = position;
            FirstPos = position;
            Center = position + new Vector2(radius, 0);
            Alpha = MathHelper.ToDegrees(0.01f);
            Offset = MathHelper.ToRadians(Game.RandomGenerator.Next(0, 365));
            Angle = Offset;
        }

        public override void MoveElevator(GameTime gameTime)
        {
            Angle += Alpha * (gameTime.ElapsedGameTime.Milliseconds * 0.001f);
            var p = Center + 
                GeometryHelper.Rotate2(FirstPos - Center, 
                Angle);
            Direction = p - lastPos;
            Body.Mesh.Offset(Direction);
            lastPos = p;
        }

        #region Console Commands
        public override void AttachToConsole(Neat.Components.Console c = null)
        {
            base.AttachToConsole(c);
            if (c == null && console == null) return;
            else if (c != null) console = c;

            console.AddCommand("et_angle", et_angle);
        }

        void et_angle(IList<string> args)
        {
            Angle = float.Parse(args[1]);
        }
        #endregion
    }
}
