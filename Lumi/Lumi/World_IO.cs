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
using System.IO;

using System.Runtime.Serialization;

namespace Lumi
{
    [Serializable]
    public partial class PlatformerWorld : ISerializable
    {
        protected PlatformerWorld(SerializationInfo info, StreamingContext context)
        {
            Game = NeatGame.Games[0];
            PhysicsSimulator.Game = Game;
            IdGenerator.Initialize();
            Messages = new PlatformerMessages();
            Entity.Game = Game;
            Entity.World = this;

            Physics = (PhysicsSimulator) info.GetValue("Physics", typeof(PhysicsSimulator));
            Entity.Physics = Physics;

            Entities = (List<Entity>) info.GetValue("Entities", typeof(List<Entity>));
            Lights = (List<Light>) info.GetValue("Lights", typeof(List<Light>));
            Textures = (List<PlatformerTexture>) info.GetValue("Textures", typeof(List<PlatformerTexture>));
            
            TopLeft = (Vector2)info.GetValue("TopLeft", typeof(Vector2));
            BottomRight = (Vector2)info.GetValue("BottomRight", typeof(Vector2));
            LimitLeft = info.GetBoolean("LimitLeft");
            LimitRight = info.GetBoolean("LimitRight");
            LimitUp = info.GetBoolean("LimitUp");
            LimitDown = info.GetBoolean("LimitDown");
            ViewOffset = (Vector2)info.GetValue("ViewOffset", typeof(Vector2));
            ViewOffsetMoveSpeed = (Vector2)info.GetValue("ViewOffsetMoveSpeed", typeof(Vector2));
            FollowThreshold = (Vector2)info.GetValue("FollowThreshold", typeof(Vector2));
            foreach (var item in Physics.Bodies)
            {
                item.Simulator = Physics;
            }

            try
            {
                UseLights = info.GetBoolean("UseLights");
            }
            catch
            {
            }

            try
            {
                Light.GlobalAmbient = new Color((Vector4)info.GetValue("GlobalAmbient", typeof(Vector4)));
            }
            catch
            {
            }

            try
            {
                BackgroundTexture = info.GetString("BackgroundTexture");
            }
            catch
            { }

            Initialize();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Physics", Physics, Physics.GetType());
            info.AddValue("Entities", Entities, Entities.GetType());
            info.AddValue("Lights", Lights, Lights.GetType());
            info.AddValue("Textures", Textures, Textures.GetType());
            
            info.AddValue("TopLeft", TopLeft);
            info.AddValue("BottomRight", BottomRight);
            info.AddValue("LimitLeft", LimitLeft);
            info.AddValue("LimitRight", LimitRight);
            info.AddValue("LimitUp", LimitUp);
            info.AddValue("LimitDown", LimitDown);
            info.AddValue("ViewOffset", ViewOffset);
            info.AddValue("ViewOffsetMoveSpeed", ViewOffsetMoveSpeed);
            info.AddValue("FollowThreshold", FollowThreshold);

            info.AddValue("UseLights", UseLights);

            info.AddValue("GlobalAmbient", Light.GlobalAmbient.ToVector4());

            info.AddValue("BackgroundTexture", BackgroundTexture);
        }

        public void Save(string path)
        {
            Serializer.ObjectToFile(this, path);
        }

        public static PlatformerWorld Load(string path)
        {
            return Serializer.FileToObject(path) as PlatformerWorld;
        }
    }
}