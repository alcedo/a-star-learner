﻿using System;
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
using Neat.Graphics;

namespace Neat.Mathematics
{
    public class LineSegment
    {
        public Vector2 StartPos;
        public Vector2 EndPos;

        public LineSegment()
        {
            StartPos = Vector2.Zero;
            EndPos = Vector2.One;
        }

        public LineSegment(Vector2 v1, Vector2 v2)
        {
            StartPos = v1;
            EndPos = v2;
        }

        public LineSegment(Vector2 position, float angle)
        {
            StartPos = position;
            EndPos = position + Vector2.UnitX;
            Turn(angle);
        }

        public LineSegment(float ax, float ay, float bx, float by)
        {
            StartPos = new Vector2(ax, ay);
            EndPos = new Vector2(bx, by);
        }

        public LineSegment(LineSegment l)
        {
            StartPos = l.StartPos;
            EndPos = l.EndPos;
        }

        public static LineSegment operator +(LineSegment l1, LineSegment l2)
        {
            return new LineSegment(l1.StartPos + l2.StartPos, l1.EndPos + l2.EndPos);
        }

        public static LineSegment operator -(LineSegment l1, LineSegment l2)
        {
            return new LineSegment(l1.StartPos - l2.StartPos, l1.EndPos - l2.EndPos);
        }

        public void Translate(Vector2 v)
        {
            StartPos += v;
            EndPos += v;
        }

        public void Turn(float alpha)
        {
            EndPos = Vector2.Transform(ToVector2(), Matrix.CreateRotationZ(alpha)) + StartPos;
        }

        public void Forward(float distance)
        {
            Translate(Vector2.Normalize(EndPos - StartPos) * distance);
        }

        public void Strafe(float distance)
        {
            Vector2 n = Vector2.Normalize(EndPos - StartPos);
            Translate(new Vector2(-n.Y * distance, n.X * distance));
        }

        public float Length()
        {
            return ToVector2().Length();
        }

        public float LengthSquared()
        {
            return ToVector2().LengthSquared();
        }

        public Vector2 ToVector2()
        {
            return EndPos - StartPos;
        }

        public Vector2 GetIntersectionPoint(LineSegment l2)
        {
            return GeometryHelper.GetIntersectionPoint(this, l2);
        }

        public void Draw(SpriteBatch sb, LineBrush lb, Color color)
        {
            lb.Draw(sb, StartPos, EndPos, color);
        }

        public void Draw(SpriteBatch sb, LineBrush lb, Vector2 offset, Color color)
        {
            lb.Draw(sb, StartPos + offset, EndPos + offset, color);
        }

        public bool Contains(Vector2 p)
        {
            return GeometryHelper.IsPointOnLine(this, p) == GeometryHelper.PointOnLineStates.PointIsOnTheSegment;
        }

        public GeometryHelper.LineIntersectionStates Intersects(LineSegment l2)
        {
            return GeometryHelper.LinesIntersect(this, l2, true, true);
        }
    }
}
