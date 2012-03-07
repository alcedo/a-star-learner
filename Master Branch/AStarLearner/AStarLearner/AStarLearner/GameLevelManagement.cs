﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XnaHelpers.GameEngine;

namespace AStarLearner
{
    class GameLevelManagement
    {
        private int Level; //Level of the game
        private List<String> gameSets; //store all the gameset names of the given level 
        private List<String> gameElements; //store all the elements of the given gameset name
        private string instruction;
        private string contFolder;

        #region Constructor
        public GameLevelManagement(int level)
        {
            this.Level = level;
            this.gameSets = new List<string>();
            this.gameElements = new List<string>();
        }
        #endregion

        #region Accessor
        public int getCurrentLevel()
        {
            return this.Level;
        }
        public string getCurrentLevelInstruction()
        {
            return this.instruction;
        }
        public string getCurrentLevelContent()
        {
            return this.contFolder;
        }
        public int getGameElementsCount()
        {
            return this.gameElements.Count;
        }
        public string getGameElementsContent(int i)
        {
            return this.gameElements[i];
        }
        #endregion

        //<summary>
        //This function is to load a proper set given a Level
        //</summary>
        public void loadGameLevel()
        {
            int numSets;
            switch (this.Level)
            {
                //LEVEL 1
                case 1:
                    numSets = 9;
                    initGameSetNames(numSets);
                    instruction = "Match the same object as";
                    break;
                //LEVEL 2
                case 2:
                    numSets = 9;
                    initGameSetNames(numSets);
                    instruction = "Match the same fruit as";
                    break;
                //LEVEL 3
                case 3:
                    numSets = 10;
                    initGameSetNames(numSets);
                    break;
                //LEVEL 4
                case 4:
                    numSets = 10;
                    initGameSetNames(numSets);
                    break;

            }

        }//end loadGameSets

        public void loadGameSets()
        {
            this.contFolder = "Level" + this.getCurrentLevel().ToString() + "\\" + this.getRandomGameSet();
        }

        #region Game Sets
        //Pushing all the Game Sets given the level
        public void initGameSetNames(int num_Sets)
        {
            for (int i = 1; i < num_Sets + 1; i++)
            {
                string name = "Set" + i;
                this.gameSets.Add(name);
            }
        }//end initGameSetName

        //Get a random game set at the current level
        public string getRandomGameSet()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            // Rand.Next picks lower bound(Inclusive) and Upper Bound Exclusive;
            int choice = rand.Next(0, this.gameSets.Count);
            return this.gameSets[choice];
        }//end getRandomGameSet()
        #endregion


        #region Game Elements
        public void addGameElements(String e)
        {
            this.gameElements.Add(e);
        }
        #endregion
    }
}

