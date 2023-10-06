using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Algorithms;

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
			var PB = new List<PictureBox>();
			for(int k =0; k <m; k++)
			{
				var pb = new PictureBox();
				this.flowLayoutPanel1.Controls.Add(pb);
				pb.Size = new Size(300,400);
				PB.Add(pb);
				DrawGraph(CurMatrices[k], PB[k]);
			}
		}
	}
}
