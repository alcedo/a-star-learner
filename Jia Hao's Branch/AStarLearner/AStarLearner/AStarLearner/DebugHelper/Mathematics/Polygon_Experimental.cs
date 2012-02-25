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
    public partial class Polygon
    {

        #region SAT
        public static bool IntervalIntersect(Polygon A, Polygon B, Vector2 VelA, Vector2 VelB,
           Vector2 axis, out float taxis, float tmax)
        {
            taxis = 0.0f;
            float min0, max0, min1, max1;

            Vector2 xVel = -VelB + VelA;

            A.GetInterval(axis, out min0, out max0);
            B.GetInterval(axis, out min1, out max1);

            float d0 = min0 - max1;
            float d1 = min1 - max0;

            if (min0 > max1 || min1 > max0)
            {
                float v = Vector2.Dot(xVel, axis);
                if (Math.Abs(v) < _epsilon)
                    return false;

                float t0 = -d0 / v;
                float t1 = d1 / v;
                if (t0 > t1) { float temp = t0; t0 = t1; t1 = temp; }

                taxis = (t0 > 0.0f) ? t0 : t1;

                return !(taxis < 0.0f || taxis > tmax);
            }
            else
            {
                // overlap. get the interval, as a the smallest of |d0| and |d1|
                // return negative number to mark it as an overlap
                taxis = (d0 > d1) ? d0 : d1;
                return true;
            }
        }

        public static bool FindMTD(List<Vector2> xAxis, List<float> taxis, out Vector2 N, ref float t)
        {
            // find collision first
            int mini = -1;
            t = 0.0f;
            N = Vector2.Zero;

            for (int i = 0; i < xAxis.Count; i++)
            {
                if (taxis[i] > 0)
                {
                    if (taxis[i] > t)
                    {
                        mini = i;
                        t = taxis[i];
                        N = xAxis[i];
                        N.Normalize();
                    }
                }
            }

            // found one
            if (mini != -1)
                return true;

            // nope, find overlaps
            mini = -1;
            for (int i = 0; i < xAxis.Count; i++)
            {
                float n = xAxis[i].Length();
                xAxis[i].Normalize();
                taxis[i] /= n;

                if (taxis[i] > t || mini == -1)
                {
                    mini = i;
                    t = taxis[i];
                    N = xAxis[i];
                }
            }

            return (mini != -1);
        }

        public static bool Collide(Polygon A, Polygon B, Vector2 VelA, Vector2 VelB,
            out Vector2 N, ref float t)
        {
            List<Vector2> xAxis = new List<Vector2>();
            List<float> taxis = new List<float>();
            Vector2 xVel = -VelB + VelA;
            int iNumAxes = 0;
            xAxis.Add(new Vector2(-xVel.Y, xVel.X));
            var fVel2 = xVel.LengthSquared();
            N = Vector2.Zero;
            float taxisI;
            if (fVel2 > _epsilon)
            {
                if (!IntervalIntersect(A, B, VelA, VelB, xAxis[iNumAxes], out taxisI, t)) return false;
                taxis.Add(taxisI);
                iNumAxes++;
            }

            for (int j = A.n - 1, i = 0; i < A.n; j = i, i++)
            {
                Vector2 E = A.Vertices[i] - A.Vertices[j];
                xAxis.Add(new Vector2(-E.Y, E.X));
                if (!IntervalIntersect(A, B, VelA, VelB, xAxis[iNumAxes], out taxisI, t)) return false;
                taxis.Add(taxisI);
                iNumAxes++;
            }

            for (int j = B.n - 1, i = 0; i < B.n; j = i, i++)
            {
                Vector2 E = B.Vertices[i] - B.Vertices[j];
                xAxis.Add(new Vector2(-E.Y, E.X));
                if (!IntervalIntersect(A, B, VelA, VelB, xAxis[iNumAxes], out taxisI, t)) return false;
                taxis.Add(taxisI);
                iNumAxes++;
            }

            #region special case for segments
            if (B.n == 2)
            {
                Vector2 E = B.Vertices[1] - B.Vertices[0];
                xAxis[iNumAxes] = E;
                if (!IntervalIntersect(A, B, VelA, VelB, xAxis[iNumAxes], out taxisI, t)) return false;
                taxis.Add(taxisI);
                iNumAxes++;
            }

            if (A.n == 2)
            {
                Vector2 E = A.Vertices[1] - A.Vertices[0];
                xAxis[iNumAxes] = E;
                if (!IntervalIntersect(A, B, VelA, VelB, xAxis[iNumAxes], out taxisI, t)) return false;
                taxis.Add(taxisI);
                iNumAxes++;
            }
            #endregion

            if (!FindMTD(xAxis, taxis, out N, ref t)) return false;

            // make sure the polygons gets pushed away from each other.
            if (Vector2.Dot(A.Vertices[0] - B.Vertices[0], N) < 0.0f) N *= -1;
            return true;
        }
        #endregion

        public void Draw(SpriteBatch spriteBatch, Graphics.LineBrush lineBrush, Color color)
        {
            Draw(spriteBatch, lineBrush, Vector2.Zero, color);
        }

        public void Draw(SpriteBatch spriteBatch, Graphics.LineBrush lineBrush, Vector2 offset, Color color)
        {
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                lineBrush.Draw(spriteBatch, Vertices[p] + offset, Vertices[q] + offset, color);
            }
        }
    }
}