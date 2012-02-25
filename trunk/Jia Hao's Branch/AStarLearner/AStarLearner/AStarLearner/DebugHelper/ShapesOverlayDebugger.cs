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
using XnaHelpers.GameEngine;
using Neat.Mathematics;
using Microsoft.Research.Kinect.Nui;
using System.Diagnostics;
using Neat.Graphics;

#endregion

namespace AStarLearner.DebugHelper
{
    class ShapesOverlayDebugger
    {
        GameTextureInstance texture;
        Vector2 position; 
 
        public ShapesOverlayDebugger()
        {
            position = new Vector2(0, 0);
        }

        public void init(GraphicsDevice gfx_handle)
        {
            texture = GameTextureInstance.CreateBlank(gfx_handle, 20, 20);
            texture.Alpha = 1.0f;
            texture.Color = Color.Blue;    
        }

        public void drawShapeOverlay(SpriteBatch spriteBatch)
        {
            texture.Draw(spriteBatch);
        }

        public void setPosition(Vector2 position)
        {
            this.position = position;
            texture.Position = position;
        }


    }
}
