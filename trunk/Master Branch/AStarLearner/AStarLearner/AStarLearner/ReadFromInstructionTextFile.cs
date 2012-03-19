using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLevelManagement
{
    public class ReadFromInstructionTextFile
    {
        private string[] allTextInFile;
        #region Constructor
        public ReadFromInstructionTextFile()
        {
            //load the file from the directory
            //Manual Documentation is required for the instructors to understand how the instruction being input 
            this.allTextInFile = System.IO.File.ReadAllLines(@"C:\InstructionFolder\Instruction.txt");
        }
        #endregion

        #region Methods
        //given a level of the game, this will return a string back to the display
        public string getLevelInstruction(int level)
        {
            int length = allTextInFile.Length;
            string temp = allTextInFile[(length-1)-level];
            int b = temp.IndexOf(':');
            return temp.Remove(0,b+1);
        }
        #endregion
    }


}
