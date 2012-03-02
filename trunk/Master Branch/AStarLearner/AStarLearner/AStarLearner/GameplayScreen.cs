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
using GameStateManagement;
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
        int score;

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
        private int INTERVAL_BTW_QUESTIONS = 2000;
        private bool questionIsCorrect = false;
        private double intervalTime = 0;

        //Transition related variables
        float pauseAlpha;
        InputAction pauseAction;

        // Kinect related variables
        private Runtime kinectRuntime;
        private readonly GameTextureInstance kinectRGBVideo = new GameTextureInstance();


        //Initiate GameLevelManagement
        private GameLevelManagement gameLevelManagement; 
        //Constructor
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);

            //retrieve the current level of the game
            gameLevelManagement = new GameLevelManagement(1);

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
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                kinectRGBVideo.Texture = new Texture2D(ScreenManager.GraphicsDevice, 640, 480, false, SurfaceFormat.Color);

                kinectRuntime = new Runtime();
                kinectRuntime.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseDepthAndPlayerIndex);
                kinectRuntime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);

                kinectRuntime.VideoFrameReady += VideoFrameReady;
                kinectRuntime.SkeletonFrameReady += SkeletonFrameReady;

                kinectRGBVideo.Position = new Vector2(183, 225);
                UI_KinectFrameOffset = new Vector2(183, 225);

                // Game Set inits.
                gameLevelManagement.loadGameLevel();
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
                    gameLogic(data.Joints);
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

        public List<GameObject> generateGameSet()
        {
            questionIsCorrect = false;
            solutionObjectReplica = null;
            currentGameSet.Clear();
            gameObjectPosition.Clear();
            gameLevelManagement.loadGameSets();
            var set = content.LoadContent<Texture2D>(gameLevelManagement.getCurrentLevelContent());
            List<string> contentName = new List<string>();
            foreach (string s in set.Keys)
                contentName.Add(s);

            List<int> contentRand = genRandomNumberList(0, contentName.Count);

            // Create solution object
            GameSprite solutionSprite = new GameSprite(set[contentName[contentRand[0]]], 1, 1, 0, 0);

            // Create selection objects
            GameSprite selectionSprite1 = new GameSprite(set[contentName[contentRand[1]]], 1, 1, 0, 0);
            GameSprite selectionSprite2 = new GameSprite(set[contentName[contentRand[2]]], 1, 1, 0, 0);
            GameSprite selectionSprite3 = new GameSprite(set[contentName[contentRand[3]]], 1, 1, 0, 0);

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

        private bool checkSolutionIntersection(Vector2 jointPosition)
        {

            Rectangle rectangle = new Rectangle((int)jointPosition.X - (JointIntersectionSize / 2),
                                   (int)jointPosition.Y - (JointIntersectionSize / 2), JointIntersectionSize, JointIntersectionSize);

            return currentGameSet[0].Collision(rectangle);

        }

        private void correctChoice(Vector2 pos)
        {
            particleEffect.Trigger(pos); //To-do:Kenny particleEffect.Trigger position to be at where the object is instead of collision
            questionIsCorrect = true;
            destroyGameSet();
            correct_snd.MultiPlay();
            score++;
            textAnimator.Start();
        }

        private void wrongChoice()
        {
            // play sound. 
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
                //Debugger
                if (debuggerOn)
                {
                    if (joint.ID == JointID.HipCenter)
                    {
                        Vector2 jointPosition = getScreenPosition(joint);
                        shapeDebugger.setPosition((jointPosition));
                    }
                }

                if (joint.ID == JointID.HandRight || joint.ID == JointID.HandLeft)
                {
                    Vector2 jointPosition = getScreenPosition(joint);

                    if (!questionIsCorrect && checkSolutionIntersection(jointPosition))
                    {
                        correctChoice(jointPosition);
                    }
                    else
                    {
                        wrongChoice();
                    }
                }
            }
        }

        private bool intervalTimeUp(GameTime gameTime)
        {
            bool intervalTimeUp = false;

            intervalTime += (float)gameTime.ElapsedGameTime.Milliseconds;

            if (intervalTime >= INTERVAL_BTW_QUESTIONS)
            {
                intervalTime = 0;
                intervalTimeUp = true;
            }
            return intervalTimeUp;
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
                // Question related variables initialization
                float SecondsPassed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                int totalGameTime = gameTime.TotalGameTime.Seconds;
                particleEffect.Update(SecondsPassed);

                // Game Logic
                if (questionIsCorrect && intervalTimeUp(gameTime))
                {
                    textAnimator.Stop();
                    generateGameSet();
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
            string instruction = gameLevelManagement.getCurrentLevelInstruction();
            Vector2 fontOrigin_instruction = UI_Font_Score.MeasureString(instruction) / 2;
            spriteBatch.DrawString(UI_Font_Score, instruction, UI_FontPosition_Instruction, Color.Black, 0, fontOrigin_instruction, 1.5f, SpriteEffects.None, 0.5f);

            string score = "Score: " + this.score;
            Vector2 fontOrigin_score = UI_Font_Score.MeasureString(score)/2;
            spriteBatch.DrawString(UI_Font_Score, score, UI_FontPosition_Score, Color.Black, 0, fontOrigin_score, 1.5f, SpriteEffects.None, 0.5f);

            string level = "Level: " + gameLevelManagement.getCurrentLevel().ToString();
            Vector2 fontOrigin_level = UI_Font_Score.MeasureString(level) / 2;
            spriteBatch.DrawString(UI_Font_Level, level, UI_FontPosition_Level, Color.Black, 0, fontOrigin_level, 1.5f, SpriteEffects.None, 0.5f);

            string time = "Time: ";
            Vector2 fontOrigin_time = UI_Font_Score.MeasureString(time) / 2;
            spriteBatch.DrawString(UI_Font_Time, time, UI_FontPosition_Time, Color.Black, 0, fontOrigin_time, 1.5f, SpriteEffects.None, 0.5f);
            //Debugger
            if (debuggerOn)
            {
                skeletonDebugger.DrawSkeletonOverlay_XNA(spriteBatch,
                                                              new LineBrush(ScreenManager.GraphicsDevice, 3),
                                                              UI_KinectFrameOffset,
                                                              new Vector2(kinectRuntime.VideoStream.Width, kinectRuntime.VideoStream.Height),
                                                              Color.Red);
            }
            spriteBatch.End();

            particleRenderer.RenderEffect(particleEffect);

            textAnimator.DrawText("Good Job!~", spriteBatch,
                      new Vector2(ScreenManager.Game.Window.ClientBounds.Width / 2, ScreenManager.Game.Window.ClientBounds.Height / 3),
                      new Vector2(ScreenManager.Game.Window.ClientBounds.Width / 2, ScreenManager.Game.Window.ClientBounds.Height - 80),
                      2.0f);

            if (IsActive)
            {
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
