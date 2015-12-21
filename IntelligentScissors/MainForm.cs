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

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if(firstClick==false)
            {
                
                firstClick = true;
                prevClick.X = e.X;
                prevClick.Y = e.Y;
                //Calculates the shortest path in O((V+E)log(V))
                G.Dijkstra((int)prevClick.X, (int)prevClick.Y);
            }
            else
            {
                
                curClick =new Vector2D();
                curClick.X = e.X;
                curClick.Y = e.Y;
                Draw();
                prevClick = curClick;
                //Destroys the previous calculation and calculates again in O((V+E)log(V))
                G.Dijkstra((int)prevClick.X, (int)prevClick.Y);
                
            }
            
        }
        public Vector2D curClick;
        private void Draw()
        {
            Vertex u = new Vertex(0, 0);
            Bitmap b = (Bitmap)pictureBox2.Image;
            int even = 0;
            u = G.Vertices[(int)curClick.X, (int)curClick.Y];
            while (u.Parent != null && (u.Parent.Item1 != prevClick.X || u.Parent.Item2 != prevClick.Y))
            {
                if (even % 2 == 0)
                { b.SetPixel((u.i), (u.j), Color.Black); }
                else
                { b.SetPixel((u.i), (u.j), Color.White); }
                even++;
                u = G.Vertices[(int)u.Parent.Item1, (int)u.Parent.Item2];
                pictureBox2.Refresh();
            }
            
            
            
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if(firstClick==true)
            {
                G.Dijkstra((int)prevClick.X, (int)prevClick.Y);
                curClick = new Vector2D();
                curClick.X = e.X;
                curClick.Y = e.Y;
                Draw();
                prevClick = curClick;
            }
        }
    }
}

//Live Wire
/**/
