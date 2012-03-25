using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XNATweener;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AStarLearner.InteractiveElements
{
    /// <summary>
    /// This class animates text elements. 
    /// Think "Microsoft Power Point animation" 
    /// 
    /// Basically a wrapper class for our font-texture
    /// </summary>
    class TextAnimator
    {

        SpriteFont font;
        Tweener tweenX;
        Tweener tweenY;

        public TextAnimator(SpriteFont f)
        {
            font = f;
        }

        public void DrawText(string displayText, Color color  ,SpriteBatch sb, Vector2 from,Vector2 to, float duration, float scaleFactor)
        {
            // Find the center of the string
            Vector2 fontOrigin = font.MeasureString(displayText) / 2;
     
            
            // tweenX refers to tweening the X coordinate of our font sprite
            if (tweenX == null)
            {
                tweenX = new Tweener(from.X, to.X, duration,XNATweener.Bounce.EaseIn);
                tweenX.Stop();
            }

            // tweenY refers to tweening the Y coordinate of our font sprite
            if (tweenY == null)
            {
                tweenY = new Tweener(from.Y, to.Y, duration,XNATweener.Bounce.EaseIn);
                tweenY.Stop();
            }

            if (tweenX.Running || tweenY.Running)
            {
                sb.Begin();
                // Draw string
                sb.DrawString(font, displayText, new Vector2(tweenX.Position, tweenY.Position),
                              color, 0, fontOrigin, scaleFactor, SpriteEffects.None, 0.5f);
                sb.End();
            }
             
        }

        public void Start()
        {
            if (tweenX != null && !tweenX.Running) tweenX.Start();
            if (tweenY != null && !tweenY.Running) tweenY.Start();
        }

        public void Stop()
        {
            if (tweenX != null)
            {
                tweenX.Stop();
                tweenX = null;
            }

            if (tweenY != null)
            {
                tweenY.Stop();
                tweenY = null;
            }
        }

        public void updateTweener(GameTime t)
        {
            if (tweenX != null) this.tweenX.Update(t);
            if (tweenY != null) this.tweenY.Update(t);
        }

    }


}
