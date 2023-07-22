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


namespace Group_choice_algos_fuzzy
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			//связывание методов с control-ами на форме
			Methods.Hp_max_length = new Method(HP_MAX_LENGTH, cb_HP_max_length, dg_HP_max_length);
			Methods.Hp_max_strength = new Method(HP_MAX_STRENGTH, cb_HP_max_strength, dg_HP_max_strength);
			Methods.Schulze_method = new Method(SCHULZE_METHOD, cb_Schulze_method, dg_Schulze_method);
			Methods.Linear_medians = new Method(LINEAR_MEDIANS, cb_Linear_medians, dg_Linear_medians);
			Methods.All_various_rankings = new Method(ALL_RANKINGS, cb_All_rankings, dg_All_rankings);

			numericUpDown_n.Minimum = 1;
			numericUpDown_m.Minimum = 1;
			numericUpDown_n.Maximum = max_number_for_spinbox;
			numericUpDown_m.Maximum = max_number_for_spinbox;

			//dataGridView_input_profiles.EnableHeadersVisualStyles = false;
			button_file.Height = textBox_file.Height + 2;
			button_n_m.Height = textBox_file.Height + 2;
			foreach (Control c in flowLayoutPanel_output.Controls)
			{
				c.MouseEnter += flowLayoutPanel_output_MouseEnter;
			}
			foreach (Control c in flowLayoutPanel_input.Controls)
			{
				c.MouseEnter += flowLayoutPanel_input_MouseEnter;
			}

			interface_coloring(this);
			clear_output();
			refresh_variables();
		}

		#region GLOBALS
		public static int n;//количество альтернатив
		public static int m;//количество экспертов
		public static List<Matrix> R_list; //список матриц нечётких предпочтений экспертов
		public static Matrix P;//суммарная матрица матриц профилей
		public static Matrix C;//общая матрица весов
		public static Matrix R;//общая матрица смежности
		#endregion GLOBALS


		void clear_output()
		{
			try
			{
				label3.Visible = false;
				foreach (var m in Methods.GetMethods())
					m.ClearMethodOutput();
			}
			catch (Exception ex) { }
		}

		void show_output()
		{
			try
			{
				label3.Visible = true;
				foreach (var m in Methods.GetMethods())
					m.ShowMethodOutput();
				flowLayoutPanel_output.Focus();
			}
			catch (Exception ex) { }
		}

		void clear_input()
		{
			try
			{
				flowLayoutPanel_input.Controls.Clear();
			}
			catch (Exception ex) { }
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
			catch (Exception ex) { }
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
			catch (Exception ex) { }
		}

		/// <summary>
		/// установка дефолтных значений переменных
		/// </summary>
		void refresh_variables()
		{
			R_list = new List<Matrix>();
			P = new Matrix { };
			C = new Matrix { };
			R = new Matrix { };
			Methods.ClearMethods();
			SetConstants(n);
		}

		/// <summary>
		/// начальное расцвечивание формы
		/// </summary>
		/// <param name="main_control"></param>
		void interface_coloring(Control main_control)
		{
			if (main_control.Controls.Count != 0)
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

		/// <summary>
		/// считать профили экспертов из файла
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void read_profiles_file(object sender, EventArgs e)
		{
			try
			{
				clear_input();
				refresh_variables();
				string s = textBox_file.Text;
				List<Matrix> matrices = new List<Matrix>();
				string[] lines = File.ReadAllLines(s).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
				var nn = lines.First().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Count();
				Matrix cur_matrix = new Matrix(nn, nn);
				for (int l = 0; l < lines.Length; l++)
				{
					if (lines[l].Length != 0)
					{
						double res;
						double[] numbers = lines[l].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
							.Select(x => double.TryParse(x, out res) ? res : INF).ToArray();
						if (numbers.Any(x => x == INF) || numbers.Length != nn)
							throw new ArgumentException(EX_number_of_alternatives);
						for (int j = 0; j < numbers.Length; j++)
							cur_matrix[l % nn, j] = numbers[j];
					}
					if (l % nn == nn - 1)
						matrices.Add(new Matrix(cur_matrix));
				}
				if (matrices.Count == 0)
					throw new ArgumentException(EX_file_empty);
				m = matrices.Count;
				n = nn;
				set_input_datagrids_matrices(matrices);
				Form1_SizeChanged(sender, e);
			}
			catch (FileNotFoundException ex)
			{
				throw new Exception(EX_file_not_found + $"\n{ex.Message}");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"\n{ex.Message}");
			}
			finally
			{
				numericUpDown_n.Value = n;
				numericUpDown_m.Value = m;
			}
		}

		/// <summary>
		/// Считывает с формы переменные n и m (количество альтернатив и экспертов)
		/// </summary>
		private void read_n_and_m(object sender, EventArgs e)
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
					n = (int)numericUpDown_n.Value;
					m = (int)numericUpDown_m.Value;
					if (n > max_number_for_spinbox || m > max_number_for_spinbox || n * m > max_number_for_cells)
						throw new ArgumentException(EX_n_m_too_big);
					else
					{
						set_input_datagrids_matrices(null);
						Form1_SizeChanged(sender, e);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"\n{ex.Message}");
			}
		}

		/// <summary>
		/// размещение таблицы для ввода профилей
		/// </summary>
		private void set_input_datagrids_matrices(List<Matrix> list_of_matrices)
		{
			clear_input();
			clear_output();
			string[] used_alternatives = new string[n];
			for (int i = 0; i < n; i++)
				used_alternatives[i] = index2symbol(i, n - 1);
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
				dgv.CellEndEdit += (object ss, DataGridViewCellEventArgs ee) =>
				{
					var cell = ((DataGridView)ss).CurrentCell;
					double res;
					if (!double.TryParse(cell.Value.ToString(), out res)
					|| res > 1 || res < 0)
						cell.Value = 0.0;
				};
				flowLayoutPanel_input.Controls.Add(dgv);

				for (int j = 0; j < n; j++)
				{
					DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
					column.Name = j.ToString();
					column.HeaderText = $"{index2symbol(j, n - 1)}";
					column.SortMode = DataGridViewColumnSortMode.NotSortable;
					dgv.Columns.Add(column);
				}
				for (int i = 0; i < n; i++)
				{
					dgv.Rows.Add();
					dgv.Rows[i].HeaderCell.Value = $"{index2symbol(i, n - 1)}";
				}

				double[,] fill_values = new double[n, n];
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
						fill_values[i, j] = 0.0;
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
		/// чтение входных профилей и запуск работы программы на выбранных алгоритмах
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void read_input_expert_profiles(object sender, EventArgs e)
		{
			try
			{
				if (n > max_count_of_alternatives)
					throw new ArgumentException(EX_n_m_too_big);
				R_list = new List<Matrix>() { };
				foreach (DataGridView dgv in flowLayoutPanel_input.Controls)
				{
					Matrix input_matrix = new Matrix(n, n);
					for (int i = 0; i < dgv.Rows.Count; i++)
						for (int j = 0; j < dgv.Columns.Count; j++)
							input_matrix[i, j] = (double)dgv[j, i].Value;
					R_list.Add(new Matrix(input_matrix));
				}
				execute_algorythms(R_list);
				Form1_SizeChanged(sender, e);
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show($"\n{ex.Message}");
			}
		}

		/// <summary>
		/// запускает выполнение выбранных алгоритмов
		/// </summary>
		/// <param name="list_of_profiles"></param>
		private void execute_algorythms(List<Matrix> Relations_list)
		{
			try
			{
				refresh_variables();
				var checkbuttons = Enumerable.Select(Methods.GetMethods(), x => x.IsExecute);
				var frames = Enumerable.Select(Methods.GetMethods(), x => x.connectedFrame);
				if (Enumerable.All(checkbuttons, x => x == false))
					throw new Exception(EX_choose_method);

				R_list = Relations_list;
				P = Matrix.Sum(R_list);
				C = make_weight_C_matrix(P);
				R = make_sum_R_profile_matrix(C);
				Methods.Set_Linear_medians();
				if (Methods.All_various_rankings.IsExecute && Methods.All_various_rankings.Rankings.Count == 0)
					Methods.Set_All_various_rankings();
				if (Methods.Linear_medians.IsExecute)
					Methods.Set_Linear_medians();
				if (Methods.All_Hamiltonian_paths.IsExecute && Methods.All_Hamiltonian_paths.Rankings.Count == 0)
					Methods.Set_All_Hamiltonian_paths();
				if (Methods.Hp_max_length.IsExecute)
					Methods.Set_Hp_max_length();
				if (Methods.Hp_max_strength.IsExecute)
					Methods.Set_Hp_max_strength();
				if (Methods.Schulze_method.IsExecute)
					Methods.Set_Schulze_method();
				List<string> Intersect = new List<string>();
				bool[] is_rankings_of_method_exist = Enumerable.Select(Methods.GetMethods(),
					x => x.IsExecute && x.Rankings != null && x.Rankings.Count != 0).ToArray();
				if (is_rankings_of_method_exist.Where(x => x == true).Count() > 1)
				{
					bool enter_intersect = false;
					foreach (Method m in Methods.GetMethods())
					{
						if (m != null && m.IsExecute == true && m.Rankings != null && m.Rankings.Count > 0)
						{
							if (enter_intersect == false)
							{
								Intersect = m.Ranks2Strings;
								enter_intersect = true;
							}
							else
								Intersect = Enumerable.Intersect(Intersect, m.Ranks2Strings).ToList();
						}
					}
				}
				set_output_table(Intersect);
				// visualize_graph(C, null);//

			}
			catch (Exception ex)
			{
				MessageBox.Show($"{ex.Message}");
			}
		}

		/// <summary>
		/// вывести на экран результирующие ранжирования
		/// </summary>
		/// <param name="Mutual_rankings"></param>
		private void set_output_table(List<string> Mutual_rankings)
		{
			clear_output();
			deactivate_input();
			label3.Text = $"Минимальное суммарное расстояние Хэмминга для мажоритарного графа: " +
				$"{R.SumDistance(R_list, Matrix.DistanceHamming)}";
			foreach (Method met in Methods.GetMethods())
			{
				if (met.IsExecute == true && met.Rankings.Count != 0)
				{
					var r = met.Rankings.Count;
					met.connectedFrame.Rows.Clear();
					met.connectedFrame.Columns.Clear();
					string[] values = new string[n];
					for (int i = 0; i < n; i++)
						values[i] = index2symbol(i, n - 1);
					for (int j = 0; j < r; j++)
					{
						DataGridViewColumn column = new DataGridViewColumn();
						column.CellTemplate = new DataGridViewTextBoxCell();
						column.HeaderText = $"Ранжи-\nрование {j + 1}";
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
					met.connectedFrame.Rows.Add();
					met.connectedFrame.Rows[met.connectedFrame.RowCount - 1].HeaderCell.Value = "Длина:";
					met.connectedFrame.Rows.Add();
					met.connectedFrame.Rows[met.connectedFrame.RowCount - 1].HeaderCell.Value = "Сила:";
					met.connectedFrame.Rows.Add();
					met.connectedFrame.Rows[met.connectedFrame.RowCount - 1].HeaderCell.Value = "Суммарное расстояние\nХэмминга:";

					met.SetCharacteristicsMinsMaxes();
					void color_characteristics(int j, double min, double max, double charact_value)
					{
						if (min < max)
						{
							if (charact_value == min)
								met.connectedFrame[j, n].Style.BackColor = color_min;
							else if (charact_value == max)
								met.connectedFrame[j, n].Style.BackColor = color_max;
						}
					}
					for (int j = 0; j < r; j++)
					{
						for (int i = 0; i < met.Rankings[j].Count; i++)
						{
							met.connectedFrame[j, i].ReadOnly = true;
							met.connectedFrame[j, i].Value = index2symbol(met.Rankings[j].Rank2List[i], n - 1);
						}
						met.connectedFrame[j, n].Value = met.Rankings[j].PathLength;
						met.connectedFrame[j, n + 1].Value = met.Rankings[j].PathStrength;
						met.connectedFrame[j, n + 2].Value = met.Rankings[j].PathSummaryDistance;
						if (met.Rankings[j].PathSummaryDistance == Methods.MinSummaryDistance)
							met.connectedFrame[j, n + 2].Value += "\nМедиана";

						for (int k = 0; k < 3; k++)
							met.connectedFrame[j, n + k].Style.BackColor = output_characteristics_bg_color;

						color_characteristics(j, met.LengthsMin, met.LengthsMax, met.Rankings[j].PathLength);
						color_characteristics(j, met.StrengthsMin, met.StrengthsMax, met.Rankings[j].PathStrength);
						color_characteristics(j, met.DistancesMin, met.DistancesMax, met.Rankings[j].PathSummaryDistance);

						if (Mutual_rankings.Count != 0 && Mutual_rankings.Contains(met.Rankings[j].Rank2String))
							for (int i = 0; i < met.Rankings[j].Count; i++)
								met.connectedFrame[j, i].Style.BackColor = color_mutual;
					}
				}

				// вывести на экран победителей по методу Шульце
				if (met.Name == SCHULZE_METHOD && Methods.SchulzeWinners != null)
				{
					string text = "";
					if (met.Rankings == null || met.Rankings.Count == 0)
						text += "Ранжирование невозможно. ";
					text += $"Победители: {string.Join(",", Methods.SchulzeWinners.Select(x => index2symbol(x, n)))}";
					met.ConnectedLabel = text;
				}
			}
			show_output();
		}

		private void Form1_SizeChanged(object sender, EventArgs e)
		{
			foreach (Method m in Methods.GetMethods())
			{
				if (m.connectedFrame?.Parent != null)
					m.connectedFrame.Parent.Width = flowLayoutPanel_output.Width - 30;
			}
			foreach (DataGridView dgv in flowLayoutPanel_input.Controls)
			{
				dgv.Width = flowLayoutPanel_input.Width
					- flowLayoutPanel_input.Padding.Right - flowLayoutPanel_input.Padding.Left
					- dgv.Margin.Right - dgv.Margin.Left;
			}
		}

		private void flowLayoutPanel_output_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_output.Focus();
		}

		private void flowLayoutPanel_input_MouseEnter(object sender, EventArgs e)
		{
			flowLayoutPanel_input.Focus();
		}
	}
}
