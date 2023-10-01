using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;

namespace SnakeGeneticLearning
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        int countofsnake = 50;
        int gridlength;
        Generation gen;
        InfoBloc[] bloclist;
        double BestSnake = 0;
        TimeSpan timer = new TimeSpan();
        double gamespeed = 1;
        double lastchange = 0;
        Random random;
        int turncounter = 0;
        SpriteFont sf;
        int gencounter;
        double[] computemap;
        KeyboardState state;
        double closenesstofood;
        int scorecount;
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 10 * countofsnake*2 + 1;
            _graphics.PreferredBackBufferHeight = 10 * countofsnake*2 + 1;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            gridlength = countofsnake * 2;
            computemap = new double[gridlength * gridlength];
            sf = Content.Load<SpriteFont>("File");
            random = new Random();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            bloclist = new InfoBloc[countofsnake];
            gen = new Generation(countofsnake);

            // TODO: use this.Content to load your game content here
            for (int i = 0; i < countofsnake; i++)
            {
                bloclist[i] = new InfoBloc(new Snake(5*gridlength, 5*gridlength), new Individual(new int[] { computemap.Length * (2 / 12) + 4, 4 }, computemap.Length, random), 0,gridlength);
                bloclist[i].snake.Food = new FoodPiece();
                bloclist[i].snake.Food.xposition = 5*gridlength;
                bloclist[i].snake.Food.yposition = 5*gridlength;
                bloclist[i].pastpos.X = 5*gridlength;
                bloclist[i].pastpos.Y = 5*gridlength;
               
            }
           

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            for (int l = 0; l < gamespeed; l++)
            {

                timer = timer.Add(new TimeSpan(0, 0, 0, 0, 1));
                turncounter++;
                bool allive = false;
                KeyboardState keys = Keyboard.GetState();
                if (keys.IsKeyDown(Keys.Up) && (timer.TotalMilliseconds - lastchange) > 500)
                {
                    gamespeed++;
                    lastchange = timer.TotalMilliseconds;
                }
                if (keys.IsKeyDown(Keys.Down) && (timer.TotalMilliseconds - lastchange) > 500)
                {
                    gamespeed--;
                    lastchange = timer.TotalMilliseconds;
                }
                if (keys.IsKeyDown(Keys.K) && !state.IsKeyDown(Keys.K))
                {
                    foreach (var i in bloclist)
                    {
                        i.snake.lifestatus = false;
                    }
                }
                if (keys.IsKeyDown(Keys.C) && !state.IsKeyDown(Keys.C))
                {
                    ;
                }
                // TODO: Add your update logic here
                foreach (var i in bloclist)
                {
                    
                    if (i.snake.lifestatus)
                    {
                      
                        for (int j = 0; j < gridlength; j++)
                        {
                            for (int k = 0; k < gridlength; k++)
                            {
                                computemap[j * gridlength + k] = i.map[j][k];
                            }
                        }
                        double[] directionoutput = i.info.network.Compute(computemap);
                        /* if (Math.Abs(directionoutput[0]) > Math.Abs(directionoutput[1]))
                         {
                             if (directionoutput[0] > 0)
                             {
                                 if (i.snake.direction != 3)
                                 {
                                     i.snake.direction = 1;
                                 }
                                 else
                                 {
                                     ;
                                 }

                             }
                             else
                             {
                                 if (i.snake.direction != 1)
                                 {
                                     i.snake.direction = 3;
                                 }
                                 else
                                 {
                                     ;
                                 }
                             }
                         }
                         else
                         {
                             if (directionoutput[1] > 0)
                             {
                                 if (i.snake.direction != 0)
                                 {
                                     i.snake.direction = 2;
                                 }
                                 else
                                 {
                                     ;
                                 }
                             }
                             else
                             {
                                 if (i.snake.direction != 2)
                                 {
                                     i.snake.direction = 0;
                                 }
                                 else
                                 {
                                     ;
                                 }
                             }
                         }*/
                        int greatest = 0;
                        double best = 0;
                        for (int k = 0; k < directionoutput.Length; k++)
                        {
                            if (directionoutput[k] > best)
                            {
                                best = directionoutput[k];
                                greatest = k;
                            }
                        }
                        if ((greatest == 1 && i.snake.lastdir == 3) || (greatest == 3 && i.snake.lastdir == 1) || (greatest == 0 && i.snake.lastdir == 2) || (greatest == 2 && i.snake.lastdir == 0))
                        {

                            i.snake.direction = i.snake.lastdir;
                        }
                        else
                        {
                            i.snake.direction = greatest;
                        }



                        i.snake.Move(random,turncounter,gridlength);
                        if (i.snake.lifestatus)
                        {
                            if(i.snake.score>1)
                            {
                                closenesstofood = (Math.Abs(i.snake.Head.xposition - i.snake.Food.xposition) + Math.Abs(i.snake.Head.yposition - i.snake.Food.yposition)) / 2;
                            }
                            
                            if (closenesstofood < 10)
                            {
                                if (i.pointsforclose < 20)
                                {
                                    i.pointsforclose = 20;
                                }
                            }
                            if (closenesstofood < 5)
                            {
                                if (i.pointsforclose < 30)
                                {
                                    i.pointsforclose = 30;
                                }
                            }
                            if (closenesstofood < 15)
                            {
                                if (i.pointsforclose < 10)
                                {
                                    i.pointsforclose = 10;
                                }
                            }
                            if (closenesstofood < 20)
                            {
                                if (i.pointsforclose < 5)
                                {
                                    i.pointsforclose = 5;
                                }
                            }
                            if (closenesstofood < 25)
                            {
                                if (i.pointsforclose < 2.5)
                                {
                                    i.pointsforclose = 2.5;
                                }
                            }
                            if((i.snake.score)>2)
                            {
                                scorecount = i.snake.score-2;
                            }
                            else
                            {
                                scorecount = 0;
                            }
                            i.fitnessval = (timer.TotalMilliseconds) + (scorecount * 500)+(i.pointsforclose*2);
                           
                        }

                        if (i.snake.lifestatus)
                        {
                            i.info.fitness = i.fitnessval;
                        }



                            i.currpos.X = (float)i.snake.Head.xposition;
                        i.currpos.Y = (float)i.snake.Head.yposition;
                        if(i.snake.lastgrow+500<turncounter)
                        {
                            i.snake.lifestatus = false;
                            i.fitnessval = 0;
                        }
                        if (turncounter % 100 == 0 && turncounter > 1)
                        {
                            if (i.snake.lifestatus && (!(Math.Abs(i.currpos.X - i.pastpos.X) > 50) && !(Math.Abs(i.currpos.Y - i.pastpos.Y) > 50)))
                            {
                                i.snake.lifestatus = false;
                                i.fitnessval = 0;
                            }
                            else
                            {
                                i.pastpos.X = i.currpos.X;
                                i.pastpos.Y = i.currpos.Y;
                            }
                        }
                        if (i.fitnessval > BestSnake)
                        {
                            BestSnake = i.fitnessval;
                        }
                        if (i.snake.lifestatus)
                        {
                            allive = true;
                            i.MapUpdate(gridlength);
                        }
                       
                    }
                }
                if (!allive)
                {
                    GenReset();
                }
               
                base.Update(gameTime);
                state = keys;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            GraphicsDevice.Clear(Color.Black);
            foreach (var i in bloclist)
            {
                i.snake.DrawSnake(i.snake.Head, _spriteBatch, 1);
                if (i.snake.lifestatus)
                {
                    _spriteBatch.DrawRectangle((float)i.snake.Food.xposition, (float)i.snake.Food.yposition, 10, 10, Color.Red, 1, 0);
                }

            }
            _spriteBatch.DrawString(sf, gencounter.ToString(), new Vector2((int)(4.5*gridlength), (int)(4.7*gridlength)), Color.White);
            // TODO: Add your drawing code here
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        public void GenReset()
        {

            for (int i = 0; i < countofsnake; i++)
            {
                gen.population[i] = bloclist[i].info;

            }
            gen.GenerationTraining(random, 0.1);
            for (int i = 0; i < countofsnake; i++)
            {
                bloclist[i].info = gen.population[i];
            }
            timer = timer.Subtract(timer);
            Console.WriteLine(BestSnake);
            BestSnake = 0;
            foreach (var i in bloclist)
            {
                i.fitnessval = 0;
                i.info.fitness = 0;
                i.snake.Head.xposition = 5*gridlength;
                i.snake.Head.yposition = 5*gridlength;
                i.snake.lifestatus = true;
                i.snake.score = 0;
                i.snake.Tail = i.snake.Head;
                i.snake.Food.xposition = 5*gridlength;
                i.snake.Food.yposition = 5*gridlength;
                i.pastpos.X = 5*gridlength;
                i.pastpos.Y = 5*gridlength;
                turncounter = 0;
                i.pointsforclose = 0;
                

            }
            gencounter++;
        }
    }

}
