using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        bool firstClick,userAutomaticAnchor=false,setValid;
        Vector2D prevClick, curClick,firstPosition;
        Bitmap temp;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, originalImage);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
            //Build the graph
            G = new Graph(ImageMatrix);
            firstClick = setValid = false;
            prevClick = new Vector2D();
            firstPosition = new Vector2D();
            temp = new Bitmap(originalImage.Image);
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            //Build the graph
            firstClick = setValid = false;
            prevClick = new Vector2D();
            firstPosition = new Vector2D();
            G = null;
            GC.Collect();
            G = new Graph(ImageMatrix);
            ImageOperations.DisplayImage(ImageMatrix, originalImage);
            temp = new Bitmap(originalImage.Image);
            
        }

        private void OriginalImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                
                MouseClick(firstPosition);
                firstClick = false;
                for (int i = 0; i < G.Width; i++)
                {
                    for (int j = 0; j < G.Height; j++)
                        G.isValid[i, j] = true;
                }
                //originalImage.Image = (Bitmap)temp.Clone();
                //originalImage.Refresh();
                return;
            }
            Vector2D Position = new Vector2D();
            Position.X = e.X;
            Position.Y = e.Y;
            MouseClick(Position);
        }

        private void originalImage_MouseMove(object sender, MouseEventArgs e)
        {
            
            if (firstClick == true)
            {
                curClick = new Vector2D();
                curClick.X = e.X;
                curClick.Y = e.Y;
                originalImage.Image = (Bitmap)temp.Clone();
                originalImage.Refresh();
                if (G.ImageVertices[(int)curClick.X, (int)curClick.Y] == null)
                    return;
                if (userAutomaticAnchor == true)
                {
                    if (AutomaticAnchor() == true)
                    {
                        return;
                    }
                }
                
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
                firstPosition = prevClick;

            }
            else
            {
                originalImage.Refresh();
                curClick = new Vector2D();
                curClick.X = e.X;
                curClick.Y = e.Y;
                setValid = true;
                Draw();
                setValid = false;
                temp = (Bitmap)originalImage.Image;
                prevClick = curClick;
            }
            //Destroys the previous calculation and calculates again in O((V+E)log(V))
            G.Dijkstra((int)prevClick.X, (int)prevClick.Y);
        }

        private void Draw()
        {
            Vertex u = new Vertex(0, 0);
            Vertex parent = new Vertex(0, 0);
            u = G.ImageVertices[(int)curClick.X, (int)curClick.Y];
            if (u == null)
                return;
            
            Graphics Gr = Graphics.FromImage(originalImage.Image);
            
            while (u.Parent != null && (u.Parent.Item1 != prevClick.X || u.Parent.Item2 != prevClick.Y))
            {
                parent = G.ImageVertices[(int)u.Parent.Item1, (int)u.Parent.Item2];
                if(setValid==true)
                G.isValid[parent.i, parent.j] = false;
                using (var p = new Pen(Color.AntiqueWhite, 1))
                {
                    
                    Gr.DrawLine(p, new Point(u.i, u.j), new Point(parent.i, parent.j));
                    
                }
                u = parent;

            }
            
            originalImage.Refresh();
            GC.Collect();

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
            u = G.ImageVertices[(int)curClick.X, (int)curClick.Y];
            if (u == null)
                return false;
            if (u.ImageVerticesToParent <= frequency)
                return false;
            int tempCount = 0;
            while (u.Parent != null && (u.Parent.Item1 != prevClick.X || u.Parent.Item2 != prevClick.Y))
            {
                if (tempCount > frequency / 2)
                    break;
                parent = G.ImageVertices[(int)u.Parent.Item1, (int)u.Parent.Item2];
                double distanceDifference = Math.Abs(u.Distance - parent.Distance);
                u = parent;
                if (distanceDifference > energyDifference)
                {
                    curClick.X = u.i;
                    curClick.Y = u.j;
                }
                tempCount++;
            }
            
            MouseClick(curClick);
            return true;
        }
    }
}