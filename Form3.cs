using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.ClassOperations.OPS_GraphDrawing;

namespace Group_choice_algos_fuzzy
{
	public partial class Form3 : Form, IFromGraphsDraw
	{
		public Form3()
		{
			InitializeComponent();
		}
		public Form3(Dictionary<string, double[,]> labeled_matrices)
		{
			InitializeComponent();
			Redraw(labeled_matrices);
		}
		public List<double[,]> CurMatrices;
		public List<string> CurLabels;
		public void Redraw(Dictionary<string, double[,]> labeled_matrices)
		{
			flowLayoutPanel1.Controls.Clear();
			int m = labeled_matrices.Count();
			var pb_list = new PictureBox[m];
			var lb_list = new System.Windows.Forms.Label[m];
			for (int k = 0; k < m; k++)
			{
				lb_list[k] = new Label();
				pb_list[k] = new PictureBox();
				lb_list[k].Text = labeled_matrices.ElementAt(k).Key;
				lb_list[k].AutoSize = true;
				pb_list[k].Size = new Size(300, 300);
				pb_list[k].SizeMode = PictureBoxSizeMode.Zoom;
				DrawGraph(labeled_matrices.ElementAt(k).Value, pb_list[k]);
				this.flowLayoutPanel1.Controls.Add(lb_list[k]);
				this.flowLayoutPanel1.Controls.Add(pb_list[k]);
				/*
				this.flowLayoutPanel1.Resize += (sender, e) => {
					foreach (var pb in this.flowLayoutPanel1.Controls.OfType<PictureBox>())
					{
						var s_outer = this.ClientSize.Height;
						var s_inner = pb.ClientSize.Height;
						if (s_outer != s_inner)
						{
							//float scale = s_outer / s_inner;
							//pb.Scale(new SizeF(scale,scale));
							pb.Size = this.flowLayoutPanel1.ClientSize;
						}
					}
				};
				*/
			}
		}
	}
}
