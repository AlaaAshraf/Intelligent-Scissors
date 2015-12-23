using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IntelligentScissors
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        Graph G;
        int frequency;
        double energyDifference;
        bool firstClick,userAutomaticAnchor=false;
        Vector2D prevClick, curClick;
        Bitmap beforeClick;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            //Build the graph
            G = new Graph(ImageMatrix);
            firstClick = false;
            prevClick = new Vector2D();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            //Build the graph
            G = new Graph(ImageMatrix);
            
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }
        
        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                firstClick = false;
                pictureBox2.Image = beforeClick;
                pictureBox2.Refresh();
                return;
            }
            Vector2D Position = new Vector2D();
            Position.X = e.X;
            Position.Y = e.Y;
            MouseClick(Position);
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (firstClick == true)
            {
                curClick = new Vector2D();
                curClick.X = e.X;
                curClick.Y = e.Y;
                if (userAutomaticAnchor == true)
                {
                    if (AutomaticAnchor() == true)
                        return;
                }
                pictureBox2.Image = beforeClick;
                pictureBox2.Refresh();
                Draw();
            }
        }

        private void MouseClick(Vector2D e)
        {
            
            if (firstClick == false)
            {

                firstClick = true;
                prevClick.X = e.X;
                prevClick.Y = e.Y;

            }
            else
            {
                curClick = new Vector2D();
                curClick.X = e.X;
                curClick.Y = e.Y;
                Draw();
                prevClick = curClick;
            }
            //Destroys the previous calculation and calculates again in O((V+E)log(V))
            G.Dijkstra((int)prevClick.X, (int)prevClick.Y);
            //Stores the image after each click
            beforeClick = (Bitmap)(pictureBox2.Image.Clone());
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            userAutomaticAnchor = !userAutomaticAnchor;
        }

        private bool AutomaticAnchor()
        {
            frequency = Convert.ToInt32(userFrequency.Text);
            frequency = int.Parse(userFrequency.Text);
            energyDifference= Convert.ToDouble(userDifference.Text);
            energyDifference = double.Parse(userDifference.Text);
            Vertex u = new Vertex(0, 0);
            Vertex parent = new Vertex(0, 0);
            u = G.Vertices[(int)curClick.X, (int)curClick.Y];
            if (u.VerticesToParent <= frequency)
                return false;
           
                parent = G.Vertices[(int)u.Parent.Item1, (int)u.Parent.Item2];
                double distanceDifference = Math.Abs(u.Distance - parent.Distance);
                if(distanceDifference>energyDifference)
                {
                    curClick.X = u.i;
                    curClick.Y = u.j;
                }
                u = parent;
            
            MouseClick(curClick);
            return true;
        }

        private void Draw()
        {
            Vertex u = new Vertex(0, 0);
            Vertex parent = new Vertex(0, 0);
            Bitmap b = new Bitmap(beforeClick);
            //int even = 0;
            u = G.Vertices[(int)curClick.X, (int)curClick.Y];
            while (u.Parent != null && (u.Parent.Item1 != prevClick.X || u.Parent.Item2 != prevClick.Y))
            {
                parent= G.Vertices[(int)u.Parent.Item1, (int)u.Parent.Item2];
                if ((u.i+u.j) % 2 == 0)
                { b.SetPixel((u.i), (u.j), Color.Black); }
                else
                { b.SetPixel((u.i), (u.j), Color.White); }
                //even++;
                u = parent;
                
            }
            pictureBox2.Image = b;
            pictureBox2.Refresh();


        }

        
    }
}