//Author: Sophia Lin
//File Name: Game1.cs
//Project Name: PASS2
//Creation Date: Nov. 24, 2022
//Modified Date: Dec. 4, 2022
//Description: Sink balls in buckets!
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Animation2D;
using Helper;

namespace GameStateDemoLogic
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        #region global variables including textures, rectangles, vectors, constants, and others used throughout game.
        
        //Store gamestates & background images index
        const int MENU = 0;
        const int HIGHSCORES = 1;
        const int PLAY = 2;
        const int GAME_OVER = 3;
        const int PAUSE = 4;

        //Store button images index
        const int EASYBTN = 0;
        const int HIGHSCOREBTN = 1;
        const int EXITBTN = 2;
        const int MENUBTN = 3;
        const int HARDBTN = 4;

        //Store bucket images index
        const int LARGE = 0;
        const int MEDIUM = 1;
        const int SMALL = 2;

        //Store fonts index
        const int BASIC = 0;
        const int TITLE = 1;
        const int DISPLAY = 2;

        //Store difficulty mode index
        const int EASY = 0;
        const int HARD = 1;

        //Store speed index (if exceeds max speed or under min speed)
        const int OVER = 0;
        const int UNDER = 1;

        //Store wind arrows index
        const int RIGHT = 0;
        const int LEFT = 1;
        const int NONE = 2;

        //Store the force of gravity
        const float GRAVITY = 9.81f / 60;

        //Store the maximum number of throws, and max/min speeds, and max amount of time on shot clock
        const int MAXIMUM_PLAYS = 5;
        const int MAX_SPEED = 20;
        const int MIN_SPEED = 5;
        const int MAX_TIME = 15000;

        //Create random generator
        static Random rng = new Random();

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Store fonts
        SpriteFont[] fonts = new SpriteFont[3];

        //Store Images
        Texture2D[] bgImgs = new Texture2D[4];
        Texture2D[] buttonImgs = new Texture2D[5];
        Texture2D[] bucketImgs = new Texture2D[3];
        Texture2D ballImg;
        Texture2D blankImg;
        Texture2D splashImg;
        Texture2D meterImg;
        Texture2D powerArrowImg;
        Texture2D trajectoryArrowImg;
        Texture2D[] windArrowsImg = new Texture2D[3];

        //Store Rectangles
        Rectangle[] bgRecs = new Rectangle[4];
        Rectangle[] buttonRecs = new Rectangle[5];
        Rectangle[] bucketRecs = new Rectangle[3];
        Rectangle[] sinkBucketRecs = new Rectangle[3];
        Rectangle ballRec;
        Rectangle blankRecTop;
        Rectangle blankRecBottom;
        Rectangle timeMeterRec;
        Rectangle actualTimeMeterRec;
        Rectangle speedMeterRec;
        Rectangle actualSpeedMeterRec;
        Rectangle powerArrowRec;
        Rectangle trajectoryArrowRec;
        Rectangle[] windArrowsRec = new Rectangle[3];

        //Store true position for ball
        Vector2 ballPos;

        //Store the origin and position of the trajectory arrow (used for rotation)
        Vector2 origin;
        Vector2 trajectoryArrowPos;

        //Store position for splash animation
        Vector2[] splashPos = new Vector2[3];

        //Store splash animation
        Animation[] splashAnims = new Animation[3];

        //For storing screen dimensions
        int screenWidth;
        int screenHeight;

        //Set beginning gamestate
        int gameState = MENU;

        //Store game data
        float speed = 5f;
        float updateSpeed = 0.3f;
        float speedPercentage = 0f;
        Vector2 ballSpeed = new Vector2(0f, 0f);
        float angle = 10f;
        int score = 0;
        byte streak = 0;
        int cntGamePlay = 0;
        int[] highscores = new int[2] { 0, 0 }; 
      
        //Store the force of wind and direction
        float wind = rng.Next(0, 16); 
        int windDir = rng.Next(1, 3); 
        float windTotal = 0;

        //Store directions and the value of the X direction
        const int POS = 1;
        const int NEG = -1;
        int dirX = POS;

        //Create mouse states
        MouseState mouse;
        MouseState prevMouse;

        //Create keyboard states
        KeyboardState kb;
        KeyboardState prevKb;

        //Create shot timer
        Timer shotTimer;

        //Store sound effects
        SoundEffect buttonClickSnd;
        SoundEffect splashSnd;
        SoundEffect throwSnd;

        //Store songs
        Song circusMusic;
        Song circusMusic2;

        //Store if the ball is thrown, if highscores are changed, if the mode is hard or easy, and if the speeds are over max or below min
        bool isBallThrown = false;
        bool[] isHighscoresChanged = new bool[2] { false, false };
        bool[] modes = new bool[2] { false, false };
        bool[] speeds = new bool[2] { false, false };
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //Display cursor
            IsMouseVisible = true;

            //Set screen dimensions
            this.graphics.PreferredBackBufferHeight = 768;
            this.graphics.PreferredBackBufferWidth = 1024;

            //Apply the screen dimension changes
            this.graphics.ApplyChanges();

            //Store screen dimensions
            screenWidth = graphics.GraphicsDevice.Viewport.Width;
            screenHeight = graphics.GraphicsDevice.Viewport.Height;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            //Load fonts
            fonts[BASIC] = Content.Load<SpriteFont>("Fonts/BasicFont");
            fonts[TITLE] = Content.Load<SpriteFont>("Fonts/TitleFont");
            fonts[DISPLAY] = Content.Load<SpriteFont>("Fonts/DisplayFont");

            //Load images
            bgImgs[MENU] = Content.Load<Texture2D>("Images/Backgrounds/MenuBg");
            bgImgs[HIGHSCORES] = Content.Load<Texture2D>("Images/Backgrounds/HighScoreBg");
            bgImgs[PLAY] = Content.Load<Texture2D>("Images/Backgrounds/CircusBg");
            bgImgs[GAME_OVER] = Content.Load<Texture2D>("Images/Backgrounds/EndGameBg");
            buttonImgs[EASYBTN] = Content.Load<Texture2D>("Images/Sprites/BtnEasy");
            buttonImgs[HIGHSCOREBTN] = Content.Load<Texture2D>("Images/Sprites/BtnHighscores");
            buttonImgs[EXITBTN] = Content.Load<Texture2D>("Images/Sprites/BtnExit");
            buttonImgs[MENUBTN] = Content.Load<Texture2D>("Images/Sprites/BtnMenu");
            buttonImgs[HARDBTN] = Content.Load<Texture2D>("Images/Sprites/BtnHard");
            bucketImgs[LARGE] = Content.Load<Texture2D>("Images/Sprites/RedBucket");
            bucketImgs[MEDIUM] = Content.Load<Texture2D>("Images/Sprites/GreenBucket");
            bucketImgs[SMALL] = Content.Load<Texture2D>("Images/Sprites/BlueBucket");
            ballImg = Content.Load<Texture2D>("Images/Sprites/ball");
            blankImg = Content.Load<Texture2D>("Images/Sprites/Blank");
            splashImg = Content.Load<Texture2D>("Images/Sprites/Splash");
            meterImg = Content.Load<Texture2D>("Images/Sprites/PowerMeter");
            powerArrowImg = Content.Load<Texture2D>("Images/Sprites/PowerArrow");
            trajectoryArrowImg = Content.Load<Texture2D>("Images/Sprites/TrajectoryArrow4");
            windArrowsImg[RIGHT] = Content.Load<Texture2D>("Images/Sprites/WindArrowRight");
            windArrowsImg[LEFT] = Content.Load<Texture2D>("Images/Sprites/WindArrowLeft");
            windArrowsImg[NONE] = Content.Load<Texture2D>("Images/Sprites/NoWind");

            //Load audio
            buttonClickSnd = Content.Load<SoundEffect>("Audio/Sound/Button Click");
            splashSnd = Content.Load<SoundEffect>("Audio/Sound/SplashSound");
            throwSnd = Content.Load<SoundEffect>("Audio/Sound/ThrowSound");
            circusMusic = Content.Load<Song>("Audio/Music/CircusMusic");
            circusMusic2 = Content.Load<Song>("Audio/Music/CircusMusic2");

            //Set up image bounding rectangles
            bgRecs[MENU] = new Rectangle(0, 0, screenWidth, screenHeight);
            bgRecs[HIGHSCORES] = new Rectangle(screenWidth / 2 - bgImgs[HIGHSCORES].Width / 2, screenHeight / 2 - bgImgs[HIGHSCORES].Height / 2, bgImgs[HIGHSCORES].Width, bgImgs[HIGHSCORES].Height);
            bgRecs[PLAY] = new Rectangle(0, 0, screenWidth, screenHeight);
            bgRecs[GAME_OVER] = new Rectangle(0, 0, screenWidth, screenHeight);
            buttonRecs[EASYBTN] = new Rectangle(screenWidth / 2 - buttonImgs[EASYBTN].Width / 2, 310, buttonImgs[EASYBTN].Width, buttonImgs[EASYBTN].Height);
            buttonRecs[HARDBTN] = new Rectangle(screenWidth / 2 - buttonImgs[HARDBTN].Width / 2, 385, buttonImgs[HARDBTN].Width, buttonImgs[HARDBTN].Height);
            buttonRecs[HIGHSCOREBTN] = new Rectangle(screenWidth / 2 - buttonImgs[HIGHSCOREBTN].Width / 2, 460, buttonImgs[HIGHSCOREBTN].Width, buttonImgs[HIGHSCOREBTN].Height);
            buttonRecs[EXITBTN] = new Rectangle(screenWidth / 2 - buttonImgs[EXITBTN].Width / 2, 540, buttonImgs[EXITBTN].Width, buttonImgs[EXITBTN].Height);
            buttonRecs[MENUBTN] = new Rectangle(screenWidth / 2 - buttonImgs[MENUBTN].Width / 2, 700, buttonImgs[MENUBTN].Width, buttonImgs[MENUBTN].Height);
            bucketRecs[LARGE] = new Rectangle(500, 640, bucketImgs[LARGE].Width, bucketImgs[LARGE].Height);
            bucketRecs[MEDIUM] = new Rectangle(720, 673, bucketImgs[MEDIUM].Width, bucketImgs[MEDIUM].Height);
            bucketRecs[SMALL] = new Rectangle(900, 720, bucketImgs[SMALL].Width, bucketImgs[SMALL].Height);
            sinkBucketRecs[LARGE] = new Rectangle(500, 650, bucketImgs[LARGE].Width, bucketImgs[LARGE].Height / 5);
            sinkBucketRecs[MEDIUM] = new Rectangle(720, 678, bucketImgs[MEDIUM].Width, bucketImgs[MEDIUM].Height / 5);
            sinkBucketRecs[SMALL] = new Rectangle(900, 723, bucketImgs[SMALL].Width, bucketImgs[SMALL].Height / 5);
            ballRec = new Rectangle(10, 743, 24, 24);
            blankRecTop = new Rectangle(0, 5, screenWidth, 40);
            blankRecBottom = new Rectangle(0, 490, screenWidth, 100);
            timeMeterRec = new Rectangle(10, 100, meterImg.Width, meterImg.Height);
            actualTimeMeterRec = new Rectangle(10, 100, meterImg.Width, meterImg.Height);
            speedMeterRec = new Rectangle(50, 100, meterImg.Width, meterImg.Height);
            actualSpeedMeterRec = new Rectangle(50, 100, meterImg.Width, 0);
            powerArrowRec = new Rectangle(82, 85, powerArrowImg.Width, powerArrowImg.Height);
            trajectoryArrowRec = new Rectangle(0, 0, trajectoryArrowImg.Width, trajectoryArrowImg.Height);
            windArrowsRec[RIGHT] = new Rectangle(screenWidth / 2 - windArrowsImg[RIGHT].Width / 2, 45, windArrowsImg[RIGHT].Width, windArrowsImg[RIGHT].Height);
            windArrowsRec[LEFT] = new Rectangle(screenWidth / 2 - windArrowsImg[LEFT].Width / 2, 45, windArrowsImg[LEFT].Width, windArrowsImg[LEFT].Height);
            windArrowsRec[NONE] = new Rectangle(screenWidth / 2 - windArrowsImg[NONE].Width / 2, 45, windArrowsImg[NONE].Width, windArrowsImg[NONE].Height);
           
            //Set true position of ball
            ballPos = new Vector2(ballRec.X, ballRec.Y);

            //Set true position of the trajectory arrow and the origin point of which it is rotating around
            trajectoryArrowPos = new Vector2(20, 755);
            origin = new Vector2(0, trajectoryArrowRec.Height / 2f);

            //Set true position of splash
            splashPos[0] = new Vector2(bucketRecs[0].X - 10, bucketRecs[0].Y - 90);
            splashPos[1] = new Vector2(bucketRecs[1].X, bucketRecs[1].Y - 60);
            splashPos[2] = new Vector2(bucketRecs[2].X - 5, bucketRecs[2].Y - 35);

            //Set up splash animation 
            splashAnims[0] = new Animation(splashImg, 9, 5, 44, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 1, splashPos[0], 0.75f, false);
            splashAnims[1] = new Animation(splashImg, 9, 5, 44, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 1, splashPos[1], 0.5f, false);
            splashAnims[2] = new Animation(splashImg, 9, 5, 44, 0, Animation.NO_IDLE, Animation.ANIMATE_ONCE, 1, splashPos[2], 0.3f, false);

            //Set timer
            shotTimer = new Timer(15000, false);

            //Set audio
            MediaPlayer.Volume = 0.7f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(circusMusic);
            SoundEffect.MasterVolume = 1f;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //Update mouse states
            prevMouse = mouse;
            mouse = Mouse.GetState();

            //Update keyboard states
            prevKb = kb;
            kb = Keyboard.GetState();

            //Update animations
            splashAnims[0].Update(gameTime);
            splashAnims[1].Update(gameTime);
            splashAnims[2].Update(gameTime);

            //Update the current gamestate
            switch (gameState)
            {
                case MENU:
                    UpdateMenu();
                    break;
                case HIGHSCORES:
                    UpdateHighscores();
                    break;
                case PLAY:
                    UpdateGame(gameTime);
                    break;
                case PAUSE:
                    UpdatePause();
                    break;
                case GAME_OVER:
                    UpdateEndGame();
                    break;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            //Draw the current gamestate
            switch (gameState)
            {
                case MENU:
                    DrawMenu();
                    break;
                case HIGHSCORES:
                    DrawHighscores();
                    break;
                case PLAY:
                    DrawGame();
                    break;
                case PAUSE:
                    DrawPause();
                    break;
                case GAME_OVER:
                    DrawEndGame();
                    break;
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Update subprogram implementation

        //Pre: None
        //Post: None
        //Desc: Update the menu
        private void UpdateMenu()
        {
            //Change to the next screen or exit if a button is pressed
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
            {
                //Change to the play or highscore screen or exit the game
                if (buttonRecs[EASYBTN].Contains(mouse.Position))
                {
                    //Change the screen to the game in easy mode
                    gameState = PLAY;
                    modes[EASY] = true;

                    //Set the audio
                    MediaPlayer.Volume = 0.1f;
                    buttonClickSnd.CreateInstance().Play();

                    //Reset game to initial values
                    isHighscoresChanged[EASY] = false;
                    isHighscoresChanged[HARD] = false;
                    ResetGame();
                    streak = 0;
                    score = 0;
                    cntGamePlay = 0;
                }
                else if (buttonRecs[HARDBTN].Contains(mouse.Position))
                {
                    //Change the screen to the game in hard mode
                    gameState = PLAY;
                    modes[HARD] = true;

                    //Set the audio
                    MediaPlayer.Volume = 0.1f;
                    buttonClickSnd.CreateInstance().Play();

                    //Reset game to initial values
                    shotTimer.ResetTimer(true);
                    isHighscoresChanged[EASY] = false;
                    isHighscoresChanged[HARD] = false;
                    ResetGame();
                    wind = rng.Next(0, 16);
                    windDir = rng.Next(1, 3);
                    streak = 0;
                    score = 0;
                    cntGamePlay = 0;
                }
                else if (buttonRecs[HIGHSCOREBTN].Contains(mouse.Position))
                {
                    //Change the screen to the highscores
                    gameState = HIGHSCORES;
                    buttonClickSnd.CreateInstance().Play();
                }
                else if (buttonRecs[EXITBTN].Contains(mouse.Position))
                {
                    //Exit the game
                    Exit();
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Update the highscores
        private void UpdateHighscores()
        {
            //Change to the menu screen if button is pressed
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
            {
                //Change to menu screen
                if (buttonRecs[MENUBTN].Contains(mouse.Position))
                {
                    //Change the screen to the menu
                    gameState = MENU;

                    //Set the audio
                    buttonClickSnd.CreateInstance().Play();
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Update the game
        private void UpdateGame(GameTime gameTime)
        {
            shotTimer.Update(gameTime.ElapsedGameTime.TotalMilliseconds);

            //Change to the pause screen if p key is pressed
            if (kb.IsKeyDown(Keys.P) && !prevKb.IsKeyDown(Keys.P))
            {
                //Change to pause screen
                gameState = PAUSE;

                //Pause music
                MediaPlayer.Pause();
            }

            //Modify the HUD meter(s)
            ModifyMeters();

            //Set the wind direction when the game is in hard mode
            if (modes[HARD] == true)
            {
                //Set the direction based on numbers generated from RNG
                if (windDir == 1)
                {
                    //Set direction to the right
                    windDir = POS;
                }
                else if (windDir == 2)
                {
                    //Set direction the the left
                    windDir = NEG;
                }
                //Calculate the total wind and direction it will travel in
                windTotal = wind * windDir / 100;
            }
            //Process the ball's angle, speed and movement based on user input
            AngleProcess();
            SpeedProcess();
            MovementProcess();
        }

        //Pre: None
        //Post: None
        //Desc: Update the pause
        private void UpdatePause()
        {
            //Change to the play screen if p key is pressed
            if (kb.IsKeyDown(Keys.P) && !prevKb.IsKeyDown(Keys.P))
            {
                //Change to play screen
                gameState = PLAY;

                //Resume music
                MediaPlayer.Resume();
            }
        }

        //Pre: None
        //Post: None
        //Desc: Update the end game
        private void UpdateEndGame()
        {
            //Change to the menu screen if the button is pressed
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton != ButtonState.Pressed)
            {
                //Change to menu screen
                if (buttonRecs[MENUBTN].Contains(mouse.Position))
                {
                    //Change the screen to the menu
                    gameState = MENU;

                    //Set audio
                    MediaPlayer.Stop();
                    MediaPlayer.Play(circusMusic);
                    buttonClickSnd.CreateInstance().Play();

                    //Set both statuses of the game modes to inactive
                    modes[EASY] = false;
                    modes[HARD] = false;
                }
            }
        }
        #endregion

        #region Draw subprogram implemenation
        //Pre: None
        //Post: None
        //Desc: Display the menu
        private void DrawMenu()
        {
            //Display background
            spriteBatch.Draw(bgImgs[MENU], bgRecs[MENU], Color.White);

            //Display buttons
            spriteBatch.Draw(buttonImgs[EASYBTN], buttonRecs[EASYBTN], Color.White);
            spriteBatch.Draw(buttonImgs[HARDBTN], buttonRecs[HARDBTN], Color.White);
            spriteBatch.Draw(buttonImgs[HIGHSCOREBTN], buttonRecs[HIGHSCOREBTN], Color.White);
            spriteBatch.Draw(buttonImgs[EXITBTN], buttonRecs[EXITBTN], Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Display the highscores
        private void DrawHighscores()
        {
            //Display backgrounds
            spriteBatch.Draw(bgImgs[MENU], bgRecs[MENU], Color.White);
            spriteBatch.Draw(bgImgs[HIGHSCORES], bgRecs[HIGHSCORES], Color.White);

            //Display button
            spriteBatch.Draw(buttonImgs[MENUBTN], buttonRecs[MENUBTN], Color.White);

            //Display information about highscores (text)
            spriteBatch.DrawString(fonts[TITLE], "SCORES", new Vector2(377, 102), Color.Black);
            spriteBatch.DrawString(fonts[TITLE], "SCORES", new Vector2(375, 100), Color.DarkBlue);
            spriteBatch.DrawString(fonts[DISPLAY], "EASY MODE", new Vector2(392, 302), Color.Black);
            spriteBatch.DrawString(fonts[DISPLAY], "EASY MODE", new Vector2(390, 300), Color.HotPink);
            spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[EASY]), new Vector2(482, 337), Color.Black);
            spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[EASY]), new Vector2(480, 335), Color.HotPink);
            spriteBatch.DrawString(fonts[DISPLAY], "HARD MODE", new Vector2(392, 402), Color.Black);
            spriteBatch.DrawString(fonts[DISPLAY], "HARD MODE", new Vector2(390, 400), Color.Red);
            spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[HARD]), new Vector2(482, 437), Color.Black);
            spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[HARD]), new Vector2(480, 435), Color.Red);
        }

        //Pre: None
        //Post: None
        //Desc: Display the game
        private void DrawGame()
        {
            //Display backgrounds
            spriteBatch.Draw(bgImgs[PLAY], bgRecs[PLAY], Color.White);
            spriteBatch.Draw(blankImg, blankRecTop, Color.LightSalmon * 0.5f);

            //Display game statistic information (text)
            spriteBatch.DrawString(fonts[BASIC], "Angle (deg): ", new Vector2(62, 7), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], "Angle (deg): ", new Vector2(60, 5), Color.Yellow);
            spriteBatch.DrawString(fonts[BASIC], Convert.ToString(angle), new Vector2(232, 7), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], Convert.ToString(angle), new Vector2(230, 5), Color.Red);
            spriteBatch.DrawString(fonts[BASIC], "Speed: ", new Vector2(287, 7), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], "Speed: ", new Vector2(285, 5), Color.Yellow);
            spriteBatch.DrawString(fonts[BASIC], Convert.ToString(Math.Round(speed, 1)), new Vector2(387, 7), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], Convert.ToString(Math.Round(speed, 1)), new Vector2(385, 5), Color.Red);
            spriteBatch.DrawString(fonts[BASIC], "Throws: ", new Vector2(452, 7), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], "Throws: ", new Vector2(450, 5), Color.Yellow);
            spriteBatch.DrawString(fonts[BASIC], Convert.ToString(5 - cntGamePlay), new Vector2(552, 7), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], Convert.ToString(5 - cntGamePlay), new Vector2(550, 5), Color.Red);
            spriteBatch.DrawString(fonts[BASIC], "Score: ", new Vector2(592, 7), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], "Score: ", new Vector2(590, 5), Color.Yellow);
            spriteBatch.DrawString(fonts[BASIC], Convert.ToString(score), new Vector2(682, 7), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], Convert.ToString(score), new Vector2(680, 5), Color.Red);
            spriteBatch.DrawString(fonts[BASIC], "Streak: " + Convert.ToString(streak), new Vector2(902, 52), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], "Streak: " + Convert.ToString(streak), new Vector2(900, 50), Color.White);
            spriteBatch.DrawString(fonts[BASIC], "PRESS P TO PAUSE", new Vector2(802, 82), Color.Black);
            spriteBatch.DrawString(fonts[BASIC], "PRESS P TO PAUSE", new Vector2(800, 80), Color.White);

            //Display the current highscore and images based on the game mode
            if (modes[HARD] == true)
            {
                //Display the hard mode high score and time meter 
                spriteBatch.DrawString(fonts[BASIC], "Highscore: " + highscores[HARD], new Vector2(752, 7), Color.Black);
                spriteBatch.DrawString(fonts[BASIC], "Highscore: " + highscores[HARD], new Vector2(750, 5), Color.RoyalBlue);
                spriteBatch.Draw(blankImg, actualTimeMeterRec, Color.Red);
                spriteBatch.Draw(meterImg, timeMeterRec, Color.White);

                //Display the wind arrow and the value of wind based on its value
                if (windTotal > 0)
                {
                    //Display the wind to the right in different colours based on its magnitude
                    if (windTotal < 0.05)
                    {
                        //Display the arrow to the right but dark orange
                        spriteBatch.Draw(windArrowsImg[RIGHT], windArrowsRec[RIGHT], Color.DarkOrange);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(452, 87), Color.Black);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(450, 85), Color.DarkOrange);
                    }
                    else if (windTotal >= 0.05 && windTotal < 0.1)
                    {
                        //Display the arrow to the right but red
                        spriteBatch.Draw(windArrowsImg[RIGHT], windArrowsRec[RIGHT], Color.Red);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(452, 87), Color.Black);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(450, 85), Color.Red);
                    }
                    else if (windTotal >= 0.1)
                    {
                        //Display the arrow to the right but dark red
                        spriteBatch.Draw(windArrowsImg[RIGHT], windArrowsRec[RIGHT], Color.DarkRed);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(452, 87), Color.Black);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(450, 85), Color.DarkRed);
                    }
                }
                else if (windTotal < 0)
                {
                    //Display the wind to the left in different colours based on its magnitude
                    if (windTotal > -0.05)
                    {
                        //Display the arrow to the left but dark orange
                        spriteBatch.Draw(windArrowsImg[LEFT], windArrowsRec[LEFT], Color.DarkOrange);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(452, 87), Color.Black);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(450, 85), Color.DarkOrange);
                    }
                    else if (windTotal <= -0.05 && windTotal > -0.1)
                    {
                        //Display the arrow to the left but red
                        spriteBatch.Draw(windArrowsImg[LEFT], windArrowsRec[LEFT], Color.Red);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(452, 87), Color.Black);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(450, 85), Color.Red);
                    }
                    else if (windTotal <= -0.1)
                    {
                        //Display the arrow to the left but dark red
                        spriteBatch.Draw(windArrowsImg[LEFT], windArrowsRec[LEFT], Color.DarkRed);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(452, 87), Color.Black);
                        spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(450, 85), Color.DarkRed);
                    }
                }
                else
                {
                    //Display the no wind image
                    spriteBatch.Draw(windArrowsImg[NONE], windArrowsRec[NONE], Color.White);
                    spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(452, 87), Color.Black);
                    spriteBatch.DrawString(fonts[BASIC], "WIND: " + Convert.ToString(windTotal), new Vector2(450, 85), Color.White);
                }
            }
            else if (modes[EASY] == true)
            {
                //Display the easy mode high score
                spriteBatch.DrawString(fonts[BASIC], "Highscore: " + highscores[EASY], new Vector2(752, 7), Color.Black);
                spriteBatch.DrawString(fonts[BASIC], "Highscore: " + highscores[EASY], new Vector2(750, 5), Color.RoyalBlue);
            }

            //Display the HUD meters and arrow and in-game objects
            spriteBatch.Draw(blankImg, actualSpeedMeterRec, Color.Lerp(Color.Yellow, Color.DarkGreen, speedPercentage));
            spriteBatch.Draw(ballImg, ballRec, Color.White);
            spriteBatch.Draw(meterImg, speedMeterRec, Color.White);
            spriteBatch.Draw(powerArrowImg, powerArrowRec, Color.Lerp(Color.Yellow, Color.DarkGreen, speedPercentage));
            spriteBatch.Draw(trajectoryArrowImg, trajectoryArrowPos, trajectoryArrowRec, Color.White, MathHelper.ToRadians(angle * -1f), origin, 1.0f, SpriteEffects.None, 1f);
            spriteBatch.Draw(bucketImgs[LARGE], bucketRecs[LARGE], Color.White);
            spriteBatch.Draw(bucketImgs[MEDIUM], bucketRecs[MEDIUM], Color.White);
            spriteBatch.Draw(bucketImgs[SMALL], bucketRecs[SMALL], Color.White);
            
            //Display invisible collision boxes
            spriteBatch.Draw(blankImg, sinkBucketRecs[LARGE], Color.White * 0f);
            spriteBatch.Draw(blankImg, sinkBucketRecs[MEDIUM], Color.White * 0f);
            spriteBatch.Draw(blankImg, sinkBucketRecs[SMALL], Color.White * 0f);

            //Display splash animations
            splashAnims[0].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
            splashAnims[1].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
            splashAnims[2].Draw(spriteBatch, Color.White, Animation.FLIP_NONE);
        }

        //Pre: None
        //Post: None
        //Desc: Display the pause
        private void DrawPause()
        {
            //Display the background
            spriteBatch.Draw(bgImgs[PLAY], bgRecs[PLAY], Color.White);

            //Display information about the pause
            spriteBatch.DrawString(fonts[TITLE], "PAUSED", new Vector2(377, 302), Color.Black);
            spriteBatch.DrawString(fonts[TITLE], "PAUSED", new Vector2(375, 300), Color.DarkBlue);
            spriteBatch.DrawString(fonts[DISPLAY], "PRESS P TO UNPAUSE", new Vector2(302, 402), Color.Black);
            spriteBatch.DrawString(fonts[DISPLAY], "PRESS P TO UNPAUSE", new Vector2(300, 400), Color.White);
        }

        //Pre: None
        //Post: None
        //Desc: Display the end game
        private void DrawEndGame()
        {
            //Display the background
            spriteBatch.Draw(bgImgs[GAME_OVER], bgRecs[GAME_OVER], Color.White);

            //Display the information about the game that just occurred 
            spriteBatch.DrawString(fonts[TITLE], "GAME OVER", new Vector2(317, 202), Color.Black);
            spriteBatch.DrawString(fonts[TITLE], "GAME OVER", new Vector2(315, 200), Color.DarkBlue);
            spriteBatch.DrawString(fonts[DISPLAY], "FINAL SCORE", new Vector2(392, 302), Color.Black);
            spriteBatch.DrawString(fonts[DISPLAY], "FINAL SCORE", new Vector2(390, 300), Color.HotPink);
            spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(score), new Vector2(487, 332), Color.Black);
            spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(score), new Vector2(485, 330), Color.HotPink);

            //Display the highscores based on the mode played
            if (modes[EASY])
            {
                //Display the new or old easy highscore
                if (isHighscoresChanged[EASY] == true)
                {
                    //Display the new highscore
                    spriteBatch.DrawString(fonts[DISPLAY], "HIGH SCORE", new Vector2(392, 372), Color.Black);
                    spriteBatch.DrawString(fonts[DISPLAY], "HIGH SCORE", new Vector2(390, 370), Color.Red);
                    spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[EASY]), new Vector2(487, 402), Color.Black);
                    spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[EASY]), new Vector2(485, 400), Color.Red);
                    spriteBatch.Draw(blankImg, blankRecBottom, Color.AliceBlue * 0.5f);
                    spriteBatch.DrawString(fonts[TITLE], ">> NEW HIGH SCORE <<", new Vector2(162, 492), Color.Black);
                    spriteBatch.DrawString(fonts[TITLE], ">> NEW HIGH SCORE <<", new Vector2(160, 490), Color.Yellow);
                }
                else if (!isHighscoresChanged[EASY] && isHighscoresChanged[HARD] == false)
                {
                    //Display the old highscore
                    spriteBatch.DrawString(fonts[DISPLAY], "HIGH SCORE", new Vector2(392, 372), Color.Black);
                    spriteBatch.DrawString(fonts[DISPLAY], "HIGH SCORE", new Vector2(390, 370), Color.Red);
                    spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[EASY]), new Vector2(487, 402), Color.Black);
                    spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[EASY]), new Vector2(485, 400), Color.Red);
                }
            }
            else if (modes[HARD])
            {
                //Display the new or old hard highscore
                if (isHighscoresChanged[HARD] == true)
                {
                    //Display the new highscore
                    spriteBatch.DrawString(fonts[DISPLAY], "HIGH SCORE", new Vector2(392, 372), Color.Black);
                    spriteBatch.DrawString(fonts[DISPLAY], "HIGH SCORE", new Vector2(390, 370), Color.Red);
                    spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[HARD]), new Vector2(482, 402), Color.Black);
                    spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[HARD]), new Vector2(480, 400), Color.Red);
                    spriteBatch.Draw(blankImg, blankRecBottom, Color.AliceBlue * 0.5f);
                    spriteBatch.DrawString(fonts[TITLE], ">> NEW HIGH SCORE <<", new Vector2(162, 492), Color.Black);
                    spriteBatch.DrawString(fonts[TITLE], ">> NEW HIGH SCORE <<", new Vector2(160, 490), Color.Yellow);
                }
                else if (!isHighscoresChanged[HARD] && isHighscoresChanged[EASY] == false)
                {
                    //Display the old highscore
                    spriteBatch.DrawString(fonts[DISPLAY], "HIGH SCORE", new Vector2(392, 372), Color.Black);
                    spriteBatch.DrawString(fonts[DISPLAY], "HIGH SCORE", new Vector2(390, 370), Color.Red);
                    spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[HARD]), new Vector2(482, 402), Color.Black);
                    spriteBatch.DrawString(fonts[DISPLAY], Convert.ToString(highscores[HARD]), new Vector2(480, 400), Color.Red);
                }
            }
            spriteBatch.Draw(buttonImgs[MENUBTN], buttonRecs[MENUBTN], Color.White);
        }
        #endregion

        #region Helper subprograms
        //Pre: None
        //Post: None
        //Desc: Reset the game
        private void ResetGame()
        {
            //Set the status of the ball to not thrown yet
            isBallThrown = false;

            //Reset the location of the ball
            ballRec = Rectangle.Empty;
            ballRec = new Rectangle(10, 743, 24, 24);
            ballPos = new Vector2(ballRec.X, ballRec.Y);

            //Reset the angle and speed of the ball
            angle = 10;
            speed = 5;
        }

        //Pre: None
        //Post: None
        //Desc: Modify the HUD meter(s)
        private void ModifyMeters()
        {
            //Calculate the current percentage of speed to determine how much the speed meter and power arrow move and then apply changes to the rectangle
            speedPercentage = (speed - 5) / (MAX_SPEED - 5);
            actualSpeedMeterRec.Height = (int)(speedMeterRec.Height * (speedPercentage));
            actualSpeedMeterRec.Y = (speedMeterRec.Y + 211) - (int)(speedMeterRec.Height * (speedPercentage));
            powerArrowRec.Y = (speedMeterRec.Y + 211) - (int)(speedMeterRec.Height * (speedPercentage)) - 15;

            //Include a time meter if the game is in hard mode
            if (modes[HARD])
            {
                //Calculate how much the time meter moves and then apply changes to the rectangle 
                actualTimeMeterRec.Height = (int)(timeMeterRec.Height * (shotTimer.GetTimeRemaining() / MAX_TIME));
                actualTimeMeterRec.Y = (timeMeterRec.Y + 211) - (int)(timeMeterRec.Height * (shotTimer.GetTimeRemaining() / MAX_TIME));
            }
        }

        //Pre: None
        //Post: None
        //Desc: Set angle based on user input
        private void AngleProcess()
        {
            //Adjust angle based on keys pressed
            if (kb.IsKeyDown(Keys.Up))
            {
                //Angle increases by 2 when up key is pressed
                angle += 2f;
                
                //Prevent the angle from exceeding 90 degrees
                if (angle > 90f)
                {
                    //Set the angle to exactly 90 degrees
                    angle = 90f;
                }
            }
            else if (kb.IsKeyDown(Keys.Down))
            {
                //Angle decreases by 2 when up key is pressed
                angle -= 2f;

                //Prevent the angle from going under 10 degrees
                if (angle < 10f)
                {
                    //Set the angle to exactly 10 degrees
                    angle = 10f;
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Set speed based on user input or automatically
        private void SpeedProcess()
        {
            //Set speeds based on easy or hard mode
            if (modes[EASY] == true)
            {
                //Set speeds based on user input where up key increases the speed and down key decreases the speed
                if (kb.IsKeyDown(Keys.Right))
                {
                    //Increase the speed
                    speed += updateSpeed;

                    //Prevent the speed from exceeding the max speed
                    if (speed > MAX_SPEED)
                    {
                        //Set the speed to the exact max speed
                        speed = MAX_SPEED;
                    }
                }
                else if (kb.IsKeyDown(Keys.Left))
                {
                    //Decrease the speed
                    speed -= updateSpeed;

                    //Prevent the speed from going under the min speed
                    if (speed < MIN_SPEED)
                    {
                        //Set the speed to the exact min speed
                        speed = MIN_SPEED;
                    }
                }
            }
            else if (modes[HARD] == true && !shotTimer.IsFinished() && isBallThrown == false)
            {
                //Sets status of speed to update speeds automatically
                if (speed <= MIN_SPEED)
                {
                    //Set speed under min speed to true
                    speeds[UNDER] = true;
                    speeds[OVER] = false;
                }
                else if (speed >= MAX_SPEED)
                {
                    //Set speed over max speed to true
                    speeds[OVER] = true;
                    speeds[UNDER] = false;
                }

                //Sets speeds automatically based on status of speed
                if (speeds[UNDER] == true)
                {
                    //Increase the speed
                    speed += updateSpeed;
                }
                else if (speeds[OVER] == true)
                {
                    //Decrease the speed
                    speed -= updateSpeed;
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Control the ball movement based on the speed, angle, and objects it hits
        private void MovementProcess()
        {
            //Given that ball has not yet been thrown, throw the ball by pressing space key OR automatically if the shot timer finishes 
            if ((kb.IsKeyDown(Keys.Space) && !prevKb.IsKeyDown(Keys.Space) && isBallThrown == false) || (modes[HARD] == true && shotTimer.IsFinished() && isBallThrown == false))
            {
                //Set status of ball to thrown
                isBallThrown = true;

                //Set audio
                throwSnd.CreateInstance().Play();

                //Prevent splash animations from animating if it has not hit a bucket
                splashAnims[0].isAnimating = false;
                splashAnims[1].isAnimating = false;
                splashAnims[2].isAnimating = false;

                //Calculate the X and Y components of the speed
                ballSpeed.X = (float)(speed * Math.Cos(angle * (Math.PI / 180)));
                ballSpeed.Y = (float)(-1 * speed * Math.Sin(angle * (Math.PI / 180)));
            }

            //Apply forces to ball and effects of the ball hitting objects if the ball is thrown
            if (isBallThrown == true)
            {
                //Set status of each individual game round
                bool isDone = false;

                //Account for wind if the game is in hard mode
                if (modes[HARD] == true)
                {
                    //Apply the force of wind to the X component of the ball speed
                    ballSpeed.X += windTotal;

                    //Deactivate the shot timer since shot has been thrown
                    shotTimer.Deactivate();
                }

                //Apply the force of gravity to the Y component of the ball speed 
                ballSpeed.Y += GRAVITY;

                //Update the location of the ball based on its new speeds and direction
                ballPos.X += ballSpeed.X * dirX;
                ballPos.Y += ballSpeed.Y;
                ballRec.X = (int)ballPos.X;
                ballRec.Y = (int)ballPos.Y;

                //Change the speed or direction of the ball's components if it hits an object.
                if (ballRec.Top <= 0)
                {
                    //Change the speed of the ball's Y component if its speed is below 0
                    if (ballSpeed.Y < 0)
                    {
                        //Set the speed of the ball's Y component to 0
                        ballSpeed.Y = 0;
                    }
                }
                else if (ballRec.Right >= screenWidth || ballRec.Left <= 0)
                {
                    //Change the direction of the speed of the ball's X component 
                    dirX *= NEG;
                }
                else if (ballRec.Bottom >= screenHeight)
                {
                    //Reset the game 
                    ResetGame();
                    dirX = POS;
                    streak = 0;
                    cntGamePlay++;

                    //Set status of round to over
                    isDone = true;

                    //Reset the hard mode components
                    if (modes[HARD] == true)
                    {
                        //Reset the wind and shot timer
                        wind = rng.Next(0, 16);
                        windDir = rng.Next(1, 3);
                        shotTimer.ResetTimer(true);
                    }
                }
                else
                {
                    //Set the status of the round to over if the ball hits the bucket
                    isDone = BallSinkBucket();
                }

                //Change to game over screen if the status of the final round is over and the 5 throws were performed
                if (isDone && cntGamePlay == MAXIMUM_PLAYS)
                {
                    //Check for a new highscore
                    CheckHighscore();

                    //Change to game over screen
                    gameState = GAME_OVER;

                    //Set audio
                    MediaPlayer.Stop();
                    MediaPlayer.Play(circusMusic2);
                }
            }
        }

        //Pre: None
        //Post: Return if the ball has sunk or not
        //Desc: Determine when the ball sinks and what happens when it sinks
        private bool BallSinkBucket()
        {
            //Set status of whether ball has sunk bucket not sunken and initialize the bucket number it hits
            bool result = false;
            int bucketNo = 0;

            //Set the bucket number based on the bucket the ball hits
            if (ballRec.Intersects(sinkBucketRecs[0]))
            {
                //Set bucket number to 1
                bucketNo = 1;
            }
            else if (ballRec.Intersects(sinkBucketRecs[1]))
            {
                //Set bucket number to 2
                bucketNo = 2;
            }
            else if (ballRec.Intersects(sinkBucketRecs[2]))
            {
                //Set bucket number to 3
                bucketNo = 3;
            }

            //Adjust the score and animations by bucket number
            AdjustBallByBucketNo(bucketNo);

            //If the ball hits a bucket, then set the status of the ball to sunken.
            if (bucketNo > 0)
                result = true;

            return result;

        }

        //Pre: A bucket is hit by the ball
        //Post: None
        //Desc: Adjust the score, streak, animation, and game reset based on the bucket the ball hits
        /// <summary>
        /// adjust score and animation by bucket no
        /// </summary>
        /// <param name="bucketNo"></param>
        private void AdjustBallByBucketNo(int bucketNo)
        {
            //Reset the game and increase score and streak when the ball hits a bucket
            if (bucketNo > 0)
            {
                //Reset the game and wind
                ResetGame();
                wind = rng.Next(0, 16);
                windDir = rng.Next(1, 3);

                //Increase the streak and counter for amount of rounds played
                streak += 1;
                cntGamePlay++;

                //Set audio (play splash sound)
                splashSnd.CreateInstance().Play();

                //Reset timer if the game mode is hard
                if (modes[HARD] == true)
                {
                    //Reset the shot timer
                    shotTimer.ResetTimer(true);
                }

                //Animate the corresponding splash animation and increase the score according to the bucket hit
                switch (bucketNo)
                {
                    case 1:
                        //Animate first splash animation
                        splashAnims[0].isAnimating = true;

                        //Increase score 
                        score += 100 * streak;
                        break;
                    case 2:
                        //Animate second splash animation
                        splashAnims[1].isAnimating = true;

                        //Increase score 
                        score += 200 * streak;
                        break;
                    case 3:
                        //Animate third splash animation
                        splashAnims[2].isAnimating = true;

                        //Increase score 
                        score += 400 * streak;
                        break;
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Check for a new highscore and replace the old one if there is a new one
        private void CheckHighscore()
        {
            //Check the highscore based on the game mode
            if (modes[EASY] == true)
            {
                //Change the highscore if the current easy mode score is higher than past easy mode highscores
                if (score > highscores[EASY])
                {
                    //Change new easy highscore to current score
                    highscores[EASY] = score;
                    isHighscoresChanged[EASY] = true;
                }
            }
            else if (modes[HARD] == true)
            {
                //Change the highscore if the current hard mode score is higher than past hard mode highscores
                if (score > highscores[HARD])
                {
                    //Change new hard highscore to current score
                    highscores[HARD] = score;
                    isHighscoresChanged[HARD] = true;
                }
            }
        }
        #endregion
    }
}
