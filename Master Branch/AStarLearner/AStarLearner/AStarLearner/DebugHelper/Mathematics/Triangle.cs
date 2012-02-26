using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#if LIVE
using Microsoft.Xna.Framework.GamerServices;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Neat.Mathematics
{
    public class Triangle
    {
        public Vector2 A;
        public Vector2 B;
        public Vector2 C;

        public Triangle()
        {
        }

        public Triangle(Triangle t)
        {
            A = t.A;
            B = t.B;
            C = t.C;
        }

        public Triangle(Polygon p)
        {
            if (p.Vertices.Count > 1)
            {
                A = p.Vertices[0];
                if (p.Vertices.Count > 2)
                {
                    B = p.Vertices[1];
                    if (p.Vertices.Count > 3)
                    {
                        C = p.Vertices[2];
                    }
                }
            }
        }

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Triangle(float xa, float ya, float xb, float yb, float xc, float yc)
        {
            A = new Vector2(xa, ya);
            B = new Vector2(xb, yb);
            C = new Vector2(xc, yc);
        }

        public Triangle Offset(Vector2 amount)
        {
            A += amount;
            B += amount;
            C += amount;
            return this;
        }

        public Triangle Scale(Vector2 amount)
        {
            A *= amount;
            B *= amount;
            C *= amount;
            return this;
        }

        public Vector2[] GetVerticesClockwise()
        {
            if (GetArea() < 0) return new Vector2[3] { A, B, C };
            else return new Vector2[3] { C, B, A };
        }

        public Vector2[] GetVerticesCounterClockwise()
        {
            if (GetArea() > 0) return new Vector2[3] { A, B, C };
            else return new Vector2[3] { C, B, A };
        }

        public float GetArea()
        {
            return
                ((A.X * B.Y - B.X * A.Y) +
                (B.X * C.Y - C.X * B.Y) +
                (C.X * A.Y - A.X * C.Y)) * 0.5f;
        }

        public bool IsInside(Vector2 point)
        {
            Vector2 a = new Vector2(C.X - B.X, C.Y - B.Y);
            Vector2 b = new Vector2(A.X - C.X, A.Y - C.Y);
            Vector2 c = new Vector2(B.X - A.X, B.Y - A.Y);
            Vector2 ap = new Vector2(point.X - A.X, point.Y - A.Y);
            Vector2 bp = new Vector2(point.X - B.X, point.Y - B.Y);
            Vector2 cp = new Vector2(point.X - C.X, point.Y - C.Y);

            var aCbp = a.X * bp.Y - a.Y * bp.X;
            var cCap = c.X * ap.Y - c.Y * ap.X;
            var bCcp = b.X * cp.Y - b.Y * cp.X;

            return ((aCbp >= 0f) && (bCcp >= 0f) && (cCap >= 0f));
        }

        public Polygon ToPolygon()
        {
            Polygon p = new Polygon();
            p.Vertices.Add(A);
            p.Vertices.Add(B);
            p.Vertices.Add(C);
            return p;
        }

        public void Draw(SpriteBatch spriteBatch, Graphics.LineBrush lineBrush, Color color)
        {
            Draw(spriteBatch, lineBrush, Vector2.Zero, color);
        }

        public void Draw(SpriteBatch spriteBatch, Graphics.LineBrush lineBrush, Vector2 offset, Color color)
        {
            lineBrush.Draw(spriteBatch, A + offset, B + offset, color);
            lineBrush.Draw(spriteBatch, B + offset, C + offset, color);
            lineBrush.Draw(spriteBatch, C + offset, A + offset, color);
        }
    }
}