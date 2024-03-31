using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System.Drawing.Imaging;

namespace Group_choice_algos_fuzzy
{
	class VisualInterfaceFuncs
	{
		/// <summary>
		/// отрисовать граф по матрице в PictureBox
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="pictureBox"></param>
		/// <returns></returns>
		public static Bitmap DrawGraph(double[,] matrix, PictureBox pictureBox)
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
		public static Graph GenerateGraph(double[,] M)
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

		/// <summary>
		/// создать картинку графа (битмап)
		/// </summary>
		/// <param name="g"></param>
		/// <param name="drawing_field"></param>
		/// <returns></returns>
		public static Bitmap DrawBitmap(Graph g, PictureBox drawing_field)
		{
			GraphRenderer renderer = new GraphRenderer(g);
			renderer.CalculateLayout();
			Bitmap bitmap = new Bitmap(
				(int)drawing_field.Width, (int)drawing_field.Height, PixelFormat.Format32bppPArgb);
			renderer.Render(bitmap);
			drawing_field.Image = (Image)bitmap;
			return bitmap;
		}

		/// <summary>
		/// обновить рисунки графов
		/// </summary>
		/// <param name="f"></param>
		/// <param name="M"></param>
		/// <param name="L"></param>
		public static void OrgraphsPics_update(IFromGraphsDraw f, List<Matrix> M, List<string> L)
		{
			if (f != null && !((Form)f).IsDisposed)
			{
				f.Redraw(M.Select(x => x.matrix_base).ToList(), L);
			}
		}

		/// <summary>
		/// сравнивалась ли альтернатива с какой-то ещё
		/// </summary>
		/// <param name="index"></param>
		/// <param name="dgv"></param>
		/// <returns></returns>
		public static bool IsCompared(int index, DataGridView dgv)
		{
			for (int j = 0; j < dgv.Rows.Count; j++)
			{
				if (index != j)
				{
					var ij = dgv[j, index].Value as double?;
					var ji = dgv[index, j].Value as double?;
					if (ij != 0 || ji != 0)
						return true;
				}
			}
			return false;
		}
		private static void SetDataGridViewDefaults_FontAndColors(DataGridView dgv)
		{
			//dgv.DefaultCellStyle.Font = new Font(font, font_size);
			dgv.DefaultCellStyle.ForeColor = font_color;
			dgv.DefaultCellStyle.BackColor = input_bg_color;
			dgv.DefaultCellStyle.SelectionForeColor = font_color;
			dgv.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.Empty;
		}

		/// <summary>
		/// настрйки для вывода DataGridView
		/// </summary>
		/// <param name="dgv"></param>
		public static void SetDataGridViewDefaults(DataGridView dgv)
		{
			dgv.AllowUserToAddRows = false;
			dgv.AllowUserToDeleteRows = false;
			dgv.AllowUserToResizeRows = false;
			dgv.AllowUserToResizeColumns = false;
			dgv.AllowUserToOrderColumns = false;
			dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
			dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
			dgv.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgv.ShowEditingIcon = false;
			dgv.DefaultCellStyle.Format = $"0.{new string('#', DIGITS_PRECISION)}";
			dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
			SetDataGridViewDefaults_FontAndColors(dgv);
			dgv.DataError += (object ss, DataGridViewDataErrorEventArgs anError) => { dgv.CancelEdit(); };
		}

	}
}
