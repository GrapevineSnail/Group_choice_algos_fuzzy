using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using static Group_choice_algos_fuzzy.Constants;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System.Drawing.Imaging;


namespace Group_choice_algos_fuzzy
{
	public partial class Form2 : Form
	{
		public Form2()
		{
			InitializeComponent();
		}
		public Form2(List<double[,]> matrices, List<string> labels)
		{
			InitializeComponent();
			Redraw(matrices, labels);
		}
		public List<double[,]> CurMatrices;
		public List<string> CurLabels;
		public void Redraw(List<double[,]> matrices, List<string> labels)
		{
			CurMatrices = matrices;
			CurLabels = labels;
			var PB = new List<PictureBox> { pictureBox1, pictureBox2, pictureBox3, pictureBox4, pictureBox5 };
			var LB = new List<System.Windows.Forms.Label> { label1, label2, label3, label4, label5 };
			for (int i = 0; i < 5; i++)
			{
				DrawGraph(CurMatrices[i], PB[i]);
				LB[i].Text = CurLabels[i];
			}
		}

		private Bitmap DrawGraph(double[,] matrix, PictureBox pictureBox)
		{
			try
			{
				var G = GenerateGraph(matrix);
				Bitmap bitmap = DrawBitmap(G, pictureBox);
				//bitmap.Save("graph_visualizing_output.png");
				pictureBox.SizeChanged += (object sender, EventArgs e) =>
				{
					DrawBitmap(G, (PictureBox)sender);
				};
				return bitmap;
			}
			catch { }
			return null;
		}

		/// <summary>
		/// создадим орграф
		/// </summary>
		/// <param name="M">матрица весов орграфа</param>
		/// <returns></returns>
		private Graph GenerateGraph(double[,] M)
		{
			if (M.GetLength(0) != M.GetLength(1))
				throw new MyException(EX_matrix_not_square);
			int n = M.GetLength(0);
			Graph graph = new Graph("");
			for (int i = 0; i < n; i++)
			{
				Node node = graph.AddNode(ind2letter[i]);
				node.Attr.LabelMargin = 1;
				node.Attr.FillColor = node_color;
				node.Attr.Shape = Shape.Circle;
				for (int j = 0; j < n; j++)
				{
					if (M[i, j] != 0 && Math.Abs(M[i, j]) != INF)
					{
						Edge edge = graph.AddEdge(ind2letter[i], string.Format("{0:0.####}", M[i, j]), ind2letter[j]);
						edge.Label.FontSize = node.Label.FontSize * 0.75;
					}
				}
			}
			return graph;
		}

		private Bitmap DrawBitmap(Graph g, PictureBox drawing_field)
		{
			GraphRenderer renderer = new GraphRenderer(g);
			renderer.CalculateLayout();
			Bitmap bitmap = new Bitmap(
				(int)drawing_field.Width, (int)drawing_field.Height, PixelFormat.Format32bppPArgb);
			renderer.Render(bitmap);
			drawing_field.Image = (Image)bitmap;
			return bitmap;
		}

	}

}
