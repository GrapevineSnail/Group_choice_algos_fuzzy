using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Pathfinding
{
    public class FzVertex
    {
        int x, y, height, width;
        string number;

        public FzVertex(int x, int y, int height, int width, string number)
        {
            this.x = x;
            this.y = y;
            this.height = height;
            this.width = width;
            this.number = number;
        }

        public int X
        {
            get { return x; }
            set { x = value; }
        }

        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public string Number
        {
            get { return number; }
            set { number = value; }
        }

        public void Draw(Graphics fzgr)
        {
            fzgr.SmoothingMode = SmoothingMode.AntiAlias;
            fzgr.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            fzgr.FillEllipse(Brushes.Bisque, X, Y, Width, Height);
            fzgr.DrawEllipse(Pens.Black, X, Y, Width, Height);
            fzgr.DrawString(Number, new Font("Verdana", 12), Brushes.Black, new PointF(X + Width / 2 - 7, Y + Height / 2 - 7));
        }
    }
}

