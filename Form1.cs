using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Constants.MyException;
using static Group_choice_algos_fuzzy.FileOperations;
using static Group_choice_algos_fuzzy.Model;
using static Group_choice_algos_fuzzy.GraphDrawingOperations;

namespace Group_choice_algos_fuzzy
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			//связывание модели с control-ами на форме

			Model.form1 = this;

			ExpertRelations.UI_Controls = new ExpertRelations.ConnectedControls(
				cb_show_input_matrices, cb_do_transitive_closure,
				numericUpDown_n, numericUpDown_m, flowLayoutPanel_input_tables);
			ExpertRelations.ExpertRelations_InputRelChanged += ExpertRelations.UpdateExpertMatrix;

			AggregatedMatrix.UI_Controls = new AggregatedMatrix.ConnectedControls(
				rb_dist_square, rb_dist_modulus, label_aggreg_matrix);
			AggregatedMatrix.R_Changed += () => {
				var rtd = AggregatedMatrix.GetRelations2Draw();
				OrgraphsPics_update(form2_result_matrices, rtd.Matrices, rtd.Labels);
			};

			Methods.All_Hamiltonian_paths.UI_Controls =
				new Method.ConnectedControls(Methods.All_Hamiltonian_paths, cb_HP, dg_HP, null);
			Methods.Schulze_method.UI_Controls =
				new Method.ConnectedControls(Methods.Schulze_method, cb_Schulze_method, dg_Schulze_method, null);
			Methods.All_various_rankings.UI_Controls =
				new Method.ConnectedControls(Methods.All_various_rankings, cb_All_rankings, dg_All_rankings, null);
			Methods.Smerchinskaya_Yashina_method.UI_Controls =
				new Method.ConnectedControls(Methods.Smerchinskaya_Yashina_method, cb_SY, dg_SY, null);

			foreach (Control c in ExpertRelations.UI_Controls.GetOutputControls)
				c.MouseDown += flowLayoutPanel_input_tables_MouseDown;
			foreach (Control c in ExpertRelations.UI_Controls.GetOutputLabels)
				c.MouseDown += flowLayoutPanel_output_info_MouseDown;
			foreach (Control c in ExpertRelations.UI_Controls.GetOutputControls)
			{
				c.MouseDown += flowLayoutPanel_output_tables_MouseDown;
				foreach (Control c1 in c.Controls)
					c1.MouseDown += flowLayoutPanel_output_tables_MouseDown;
			}
			set_controls_size();
			interface_coloring(this);
			ClearModelDerivatives();
		}
		public static Form2 form2_result_matrices = null;
		public static Form3 form3_input_expert_matrices = null;

		void ClearModelDerivatives()
		{
			Methods.Clear(); 
			Methods.UI_ClearMethods();
			AggregatedMatrix.Clear();
			AggregatedMatrix.UI_Controls.UI_Clear();
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
		/// <summary>
		/// обновление размеров визуальных элементов после их изменения...
		/// </summary>
		public void set_controls_size()
		{
			System.Drawing.Size get_table_size(DataGridView dgv)
			{
				var Width = dgv.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + 2 * dgv.RowHeadersWidth;
				var Height = dgv.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + 2 * dgv.ColumnHeadersHeight;
				return new System.Drawing.Size(Width, Height);
			}

			button_read_file.Height = textBox_file.Height + 2;
			button_n_m.Height = textBox_file.Height + 2;

			foreach (DataGridView dgv in ExpertRelations.UI_Controls.GetOutputDataGridViews)
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

					System.Windows.Forms.Label lab = m?.UI_Controls.ConnectedLabel;
					if (lab != null)
					{
						lab.Location = new System.Drawing.Point(0, dgv.Location.Y + dgv.Height);
					}
				}
			}
		}
		/// <summary>
		/// Считывает с формы переменные n и m (количество альтернатив и экспертов)
		/// </summary>
		/// 
		private void button_n_m_Click(object sender, EventArgs e)
		{
			try
			{
				if(numericUpDown_n.Value != n || numericUpDown_m.Value != m)
				{
					ExpertRelations.UI_Controls.UpdateModel_n_m();
					ClearModelDerivatives();
					var matrices = new List<Matrix>(m);
					for (int k = 0; k < m; k++)
					{
						matrices.Add(new Matrix(n));
					}
					ExpertRelations.UpdateExpertMatrices(matrices);
				}
				ExpertRelations.UI_Controls.UI_Show();
				ExpertRelations.UI_Controls.UI_Activate();
			}
			catch (MyException ex) { ex.Info(); }
		}

		/// <summary>
		/// считать профили экспертов из файла
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_read_file_Click(object sender, EventArgs e)
		{
			try
			{
				try
				{
					List<Matrix> matrices = ReadFileWithMatrices(textBox_file.Text, out var absolute_filename);
					textBox_file.Text = absolute_filename;
					m = matrices.Count;
					n = matrices.First().n;
					ClearModelDerivatives();
					ExpertRelations.UpdateExpertMatrices(matrices);
					ExpertRelations.UI_Controls.UI_Show();
					ExpertRelations.UI_Controls.UI_Activate();
				}
				catch (FileNotFoundException ex)
				{
					throw new MyException($"{ex.Message}");
				}
			}
			catch (MyException ex) { ex.Info(); }
		}

		/// <summary>
		/// чтение входных профилей и запуск работы программы на выбранных алгоритмах
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_run_program_Click(object sender, EventArgs e)
		{
			try
			{
				if (cb_All_rankings.Checked && n > max_count_of_alternatives)
					throw new MyException(EX_n_m_too_big);
				if (cb_do_transitive_closure.Checked)
				{
					var matrices = new List<Matrix>();
					matrices = ExpertRelations.GetModelMatrices().Select(x => x.Cast2Fuzzy.TransClosured.ToMatrix).ToList();
					ExpertRelations.UpdateExpertMatrices(matrices);

				}
				AggregatedMatrix.UI_Controls.UI_Clear();
				Methods.UI_ClearMethods();
				ExpertRelations.UI_Controls.UI_Deactivate();
				Methods.ExecuteAlgorythms();
				AggregatedMatrix.UI_Controls.UI_Show();
				Methods.UI_ShowMethods();
				form1.set_controls_size();
			}
			catch (MyException ex) { ex.Info(); }
		}
		/// <summary>
		/// тестовая кнопочка, для разработчика
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_for_tests_Click(object sender, EventArgs e)
		{
			/*
			RefreshModel();//?
			var matrices = new List<FuzzyRelation>();

			label_aggreg_matrix.Text = Directory.GetCurrentDirectory().ToString();
			//список всех файлов директории !!!
			List<string> files = new List<string>() { textBox_file.Text };//

			foreach (string filename in files)
			{
				string[] lines = File.ReadAllLines(filename)
					.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
				var chars_for_split = new char[] { ' ' };
				var n = lines.First().Split(chars_for_split, StringSplitOptions.RemoveEmptyEntries).Count();
				var cur_matrix = new FuzzyRelation(n);
				for (int l = 0; l < lines.Length; l++)
				{
					if (lines[l].Length != 0)
					{
						double res;
						double[] numbers = lines[l].Split(chars_for_split, StringSplitOptions.RemoveEmptyEntries)
							.Select(x => double.TryParse(x, out res) ? res : INF).ToArray();
						for (int j = 0; j < numbers.Length; j++)
							cur_matrix[l % n, j] = numbers[j];
					}
					if (l % n == n - 1)
						matrices.Add(cur_matrix.Cast2Fuzzy);
				}
				m = matrices.Count;
				//R_list = Enumerable.Select(R_list, x => Matrix.GetAsymmetricPart(x)).ToList();
				var Intersect = Methods.ExecuteAlgorythms();
				set_output_results(Intersect);//вывод в файл или начисление статистики

				//вывод статистики в файл
			}
			*/
		}

		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			set_controls_size();
		}
		private void button_visualize_orgraph_Click(object sender, EventArgs e)
		{
			var rtd = AggregatedMatrix.GetRelations2Draw();
			var M = rtd.Matrices;
			var L = rtd.Labels;
			if (M.Any(x => x != null))
			{
				form2_result_matrices?.Dispose();
				form2_result_matrices = new Form2();
				OrgraphsPics_update(form2_result_matrices, M, L);
				form2_result_matrices.Show();
			}
			//входные матрицы экспертов
			M = flowLayoutPanel_input_tables.Controls.OfType<DataGridView>()
					   .Select(x => Matrix.GetFromDataGridView(x)).ToList();
			L = new List<string>();
			for (int i = 0; i < M.Count; i++)
			{
				L.Add($"Expert{i}:");
			}
			if (M.Any(x => x != null))
			{
				form3_input_expert_matrices?.Dispose();
				form3_input_expert_matrices = new Form3();
				OrgraphsPics_update(form3_input_expert_matrices, M, L);
				form3_input_expert_matrices.Show();
			}
		}
		private void flowLayoutPanel_output_info_MouseDown(object sender, MouseEventArgs e)
		{
			flowLayoutPanel_output_info.Focus();
		}
		private void flowLayoutPanel_output_tables_MouseDown(object sender, MouseEventArgs e)
		{
			flowLayoutPanel_output_tables.Focus();
		}
		private void flowLayoutPanel_input_tables_MouseDown(object sender, MouseEventArgs e)
		{
			flowLayoutPanel_input_tables.Focus();
		}

	}
}
