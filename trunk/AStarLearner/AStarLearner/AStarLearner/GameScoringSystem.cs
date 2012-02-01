using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AStarLearner
{


    class GameScoringSystem
    {
        /***************    Scoring Constants     ****************/
        // This score increment is per moves made and is independent of the speed and combo.
        private const int baseScore = 2;
        // Points given for speed
        private const int speedScore = 1;
        // Points given for Bonus 
        private const int bonusScore = 10;

        /****************      Variables         *****************/
        // Keeps tracks of the number of consecutive correct moves made. 
        private int comboCount = 0;
        // Keeps track of the total score 
        private int totalScore = 0;
        // Keeps track of the previous game time in seconds, for time related bonus.
        private int previousGameTime = 0;

        // Dictionary that maps username + userid to his score

        // Write and save the score to a file. 

        // This function records the score of the user whenever a correct move is being made
        public void addScore(int gameTime)
        {
            int score = 0;  
            comboCount++;

            // Calculate how fast the user managed to get a correct answer. if < 3 seconds, award extra pts
            if (gameTime - previousGameTime < 3)  score = score + speedScore;
            
            if(comboCount == 3) 

            // Tabulate final score 
        }

        // Subtract scores from user when a wrong move is made. 
        public void decreaseScore()
        {
            comboCount = 0;
        }

        /// <summary>
        /// This class stores all relevant game score information
        /// </summary>
        class UserScore
        {
            private string name;            // User name 
            private int userID;             // User ID 
            private double score;           // Current On-going game score. Update high score if higher. 
            private double highScore;       // Highest score achieve
            private int highestCombo;       // Max combo scores achieved. 
            private int maxLevel;           // Max level that the user has achieved 
            private double gameDuration;    // Stores the duration played by user

        }
    }



}
