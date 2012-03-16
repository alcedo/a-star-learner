using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameTimeManagement
{
    class GameTimeManager
    {
        // Timing Counter
        private int INTERVAL_BTW_QUESTIONS = 2000;
        private double intervalBtwQuestion;
        private int INTERVAL_PER_QUESTIONS = 20000;
        private double intervalPerQuestion;
        private int INTERVAL_PER_GAME_ROUND = 120000;
        private double intervalPerGameRound;

        #region Constructor        
        public GameTimeManager()
        {
            intervalBtwQuestion = 0;
            intervalPerQuestion = 0;
            intervalPerGameRound = 0;
        }
        #endregion

        public double getRoundTime()
        {
            return Math.Round(intervalPerGameRound / 1000);
        }

        public void restartQuestionTimeCounter()
        {
            intervalBtwQuestion = 0;
            intervalPerQuestion = 0;
        }

        public bool intervalPerGameRoundUp(GameTime gameTime)
        {
            intervalPerGameRound += (float)gameTime.ElapsedGameTime.Milliseconds;
            if (intervalPerGameRound >= INTERVAL_PER_GAME_ROUND)
            {
                return true;
            }
            return false;
        }

        public bool intervalBtwQuestionUp(GameTime gameTime)
        {
            intervalBtwQuestion += (float)gameTime.ElapsedGameTime.Milliseconds;
            if (intervalBtwQuestion >= INTERVAL_BTW_QUESTIONS)
            {
                return true;
            }
            return false;
        }

        public bool intervalPerQuestionUp(GameTime gameTime)
        {
            intervalPerQuestion += (float)gameTime.ElapsedGameTime.Milliseconds;
            if (intervalPerQuestion >= INTERVAL_PER_QUESTIONS)
            {
                return true;
            }
            return false;
        }
    }
}
