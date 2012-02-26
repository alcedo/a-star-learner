using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Polygon_SAT.cs
//Written by Saeed Afshari (www.saeedoo.com)

//Based on Olivier Renault's implementation of
//the Separating axis theorem (SAT).

//Can be used for Collision Detection and Resolution,
//This version can only handle slow-moving polygons.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#if LIVE
using Microsoft.Xna.Framework.GamerServices;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace Neat.Mathematics
{
    public partial class Polygon
    {
        public void GetInterval(Vector2 axis, out float min, out float max)
        {
            min = max = (Vector2.Dot(Vertices[0], axis));
            for (int i = 1; i < Vertices.Count; i++)
            {
                float d = Vector2.Dot(Vertices[i], axis);
                if (d < min) min = d;
                else if (d > max) max = d;
            }
        }

        public static bool IntervalIntersect(Polygon A, Polygon B, Vector2 axis, out Vector2 push)
        {
            push = Vector2.Zero;
            float min0 = 0f, max0 = 0f,
                min1 = 0f, max1 = 0f;
            A.GetInterval(axis, out min0, out max0);
            B.GetInterval(axis, out min1, out max1);

            if (min0 > max1 || min1 > max0) return false;

            float d0 = max0 - min1;
            float d1 = max1 - min0;
            float d = d0 < d1 ? d0 : d1;
            push = axis * d / axis.LengthSquared();
            return true;
        }

        public static bool TrianglesCollide(Polygon A, Polygon B, out Vector2 MTD)
        {
            MTD = Vector2.Zero;
            if (A.Triangles == null) A.Triangulate();
            if (B.Triangles == null) B.Triangulate();

            foreach (var triA in A.Triangles)
            {
                Polygon pA = triA.ToPolygon();
                foreach (var triB in B.Triangles)
                {
                    if (Polygon.Collide(pA, triB.ToPolygon(), out MTD))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TrianglesCollide(Polygon A, Polygon B)
        {
            Vector2 MTD;
            return TrianglesCollide(A, B, out MTD);
        }

        public static bool RectanglesCollide(Polygon A, Polygon B, out Vector2 MTD)
        {
            MTD = Vector2.Zero;
            Rectangle rA = A.GetBoundingRect();
            Rectangle rB = B.GetBoundingRect();
            Rectangle rC = Rectangle.Intersect(rA, rB);
            if (rC == Rectangle.Empty) return false;
            if (rC.Width > rC.Height)
            {
                //DO MTD.Y
                MTD.Y = rC.Height;
                if (rC.Y - rA.Y < 0) MTD.Y *= -1;
            }
            else
            {
                //DO MTD.X
                MTD.X = rC.Width;
                if (rC.X - rA.X < 0) MTD.X *= -1;
            }
            return true;
        }

        public static bool RectanglesCollide(Polygon A, Polygon B)
        {
            Rectangle rA = A.GetBoundingRect();
            Rectangle rB = B.GetBoundingRect();
            Rectangle rC = Rectangle.Intersect(rA, rB);
            return (rC != Rectangle.Empty);
        }

        public static bool Collide(Polygon A, Polygon B, out Vector2 MTD)
        {
            MTD = Vector2.Zero;
            int n = 0;
            if (A == null || B == null) return false;

            float mind2 = 0;

            //test separation axes of A
            for (int j = A.Vertices.Count - 1, i = 0; i < A.Vertices.Count; j = i, i++)
            {
                Vector2 E = A.Vertices[i] - A.Vertices[j];
                Vector2 N = new Vector2(-E.Y, E.X);
                Vector2 p;
                if (!IntervalIntersect(A, B, N, out p)) return false;
                float n2 = p.LengthSquared();
                if (n2 < mind2 || n == 0) { MTD = p; mind2 = n2; }
                n++;
            }
            //test separation axes of B
            for (int j = B.Vertices.Count - 1, i = 0; i < B.Vertices.Count; j = i, i++, n++)
            {
                Vector2 E = B.Vertices[i] - B.Vertices[j];
                Vector2 N = new Vector2(-E.Y, E.X);
                Vector2 p;
                if (!IntervalIntersect(A, B, N, out p)) return false;
                float n2 = p.LengthSquared();
                if (n2 < mind2 || n == 0) { MTD = p; mind2 = n2; }
            }

            if (Vector2.Dot(A.Vertices[0] - B.Vertices[0], MTD) < 0.0f) MTD *= -1;

            return true;
        }

        public static bool Collide(Polygon A, Polygon B)
        {
            Vector2 MTD;
            return Collide(A, B, out MTD);
        }
    }
}