﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.FileOperations;
using static Group_choice_algos_fuzzy.Model;
using static Group_choice_algos_fuzzy.VisualInterfaceFuncs;
using static System.IO.DirectoryInfo;
using System.ComponentModel;
using System.Reflection;

namespace Group_choice_algos_fuzzy
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			//связывание методов с control-ами на форме
			Methods.All_Hamiltonian_paths.VisualFormConnectedControls.Set(cb_HP, dg_HP);
			Methods.Schulze_method.VisualFormConnectedControls.Set(cb_Schulze_method, dg_Schulze_method);
			Methods.All_various_rankings.VisualFormConnectedControls.Set(cb_All_rankings, dg_All_rankings);
			Methods.Smerchinskaya_Yashina_method.VisualFormConnectedControls.Set(cb_SY, dg_SY);

			AggregatedMatrix.R_Changed += R_UpdateGraphPicture;

			button_read_file.Height = textBox_file.Height + 2;
			button_n_m.Height = textBox_file.Height + 2;

			foreach (Control c in flowLayoutPanel_input_tables.Controls)
				c.MouseDown += flowLayoutPanel_input_tables_MouseDown;
			foreach (Control c in flowLayoutPanel_output_info.Controls.OfType<Label>())
				c.MouseDown += flowLayoutPanel_output_info_MouseDown;
			foreach (Control c in flowLayoutPanel_output_tables.Controls)
			{
				c.MouseDown += flowLayoutPanel_output_tables_MouseDown;
				foreach (Control c1 in c.Controls)
					c1.MouseDown += flowLayoutPanel_output_tables_MouseDown;
			}
			interface_coloring(this);
			clear_output();
		}
		Form2 form2_result_matrices = null;
		Form3 form3_input_expert_matrices = null;

		#region Обновление матриц экспертов (Model)
		public List<Matrix> ExpertMatricesInUI;
		//public delegate void ExpertMatricesInUI_EventHandler(object sender, ExpertMatricesEventArgs e);
		public event EventHandler<ExpertMatricesEventArgs> ExpertMatricesInUI_EventHandler;

		protected virtual void ExpertMatricesInUI_Update(object sender, ExpertMatricesEventArgs e)
		{
			EventHandler<ExpertMatricesEventArgs> handler = ExpertMatricesInUI_EventHandler;
			handler?.Invoke(this, e);
		}
		public class ExpertMatricesEventArgs : EventArgs
		{
			public int expert_index { get; set; }
			public Matrix fill_values { get; set; }
		}
		List<DataGridView> GetDGVList()
		{
			return flowLayoutPanel_input_tables.Controls.OfType<DataGridView>().ToList();
		}
		List<Matrix> GetMatrixListFromDGVs()
		{
			return GetDGVList().Select(x => Matrix.GetFromDataGridView(x)).ToList();
		}
		void DeactivateSymmetricCell(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				var dd = sender as DataGridView;
				int i = e.RowIndex;
				int j = e.ColumnIndex;
				if (i == j)
					color_input_cell(dd, i, j, input_bg_color_disabled);
				else
				{
					double Mij, Mji;
					double.TryParse(dd[j, i]?.Value?.ToString(), out Mij);
					double.TryParse(dd[i, j]?.Value?.ToString(), out Mji);
					if (Mij == 0 && Mji != 0)
					{
						color_input_cell(dd, i, j, input_bg_color_disabled);
						color_input_cell(dd, j, i, input_bg_color);
					}
					else if (Mij != 0 && Mji == 0)
					{
						color_input_cell(dd, i, j, input_bg_color);
						color_input_cell(dd, j, i, input_bg_color_disabled);
					}
					else
					{
						color_input_cell(dd, i, j, input_bg_color);
						color_input_cell(dd, j, i, input_bg_color);
					}
				}
			}
			catch (MyException ex) { ex.Info(); }
		}
		void UpdateExpertDGV(List<Matrix> lst_of_matrices, int expert_index)
		{
			try
			{
				var DGV = GetDGVList()[expert_index];
				Matrix fill_values = lst_of_matrices[expert_index];
				Matrix.SetToDataGridView(fill_values, DGV);
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
						DeactivateSymmetricCell(DGV, new DataGridViewCellEventArgs(i, j));
			}
			catch { }
		}
		/// <summary>
		/// обновить рисунки графов - матриц экспертов
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void UpdateExpertGraphs(object sender, DataGridViewCellEventArgs e)
		{
			var M = GetMatrixListFromDGVs();
			var L = new List<string>();
			for (int i = 0; i < M.Count; i++)
			{
				L.Add($"Expert{i}:");
			}
			OrgraphsPics_update(form3_input_expert_matrices, M, L);
		}
		public void UpdateExpertMatrices(object sender, ExpertMatricesEventArgs e)
		//ref List<Matrix> expert_matrices, int expert_index, Matrix fill_values)
		{
			ExpertMatricesInUI[e.expert_index] = e.fill_values;
			UpdateExpertDGV(ExpertMatricesInUI, e.expert_index);
		}
		public void UpdateExpertMatrices_wrapper(object sender, int expert_index, Matrix fill_values)
		{
			var args = new ExpertMatricesEventArgs();
			args.expert_index = expert_index;
			args.fill_values = fill_values;
			ExpertMatricesInUI_EventHandler?.Invoke(sender, args);
		}
		#endregion Обновление матриц экспертов (Model)


		public void R_UpdateGraphPicture()
		{
			var rtd = AggregatedMatrix.GetRelations2Draw();
			OrgraphsPics_update(form2_result_matrices, rtd.Matrices, rtd.Labels);
		}
		public void R_Set(List<FuzzyRelation> experts_relations)
		{
			AggregatedMatrix.Avg = Matrix.Average(FuzzyRelation.ToMatrixList(experts_relations)).ToFuzzy;
			AggregatedMatrix.Med = Matrix.Median(FuzzyRelation.ToMatrixList(experts_relations)).ToFuzzy;
			if (rb_dist_square.Checked)
				AggregatedMatrix.R = AggregatedMatrix.Avg;
			else if (rb_dist_modulus.Checked)
				AggregatedMatrix.R = AggregatedMatrix.Med;
			else
				throw new MyException(EX_choose_distance_func);
		}

		/// <summary>
		/// установка дефолтных значений переменных
		/// </summary>
		void refresh_variables()
		{
			R_list = new List<FuzzyRelation>();
			AggregatedMatrix.ClearAggregatedMatrices();
			Methods.ClearMethods();
		}


		/// <summary>
		/// начальное расцвечивание формы
		/// </summary>
		/// <param name="main_control"></param>
		void interface_coloring(Control main_control)
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
		void clear_output()
		{
			try
			{
				label3.Text = "";
				foreach (var m in Methods.GetMethods())
					m.VisualFormConnectedControls.ClearOutput();
			}
			catch (MyException ex) { }
		}
		void show_output()
		{
			try
			{
				foreach (var m in Methods.GetMethods())
					m.VisualFormConnectedControls.ShowOutput();
			}
			catch (MyException ex) { }
		}
		void clear_input()
		{
			try
			{
				flowLayoutPanel_input_tables.Controls.Clear();
			}
			catch (MyException ex) { }
		}
		void activate_input()
		{
			try
			{
				foreach (DataGridView dgv in flowLayoutPanel_input_tables.Controls)
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
		void deactivate_input()
		{
			try
			{
				foreach (DataGridView dgv in flowLayoutPanel_input_tables.Controls)
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
		void color_input_cell(DataGridView dgv, int row, int col, Color color)
		{
			try
			{
				dgv[col, row].Style.BackColor = color;
			}
			catch (MyException ex) { }
		}

		/// <summary>
		/// обновление размеров визуальных элементов после их изменения...
		/// </summary>
		private void set_controls_size()
		{
			System.Drawing.Size get_table_size(DataGridView dgv)
			{
				var Width = dgv.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + 2 * dgv.RowHeadersWidth;
				var Height = dgv.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + 2 * dgv.ColumnHeadersHeight;
				return new System.Drawing.Size(Width, Height);
			}

			foreach (DataGridView dgv in flowLayoutPanel_input_tables.Controls)
			{
				dgv.Dock = DockStyle.None;
				dgv.Size = get_table_size(dgv);
			}

			foreach (Method m in Methods.GetMethods())
			{
				DataGridView dgv = m?.VisualFormConnectedControls.ConnectedTableFrame;

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

					Label lab = m?.VisualFormConnectedControls.ConnectedLabel_control;
					if (lab != null)
					{
						lab.Location = new System.Drawing.Point(0, dgv.Location.Y + dgv.Height);
					}
				}
			}
		}

		/// <summary>
		/// запускает выполнение выбранных алгоритмов
		/// </summary>
		/// <param name="list_of_profiles"></param>
		private List<string> execute_algorythms(List<FuzzyRelation> ExpertsRelationsList)
		{
			List<string> Intersect = new List<string>();//общие ранжирования для использованных методов
			try
			{
				if (ExpertsRelationsList.Count == 0)
					throw new MyException(EX_bad_expert_profile);
				R_list = ExpertsRelationsList;
				R_Set(R_list);
				var checkbuttons = Methods.GetMethods().Select(x => x.IsExecute);
				if (checkbuttons.All(x => x == false))
					throw new MyException(EX_choose_method);

				if (Methods.All_various_rankings.IsExecute)
					Methods.Set_All_various_rankings(n);
				if (Methods.All_Hamiltonian_paths.IsExecute)
					Methods.Set_All_Hamiltonian_paths(AggregatedMatrix.R);
				if (Methods.Schulze_method.IsExecute)
					Methods.Set_Schulze_method(n, AggregatedMatrix.R);
				if (Methods.Smerchinskaya_Yashina_method.IsExecute)
					Methods.Set_Smerchinskaya_Yashina_method();

				var is_rankings_of_method_exist = new List<Method>();
				foreach (Method m in Methods.GetMethods())
				{
					if (m.IsExecute && (m.HasRankings))
						is_rankings_of_method_exist.Add(m);
				}
				//foreach (Method met in is_rankings_of_method_exist)
				//	met.SetCharacteristicsBestWorst();
				if (is_rankings_of_method_exist.Count() > 1)
				{
					bool enter_intersect = false;
					foreach (Method met in is_rankings_of_method_exist)
					{
						//met.SetCharacteristicsBestWorst();
						if (enter_intersect == false)
						{
							Intersect = met.Ranks2Strings;
							enter_intersect = true;
						}
						else
							Intersect = Enumerable.Intersect(Intersect, met.Ranks2Strings).ToList();
					}
				}
			}
			catch (MyException ex) { ex.Info(); }
			return Intersect;
		}

		/// <summary>
		/// размещение таблицы для ввода профилей
		/// </summary>
		private void set_input_datagrids()
		{
			if (numericUpDown_n.Minimum <= n && n <= numericUpDown_n.Maximum &&
				numericUpDown_m.Minimum <= m && m <= numericUpDown_m.Maximum)
			{
				numericUpDown_n.Value = n;
				numericUpDown_m.Value = m;
			}
			clear_input();
			clear_output();

			try
			{
				bool some_contradictory_profiles = false;
				for (int expert = 0; expert < m; expert++)
				{
					int comparsion_trials = 0;//сколько отредактированных ячеек уже было
					bool[] is_compared_alternative = new bool[n];//сравнима ли альтернатива

					void CheckCellWhenValueChanged(object sender, DataGridViewCellEventArgs e)
					{//что должно происходить при завершении редактирования ячейки
						if (!cb_show_input_matrices.Checked)
							return;
						try
						{
							FuzzyRelation changed_matrix;
							var dd = sender as DataGridView;
							///////
							//int exp_index = flowLayoutPanel_input_tables.Controls.GetChildIndex(dd);
							int exp_index = GetDGVList().IndexOf(dd);
							///////
							int i = e.RowIndex;
							int j = e.ColumnIndex;
							double Mij, Mji;
							var p = double.TryParse(dd[j, i]?.Value?.ToString(), out Mij);
							double.TryParse(dd[i, j]?.Value?.ToString(), out Mji);
							if (!p || Mij > 1 || Mij < 0 || i == j)
							{
								dd[j, i].Value = 0.0;
								changed_matrix = Matrix.GetFromDataGridView(dd).ToFuzzy;
							}
							else
							{
								comparsion_trials++;
								if (Mij == 0)
								{
									is_compared_alternative[i] = IsCompared(i, dd);
									is_compared_alternative[j] = IsCompared(j, dd);
								}
								else
								{
									is_compared_alternative[i] = true;
									is_compared_alternative[j] = true;
								}
								changed_matrix = Matrix.GetFromDataGridView(dd).ToFuzzy;
								//транзитивное замыкание не должно содержать циклов
								if (changed_matrix.IsHasCycle())
									throw new MyException(EX_contains_cycle);
								changed_matrix = PerformTransClosure(changed_matrix, out bool is_need_update);
							}
							UpdateExpertMatrices_wrapper(sender, exp_index, changed_matrix);
						}
						catch (MyException ex) { ex.Info(); }
					}
					FuzzyRelation PerformTransClosure(FuzzyRelation matrix, out bool is_need_update)
					{
						is_need_update = false;
						try
						{
							if (cb_do_transitive_closure.Checked &&
							(comparsion_trials == n - 1 || is_compared_alternative.All(x => x == true)))
							{
								if (!matrix.IsTransitive())
								{
									is_need_update = true;
									matrix = matrix.TransitiveClosure();

								}
							}
							return matrix;
						}
						catch (MyException ex)
						{
							ex.Info();
							return matrix;
						}
					}

					FuzzyRelation input_matrix = ExpertMatricesInUI[expert].ToFuzzy;
					input_matrix = input_matrix.NormalizeElems(out var is_norm).ToFuzzy;
					if (!is_norm)
					{
						new MyException(EX_matrix_was_normalized).Info();
					}
					if (input_matrix.IsHasCycle())
					{
						some_contradictory_profiles = true;
					}
					else
					{
						input_matrix = PerformTransClosure(input_matrix, out bool is_need_update);
						if (is_need_update)
						{
							UpdateExpertMatrices_wrapper(this, expert, input_matrix);
						}
					}


					if (cb_show_input_matrices.Checked)
					{
						double[,] fill_values = input_matrix.matrix_base;
						DataGridView dgv = new DataGridView();
						SetDataGridViewDefaults(dgv);
						dgv.CellEndEdit += CheckCellWhenValueChanged;
						dgv.CellEndEdit += DeactivateSymmetricCell;
						dgv.CellEndEdit += UpdateExpertGraphs;
						flowLayoutPanel_input_tables.Controls.Add(dgv);

						for (int j = 0; j < n; j++)
						{
							DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
							column.Name = j.ToString();
							column.HeaderText = $"{ind2letter[j]}";
							column.SortMode = DataGridViewColumnSortMode.NotSortable;
							dgv.Columns.Add(column);
						}
						for (int i = 0; i < n; i++)
						{
							dgv.Rows.Add();
							dgv.Rows[i].HeaderCell.Value = $"{ind2letter[i]}";
						}
						for (int i = 0; i < dgv.Rows.Count; i++)
						{
							for (int j = 0; j < dgv.Columns.Count; j++)
							{
								dgv[j, i].ValueType = typeof(double);
								dgv[j, i].Value = fill_values[i, j];
								DeactivateSymmetricCell(dgv, new DataGridViewCellEventArgs(j, i));
							}
						}
						for (int i = 0; i < n; i++)
						{
							is_compared_alternative[i] = IsCompared(i, dgv);
						}

					}

					UpdateExpertMatrices_wrapper(this, expert, input_matrix);
					UpdateExpertGraphs(null, null);

				}
				if (some_contradictory_profiles)
					throw new MyException(EX_contains_cycle);
			}
			catch (MyException ex) { ex.Info(); }
			activate_input();
			set_controls_size();
		}

		/// <summary>
		/// вывести на экран результирующие ранжирования
		/// </summary>
		/// <param name="Mutual_rankings"></param>
		private async void set_output_results(List<string> Mutual_rankings)
		{
			clear_output();
			deactivate_input();
			try
			{
				var tex = $"";
				string for_print_matrices(Matrix M)
				{
					return M?.Matrix2String(true);
					//+ "Матрица смежности:\n" + M?.AdjacencyMatrix.Matrix2String(true);
				}
				if (AggregatedMatrix.R != null)
				{
					tex += CR_LF + $"Агрегированное отношение R:{CR_LF}"
						+ for_print_matrices(AggregatedMatrix.R);

					tex += CR_LF + $"Асимметричная часть As(R) агрегированного отношения R:{CR_LF}"
						+ for_print_matrices(AggregatedMatrix.R.Asymmetric);

					tex += CR_LF + $"Транзитивное замыкание Tr(R) агрегированного отношения R:{CR_LF}"
						+ for_print_matrices(AggregatedMatrix.R.TransClosured);

					tex += CR_LF + $"Отношение с разбитыми циклами Acyc(R) агрегированного отношения R:{CR_LF}"
						+ for_print_matrices(AggregatedMatrix.R.DestroyedCycles);

					tex += CR_LF + $"Транзитивное замыкание Tr(Acyc(R)) отношения с разбитыми циклами Acyc(R) агрегированного отношения R:{CR_LF}"
						+ for_print_matrices(AggregatedMatrix.R.DestroyedCycles.TransClosured);
				}
				label3.Text = tex;
				void set_column(DataGridView dgv, int j)
				{
					DataGridViewColumn column = new DataGridViewColumn();
					column.CellTemplate = new DataGridViewTextBoxCell();
					column.HeaderText = $"Ранжиро-\nвание {j + 1}";
					column.Name = j.ToString();
					column.HeaderCell.Style.BackColor = window_bg_color;
					column.FillWeight = 1;
					dgv.Columns.Add(column);
				}
				void set_row(DataGridView dgv, int i)
				{
					dgv.Rows.Add();
					dgv.Rows[i].HeaderCell.Value = $"Место {i + 1}";
				}
				//создание чистого файла для вывода ранжирований в виде матриц
				using (StreamWriter writer = new StreamWriter(OUT_FILE, false))
				{
					await writer.WriteLineAsync("");
				}
				foreach (Method met in Methods.GetMethods())
				{
					if (met.IsExecute == true)
					{
						if (met.Rankings == null || met.Rankings.Count == 0)
						{
							met.VisualFormConnectedControls.ConnectedLabel = "Ранжирование невозможно. ";
							if (met.Levels != null && met.Levels.Count != 0)
							{//ранжирований нет, но можно задать разбиение на уровни
								int col = 0;
								set_column(met.VisualFormConnectedControls.ConnectedTableFrame, col);
								met.VisualFormConnectedControls.ConnectedTableFrame.Columns[col].HeaderText = $"Разбиение\nна уровни";
								for (int i = 0; i < met.Levels.Count; i++)
								{
									set_row(met.VisualFormConnectedControls.ConnectedTableFrame, i);
								}
								for (int i = 0; i < met.Levels.Count; i++)
								{
									met.VisualFormConnectedControls.ConnectedTableFrame[col, i].ReadOnly = true;
									met.VisualFormConnectedControls.ConnectedTableFrame[col, i].Value =
										string.Join(",", met.Levels[i].Select(x => ind2letter[x]).ToArray());
								}
							}
						}
						else if (met.Rankings.Count > 0)
						{
							//запись в файл всех полученных ранжирований метода
							using (StreamWriter writer = new StreamWriter(OUT_FILE, true))
							{//если есть ранжирование и оно действительно включает все альтернативы
								await writer.WriteLineAsync(CR_LF);
								var text = string.Join(CR_LF + CR_LF,
									met.Rankings
									.Where(x => x.Count == n)
									.Select(x => x.Rank2Matrix.Matrix2String(false)).ToArray());
								await writer.WriteLineAsync(text);
							}

							var r = met.Rankings.Count;
							for (int j = 0; j < r; j++)
							{
								set_column(met.VisualFormConnectedControls.ConnectedTableFrame, j);
							}
							for (int i = 0; i < n; i++)
							{
								set_row(met.VisualFormConnectedControls.ConnectedTableFrame, i);
							}

							//добавить в конец datagrid-а строку с характеристикой ранжирования
							int add_row_with_characteristic(string label)
							{
								met.VisualFormConnectedControls.ConnectedTableFrame.Rows.Add();
								int i = met.VisualFormConnectedControls.ConnectedTableFrame.Rows.Count - 1;
								met.VisualFormConnectedControls.ConnectedTableFrame.Rows[i].HeaderCell.Value = label;
								return i;
							}
							//задать значение характеристики ранжирования и раскрасить
							void display_characteristic(int j, int i, double min, double max,
								Characteristic characteristic)
							{
								met.VisualFormConnectedControls.ConnectedTableFrame[j, i].Value = characteristic.Value;
								met.VisualFormConnectedControls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_bg_color;
								if (min < max)
								{
									if (characteristic.Value == min)
										met.VisualFormConnectedControls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_min_color;
									else if (characteristic.Value == max)
										met.VisualFormConnectedControls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_max_color;
								}
								else if (characteristic.ValuesList != null && characteristic.ValuesList.Count != 0)
								{
									met.VisualFormConnectedControls.ConnectedTableFrame[j, i].Value = string.Join(CR_LF,
										characteristic.ValuesList);
									if (met.RanksCharacteristics.IsInPareto[j])
										met.VisualFormConnectedControls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_max_color;
								}
							}

							var some_rank = met.Rankings.First();
							add_row_with_characteristic(some_rank.Cost.Label);
							add_row_with_characteristic(some_rank.Strength.Label);
							add_row_with_characteristic(some_rank.SummaryDistance.modulus.Label);
							add_row_with_characteristic(some_rank.SummaryDistance.square.Label);
							add_row_with_characteristic(some_rank.CostsExperts.Label);

							for (int j = 0; j < r; j++)
							{
								for (int i = 0; i < met.Rankings[j].Count; i++)
								{
									met.VisualFormConnectedControls.ConnectedTableFrame[j, i].ReadOnly = true;
									met.VisualFormConnectedControls.ConnectedTableFrame[j, i].Value = ind2letter[met.Rankings[j].Rank2List[i]];
								}
								if (Mutual_rankings.Count != 0 && Mutual_rankings.Contains(met.Rankings[j].Rank2String))
								{
									for (int i = 0; i < n; i++)
										met.VisualFormConnectedControls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_mutual_color;
								}

								display_characteristic(j, n, 
									met.RanksCharacteristics.Cost.ValueMin, 
									met.RanksCharacteristics.Cost.ValueMax,
									met.Rankings[j].Cost);
								display_characteristic(j, n + 1, 
									met.RanksCharacteristics.Strength.ValueMin, 
									met.RanksCharacteristics.Strength.ValueMax,
									met.Rankings[j].Strength);
								display_characteristic(j, n + 2,
									met.RanksCharacteristics.MinDistance.modulus.Value, 
									met.RanksCharacteristics.MaxDistance.modulus.Value,
									met.Rankings[j].SummaryDistance.modulus);
								display_characteristic(j, n + 3,
									met.RanksCharacteristics.MinDistance.square.Value, 
									met.RanksCharacteristics.MaxDistance.square.Value,
									met.Rankings[j].SummaryDistance.square);
								display_characteristic(j, n + 4,
									INF, 
									INF,
									met.Rankings[j].CostsExperts);
							}
						}
					}

					// вывести на экран победителей
					if (met.Winners != null && met.Winners.Count > 0)
					{
						string text = met.VisualFormConnectedControls.ConnectedLabel;
						text += $"Недоминируемые альтернативы: {string.Join(",", met.Winners.Select(x => ind2letter[x]))}";
						met.VisualFormConnectedControls.ConnectedLabel = text;
					}
				}
			}
			catch (MyException ex) { ex.Info(); }
			show_output();
			set_controls_size();
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
					List<Matrix> matrices = new List<Matrix>();
					FindFile(textBox_file.Text, out string absolute_file_name);

					string[] lines = File.ReadAllLines(absolute_file_name)
						.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();//ReadAllLines вызывает FileNotFoundException
					textBox_file.Text = absolute_file_name;
					var chars_for_split = new char[] { ' ', '	' };
					var nn = lines.First().Split(chars_for_split, StringSplitOptions.RemoveEmptyEntries).Count();
					Matrix cur_matrix = new Matrix(nn, nn);
					for (int l = 0; l < lines.Length; l++)
					{
						if (lines[l].Length != 0)
						{
							double res;
							double[] numbers = lines[l].Split(chars_for_split, StringSplitOptions.RemoveEmptyEntries)
								.Select(x => double.TryParse(x, out res) ? res : INF).ToArray();
							if (numbers.Any(x => x == INF) || numbers.Length != nn)
								throw new MyException(EX_bad_file);
							for (int j = 0; j < numbers.Length; j++)
								cur_matrix[l % nn, j] = numbers[j];
						}
						if (l % nn == nn - 1)
							matrices.Add(new Matrix(cur_matrix));
					}
					if (matrices.Count == 0)
						throw new MyException(EX_bad_file);
					m = matrices.Count;
					n = nn;

					refresh_variables();
					ExpertMatricesInUI = matrices;
					set_input_datagrids();
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
				R_list = new List<FuzzyRelation>();
				foreach (DataGridView dgv in flowLayoutPanel_input_tables.Controls)
				{
					var input_matrix = Matrix.GetFromDataGridView(dgv).ToFuzzy;
					R_list.Add(input_matrix);
				}
				if (cb_do_transitive_closure.Checked)
				{
					R_list = R_list.Select(x => x.ToFuzzy.TransitiveClosure()).ToList();
				}
				//выполняет проверку и выводит уведомления о наличии циклов
				ExpertMatricesInUI = FuzzyRelation.ToMatrixList(R_list);
				set_input_datagrids();
				var Intersect = execute_algorythms(R_list);
				set_output_results(Intersect);
				// visualize_graph(C, null);//
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
				if (n == (int)numericUpDown_n.Value && m == (int)numericUpDown_m.Value)
				{
					clear_output();
					activate_input();
				}
				else
				{
					if (cb_All_rankings.Checked && (
						(int)numericUpDown_n.Value > max_count_of_alternatives ||
						(int)numericUpDown_m.Value > max_count_of_experts
						))
						throw new MyException(EX_n_m_too_big);
					n = (int)numericUpDown_n.Value;
					m = (int)numericUpDown_m.Value;

					refresh_variables();
					ExpertMatricesInUI = new List<Matrix>(m);
					for(int k =0; k < m; k++)
					{
						ExpertMatricesInUI.Add(new Matrix(n));
					}
					set_input_datagrids();
				}
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
			refresh_variables();//?
			var matrices = new List<FuzzyRelation>();

			label3.Text = Directory.GetCurrentDirectory().ToString();
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
						matrices.Add(cur_matrix.ToFuzzy);
				}
				m = matrices.Count;
				//R_list = Enumerable.Select(R_list, x => Matrix.GetAsymmetricPart(x)).ToList();
				var Intersect = execute_algorythms(matrices);
				set_output_results(Intersect);//вывод в файл или начисление статистики

				//вывод статистики в файл
			}
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
