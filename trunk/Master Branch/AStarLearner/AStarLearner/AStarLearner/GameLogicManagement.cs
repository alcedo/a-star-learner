using Microsoft.Research.Kinect.Nui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace GameLogicManagement
{
    /// <summary>
    /// There are basically 2 kinds of GameType currently.
    /// 
    /// 1.) Loading of GameContent from folder and allowing user to mix and match
    /// 2.) Dynamically generating Sprite fonts for mathematical calculations
    /// 
    /// We can add more gamelogic types for different form of gameplay in the future 
    /// </summary>
    class GameLogicManager
    {
        public enum GameType {MIX_N_MATCH, MATH};

        private GameType gameType; 

        public GameLogicManager(GameType type)
        {
            this.gameType = type;
        }




    }
}
