using Microsoft.Research.Kinect.Nui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace GameLogicManagement
{
    /// <summary>
    /// This class wraps and handles the various GameType that we can have in our game
    /// There are basically 2 kinds of GameType currently.
    /// 
    /// 1.) Loading of GameContent from folder and allowing user to mix and match
    /// 2.) Dynamically generating Sprite fonts for mathematical calculations
    /// 
    /// We can add more gamelogic types for different form of gameplay in the future 
    /// </summary>
    public class GameLogicManager
    {
        private GameType currentGameType;

        public GameLogicManager(GameType type)
        {
            currentGameType = type;
        }



    }
}
