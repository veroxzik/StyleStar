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
    public class QuadTexture
    {
        protected VertexPositionNormalTexture[] verts;
        protected Texture2D texture;

        public QuadTexture(Texture2D _texture)
        {
            texture = _texture;
            verts = new VertexPositionNormalTexture[6];
        }


        public virtual void Draw(Matrix view, Matrix projection)
        {
            Globals.Effect.View = view;
            Globals.Effect.Projection = projection;
            Globals.Effect.TextureEnabled = true;
            Globals.Effect.Texture = texture;

            foreach (var pass in Globals.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Globals.GraphicsManager.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    verts,
                    0,
                    2);
            }
        }

        public void SetVerts(float xl, float xr, float y1, float y2)
        {
            SetVerts(xl, xr, y1, xl, xr, y2, 0);
        }

        public void SetVerts(float x1l, float x1r, float y1, float x2l, float x2r, float y2)
        {
            SetVerts(x1l, x1r, y1, x2l, x2r, y2, 0);
        }

        public void SetVerts(float xl, float xr, float y1, float y2, float z)
        {
            SetVerts(xl, xr, y1, xl, xr, y2, z);
        }

        public void SetVerts(float x1l, float x1r, float y1, float x2l, float x2r, float y2, float z)
        {
            if(verts == null)
                verts = new VertexPositionNormalTexture[6];

            verts[0].Position = new Vector3(x1r, y1 + (float)Globals.YOffset, z);
            verts[1].Position = new Vector3(x2r, y2 + (float)Globals.YOffset, z);
            verts[2].Position = new Vector3(x1l, y1 + (float)Globals.YOffset, z);

            verts[3].Position = verts[1].Position;
            verts[4].Position = new Vector3(x2l, y2 + (float)Globals.YOffset, z);
            verts[5].Position = verts[2].Position;

            int repetitions = 1;

            verts[0].TextureCoordinate = new Vector2(0, repetitions);
            verts[1].TextureCoordinate = new Vector2(0, 0);
            verts[2].TextureCoordinate = new Vector2(repetitions, repetitions);

            verts[3].TextureCoordinate = verts[1].TextureCoordinate;
            verts[4].TextureCoordinate = new Vector2(repetitions, 0);
            verts[5].TextureCoordinate = verts[2].TextureCoordinate;
        }

        //public void SetVerts(Point ll, Point lr, Point ul, Point ur)
        //{
        //    if (verts == null)
        //        verts = new VertexPositionNormalTexture[6];

        //    verts[0].Position = new Vector3(lr.X, y1 + (float)Globals.YOffset, 0);
        //    verts[1].Position = new Vector3(x2r, y2 + (float)Globals.YOffset, 0);
        //    verts[2].Position = new Vector3(x1l, y1 + (float)Globals.YOffset, 0);

        //    verts[3].Position = verts[1].Position;
        //    verts[4].Position = new Vector3(x2l, y2 + (float)Globals.YOffset, 0);
        //    verts[5].Position = verts[2].Position;

        //    int repetitions = 1;

        //    verts[0].TextureCoordinate = new Vector2(0, repetitions);
        //    verts[1].TextureCoordinate = new Vector2(0, 0);
        //    verts[2].TextureCoordinate = new Vector2(repetitions, repetitions);

        //    verts[3].TextureCoordinate = verts[1].TextureCoordinate;
        //    verts[4].TextureCoordinate = new Vector2(repetitions, 0);
        //    verts[5].TextureCoordinate = verts[2].TextureCoordinate;
        //}
    }
}
