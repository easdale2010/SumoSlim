using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using particle_syatem;
using System.IO;
using gamelib2d;

namespace teamgame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ParticleEngine[] particleEngine = new ParticleEngine[numberofgoodfood];

        int displaywidth = 1200;
        int displayheight = 800;

        float gravity;
        float speed = 0.65f;
        float friction = 0.96f;

        int lives = 6;
        int score = 0;

        SoundEffect soundtrack1;

        SoundEffect bananacollect, death, playerjump, weightgain, weightloss, menuchange, menuselect;
        SoundEffectInstance  music1;
        
        SpriteFont mainfont;
        float jumpcooldown = 0;
        Boolean gameover = false;
        Boolean vicgameover = false;

        animation collectgoodfood, collectbadfood;

        struct sprites2d
        {
            public Texture2D image;                                         //Structure For Sprites
            public Rectangle rect;
            public BoundingBox bbox;
            public BoundingSphere bsphere;
            public Vector3 position;
            public Vector3 oldposition;
            public Vector3 velocity;
            public Vector2 origin;
            public float size;
            public float rotation;
            public Boolean visible;
            public Boolean jumped;
        }
        struct graphics2d
        {
            public Texture2D image;                             //Structure For Graphics
            public Rectangle rect;
        }
        graphics2d background, controls, gameoverimage, vicscreen;
        Random randomiser = new Random();

        int gamestate =-1;
        float gameruntime = 0;

        const int numberofgoodfood = 10;
        const int numberofbadfood = 100;
        sprites2d[] gfood = new sprites2d[numberofgoodfood];
        sprites2d[] bfood = new sprites2d[numberofbadfood];
        MouseState mouse;
        GamePadState[] pad = new GamePadState[4];
        Boolean released = false;
        sprites2d gamebackground,gamebackground2;
        sprites2d sumoslim;
        sprites2d sumoslimmedium;

        const int numberofoptions = 4;
        sprites2d[,] menuoptions = new sprites2d[numberofoptions, 2];
        int optionselected = -1;

        const int numberofhighscores = 10;                              // Number of high scores to store
        int[] highscores = new int[numberofhighscores];                 // Array of high scores
        string[] highscorenames = new string[numberofhighscores];       // Array of high score names
        const int maxnamelength = 30;   // Maximum name length for high score table
        float keycounter = 0;           // Counter for delay between key strokes
        const float keystrokedelay = 200;   // Delay between key strokes in milliseconds
        int lasthighscore = numberofhighscores - 1;

        KeyboardState keys;                             // Variable to hold keyboard state
        KeyboardState lastkeystate;
        Boolean keyboardreleased = true;



        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.graphics.PreferredBackBufferWidth = displaywidth;
            this.graphics.PreferredBackBufferHeight = displayheight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            displaywidth = graphics.GraphicsDevice.Viewport.Width;
            displayheight = graphics.GraphicsDevice.Viewport.Height;

            base.Initialize();
        }

        void loadsprites(ref sprites2d sprites, string spritename, int x, int y, float msize)               //Void To Load Sprites
        {
            sprites.image = Content.Load<Texture2D>(spritename);
            sprites.position = new Vector3((float)x, (float)y, 0);
            sprites.size = msize;
            sprites.rect.Width = (int)(sprites.image.Width * msize);
            sprites.rect.Height = (int)(sprites.image.Height * msize);
            sprites.rect.Y = y;
            sprites.rect.X = x;
            sprites.origin.X = sprites.image.Width / 2;
            sprites.origin.Y = sprites.image.Height / 2;

            sprites.bsphere = new BoundingSphere(sprites.position, sprites.image.Width / 2);
            sprites.bbox = new BoundingBox(new Vector3(sprites.position.X - sprites.rect.Width / 2, sprites.position.Y - sprites.rect.Height / 2, 0),
                new Vector3(sprites.position.X + sprites.rect.Width / 2, sprites.position.Y + sprites.rect.Height / 2, 0));
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);



            // Load in high scores
            if (File.Exists(@"highscore.txt")) // This checks to see if the file exists
            {
                StreamReader sr = new StreamReader(@"highscore.txt");	// Open the file

                String line;		// Create a string variable to read each line into
                for (int i = 0; i < numberofhighscores && !sr.EndOfStream; i++)
                {
                    line = sr.ReadLine();	// Read the first line in the text file
                    highscorenames[i] = line.Trim(); // Read high score name

                    if (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();	// Read the first line in the text file
                        line = line.Trim(); 	// This trims spaces from either side of the text
                        highscores[i] = (int)Convert.ToDecimal(line);	// This converts line to numeric
                    }
                }
                sr.Close();			// Close the file
            }
            Array.Sort(highscores, highscorenames);
            Array.Reverse(highscores);
            Array.Reverse(highscorenames);

            collectgoodfood = new animation(Content, "magic_008", 0, 0, 2, Color.White, false, 26, 3, 5);
            collectbadfood = new animation(Content, "magic_008", 0, 0, 2, Color.Red, false, 26, 3, 5);

            death = Content.Load<SoundEffect>("ballhit");
            mainfont = Content.Load<SpriteFont>("font");                                    //Loads Font              

            background.image = Content.Load<Texture2D>("Main_Menu");                         //Loads Menu Background
            background.rect.Y = 0;
            background.rect.X = 0;
            background.rect.Width = displaywidth;
            background.rect.Height = displayheight;

            controls.image = Content.Load<Texture2D>("controls");                           //Loads Controls
            controls.rect.Y = 0;
            controls.rect.X = 0;
            controls.rect.Width = displaywidth;
            controls.rect.Height = displayheight;

            soundtrack1 = Content.Load<SoundEffect>("explosive_attack");
            music1 = soundtrack1.CreateInstance();
            music1.IsLooped = true;
            music1.Volume = 0.35f;
            

            gamebackground.image = Content.Load<Texture2D>("desert_BG");                    //Loads Game Background
            gamebackground.position.Y = 0;
            gamebackground.position.X = 0;
            gamebackground.rect.Width = displaywidth;
            gamebackground.rect.Height = displayheight;
            gamebackground.velocity = new Vector3(-2, 0, 0);

            gamebackground2.image = Content.Load<Texture2D>("desert_BG");                   //Loads Scrolling Background
            gamebackground2.position.Y = 0;
            gamebackground2.position.X = displaywidth;
            gamebackground2.rect.Width = displaywidth;
            gamebackground2.rect.Height = displayheight;
            gamebackground2.velocity = new Vector3(-2, 0, 0);

            gameoverimage.image = Content.Load<Texture2D>("gameoverimage");
            gameoverimage.rect.Y = 0;
            gameoverimage.rect.X = 0;
            gameoverimage.rect.Width = displaywidth;
            gameoverimage.rect.Height = displayheight;


            vicscreen.image = Content.Load<Texture2D>("victoryscreen1");
            vicscreen.rect.Y = 0;
            vicscreen.rect.X = 0;
            vicscreen.rect.Width = displaywidth;
            vicscreen.rect.Height = displayheight;


            loadsprites(ref menuoptions[0, 0], "START", displaywidth / 2, 200, 1.25f);                    //Loads Menu Options
            loadsprites(ref menuoptions[0, 1], "START LOGO", displaywidth / 2, 200, 1.25f);
            loadsprites(ref menuoptions[1, 0], "CONTROLS (2)", displaywidth / 2, 300, 1.25f);
            loadsprites(ref menuoptions[1, 1], "CONTROLS BUTTON", displaywidth / 2, 300, 1.25f);
            loadsprites(ref menuoptions[2, 0], "HIGH SCORE", displaywidth / 2, 400, 1.25f);
            loadsprites(ref menuoptions[2, 1], "HIGH SCORE LOGO", displaywidth / 2, 400, 1.25f);
            loadsprites(ref menuoptions[3, 0], "EXIT", displaywidth / 2, 500, 1.25f);
            loadsprites(ref menuoptions[3, 1], "EXIT LOGO", displaywidth / 2, 500, 1.25f);
           
            playerjump = Content.Load<SoundEffect>("jump sound 1");
            bananacollect = Content.Load<SoundEffect>("collectgfood");
           
            
           
            menuchange = Content.Load<SoundEffect>("MENU A_Select");                        //Loads Menu Sounds
            menuselect = Content.Load<SoundEffect>("menuSound");


            for (int i = 0; i < numberofbadfood; i++)    // load bad foods
            {
                if (i < 20)
                {
                    bfood[i].image = Content.Load<Texture2D>("chipsPHG");
                   
                }
                if (i >= 20 && i < 40)
                {
                    bfood[i].image = Content.Load<Texture2D>("CheeseburgerRage");
                    
                }
                if (i >= 40 && i < numberofbadfood)
                {
                    bfood[i].image = Content.Load<Texture2D>("evilHotdog");
                  
                }

                bfood[i].origin.Y = bfood[i].image.Height / 2;
                bfood[i].origin.X = bfood[i].image.Width / 2;
                bfood[i].position.Y = 640;
                bfood[i].position.X = 400;
                bfood[i].rect.Width = (int)(bfood[i].image.Width *0.3f);
                bfood[i].rect.Height = (int)(bfood[i].image.Height *0.27f);
                
            }

            for (int i = 0; i < numberofgoodfood; i++)
            {
                gfood[i].image = Content.Load<Texture2D>("banana");
                gfood[i].origin.Y = gfood[i].image.Height / 2;
                gfood[i].origin.X = gfood[i].image.Width / 2;
                gfood[i].position.Y = 300;
                gfood[i].position.X = 1200;
                gfood[i].rect.Width = (int)(gfood[i].image.Width / 2);
                gfood[i].rect.Height = (int)(gfood[i].image.Height / 2);
                gfood[i].velocity = new Vector3(-5, 0, 0);
            }

            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(Content.Load<Texture2D>("circle"));
            textures.Add(Content.Load<Texture2D>("star"));
            textures.Add(Content.Load<Texture2D>("diamond"));
            for (int i = 0; i < numberofgoodfood; i++)
            particleEngine[i] = new ParticleEngine(textures, new Vector2(450, 420));

            spawnfood();

        }
        public void loadchartecter(string spritename)
        {
            sumoslim.image = Content.Load<Texture2D>("spritename");                                              //Loads Player
            sumoslim.origin.Y = sumoslim.image.Height / 2;
            sumoslim.origin.X = sumoslim.image.Width / 2;  

        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // Save high scores
            StreamWriter sw = new StreamWriter(@"highscore.txt");
            for (int i = 0; i < numberofhighscores; i++)
            {
                sw.WriteLine(highscorenames[i]);
                sw.WriteLine(highscores[i].ToString());
            }
            sw.Close();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
                                                                                // Reads Keybaord And Joypad
            keys = Keyboard.GetState();                     // Read keyboard
            keyboardreleased = (keys != lastkeystate);      // Has keyboard input changed
            pad[0] = GamePad.GetState(PlayerIndex.One);

            float timebetweenupdates = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                                                                                        //Allows Game To Exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keys.IsKeyDown(Keys.Escape))
                this.Exit();

            switch (gamestate)
            {
                case -1:
                                                                                //Loads Menu
                    updatemenu();
                    break;
                case 0:                                                         //Calls Update Game To Start
                    updategame(timebetweenupdates);
                    break;
                case 1:                                                        //Calls Update Controls
                    updatecontrols();
                    break;
                case 2:                                                         //Calls Highscore
                    updatehighscore();
                    break;
                case 3:                                                         
                    updateexit();
                    break;
                default:                                                        //Allows To Exit
                    this.Exit();
                    break;
            }

            base.Update(gameTime);
        }

        public void updatemenu()
        {
            if (released)                                                   //Updates Menu Based On User Input
            {
                if (pad[0].ThumbSticks.Left.Y > 0.5f)
                {
                    optionselected--;
                    released = false;
                    menuchange.Play();
                }
                if (pad[0].ThumbSticks.Left.Y < -0.5f)
                {
                    optionselected++;
                    released = false;
                    menuchange.Play();
                }
            }
            else
                if (Math.Abs(pad[0].ThumbSticks.Left.Y) < 0.5f)
                {                                                       
                    released = true;
                }

            for (int i = 0; i < numberofoptions; i++)
            {
                if (optionselected >= numberofoptions) optionselected = numberofoptions - 1;
                if (optionselected < 0) optionselected = 0;

                if (pad[0].Buttons.A == ButtonState.Pressed)
                {                                                        //Calls New Method Based On User Input
                    gamestate = optionselected;
                    if (gamestate == 0)
                    {
                        reset();
                    }
                    menuselect.Play();
                    released = false;
                }
            }
        }

        void reset() // allows the game to be replayed
        {
            gameover = false;
            score = 0;
            lives = 16;

            sumoslim.position.X = 375;
            sumoslim.position.Y = 700;

            gravity = 4.6f;

            reset2("SS_thin",0);

            spawnfood();
        }

        void reset2(string graphicfilename, float neggravity)  // adds grativy to sulmo slim
        {
            sumoslim.image = Content.Load<Texture2D>(graphicfilename);                                              //Loads Player
            sumoslim.origin.Y = sumoslim.image.Height / 2;
            sumoslim.origin.X = sumoslim.image.Width / 2;
            sumoslim.rect.Width = (int)(sumoslim.image.Width / 2);
            sumoslim.rect.Height = (int)(sumoslim.image.Height / 2);

            
            gravity += neggravity;

        }


        public void drawmenu()
        {                                                                                                   //Draws Menu
            spriteBatch.Begin();

            spriteBatch.Draw(background.image, background.rect, Color.White);

            for (int i = 0; i < numberofoptions; i++)
            {
                if (optionselected == i)
                    spriteBatch.Draw(menuoptions[i, 1].image, menuoptions[i, 1].rect, null, Color.White, 0, menuoptions[i, 1].origin, SpriteEffects.None, 0);
                else
                    spriteBatch.Draw(menuoptions[i, 0].image, menuoptions[i, 0].rect, null, Color.White, 0, menuoptions[i, 0].origin, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }

        public void spawnfood()
        {
            bfood[0].position.X = 800;
            bfood[0].position.Y = 640;

            for (int i = 1; i < numberofbadfood; i++)
            {
                bfood[i].visible = true;
                bfood[i].jumped = false;
                bfood[i].position.X = bfood[i - 1].position.X + bfood[i - 1].rect.Width +500 + randomiser.Next(250);
                bfood[i].velocity = new Vector3(-3, 0, 0);
            }

            gfood[0].position.X = 1000;
            gfood[0].position.Y = 300;

            for (int i = 1; i < numberofgoodfood; i++)
            {
                gfood[i].visible = true;
                gfood[i].position.X = gfood[i - 1].position.X + gfood[i - 1].rect.Width * 60 + randomiser.Next(150);
            }
        }

        public void updategame(float gtime)                                                 //Main Game Code (UPDATE Code)
        {
            
            if (!gameover)
            {
                // gives partile engines loccation and update it
                for(int i=0;i<numberofgoodfood;i++)
                    if (gfood[i].visible)
                    {
                        particleEngine[i].EmitterLocation = new Vector2(gfood[i].position.X, gfood[i].position.Y + 50);
                        particleEngine[i].Update();

                    }

                if (music1.State == SoundState.Stopped) music1.Play();

                collectgoodfood.update(gtime);
                collectbadfood.update(gtime);

                // keeps the numberof player lives to 6 or less
                if (lives > 6)
                    lives = 6;

                gameruntime += gtime;
                jumpcooldown -= gtime;

                gamebackground.position += gamebackground.velocity;                             //Scrolls Background
                gamebackground.rect.Y = (int)gamebackground.position.Y;
                gamebackground.rect.X = (int)gamebackground.position.X;

                gamebackground2.position += gamebackground2.velocity;
                gamebackground2.rect.Y = (int)gamebackground2.position.Y;
                gamebackground2.rect.X = (int)gamebackground2.position.X;
                                                                                                          //Screen Scroll
                if (gamebackground.rect.Right < 0)                                     //Once BG1 leaves Screen, Resets To The Right Of BG2
                    gamebackground.position.X = gamebackground2.rect.Right;
                if (gamebackground2.rect.Right < 0)                                    //Once BG2 leaves Screen, Resets To The Right Of BG1
                    gamebackground2.position.X = gamebackground.rect.Right;

                sumoslim.velocity.X += pad[0].ThumbSticks.Left.X * speed;                       //Allows User Control
                sumoslim.position += sumoslim.velocity;


                                                                                                //Sets Players Jump Height
                if (sumoslim.position.Y > displayheight - 125 - sumoslim.rect.Height / 2)
                {
                    sumoslim.position.Y = displayheight - 125 - sumoslim.rect.Height / 2;
                    sumoslim.velocity.Y = 0;
                }
                                                                                                //Sets Control And Makes Player Jump
                if (pad[0].Buttons.A == ButtonState.Pressed && sumoslim.velocity.Y == 0 && released)
                {
                    released = false;
                    sumoslim.velocity.Y -= 60;
                    jumpcooldown = 100;
                    playerjump.Play();
                }
                                                                                                //Allows Player To Jump Again
                if (pad[0].Buttons.A == ButtonState.Released)
                {
                    released = true;
                }   
                                                                                            //Sets Bounderies Within Screen
                if (sumoslim.position.X + sumoslim.rect.Width / 2 >= displaywidth)
                    sumoslim.position.X = displaywidth - sumoslim.rect.Width / 2;
                if (sumoslim.position.X <= sumoslim.rect.Width / 2)                         //Sets Bounderies Within Screen
                    sumoslim.position.X = sumoslim.rect.Width / 2;

                                                                            
                sumoslim.velocity.Y += gravity;
                                                                //Applies Gravity And Friction
                sumoslim.velocity *= friction;
                
                sumoslim.bbox = new BoundingBox(new Vector3(sumoslim.position.X - sumoslim.rect.Width / 2, sumoslim.position.Y - sumoslim.rect.Height / 2, 0),
                        new Vector3(sumoslim.position.X + sumoslim.rect.Width / 2, sumoslim.position.Y + sumoslim.rect.Height / 2, 0));  // bounding box for player

                                                                //Sets Rectangle To The Position
                sumoslim.rect.Y = (int)sumoslim.position.Y;
                sumoslim.rect.X = (int)sumoslim.position.X;


                for (int i = 0; i < numberofbadfood; i++)       //Bounding box for bad foods and adds velocity, sets rectangle to the position for bad food
                {
                    bfood[i].velocity *= 1.0001f;
                    bfood[i].position += bfood[i].velocity;             //bad food moves based on velocity
                    bfood[i].rect.Y = (int)bfood[i].position.Y;         //sets rectangle to position
                    bfood[i].rect.X = (int)bfood[i].position.X;         //sets rectangle to position
                    bfood[i].bbox = new BoundingBox(new Vector3(bfood[i].position.X - bfood[i].rect.Width / 2, bfood[i].position.Y - bfood[i].rect.Height / 2, 0),
                        new Vector3(bfood[i].position.X + bfood[i].rect.Width / 2, bfood[i].position.Y + bfood[i].rect.Height / 2, 0));     //Bounding box for bad food

                }

                for (int i = 0; i < numberofbadfood; i++)
                {
                    if (sumoslim.bbox.Intersects(bfood[i].bbox) && bfood[i].visible)    //Checks for collision between player and bad food and takes lifes away accordingly 
                    {
                        bfood[i].visible = false;
                        lives--;

                        if (lives == 4|| lives ==3)             //If lives is at 3 or 4, make him medium size
                            reset2("SS_Medium",0.3f);
                        if (lives == 2 || lives == 1)            //If lives is at 1 or 2, make him fattest size
                            reset2("SS_Fat",0.3f);
                        collectbadfood.start(sumoslim.position);
                        death.Play();
                    }
                
                }

                for (int i = 0; i < numberofbadfood; i++)
                {
                    if (bfood[i].visible && bfood[i].position.X < sumoslim.position.X && !bfood[i].jumped)
                    {
                        bfood[i].jumped = true;                    //If player jumps over bad food, player gains points
                        score += 5750;
                        
                    }
                }
                for (int i = 0; i < numberofgoodfood; i++)       //Bounding box for bad foods and adds velocity, sets rectangle to the position for bad food
                {
                    gfood[i].position += gfood[i].velocity;                 //good food moves based on velocity
                    gfood[i].rect.Y = (int)gfood[i].position.Y;             //sets rectangle to position
                    gfood[i].rect.X = (int)gfood[i].position.X;             //sets rectangle to position
                    gfood[i].bbox = new BoundingBox(new Vector3(gfood[i].position.X - gfood[i].rect.Width / 2, gfood[i].position.Y - gfood[i].rect.Height / 2, 0),
                        new Vector3(gfood[i].position.X + gfood[i].rect.Width / 2, gfood[i].position.Y + gfood[i].rect.Height / 2, 0));         //Boundingbox for good food

                }

                for (int i = 0; i < numberofgoodfood; i++)
                {
                    if (sumoslim.bbox.Intersects(gfood[i].bbox) && gfood[i].visible)    //Checks for collision between player and bad food and takes lifes away accordingly 
                    {
                        gfood[i].visible = false;
                        score +=100;                    //If player touches good food, player gets score added as well as an extra life
                        lives += 1;
                        bananacollect.Play();

                        if (lives == 5)                         //If lives is 5 make player thinest size
                            reset2("SS_thin", -0.3f);
                        if (lives == 3)                         //If lives is 3 make player medium size
                            reset2("SS_Medium",-.3f);
                        collectgoodfood.start(sumoslim.position);
                                   
                    }

                }

                if (lives <= 0)                                        //If lives reach a certain point the game is over
                {
                    if (music1.State == SoundState.Playing) music1.Stop();

                    gameover = true;
                    if (score > highscores[lasthighscore])
                    {
                        highscorenames[lasthighscore] = "";
                    }
                }

                if (score >= 5750)
                {
                    if (music1.State == SoundState.Playing) music1.Stop();

                    vicgameover = true;

                    if (score > highscores[lasthighscore])
                    {
                        highscorenames[lasthighscore] = "";
                    }
                }


            }
            else                                       //If game is over, return to the game menu if corresponding button is pressed
            {
                gameisover(gtime);

//                if (pad[0].Buttons.B == ButtonState.Pressed) gamestate = -1;
  //              if (pad[0].Buttons.Back == ButtonState.Pressed) gamestate = -1;             //Resets To Start Of Game Menu
            }

        }

        void gameisover(float gtime)
        {
            if (score > highscores[lasthighscore])
            {
                keycounter -= gtime; // Counter to delay between keys of the same value being entered
                if (keyboardreleased)
                {
                    if (keys.IsKeyDown(Keys.Back) && highscorenames[lasthighscore].Length > 0 && keycounter < 0)
                    {
                        highscorenames[lasthighscore] = highscorenames[lasthighscore].Substring(0, highscorenames[lasthighscore].Length - 1);
                        keycounter = keystrokedelay;
                    }
                    else
                    {
                        char nextchar = getnextkey();
                        char lastchar = '!';
                        if (highscorenames[lasthighscore].Length > 0)
                            lastchar = Convert.ToChar(highscorenames[lasthighscore].Substring(highscorenames[lasthighscore].Length - 1, 1));
                        if (nextchar != '!' && (nextchar != lastchar || keycounter < 0))
                        {
                            keycounter = keystrokedelay;
                            highscorenames[lasthighscore] += nextchar;
                            if (highscorenames[lasthighscore].Length > maxnamelength)
                                highscorenames[lasthighscore] = highscorenames[lasthighscore].Substring(0, maxnamelength);
                        }
                    }
                }

            }


            // Allow game to return to the main menu
            if (pad[0].Buttons.B == ButtonState.Pressed || keys.IsKeyDown(Keys.Enter))
            {
                if (score > highscores[lasthighscore])
                {
                    highscores[lasthighscore] = score;
                }

                // Sort the high score table
                Array.Sort(highscores, highscorenames);
                Array.Reverse(highscores);
                Array.Reverse(highscorenames);

                gamestate = -1;
            }
        }


        public void drawgame()
        {                                                                           //Draws The Game
            spriteBatch.Begin();

            spriteBatch.Draw(gamebackground.image, gamebackground.rect, Color.White);                   //Draws background1

            spriteBatch.Draw(gamebackground2.image, gamebackground2.rect, Color.White);                 //Draws background 2

            for(int i=0;i <numberofbadfood;i++)
            {
                if(bfood[i].visible)                                                            //Draws bad food
                spriteBatch.Draw(bfood[i].image, bfood[i].rect, null, Color.White, 0, bfood[i].origin, SpriteEffects.None, 0);
            }
            for (int i = 0; i < numberofgoodfood; i++)
            {
                if (gfood[i].visible)                                                            //Draws bad food
                    spriteBatch.Draw(gfood[i].image, gfood[i].rect, null, Color.White, 0, gfood[i].origin, SpriteEffects.None, 0);
            }
            spriteBatch.Draw(sumoslim.image, sumoslim.rect, null, Color.White, 0, sumoslim.origin, SpriteEffects.None, 0);          //Draws player

            spriteBatch.DrawString(mainfont, "Lives: " + lives.ToString(), new Vector2(100, 30), Color.Red);
                                                                                                                   //Draws lives and score
            spriteBatch.DrawString(mainfont, "Score: " + score.ToString(), new Vector2(900, 30), Color.Red);

            for (int i = 0; i < numberofgoodfood; i++)
            {
                if(gfood[i].visible)
                particleEngine[i].Draw(spriteBatch);            //Adds particle effect for moving banana
            }

            collectgoodfood.drawme(ref spriteBatch);
            collectbadfood.drawme(ref spriteBatch);

            if (gameover)
            {
                spriteBatch.Draw(gameoverimage.image, gameoverimage.rect, Color.White);

                if (score > highscores[lasthighscore])
                {
                    spriteBatch.DrawString(mainfont, "New High Score Enter Name", new Vector2(displaywidth / 2 - (int)(mainfont.MeasureString("New High Score Enter Name").X * (1f / 2f)), displayheight - 100),
                            Color.Blue, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(mainfont, highscorenames[numberofhighscores - 1], new Vector2(displaywidth / 2 - (int)(mainfont.MeasureString("New High Score Enter Name").X * (1f / 2f)), displayheight - 40),
                            Color.Blue, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                }
                
                
                //If game is over, it draws gameover image and text
                spriteBatch.DrawString(mainfont, " Press 'B' on gamepad to return to the menu", new Vector2(360, 500), Color.Blue);
            }


            if (vicgameover)
            {
                spriteBatch.Draw(vicscreen.image, vicscreen.rect, Color.White);

                if (score > highscores[lasthighscore])
                {
                    spriteBatch.DrawString(mainfont, "VICTORY: Enter Name", new Vector2(displaywidth / 2 - (int)(mainfont.MeasureString("VICTORY: Enter Name").X * (1f / 2f)), displayheight - 100),
                            Color.Blue, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                    spriteBatch.DrawString(mainfont, highscorenames[numberofhighscores - 1], new Vector2(displaywidth / 2 - (int)(mainfont.MeasureString("VICTORY: Enter Name").X * (1f / 2f)), displayheight - 40),
                            Color.Blue, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                }


                //If game is over, it draws gameover image and text
                spriteBatch.DrawString(mainfont, " Press 'B' on gamepad to return to the menu", new Vector2(100, 300), Color.Blue);
            }
            else
            {
                float gtime = 0;
                gameisover(gtime);
            }

            spriteBatch.End();
        }

        public void updatecontrols()
        {
            if (pad[0].Buttons.B == ButtonState.Pressed) gamestate = -1;                      //Resets To Start Of Game Menu          
            
            
        }

        public void drawcontrols()
        {
            spriteBatch.Begin();

            spriteBatch.Draw(controls.image, controls.rect, Color.White);                       //Draws Controls 

            spriteBatch.End();
        }

        public void updatehighscore()
        {
                   //Updates Highscore
               

                if (pad[0].Buttons.B == ButtonState.Pressed) gamestate = -1;
        }


        
        public void drawhighscore()
        {
            // Draw graphics for High Score table
            spriteBatch.Begin();
            spriteBatch.Draw(background.image, background.rect, Color.White);
            // Draw top ten high scores
            for (int i = 0; i < numberofhighscores; i++)
            {
                if (highscorenames[i].Length >= 24)
                    spriteBatch.DrawString(mainfont, (i + 1).ToString("0") + ". " + highscorenames[i].Substring(0, 24), new Vector2(525, 100 + (i * 30)),
                        Color.Red, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                else
                    spriteBatch.DrawString(mainfont, (i + 1).ToString("0") + ". " + highscorenames[i], new Vector2(525, 100 + (i * 30)),
                        Color.Red, MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                spriteBatch.DrawString(mainfont, highscores[i].ToString("0"), new Vector2(displaywidth - 180, 100 + (i * 30)),
                    Color.Red  , MathHelper.ToRadians(0), new Vector2(0, 0), 1f, SpriteEffects.None, 0);
            }


            spriteBatch.End();
        }
        

        public void updateexit()
        {                                                               //Allows Player To Exit
            if (pad[0].Buttons.A == ButtonState.Pressed) this.Exit();
            menuselect.Play();                                              //Plays Sound
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (gamestate)
            {
                case -1:
                    drawmenu();                                         //Draws Menu, Game, Controls, Highscore
                    break;
                case 0:
                    drawgame();
                    break;
                case 1:
                    drawcontrols();
                    break;
                case 2:
                    drawhighscore();
                    break;
                case 3:
                    this.Exit();
                    break;                              
                default:                                            //Allows To Exit
                    this.Exit();
                    break;
            }

            base.Draw(gameTime);
        }




        public static char getnextkey()
        {
            // Read keyboard
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.A))
                return 'A';
            else if (keys.IsKeyDown(Keys.B))
                return 'B';
            else if (keys.IsKeyDown(Keys.C))
                return 'C';
            else if (keys.IsKeyDown(Keys.D))
                return 'D';
            else if (keys.IsKeyDown(Keys.E))
                return 'E';
            else if (keys.IsKeyDown(Keys.F))
                return 'F';
            else if (keys.IsKeyDown(Keys.G))
                return 'G';
            else if (keys.IsKeyDown(Keys.H))
                return 'H';
            else if (keys.IsKeyDown(Keys.I))
                return 'I';
            else if (keys.IsKeyDown(Keys.J))
                return 'J';
            else if (keys.IsKeyDown(Keys.K))
                return 'K';
            else if (keys.IsKeyDown(Keys.L))
                return 'L';
            else if (keys.IsKeyDown(Keys.M))
                return 'M';
            else if (keys.IsKeyDown(Keys.N))
                return 'N';
            else if (keys.IsKeyDown(Keys.O))
                return 'O';
            else if (keys.IsKeyDown(Keys.P))
                return 'P';
            else if (keys.IsKeyDown(Keys.Q))
                return 'Q';
            else if (keys.IsKeyDown(Keys.R))
                return 'R';
            else if (keys.IsKeyDown(Keys.S))
                return 'S';
            else if (keys.IsKeyDown(Keys.T))
                return 'T';
            else if (keys.IsKeyDown(Keys.U))
                return 'U';
            else if (keys.IsKeyDown(Keys.V))
                return 'V';
            else if (keys.IsKeyDown(Keys.W))
                return 'W';
            else if (keys.IsKeyDown(Keys.X))
                return 'X';
            else if (keys.IsKeyDown(Keys.Y))
                return 'Y';
            else if (keys.IsKeyDown(Keys.Z))
                return 'Z';
            else if (keys.IsKeyDown(Keys.D0))
                return '0';
            else if (keys.IsKeyDown(Keys.D1))
                return '1';
            else if (keys.IsKeyDown(Keys.D2))
                return '2';
            else if (keys.IsKeyDown(Keys.D3))
                return '3';
            else if (keys.IsKeyDown(Keys.D4))
                return '4';
            else if (keys.IsKeyDown(Keys.D5))
                return '5';
            else if (keys.IsKeyDown(Keys.D6))
                return '6';
            else if (keys.IsKeyDown(Keys.D7))
                return '7';
            else if (keys.IsKeyDown(Keys.D8))
                return '8';
            else if (keys.IsKeyDown(Keys.D9))
                return '9';
            else if (keys.IsKeyDown(Keys.Space))
                return ' ';
            else
                return '!';
        }





    }
}
