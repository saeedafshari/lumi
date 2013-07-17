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
using Console = Neat.Components.Console;

namespace Lumi
{
    public partial class Entity
    {
        [NonSerialized]
        protected Console console;

        public virtual void AttachToConsole(Console c=null)
        {
            if (c == null && console == null) return;
            else if (c != null) console = c;

            console.AddCommand("et_sprite", et_sprite, console.AutocompleteSprites);
            console.AddCommand("et_flip", et_flip, console.AutocompleteBoolean);
            console.AddCommand("et_flipy", o => console.GetSetAction(o, ref _flipy), console.AutocompleteBoolean);
            console.AddCommand("et_collect", et_collect, console.AutocompleteBoolean);
            console.AddCommand("et_class", et_class);
            console.AddCommand("et_unclass", o =>
                {
                    console.WriteLine(EntityClass.Remove(o[1]).ToString());
                }, o =>
                {
                    return EntityClass.ToArray();
                });
            console.AddCommand("et_clearclasses", o => EntityClass.Clear());
            console.AddCommand("et_tint", et_tint, console.AutocompleteColors);
            console.AddCommand("et_drawborders", et_drawborders, console.AutocompleteBoolean);
            console.AddCommand("et_tag", et_tag);
            console.AddCommand("et_selbody", et_selbody);
            console.AddCommand("et_deathtimer", et_deathtimer);
            console.AddCommand("et_dead", et_dead, console.AutocompleteBoolean);
            console.AddCommand("et_name", o => Name = o[1]);
            console.AddCommand("et_drawnolight", o => DrawNoLight = bool.Parse(o[1]), console.AutocompleteBoolean);
            console.AddCommand("et_dropshadow", o => DropShadow = bool.Parse(o[1]), console.AutocompleteBoolean);
            console.AddCommand("et_depth", o => Depth = float.Parse(o[1]));
            Body.AttachToConsole();
        }

        public string GetClasses()
        {
            string s = "";
            foreach (var item in EntityClass)
                s += (item + " ");
            return s;
        }

        void et_sprite(IList<string> args)
        {
            Sprite = args[1];
        }

        void et_flip(IList<string> args)
        {
            Flip = bool.Parse(args[1]);
        }

        void et_collect(IList<string> args)
        {
            CollectBody = bool.Parse(args[1]);
        }

        void et_class(IList<string> args)
        {
            if (args.Count == 1)
            {
                foreach (var item in EntityClass)
                    console.Write(item + " ");
                console.WriteLine();
            }
            else
            {
                for (int i = 1; i < args.Count; i++)
                {
                    console.WriteLine(EntityClass.Add(args[i]).ToString());    
                }
            }
        }

        void et_tint(IList<string> args)
        {
            Tint = console.ParseColor(console.Args2Str(args, 1));
        }

        void et_drawborders(IList<string> args)
        {
            DrawBorders = bool.Parse(args[1]);
        }

        void et_tag(IList<string> args)
        {
            Tag = console.Args2Str(args, 1);
        }

        void et_selbody(IList<string> args)
        {
            Body.AttachToConsole();
        }

        void et_deathtimer(IList<string> args)
        {
            DeathTimer = new TimeSpan(0, 0, 0, 0, int.Parse(args[1]));
        }

        void et_dead(IList<string> args)
        {
            Dead = bool.Parse(args[1]);
        }
    }
}