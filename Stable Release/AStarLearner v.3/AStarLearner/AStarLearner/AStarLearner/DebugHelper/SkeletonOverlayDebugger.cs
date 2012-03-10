#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Research.Kinect;
 
using Neat.Mathematics;
using Microsoft.Research.Kinect.Nui;
using System.Diagnostics;
using Neat.Graphics;

#endregion

 

namespace AStarLearner.DebugHelper 
{
    class SkeletonOverlayDebugger
    {

        //////////////////////////////////
        /** Debugger related variables **/
        //////////////////////////////////
        public SkeletonData[] Skeletons = new SkeletonData[2];
        int trackedSkeletonsCount = 0;
        public int TrackedSkeletonsCount { get { return trackedSkeletonsCount; } }
        private Runtime kinectRuntime;

        // Constructor
        public SkeletonOverlayDebugger(Runtime r)
        {
            kinectRuntime = r;
        }

        public void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var trackedSkeletons = from s in e.SkeletonFrame.Skeletons
                                   where
                                       s.TrackingState == SkeletonTrackingState.Tracked
                                   select s;
            trackedSkeletonsCount = trackedSkeletons.Count();
            for (int i = 0; i < trackedSkeletonsCount; i++)
            {
                Skeletons[i] = trackedSkeletons.ElementAt(i);
            }
        }

        // The difference between this function and DrawSkeletonOverlay is that this function takes into account
        // Depth resizing of the skeleton. ie: when user walks nearer, the line b.w each joint stretches
        public void DrawSkeletonOverlay_XNA(SpriteBatch spriteBatch, LineBrush lb, Vector2 position, Vector2 size, Color color, int skeletonId = 0)
        {

            if (Skeletons.Length <= skeletonId || Skeletons[skeletonId] == null)
            {
                //Skeleton not found. Draw an X
                lb.Draw(spriteBatch, position, position + size, color);
                lb.Draw(spriteBatch, new LineSegment(position.X + size.X, position.Y, position.X, position.Y + size.Y), color);
                return;
            }

            //Right Hand
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.HandRight,  skeletonId), ToVector2_XNA(JointID.WristRight, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.WristRight,  skeletonId), ToVector2_XNA(JointID.ElbowRight,  skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.ElbowRight,  skeletonId), ToVector2_XNA(JointID.ShoulderRight, skeletonId), color, position);

            //Head & Shoulders
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.ShoulderRight,  skeletonId), ToVector2_XNA(JointID.ShoulderCenter, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.Head, skeletonId), ToVector2_XNA(JointID.ShoulderCenter,  skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.ShoulderCenter, skeletonId), ToVector2_XNA(JointID.ShoulderLeft,  skeletonId), color, position);

            //Left Hand
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.HandLeft,  skeletonId), ToVector2_XNA(JointID.WristLeft,  skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.WristLeft,  skeletonId), ToVector2_XNA(JointID.ElbowLeft,  skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.ElbowLeft, skeletonId), ToVector2_XNA(JointID.ShoulderLeft,  skeletonId), color, position);

            //Hips & Spine
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.HipLeft,  skeletonId), ToVector2_XNA(JointID.HipCenter, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.HipRight,  skeletonId), ToVector2_XNA(JointID.HipCenter, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.Spine,  skeletonId), ToVector2_XNA(JointID.HipCenter, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.Spine, skeletonId), ToVector2_XNA(JointID.ShoulderCenter, skeletonId), color, position);

            //Left foot
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.HipLeft,  skeletonId), ToVector2_XNA(JointID.KneeLeft,  skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.KneeLeft,  skeletonId), ToVector2_XNA(JointID.AnkleLeft,  skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.AnkleLeft, skeletonId), ToVector2_XNA(JointID.FootLeft, skeletonId), color, position);

            //Right foot
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.HipRight,  skeletonId), ToVector2_XNA(JointID.KneeRight, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.KneeRight,  skeletonId), ToVector2_XNA(JointID.AnkleRight, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2_XNA(JointID.AnkleRight, skeletonId), ToVector2_XNA(JointID.FootRight,  skeletonId), color, position);
        }

        // Simply draw a skeleton overlay without taking into account depth 
        public void DrawSkeletonOverlay(SpriteBatch spriteBatch, LineBrush lb, Vector2 position, Vector2 size, Color color, int skeletonId = 0)
        {
           
            if (Skeletons.Length <= skeletonId || Skeletons[skeletonId] == null)
            {
                //Skeleton not found. Draw an X
                lb.Draw(spriteBatch, position, position + size, color);
                lb.Draw(spriteBatch, new LineSegment(position.X + size.X, position.Y, position.X, position.Y + size.Y), color);
                return;
            }

            //Right Hand
            lb.Draw(spriteBatch, ToVector2(JointID.HandRight, size, skeletonId), ToVector2(JointID.WristRight, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.WristRight, size, skeletonId), ToVector2(JointID.ElbowRight, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.ElbowRight, size, skeletonId), ToVector2(JointID.ShoulderRight, size, skeletonId), color, position);

            //Head & Shoulders
            lb.Draw(spriteBatch, ToVector2(JointID.ShoulderRight, size, skeletonId), ToVector2(JointID.ShoulderCenter, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.Head, size, skeletonId), ToVector2(JointID.ShoulderCenter, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.ShoulderCenter, size, skeletonId), ToVector2(JointID.ShoulderLeft, size, skeletonId), color, position);

            //Left Hand
            lb.Draw(spriteBatch, ToVector2(JointID.HandLeft, size, skeletonId), ToVector2(JointID.WristLeft, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.WristLeft, size, skeletonId), ToVector2(JointID.ElbowLeft, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.ElbowLeft, size, skeletonId), ToVector2(JointID.ShoulderLeft, size, skeletonId), color, position);

            //Hips & Spine
            lb.Draw(spriteBatch, ToVector2(JointID.HipLeft, size, skeletonId), ToVector2(JointID.HipCenter, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.HipRight, size, skeletonId), ToVector2(JointID.HipCenter, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.Spine, size, skeletonId), ToVector2(JointID.HipCenter, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.Spine, size, skeletonId), ToVector2(JointID.ShoulderCenter, size, skeletonId), color, position);

            //Left foot
            lb.Draw(spriteBatch, ToVector2(JointID.HipLeft, size, skeletonId), ToVector2(JointID.KneeLeft, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.KneeLeft, size, skeletonId), ToVector2(JointID.AnkleLeft, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.AnkleLeft, size, skeletonId), ToVector2(JointID.FootLeft, size, skeletonId), color, position);

            //Right foot
            lb.Draw(spriteBatch, ToVector2(JointID.HipRight, size, skeletonId), ToVector2(JointID.KneeRight, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.KneeRight, size, skeletonId), ToVector2(JointID.AnkleRight, size, skeletonId), color, position);
            lb.Draw(spriteBatch, ToVector2(JointID.AnkleRight, size, skeletonId), ToVector2(JointID.FootRight, size, skeletonId), color, position);
        }

        public Vector3 ToVector3(JointID joint, int skeletonId = 0)
        {
            var p = Skeletons[skeletonId].Joints[joint].Position;
            return new Vector3(p.X, p.Y, p.Z);
        }

        public Vector2 ToVector2(JointID joint, int skeletonId = 0)
        {
            var p = ToVector3(joint, skeletonId);
            return new Vector2(p.X, p.Y);
        }

        public static Vector2 ToVector2(Microsoft.Research.Kinect.Nui.Vector p, Vector2 scale)
        {
            var half = scale / 2.0f;
            return (new Vector2(p.X, p.Y) * half * new Vector2(1, -1)) + half;
        }

        public Vector2 ToVector2(JointID joint, Vector2 scale, int skeletonId = 0)
        {
            return ToVector2(Skeletons[skeletonId].Joints[joint].Position, scale);
        }

        public Vector2 ToVector2_XNA(JointID joint, int skeletonId = 0)
        {
            // the function GetScreenPosition calculates the joint position using depth info as well
            return Skeletons[skeletonId].Joints[joint].GetScreenPosition(kinectRuntime,
                kinectRuntime.VideoStream.Width, kinectRuntime.VideoStream.Height);
        }

        public static Vector3 ToVector3(Microsoft.Research.Kinect.Nui.Vector p, Vector3 scale)
        {
            var half = scale / 2.0f;
            return (new Vector3(p.X, p.Y, p.Z) * half * new Vector3(1, -1, 1)) + half;
        }

        public Vector3 ToVector3(JointID joint, Vector3 scale, int skeletonId = 0)
        {
            return ToVector3(Skeletons[skeletonId].Joints[joint].Position, scale);
        }

        public int InferredJointsCount(int skeletonId = 0)
        {
            if (trackedSkeletonsCount <= skeletonId || Skeletons[skeletonId] == null) return (int)JointID.Count;
            int r = 0;
            for (JointID i = 0; i < JointID.Count; i++)
            {
                if (Skeletons[skeletonId].Joints[i].TrackingState != JointTrackingState.Tracked) r++;
            }
            return r;
        }


 


    }
}
