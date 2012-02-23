using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Neat.Mathematics;

namespace Neat.Graphics
{
    public class LineBrush
    {
        Texture2D texture;
        int _thickness;
        Vector2 x = new Vector2(1, 0);

        public LineBrush(GraphicsDevice device, int thickness)
        {
            Create(device, thickness);
        }

        public void Create(GraphicsDevice device, int thickness)
        {
            _thickness = thickness;
            texture = new Texture2D(device, 2, thickness * 2);
            int size = texture.Width * texture.Height;
            Color[] pixmap = new Color[size];
            for (int i = 0; i < size; i++)
                pixmap[i] = Color.White;
            texture.SetData(pixmap);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color)
        {
            Vector2 difference, normalizedDifference, scale;
            float theta, rotation;

            Vector2.Subtract(ref point2, ref point1, out difference);

            Vector2.Normalize(ref difference, out normalizedDifference);
            Vector2.Dot(ref x, ref normalizedDifference, out theta);

            theta = (float)Math.Acos(theta);
            if (difference.Y < 0)
            {
                theta = -theta;
            }
            rotation = theta;

            float desiredLength = difference.Length();
            scale.X = desiredLength / texture.Width;
            scale.Y = 1;
            
            spriteBatch.Draw(
                texture,
                point1,
                null,
                color,
                rotation, //theta,
                new Vector2(0, _thickness / 2f + 1), //Vector2.Zero,
                scale,
                SpriteEffects.None,
                0);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, Vector2 offset)
        {
            Draw(spriteBatch, point1 + offset, point2 + offset, color);
        }

        public void Draw(SpriteBatch spriteBatch, LineSegment segment, Color color)
        {
            Draw(spriteBatch, segment.StartPos, segment.EndPos, color);
        }

        public void Draw(SpriteBatch spriteBatch, LineSegment segment, Color color, Vector2 offset)
        {
            Draw(spriteBatch, segment.StartPos + offset, segment.EndPos + offset, color);
        }

        public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            DrawRectangle(spriteBatch, rect, color, Vector2.Zero);
        }

        public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color, Vector2 offset)
        {
            Draw(spriteBatch, new Vector2((float)rect.Left, (float)rect.Top), new Vector2((float)rect.Right, (float)rect.Top), color, offset);
            Draw(spriteBatch, new Vector2((float)rect.Left, (float)rect.Bottom), new Vector2((float)rect.Right, (float)rect.Bottom), color, offset);
            Draw(spriteBatch, new Vector2((float)rect.Left, (float)rect.Top), new Vector2((float)rect.Left, (float)rect.Bottom), color, offset);
            Draw(spriteBatch, new Vector2((float)rect.Right, (float)rect.Top), new Vector2((float)rect.Right, (float)rect.Bottom), color, offset);
        }
    }
}
