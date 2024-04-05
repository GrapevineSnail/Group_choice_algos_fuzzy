using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Form1;
using static Group_choice_algos_fuzzy.Model;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using System.Drawing.Imaging;
using System.Data;
using System.Text.RegularExpressions;
using static Group_choice_algos_fuzzy.FileOperations;
using static Group_choice_algos_fuzzy.VisualInterfaceFuncs;
using static System.IO.DirectoryInfo;
using System.ComponentModel;
using System.Reflection;



namespace Group_choice_algos_fuzzy
{
	public class VisualInterfaceFuncs
	{
		public VisualInterfaceFuncs(Form1 f1, Form2 f2, Form3 f3_input_expert_matrices) {
			form1 = f1;
			form2 = f2;
			form3_input_expert_matrices = f3_input_expert_matrices;
		}
		public static Form1 form1;
		public static Form2 form2;
		public static Form3 form3_input_expert_matrices;

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

		/// <summary>
		/// начальное расцвечивание формы
		/// </summary>
		/// <param name="main_control"></param>
		public void interface_coloring(Control main_control)
		{
			try
			{
				foreach (Control c in main_control.Controls)
				{
					if (c as Button != null)
					{
						var b = c as Button;
						b.BackColor = button_bg_color;
						b.FlatAppearance.BorderColor = button_bg_color;
					}
					else
						interface_coloring(c);
				}
			}
			catch (MyException ex) { }
		}
		public static void activate_dgvs(Control.ControlCollection DGVs)
		{
			try
			{
				foreach (DataGridView dgv in DGVs)
				{
					int rws = dgv.RowCount;
					int cls = dgv.ColumnCount;
					for (int i = 0; i < rws; i++)
					{
						for (int j = 0; j < cls; j++)
						{
							color_input_cell(dgv, i, j, input_bg_color);
						}
					}
					dgv.ReadOnly = false;
				}
			}
			catch (MyException ex) { }
		}
		public static void deactivate_dgvs(Control.ControlCollection DGVs)
		{
			try
			{
				foreach (DataGridView dgv in DGVs)
				{
					int rws = dgv.RowCount;
					int cls = dgv.ColumnCount;
					for (int i = 0; i < rws; i++)
					{
						for (int j = 0; j < cls; j++)
						{
							color_input_cell(dgv, i, j, input_bg_color_disabled);
						}
					}
					dgv.ReadOnly = true;
				}
			}
			catch (MyException ex) { }
		}

		/// <summary>
		/// обновление размеров визуальных элементов после их изменения...
		/// </summary>
		public static void set_controls_size()
		{
			System.Drawing.Size get_table_size(DataGridView dgv)
			{
				var Width = dgv.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + 2 * dgv.RowHeadersWidth;
				var Height = dgv.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + 2 * dgv.ColumnHeadersHeight;
				return new System.Drawing.Size(Width, Height);
			}

			foreach (DataGridView dgv in form1.flowLayoutPanel_input_tables.Controls)
			{
				dgv.Dock = DockStyle.None;
				dgv.Size = get_table_size(dgv);
			}

			foreach (Method m in Methods.GetMethods())
			{
				DataGridView dgv = m?.UI_Controls.ConnectedTableFrame;

				if (dgv != null)
				{
					dgv.AutoResizeColumnHeadersHeight();
					GroupBox frame = (GroupBox)dgv?.Parent;
					if (frame != null)
					{
						frame.Dock = DockStyle.Top;
						frame.AutoSize = true;
					}
					dgv.Dock = DockStyle.None;
					dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left;
					dgv.AutoSize = true;
					dgv.Size = get_table_size(dgv);

					System.Windows.Forms.Label lab = m?.UI_Controls.ConnectedLabelControl;
					if (lab != null)
					{
						lab.Location = new System.Drawing.Point(0, dgv.Location.Y + dgv.Height);
					}
				}
			}
		}




	}
}
