#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using AStarLearner;
using AStarLearner.DebugHelper;
using AStarLearner.InteractiveElements;
using GameLogicManagement;
using GameLevelManagement;
using GameStateManagement;
using GameTimeManagement;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Neat.Graphics;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using XNATweener;
using XnaHelpers.GameEngine;
#endregion

namespace GameStateManagementSample
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Initialization

        private bool debuggerOn = true;

        private GameSFX correct_snd;
        private ParticleEffect particleEffect;
        private Renderer particleRenderer;

        private List<String> gameSetList = new List<String>();
        public Random rand = new Random((int)DateTime.Now.Ticks);

        // Each gameSet would have 1 Correct Solution Object and multiple selection Object
        // The correct solution object should be placed in the first index.
        private ContentManager content;
        private List<GameObject> currentGameSet = new List<GameObject>();
        private GameObject solutionObjectReplica;
        private List<Vector2> gameObjectPosition = new List<Vector2>(); // Position of 6 choices

        private const int JointIntersectionSize = 80;
   

        // Display Interface
        Texture2D UI_FrameLayer;
        Vector2 UI_FrameLayerPosition;
        Vector2 UI_KinectFrameOffset;

        SpriteFont UI_Font_Instruction;
        Vector2 UI_FontPosition_Instruction;

        SpriteFont UI_Font_Score;
        Vector2 UI_FontPosition_Score;

        SpriteFont UI_Font_Time;
        Vector2 UI_FontPosition_Time;

        SpriteFont UI_Font_Level;
        Vector2 UI_FontPosition_Level;

        //Interactive Elements
        TextAnimator textAnimator;

        // Question related variables
        private bool questionIsCorrect = false;

        enum Selection { Correct, Wrong, Nil};
        
        //Transition related variables
        float pauseAlpha;
        InputAction pauseAction;

        // Kinect related variables
        private Runtime kinectRuntime;
        private readonly GameTextureInstance kinectRGBVideo = new GameTextureInstance();


        //Initiate GameLevelManagement
        private GameLevelManager gameLevelManager;
        private GameTimeManager gameTimeManager;
        private int currentLevel;
        //Constructor
        public GameplayScreen(int level)
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);

            //retrieve the current level of the game
            this.currentLevel = level;
            gameTimeManager = new GameTimeManager();

         }

        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            PlayerIndex player;
            if (pauseAction.Evaluate(input, ControllingPlayer, out player) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(GameStateManagementGame.gameWidth), ControllingPlayer);
            }

        }


        #endregion
        
        #region Game Graphic and Game Set initialization

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                {
                    content = new ContentManager(ScreenManager.Game.Services, "Content");
                }

                kinectRGBVideo.Texture = new Texture2D(ScreenManager.GraphicsDevice, 640, 480, false, SurfaceFormat.Color);

                kinectRuntime = new Runtime();
                kinectRuntime.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseDepthAndPlayerIndex);
                kinectRuntime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);

                kinectRuntime.VideoFrameReady += VideoFrameReady;
                kinectRuntime.SkeletonFrameReady += SkeletonFrameReady;

                kinectRGBVideo.Position = new Vector2(183, 225);
                UI_KinectFrameOffset = new Vector2(183, 225);

                // Game Set inits.
                //Game Level is initiated which has all the number of sets game and instruction
                gameLevelManager = new GameLevelManager(this.currentLevel, ref content);
                generateGameSet();

                // Load sounds 
                correct_snd = new GameSFX(content.Load<SoundEffect>("kling"));

                //Loads the background music and plays it
                GameMusic background = new GameMusic(content.Load<Song>("background_music3"));
                background.PlayLooping();

                particleEffect = new ParticleEffect();
                particleRenderer = new SpriteBatchRenderer
                {
                    GraphicsDeviceService = (IGraphicsDeviceService)ScreenManager.Game.Services.GetService(typeof(IGraphicsDeviceService))
                };
                particleRenderer.LoadContent(content);
                particleEffect = content.Load<ParticleEffect>(("BasicExplosion"));
                particleEffect.LoadContent(content);
                particleEffect.Initialise();

                //Interface Layer
                UI_FrameLayer = content.Load<Texture2D>("frame");
                UI_FrameLayerPosition = new Vector2(0, 0);
                UI_Font_Instruction = content.Load<SpriteFont>("SpriteFont");
                UI_FontPosition_Instruction = new Vector2(500, 90);
                UI_Font_Score = content.Load<SpriteFont>("SpriteFont");
                UI_FontPosition_Score = new Vector2(500, 22);
                UI_Font_Level = content.Load<SpriteFont>("SpriteFont");
                UI_FontPosition_Level = new Vector2(80, 22);
                UI_Font_Time = content.Load<SpriteFont>("SpriteFont");
                UI_FontPosition_Time = new Vector2(850, 22);

                //Interactive Elements
                textAnimator = new TextAnimator(content.Load<SpriteFont>("SpriteFont"));

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();

                // Debugger
                if (debuggerOn)
                {
                    skeletonDebugger = new SkeletonOverlayDebugger(kinectRuntime);
                    shapeDebugger.init(ScreenManager.GraphicsDevice);
                }
            }
        }

        private void VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage image = e.ImageFrame.Image;
            kinectRGBVideo.Texture = image.ToTexture2D(ScreenManager.GraphicsDevice);
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;

            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (data.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // Game Logic
                    if (IsActive) // Process game logic if the screen is active and infront .
                    {
                        gameLogic(data.Joints);
                    }
                    //Debugger
                    if (debuggerOn)
                    {
                        debuggerSkeleton(data.Joints);
                    }
                }
            }

            //Debugger
            if (debuggerOn)
            {
                skeletonDebugger.SkeletonFrameReady(sender, e);
            }
        }

        public override void Deactivate()
        {
           base.Deactivate();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            content.Unload();
            kinectRuntime.Uninitialize();
        }

        #endregion

        #region Debug Initialization
        SkeletonOverlayDebugger skeletonDebugger;
        ShapesOverlayDebugger shapeDebugger = new ShapesOverlayDebugger();
        private readonly List<GameTextureInstance> hotSpots = new List<GameTextureInstance>();
        private readonly List<GameTextureInstance> skeletonSpots = new List<GameTextureInstance>();
        private readonly List<GameTextureInstance> debugSpots = new List<GameTextureInstance>();
        #endregion

        #region Game Logic Helper

        public void destroyGameSet()
        {
            //solutionObjectReplica = null; // Find a way to remove solutionsprite as well
            currentGameSet.Clear();
            gameObjectPosition.Clear();
            gameObjectPosition.Clear();
        }

        /// <summary>
        /// This function helps to initialize a game set. This game set would then be 
        /// drawn subsequently in the draw() function. 
        /// 
        /// A Game set consists of A solution object and various selection object.
        /// </summary>
        /// <returns></returns>
        public List<GameObject> generateGameSet()
        {
            questionIsCorrect = false;
            solutionObjectReplica = null;
            currentGameSet.Clear();
            gameObjectPosition.Clear();
            
            //var set = content.LoadContent<Texture2D>(gameLevelManager.getCurrentLevelContent());
            //List<string> contentName = new List<string>();
            //foreach (string s in set.Keys)
              //  contentName.Add(s);

            List<int> contentRand = genRandomNumberList(0, gameLevelManager.getContentTextures().Count);

            // Create solution object
            GameSprite solutionSprite = new GameSprite(gameLevelManager.getTextureGraphics(contentRand[0]), 1, 1, 0, 0);
            //GameSprite solutionSprite = new GameSprite(set[contentName[contentRand[0]]], 1, 1, 0, 0);

            // Create selection objects
            GameSprite selectionSprite1 = new GameSprite(gameLevelManager.getTextureGraphics(contentRand[1]), 1, 1, 0, 0);
            GameSprite selectionSprite2 = new GameSprite(gameLevelManager.getTextureGraphics(contentRand[2]), 1, 1, 0, 0);
            GameSprite selectionSprite3 = new GameSprite(gameLevelManager.getTextureGraphics(contentRand[3]), 1, 1, 0, 0);
            //GameSprite selectionSprite1 = new GameSprite(set[contentName[contentRand[1]]], 1, 1, 0, 0);
            //GameSprite selectionSprite2 = new GameSprite(set[contentName[contentRand[2]]], 1, 1, 0, 0);
            //GameSprite selectionSprite3 = new GameSprite(set[contentName[contentRand[3]]], 1, 1, 0, 0);

            // Store positions. These are namely 2 on the left, 2 on the right
            gameObjectPosition.Add(new Vector2(190, 235)); //top left 
            gameObjectPosition.Add(new Vector2(190, 560)); // bottom left 
            gameObjectPosition.Add(new Vector2(735, 235)); // top right 
            gameObjectPosition.Add(new Vector2(735, 560)); //btm right 

            // Generate position placement choices randomly
            List<int> randPosn = new List<int>();

            while (randPosn.Count != gameObjectPosition.Count)
            {
                int choice = rand.Next(0, gameObjectPosition.Count);
                if (!randPosn.Contains(choice))
                    randPosn.Add(choice);
            }

            // Create solution object
            GameObject solutionObj = new GameObject(solutionSprite, gameObjectPosition[randPosn[0]]);
            solutionObj.isSolutionObject = true;

            // Create a replica of the solution object for question display 
            solutionObjectReplica = new GameObject(solutionSprite, 470, 105);

            GameObject selectionObject1 = new GameObject(selectionSprite1, gameObjectPosition[randPosn[1]]);
            GameObject selectionObject2 = new GameObject(selectionSprite2, gameObjectPosition[randPosn[2]]);
            GameObject selectionObject3 = new GameObject(selectionSprite3, gameObjectPosition[randPosn[3]]);

            // Add to current game set 
            currentGameSet.Add(solutionObj); //Solution is always the first
            currentGameSet.Add(selectionObject1);
            currentGameSet.Add(selectionObject2);
            currentGameSet.Add(selectionObject3);

            return currentGameSet;
        }
        
        // This function checks for intersection between game objects.
        private Selection checkSolutionIntersection(Vector2 jointPosition)
        {

            Rectangle rectangle = new Rectangle((int)jointPosition.X - (JointIntersectionSize / 2),
                                   (int)jointPosition.Y - (JointIntersectionSize / 2), JointIntersectionSize, JointIntersectionSize);

            foreach (GameObject obj in currentGameSet)
            {
                if (obj.Collision(rectangle))
                {
                    if (obj.isSolutionObject)
                        return Selection.Correct;
                    else
                        return Selection.Wrong;
                }
            }

            return Selection.Nil;

            // note: currentGameSet[0].Collision(rectangle); , currentGameSet[0] refers to solution object
        }

        /// <summary>
        /// This function triggers whenever a correct choice has been made by the user
        /// </summary>
        /// <param name="pos">Position of correct solution</param>
      
        private void correctChoice(Vector2 pos)
        {
            questionIsCorrect = true;
            //Particle Effect 
            particleEffect.Trigger(pos);    
            //Change GameSet
            destroyGameSet();
            //Play sound
            correct_snd.MultiPlay();      
            // Add Score
            GameScoringSystem.Instance.addScore(gameTimeManager.GameTime.ElapsedGameTime.Milliseconds);
            GameScoringSystem.Instance.checkWinningCondition(ref this.gameLevelManager, ref content);

            // Display Encouragements
            textAnimator.Start();
        }

        private void wrongChoice()
        {
            // task: Make sure this function is called only once / game set 
            // GameScoringSystem.Instance.decreaseScore();
        }

        /// <summary>
        /// Return an list of random numbers, that is guaranteed to have
        /// no repetition within the list itself 
        /// </summary>
        public List<int> genRandomNumberList(int min, int max)
        {
            // Fill this list of random numbers, w/o repetitions
            List<int> choice = new List<int>();
            while (choice.Count != max)
            {
                int num = rand.Next(min, max);

                if (!choice.Contains(num))
                    choice.Add(num);
            }

            return choice;
        }

        // Get the correcet joint position corrected with Offset for current UI
        public Vector2 getScreenPosition(Joint joint)
        {
            return (joint.GetScreenPosition(kinectRuntime, 640, 480) + UI_KinectFrameOffset);
        }
        #endregion

        #region Game Logic
        public void gameLogic(JointsCollection joints)
        {
            foreach (Joint joint in joints)
            {
                if (joint.ID == JointID.HandRight || joint.ID == JointID.HandLeft)
                {
                    Vector2 jointPosition = getScreenPosition(joint);

                    // Check if user made any selections
                    Selection selection = checkSolutionIntersection(jointPosition);

                    if (selection == Selection.Correct)
                    {
                        correctChoice(jointPosition);
                    }
                    else if(selection == Selection.Wrong)
                    {
                        wrongChoice();
                    }
                   
                    /*
                    //task: why put !questionIsCorrect? as checking variable?
                    if (!questionIsCorrect && checkSolutionIntersection(jointPosition)) // User selects the right choice
                    {
                        correctChoice(jointPosition);
                    }
                    else if (checkSolutionIntersection(jointPosition)) // User selects wrong choice
                    {
                        wrongChoice();
                    }*/


                }
            }
        }

        public void debuggerSkeleton(JointsCollection joints)
        {
            foreach (Joint joint in joints)
            {
                if (joint.ID == JointID.HipCenter)
                {
                    Vector2 jointPosition = getScreenPosition(joint);
                    shapeDebugger.setPosition((jointPosition));
                }
            }
        }




        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                //Duplicate and store game time to allow other functions to acccess the game time var.
                gameTimeManager.GameTime = gameTime;

                // Question related variables initialization
                float SecondsPassed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                particleEffect.Update(SecondsPassed);
                
                // Game Logic
                if (gameTimeManager.intervalPerQuestionUp(gameTime))
                {
                    generateGameSet();
                    gameTimeManager.restartQuestionTimeCounter();
                }

                if (questionIsCorrect && gameTimeManager.intervalBtwQuestionUp(gameTime))
                {
                    textAnimator.Stop();
                    generateGameSet();
                    gameTimeManager.restartQuestionTimeCounter();
                }

                if (gameTimeManager.intervalPerGameRoundUp(gameTime))
                {
                    LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
                }

                textAnimator.updateTweener(gameTime);
            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);
           

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            
            spriteBatch.Begin();
            kinectRGBVideo.Draw(spriteBatch);
            spriteBatch.Draw(UI_FrameLayer, UI_FrameLayerPosition, Color.White);

            //Printing out question, score
            string instruction = gameLevelManager.getCurrentLevelInstruction();
            Vector2 fontOrigin_instruction = UI_Font_Score.MeasureString(instruction) / 2;
            spriteBatch.DrawString(UI_Font_Score, instruction, UI_FontPosition_Instruction, Color.Black, 0, fontOrigin_instruction, 1.5f, SpriteEffects.None, 0.5f);

            string score = "Score: " + GameScoringSystem.Instance.getScore();
            Vector2 fontOrigin_score = UI_Font_Score.MeasureString(score)/2;
            spriteBatch.DrawString(UI_Font_Score, score, UI_FontPosition_Score, Color.Black, 0, fontOrigin_score, 1.5f, SpriteEffects.None, 0.5f);

            string level = "Level: " + gameLevelManager.getCurrentLevel().ToString();
            Vector2 fontOrigin_level = UI_Font_Score.MeasureString(level) / 2;
            spriteBatch.DrawString(UI_Font_Level, level, UI_FontPosition_Level, Color.Black, 0, fontOrigin_level, 1.5f, SpriteEffects.None, 0.5f);

            string time = "Time: " + gameTimeManager.getRoundTime();
            Vector2 fontOrigin_time = UI_Font_Score.MeasureString(time) / 2;
            spriteBatch.DrawString(UI_Font_Time, time, UI_FontPosition_Time, Color.Black, 0, fontOrigin_time, 1.5f, SpriteEffects.None, 0.5f);


            if (debuggerOn)     //Debugger
            {
                skeletonDebugger.DrawSkeletonOverlay_XNA(spriteBatch,
                                                              new LineBrush(ScreenManager.GraphicsDevice, 3),
                                                              UI_KinectFrameOffset,
                                                              new Vector2(kinectRuntime.VideoStream.Width, kinectRuntime.VideoStream.Height),
                                                              Color.Red);
            }

            spriteBatch.End();


            if (IsActive)
            {
                particleRenderer.RenderEffect(particleEffect);

                // Draw text. Text tween position are being updated in Update() 
                textAnimator.DrawText("Good Job!~", spriteBatch,
                      new Vector2(ScreenManager.Game.Window.ClientBounds.Width / 2, ScreenManager.Game.Window.ClientBounds.Height / 3),
                      new Vector2(ScreenManager.Game.Window.ClientBounds.Width / 2, ScreenManager.Game.Window.ClientBounds.Height - 80),
                      2.0f);

                foreach (GameObject g in currentGameSet)
                    g.Draw(spriteBatch);

                solutionObjectReplica.Draw(spriteBatch);
            }

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);
                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }
        #endregion
    }
}
