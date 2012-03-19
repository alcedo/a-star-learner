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
        private List<String> allTextInFile2;
        private String[] allTextInFile1;
        #region Constructor
        public ReadFromInstructionTextFile(ref ContentManager content)
        {
            //load the file from the directory
            //Manual Documentation is required for the instructors to understand how the instruction being input 
            

            //Method 1
            string address = content.RootDirectory + "//InstructionFolder/Instruction.txt";
            this.allTextInFile1 = System.IO.File.ReadAllLines(@address);
            
        }
        #endregion

        #region Methods
        //given a level of the game, this will return a string back to the display
        public string getLevelInstruction(int level)
        {
            int length = allTextInFile1.Length;
            string temp = allTextInFile1[(length-1)-level];
            int b = temp.IndexOf(':');
            return temp.Remove(0,b+1);
        }
        #endregion
    }


}
