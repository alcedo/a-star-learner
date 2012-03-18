using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XnaHelpers.GameEngine;
using Microsoft.Research.Kinect.Nui;

namespace GameLogicManagement
{
    /// <summary>
    ///  Differnt gamePlay type class should implement this interface
    /// </summary>
    public interface GameType
    {
        /// <summary>
        /// This function should fill up the CurrentGameSet with the approrpriate
        /// type of images and Game Objects
        /// </summary>
        /// <param name="currentGameSet"></param>
        void generateGameSet();

        /// <summary>
        /// Reverse and clear stuff
        /// </summary>
        void destroyGameSet();

        /// <summary>
        /// Any form of initialization should be done here
        /// </summary>
        void init();

        /// <summary>
        /// This function handles all the processing required 
        /// </summary>
        /// <param name="joints"></param>
        void gameLogic(JointsCollection joints);


    }


    /*
      /// <summary>
      /// This class handles the game play mechanics for a simple mix and match game
      /// </summary>
      public class MixAndMatch : GameType
      {
        

      }

  
     
      public class MathGame : GameType
      {

      }
     
   */
}

