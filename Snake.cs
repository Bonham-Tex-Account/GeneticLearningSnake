using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System.Collections.Generic;
using System;

namespace SnakeGeneticLearning
{
    class InfoBloc
    {
        public Snake snake;
        public Individual info;
        public double fitnessval;
        public Vector2 currpos;
        public Vector2 pastpos;
        public byte[][] map;
        public double pointsforclose;

        public InfoBloc(Snake Snake1, Individual Info, double Fitnessval, int count)
        {
            snake = Snake1;
            info = Info;
            fitnessval = Fitnessval;
            map = new byte[count][];
            for (int i = 0; i < count; i++)
            {
                map[i] = new byte[count];
            }


        }
        public void MapUpdate(int count)
        {
            if (snake.lifestatus)
            {
                for (int i = 0; i < count; i++)
                {
                    for (int k = 0; k < count; k++)
                    {
                        map[i][k] = 0;
                    }
                }
                map[(int)(snake.Food.xposition / 10)][(int)(snake.Food.yposition / 10)] = 2;
                for (SnakePiece temp = snake.Head; temp != snake.Tail; temp = temp.Next)
                {
                    if (temp.xposition >= 0 && temp.yposition >= 0 && temp.xposition <count && temp.yposition <count)
                    {
                        map[(int)(temp.xposition / 10)][(int)(temp.yposition / 10)] = 1;
                    }

                }
            }

        }

    }

    class Snake
    {
        public SnakePiece Head;
        public SnakePiece Tail;
        public FoodPiece Food;
        public int direction;
        public int score = 0;
        public bool lifestatus = true;
        public int lastdir;
        public int lastgrow;
        public Snake(double xPosition, double yPosition)
        {
            Head = new SnakePiece();
            Head.xposition = xPosition;
            Head.yposition = yPosition;
            Tail = Head;
        }
        public void Grow(Random random, int turncounter, int count)
        {
            lastgrow = turncounter;
            score++;
            Tail.Next = new SnakePiece();
            Tail.Next.Previous = Tail;
            Tail = Tail.Next;
            Tail.xposition = 500;
            Tail.yposition = 500;

            Food.xposition = random.Next(1, (int)(count));
            Food.xposition *= 10;



            Food.yposition = random.Next(1, (int)(count));
            Food.yposition *= 10;
            ;
        }
        public void Move(Random random, int turncounter, int count)
        {
            if (lifestatus && (Head.xposition < 0 || Head.yposition < 0 || Head.yposition + 10 > 10*count || Head.xposition + 10 > 10*count))
            {
                lifestatus = false;
            }

            if (lifestatus)
            {


                if ((Food.xposition == Head.xposition) && (Food.yposition == Head.yposition))
                {
                    Grow(random, turncounter, count);
                }
                if (score < -1)
                {
                    Head.Previous = Tail;
                    Tail.Next = Head;
                    Tail = Tail.Previous;
                    Tail.Next = null;
                    Head = Head.Previous;

                }
                if (Head == Tail && score > 2)
                {
                    ;
                }
                double x = Head.xposition;
                double y = Head.yposition;
                if (direction == 0)
                {

                    if (lastdir == 2)
                        ;
                    Head.Next.yposition = Head.yposition;
                    Head.Next.xposition = Head.xposition;
                    Head.yposition -= 10;
                    lastdir = 0;


                }
                if (direction == 1)
                {
                    if (lastdir == 3)
                        ;

                    Head.Next.yposition = Head.yposition;
                    Head.Next.xposition = Head.xposition;
                    Head.xposition += 10;
                    lastdir = 1;
                }
                if (direction == 2)
                {
                    if (lastdir == 0)
                        ;
                    Head.Next.yposition = Head.yposition;
                    Head.Next.xposition = Head.xposition;
                    Head.yposition += 10;
                    lastdir = 2;
                }
                if (direction == 3)
                {
                    if (lastdir == 1)
                        ;

                    Head.Next.yposition = Head.yposition;
                    Head.Next.xposition = Head.xposition;
                    Head.xposition -= 10;
                    lastdir = 3;
                }
                for (SnakePiece temp = Head.Next; temp != Tail.Next; temp = temp.Next)
                {
                    double x2 = temp.xposition;
                    double y2 = temp.yposition;
                    temp.xposition = x;
                    temp.yposition = y;
                    x = x2;
                    y = y2;
                }
            }
            for (SnakePiece temp = Head.Next; temp != Tail.Next; temp = temp.Next)
            {
                if (lifestatus && (Head.xposition == temp.xposition && Head.yposition == temp.yposition))
                {
                    lifestatus = false;
                }
            }
        }
        public void DrawSnake(SnakePiece curr, SpriteBatch sp, double opacity)
        {
            curr.temp.X = (float)curr.xposition;
            curr.temp.Y = (float)curr.yposition;
            curr.temp.Height = curr.size;
            curr.temp.Width = curr.size;
            if (lifestatus)
            {
                sp.FillRectangle(curr.temp, Color.White, (float)opacity);
            }
            else
            {
                sp.FillRectangle(curr.temp, Color.Yellow, (float)opacity);
            }
            if (curr.Next != null)
            {
                DrawSnake(curr.Next, sp, opacity);
            }

        }
    }
    class SnakePiece
    {
        public SnakePiece Next;
        public SnakePiece Previous;
        public double xposition;
        public double yposition;
        public int size = 10;
        public RectangleF temp = new RectangleF();
    }
    class FoodPiece
    {
        public double xposition;
        public double yposition;
        public int size = 10;
    }

}
