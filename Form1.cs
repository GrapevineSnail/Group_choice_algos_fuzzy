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
using static Group_choice_algos_fuzzy.Fuzzy;


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

			button_read_file.Height = textBox_file.Height + 2;
			button_n_m.Height = textBox_file.Height + 2;
			foreach (Control c in flowLayoutPanel_input.Controls)
			{
				c.MouseEnter += flowLayoutPanel_input_MouseEnter;
			}
			foreach (Control c in flowLayoutPanel_output_tables.Controls)
			{
				c.MouseEnter += flowLayoutPanel_output_MouseEnter;
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
		public static FuzzyRelation R;//агрегированная матрица матриц профилей
		public static FuzzyRelation R_avg;//агрегированная матрица матриц профилей (среднее)
		public static FuzzyRelation R_med;//агрегированная матрица матриц профилей (медианные)
		
		public static Matrix r;//общая матрица смежности
		public static Matrix r_avg;
		public static Matrix r_med;
		#endregion GLOBALS


		/// <summary>
		/// установка дефолтных значений переменных
		/// </summary>
		void refresh_variables()
		{
			R_list = new List<FuzzyRelation>();
			R = new FuzzyRelation { };
			R_avg = new FuzzyRelation { };
			R_med = new FuzzyRelation { };
			r = new Matrix { };
			r_avg = new Matrix { };
			r_med = new Matrix { };
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
				label3.Visible = false;
				foreach (var m in Methods.GetMethods())
					m.ClearMethodOutput();
			}
			catch (MyException ex) { }
		}

		void show_output()
		{
			try
			{
				label3.Visible = true;
				foreach (var m in Methods.GetMethods())
					m.ShowMethodOutput();
				flowLayoutPanel_output_tables.Focus();
			}
			catch (MyException ex) { }
		}

		void clear_input()
		{
			try
			{
				flowLayoutPanel_input.Controls.Clear();
			}
			catch (MyException ex) { }
		}

		void activate_input()
		{
			try
			{
				foreach (DataGridView dgv in flowLayoutPanel_input.Controls)
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
				foreach (DataGridView dgv in flowLayoutPanel_input.Controls)
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
				refresh_variables();
				var checkbuttons = Methods.GetMethods().Select(x => x.IsExecute);
				var frames = Methods.GetMethods().Select(x => x.connectedFrame);
				if (checkbuttons.All(x => x == false))
					throw new MyException(EX_choose_method);

				if (ExpertsRelationsList.Count == 0)
					throw new MyException(EX_bad_expert_profile);

				R_list = ExpertsRelationsList;
				R_avg = Matrix.Average(FuzzyRelation.ToMatrixList(R_list)).ToFuzzy();
				r_avg = Matrix.MakeAdjacencyMatrix(R_avg);
				R_med = Matrix.Median(FuzzyRelation.ToMatrixList(R_list)).ToFuzzy();
				r_med = Matrix.MakeAdjacencyMatrix(R_med);

				if (rb_dist_square.Checked)
				{
					R = R_avg;
					r = r_avg;
				}
				else if (rb_dist_modulus.Checked)
				{
					R = R_med;
					r = r_med;
				}
				else
					throw new MyException(EX_choose_distance_func);

				if (Methods.All_various_rankings.Rankings.Count == 0)
					Methods.Set_All_various_rankings(n);
				Methods.MinSummaryModulusDistance = Methods.All_various_rankings.Rankings
					.Select(x => x.PathSummaryDistance_modulus.Value).Min();
				Methods.MinSummarySquareDistance = Methods.All_various_rankings.Rankings
					.Select(x => x.PathSummaryDistance_square.Value).Min();

				if (Methods.All_various_rankings.IsExecute && Methods.All_various_rankings.Rankings.Count == 0)
					Methods.Set_All_various_rankings(n);
				if (Methods.All_Hamiltonian_paths.IsExecute && Methods.All_Hamiltonian_paths.Rankings.Count == 0)
					Methods.Set_All_Hamiltonian_paths(R);
				if (Methods.Hp_max_length.IsExecute)
					Methods.Set_Hp_max_length(R);
				if (Methods.Hp_max_strength.IsExecute)
					Methods.Set_Hp_max_strength(R);
				if (Methods.Schulze_method.IsExecute)
					Methods.Set_Schulze_method(n, R);

				var is_rankings_of_method_exist = Methods.GetMethodsExecutedWhithResult();
				foreach (Method met in is_rankings_of_method_exist)
					met.SetCharacteristicsMinsMaxes();
				if (is_rankings_of_method_exist.Count() > 1)
				{
					bool enter_intersect = false;
					foreach (Method met in is_rankings_of_method_exist)
					{
						met.SetCharacteristicsMinsMaxes();
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
			catch (MyException ex)
			{
				MessageBox.Show($"{ex.Message}");
			}
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
				dgv.DataError += (object ss, DataGridViewDataErrorEventArgs anError) => { dgv.CancelEdit(); };
				dgv.CellEndEdit += (object d, DataGridViewCellEventArgs ee) =>
				{
					var dd = d as DataGridView;
					var cell = dd.CurrentCell;
					double res;
					if (!double.TryParse(cell.Value.ToString(), out res) || res > 1 || res < 0
					|| cell.RowIndex == cell.ColumnIndex)
						cell.Value = 0.0;
					else // эксперт вводит асимметричное отношение
						dd[cell.RowIndex, cell.ColumnIndex].Value = 0.0;

				};
				flowLayoutPanel_input.Controls.Add(dgv);

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

				double[,] fill_values = new double[n, n];
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < n; j++)
						fill_values[i, j] = 0.0;
				}
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
			}
			activate_input();
		}

		/// <summary>
		/// вывести на экран результирующие ранжирования
		/// </summary>
		/// <param name="Mutual_rankings"></param>
		private void set_output_results(List<string> Mutual_rankings)
		{
			clear_output();
			deactivate_input();
			try
			{
				var tex = $"Минимальное суммарное расстояние для агрегированного графа:\n" +
					$"'модуль разности': {Methods.MinSummaryModulusDistance}\n" +
					$"'квадрат разности': {Methods.MinSummarySquareDistance}\n";
				tex += "\nАгрегированное отношение: \n" + R.Matrix2String();
				tex += "\nМатрица смежности агрегированного отношения: \n" + r.Matrix2String();
				label3.Text = tex;
				foreach (Method met in Methods.GetMethods())
				{
					if (met.IsExecute == true && met.Rankings.Count != 0)
					{
						var r = met.Rankings.Count;
						for (int j = 0; j < r; j++)
						{
							DataGridViewColumn column = new DataGridViewColumn();
							column.CellTemplate = new DataGridViewTextBoxCell();
							column.HeaderText = $"Ранжиро-\nвание {j + 1}";
							column.Name = j.ToString();
							column.HeaderCell.Style.BackColor = window_background;
							column.FillWeight = 1;
							met.connectedFrame.Columns.Add(column);
						}
						for (int i = 0; i < n; i++)
						{
							met.connectedFrame.Rows.Add();
							met.connectedFrame.Rows[i].HeaderCell.Value = $"Место {i + 1}";
						}

						//добавить в конец datagrid-а строку с характеристикой ранжирования
						int add_row_with_characteristic(string label)
						{
							met.connectedFrame.Rows.Add();
							int i = met.connectedFrame.Rows.Count - 1;
							met.connectedFrame.Rows[i].HeaderCell.Value = label;
							return i;
						}
						//задать значение характеристики ранжирования и раскрасить
						void display_characteristic(int j, int i, double min, double max,
							Ranking.Characteristic characteristic)
						{
							met.connectedFrame[j, i].Value = characteristic.Value;
							met.connectedFrame[j, i].Style.BackColor = output_characteristics_bg_color;
							if (min < max)
							{
								if (characteristic.Value == min)
									met.connectedFrame[j, i].Style.BackColor = color_min;
								else if (characteristic.Value == max)
									met.connectedFrame[j, i].Style.BackColor = color_max;
							}
						}

						var some_rank = met.Rankings.First();
						add_row_with_characteristic(some_rank.PathLength.Label);
						add_row_with_characteristic(some_rank.PathStrength.Label);
						add_row_with_characteristic(some_rank.PathSummaryDistance_modulus.Label);
						add_row_with_characteristic(some_rank.PathSummaryDistance_square.Label);

						for (int j = 0; j < r; j++)
						{
							for (int i = 0; i < n; i++)
							{
								met.connectedFrame[j, i].ReadOnly = true;
								met.connectedFrame[j, i].Value = ind2letter[met.Rankings[j].Rank2List[i]];
							}
							if (Mutual_rankings.Count != 0 && Mutual_rankings.Contains(met.Rankings[j].Rank2String))
							{
								for (int i = 0; i < n; i++)
									met.connectedFrame[j, i].Style.BackColor = color_mutual;
							}

							display_characteristic(j, n, met.MinLength, met.MaxLength,
								met.Rankings[j].PathLength);
							display_characteristic(j, n + 1, met.MinStrength, met.MaxStrength,
								met.Rankings[j].PathStrength);
							display_characteristic(j, n + 2, met.MinDistance_modulus, met.MaxDistance_modulus,
								met.Rankings[j].PathSummaryDistance_modulus);
							display_characteristic(j, n + 3, met.MinDistance_square, met.MaxDistance_square,
								met.Rankings[j].PathSummaryDistance_square);
						}
					}

					// вывести на экран победителей по методу Шульце
					if (met.ID == SCHULZE_METHOD && Methods.SchulzeWinners != null)
					{
						string text = "";
						if (met.Rankings == null || met.Rankings.Count == 0)
							text += "Ранжирование невозможно. ";
						text += $"Победители: {string.Join(",", Methods.SchulzeWinners.Select(x => ind2letter[x]))}";
						met.ConnectedLabel = text;
					}
				}
				show_output();
			}
			catch (MyException ex)
			{
				MessageBox.Show($"{ex.Message}");
			}
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
					refresh_variables();
					List<Matrix> matrices = new List<Matrix>();
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
					set_input_datagrids(matrices);
					Form1_SizeChanged(sender, e);
				}
				catch (FileNotFoundException ex)
				{
					throw new MyException($"{ex.Message}");
				}
			}
			catch (MyException ex)
			{
				MessageBox.Show($"{ex.Message}");
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
				if (n > max_count_of_alternatives)
					throw new MyException(EX_n_m_too_big);
				R_list = new List<FuzzyRelation>() { };
				foreach (DataGridView dgv in flowLayoutPanel_input.Controls)
				{
					Matrix input_matrix = new Matrix(n, n);
					for (int i = 0; i < dgv.Rows.Count; i++)
						for (int j = 0; j < dgv.Columns.Count; j++)
							input_matrix[i, j] = (double)dgv[j, i].Value;
					R_list.Add(new FuzzyRelation(input_matrix));
				}
				R_list = R_list.Select(x => x.AsPart).ToList();
				set_input_datagrids(FuzzyRelation.ToMatrixList(R_list));
				var Intersect = execute_algorythms(R_list);
				set_output_results(Intersect);
				// visualize_graph(C, null);//
				Form1_SizeChanged(sender, e);
			}
			catch (MyException ex)
			{
				MessageBox.Show($"{ex.Message}");
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
					set_input_datagrids(null);
					Form1_SizeChanged(sender, e);
				}
			}
			catch (MyException ex)
			{
				MessageBox.Show($"{ex.Message}");
			}
		}

		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			foreach (Method m in Methods.GetMethods())
			{
				if (m.connectedFrame?.Parent != null)
					m.connectedFrame.Parent.Width = flowLayoutPanel_output_tables.Width - 30;
			}
			foreach (DataGridView dgv in flowLayoutPanel_input.Controls)
			{
				dgv.Width = dgv.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + 2 * dgv.RowHeadersWidth;
				dgv.Height = dgv.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + 2 * dgv.ColumnHeadersHeight;
			}
		}

		private void flowLayoutPanel_output_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_output_tables.Focus();
		}

		private void flowLayoutPanel_input_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_input.Focus();
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
						matrices.Add(new FuzzyRelation(cur_matrix));
				}
				m = matrices.Count;
				//R_list = Enumerable.Select(R_list, x => Matrix.GetAsymmetricPart(x)).ToList();
				var Intersect = execute_algorythms(matrices);
				set_output_results(Intersect);//вывод в файл или начисление статистики

				//вывод статистики в файл
			}
		}
	}
}
