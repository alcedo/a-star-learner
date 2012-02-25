using System;
using System.Collections.Generic;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using XnaHelpers.GameEngine;
using ProjectMercury;
using ProjectMercury.Emitters;
using ProjectMercury.Modifiers;
using ProjectMercury.Renderers;
using AStarLearner.DebugHelper;
using Neat.Graphics;
using XNATweener;
using AStarLearner.InteractiveElements;

namespace AStarLearner
{
    public class MainGame : Game
    {

        // Debugger overlay
        SkeletonOverlayDebugger skeletonDebugger;
        ShapesOverlayDebugger shapeDebugger = new ShapesOverlayDebugger();
   
        // Kenny Debug
        public bool kennyDebug = false;

        private readonly List<GameTextureInstance> hotSpots = new List<GameTextureInstance>();
        private readonly List<GameTextureInstance> skeletonSpots = new List<GameTextureInstance>();
        private readonly List<GameTextureInstance> debugSpots = new List<GameTextureInstance>();
       
        // XNA game engine variables 
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Game window dimensions
        public const int gameWidth = 1000;
        public const int gameHeight = 708;
        public const bool isFullScreen = false;

        //User interface (FrameBorder layer) 
        Texture2D UI_FrameLayer;
        Vector2 UI_FrameLayerPosition;
        Vector2 kinectFrameOffset;

        //Interactive Elements
        TextAnimator textAnimator;

        //Sprite Font: Display font on the screen
        SpriteFont font;
        Vector2 fontPos;
        int score;

        public Random rand = new Random((int)DateTime.Now.Ticks);

        private const int EdgeOffset = 10;
        private const int HotSpotSizes = 100;
        private const float HotSpotAlpha = 0.5f;
        private const int JointIntersectionSize = 80;

        private Runtime kinectRuntime;
        private readonly GameTextureInstance kinectRGBVideo = new GameTextureInstance();

        GameSFX correct_snd;
        ParticleEffect particleEffect;
        Renderer particleRenderer;


        // Each gameSet would have 1 Correct Solution Object and multiple selection Object
        // The correct solution object should be placed in the first index.
        private List<GameObject> currentGameSet = new List<GameObject>();

        //Stores a list of gameSet.
        private List<String> gameSetList = new List<String>();

        // We would have 6 possible MCQ choices, to be displayed. ie: 3 on the left and 3 on the right. 
        // this list stores the coordinates to determine where the selection object should be placed. 
        private List<Vector2> gameObjectPosition = new List<Vector2>();

        private GameObject solutionObjectReplica;

        //Question related Variable
        private int intervalBtwQuestion = 3000;
        private bool questionIsCorrect = false;
        private double intervalTime = 0;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = gameWidth;
            graphics.PreferredBackBufferHeight = gameHeight;

            this.graphics.IsFullScreen = isFullScreen;

            Content.RootDirectory = "Content";

            particleEffect = new ParticleEffect();
            particleRenderer = new SpriteBatchRenderer
            {
                GraphicsDeviceService = graphics
            };

        }

        #region Gameplay functions and logic
        /// <summary>
        /// This function would randomly select and set for us the correct solution. and 
        /// pick random "non correct" red - herrings. 
        /// </summary>
        public void initGameSetList()
        {
            gameSetList.Add("Set1"); gameSetList.Add("Set6");
            gameSetList.Add("Set2"); gameSetList.Add("Set7");
            gameSetList.Add("Set3"); gameSetList.Add("Set8");
            gameSetList.Add("Set4"); gameSetList.Add("Set9");
            gameSetList.Add("Set5"); gameSetList.Add("Set10");

        }

        public string getRandomGameSet()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            // Rand.Next picks lower bound(Inclusive) and Upper Bound Exclusive;
            int choice = rand.Next(0, gameSetList.Count);
            return gameSetList[choice];
        }

        /// <summary>
        /// Return an list of random numbers, that is guaranteed to have no repetition within the list
        /// itself 
        /// </summary>
        /// <param name="min">Min Number Range</param>
        /// <param name="max">Max Number Range</param>
        /// <returns>a list of ordered list of numbers</returns>
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

        //Destory game set
        public void destroyGameSet()
        {
            //solutionObjectReplica = null;
            currentGameSet.Clear();
            gameObjectPosition.Clear();
            gameObjectPosition.Clear();
        }

        // Remember to draw objects
        public List<GameObject> generateGameSet()
        {
            questionIsCorrect = false;
            solutionObjectReplica = null;
            currentGameSet.Clear();
            gameObjectPosition.Clear();

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


            //---Ryan change the cooordinates 
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

            // Create a replica of the solution object for display purposes only 
            solutionObjectReplica = new GameObject(solutionSprite, 500, 125);

            // left hand side
            GameObject selectionObject1 = new GameObject(selectionSprite1, gameObjectPosition[randPosn[1]]);
            GameObject selectionObject2 = new GameObject(selectionSprite2, gameObjectPosition[randPosn[2]]);

            // right hand side 
            GameObject selectionObject3 = new GameObject(selectionSprite3, gameObjectPosition[randPosn[3]]);

            // Kenny Debug Initi
            GameObject lefthand = new GameObject(selectionSprite3, new Vector2(0, 0));
            GameObject righthand = new GameObject(selectionSprite3, new Vector2(0, 0));
          
            // Add to current game set 
            currentGameSet.Add(solutionObj); //Solution is always the first
            currentGameSet.Add(selectionObject1);
            currentGameSet.Add(selectionObject2);
            currentGameSet.Add(selectionObject3);
            // Kenny Debug
            if (kennyDebug)
            {
                currentGameSet.Add(lefthand);
                currentGameSet.Add(righthand);
            }

            return currentGameSet;
        }


        // place this in the main game loop
        public void gameLogic(JointsCollection joints)
        {
            foreach (Joint joint in joints)
            {
                //DEBUGGING STUFF
                if (joint.ID == JointID.HipCenter)
                {
                     Vector2 jointPosition = joint.GetScreenPosition(kinectRuntime, 640, 480);
                     this.shapeDebugger.setPosition((jointPosition + kinectFrameOffset));
                }

                if (joint.ID == JointID.HandRight)
                {
                    // Place solution object replica on the person's hand
                    Vector2 jointPosition = joint.GetScreenPosition(kinectRuntime, 640,480);
                    //solutionObjectReplica.Position = jointPosition;
                    if (kennyDebug)
                    {
                        if (kennyDebug)
                        {
                            updateHands("right", (jointPosition + kinectFrameOffset));
                        }
                    }

                    if (!questionIsCorrect && checkSolutionIntersection((jointPosition + kinectFrameOffset)))
                    {
                        correctChoice((jointPosition + kinectFrameOffset));
                    }
                    else
                    {
                        wrongChoice();
                    }

                }

                if (joint.ID == JointID.HandLeft)
                {
                    // Place solution object replica on the person's hand
                    Vector2 jointPosition = joint.GetScreenPosition(kinectRuntime, 640, 480);
 
                    //solutionObjectReplica.Position = jointPosition;

                    if (kennyDebug)
                    {
                        updateHands("left", (jointPosition + kinectFrameOffset));
                    }

                    if (!questionIsCorrect && checkSolutionIntersection((jointPosition + kinectFrameOffset)))
                    {

                        particleEffect.Trigger(jointPosition); //To-do:Victor Please help to confirm if this can be remove as particleEffect.Trigger is already called in correctChoice
                        correctChoice((jointPosition + kinectFrameOffset));
                    }
                    else
                    {
                        wrongChoice();
                    }
                }
            }
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
            //generateGameSet(); //remove this line once destroyGameSet() is implemented.
            correct_snd.MultiPlay();
            this.score++;
            textAnimator.Start();
        }

        private void wrongChoice()
        {
            // play sound. 

        }

        private void updateHands(String hand, Vector2 pos) 
        {
            // Kenny Debug
            var set = Content.LoadContent<Texture2D>("Level1\\" + getRandomGameSet());

            List<string> contentName = new List<string>();
            foreach (string s in set.Keys)
                contentName.Add(s);

            List<int> contentRand = genRandomNumberList(0, contentName.Count);
            GameSprite selectionSprite4 = new GameSprite(set[contentName[contentRand[3]]], 1, 1, 0, 0);

            GameObject onehand = new GameObject(selectionSprite4, pos);
            if (currentGameSet.Count > 0)
            {
                if (hand.Equals("left"))
                {
                    currentGameSet[4] = onehand;
                }
                else if (hand.Equals("right"))
                {
                    currentGameSet[5] = onehand;
                }
            }
        }


        private bool intervalTimeUp(GameTime gameTime)
        {
            bool intervalTimeUp = false;

            intervalTime += (float)gameTime.ElapsedGameTime.Milliseconds;

            if (intervalTime >= intervalBtwQuestion)
            {
                intervalTime = 0;
                intervalTimeUp = true;
            }
            return intervalTimeUp;
        }

        private void ResetSquareColors()
        {
            foreach (GameTextureInstance texture in hotSpots)
                texture.Color = Color.Red;
        }

        #endregion

        #region XNA Run time functions
        ////////////////////////////////
        /** XNA Run time functions **/
        ////////////////////////////////
        protected override void Initialize()
        {

            kinectRuntime = new Runtime();
            kinectRuntime.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseDepthAndPlayerIndex);
            kinectRuntime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);

            kinectRuntime.VideoFrameReady += VideoFrameReady;
            kinectRuntime.SkeletonFrameReady += SkeletonFrameReady;

            particleEffect.Initialise();

            // Init Debugger overlay
            skeletonDebugger = new SkeletonOverlayDebugger(this.kinectRuntime);

            
            base.Initialize();
        }


        protected override void OnExiting(object sender, EventArgs args)
        {
            kinectRuntime.Uninitialize();
            base.OnExiting(sender, args);
        } 

        protected override void LoadContent()
        {
            // Rendering inits. 
            spriteBatch = new SpriteBatch(GraphicsDevice);
            kinectRGBVideo.Texture = new Texture2D(GraphicsDevice, gameWidth, gameHeight, false, SurfaceFormat.Color);
            kinectRGBVideo.Position = new Vector2(183, 228); //Ryan --- changed position
            this.kinectFrameOffset = new Vector2(183, 228);

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

            //Handling Interface Layer on the screen
            UI_FrameLayer = Content.Load<Texture2D>("frame");
            UI_FrameLayerPosition = new Vector2(0, 0);

            //Handling Font Sprite on the screen
            font = Content.Load<SpriteFont>("SpriteFont");
            fontPos = new Vector2(535, 25);

            // Interactive elements (text animator) 
            textAnimator = new TextAnimator(Content.Load<SpriteFont>("SpriteFont"));

            // Debug inits
            shapeDebugger.init(GraphicsDevice);

            /*
            //skeleton right hand
            GameTextureInstance texture = GameTextureInstance.CreateBlank(GraphicsDevice, 20, 20);
            texture.Position = new Vector2(0, 0);
            texture.Alpha = 1;
            texture.Color = Color.Orange;
            skeletonSpots.Add(texture);

            //Left hand
            texture = GameTextureInstance.CreateBlank(GraphicsDevice, 20, 20);
            texture.Position = new Vector2(0, 0);
            texture.Alpha = 1;
            texture.Color = Color.BlueViolet;
            skeletonSpots.Add(texture);

            //hotspots
            texture = GameTextureInstance.CreateBlank(GraphicsDevice, HotSpotSizes, HotSpotSizes);
            texture.Position = new Vector2(EdgeOffset);
            texture.Alpha = HotSpotAlpha;
            hotSpots.Add(texture);

            texture = GameTextureInstance.CreateBlank(GraphicsDevice, HotSpotSizes, HotSpotSizes);
            texture.Position = new Vector2(GraphicsDevice.Viewport.Width - texture.Texture.Width - EdgeOffset, EdgeOffset);
            texture.Alpha = HotSpotAlpha;
            hotSpots.Add(texture);

            texture = GameTextureInstance.CreateBlank(GraphicsDevice, HotSpotSizes, HotSpotSizes);
            texture.Position = new Vector2(10, GraphicsDevice.Viewport.Height - texture.Texture.Height - EdgeOffset);
            texture.Alpha = HotSpotAlpha;
            hotSpots.Add(texture);

            texture = GameTextureInstance.CreateBlank(GraphicsDevice, HotSpotSizes, HotSpotSizes);
            texture.Position = new Vector2(GraphicsDevice.Viewport.Width - texture.Texture.Width - EdgeOffset, GraphicsDevice.Viewport.Height - texture.Texture.Height - EdgeOffset);
            texture.Alpha = HotSpotAlpha;
            hotSpots.Add(texture);

            //debug textures
            GameTextureInstance texture = GameTextureInstance.CreateBlank(GraphicsDevice, 80, 80);
            texture.Alpha = 0.3f;
            texture.Color = Color.HotPink;
            debugSpots.Add(texture);

            texture = GameTextureInstance.CreateBlank(GraphicsDevice, 80, 80);
            texture.Alpha = 0.3f;
            texture.Color = Color.HotPink;
            debugSpots.Add(texture);
 

            ResetSquareColors();
            */
        }

        protected override void Update(GameTime gameTime)
        {
            
            float SecondsPassed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            int totalGameTime = gameTime.TotalGameTime.Seconds;

            particleEffect.Update(SecondsPassed);

            if (questionIsCorrect && intervalTimeUp(gameTime))
            {
                textAnimator.Stop();
                generateGameSet();
            }

            textAnimator.updateTweener(gameTime);

            base.Update(gameTime);


        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();  // Begin Sprite batch 

            // Draw textures from the Kinect Camera Stream
            kinectRGBVideo.Draw(spriteBatch);

            // Draw UI frame Border 
            spriteBatch.Draw(UI_FrameLayer, UI_FrameLayerPosition, Color.White);

            // Draw Font Sprites
            string output = "" + this.score;
            //string output = "" + intervalTime;

            // Find the center of the string
            Vector2 fontOrigin = font.MeasureString(output) / 2;
            // Draw fontSprite
            spriteBatch.DrawString(font, output, fontPos, Color.Black, 0, fontOrigin, 1.5f, SpriteEffects.None, 0.5f);


            ////////////////////////// Deubg Overlays ///////////////////////////////////
            // Note: kinectRuntime.VideoStream.Width / Height is equivalent to the resolution set in init function
            this.skeletonDebugger.DrawSkeletonOverlay_XNA(spriteBatch,
                new LineBrush(GraphicsDevice, 1), this.kinectFrameOffset,
                new Vector2(kinectRuntime.VideoStream.Width, kinectRuntime.VideoStream.Height),
                Color.Red);

            // this.shapeDebugger.drawShapeOverlay(spriteBatch);

            spriteBatch.End();   // End Sprite batch 


            /////// Interactive Element (text anim) ///////// 
            textAnimator.DrawText("Good Job!~", spriteBatch,
                                  new Vector2(gameWidth / 2, -gameHeight / 4),
                                  new Vector2(gameWidth / 2, gameHeight -20),
                                  2.0f);


            particleRenderer.RenderEffect(particleEffect);

            foreach (GameObject g in currentGameSet)
                g.Draw(spriteBatch);

            solutionObjectReplica.Draw(spriteBatch);

            /*
                        foreach (GameTextureInstance texture in hotSpots)
                            texture.Draw(spriteBatch);

                        foreach (GameTextureInstance texture in skeletonSpots)
                            texture.Draw(spriteBatch);

                        foreach (GameTextureInstance texture in debugSpots)
                            texture.Draw(spriteBatch);


                        spriteBatch.End(); */

            base.Draw(gameTime);
        }
        #endregion

        #region Kinect run time Statements
        ////////////////////////////////
        /** Kinect Related Functions **/
        ////////////////////////////////
        private void VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage image = e.ImageFrame.Image;
            kinectRGBVideo.Texture = image.ToTexture2D(GraphicsDevice);
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            ResetSquareColors();

            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                if (data.TrackingState == SkeletonTrackingState.Tracked)
                {
                    gameLogic(data.Joints);
                }
            }

            skeletonDebugger.SkeletonFrameReady(sender, e);
        }
        #endregion


    }
}
