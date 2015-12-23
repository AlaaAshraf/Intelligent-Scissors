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
        int frequency, contrast;
        bool firstClick;
        Vector2D prevClick;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            //Build the graph
            G = new Graph(ImageMatrix);
            firstClick = new bool();
            firstClick = false;
            prevClick = new Vector2D();
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }
        public Vector2D curClick;
        Bitmap beforeClick;
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
        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (firstClick == true)
            {
                curClick = new Vector2D();
                curClick.X = e.X;
                curClick.Y = e.Y;
                /*if (AutomaticAnchor() == true)
                    return;*/
                pictureBox2.Image = beforeClick;
                pictureBox2.Refresh();
                Draw();
            }
        }

        private bool AutomaticAnchor()
        {
            Vertex u = new Vertex(0, 0);
            Vertex parent = new Vertex(0, 0);
            u = G.Vertices[(int)curClick.X, (int)curClick.Y];
            if (u.VerticesToParent <= frequency)
                return false;
           
                parent = G.Vertices[(int)u.Parent.Item1, (int)u.Parent.Item2];
                double distanceDifference = Math.Abs(u.Distance - parent.Distance);
                if(distanceDifference>contrast)
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

//Live Wire
/**/
