#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using GameStateManagement;
using Microsoft.Xna.Framework;

namespace GameStateManagementSample
{
    /// <summary>
    /// This main game class is extremely simple: all the interesting
    /// stuff happens in the ScreenManager component.
    /// </summary>
    public class GameStateManagementGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;
        ScreenFactory screenFactory;

        /// <summary>
        /// The main game constructor.
        /// </summary>
        public GameStateManagementGame()
        {
            Content.RootDirectory = "Content";
            const int gameWidth = 640;
            const int gameHeight = 480;

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = gameHeight;
            graphics.PreferredBackBufferWidth = gameWidth;

            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Create the screen factory and add it to the Services
            screenFactory = new ScreenFactory();
            Services.AddService(typeof(IScreenFactory), screenFactory);
            
            // Create the screen manager component.
            screenManager = new ScreenManager(this);

            // Game components provide a modular way of adding functionality to a game.
            // A registered component will have its draw, update, and initialize
            // methods called from the Game.Initialize, Game.Update, and Game.Draw methods.
            Components.Add(screenManager);

            // On Windows and Xbox we just add the initial screens
            AddInitialScreens();
        }

        private void AddInitialScreens()
        {
            // Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen(), null);

            // Add Main Menu Screen
            screenManager.AddScreen(new MainMenuScreen(Window.ClientBounds.Width, Content, graphics), null);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }
    }
}
