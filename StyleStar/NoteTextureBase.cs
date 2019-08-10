using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace StyleStar
{
    public abstract class NoteTextureBase
    {
        protected bool IsLoaded { get; set; }
        protected Note parent;
        protected Note prevNote;

        protected VertexPositionNormalTexture[] verts;
        protected BasicEffect effect;
        protected Texture2D texture;

        protected float z = 0f;

        public NoteTextureBase(Note _parent)
        {
            parent = _parent;
        }

        public NoteTextureBase(Note _parent, Note _prevNote)
        {
            parent = _parent;
            prevNote = _prevNote;
        }

        public virtual void Draw(double currentBeat, Matrix view, Matrix projection)
        {
            effect.View = view;
            effect.Projection = projection;
            effect.TextureEnabled = true;
            effect.Texture = texture;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Globals.GraphicsManager.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    verts,
                    0,
                    2);
            }
        }

        protected void SetVerts(float x1l, float x1r, float y1, float x2l, float x2r, float y2)
        {
            SetVerts(x1l, x1r, y1, x2l, x2r, y2, z);
        }

        protected void SetVerts(float x1l, float x1r, float y1, float x2l, float x2r, float y2, float z)
        {
            if (verts == null)
                GenerateVerts(x1l, x1r, y1, x2l, x2r, y2, z);
            else
            {
                //verts[0].Position.X = x1r;
                //verts[0].Position.Y = y1 + (float)Globals.YOffset;
                //verts[0].Position.Z = z;

                //verts[1].Position.X = x2r;
                //verts[1].Position.Y = y2 + (float)Globals.YOffset;
                //verts[1].Position.Z = z;

                //verts[2].Position.X = x1l;
                //verts[2].Position.Y = y1 + (float)Globals.YOffset;
                //verts[2].Position.Z = z;

                //verts[3].Position = verts[1].Position;

                //verts[4].Position.X = x2l;
                //verts[4].Position.Y = y2 + (float)Globals.YOffset;
                //verts[4].Position.Z = z;

                //verts[5].Position = verts[2].Position;

                verts[0].Position.X = x2l;
                verts[0].Position.Y = y2 + (float)Globals.YOffset;
                verts[0].Position.Z = z;

                verts[1].Position.X = x1l;
                verts[1].Position.Y = y1 + (float)Globals.YOffset;
                verts[1].Position.Z = z;

                verts[2].Position.X = x2r;
                verts[2].Position.Y = y2 + (float)Globals.YOffset;
                verts[2].Position.Z = z;

                verts[3].Position = verts[1].Position;

                verts[4].Position.X = x1r;
                verts[4].Position.Y = y1 + (float)Globals.YOffset;
                verts[4].Position.Z = z;

                verts[5].Position = verts[2].Position;
            }
        }

        private void GenerateVerts(float x1l, float x1r, float y1, float x2l, float x2r, float y2, float z)
        {
            verts = new VertexPositionNormalTexture[6];

            //verts[0].Position = new Vector3(x1r, y1 + (float)Globals.YOffset, z);
            //verts[1].Position = new Vector3(x2r, y2 + (float)Globals.YOffset, z);
            //verts[2].Position = new Vector3(x1l, y1 + (float)Globals.YOffset, z);

            //verts[3].Position = verts[1].Position;
            //verts[4].Position = new Vector3(x2l, y2 + (float)Globals.YOffset, z);
            //verts[5].Position = verts[2].Position;

            verts[0].Position = new Vector3(x2l, y2 + (float)Globals.YOffset, z);
            verts[1].Position = new Vector3(x1l, y1 + (float)Globals.YOffset, z);
            verts[2].Position = new Vector3(x2r, y2 + (float)Globals.YOffset, z);

            verts[3].Position = verts[1].Position;
            verts[4].Position = new Vector3(x1r, y1 + (float)Globals.YOffset, z);
            verts[5].Position = verts[2].Position;

            int repetitions = 1;

            //verts[0].TextureCoordinate = new Vector2(0, 0);
            //verts[1].TextureCoordinate = new Vector2(0, repetitions);
            //verts[2].TextureCoordinate = new Vector2(repetitions, 0);

            //verts[3].TextureCoordinate = verts[1].TextureCoordinate;
            //verts[4].TextureCoordinate = new Vector2(repetitions, repetitions);
            //verts[5].TextureCoordinate = verts[2].TextureCoordinate;

            verts[0].TextureCoordinate = new Vector2(0, repetitions);
            verts[1].TextureCoordinate = new Vector2(0, 0);
            verts[2].TextureCoordinate = new Vector2(repetitions, repetitions);

            verts[3].TextureCoordinate = verts[1].TextureCoordinate;
            verts[4].TextureCoordinate = new Vector2(repetitions, 0);
            verts[5].TextureCoordinate = verts[2].TextureCoordinate;

            //effect = new BasicEffect(Globals.GraphicsManager.GraphicsDevice);
            effect = Globals.Effect;
        }
    }
}
