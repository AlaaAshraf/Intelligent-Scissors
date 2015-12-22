using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IntelligentScissors
{

    public class Vertex
    {
        public  Tuple<int, int> Parent;
        public double Distance;
        //(i,j)->position of the vertex in the array Vertices
        //index of the vertex in the heap
        public int i, j,index;
        public Vertex(int x,int y)
        {
            j = y;
            i = x;
            Parent = null;
            Distance = double.MaxValue;
        }
    }

    public class priority_queue
    {
        int length;
        public void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        public void Swap<T>( T lhs,  T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        public int heap_size;
        public Vertex[] arr;
        public int parent(int i)
        {
            return i / 2;
        }
        public int left(int i)
        {
            return 2 * i;
        }
        public int right(int i)
        {
            return 2 * i + 1;
        }
        public void min_heapify(int i)
        {
            if (heap_size == 0)
                return;
            int l = left(i), r = right(i),smallest=i;
            if (l <= heap_size && arr[l].Distance < arr[smallest].Distance)
                smallest = l;
            if (r <= heap_size && arr[r].Distance < arr[smallest].Distance)
                smallest = r;
            if (smallest!= i)
            {
                Swap<int>(ref arr[smallest].index,ref arr[i].index);
                Swap<Vertex>(ref arr[smallest], ref arr[i]);
                
                min_heapify(smallest);
            }

        }
        public priority_queue(ref Vertex[,] Vertices,int Height, int Width)
        {
            length = Height * Width;
            heap_size = length;
            arr = new Vertex[length+1];
            int index = 1;
            for(int i=0; i<Width; i++)
            {
                for(int j=0; j<Height; j++)
                {
                    arr[index] = Vertices[i, j];
                    //Vertices[i, j].Distance = 757;
                    Vertices[i, j].index = index;
                    index++;
                }
            }

            for(int i=length/2; i>0; i--)
            {
                min_heapify(i);
            }
        }
        public Vertex extract_min()
        {
            Vertex min = arr[1];
            arr[1] = arr[heap_size];
            arr[1].index = 1;
            heap_size--;
            min_heapify(1);
            return min;
        }
    }

    public class Graph
    {
        //first dimension = right neighbour, Second = bottom, third = left, fourth = upper
        public double[,,] Weight;
        public int Height, Width;
        RGBPixel[,] ImageMatrix;
        //First dimension represents the width and the second represents the height
        public Vertex[,] Vertices;
        public priority_queue Q;
        public Graph(RGBPixel[,] ImageMatrix)
        {
            this.ImageMatrix = ImageMatrix;
            //Get Width and Height
            Height = ImageOperations.GetHeight(ImageMatrix);
            Width = ImageOperations.GetWidth(ImageMatrix);

            //Allocate 2D array of vertices
            Vertices = new Vertex[Width, Height];

            //Caclulate the Weight between pixels
            //first dimension = right neighbour, Second = bottom, third = left, fourth = upper
            Weight = new double[Width, Height,4];
            for(int i=0; i<Width; i++)
            {
                for(int j=0; j<Height; j++)
                {
                    //Allocate a new vertex
                    Vertices[i, j] = new Vertex(i,j);
                    Vector2D Energy = new Vector2D();
                    Energy = ImageOperations.CalculatePixelEnergies(i, j, ImageMatrix);
                    if (Energy.X != 0)
                        Weight[i, j, 0] = 1.0 / Energy.X;
                    else
                        Weight[i, j, 0] = 1E300;
                    if (Energy.Y != 0)
                        Weight[i, j, 1] = 1.0 / Energy.Y;
                    else Weight[i, j, 1] = 1E300;
                    if (j > 0)
                        Weight[i, j, 2] =Weight[i,j-1,0];
                    else Weight[i, j, 2] = double.MaxValue;
                    if (i > 0)
                        Weight[i, j, 3] = Weight[i-1,j,1];
                    else Weight[i, j, 3] = double.MaxValue;
                }
            }
        }
        
        void Relax_All(ref Vertex u)
        {
            //Relaxes the edges between u and all its neighbours
            if (u.j < Height - 1)
                Relax(ref u, ref Vertices[u.i, u.j + 1], Weight[u.i, u.j, 0]);

            if (u.i < Width - 1)
                Relax(ref u, ref Vertices[u.i+1, u.j], Weight[u.i, u.j, 1]);

            if (u.j >0)
                Relax(ref u, ref Vertices[u.i, u.j - 1], Weight[u.i, u.j, 2]);

            if (u.i >0)
                Relax(ref u, ref Vertices[u.i - 1, u.j], Weight[u.i, u.j, 3]);
        }
        void Relax(ref Vertex u, ref Vertex v, double w)
        {
            //Relaxes the edges between u and v
            if(v.Distance>u.Distance+ w)
            {
                v.Distance = u.Distance + w;
                Tuple<int, int> temp = new Tuple<int, int>(u.i, u.j);
                v.Parent = temp;
                while (Q.parent(v.index)>0&&Q.arr[v.index].Distance<Q.arr[Q.parent(v.index)].Distance)
                {
                    
                    Q.Swap<Vertex>(ref Q.arr[v.index],ref  Q.arr[Q.parent(v.index)]);
                    Q.Swap<int>(ref Q.arr[v.index].index, ref  Q.arr[Q.parent(v.index)].index);
                } 
            }
        }
        public void Dijkstra(int x, int y)
        {
            //Destroys any previous calculations and calculate the shortest path from the given point
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Vertices[i, j] = new Vertex(i, j);
                }
            }
            //Set the source distance to zero
            Vertices[x, y].Distance = 0;
            //Pruning

            Vertex[,] Vertices1;
            int Width2=0, Height2=0,x1,x2,y1,y2;
            int diff = 100;
            x1 = Math.Max(x - diff, 0);
            x2 = Math.Min(Width, x + diff);
            y1 = Math.Max(y - diff, 0);
            y2 = Math.Min(Height, y + diff);
            Width2 = x2 - x1;
            Height2= y2-y1;
            Vertices1 = new Vertex[Width2, Height2];
            int w1 = 0, h1 = 0;
            for(int i= x1; i< x2; i++)
            {
                h1 = 0;
                for(int j= y1; j< y2; j++)
                {
                    Vertices1[w1, h1] = Vertices[i, j];
                    h1++;
                }
                w1++;
            }
            //Priority queue using heap containing all vertices
            Q = new priority_queue(ref Vertices1, Height2, Width2);
            //End of Pruning

            
            //Q = new priority_queue(ref Vertices, Height, Width);

            while (Q.heap_size>0)
            {
                //Extract the vertex with minimum distance and relax its edges
                Vertex u = Q.extract_min();
                int i = u.i, j = u.j;
                Relax_All(ref u);
            }
        }
       

    }
}
