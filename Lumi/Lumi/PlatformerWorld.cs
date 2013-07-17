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
    public partial class PlatformerWorld
    {
        public PhysicsSimulator Physics;
        public List<Entity> Entities;
        public List<Light> Lights;
        public List<PlatformerTexture> Textures;
        public string BackgroundTexture = null;

        public PlatformerMessages Messages;

        Vector2 _viewOffsetMoveSpeed = new Vector2(5);
        Vector2 _bottomRight = new Vector2(2048);
        Vector2 _followThreshold = new Vector2(200, 0);
        
        public Vector2 ViewOffset { get; set; }
        public Vector2 ViewOffsetMoveSpeed { get { return _viewOffsetMoveSpeed; } set { _viewOffsetMoveSpeed = value; } }
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomRight { get { return _bottomRight; } set { _bottomRight = value; } }
        public Vector2 FollowThreshold { get { return _followThreshold; } set { _followThreshold = value; } }
        public bool _limitLeft = true;
        public bool _limitRight = true;
        public bool _limitUp = true;
        public bool _limitDown = true;

        public bool LimitLeft { get { return _limitLeft; } set { _limitLeft = value; } }
        public bool LimitRight { get { return _limitRight; } set { _limitRight = value; } }
        public bool LimitUp { get { return _limitUp; } set { _limitUp = value; } }
        public bool LimitDown { get { return _limitDown; } set { _limitDown = value; } }

        NeatGame _game;

        public NeatGame Game { get { return _game; } set { _game = value; } }

        [NonSerialized]
        public PlatformerEditor Editor;

        public bool Paused { get; set; }
        public bool FollowMode { get; set; }

        public PlatformerWorld(NeatGame game)
        {
            this.Game = game;

            PhysicsSimulator.Game = Game;
            Physics = new PhysicsSimulator();
            Entities = new List<Entity>();
            Textures = new List<PlatformerTexture>();
            Lights = new List<Light>();

            IdGenerator.Initialize();
            Messages = new PlatformerMessages();
            Entity.Game = Game;
            Entity.Physics = Physics;
            Entity.World = this;
            Physics.SpeedCoef *= 1.25f;
            Physics.Gravity = new Vector2(2f);

            Initialize();
        }

        void Initialize()
        {
            FollowMode = true;
            Paused = false;

            InitializeEntities();
            InitRender();
            AttachToConsole();
            Editor = new PlatformerEditor(this);
        }

        public List<Entity> GetByTag(string tag)
        {
            return Entities.Where(o => o.Tag == tag).ToList();
        }

        public void AddEntity(Entity e, bool sort = true)
        {
            Entities.Add(e);
            e.Initialize();
            if (sort) SortEntities();
        }

        public void InitializeEntities()
        {
            var b = 0;
            var r = 0;
            foreach (var item in Entities)
            {
                item.Initialize();
                var rect = item.Body.Mesh.GetBoundingRect();
                if (rect.Bottom > b) b = rect.Bottom;
                if (rect.Right > r) r = rect.Right;
            }

            //BottomRight = new Vector2(r,b);
        }

        public void SortTextures()
        {
            Textures = Textures.OrderBy(o => o.Depth).ToList();
        }

        public void SortEntities()
        {
            Entities = Entities.OrderBy(o => o.Depth).ToList();
        }

        public void Follow(Vector2 target)
        {
            Vector2 center = new Vector2(Game.GameWidth, Game.GameHeight) / 2.0f;
            float tLeft = center.X - FollowThreshold.X;
            float tRight = center.X + FollowThreshold.X;
            float tUp = center.Y - FollowThreshold.Y;
            float tDown = center.Y + FollowThreshold.Y;

            var ViewPoint = -ViewOffset;
            //Focus on target (left, right)
            /*
            if (target.X < ViewPoint.X + FollowThreshold.X)
                ViewPoint.X = target.X - FollowThreshold.X;
            if (target.X > ViewPoint.X + Game.GameWidth - FollowThreshold.X)
                ViewPoint.X = target.X - Game.GameWidth + FollowThreshold.X;
            */
            
            if (target.X < ViewPoint.X + tLeft)
                ViewPoint.X = target.X - tLeft;
            if (target.X > ViewPoint.X + tRight)
                ViewPoint.X = target.X - tRight;

            //Focus on target (up, down)
            if (target.Y < ViewPoint.Y + tUp)
                ViewPoint.Y = target.Y - tUp;
            if (target.Y > ViewPoint.Y + tDown)
                ViewPoint.Y = target.Y - tDown;

            //Bound camera
            if (LimitLeft && ViewPoint.X < TopLeft.X) ViewPoint.X = TopLeft.X;
            if (LimitUp && ViewPoint.Y < TopLeft.Y) ViewPoint.Y = TopLeft.Y;
            if (LimitRight && ViewPoint.X + Game.GameWidth > BottomRight.X) ViewPoint.X = BottomRight.X - Game.GameWidth;
            if (LimitDown && ViewPoint.Y + Game.GameHeight > BottomRight.Y) ViewPoint.Y = BottomRight.Y - Game.GameHeight;

            ViewOffset = -ViewPoint;
        }

        public void UpdateGame(GameTime gameTime)
        {
            if (FollowMode)
                Follow(GeometryHelper.Point2Vector(Entities[0].Body.Mesh.GetBoundingRect().Center));

            //////////////////
            //Remove Dead Bodies
            //////////////////
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].CollectBody)
                {
                    Physics.Bodies.Remove(Entities[i].Body);
                    Entities.RemoveAt(i);
                }
            }

            //////////////////
            //Process Messages
            //////////////////
            if (Messages.Contains(IdGenerator.OffsetDown))
            {
                ViewOffset -= Vector2.UnitY * ViewOffsetMoveSpeed;
            }
            else if (Messages.Contains(IdGenerator.OffsetUp))
            {
                ViewOffset += Vector2.UnitY * ViewOffsetMoveSpeed;
            }
            else if (Messages.Contains(IdGenerator.OffsetLeft))
            {
                ViewOffset += Vector2.UnitX * ViewOffsetMoveSpeed;
            }
            else if (Messages.Contains(IdGenerator.OffsetRight))
            {
                ViewOffset -= Vector2.UnitX * ViewOffsetMoveSpeed;
            }
            

            //////////////////
            //Update Entities
            //////////////////
            for (int i = 0; i < Entities.Count; i++)
			{
                var item = Entities[i];
                item.Messages = Messages;
                item.Update(gameTime);
            }

            /////////////////
            //Update Physics
            /////////////////
            Physics.Update(gameTime);

            /////////////////
            //Clear Messages
            /////////////////
            Messages.Clear();
        }

        public void Update(GameTime gameTime)
        {
            if (!Paused)
                UpdateGame(gameTime);

            if (Editor.Enabled)
            {
                Editor.HandleInput(gameTime);
                Editor.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            DrawGame(gameTime);

            if (Editor.Enabled)
                Editor.Draw(gameTime);
        }
    }

    public class PlatformerMessages : HashSet<int>
    {
    }
}