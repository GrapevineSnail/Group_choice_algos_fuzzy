using System.Collections.Generic;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Algorithms;


namespace Group_choice_algos_fuzzy
{
	public partial class Form2 : Form, IFromGraphsDraw
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


	}

}
