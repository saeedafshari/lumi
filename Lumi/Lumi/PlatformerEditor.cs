using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Neat;
using Neat.Mathematics;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Console = Neat.Components.Console;
using Neat.Graphics;

namespace Lumi
{
    [Serializable]
    public class PlatformerEditor
    {
        PlatformerWorld World;

        public NeatGame Game { get { return World.Game; } }
        public Console Console { get { return Game.Console; } }

        public Vector2 SelectedPos1 { get; set; }
        public Vector2 SelectedPos2 { get; set; }

        public Color EditColor = Color.White;

        [NonSerialized]
        public bool Enabled = true;

        public enum EditorStates
        {
            Idle,
            SelectPos,
            Select,
            ModifyEntity,
            ModifyLight,
            SelectTexture,
            ModifyTexture,
            Polygon
        }

        [NonSerialized]
        EditorStates _state = EditorStates.Idle;
        public EditorStates State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                Console.Run("et_echo State> " + State.ToString());
            }
        }

        const float pickDistance = 32.0f;

        [NonSerialized]
        Entity selectedEntity = null;

        [NonSerialized]
        Light selectedLight = null;

        [NonSerialized]
        PlatformerTexture selectedTexture, candidTexture = null;

        [NonSerialized]
        bool revealTex = false;

        [NonSerialized]
        Polygon selectedMesh = null;

        [NonSerialized]
        int selectedVertexIndex = -1;

        float gridSize = 10;
        bool snapToGrid = true;
        bool showGrid = false;

        Vector2 mousePos { get { 
            var virtualPos = Game.MousePosition - World.ViewOffset;
            if (!snapToGrid) return virtualPos;

            virtualPos.X = (float)(Math.Round((double)(virtualPos.X / gridSize)) * gridSize);
            virtualPos.Y = (float)(Math.Round((double)(virtualPos.Y / gridSize)) * gridSize);
            return virtualPos;
        } }

        Vector2 textureUvMoveSpeed = new Vector2(0.01f);

        public PlatformerEditor(PlatformerWorld world)
        {
            this.World = world;
            AttachToConsole(Game.Console);

            InitializeSpawns();
        }

        public void Update(GameTime gameTime)
        {
            if (!Enabled) return;
        }

        public void HandleInput(GameTime gameTime)
        {
            if (!Enabled) return;
            if (Console.IsActive) return;

            Vector2 offsetMoveCoef = Vector2.One;
            if (Game.IsPressed(Keys.LeftShift, Keys.RightShift)) offsetMoveCoef *= 3;

            switch (State)
            {
                case EditorStates.SelectPos:
                    if (Game.IsMouseClicked()) SelectedPos1 = mousePos;
                    if (Game.IsRightMouseClicked()) SelectedPos2 = mousePos;

                    if (Game.IsPressed(Keys.LeftAlt, Keys.RightAlt))
                    {
                        var min = GeometryHelper.Min(SelectedPos1, SelectedPos2);
                        var max = GeometryHelper.Max(SelectedPos1, SelectedPos2);
                        if (Game.IsTapped(Keys.W))
                        {
                            Spawn("wall");
                        }
                        if (Game.IsTapped(Keys.P))
                        {
                            Spawn("platform");
                        }
                        if (Game.IsTapped(Keys.E))
                        {
                            Spawn("enemy");
                        }
                        if (Game.IsTapped(Keys.H))
                        {
                            Spawn("elevatorh");
                        }
                        if (Game.IsTapped(Keys.V))
                        {
                            Spawn("elevatorv");
                        }
                        if (Game.IsTapped(Keys.L))
                        {
                            Spawn("light");
                        }
                        if (Game.IsTapped(Keys.T))
                        {
                            Spawn("texture");
                            //State = EditorStates.ModifyTexture;
                        }
                    }
                    break;
                case EditorStates.Select:
                    if (Game.IsMouseClicked()) //LMouse = Entity
                    {
                        Entity e = null;
                        float dist = pickDistance + 1;

                        foreach (var item in World.Entities)
                        {
                            foreach (var vertex in item.Body.Mesh.Vertices)
                            {
                                float thistance = Vector2.Distance(vertex, mousePos);
                                if (thistance < dist)
                                {
                                    dist = thistance;
                                    e = item;
                                }
                            }
                        }

                        if (dist < pickDistance && e != null)
                        {
                            selectedEntity = e;
                            selectedMesh = e.Body.Mesh;
                            e.AttachToConsole(Console);
                            e.Body.AttachToConsole();
                            State = EditorStates.ModifyEntity;
                        }
                    }
                    if (Game.IsRightMouseClicked()) //RMouse = Light
                    {
                        Light l = null;
                        float dist = pickDistance + 1;

                        foreach (var item in World.Lights)
                        {
                            float thistance = Vector2.Distance(item.Position, mousePos);
                            if (thistance < dist)
                            {
                                dist = thistance;
                                l = item;
                            }
                        }

                        if (dist < pickDistance && l != null)
                        {
                            selectedLight = l;
                            l.AttachToConsole(Console);
                            State = EditorStates.ModifyLight;
                        }
                    }
                    break;
                case EditorStates.ModifyEntity:
                    if (selectedEntity == null) break;
                    if (Game.IsPressed(Keys.LeftAlt, Keys.RightAlt))
                    {
                        if (Game.IsTapped(Keys.Delete))
                        {
                            selectedEntity.CollectBody = true;
                            selectedMesh = null;
                            selectedEntity = null;
                            selectedVertexIndex = -1;
                            State = EditorStates.Select;
                        }
                        if (Game.IsMouseClicked())
                        {
                            var mesh = selectedEntity.Body.Mesh;
                            mesh.Offset(mousePos - mesh.GetPosition());
                        }
                        if (Game.IsRightMouseClicked())
                        {
                            var mesh = selectedEntity.Body.Mesh;
                            mesh.Resize(mousePos - mesh.GetPosition());
                        }
                        if (Game.IsTapped(Keys.L))
                        {
                            selectedEntity.DrawNoLight = !selectedEntity.DrawNoLight;
                        }
                        if (Game.IsTapped(Keys.S))
                        {
                            selectedEntity.DropShadow = !selectedEntity.DropShadow;
                        }
                    };
                    break;
                case EditorStates.ModifyLight:
                    if (selectedLight == null) break;
                    if (Game.IsPressed(Keys.LeftAlt, Keys.RightAlt))
                    {
                        if (Game.IsTapped(Keys.Delete))
                        {
                            World.Lights.Remove(selectedLight);
                            State = EditorStates.Select;
                        }
                        if (Game.IsMousePressed())
                        {
                            selectedLight.Position = mousePos;
                        }
                        if (Game.IsRightMousePressed())
                        {
                            selectedLight.Target = mousePos;
                        }
                    }
                    else
                    {
                        if (Game.IsMouseClicked())
                        {
                            var a = Vector2.Normalize(selectedLight.Target - selectedLight.Position);
                            var b = Vector2.Normalize(mousePos - selectedLight.Position);
                            selectedLight.Angle = (float)Math.Acos(Vector2.Dot(a, b));
                            /*selectedLight.Angle = MathHelper.PiOver2 - Vector2.Dot(
                                Vector2.Normalize(selectedLight.Target - selectedLight.Position),
                                Vector2.Normalize(mousePos - selectedLight.Position));*/
                        }
                        if (Game.IsRightMouseClicked())
                        {
                            selectedLight.Range = Vector2.Distance(mousePos, selectedLight.Position) /
                                (selectedLight.Target - selectedLight.Position).Length();
                        }
                    }
                    break;
                case EditorStates.SelectTexture:
                    revealTex = (Game.IsPressed(Keys.LeftShift, Keys.RightShift));
                    candidTexture = null;
                    for (int i = World.Textures.Count - 1; i >= 0; i--)
                    {
                        var item = World.Textures[i];
                        if (GeometryHelper.Vectors2Rectangle(item.Position, item.Size).Contains(
                            GeometryHelper.Vector2Point(mousePos)))
                        {
                            candidTexture = item;
                            break;
                        }
                    }

                    if (Game.IsMouseClicked())
                    {
                        if (candidTexture != null)
                        {
                            selectedTexture = candidTexture;
                            selectedTexture.AttachToConsole(Console);
                            State = EditorStates.ModifyTexture;
                        }
                    }
                    break;
                case EditorStates.ModifyTexture:
                    if (selectedTexture == null) break;
                    revealTex = (Game.IsPressed(Keys.LeftShift, Keys.RightShift));

                    if (Game.IsPressed(Keys.LeftAlt, Keys.RightAlt))
                    {
                        if (Game.IsTapped(Keys.Delete))
                        {
                            World.Textures.Remove(selectedTexture);
                            selectedTexture = null;
                            candidTexture = null;
                            State = EditorStates.SelectTexture;
                            break;
                        }
                        if (Game.IsMousePressed())
                        {
                            selectedTexture.Position = mousePos;
                        }
                        if (Game.IsRightMousePressed())
                        {
                            var min = GeometryHelper.Min(selectedTexture.Position, mousePos);
                            var max = GeometryHelper.Max(selectedTexture.Position, mousePos);
                            selectedTexture.Position = min;
                            selectedTexture.Size = max - min;
                        }
                        if (Game.IsTapped(Keys.X))
                            selectedTexture.WrapX = !selectedTexture.WrapX;
                        if (Game.IsTapped(Keys.Y))
                            selectedTexture.WrapY = !selectedTexture.WrapY;
                        if (Game.IsTapped(Keys.A))
                            Console.Run("tx_size auto");
                    }
                    else
                    {
                        if (Game.IsPressed(Keys.L))
                            selectedTexture.Uv += offsetMoveCoef * textureUvMoveSpeed * Vector2.UnitX;
                        if (Game.IsPressed(Keys.J))
                            selectedTexture.Uv -= offsetMoveCoef * textureUvMoveSpeed * Vector2.UnitX;
                        if (Game.IsPressed(Keys.K))
                            selectedTexture.Uv += offsetMoveCoef * textureUvMoveSpeed * Vector2.UnitY;
                        if (Game.IsPressed(Keys.I))
                            selectedTexture.Uv -= offsetMoveCoef * textureUvMoveSpeed * Vector2.UnitY;
                    }
                    break;
                case EditorStates.Polygon:
                    if (selectedEntity == null || selectedMesh == null)
                    {
                        Console.WriteLine("et_echo No Entity Selected");
                        State = EditorStates.Select;
                    }
                    if (Game.IsPressed(Keys.LeftAlt, Keys.RightAlt))
                    {
                        if (selectedVertexIndex >= 0)
                        {
                            if (Game.IsRightMouseClicked())
                            {
                                selectedMesh.Vertices[selectedVertexIndex] = mousePos;
                                selectedMesh.Triangulate();
                            }
                            if (Game.IsPressed(Keys.Delete) && selectedMesh.Vertices.Count > 3)
                            {
                                selectedMesh.Vertices.RemoveAt(selectedVertexIndex);
                                selectedMesh.Triangulate();
                                selectedVertexIndex = -1;
                            }
                        }
                        if (Game.IsMouseClicked())
                        {
                            int n = selectedMesh.Vertices.Count;
                            int selP = -1;
                            int selQ = -1;
                            var dist = pickDistance * pickDistance + 1;
                            Vector2 v = Vector2.Zero;
                            for (int p = n - 1, q = 0; q < n; p = q++)
                            {
                                var cv = GeometryHelper.GetShortestVectorToLine(
                                    new LineSegment(selectedMesh.Vertices[p], selectedMesh.Vertices[q]),
                                    mousePos);
                                var cvls = (mousePos - cv).LengthSquared();
                                if (cvls < dist)
                                {
                                    dist = cvls;
                                    selP = p;
                                    selQ = q;
                                    v = cv;
                                }
                            }

                            if (dist <= pickDistance * pickDistance && selP >= 0)
                            {
                                selectedMesh.Vertices.Insert(selQ, v);
                                selectedMesh.Triangulate();
                                selectedVertexIndex = selQ;
                            }
                        }
                    }
                    else
                    {
                        if (Game.IsMouseClicked())
                        {
                            float dist = pickDistance * pickDistance + 1;
                            int idx = -1;

                            for (int i = 0; i < selectedMesh.Vertices.Count; i++)
                            {
                                var d = (selectedMesh.Vertices[i] - mousePos).LengthSquared();
                                if (d < dist)
                                {
                                    dist = d;
                                    idx = i;
                                }
                            }

                            if (dist <= pickDistance * pickDistance && idx >= 0) selectedVertexIndex = idx;
                        }
                    }
                    break;
            }

            

            if (Game.IsPressed(Keys.NumPad4))
                World.ViewOffset += Vector2.UnitX * World.ViewOffsetMoveSpeed * offsetMoveCoef;
            if (Game.IsPressed(Keys.NumPad6))
                World.ViewOffset -= Vector2.UnitX * World.ViewOffsetMoveSpeed * offsetMoveCoef;
            if (Game.IsPressed(Keys.NumPad8))
                World.ViewOffset += Vector2.UnitY * World.ViewOffsetMoveSpeed * offsetMoveCoef;
            if (Game.IsPressed(Keys.NumPad2))
                World.ViewOffset -= Vector2.UnitY * World.ViewOffsetMoveSpeed * offsetMoveCoef;

            if (Game.IsTapped(Keys.F1)) State = EditorStates.Idle;
            if (Game.IsTapped(Keys.F2)) State = EditorStates.SelectPos;
            if (Game.IsTapped(Keys.F3)) State = EditorStates.Select;
            if (Game.IsTapped(Keys.F4)) State = EditorStates.ModifyEntity;
            if (Game.IsTapped(Keys.F5)) State = EditorStates.ModifyLight;
            if (Game.IsTapped(Keys.F6)) State = EditorStates.SelectTexture;
            if (Game.IsTapped(Keys.F7)) State = EditorStates.ModifyTexture;
            if (Game.IsTapped(Keys.F8)) State = EditorStates.Polygon;
            if (Game.IsTapped(Keys.Scroll)) World.FollowMode = !World.FollowMode;

            if (Game.IsPressed(Keys.LeftAlt, Keys.RightAlt))
            {
                if (Game.IsTapped(Keys.D0)) World.Paused = !World.Paused;
                if (Game.IsTapped(Keys.OemPlus,Keys.Add)) showGrid = !showGrid;
                if (Game.IsTapped(Keys.OemMinus,Keys.Subtract)) snapToGrid = !snapToGrid;
            }
        }

        public void Draw(GameTime gameTime)
        {
            var cross16x1 = Game.GetSlice("cross16x1");
            var cross16x2 = Game.GetSlice("cross16x2");
            var crossx = Game.GetSlice("crossx");
            var crosso = Game.GetSlice("crosso");
            var cross32 = Game.GetSlice("cross32");

            switch (State)
            {
                case EditorStates.SelectPos:
                    foreach (var item in World.Entities)
                    {
                        item.DoDrawBorders(gameTime, World.ViewOffset, Color.Gray);
                    }
                    Game.Write(GeometryHelper.Vector2String(SelectedPos1), SelectedPos1 + World.ViewOffset + new Vector2(16));
                    Game.Write(GeometryHelper.Vector2String(SelectedPos2), SelectedPos2 + World.ViewOffset + new Vector2(16));
                    Game.Write("[LMouse] Point1   [RMouse] Point2   [ALT+W] New Wall   [ALT+P] New Platform   [ALT+L] New Light   [ALT+T] New Texture", 
                        new Vector2(5, Game.GameHeight - 46));
                    break;
                case EditorStates.Select:
                    foreach (var item in World.Entities)
                    {
                        item.DoDrawBorders(gameTime, World.ViewOffset);
                        foreach (var vertex in item.Body.Mesh.Vertices)
                        {
                            Game.SpriteBatch.Draw(crossx.Texture, vertex + World.ViewOffset - crossx.Size / 2.0f, crossx.Crop, EditColor);
                        }
                    }
                    foreach (var item in World.Lights)
                    {
                        Game.SpriteBatch.Draw(crosso.Texture, item.Position + World.ViewOffset - crosso.Size / 2.0f, crosso.Crop, 
                            //(gameTime.ElapsedGameTime.Seconds % 2 == 0 ?
                            //new Color(EditColor.ToVector3() - item.Diffuse.ToVector3()) ://Negate Color
                            //item.Diffuse)
                            GraphicsHelper.GetRandomColor()
                            ); 
                    }
                    Game.Write("[LMouse] Select Entity   [RMouse] Select Light",
                        new Vector2(5, Game.GameHeight - 46));
                    break;
                case EditorStates.ModifyEntity:
                    if (selectedEntity == null) break;
                    foreach (var item in World.Entities)
                    {
                        item.DoDrawBorders(gameTime, World.ViewOffset, Color.Gray);
                    }
                    selectedEntity.DoDrawBorders(gameTime, World.ViewOffset, GraphicsHelper.GetRandomColor());
                    foreach (var vertex in selectedEntity.Body.Mesh.Vertices)
                    {
                        Game.SpriteBatch.Draw(crossx.Texture, vertex + World.ViewOffset - crossx.Size / 2.0f, crossx.Crop, EditColor);
                    }
                    Game.Write("Name: " + selectedEntity.Name +
                        "\nSprite: " + selectedEntity.Sprite +
                        "\nTint: " + GeometryHelper.Vector2String(selectedEntity.Tint.ToVector4()) +
                        "\nFlip: " + selectedEntity.Flip +
                        "\nClass: " + selectedEntity.GetClasses() +
                        "\nBorders: " + selectedEntity.DrawBorders + "   ~~~   DrawNoLight: " + selectedEntity.DrawNoLight + "   ~~~   Shadow: " + selectedEntity.DropShadow,
                        new Vector2(50));
                    Game.Write("[ALT+DEL] Delete   [ALT+LMouse] Move   [ALT+RMouse] Resize   [ALT+L] Toggle DrawNoLight   [ALT+S] Toggle Shadow",
                        new Vector2(5, Game.GameHeight - 46));
                    break;
                case EditorStates.ModifyLight:
                    if (selectedLight == null) break;
                    Game.Write("Falloff:" + selectedLight.Falloff +
                        "\nAng:" + selectedLight.Angle + " ~~~ " + selectedLight.AngleDeg +
                        "\nRange:" + selectedLight.Range +
                        "\nIntensity: " + selectedLight.Intensity +
                        "\nColor: " + GeometryHelper.Vector2String(selectedLight.Diffuse.ToVector4()),
                        new Vector2(50));
                    Game.SpriteBatch.Draw(crosso.Texture, selectedLight.Position + World.ViewOffset - crosso.Size / 2.0f, crosso.Crop,
                            GraphicsHelper.GetRandomColor());
                    Game.BasicLine.Draw(Game.SpriteBatch, selectedLight.Position, selectedLight.Target, EditColor, World.ViewOffset);
                    Game.Write("[ALT+DEL] Delete   [ALT+LMouse] Position   [ALT+RMouse] Target   [LMouse] Angle   [RMouse] Range",
                        new Vector2(5, Game.GameHeight - 46));
                    break;
                case EditorStates.SelectTexture:
                    if (candidTexture != null)
                    {
                        if (revealTex)
                            World.DrawTexture(candidTexture, gameTime);
                        Game.BasicLine.DrawRectangle(Game.SpriteBatch,
                            GeometryHelper.Vectors2Rectangle(candidTexture.Position, candidTexture.Size),
                            GraphicsHelper.GetRandomColor(),
                            World.ViewOffset);
                        Game.Write("Position: " + GeometryHelper.Vector2String(candidTexture.Position) + "   ~~~   Size: " + GeometryHelper.Vector2String(candidTexture.Size) +
                            "\nScrollRate: " + GeometryHelper.Vector2String(candidTexture.ScrollRate) +
                            "\nSprite: " + candidTexture.Sprite + "   ~~~   Depth: " + candidTexture.Depth +
                            "\nWrapX: " + candidTexture.WrapX + "   ~~~   WrapY: " + candidTexture.WrapY +
                            "\nTint: " + candidTexture.Tint,
                            new Vector2(50));
                        Game.Write("[LMouse] Select   [Shift] Reveal Texture",
                        new Vector2(5, Game.GameHeight - 46));
                    }
                    break;
                case EditorStates.ModifyTexture:
                    if (selectedTexture == null) break;
                    if (revealTex)
                        World.DrawTexture(selectedTexture, gameTime);
                    Game.BasicLine.DrawRectangle(Game.SpriteBatch,
                            GeometryHelper.Vectors2Rectangle(selectedTexture.Position, selectedTexture.Size),
                            GraphicsHelper.GetRandomColor(),
                            World.ViewOffset);
                    Game.Write("Position: " + GeometryHelper.Vector2String(selectedTexture.Position) + "   ~~~   Size: " + GeometryHelper.Vector2String(selectedTexture.Size) +
                            "\nScrollRate: " + GeometryHelper.Vector2String(selectedTexture.ScrollRate) +
                            "\nSprite: " + selectedTexture.Sprite + "   ~~~   Depth: " + selectedTexture.Depth +
                            "\nWrapX: " + selectedTexture.WrapX + "   ~~~   WrapY: " + selectedTexture.WrapY +
                            "\nTint: " + selectedTexture.Tint,
                            new Vector2(50));
                    Game.Write("[ALT+LMouse] Move   [ALT+RMouse] Resize   [ALT+DEL] Delete   [ALT+A] Auto Size   [ALT+X] Wrap X   [ALT+Y] Wrap Y   [Shift] Reveal Texture",
                        new Vector2(5, Game.GameHeight - 46));
                    break;
                case EditorStates.Polygon:
                    if (selectedMesh == null) break;
                    foreach (var item in World.Entities)
                    {
                        item.DoDrawBorders(gameTime, World.ViewOffset);
                        foreach (var vertex in item.Body.Mesh.Vertices)
                        {
                            Game.SpriteBatch.Draw(crossx.Texture, vertex + World.ViewOffset - crossx.Size / 2.0f, crossx.Crop, EditColor);
                        }
                    }
                    selectedMesh.Triangles.ForEach(t =>
                        t.Draw(Game.SpriteBatch, Game.BasicLine, World.ViewOffset, GraphicsHelper.GetRandomColor()));
                    if (selectedVertexIndex >= 0)
                        Game.SpriteBatch.Draw(crosso.Texture, 
                            selectedMesh.Vertices[selectedVertexIndex] + World.ViewOffset - crosso.Size / 2.0f, crosso.Crop, EditColor);
                    Game.Write("[LMouse] Select Vertex   [ALT+LMouse] Split   [ALT+RMouse] Move   [ALT+DEL] Delete",
                        new Vector2(5, Game.GameHeight - 46));
                    break;
            }
            Game.SpriteBatch.Draw(cross16x1.Texture, SelectedPos1 + World.ViewOffset - cross16x1.Size / 2.0f, cross16x1.Crop, EditColor);
            Game.SpriteBatch.Draw(cross16x2.Texture, SelectedPos2 + World.ViewOffset - cross16x2.Size / 2.0f, cross16x2.Crop, EditColor);

            if (showGrid)
            {
                var currentMouse = mousePos + World.ViewOffset;
                Color baseColor = new Color(0.6f,0.5f,0.5f,0.5f);
                float falloff = 0.91f;
                //left
                Vector4 lc = baseColor.ToVector4();
                for (float i = currentMouse.X; i >= 0; i-=gridSize, lc *= falloff)
                    Game.BasicLine.Draw(Game.SpriteBatch, new LineSegment(i, 0, i, Game.GameHeight), new Color(lc));

                //right
                lc = baseColor.ToVector4();
                for (float i = currentMouse.X; i <= Game.GameWidth; i += gridSize, lc *= falloff)
                    Game.BasicLine.Draw(Game.SpriteBatch, new LineSegment(i, 0, i, Game.GameHeight), new Color(lc));

                //up
                lc = baseColor.ToVector4();
                for (float i = currentMouse.Y; i >= 0; i -= gridSize, lc *= falloff)
                    Game.BasicLine.Draw(Game.SpriteBatch, new LineSegment(0, i, Game.GameWidth, i), new Color(lc));

                //down
                lc = baseColor.ToVector4();
                for (float i = currentMouse.Y; i <= Game.GameHeight; i += gridSize, lc *= falloff)
                    Game.BasicLine.Draw(Game.SpriteBatch, new LineSegment(0, i, Game.GameWidth, i), new Color(lc));
            }

            if (!World.FollowMode)
            {
                Game.BasicLine.Draw(Game.SpriteBatch, new LineSegment(0, Game.GameHeight / 2.0f, Game.GameWidth, Game.GameHeight / 2.0f), Color.DarkBlue);
                Game.BasicLine.Draw(Game.SpriteBatch, new LineSegment(Game.GameWidth / 2.0f, 0, Game.GameWidth / 2.0f, Game.GameHeight), Color.DarkBlue);
            }

            Game.SpriteBatch.Draw(cross32.Texture, mousePos + World.ViewOffset - cross32.Size / 2.0f, cross32.Crop, EditColor);

            Game.Write("[F1] Idle   [F2] SelectPos   [F3] Select   [F4] ModifyEntity   [F5] ModifyLight   [F6] SelectTexture   [F7] ModifyTexture   [F8] Polygon\n" +
                "[ScrollLock] Toggle Follow   [ALT+0] Pause   [ALT++] Toggle Grid   [ALT+-] Toggle Snap", new Vector2(5, Game.GameHeight - 32));
            Game.Write("Mode: " + State.ToString() +
                "\nViewPort: " + GeometryHelper.Vector2String(World.ViewOffset) + 
                "\nAB: " + GeometryHelper.Vector2String(SelectedPos2-SelectedPos1) +
                "\nCross: " + GeometryHelper.Vector2String(mousePos), new Vector2(Game.GameWidth - 200, Game.GameHeight - 56));
        }

        public void AttachToConsole(Console console)
        {
            console.AddCommand("ed_gridsize", o =>
                {
                    if (o.Count > 1) gridSize = float.Parse(o[1]);
                    else console.WriteLine(gridSize.ToString());
                });
            console.AddCommand("ed_snapgrid", o =>
                {
                    if (o.Count > 1) snapToGrid = bool.Parse(o[1]);
                    else console.WriteLine(snapToGrid.ToString());
                }, console.AutocompleteBoolean);
            console.AddCommand("ed_showgrid", o =>
                {
                    if (o.Count > 1) showGrid = bool.Parse(o[1]);
                    else console.WriteLine(showGrid.ToString());
                }, console.AutocompleteBoolean);
            console.AddCommand("ed_selectentity", o =>
                {
                    Entity se = World.Entities.First(e => e.Name.ToLower().Trim() == o[1].ToLower());
                    if (se != null) selectedEntity = se;
                },
                o => {
                    var l = new List<string>();
                    foreach (var item in World.Entities)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Name))
                            l.Add(item.Name);
                    }
                    return l;
                });
            console.AddCommand("ed_spawn", o =>
                {
                    if (o[1] != null) Spawn(o[1].ToLower());
                }, o =>
                {
                    return spawnTable.Keys.ToArray();
                });
            console.AddCommand("ed_color", o =>
                {
                    console.WriteLine(GeometryHelper.Vector2String(
                        o.Count == 1 ? EditColor.ToVector4() :
                        (EditColor = console.ParseColor(console.Args2Str(o, 1))).ToVector4()));
                }, console.AutocompleteColors);
            console.AddCommand("ed_pos1", o =>
                {
                    console.WriteLine(GeometryHelper.Vector2String(o.Count > 1 ?
                        SelectedPos1 = GeometryHelper.String2Vector(o[1]) : SelectedPos1));
                }, o =>
                {
                    return new[] { GeometryHelper.Vector2String(mousePos) };
                });
            console.AddCommand("ed_pos2", o =>
                {
                    console.WriteLine(GeometryHelper.Vector2String(o.Count > 1 ?
                        SelectedPos2 = GeometryHelper.String2Vector(o[1]) : SelectedPos2));
                }, o =>
                {
                    return new[] { GeometryHelper.Vector2String(mousePos) };
                });
        }

        Dictionary<string, Action> spawnTable = new Dictionary<string, Action>();
        Vector2 min { get { return GeometryHelper.Min(SelectedPos1, SelectedPos2); } }
        Vector2 max { get { return GeometryHelper.Max(SelectedPos1, SelectedPos2); } }

        void InitializeSpawns()
        {
            spawnTable.Add("banana", () => World.AddEntity(selectedEntity = new Banana(SelectedPos1)));
            spawnTable.Add("strawberry", () => World.AddEntity(selectedEntity = new Strawberry(SelectedPos1)));
            spawnTable.Add("cherry", () => World.AddEntity(selectedEntity = new Cherry(SelectedPos1)));
            spawnTable.Add("lollipop", () => World.AddEntity(selectedEntity = new Lollipop(SelectedPos1)));
            spawnTable.Add("stick", () => World.AddEntity(selectedEntity = new Stick(SelectedPos1)));
            spawnTable.Add("flash", () => World.AddEntity(selectedEntity = new Flash(SelectedPos1)));
            spawnTable.Add("tablet", () => World.AddEntity(selectedEntity = new Tablet(SelectedPos1)));
            spawnTable.Add("battery", () => World.AddEntity(selectedEntity = new Battery(SelectedPos1)));
            spawnTable.Add("wall", () => World.AddEntity(
                    selectedEntity = new Wall(Polygon.BuildRectangle(
                    GeometryHelper.Vectors2Rectangle(min, max - min)))));
            spawnTable.Add("platform", () => World.AddEntity(
                                selectedEntity = new Platform(Polygon.BuildRectangle(
                                GeometryHelper.Vectors2Rectangle(min, max - min)))));
            spawnTable.Add("enemy", () => World.AddEntity(selectedEntity = new Enemy(SelectedPos1)));
            spawnTable.Add("elevatorh", () => 
                {
                    World.AddEntity(selectedEntity = new ElevatorH(SelectedPos1));
                    (selectedEntity as ElevatorH).FlipTime =
                        new TimeSpan(0, 0, 0, 0,
                        (int)((SelectedPos2 - SelectedPos1).Length() / selectedEntity.Body.MaxSpeed.X));
                    if (SelectedPos2.X < SelectedPos1.X) (selectedEntity as Elevator).Direction *= -1;
                });
            spawnTable.Add("elevatorv", () =>
                {
                    World.AddEntity(selectedEntity = new ElevatorV(SelectedPos1));
                        (selectedEntity as ElevatorV).FlipTime =
                        new TimeSpan(0, 0, 0, 0,
                        (int)((SelectedPos2 - SelectedPos1).Length() / selectedEntity.Body.MaxSpeed.Y));
                    if (SelectedPos2.Y < SelectedPos1.Y) (selectedEntity as Elevator).Direction *= -1;
                });
            spawnTable.Add("light", () =>
                {
                    World.Lights.Add(selectedLight = new Light
                    {
                        Position = SelectedPos1,
                        Target = SelectedPos2,
                        Diffuse = Color.White,
                        Angle = MathHelper.ToRadians(90),
                        Falloff = 1f,
                        Intensity = 1f,
                        Range = 1f,
                        Enabled = true
                    });
                    selectedLight.AttachToConsole(Game.Console);
                });
            spawnTable.Add("texture", () =>
                {
                    World.Textures.Add(selectedTexture = new PlatformerTexture
                    {
                        Position = min,
                        Size = max - min,
                        Sprite = "solid",
                        Tint = Color.White,
                        WrapX = true,
                        WrapY = true,
                        ScrollRate = Vector2.One
                    });
                    World.SortTextures();
                    selectedTexture.AttachToConsole(Console);
                });
            spawnTable.Add("monster_yellowworm", () => World.AddEntity(selectedEntity = new YellowWorm(SelectedPos1)));
            spawnTable.Add("monster_white_bird", () => World.AddEntity(selectedEntity = new WhiteBird(SelectedPos1)));
            spawnTable.Add("monster_eye_monster", () => World.AddEntity(selectedEntity = new EyeMonster(SelectedPos1)));
            spawnTable.Add("monster_stickyu", () => World.AddEntity(selectedEntity = new StickyU(SelectedPos1)));
            spawnTable.Add("monster_stickyl", () => World.AddEntity(selectedEntity = new Sticky(SelectedPos1)));
            spawnTable.Add("monster_stickyr", () =>
                { 
                    World.AddEntity(selectedEntity = new Sticky(SelectedPos1));
                    selectedEntity.Body.GravityNormal *= -1;
                });
            spawnTable.Add("monster_roach", () => World.AddEntity(selectedEntity = new Roach(SelectedPos1)));
        }
        
        public void Spawn(string entityCode)
        {
            if (spawnTable.ContainsKey(entityCode))
                spawnTable[entityCode]();
            else
                Game.Console.WriteLine("Object " + entityCode + " not recognized.");
        }
    }
}
