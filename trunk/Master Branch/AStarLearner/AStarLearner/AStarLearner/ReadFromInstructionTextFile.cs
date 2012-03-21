using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace GameLevelManagement
{
    public class ReadFromInstructionTextFile
    {
        
        private String[] allTextInFile;
        private int gameLevel;
        #region Constructor
        public ReadFromInstructionTextFile(ref ContentManager content, int level)
        {
            //load the file from the directory
            //Manual Documentation is required for the instructors to understand how the instruction being input 
            string address = content.RootDirectory + "//InstructionFolder/Instruction.txt";
            this.allTextInFile = System.IO.File.ReadAllLines(@address);
            gameLevel = level;
        }
        #endregion

        #region Methods
        //given a level of the game, this will return a string back to the display
        public string getLevelInstruction()
        {
            int length = allTextInFile.Length;
            string temp = allTextInFile[this.gameLevel + 8 -1 ];
            int b = temp.IndexOf(':');
            return temp.Remove(0,b+1);
        }
        #endregion
    }
}
