using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AStarLearner
{
    // Usage: GameScoringSystem.Instance.addScore(int totalGameTime);
    public sealed class GameScoringSystem
    {
        // Singleton pattern
        static readonly GameScoringSystem _instance = new GameScoringSystem();

        public static GameScoringSystem Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// This is a private constructor, meaning no outsides have access.
        /// </summary>
        private GameScoringSystem()
        {
            
        }


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
       
        // Record Current User session score 
        private UserScoreRecords userRecords;

        public void setUserSessionInfo(UserScoreRecords userRecords)
        {
            this.userRecords = userRecords;
        }
                
        // Write and save the score to a file. (Use NLog for logging) 
        
        // This function records the score of the user whenever a correct move is being made
        public void addScore(int gameTime)
        {
            int score = 0;  
            
            // Calculate how fast the user managed to get a correct answer. if < 3 seconds, award extra pts
            if (gameTime - previousGameTime < 3)  score = score + speedScore;

            if (comboCount > 3) score = score + comboCount;    // Combo score 

            if (comboCount % 10 == 0) score = score + bonusScore;

            totalScore += score; // Update overall score.
            
            comboCount++;
        }

        // Subtract scores from user when a wrong move is made. 
        public void decreaseScore()
        {
            // Reset combo counter 
            comboCount = 0; 
            totalScore -= baseScore;
        }
        
        /// <summary>
        /// This class stores all relevant game score information
        /// </summary>
        public class UserScoreRecords
        {
            public string name;            // User name 
            public int userID;             // User ID 
            public double score;           // Current On-going game score. Update high score if higher. 
            public double highScore;       // Highest score achieve
            public int highestCombo;       // Max combo scores achieved. 
            public int maxLevel;           // Max level that the user has achieved 
            public double gameDuration;    // Stores the duration played by user

        }
    }



}
