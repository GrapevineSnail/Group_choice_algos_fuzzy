using System;
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
			//связывание модели с control-ами на форме

			ExpertRelations.UI_Controls = new ExpertRelations.ConnectedControls(
				cb_show_input_matrices, cb_do_transitive_closure, 
				numericUpDown_n, numericUpDown_m, flowLayoutPanel_input_tables);

			ExpertRelations_EventHandler += ExpertRelations.UpdateExpertMatrix;


			Methods.All_Hamiltonian_paths.UI_Controls =
				new Method.ConnectedControls(Methods.All_Hamiltonian_paths, cb_HP, dg_HP);
			Methods.Schulze_method.UI_Controls =
				new Method.ConnectedControls(Methods.Schulze_method, cb_Schulze_method, dg_Schulze_method);
			Methods.All_various_rankings.UI_Controls =
				new Method.ConnectedControls(Methods.All_various_rankings, cb_All_rankings, dg_All_rankings);
			Methods.Smerchinskaya_Yashina_method.UI_Controls =
				new Method.ConnectedControls(Methods.Smerchinskaya_Yashina_method, cb_SY, dg_SY);

			AggregatedMatrix.UI_Controls = new AggregatedMatrix.ConnectedControls(rb_dist_square, rb_dist_modulus, label_aggreg_matrix);
			AggregatedMatrix.R_Changed += R_UpdateGraphPicture;

			VIF = new VisualInterfaceFuncs(this, form2_result_matrices, form3_input_expert_matrices);
			Model.form1 = this;

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

			VIF.interface_coloring(this);
			Methods.Clear();
		}
		public static Form2 form2_result_matrices = null;
		public static Form3 form3_input_expert_matrices = null;
		public VisualInterfaceFuncs VIF;

		public void R_UpdateGraphPicture()
		{
			var rtd = AggregatedMatrix.GetRelations2Draw();
			OrgraphsPics_update(form2_result_matrices, rtd.Matrices, rtd.Labels);
		}
		public void R_Set(List<FuzzyRelation> experts_relations)
		{
			AggregatedMatrix.Avg = Matrix.Average(FuzzyRelation.ToMatrixList(experts_relations)).Cast2Fuzzy;
			AggregatedMatrix.Med = Matrix.Median(FuzzyRelation.ToMatrixList(experts_relations)).Cast2Fuzzy;
			if (rb_dist_square.Checked)
				AggregatedMatrix.R = AggregatedMatrix.Avg;
			else if (rb_dist_modulus.Checked)
				AggregatedMatrix.R = AggregatedMatrix.Med;
			else
				throw new MyException(EX_choose_distance_func);
		}

		void RefreshModel()
		{
			ExpertRelations.Clear();
			AggregatedMatrix.Clear();
			Methods.Clear();
		}

		/// <summary>
		/// Считывает с формы переменные n и m (количество альтернатив и экспертов)
		/// </summary>
		/// 
		private void button_n_m_Click(object sender, EventArgs e)
		{
			try
			{
				if (numericUpDown_n.Value == n && numericUpDown_m.Value == m)
				{
					ExpertRelations.UI_Controls.UI_Activate();
					return;
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

					RefreshModel();
					ExpertRelations.RListMatrix = new List<Matrix>(m);
					for (int k = 0; k < m; k++)
					{
						ExpertRelations.RListMatrix.Add(new Matrix(n));
					}
					ExpertRelations.UI_Controls.UI_Show();
					ExpertRelations.UI_Controls.UI_Activate();
				}
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

					RefreshModel();
					ExpertRelations.RListMatrix = matrices;
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
					ExpertRelations.RListFuzzyRel = ExpertRelations.RListFuzzyRel.Select(x => x.TransitiveClosure()).ToList();
				}
				//выполняет проверку и выводит уведомления о наличии циклов
				ExpertRelations.UI_Controls.UI_Show();
				var Intersect = Methods.ExecuteAlgorythms();
				set_output_results(Intersect);
			}
			catch (MyException ex) { ex.Info(); }
		}

		/// <summary>
		/// вывести на экран результирующие ранжирования
		/// </summary>
		/// <param name="Mutual_rankings"></param>
		private async void set_output_results(List<string> Mutual_rankings)
		{
			Methods.Clear();
			deactivate_dgvs(ExpertRelations.UI_Controls.ConnectedTables);
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
				label_aggreg_matrix.Text = tex;
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
							met.UI_Controls.ConnectedLabelText = "Ранжирование невозможно. ";
							if (met.Levels != null && met.Levels.Count != 0)
							{//ранжирований нет, но можно задать разбиение на уровни
								int col = 0;
								set_column(met.UI_Controls.ConnectedTableFrame, col);
								met.UI_Controls.ConnectedTableFrame.Columns[col].HeaderText = $"Разбиение\nна уровни";
								for (int i = 0; i < met.Levels.Count; i++)
								{
									set_row(met.UI_Controls.ConnectedTableFrame, i);
								}
								for (int i = 0; i < met.Levels.Count; i++)
								{
									met.UI_Controls.ConnectedTableFrame[col, i].ReadOnly = true;
									met.UI_Controls.ConnectedTableFrame[col, i].Value =
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
								set_column(met.UI_Controls.ConnectedTableFrame, j);
							}
							for (int i = 0; i < n; i++)
							{
								set_row(met.UI_Controls.ConnectedTableFrame, i);
							}

							//добавить в конец datagrid-а строку с характеристикой ранжирования
							int add_row_with_characteristic(string label)
							{
								met.UI_Controls.ConnectedTableFrame.Rows.Add();
								int i = met.UI_Controls.ConnectedTableFrame.Rows.Count - 1;
								met.UI_Controls.ConnectedTableFrame.Rows[i].HeaderCell.Value = label;
								return i;
							}
							//задать значение характеристики ранжирования и раскрасить
							void display_characteristic(int j, int i, double min, double max,
								Characteristic characteristic)
							{
								met.UI_Controls.ConnectedTableFrame[j, i].Value = characteristic.Value;
								met.UI_Controls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_bg_color;
								if (min < max)
								{
									if (characteristic.Value == min)
										met.UI_Controls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_min_color;
									else if (characteristic.Value == max)
										met.UI_Controls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_max_color;
								}
								else if (characteristic.ValuesList != null && characteristic.ValuesList.Count != 0)
								{
									met.UI_Controls.ConnectedTableFrame[j, i].Value = string.Join(CR_LF,
										characteristic.ValuesList);
									if (met.RanksCharacteristics.IsInPareto[j])
										met.UI_Controls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_max_color;
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
									met.UI_Controls.ConnectedTableFrame[j, i].ReadOnly = true;
									met.UI_Controls.ConnectedTableFrame[j, i].Value = ind2letter[met.Rankings[j].Rank2List[i]];
								}
								if (Mutual_rankings.Count != 0 && Mutual_rankings.Contains(met.Rankings[j].Rank2String))
								{
									for (int i = 0; i < n; i++)
										met.UI_Controls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_mutual_color;
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
						string text = met.UI_Controls.ConnectedLabelText;
						text += $"Недоминируемые альтернативы: {string.Join(",", met.Winners.Select(x => ind2letter[x]))}";
						met.UI_Controls.ConnectedLabelText = text;
					}
				}
			}
			catch (MyException ex) { ex.Info(); }
			Methods.UI_Show();
			set_controls_size();
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
