using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameTimeManagement
{
    class GameTimeManager
    {
        // Timing Counter Constants
        private int INTERVAL_BTW_QUESTIONS = 2000;
        private int INTERVAL_PER_QUESTIONS = 20000;
        private int INTERVAL_PER_GAME_ROUND = 120000;
        
        // Tracking variables for calculation purposes
        private double intervalBtwQuestion;
        private double intervalPerQuestion;
        private double intervalPerGameRound;

        // Stores game time internally so that other functions would be able to access the game time
        public GameTime GameTime { get; set; }
     
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
