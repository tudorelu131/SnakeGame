using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snek
{
    public partial class GameInstance : Form
    {
        LinkedList<RectangleF> snake = new LinkedList<RectangleF>();
        Rectangle fruit;
        Rectangle GameArea;
        int counter = 0;
        int snakeWidth = 18, snakeHeight = 18;
        bool up, down, left, right;
        public GameInstance()
        {
            InitializeComponent();
            this.Resize += GameInstace_Resize;
            updateGameArea();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            for(int i = 0; i < 3; i++)
                snake.AddFirst(new RectangleF(101 - i * 24, 101, snakeWidth, snakeHeight));
            timer1.Interval = 100;
            Random rand = new Random();
            label1.Text = "Score: 0";
            fruit = new Rectangle(rand.Next(0,20) * GameArea.Width/20 + GameArea.Left + 3, rand.Next(0, 20) * GameArea.Height/20 + GameArea.Top + 3, 18, 18);
        }

        private void KeyDownEvent(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Up)
            {
                up = true;
                down = false;
                left = false;
                right = false;
                e.Handled = true;
            }
            if(e.KeyCode == Keys.Down)
            {
                up = false;
                down = true;
                left = false;
                right = false;
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Left)
            {
                up = false;
                down = false;
                left = true;
                right = false;
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Right)
            {
                up = false;
                down = false;
                left = false;
                right = true;
                e.Handled = true;
            }
        }

        private void GameRefresh(object sender, EventArgs e)
        {

            if (up || down || left || right)
            {
                updatePositions();
                Collision();
            }
            Invalidate();
        }

        private void GameInstace_Resize(object sender, EventArgs e)
        {
            updateGameArea();
            Invalidate(); // Forces the form to repaint
        }
        private void updateGameArea()
        {
            int marginWidth = ClientSize.Width/20;
            int marginHeight = ClientSize.Height/20;
            GameArea = new Rectangle(
                marginWidth,
                marginHeight + 30,
                ClientSize.Width - (marginWidth * 2),
                ClientSize.Height - (marginHeight * 2) - 20
            );
        }

        private void FormClosed_Event(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void PaintEvent(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.Black, 2);
            Pen snek = new Pen(Color.Green, 2);
            Graphics g = e.Graphics;
            SolidBrush brush = new SolidBrush(Color.RosyBrown);
            SolidBrush snekBrush = new SolidBrush(Color.Green);
            SolidBrush fruitBrush = new SolidBrush(Color.Red);
            
            g.FillRectangle(brush, GameArea);

            float cellWidth = (float)GameArea.Width / 20;
            float cellHeight = (float)GameArea.Height / 20;

            for(int i = 0; i <= 20; i++)
            {
                float x = GameArea.Left + i * cellWidth;
                g.DrawLine(pen,x, GameArea.Top, x, GameArea.Bottom);
            }

            for(int i = 0; i <= 20;i++)
            {
                float y = GameArea.Top + i * cellHeight;
                g.DrawLine(pen, GameArea.Left, y, GameArea.Right, y);
            }
            
            byte[] snekHead = (byte[])Properties.Resources.ResourceManager.GetObject("SnekHead");
            using(MemoryStream ms = new MemoryStream(snekHead))
            {
                Image img = Image.FromStream(ms);
                g.DrawImage(img, Rectangle.Round(snake.Last.Value));
            }
            using (MemoryStream ms = new MemoryStream(Properties.Resources.SnekBody))
            {
                Image img = Image.FromStream(ms);
                foreach (var segment in snake.Take(snake.Count() - 1))
                {
                    g.DrawImage(img, segment);
                }
            }

            g.FillRectangle(fruitBrush, fruit);
        }
        private void updatePositions()
        {
            // Calculate the size of a single grid cell as floating-point values
            float cellWidth = (float)GameArea.Width / 20;
            float cellHeight = (float)GameArea.Height / 20;

            // Get the current head position
            var head = snake.Last.Value;

            // Calculate the new head position based on the direction
            RectangleF newHead = new RectangleF(head.X, head.Y, snakeWidth, snakeHeight);
            if (down)
                newHead = new RectangleF(head.X, head.Y + cellHeight, snakeWidth, snakeHeight);
            if (up)
                newHead = new RectangleF(head.X, head.Y - cellHeight, snakeWidth, snakeHeight);
            if (right)
                newHead = new RectangleF(head.X + cellWidth, head.Y, snakeWidth, snakeHeight);
            if (left)
                newHead = new RectangleF(head.X - cellWidth, head.Y, snakeWidth, snakeHeight);


            // Convert the new head position back to an integer Rectangle
            snake.AddLast(newHead);

            // Remove the tail to maintain the snake's length
            if (!newHead.IntersectsWith(fruit))
                snake.RemoveFirst();
            else
            {
                counter++;
                label1.Text = "Score: " + counter;
                SpawnFruit();
            }
        }
        private void SpawnFruit()
        {
            Random rand = new Random();
            do
            {
                fruit = new Rectangle(rand.Next(0, 20) * GameArea.Width / 20 + GameArea.Left + 3, rand.Next(0, 20) * GameArea.Height / 20 + GameArea.Top + 3, 18, 18);
            } while (snake.Any(segment => segment.IntersectsWith(fruit)));
        }
        private void Collision()
        {
            Rectangle head = Rectangle.Round(snake.Last.Value);
            foreach(var segment in snake.Take(snake.Count-1))
            {
                Rectangle segmentRect = Rectangle.Round(segment);
                if (segmentRect.IntersectsWith(head))
                {
                    up = down = left = right = false;
                    timer1.Stop();
                    MessageBox.Show("You Lost!");
                    Application.Exit();
                }
                if(head.X < GameArea.Left || head.X > GameArea.Right || head.Y < GameArea.Top || head.Y > GameArea.Bottom)
                {
                    up = down = left = right = false;
                    timer1.Stop();
                    MessageBox.Show($"You Lost!\nFinal Score:{counter}");
                    Application.Exit();
                }
            }
        }
    }
}
