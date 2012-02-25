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
using GameStateManagement;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
        #region Constructor
        private int windowWidth;
        private GraphicsDevice graphicDevice;
        private GameSFX correct_snd;
        private ParticleEffect particleEffect;
        private Renderer particleRenderer;

        private List<String> gameSetList = new List<String>();
        public Random rand = new Random((int)DateTime.Now.Ticks);

        // Each gameSet would have 1 Correct Solution Object and multiple selection Object
        // The correct solution object should be placed in the first index.
        private ContentManager Content;
        private List<GameObject> currentGameSet = new List<GameObject>();
        private GameObject solutionObjectReplica;

        // We would have 6 possible MCQ choices, to be displayed. ie: 3 on the left and 3 on the right. 
        // this list stores the coordinates to determine where the selection object should be placed. 
        private List<Vector2> gameObjectPosition = new List<Vector2>();

        private const int JointIntersectionSize = 80;

        //Constructor
        public GameplayScreen(int width, ContentManager content, GraphicsDeviceManager manager)
        {
            windowWidth = width;
            Content = content;
            graphicDevice = manager.GraphicsDevice;
            particleEffect = new ParticleEffect();
            particleRenderer = new SpriteBatchRenderer
            {
                GraphicsDeviceService = manager
            };
        }
        #endregion

        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        Random random = new Random();

        float pauseAlpha;

        InputAction pauseAction;

        // Kinect Vars
        private Runtime kinectRuntime;
        private readonly GameTextureInstance kinectRGBVideo = new GameTextureInstance();

        #endregion

        #region KinectInitialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                gameFont = content.Load<SpriteFont>("gamefont");

                kinectRGBVideo.Texture = new Texture2D(ScreenManager.GraphicsDevice, 640, 480, false, SurfaceFormat.Color);

                kinectRuntime = new Runtime();
                kinectRuntime.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseDepthAndPlayerIndex);
                kinectRuntime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);

                kinectRuntime.VideoFrameReady += VideoFrameReady;
                kinectRuntime.SkeletonFrameReady += SkeletonFrameReady;

                // Game Set inits.
                initGameSetList();
                generateGameSet();

                // Load sounds 
                correct_snd = new GameSFX(Content.Load<SoundEffect>("kling"));

                //Loads the background music and plays it
                GameMusic background = new GameMusic(Content.Load<Song>("background_music3"));
                background.PlayLooping();

                particleRenderer.LoadContent(Content);

                particleEffect = Content.Load<ParticleEffect>(("BasicExplosion"));
                particleEffect.LoadContent(Content);
                particleEffect.Initialise();

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
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
           //  ResetSquareColors();

            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (data.TrackingState == SkeletonTrackingState.Tracked)
                {
                    // gameLogic(data.Joints);
                }
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

        #region GameInitHelper

        /// <summary>
        /// This function would randomly select and set for us the correct solution  
        /// and pick random "non correct" red - herrings. 
        /// </summary>
        public void initGameSetList()
        {
            gameSetList.Add("Set1"); gameSetList.Add("Set6");
            gameSetList.Add("Set2"); gameSetList.Add("Set7");
            gameSetList.Add("Set3"); gameSetList.Add("Set8");
            gameSetList.Add("Set4"); gameSetList.Add("Set9");
            gameSetList.Add("Set5"); gameSetList.Add("Set10");

        }

        #endregion

        #region GameLogicHelper
        // Remember to draw objects
        public List<GameObject> generateGameSet()
        {
            solutionObjectReplica = null;
            currentGameSet.Clear();
            gameObjectPosition.Clear();
            int xSpacing = 5;

            var set = Content.LoadContent<Texture2D>("Level1\\" + getRandomGameSet());

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
            // GameSprite selectionSprite4 = new GameSprite(set1["yellow_ball"]  , 1, 1, 0, 0);

            // Store positions. These are namely 2 on the left, 2 on the right
            gameObjectPosition.Add(new Vector2(0, xSpacing)); //top left 
            gameObjectPosition.Add(new Vector2(0, solutionSprite.Height * 4)); // bottom left 
            gameObjectPosition.Add(new Vector2(windowWidth - solutionSprite.Width, 0)); // top right 
            gameObjectPosition.Add(new Vector2(windowWidth - solutionSprite.Width, solutionSprite.Height * 4)); //btm right

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

            // Create a replica of the solution object for display purposes only 
            solutionObjectReplica = new GameObject(solutionSprite, windowWidth / 2, 0);

            // left hand side
            GameObject selectionObject1 = new GameObject(selectionSprite1, gameObjectPosition[randPosn[1]]);
            GameObject selectionObject2 = new GameObject(selectionSprite2, gameObjectPosition[randPosn[2]]);

            // right hand side 
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
            generateGameSet(); //To-do:Kenny give a second or two of pause before the next set comes in
            correct_snd.MultiPlay();
        }

        private void wrongChoice()
        {
            // play sound. 
        }

        public string getRandomGameSet()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            // Rand.Next picks lower bound(Inclusive) and Upper Bound Exclusive;
            int choice = rand.Next(0, gameSetList.Count);
            return gameSetList[choice];
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
        #endregion

        #region GameLogic

        public void gameLogic(JointsCollection joints)
        {
            foreach (Joint joint in joints)
            {

                if (joint.ID == JointID.HandRight)
                {
                    // Place solution object replica on the person's hand
                    Vector2 jointPosition = joint.GetScreenPosition(kinectRuntime, graphicDevice.Viewport.Width, graphicDevice.Viewport.Height);
                    //solutionObjectReplica.Position = jointPosition;

                    if (checkSolutionIntersection(jointPosition))
                    {
                        correctChoice(jointPosition);
                    }
                    else
                    {
                        wrongChoice();
                    }

                }

                if (joint.ID == JointID.HandLeft)
                {
                    // Place solution object replica on the person's hand
                    Vector2 jointPosition = joint.GetScreenPosition(kinectRuntime, graphicDevice.Viewport.Width, graphicDevice.Viewport.Height);
                    //solutionObjectReplica.Position = jointPosition;

                    if (checkSolutionIntersection(jointPosition))
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
                // Game Update Logic here.
                float SecondsPassed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                int totalGameTime = gameTime.TotalGameTime.Seconds;
                particleEffect.Update(SecondsPassed);
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
            spriteBatch.End();

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
