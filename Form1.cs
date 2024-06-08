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

			ClearModelDerivatives(true);

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


		void ClearModelDerivatives(bool clear_UI)
		{
			Methods.Clear();
			AggregatedMatrix.Clear();
			if (clear_UI)
			{
				Methods.ClearMethodsOutputView();
				AggregatedMatrix.UI_Controls.UI_Clear();
			}
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
			button_read_file.Height = textBox_file.Height + 2;
			button_n_m.Height = textBox_file.Height + 2;
			foreach (DataGridView dgv in ExpertRelations.UI_ControlsAndView.GetOutputDataGridViews)
			{
				dgv.Dock = DockStyle.None;
				dgv.Size = OPS_DataGridView.GetTableSize(dgv);
			}
			foreach (Method m in Methods.GetMethods())
			{
				DataGridView dgv = m?.UI_Controls.ConnectedTableFrame;
				if (dgv != null)
				{
					dgv.AutoResizeColumnHeadersHeight();
					dgv.AutoResizeRows();
					dgv.RowHeadersWidth = row_headers_width;
					//dgv.Dock = DockStyle.None;
					//dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left;
					//dgv.AutoSize = true;
					dgv.Size = OPS_DataGridView.GetTableSize(dgv);
					Label lab = m?.UI_Controls.ConnectedLabel;
					if (lab != null)
					{
						lab.Location = new System.Drawing.Point(0, dgv.Location.Y + dgv.Height);
					}
				}
			}
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
				ExpertRelations.Model.CheckAndSetMatrices(ExpertRelations.Model.GetMatrices(), true);
				AggregatedMatrix.UI_Controls.UI_Clear();
				Methods.ClearMethodsOutputView();
				ExpertRelations.UI_ControlsAndView.UI_Deactivate();
				Methods.ExecuteAlgorythms();
				AggregatedMatrix.UI_Controls.UI_Show();
				Methods.ShowMethodsOutputView(true, true, OUT_FILE, true);
				set_controls_size();
			}
			catch (MyException ex) { ex.Info(); }
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
					ExpertRelations.UI_ControlsAndView.UpdateModel_n_m_fromView();
					ClearModelDerivatives(true);
					var matrices = new List<Matrix>(m);
					for (int k = 0; k < m; k++)
					{
						matrices.Add(new Matrix(n));
					}
					ExpertRelations.Model.SetMatrices(matrices, true);
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
				if (!ReadFileOneTest(textBox_file.Text,	out var absolute_filename, 
					out List<Matrix> matrices, out string bad_string))
					throw new FileNotFoundException();
				textBox_file.Text = absolute_filename;
				if (matrices is null || matrices.Count == 0)
					throw new MyException(EX_bad_file + CR_LF + bad_string);
				ClearModelDerivatives(true);
				ExpertRelations.Model.SetMatrices(matrices, true);
				ExpertRelations.UI_ControlsAndView.UI_Show();
				ExpertRelations.UI_ControlsAndView.UI_Activate();
				set_controls_size();
			}
			catch (FileNotFoundException ex) { Info(ex); }
			catch (MyException ex) { ex.Info(); }
		}

		/// <summary>
		/// прогнать несколько тестов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_read_file_several_tests_Click(object sender, EventArgs e)
		{
			try
			{
				if (!ReadFileSeveralTests(textBox_file.Text, out var absolute_filename,
					out List<List<Matrix>> tests, out List<string[]> rawtext_tests))
					throw new FileNotFoundException();
				textBox_file.Text = absolute_filename;
				if (tests is null || tests.Count == 0)
					throw new MyException(EX_bad_file + CR_LF + absolute_filename);
				string out_filename = WriteToFile($"<root>{CR_LF}", OUT_FILE, false);
				for (int t = 0; t < tests.Count; t++)
				{
					string out_testtext = "";
					try
					{
						List<Matrix> matrices = tests[t];
						if (matrices is null || matrices.Count == 0)
							throw new MyException(EX_bad_file +
								CR_LF + absolute_filename + CR_LF + string.Join(CR_LF, rawtext_tests[t]));
						ClearModelDerivatives(false);
						ExpertRelations.Model.CheckAndSetMatrices(matrices, false);
						Methods.ExecuteAlgorythms();
						out_testtext += Methods.ShowMethodsOutputView(false, false);
					}
					catch (MyException ex)
					{
						out_testtext += ex.InfoTextXML() + CR_LF;
					}
					catch (Exception ex)
					{
						out_testtext += InfoTextXML(ex) + CR_LF;
					}
					finally
					{
						WriteToFile(out_testtext, out_filename, true);
					}
				}
				WriteToFile($"{CR_LF}</root>", out_filename, true);
				throw new MyException($"{INF_output_written_in_file} {out_filename}");
			}
			catch (FileNotFoundException ex) { Info(ex); }
			catch (MyException ex) { ex.Info(); }
			catch (Exception ex) { Info(ex); }
		}
		/// <summary>
		/// вывести рисунки графов на формах
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
		private void dg_RowHeightChanged(object sender, DataGridViewRowEventArgs e)
		{
			var dgv = sender as DataGridView;
			(dgv).Size = OPS_DataGridView.GetTableSize(dgv);
		}
		private void dg_All_rankings_RowHeadersWidthChanged(object sender, EventArgs e)
		{
			var dgv = sender as DataGridView;
			(dgv).Size = OPS_DataGridView.GetTableSize(dgv);
		}
	}
}
