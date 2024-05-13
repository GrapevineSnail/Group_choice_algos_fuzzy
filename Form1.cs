using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.ClassOperations;
using static Group_choice_algos_fuzzy.ClassOperations.OPS_File;
using static Group_choice_algos_fuzzy.ClassOperations.OPS_GraphDrawing;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Constants.MyException;
using static Group_choice_algos_fuzzy.Model;

namespace Group_choice_algos_fuzzy
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			//связывание модели с control-ами на форме
			Model.form1 = this;

			ExpertRelations.UI_ControlsAndView = new ExpertRelations.ConnectedControlsAndView(
				cb_show_input_matrices, cb_do_transitive_closure,
				numericUpDown_n, numericUpDown_m, flowLayoutPanel_input_tables);

			AggregatedMatrix.UI_Controls = new AggregatedMatrix.ConnectedControls(
				rb_dist_square, rb_dist_modulus, label_aggreg_matrix);
			AggregatedMatrix.R_Changed += () =>
			{
				var rtd = AggregatedMatrix.GetRelations2Show();
				UpdateOrgraphPics(form2_result_matrices, rtd);
			};

			Methods.All_Hamiltonian_paths.UI_Controls =
				new Method.ConnectedControls(Methods.All_Hamiltonian_paths, cb_HP, dg_HP, null);
			Methods.Schulze_method.UI_Controls =
				new Method.ConnectedControls(Methods.Schulze_method, cb_Schulze_method, dg_Schulze_method, null);
			Methods.All_various_rankings.UI_Controls =
				new Method.ConnectedControls(Methods.All_various_rankings, cb_All_rankings, dg_All_rankings, null);
			Methods.Smerchinskaya_Yashina_method.UI_Controls =
				new Method.ConnectedControls(Methods.Smerchinskaya_Yashina_method, cb_SY, dg_SY, null);

			ClearModelDerivatives();

			this.Activated += (object sender, EventArgs args) =>
			{
				set_focus_handlers_enter_mouse(flowLayoutPanel_input_tables,
					flowLayoutPanel_input_tables_MouseEnter, true);
				set_focus_handlers_enter_mouse(flowLayoutPanel_output_info,
					flowLayoutPanel_output_info_MouseEnter, true);
				set_focus_handlers_enter_mouse(flowLayoutPanel_output_tables,
					flowLayoutPanel_output_tables_MouseEnter, true);

			};
			this.Deactivate += (object sender, EventArgs args) =>
			{
				set_focus_handlers_enter_mouse(flowLayoutPanel_input_tables,
					flowLayoutPanel_input_tables_MouseEnter, false);
				set_focus_handlers_enter_mouse(flowLayoutPanel_output_info,
					flowLayoutPanel_output_info_MouseEnter, false);
				set_focus_handlers_enter_mouse(flowLayoutPanel_output_tables,
					flowLayoutPanel_output_tables_MouseEnter, false);
			};

			interface_coloring(this);
			set_controls_size();
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
			catch { }
		}
		/// <summary>
		/// чтобы можно было прокручивать колёсиком при наведении мышки
		/// </summary>
		/// <param name="f"></param>
		/// <param name="action"></param>
		public void set_focus_handlers_enter_mouse(Control f, EventHandler action, bool set_action)
		{
			if (set_action)
				f.MouseEnter += action;
			else
				f.MouseEnter -= action;
			foreach (Control c in f.Controls)
			{
				set_focus_handlers_enter_mouse(c, action, set_action);
			}

		}
		/// <summary>
		/// обновление размеров визуальных элементов после их изменения...
		/// </summary>
		void set_controls_size()
		{
			System.Drawing.Size get_table_size(DataGridView dgv)
			{
				var Width = dgv.Columns.GetColumnsWidth(DataGridViewElementStates.Visible)
					+ dgv.RowHeadersWidth + row_min_height;
				var Height = dgv.Rows.GetRowsHeight(DataGridViewElementStates.Visible)
					+ dgv.ColumnHeadersHeight + row_min_height;
				return new System.Drawing.Size(Width, Height);
			}

			button_read_file.Height = textBox_file.Height + 2;
			button_n_m.Height = textBox_file.Height + 2;

			foreach (DataGridView dgv in ExpertRelations.UI_ControlsAndView.GetOutputDataGridViews)
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
					dgv.AutoResizeRows();
					dgv.RowHeadersWidth = row_headers_width;
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

					Label lab = m?.UI_Controls.ConnectedLabel;
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
				if (numericUpDown_n.Value != n || numericUpDown_m.Value != m)
				{
					ExpertRelations.UI_ControlsAndView.UpdateModel_n_m();
					ClearModelDerivatives();
					var matrices = new List<Matrix>(m);
					for (int k = 0; k < m; k++)
					{
						matrices.Add(new Matrix(n));
					}
					ExpertRelations.Model.SetMatrices(matrices);
				}
				ExpertRelations.UI_ControlsAndView.UI_Show();
				ExpertRelations.UI_ControlsAndView.UI_Activate();
				set_controls_size();
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
					ExpertRelations.Model.SetMatrices(matrices);
					ExpertRelations.UI_ControlsAndView.UI_Show();
					ExpertRelations.UI_ControlsAndView.UI_Activate();
					set_controls_size();
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
				if (cb_All_rankings.Checked && n > max_count_of_alternatives_2ALL_RANKS)
					throw new MyException(EX_n_too_big);
				ExpertRelations.Model.CheckAndSetMatrices(ExpertRelations.Model.GetMatrices());
				AggregatedMatrix.UI_Controls.UI_Clear();
				Methods.UI_ClearMethods();
				ExpertRelations.UI_ControlsAndView.UI_Deactivate();
				Methods.ExecuteAlgorythms();
				AggregatedMatrix.UI_Controls.UI_Show();
				Methods.UI_ShowMethods(true);
				set_controls_size();
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

		private void button_visualize_orgraph_Click(object sender, EventArgs e)
		{
			var rtd = AggregatedMatrix.GetRelations2Show();
			if (rtd.Values.Any(x => x != null))
			{
				form2_result_matrices?.Dispose();
				form2_result_matrices = new Form2();
				UpdateOrgraphPics(form2_result_matrices, rtd);
				form2_result_matrices.Show();
			}
			if (ExpertRelations.Model.GetMatrices().Any(x => x != null))
			{
				form3_input_expert_matrices?.Dispose();
				form3_input_expert_matrices = new Form3();
				ExpertRelations.UI_ControlsAndView.UpdateExpertGraphs();
				form3_input_expert_matrices.Show();
			}
		}
		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			set_controls_size();
		}
		/// <summary>
		/// убрано из Form1.Designer
		/// </summary>
		private void flowLayoutPanel_input_tables_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_input_tables.Focus();
		}
		/// <summary>
		/// убрано из Form1.Designer
		/// </summary>
		private void flowLayoutPanel_output_info_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_output_info.Focus();
		}
		/// <summary>
		/// убрано из Form1.Designer
		/// </summary>
		private void flowLayoutPanel_output_tables_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_output_tables.Focus();
		}
	}
}
