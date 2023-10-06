using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;


namespace Group_choice_algos_fuzzy
{
	/// <summary>
	/// AUXILIARY FUNCS
	/// </summary>
	public class Algorithms
	{
		/// <summary>
		/// поиск файлов в директориях
		/// </summary>
		/// <param name="directory_with_file"></param>
		/// <param name="file_name"></param>
		/// <returns></returns>
		public static string FindFile(string directory_with_file, string file_name)
		{
			string absolute_file_name = file_name;
			//сначала пробуем искать в заданной директории
			if (!new object[] { null, "" }.Contains(directory_with_file))
			{
				var path_and_filename = Path.Combine(
					Path.Combine(PROJECT_DIRECTORY, directory_with_file)
					, file_name);
				if (File.Exists(path_and_filename))
				{
					absolute_file_name = path_and_filename;
				}
			}
			//если не нашли, то поищем по всем каталогам внутри, берём первый попавшийся
			if (absolute_file_name == file_name)
			{
				string[] allFoundFiles = Directory.GetFiles(
					PROJECT_DIRECTORY, file_name, SearchOption.AllDirectories);
				if (allFoundFiles.Length != 0)
					absolute_file_name = allFoundFiles.First();
			}
			return absolute_file_name;
		}
		/// <summary>
		/// сравнивалась ли альтернатива с какой-то ещё
		/// </summary>
		/// <param name="index"></param>
		/// <param name="dgv"></param>
		/// <returns></returns>
		public static bool IsCompared(int index, DataGridView dgv)
		{
			for (int j = 0; j < dgv.Rows.Count; j++)
			{
				if (index != j)
				{
					var ij = dgv[j, index].Value as double?;
					var ji = dgv[index, j].Value as double?;
					if (ij != 0 || ji != 0)
						return true;
				}
			}
			return false;
		}
		private static void SetDataGridViewDefaults_FontAndColors(DataGridView dgv)
		{
			//dgv.DefaultCellStyle.Font = new Font(font, font_size);
			dgv.DefaultCellStyle.ForeColor = font_color;
			dgv.DefaultCellStyle.BackColor = input_bg_color;
			dgv.DefaultCellStyle.SelectionForeColor = font_color;
			dgv.DefaultCellStyle.SelectionBackColor = Color.Empty;
		}


		/// <summary>
		/// настрйки для вывода DataGridView
		/// </summary>
		/// <param name="dgv"></param>
		public static void SetDataGridViewDefaults(DataGridView dgv)
		{
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
			dgv.DefaultCellStyle.Format = $"0.{new string('#', DIGITS_PRECISION)}";
			dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
			SetDataGridViewDefaults_FontAndColors(dgv);
			dgv.DataError += (object ss, DataGridViewDataErrorEventArgs anError) => { dgv.CancelEdit(); };
		}
		
		/// <summary>
		/// веса данного пути
		/// </summary>
		/// <param name="vertices_list"></param>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		public static List<double> WeightsOfPath(List<int> vertices_list, Matrix Weights_matrix)
		{
			List<double> weights_list = new List<double>();
			var l = vertices_list.Count;
			// при l = 0 нет пути
			// при l = 1 путь (a) "ничего не делать" - нет пути, так как нет петли
			if (l > 1)
			{  // включает и путь-петлю (a,a)
				for (int i = 0; i < l - 1; i++)
					weights_list.Add(Weights_matrix[vertices_list[i], vertices_list[i + 1]]);
			}
			return weights_list;
		}

		/// <summary>
		/// стоимость пути (суммарный вес)
		/// </summary>
		/// <param name="vertices_list"></param>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		public static double PathCost(List<int> vertices_list, Matrix Weights_matrix)
		{
			return WeightsOfPath(vertices_list, Weights_matrix).Sum();
		}

		/// <summary>
		/// сила пути (пропускная способность)
		/// </summary>
		/// <param name=""></param>
		/// <param name=""></param>
		/// <returns></returns>
		public static double PathStrength(List<int> vertices_list, Matrix Weights_matrix)
		{
			var wp = WeightsOfPath(vertices_list, Weights_matrix);
			return wp.Count == 0 ? INF : wp.Min();
		}

		/// <summary>
		/// нахождение Гамильтоновых путей
		/// </summary>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		public static List<List<int>>[,] Hamiltonian_paths_through_matrix_degree(Matrix Weights_matrix)
		{
			int n = Weights_matrix.GetLength(0);

			string[,] matrices_mult_sym(string[,] M1, string[,] M2)
			{
				int n1 = M1.GetLength(0);
				int n2 = M2.GetLength(1);
				int m = M1.GetLength(1); // = M2.GetLength(0);
				var sep = new char[] { MARK };
				string[,] ans = new string[n1, n2];
				for (int i = 0; i < n1; i++)
					for (int j = 0; j < n2; j++)
					{
						if (i == j)
							ans[i, j] = ZER;//обнулене петель
						else
						{
							string a_ij = "";
							for (int k = 0; k < m; k++)
							{
								var lhs = M1[i, k];
								var rhs = M2[k, j];
								if (lhs == ZER || rhs == ZER)
									a_ij += "";
								else//перемножение двух скобок, каждая скобка - сумма нескольких слагаемых
								{
									var parts1 = lhs.Split(PLS);
									var parts2 = rhs.Split(PLS);
									foreach (string l in parts1)
										foreach (string r in parts2)
											if (!l.Contains(ind2sym[i]) && !l.Contains(ind2sym[j])//если старт или конец пути циклят
												&& !r.Contains(ind2sym[i]) && !r.Contains(ind2sym[j]))
											{
												if (l == ONE)
													a_ij += PLS + r;
												else if (r == ONE)
													a_ij += PLS + l;
												else if (l.Split(sep, StringSplitOptions.RemoveEmptyEntries)
													.Intersect(r.Split(sep, StringSplitOptions.RemoveEmptyEntries))
													.ToArray().Length == 0)//если внутри тоже нет циклов
													a_ij += PLS + l + r;
											}
								}
							}
							if (a_ij == "")
								a_ij = ZER;
							ans[i, j] = a_ij.TrimStart(PLS);
						}
					}
				return ans;
			}

			Matrix Q_int = new Matrix(n, n);
			string[,] Q = new string[n, n];
			string[,] H = new string[n, n];
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
				{
					if (Math.Abs(Weights_matrix[i, j]) == INF || i == j)// с занулением диагонали
					{
						Q_int[i, j] = 0;
						Q[i, j] = ZER;
						H[i, j] = ZER;
					}
					else
					{
						Q_int[i, j] = 1;
						Q[i, j] = ONE;
						H[i, j] = ind2sym[j];
					}
				}
			Matrix Q_paths_cnt = Q_int.Pow(n - 1); //Paths count between vertices			
			int paths_cnt = 0; //Total paths count
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
					if (i != j)
						paths_cnt += (int)Math.Truncate(Q_paths_cnt[i, j]);

			for (int i = 2; i < n; i++)
				Q = matrices_mult_sym(H, Q);

			List<List<int>>[,] Paths_matrix = new List<List<int>>[n, n];
			if (n == 1)
			{
				Paths_matrix[0, 0] = new List<List<int>> { new List<int> { 0 } };
				return Paths_matrix;
			}
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
				{
					Paths_matrix[i, j] = new List<List<int>>();
					if (Q[i, j] != ZER)
					{
						if (n > 2)
						{
							List<string> some_sym_paths = new List<string>();
							some_sym_paths = some_sym_paths.Concat(Q[i, j].Split(PLS)).ToList();
							foreach (string sym_path in some_sym_paths)
							{
								var inds_str = (ind2sym[i] + sym_path + ind2sym[j]).TrimStart(MARK).Split(MARK);
								Paths_matrix[i, j].Add(inds_str.Select(x => int.Parse(x)).ToList());
							}
						}
						else if (n == 2)
							Paths_matrix[i, j].Add(new List<int> { i, j });
					}
				}
			return Paths_matrix;
		}

	}

}
