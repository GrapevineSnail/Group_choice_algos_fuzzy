using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Algorithms;
using static System.IO.DirectoryInfo;


namespace Group_choice_algos_fuzzy
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			//связывание методов с control-ами на форме
			Methods.Hp_max_length.SetConnectedControls(cb_HP_max_length, dg_HP_max_length);
			Methods.Hp_max_strength.SetConnectedControls(cb_HP_max_strength, dg_HP_max_strength);
			Methods.Schulze_method.SetConnectedControls(cb_Schulze_method, dg_Schulze_method);
			Methods.All_various_rankings.SetConnectedControls(cb_All_rankings, dg_All_rankings);
			Methods.Smerchinskaya_Yashina_method.SetConnectedControls(cb_SY, dg_SY);

			button_read_file.Height = textBox_file.Height + 2;
			button_n_m.Height = textBox_file.Height + 2;

			foreach (Control c in flowLayoutPanel_input_tables.Controls)
				c.MouseEnter += flowLayoutPanel_input_MouseEnter;
			foreach (Control c in flowLayoutPanel_output_info.Controls)
				c.MouseEnter += flowLayoutPanel_output_info_MouseEnter;
			foreach (Control c in flowLayoutPanel_output_tables.Controls)
			{
				c.MouseEnter += flowLayoutPanel_output_tables_MouseEnter;
				foreach (Control c1 in c.Controls)
					c1.MouseEnter += flowLayoutPanel_output_tables_MouseEnter;
			}
			interface_coloring(this);
			clear_output();
		}

		#region GLOBALS
		private static int _n;//количество альтернатив
		private static int _m;//количество экспертов
		public static int n
		{
			set
			{
				_n = value;
				SetConstants(n);
			}
			get { return _n; }
		}
		public static int m
		{
			set
			{
				_m = value;
			}
			get { return _m; }
		}
		public static List<FuzzyRelation> R_list; //список матриц нечётких предпочтений экспертов
		public struct R //агрегированная матрица матриц профилей
		{
			public static FuzzyRelation aggregated;
			public static FuzzyRelation avg;//агрегированная матрица матриц профилей (среднее)
			public static FuzzyRelation med;//агрегированная матрица матриц профилей (медианные)
		}
		#endregion GLOBALS


		/// <summary>
		/// установка дефолтных значений переменных
		/// </summary>
		void refresh_variables()
		{
			R_list = new List<FuzzyRelation>();
			R.aggregated = new FuzzyRelation(n);
			R.avg = new FuzzyRelation(n);
			R.med = new FuzzyRelation(n);
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
						b.BackColor = button_background;
						b.FlatAppearance.BorderColor = button_background;
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
					m.ClearMethodOutput();
			}
			catch (MyException ex) { }
		}

		void show_output()
		{
			try
			{
				foreach (var m in Methods.GetMethods())
					m.ShowMethodOutput();
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
					dgv.ReadOnly = false;
					foreach (DataGridViewColumn column in dgv.Columns)
						column.DefaultCellStyle.BackColor = input_bg_color;
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
					dgv.ReadOnly = true;
					foreach (DataGridViewColumn column in dgv.Columns)
						column.DefaultCellStyle.BackColor = disabled_input_bg_color;
				}
			}
			catch (MyException ex) { }
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
				R.avg = Matrix.Average(FuzzyRelation.ToMatrixList(R_list)).ToFuzzy;
				R.med = Matrix.Median(FuzzyRelation.ToMatrixList(R_list)).ToFuzzy;
				if (rb_dist_square.Checked)
					R.aggregated = R.avg;
				else if (rb_dist_modulus.Checked)
					R.aggregated = R.med;
				else
					throw new MyException(EX_choose_distance_func);

				var checkbuttons = Methods.GetMethods().Select(x => x.IsExecute);
				if (checkbuttons.All(x => x == false))
					throw new MyException(EX_choose_method);

				if (Methods.All_various_rankings.IsExecute)
					Methods.Set_All_various_rankings(n);
				if (Methods.All_Hamiltonian_paths.IsExecute)
					Methods.Set_All_Hamiltonian_paths(R.aggregated);
				if (Methods.Hp_max_length.IsExecute)
					Methods.Set_Hp_max_length(R.aggregated);
				if (Methods.Hp_max_strength.IsExecute)
					Methods.Set_Hp_max_strength(R.aggregated);
				if (Methods.Schulze_method.IsExecute)
					Methods.Set_Schulze_method(n, R.aggregated);
				if (Methods.Smerchinskaya_Yashina_method.IsExecute)
					Methods.Set_Smerchinskaya_Yashina_method(R.aggregated);

				var is_rankings_of_method_exist = Methods.GetMethodsExecutedWhithResult();
				foreach (Method met in is_rankings_of_method_exist)
					met.SetCharacteristicsBestWorst();
				if (is_rankings_of_method_exist.Count() > 1)
				{
					bool enter_intersect = false;
					foreach (Method met in is_rankings_of_method_exist)
					{
						met.SetCharacteristicsBestWorst();
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
		private void set_input_datagrids(List<Matrix> list_of_matrices)
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
					DataGridView dgv = new DataGridView();
					dgv.AllowUserToAddRows = false;
					dgv.AllowUserToDeleteRows = false;
					dgv.AllowUserToResizeRows = false;
					dgv.AllowUserToResizeColumns = false;
					dgv.AllowUserToOrderColumns = false;
					dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
					dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
					dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
					dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
					dgv.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
					dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
					dgv.ShowEditingIcon = false;
					dgv.DefaultCellStyle.Format = $"0.{new string('#', digits_precision)}";
					dgv.DataError += (object ss, DataGridViewDataErrorEventArgs anError) => { dgv.CancelEdit(); };
					dgv.CellEndEdit += (object d, DataGridViewCellEventArgs ee) =>
					{//что должно происходить при завершении редактирования ячейки
						try
						{
							var dd = d as DataGridView;
							var cell = dd.CurrentCell;
							var i = cell.RowIndex;
							var j = cell.ColumnIndex;
							double res;
							if (!double.TryParse(cell.Value.ToString(), out res) || res > 1 || res <= 0 || i == j)
								cell.Value = 0.0;
							else
							{// эксперт вводит асимметричное отношение
							 //dd[i, j].Value = 0.0;
								if (!SetAsymmetricClosuredProfile(dd))
								{
									cell.Value = 0.0;
									throw new MyException(EX_not_transitive_profile);
								}
							}
						}
						catch (MyException ex) { ex.Info(); }
					};
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

					double[,] fill_values = new double[n, n];//инициализирован 0.0
					if (list_of_matrices != null && list_of_matrices.Count != 0)
					{
						for (int i = 0; i < list_of_matrices[expert].n; i++)
							for (int j = 0; j < list_of_matrices[expert].m; j++)
								fill_values[i, j] = list_of_matrices[expert][i, j];
					}
					for (int i = 0; i < dgv.Rows.Count; i++)
						for (int j = 0; j < dgv.Columns.Count; j++)
						{
							dgv[j, i].ReadOnly = false;
							dgv[j, i].Value = fill_values[i, j];
							dgv[j, i].ValueType = typeof(double);
						}
					if (!SetAsymmetricClosuredProfile(dgv))
						some_contradictory_profiles = true;
				}
				if (some_contradictory_profiles)
					throw new MyException(EX_not_transitive_profile);
			}
			catch (MyException ex) { ex.Info(); }
			activate_input();

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
				//создание чистого файла для вывода ранжирований в виде матриц
				using (StreamWriter writer = new StreamWriter(out_file, false))
				{
					await writer.WriteLineAsync("");
				}
				var tex = $"Минимальное суммарное расстояние среди всевозможных ранжирований:\n" +
					$"'модуль разности': {Methods.MinSummaryModulusDistance}\n" +
					$"'квадрат разности': {Methods.MinSummarySquareDistance}\n";
				tex += CR_LF + "Агрегированное отношение R: \n"
					+ R.aggregated?.Matrix2String(true);
				tex += CR_LF + "Матрица смежности агрегированного отношения: \n"
					+ R.aggregated?.AdjacencyMatrix.Matrix2String(true);
				tex += CR_LF + "Асимметричная часть As R агрегированного отношения R: \n"
					+ R.aggregated?.AsymmetricPart.Matrix2String(true);
				tex += CR_LF + "Матрица смежности агрегированного отношения: \n"
					+ R.aggregated?.AsymmetricPart.AdjacencyMatrix.Matrix2String(true);
				label3.Text = tex;
				foreach (Method met in Methods.GetMethods())
				{
					if (met.IsExecute == true)
					{
						if (met.Rankings == null || met.Rankings.Count == 0)
							met.ConnectedLabel = "Ранжирование невозможно. ";
						else if (met.Rankings.Count > 0)
						{
							//запись в файл всех полученных ранжирований метода
							using (StreamWriter writer = new StreamWriter(out_file, true))
							{
								var text = string.Join(CR_LF + CR_LF + CR_LF,
									met.Rankings.Select(x => x.Rank2Matrix.Matrix2String(false)).ToArray());
								await writer.WriteLineAsync(text);
							}

							var r = met.Rankings.Count;
							for (int j = 0; j < r; j++)
							{
								DataGridViewColumn column = new DataGridViewColumn();
								column.CellTemplate = new DataGridViewTextBoxCell();
								column.HeaderText = $"Ранжиро-\nвание {j + 1}";
								column.Name = j.ToString();
								column.HeaderCell.Style.BackColor = window_background;
								column.FillWeight = 1;
								met.connectedTableFrame.Columns.Add(column);
							}
							for (int i = 0; i < n; i++)
							{
								met.connectedTableFrame.Rows.Add();
								met.connectedTableFrame.Rows[i].HeaderCell.Value = $"Место {i + 1}";
							}

							//добавить в конец datagrid-а строку с характеристикой ранжирования
							int add_row_with_characteristic(string label)
							{
								met.connectedTableFrame.Rows.Add();
								int i = met.connectedTableFrame.Rows.Count - 1;
								met.connectedTableFrame.Rows[i].HeaderCell.Value = label;
								return i;
							}
							//задать значение характеристики ранжирования и раскрасить
							void display_characteristic(int j, int i, double min, double max,
								Ranking.Characteristic characteristic)
							{
								met.connectedTableFrame[j, i].Value = characteristic.Value;
								met.connectedTableFrame[j, i].Style.BackColor = output_characteristics_bg_color;
								if (min < max)
								{
									if (characteristic.Value == min)
										met.connectedTableFrame[j, i].Style.BackColor = color_min;
									else if (characteristic.Value == max)
										met.connectedTableFrame[j, i].Style.BackColor = color_max;
								}
								else if (characteristic.ValuesList != null && characteristic.ValuesList.Count != 0)
								{
									met.connectedTableFrame[j, i].Value = string.Join(CR_LF,
										characteristic.ValuesList);
									if (met.IsInPareto[j])
										met.connectedTableFrame[j, i].Style.BackColor = color_max;
								}
							}

							var some_rank = met.Rankings.First();
							add_row_with_characteristic(some_rank.PathCost.Label);
							add_row_with_characteristic(some_rank.PathStrength.Label);
							add_row_with_characteristic(some_rank.PathSummaryDistance.modulus.Label);
							add_row_with_characteristic(some_rank.PathSummaryDistance.square.Label);
							add_row_with_characteristic(some_rank.PathExpertCosts.Label);

							for (int j = 0; j < r; j++)
							{
								for (int i = 0; i < n; i++)
								{
									met.connectedTableFrame[j, i].ReadOnly = true;
									met.connectedTableFrame[j, i].Value = ind2letter[met.Rankings[j].Rank2List[i]];
								}
								if (Mutual_rankings.Count != 0 && Mutual_rankings.Contains(met.Rankings[j].Rank2String))
								{
									for (int i = 0; i < n; i++)
										met.connectedTableFrame[j, i].Style.BackColor = color_mutual;
								}

								display_characteristic(j, n, met.MinLength, met.MaxLength,
									met.Rankings[j].PathCost);
								display_characteristic(j, n + 1, met.MinStrength, met.MaxStrength,
									met.Rankings[j].PathStrength);
								display_characteristic(j, n + 2,
									met.MinDistance.modulus.Value, met.MaxDistance.modulus.Value,
									met.Rankings[j].PathSummaryDistance.modulus);
								display_characteristic(j, n + 3,
									met.MinDistance.square.Value, met.MaxDistance.square.Value,
									met.Rankings[j].PathSummaryDistance.square);
								display_characteristic(j, n + 4,
									INF, INF,
									met.Rankings[j].PathExpertCosts);
							}
						}
					}

					// вывести на экран победителей
					if (met.Winners != null && met.Winners.Count > 0)
					{
						string text = met.ConnectedLabel;
						text += $"Победители: {string.Join(",", met.Winners.Select(x => ind2letter[x]))}";
						met.ConnectedLabel = text;
					}
				}
				show_output();
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
					string projectDirectory = new DirectoryInfo(
						AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
					string file_name = Path.GetFileName(textBox_file.Text);
					string[] allFoundFiles = Directory.GetFiles(
						projectDirectory, file_name, SearchOption.AllDirectories);
					textBox_file.Text = allFoundFiles[0];
					string[] lines = File.ReadAllLines(textBox_file.Text)
						.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
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
					set_input_datagrids(matrices);
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
				if (n > max_count_of_alternatives)
					throw new MyException(EX_n_m_too_big);
				R_list = new List<FuzzyRelation>() { };
				foreach (DataGridView dgv in flowLayoutPanel_input_tables.Controls)
				{
					var input_matrix = new FuzzyRelation(n);
					for (int i = 0; i < dgv.Rows.Count; i++)
						for (int j = 0; j < dgv.Columns.Count; j++)
							input_matrix[i, j] = (double)dgv[j, i].Value;
					R_list.Add(input_matrix);
				}
				if (R_list.Any(x => x.IsHasCycle(out _)))
					throw new MyException(EX_not_transitive_profile);
				R_list = R_list.Select(x => x.ToFuzzy.TransitiveClosure()).ToList();
				set_input_datagrids(FuzzyRelation.ToMatrixList(R_list));
				var Intersect = execute_algorythms(R_list);
				set_output_results(Intersect);
				// visualize_graph(C, null);//
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
				if (n == (int)numericUpDown_n.Value && m == (int)numericUpDown_m.Value)
				{
					clear_output();
					activate_input();
				}
				else
				{
					if ((int)numericUpDown_n.Value > max_count_of_alternatives ||
						(int)numericUpDown_m.Value > max_count_of_experts)
						throw new MyException(EX_n_m_too_big);
					n = (int)numericUpDown_n.Value;
					m = (int)numericUpDown_m.Value;

					refresh_variables();
					set_input_datagrids(null);
					set_controls_size();
				}
			}
			catch (MyException ex) { ex.Info(); }
		}

		private void set_controls_size()
		{
			Size get_table_size(DataGridView dgv)
			{
				var Width = dgv.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + 2 * dgv.RowHeadersWidth;
				var Height = dgv.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + 2 * dgv.ColumnHeadersHeight;
				return new Size(Width, Height);
			}

			foreach (DataGridView dgv in flowLayoutPanel_input_tables.Controls)
			{
				dgv.Dock = DockStyle.None;
				dgv.Size = get_table_size(dgv);
			}

			foreach (Method m in Methods.GetMethods())
			{
				DataGridView dgv = m?.connectedTableFrame;

				if (dgv != null)
				{
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

					Label lab = m?.connectedLabel;
					if (lab != null)
					{
						lab.Location = new Point(0, dgv.Location.Y + dgv.Height);
					}
				}
			}
		}

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
		private void flowLayoutPanel_input_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_input_tables.Focus();
		}
		private void flowLayoutPanel_output_tables_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_output_tables.Focus();
		}
		private void flowLayoutPanel_output_info_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_output_info.Focus();
		}




	}
}
