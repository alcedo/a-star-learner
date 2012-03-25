using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XnaHelpers.GameEngine;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace GameLevelManagement
{

    public class GameLevelManager
    {
        private int Level; //Level of the game
        private List<String> gameSets; //store all the gameset names of the given level

        public const int MAXLEVEL = 2; // Max possible amount of level for our game

        private ReadFromInstructionTextFile instructionFile;
        private string instruction;
        private string contFolderPath;
        private List<Texture2D> contentTextures;
        private int numOfLevels;

        #region Constructor
        public GameLevelManager(int level, ref ContentManager content)
        {
            this.Level = level;
            this.gameSets = new List<string>();
            instructionFile = new ReadFromInstructionTextFile(ref content, this.Level);
            contentTextures = new List<Texture2D>();
            numOfLevels = getNumOfTotalGameLevels();
            this.initGameLevel();
            //this.loadGameSets(ref content);
        }
        #endregion

        #region Accessor
        public int getCurrentLevel()
        {
            return this.Level;
        }

        public string getCurrentLevelInstruction()
        {
            return this.instructionFile.getLevelInstruction();
        }

        public string getCurrentLevelContent()
        {
            return this.contFolderPath;
        }
        //Getting total number of level by counting how many files in the Directory "Levels"
        public int getNumOfTotalGameLevels()
        {
            //Note that this only count when the levelfolder contains files. 
            string[] NameString = Directory.GetDirectories("Content/Levels/", "*", SearchOption.TopDirectoryOnly);
            this.numOfLevels = NameString.Length;
            
            return this.numOfLevels;
        }

        public int getNumOfGameSetsLevel(int level)
        {
            //Note that this only count when the setfolder contains files. 
            string[] NameString = Directory.GetDirectories("Content/Levels/Level" + level, "*", SearchOption.TopDirectoryOnly);
            return NameString.Length;
        }

        public List<Texture2D> getContentTextures()
        {
            return this.contentTextures;
        }

        //Access which game objects
        public Texture2D getTextureGraphics(int position)
        {
            return contentTextures[position];
        }
        #endregion

        #region Setters

        public void setLevel(int level)
        {
            if (level <= MAXLEVEL) this.Level = level;
        }

        #endregion
        //<summary>
        //This function is to load a proper set given a Level
        //</summary>
        public void initGameLevel()
        {
            int numSets;
            numSets = getNumOfGameSetsLevel(this.Level);
            initGameSetNames(numSets);
            this.instruction = getCurrentLevelInstruction();
        }//end loadGameSets


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
            Console.WriteLine("choice:" + choice);
            return this.gameSets[choice];
        }//end getRandomGameSet()

        //Loading all the game objects into a list of texture2D 
        public void loadGameSets(ref ContentManager content)
        {
            this.contFolderPath = "Levels/Level" + this.getCurrentLevel().ToString() + "/" + this.getRandomGameSet();
            //clear the contentTextures
            this.contentTextures = new List<Texture2D>();
            for (int i = 0; i < 5; i++)
            {
                string path = contFolderPath + "/" + (i + 1);
                Texture2D texture = content.Load<Texture2D>(path);
                this.contentTextures.Add(texture);
            }
        }
        #endregion


    }
}


