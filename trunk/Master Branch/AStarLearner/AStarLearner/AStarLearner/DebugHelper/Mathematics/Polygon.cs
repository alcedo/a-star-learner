using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Polygon.cs
//Written by Saeed Afshari (www.saeedoo.com)

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
    public partial class Polygon
    {
        public enum PolygonCollisionClass
        {
            Rectangle,
            Convex,
            Concave
        }
        public List<Vector2> Vertices;
        
        const float _epsilon = 0.0000001f;
        public bool AutoTriangulate = false;

        //PolygonCollisionClass collisionClass = PolygonCollisionClass.Concave;
        //public PolygonCollisionClass CollisionClass { get { return collisionClass; } }

        private int n { get { return Vertices.Count; } }

        public Polygon()
        {
            Vertices = new List<Vector2>();
        }

        public Polygon(List<Vector2> vs)
        {
            Vertices = new List<Vector2>();
            foreach (var item in vs)
                Vertices.Add(item);
        }

        public Polygon(Polygon source)
        {
            Vertices = new List<Vector2>();
            foreach (var item in source.Vertices)
                Vertices.Add(item);
            if (source.Triangles != null)
            {
                Triangles = new List<Triangle>();
                foreach (var item in source.Triangles)
                    Triangles.Add(new Triangle(item));
            }
            AutoTriangulate = source.AutoTriangulate;
            //collisionClass = source.CollisionClass;
        }

        public Polygon(Polygon source, Vector2 offset)
        {
            Vertices = new List<Vector2>();
            foreach (var item in source.Vertices)
                Vertices.Add(item+offset);
            AutoTriangulate = source.AutoTriangulate;
            //collisionClass = source.CollisionClass;
            Triangulate();
        }

        public static Polygon BuildRectangle(Rectangle r)
        {
            return BuildRectangle(r.X, r.Y, r.Width, r.Height);
        }

        public static Polygon BuildRectangle(float x, float y, float width, float height)
        {
            Polygon result = new Polygon();// { collisionClass = PolygonCollisionClass.Rectangle };
            result.AddVertex(x, y);
            result.AddVertex(x + width, y);
            result.AddVertex(x + width, y + height);
            result.AddVertex(x, y + height);
            return result;
        }

        public static Polygon BuildCircle(int vertices, Vector2 center, float radius)
        {
            Polygon result = new Polygon();// { collisionClass = PolygonCollisionClass.Convex };
            for (double i = 0; i < MathHelper.TwoPi; i += (float)MathHelper.TwoPi / (float)vertices)
            {
                result.AddVertex(center + radius * new Vector2((float)(Math.Cos(i)), (float)(Math.Sin(i))));
            }
            return result;
        }

        public void AddVertex(Vector2 v)
        {
            Vertices.Add(v);
            if (AutoTriangulate) Triangulate();
        }

        public void AddVertex(float x, float y)
        {
            AddVertex(new Vector2(x, y));
            if (AutoTriangulate) Triangulate();
        }

        public Polygon Offset(Vector2 amount)
        {
            for (int i = 0; i < n; i++)
                Vertices[i] += amount;
            if (AutoTriangulate && Triangles != null)
            {
                if (Triangles != null)
                    for (int i = 0; i < Triangles.Count; i++)
                    {
                        Triangles[i].Offset(amount);
                    }
                else Triangulate();
            }
            return this;
        }

        public Polygon Scale(Vector2 amount)
        {
            for (int i = 0; i < n; i++)
                Vertices[i] *= amount;
            if (AutoTriangulate && Triangles != null)
            {
                if (Triangles != null)
                    for (int i = 0; i < Triangles.Count; i++)
                    {
                        Triangles[i].Scale(amount);
                    }
                else Triangulate();
            }
            return this;
        }
        public Vector2 GetPosition()
        {
            Vector2 pos = new Vector2(float.MaxValue, float.MaxValue);
            foreach (var item in Vertices)
            {
                if (pos.X > item.X) pos.X = item.X;
                if (pos.Y > item.Y) pos.Y = item.Y;
            }
            return pos;
        }

        public Rectangle GetBoundingRect()
        {
            Vector2 pos, size;
            GetPositionAndSize(out pos, out size);
            return
                //Geometry2D.Vectors2Rectangle(pos, size);
                new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
        }

        public void GetPositionAndSize(out Vector2 pos, out Vector2 size)
        {
            pos = GetPosition();
            Vector2 big = new Vector2(float.MinValue, float.MinValue);
            foreach (var item in Vertices)
            {
                if (big.X < item.X) big.X = item.X;
                if (big.Y < item.Y) big.Y = item.Y;
            }
            size = big - pos;
        }

        public List<LineSegment> GetEdges()
        {
            List<LineSegment> result = new List<LineSegment>();
            for (int p = n - 1, q = 0; q < n; p = q++) result.Add(new LineSegment(Vertices[p], Vertices[q]));
            return result;
        }

        public float GetArea()
        {
            float a = 0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
                a += Vertices[p].X * Vertices[q].Y - Vertices[q].X * Vertices[p].Y; //Outer Product
            return (a * 0.5f);
        }

        public List<Vector2> GetVerticesCounterClockwise()
        {
            if (GetArea() < 0f || Vertices.Count == 1) return Vertices;
            else
            {
                var V = new List<Vector2>();
                for (int i = n - 1; i >= 0; i--)
                    V.Add(Vertices[i]);
                return V;
            }
        }

        public List<Vector2> GetVerticesClockwise()
        {
            if (GetArea() > 0f || Vertices.Count == 1) return Vertices;
            else
            {
                var V = new List<Vector2>();
                for (int i = n - 1; i >= 0; i--)
                    V.Add(Vertices[i]);
                return V;
            }
        }
    }
}
