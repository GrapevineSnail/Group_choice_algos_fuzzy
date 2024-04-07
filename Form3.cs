using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.GraphDrawingOperations;

namespace Group_choice_algos_fuzzy
{
	public partial class Form3 : Form, IFromGraphsDraw
	{
		public Form3()
		{
			InitializeComponent();
		}
		public Form3(List<double[,]> matrices, List<string> labels)
		{
			InitializeComponent();
			Redraw(matrices, labels);
		}
		public List<double[,]> CurMatrices;
		public List<string> CurLabels;
		public void Redraw(List<double[,]> matrices, List<string> labels)
		{
			flowLayoutPanel1.Controls.Clear();
			CurMatrices = matrices;
			CurLabels = labels;
			int m = matrices.Count();
			var pb_list = new List<PictureBox>();
			var lb_list = new List<System.Windows.Forms.Label>();
			for (int k =0; k <m; k++)
			{
				lb_list.Add(new Label());
				pb_list.Add(new PictureBox());
				this.flowLayoutPanel1.Controls.Add(lb_list[k]);
				this.flowLayoutPanel1.Controls.Add(pb_list[k]);
				pb_list[k].Size = new Size(400,400);
				lb_list[k].Text = CurLabels[k];
				lb_list[k].AutoSize = true;
				DrawGraph(CurMatrices[k], pb_list[k]);

			}
		}
	}
}
