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
			form1_mainform = this;

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
			ColorUI(this);
			UpdateControlsSize();

			Dictionary<Control, EventHandler> controls_n_handlers = new Dictionary<Control, EventHandler> {
				{ flowLayoutPanel_input_tables  , flowLayoutPanel_input_tables_MouseEnter  },
				{ flowLayoutPanel_output_info   , flowLayoutPanel_output_info_MouseEnter   },
				{ flowLayoutPanel_output_tables , flowLayoutPanel_output_tables_MouseEnter }
			};
			this.Activated += (object sender, EventArgs args) =>
			{
				foreach (var key in controls_n_handlers.Keys)
				{
					SetFocusHandlersEnterMouse(key, controls_n_handlers[key], true);
				}
			};
			this.Deactivate += (object sender, EventArgs args) =>
			{
				foreach (var key in controls_n_handlers.Keys)
				{
					SetFocusHandlersEnterMouse(key, controls_n_handlers[key], false);
				}
			};
		}

		#region FIELDS
		public static Form1 form1_mainform = null;
		public static Form2 form2_result_matrices = null;
		public static Form3 form3_input_expert_matrices = null;
		#endregion FIELDS

		#region AUXILIARY FUNCTIONS
		/// <summary>
		/// очистить производные объекты, оставшиеся после выполнения алгоритмов
		/// </summary>
		/// <param name="clear_UI"></param>
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
		void ColorUI(Control main_control)
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
						ColorUI(c);
				}
			}
			catch { }
		}
		/// <summary>
		/// чтобы можно было прокручивать колёсиком при наведении мышки
		/// </summary>
		/// <param name="f"></param>
		/// <param name="action"></param>
		/// <param name="set_action">установаить (true), либо удалить обработчик</param>
		void SetFocusHandlersEnterMouse(Control f, EventHandler action, bool set_action)
		{
			if (set_action)
				f.MouseEnter += action;
			else
				f.MouseEnter -= action;
			foreach (Control c in f.Controls)
			{
				SetFocusHandlersEnterMouse(c, action, set_action);
			}
		}
		/// <summary>
		/// обновление размеров визуальных элементов после их изменения...
		/// </summary>
		void UpdateControlsSize()
		{
			button_read_file.Height = textBox_file.Height + 2;
			button_n_m.Height = textBox_file.Height + 2;
			foreach (DataGridView dgv in ExpertRelations.UI_ControlsAndView.GetOutputDataGridViews)
			{
				dgv.Dock = DockStyle.None;
				dgv.Size = OPS_DataGridView.GetTableSize(dgv);
			}
			groupBox3.Dock = DockStyle.Fill;
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
		#endregion AUXILIARY FUNCTIONS

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
				UpdateControlsSize();
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
				UpdateControlsSize();
			}
			catch (MyException ex) { ex.Info(); }
		}
		/// <summary>
		/// считать матрицы экспертов из файла (один тест)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_read_file_Click(object sender, EventArgs e)
		{
			try
			{
				if (!ReadFileOneTest(textBox_file.Text, out var absolute_filename,
					out List<Matrix> matrices, out string bad_string))
					throw new FileNotFoundException();
				textBox_file.Text = absolute_filename;
				if (matrices is null || matrices.Count == 0 || bad_string?.Length > 0)
					throw new MyException(EX_bad_file + CR_LF + bad_string);
				ClearModelDerivatives(true);
				ExpertRelations.Model.SetMatrices(matrices, true);
				ExpertRelations.UI_ControlsAndView.UI_Show();
				ExpertRelations.UI_ControlsAndView.UI_Activate();
				UpdateControlsSize();
			}
			catch (FileNotFoundException ex) { Info(ex); }
			catch (MyException ex) { ex.Info(); }
		}
		/// <summary>
		/// считать несколько групп экспертных матриц из файла (прогнать несколько тестов)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void button_read_file_several_tests_Click(object sender, EventArgs e)
		{
			try
			{
				if (!ReadFileSeveralTests(textBox_file.Text, out var input_absolute_filename,
					out List<List<Matrix>> tests, out List<string[]> unparsed_text,
					out string error_text))
					throw new MyException("Возможные ошибки:" + CR_LF
						+ new FileNotFoundException().Message + CR_LF
						+ error_text);
				textBox_file.Text = input_absolute_filename;
				if (tests is null || tests.Count == 0)
					throw new MyException(EX_bad_file + CR_LF + input_absolute_filename);
				//всё в файл пишется сразу же, чтоб не оставить пустой файл после падения
				string out_absolute_filename = WriteToFile($"<root>{CR_LF}", OUT_FILE, false);
				for (int t = 0; t < tests.Count; t++)
				{
					string text = "";
					try
					{
						List<Matrix> matrices = tests[t];
						if (matrices is null || matrices.Count == 0)
							throw new MyException(EX_bad_file + CR_LF
								+ input_absolute_filename + CR_LF
								+ string.Join(CR_LF, unparsed_text[t]));
						ClearModelDerivatives(false);
						ExpertRelations.Model.CheckAndSetMatrices(matrices, false);
						Methods.ExecuteAlgorythms();
						text += Methods.ShowMethodsOutputView(false, false);
					}
					catch (MyException ex) { text += ex.InfoTextXML() + CR_LF; }
					catch (Exception ex) { text += InfoTextXML(ex) + CR_LF; }
					finally
					{
						WriteToFile(text, out_absolute_filename, true);
					}
				}
				WriteToFile($"{CR_LF}</root>", out_absolute_filename, true);
				throw new MyException(INF_output_written_in_file + CR_LF + out_absolute_filename);//информационное сообщение об окончании работы
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
		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			UpdateControlsSize();
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
