using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.ClassOperations.OPS_GraphDrawing;


namespace Group_choice_algos_fuzzy
{
	public partial class Form2 : Form, IFromGraphsDraw
	{
		public Form2()
		{
			InitializeComponent();
		}
		public Form2(Dictionary<string, double[,]> labeled_matrices)
		{
			InitializeComponent();
			Redraw(labeled_matrices);
		}
		public void Redraw(Dictionary<string, double[,]> labeled_matrices)
		{
			tableLayoutPanel1.Controls.Clear();
			var K = labeled_matrices.Count;
			//var CCnt = tableLayoutPanel1.ColumnCount;
			tableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
			tableLayoutPanel1.AutoScroll = true;
			tableLayoutPanel1.Padding = new Padding(0);
			tableLayoutPanel1.Margin = new Padding(0);
			var pb_list = new PictureBox[K];
			var lb_list = new System.Windows.Forms.Label[K];
			for (int k = 0; k < K; k++)
			{
				lb_list[k] = new Label();
				pb_list[k] = new PictureBox();

				lb_list[k].Text = labeled_matrices.ElementAt(k).Key;
				lb_list[k].AutoSize = false;
				lb_list[k].Dock = DockStyle.Fill;

				DrawGraph(labeled_matrices.ElementAt(k).Value, pb_list[k]);
				pb_list[k].SizeMode = PictureBoxSizeMode.StretchImage;
				pb_list[k].Dock = DockStyle.Fill;

				var container = new TableLayoutPanel();
				container.AutoScroll = true;
				container.Padding = new Padding(0);
				container.Margin = new Padding(0);
				container.Controls.Add(pb_list[k]);
				container.Controls.Add(lb_list[k]);
				container.Dock = DockStyle.Fill;
				container.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
				container.RowStyles.Add(new RowStyle(SizeType.Percent,20F));
				
				//var pos_lab = new TableLayoutPanelCellPosition(k % CCnt, k/CCnt - k % (CCnt));
				//tableLayoutPanel1.SetCellPosition(container, pos_lab);
				this.tableLayoutPanel1.Controls.Add(container);
			}
		}
	}
}
