using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Neat.Mathematics
{
    public static class GeometryHelper
    {
#if WINDOWS
        public static Point GetMousePosition()
        {
            return new Point(Mouse.GetState().X, Mouse.GetState().Y);
        }
        public static Rectangle GetMouseRectangle()
        {
            return new Rectangle(Mouse.GetState().X, Mouse.GetState().Y, 1, 1);
        }
#endif

        public static Vector2 GetIntersectionPoint(Vector2 line1_a, Vector2 line1_b, Vector2 line2_a, Vector2 line2_b)
        {
            LineSegment line1 = new LineSegment();
            line1.StartPos = line1_a;
            line1.EndPos = line1_b;

            LineSegment line2 = new LineSegment();
            line2.StartPos = line2_a;
            line2.EndPos = line2_b;

            return GetIntersectionPoint(line1, line2);
        }

        public static bool IsNaN(Vector2 v)
        {
            return float.IsNaN(v.X);
        }

        public static Vector2 GetIntersectionPoint(LineSegment firstLine, LineSegment secondLine)
        {
            return GetIntersectionPoint(firstLine, secondLine, true, true);
        }
        public static Vector2 GetIntersectionPoint(LineSegment firstLine, LineSegment secondLine, bool firstSegment, bool secondSegment)
        {
            double Ua, Ub;

            // Equations to determine whether lines intersect
            Ua = ((secondLine.EndPos.X - secondLine.StartPos.X) * (firstLine.StartPos.Y - secondLine.StartPos.Y) - (secondLine.EndPos.Y - secondLine.StartPos.Y) * (firstLine.StartPos.X - secondLine.StartPos.X)) /
                    ((secondLine.EndPos.Y - secondLine.StartPos.Y) * (firstLine.EndPos.X - firstLine.StartPos.X) - (secondLine.EndPos.X - secondLine.StartPos.X) * (firstLine.EndPos.Y - firstLine.StartPos.Y));

            Ub = ((firstLine.EndPos.X - firstLine.StartPos.X) * (firstLine.StartPos.Y - secondLine.StartPos.Y) - (firstLine.EndPos.Y - firstLine.StartPos.Y) * (firstLine.StartPos.X - secondLine.StartPos.X)) /
                    ((secondLine.EndPos.Y - secondLine.StartPos.Y) * (firstLine.EndPos.X - firstLine.StartPos.X) - (secondLine.EndPos.X - secondLine.StartPos.X) * (firstLine.EndPos.Y - firstLine.StartPos.Y));

            if (((Ua >= 0.0f && Ua <= 1.0f)||(!firstSegment)) && 
                ((Ub >= 0.0f && Ub <= 1.0f)||(!secondSegment)))
            {
                double x = firstLine.StartPos.X + Ua * (firstLine.EndPos.X - firstLine.StartPos.X);
                double y = firstLine.StartPos.Y + Ua * (firstLine.EndPos.Y - firstLine.StartPos.Y);

                return new Vector2((float)x, (float)y);
            }
            else
            {
                return new Vector2(float.NaN);
            }
        }

        public enum LineIntersectionStates
        {
            Intersect,
            Overlap,
            None
        }
        public static LineIntersectionStates LinesIntersect(LineSegment firstLine, LineSegment secondLine, bool firstSegment, bool secondSegment)
        {
            var ip = GetIntersectionPoint(firstLine, secondLine, firstSegment, secondSegment);

            if ((firstLine.Contains(secondLine.StartPos) && firstLine.Contains(secondLine.EndPos)) ||
                (secondLine.Contains(firstLine.StartPos) && secondLine.Contains(firstLine.EndPos)) ||
                (firstLine.Contains(secondLine.StartPos) && secondLine.Contains(firstLine.EndPos)) ||
                (firstLine.Contains(secondLine.EndPos) && secondLine.Contains(firstLine.StartPos)))
            {
                return LineIntersectionStates.Overlap;
            }
            
            if (IsNaN(ip)) return LineIntersectionStates.None;
            return LineIntersectionStates.Intersect;
        }

        public static Vector2 GetShortestVectorToLine(LineSegment line, Vector2 p)
        {
            return GetShortestVectorToLine(line, p, true);
        }
        public static Vector2 GetShortestVectorToLine(LineSegment line, Vector2 p, bool segment)
        {
            var u = ((p.X - line.StartPos.X) * (line.EndPos.X - line.StartPos.X) + (p.Y - line.StartPos.Y) * (line.EndPos.Y - line.StartPos.Y)) / line.LengthSquared();
            if (!segment || (u >= 0 && u <= 1))
                return new Vector2(
                    line.StartPos.X + u * (line.EndPos.X - line.StartPos.X),
                    line.StartPos.Y + u * (line.EndPos.Y - line.StartPos.Y));
            else
                return new Vector2(float.NaN);
        }

        public static Vector2 MoveInCircle(GameTime gameTime, float speed)
        {
            double time = gameTime.TotalGameTime.TotalSeconds * speed;

            float x = (float)Math.Cos(time);
            float y = (float)Math.Sin(time);
           
            return new Vector2(x, y);
        }

        public enum PointOnLineStates
        {
            PointIsNotOnTheInfiniteLine,
            PointIsNotOnTheOpenRayP,
            PointIsOnTheSegment,
            PointIsNotOnTheOpenRayQ
        }

        public static PointOnLineStates IsPointOnLine(LineSegment pq, Vector2 t)
        {
            return IsPointOnLine(ref pq.StartPos, ref pq.EndPos, ref t);
        }
        public static PointOnLineStates IsPointOnLine(Vector2 p, Vector2 q, Vector2 t)
        {
            return IsPointOnLine(ref p, ref q, ref t);
        }
        public static PointOnLineStates IsPointOnLine(ref Vector2 p, ref Vector2 q, ref Vector2 t)
        {
            if (Math.Abs((q.Y - p.Y) * (t.X - p.X) - (t.Y - p.Y) * (q.X - p.X)) >=
                Math.Max(Math.Abs(q.X - p.X), Math.Abs(q.Y - p.Y))) return PointOnLineStates.PointIsNotOnTheInfiniteLine;
            if ((q.X < p.X && p.X < t.X) || (q.Y < p.Y && p.Y < t.Y)) return PointOnLineStates.PointIsNotOnTheOpenRayP;
            if ((t.X < p.X && p.X < q.X) || (t.Y < p.Y && p.Y < q.Y)) return PointOnLineStates.PointIsNotOnTheOpenRayP;
            if ((p.X < q.X && q.X < t.X) || (p.Y < q.Y && q.Y < t.Y)) return PointOnLineStates.PointIsNotOnTheOpenRayQ;
            if ((t.X < q.X && q.X < p.X) || (t.Y < q.Y && q.Y < p.Y)) return PointOnLineStates.PointIsNotOnTheOpenRayQ;
            return PointOnLineStates.PointIsOnTheSegment;
        }

        public static Rectangle MoveRectangle(Rectangle r, Point offset)
        {
            return new Rectangle(r.X + offset.X, r.Y + offset.Y,
                r.Width, r.Height);
        }
        public static Rectangle MoveRectangle(Rectangle r, Vector2 offset)
        {
            return MoveRectangle(r, new Point((int)offset.X, (int)offset.Y));
        }

        public static bool IsVectorInCircle(Vector2 position, float radius, Vector2 vector)
        {
            return (position - vector).Length() < radius;
        }

        public static bool IsVectorInRectangle(Vector2 position, Vector2 size, Vector2 vector)
        {
            return 
                (vector.X > position.X) &&
                (vector.X < position.X + size.X) &&
                (vector.Y > position.Y) &&
                (vector.Y < position.Y + size.Y);
        }
        public static bool IsVectorInRectangle(Rectangle r, Vector2 vector)
        {
            return r.Contains(Vector2Point(vector));
        }

        //Deprecated
        public static bool IsVectorInside(Vector3 point, List<Vector3> polygon)
        {
            int i, j = 0;
            bool c = false;
            for (i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((((polygon[i].Y <= point.Y) && (point.Y < polygon[j].Y)) ||
                    ((polygon[j].Y <= point.Y) && (point.Y < polygon[i].Y))) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) /
                    (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                    c = !c;
            }
            return c;
        }

        public static Point Vector2Point(Vector2 vector)
        {
            return new Point((int)(vector.X), (int)(vector.Y));
        }
        public static Vector2 Point2Vector(Point point)
        {
            return new Vector2((float)(point.X), (float)(point.Y));
        }

        public static Rectangle Vectors2Rectangle(Vector2 position,Vector2 size)
        {
            return new Rectangle((int)(position.X), (int)(position.Y), (int)(size.X), (int)(size.Y));
        }
        public static Rectangle Points2Rectangle(Point position, Point size)
        {
            return new Rectangle((position.X), (position.Y), (size.X), (size.Y));
        }

        public static string Coords2String(Vector2 p)
        {
            return "(" + p.X.ToString() + "," + p.Y.ToString() + ")";
        }
        public static string Coords2String(Point  p)
        {
            return "(" + p.X.ToString() + "," + p.Y.ToString() + ")";
        }

        public static Vector2 String2Vector(string s)
        {
            switch (s.Trim().ToLower())
            {
                case ("zero") : { return Vector2.Zero; }
                case ("one")  : { return Vector2.One; }
                case ("unitx"): { return Vector2.UnitX; }
                case ("unity"): { return Vector2.UnitY;}
            }
            var elements = s.Split(',');
            Vector2 result = Vector2.Zero;
            result.X = float.Parse(elements[0].Trim());
            result.Y = float.Parse(elements[1].Trim());
            return result;
        }
        public static Vector3 String2Vector3(string s)
        {
            switch (s.Trim().ToLower())
            {
                case ("zero"): { return Vector3.Zero; }
                case ("one"): { return Vector3.One; }
                case ("unitx"): { return Vector3.UnitX; }
                case ("unity"): { return Vector3.UnitY; }
                case ("unitz"): { return Vector3.UnitZ; }
                case ("up"): { return Vector3.Up; }
                case ("down"): { return Vector3.Down; }
                case "forward": return Vector3.Forward;
                case "backward": return Vector3.Backward;
            }
            var elements = s.Split(',');
            Vector3 result = Vector3.Zero;
            result.X = float.Parse(elements[0].Trim());
            result.Y = float.Parse(elements[1].Trim());
            result.Z = float.Parse(elements[2].Trim());
            return result;
        }
        public static Vector4 String2Vector4(string s)
        {
            switch (s.Trim().ToLower())
            {
                case ("zero"): { return Vector4.Zero; }
                case ("one"): { return Vector4.One; }
                case ("unitx"): { return Vector4.UnitX; }
                case ("unity"): { return Vector4.UnitY; }
                case ("unitz"): { return Vector4.UnitZ; }
                case ("unitw"): { return Vector4.UnitW; }
            }
            var elements = s.Split(',');
            Vector4 result = Vector4.Zero;
            result.X = float.Parse(elements[0].Trim());
            result.Y = float.Parse(elements[1].Trim());
            result.Z = float.Parse(elements[2].Trim());
            result.W = float.Parse(elements[3].Trim());
            return result;
        }

        public static string Vector2String(Vector2 v)
        {
            return v.X.ToString() + "," + v.Y.ToString();
        }
        public static string Vector2String(Vector3 v)
        {
            return v.X.ToString() + "," + v.Y.ToString() + "," + v.Z.ToString();
        }
        public static string Vector2String(Vector4 v)
        {
            return v.X.ToString() + "," + v.Y.ToString() + "," + v.Z.ToString()+","+v.W;
        }
        
        public static string Point2String(Point p)
        {
            return Vector2String(Point2Vector(p));
        }
        public static Point String2Point(string s)
        {
            return Vector2Point(String2Vector(s));
        }

        public static float Min(params float[] nums)
        {
            float r = nums[0];
            foreach (var item in nums)
            {
                if (r > item) r = item;               
            }
            return r;
        }
        public static float Max(params float[] nums)
        {
            float r = 0;
            foreach (var item in nums)
            {
                if (r < item) r = item;
            }
            return r;
        }

        public static Vector2 Min(params Vector2[] vecs)
        {
            Vector2 v = vecs[0];
            foreach (var item in vecs)
            {
                if (v.X > item.X) v.X = item.X;
                if (v.Y > item.Y) v.Y = item.Y;
            }
            return v;
        }
        public static Vector2 Max(params Vector2[] vecs)
        {
            Vector2 v = vecs[0];
            foreach (var item in vecs)
            {
                if (v.X < item.X) v.X = item.X;
                if (v.Y < item.Y) v.Y = item.Y;
            }
            return v;
        }

        public static Vector2 Polar2Cart(float r, float t)
        {
            return new Vector2(
                r * (float)Math.Cos((double)t),
                r * (float)Math.Sin((double)t));
        }

        public static Vector2 Rotate2(Vector2 v, float a)
        {
            return new Vector2(
                (float)(v.X * Math.Cos(a) - v.Y * Math.Sin(a)),
                (float)(v.X * Math.Sin(a) + v.Y * Math.Cos(a)));
        }
    }
}
