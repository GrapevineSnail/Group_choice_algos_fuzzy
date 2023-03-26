using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Group_choice_algos_fuzzy
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			Methods.Hp_max_length = new Method(HP_MAX_LENGTH, cb_HP_max_length, dg_HP_max_length);
			Methods.Hp_max_strength = new Method(HP_MAX_STRENGTH, cb_HP_max_strength, dg_HP_max_strength);
			Methods.Schulze_method = new Method(SCHULZE_METHOD, cb_Schulze_method, dg_Schulze_method);
			Methods.Linear_medians = new Method(LINEAR_MEDIANS, cb_Linear_medians, dg_Linear_medians);
			Methods.All_various_rankings = new Method(ALL_RANKINGS, cb_All_rankings, dg_All_rankings);


			numericUpDown_n.Maximum = max_number_for_spinbox;
			numericUpDown_m.Maximum = max_number_for_spinbox;
			//dataGridView_input_profiles.EnableHeadersVisualStyles = false;
			button_file.Height = textBox_file.Height + 2;
			button_n_m.Height = textBox_file.Height + 2;
			foreach(Control c in flowLayoutPanel_output.Controls)
			{
				c.MouseEnter += flowLayoutPanel_output_MouseEnter;
			}
			foreach (Control c in flowLayoutPanel_input.Controls)
			{
				c.MouseEnter += flowLayoutPanel_input_MouseEnter;
			}

			void interface_coloring(Control o)
			{
				if (o.Controls.Count != 0)
					foreach (Control c in o.Controls)
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
			interface_coloring(this);
			clear_output();
			refresh_variables();
		}

		#region FIELDS

		static int n;//количество альтернатив
		static int m;//количество экспертов
		const int max_count_of_alternatives = 9;
		const int max_number_for_spinbox = 300;
		const int max_number_for_cells = 3000;

		static Color error_color = ColorTranslator.FromHtml("#FFBBBB");
		static Color input_bg_color = Color.White;
		static Color output_characteristics_bg_color = ColorTranslator.FromHtml("#FFDFBFFA");
		static Color disabled_input_bg_color = ColorTranslator.FromHtml("#FFCCCCCC");
		static Color color_min = ColorTranslator.FromHtml("#BBEEFF");
		static Color color_max = ColorTranslator.FromHtml("#FFEEBB");
		static Color color_mutual = ColorTranslator.FromHtml("#D0FFBB");
		static Color window_background = Color.AntiqueWhite; //ColorTranslator.FromHtml("#FAEBD7");
		static Color button_background = Color.Bisque; //ColorTranslator.FromHtml("#FFE4C4");
		static Color disabled_button_background = ColorTranslator.FromHtml("#D9D9D9");
		const float button_borderwidth = 5;
		const string font = "Book Antiqua";
		const string font_mono = "Courier New";
		const float font_size = 10;

		static List<Matrix> R_list; //список матриц нечётких предпочтений экспертов
		static Matrix P;//суммарная матрица матриц профилей
		static Matrix C;//общая матрица весов
		static Matrix R;//общая матрица смежности

		const string ZER = "0";
		const string ONE = "1";
		const char PLS = '+';
		const char mark = 'a';
		static Dictionary<string, int> sym2ind = new Dictionary<string, int>();
		static Dictionary<int, string> ind2sym = new Dictionary<int, string>();
		static double INF = double.PositiveInfinity;
		const int ALL_RANKINGS = 0;
		const int ALL_HP = 1;
		const int LINEAR_MEDIANS = 2;
		const int HP_MAX_LENGTH = 3;
		const int HP_MAX_STRENGTH = 4;
		const int SCHULZE_METHOD = 5;

		#endregion FIELDS


		#region CLASSES
		public class Matrix
		{
			public Matrix() { matrix = new double[matrix.GetLength(0), matrix.GetLength(1)]; }
			public Matrix(int n, int m) { matrix = new double[n, m]; }
			public Matrix(double[,] M) { matrix = M; }
			public Matrix(int[,] M)
			{
				matrix = new double[M.GetLength(0), M.GetLength(1)];
				for (int i = 0; i < this.n; i++)
					for (int j = 0; j < this.m; j++)
						matrix[i, j] = M[i, j];
			}
			public Matrix(Matrix M) { this.matrix = (double[,])M.matrix.Clone(); }
			private double[,] matrix = new double[,] { };
			public double this[int i, int j]
			{
				get { return matrix[i, j]; }
				set { matrix[i, j] = value; }
			}
			/// <summary>
			/// количество строк матрицы
			/// </summary>
			public int n
			{
				get { return matrix.GetLength(0); }
			}
			/// <summary>
			/// количество столбцов матрицы
			/// </summary>
			public int m
			{
				get { return matrix.GetLength(1); }
			}
			public Matrix Self { get { return this; } }
			public static Matrix operator *(double c, Matrix R1)
			{
				var R = new Matrix(R1);
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
						R[i, j] *= c;
				return R;
			}
			public static Matrix operator *(Matrix R1, double c)
			{
				return c * R1;
			}
			public static Matrix operator *(Matrix M1, Matrix M2)
			{
				int l = M1.GetLength(1); // = M2.GetLength(0);
				Matrix R = new Matrix(M1.n, M2.m);
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
					{
						double a_ij = 0;
						for (int k = 0; k < l; k++)
							a_ij += M1[i, k] * M2[k, j];
						R[i, j] = a_ij;
					}
				return R;
			}
			public static Matrix operator /(Matrix R1, double c)
			{
				return (1 / c) * R1;
			}
			public static Matrix operator +(Matrix R1, Matrix R2)
			{
				var R = new Matrix(R1);
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
						R[i, j] += R2[i, j];
				return R;
			}
			public static Matrix operator -(Matrix R1, Matrix R2)
			{
				return R1 + (-1) * R2;
			}
			public int GetLength(int dimension)
			{
				return matrix.GetLength(dimension);
			}
			public Matrix Transpose()
			{
				var R = new Matrix(this.n, this.m);
				for (int i = 0; i < R.n; i++)
					for (int j = i + 1; j < R.m; j++)
					{
						var t = R[i, j];
						R[i, j] = R[j, i];
						R[j, i] = t;
					}
				return R;
			}
			/// <summary>
			/// матрица из нулей
			/// </summary>
			/// <param name="n"></param>
			/// <param name="m"></param>
			/// <returns></returns>
			public static Matrix Zeros(int n, int m)
			{
				var R = new Matrix(n, m);
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
						R[i, j] = 0;
				return R;
			}
			/// <summary>
			/// единичная матрица
			/// </summary>
			/// <param name="n"></param>
			/// <param name="m"></param>
			/// <returns></returns>
			public static Matrix Eye(int n, int m)
			{
				var R = new Matrix(n, m);
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
						R[i, j] = (i == j) ? 1 : 0;
				return R;
			}
			/// <summary>
			/// возведение в степень
			/// </summary>
			public Matrix Pow(int p)
			{//квадратная матрица
				if (this.n != this.m)
					throw new Exception("Для возведения в степень матрица должна быть квадратной");
				var R = Eye(this.n, this.m);
				for (int i = 0; i < p; i++)
					R *= this;
				return R;
			}
			/// <summary>
			/// сумма матриц из списка
			/// </summary>
			/// <param name="M_list"></param>
			/// <returns></returns>
			public static Matrix Sum(List<Matrix> M_list)
			{
				var R = Zeros(M_list.Last().n, M_list.Last().m);
				foreach (Matrix Rj in M_list)
					R += Rj;
				return R;
			}
			/// <summary>
			/// среднее арифметическое матриц из списка
			/// </summary>
			/// <param name="list_of_matrices"></param>
			/// <returns></returns>
			static Matrix Avg(List<Matrix> list_of_matrices)
			{
				return Sum(list_of_matrices) / list_of_matrices.Count();
			}
			/// <summary>
			/// матрица из модулей её элементов
			/// </summary>
			/// <param name="M"></param>
			/// <returns></returns>
			public static Matrix Abs(Matrix M)
			{
				var R = new Matrix(M.n, M.m);
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
						R[i, j] = Math.Abs(M[i, j]);
				return R;
			}
			/// <summary>
			/// сумма всех элементов матрицы
			/// </summary>
			/// <param name="M"></param>
			/// <returns></returns>
			public static double ElemSum(Matrix M)
			{
				double ans = 0;
				for (int i = 0; i < M.n; i++)
					for (int j = 0; j < M.m; j++)
						ans += M[i, j];
				return ans;
			}
			/// <summary>
			/// расстояние между матрицами
			/// </summary>
			/// <param name="M1"></param>
			/// <param name="M2"></param>
			/// <param name="elem_diff"></param>
			/// <returns></returns>
			private static double Distance(Matrix M1, Matrix M2, Func<double, double, double> elem_diff)
			{
				if (M1.n != M2.n || M1.m != M2.m)
					throw new ArgumentException("Не совпадают размерности матриц");
				double ans = 0;
				for (int i = 0; i < M1.n; i++)
					for (int j = i + 1; j < M1.m; j++)
						ans += elem_diff(M1[i, j], M2[i, j]);
				return ans;
			}
			public static double DistanceSquare(Matrix M1, Matrix M2)
			{
				Func<double, double, double> f = (x, y) => Math.Pow(x - y, 2);
				return Distance(M1, M2, f);
			}
			public static double DistanceModulus(Matrix M1, Matrix M2)
			{
				Func<double, double, double> f = (x, y) => Math.Abs(x - y);
				return Distance(M1, M2, f);
			}
			/// <summary>
			/// вычисляет расстояние Хэмминга между двумя матрицами
			/// </summary>
			/// <param name="M1"></param>
			/// <param name="M2"></param>
			/// <returns></returns>
			public static double DistanceHamming(Matrix M1, Matrix M2)
			{// вход: матрицы одинаковой размерности только из 1 и 0
				for (int i = 0; i < M1.n; i++)
					for (int j = 0; j < M1.m; j++)
						if (!(M1[i, j] == 1 || M1[i, j] == 0) || !(M2[i, j] == 1 || M2[i, j] == 0))
							throw new ArgumentException("Некорректные матрицы");
				return DistanceModulus(M1, M2);
			}
			/// <summary>
			/// суммарное расстояние от заданной (left_value) матрицы до всех остальных матриц из списка
			/// </summary>
			public double SumDistance(List<Matrix> List_of_other_R, Func<Matrix, Matrix, double> distance_function)
			{
				// параметром - список матриц смежности
				double sum_dist = 0;
				foreach (Matrix other_R in List_of_other_R)
					sum_dist += distance_function(R, other_R);
				return sum_dist;
			}
		}

		/// <summary>
		/// все методы
		/// </summary>
		static public class Methods
		{
			static public Method All_various_rankings = new Method(ALL_RANKINGS);
			static public Method All_Hamiltonian_paths = new Method(ALL_HP);
			static public Method Linear_medians = new Method(LINEAR_MEDIANS);
			static public Method Hp_max_length = new Method(HP_MAX_LENGTH);
			static public Method Hp_max_strength = new Method(HP_MAX_STRENGTH);
			static public Method Schulze_method = new Method(SCHULZE_METHOD);//имеет результирующее ранжирование по методу Шульце (единственно)

			static public double MinHammingDistance;//расстояние Хэмминга линейных медиан
			static public double MaxLength;//длина пути длиннейших Гаммильтоновых путей
			static public double MaxStrength;//сила пути сильнейших Гаммильтоновых путей
			static public List<int> SchulzeWinners = null;//победители по методу Шульце

			/// <summary>
			/// выдаёт все используемые методы
			/// </summary>
			/// <returns></returns>
			static public Method[] GetMethods()
			{
				Type t = typeof(Methods);
				return t.GetFields().Select(x => x.GetValue(t) as Method).Where(x => x != null).ToArray();
			}

			/// <summary>
			/// создание всех возможных ранжирований данных альтернатив
			/// </summary>
			/// <returns></returns>
			static public void Set_All_various_rankings()
			{
				List<List<int>> permutations_of_elements(List<int> elements)
				{
					var l = elements.Count;
					if (l == 0)
						return new List<List<int>> { };
					else if (l == 1)
						return new List<List<int>> { new List<int> { elements[0] } };
					else
					{
						List<List<int>> perms = new List<List<int>>() { };
						for (int i = 0; i < l; i++)
						{
							List<int> elems = new List<int> { };
							elems.AddRange(elements);
							elems[i] = elements[0];
							foreach (List<int> p in permutations_of_elements(elems.Skip(1).ToList()))
								perms.Add(new List<int> { elements[i] }.Concat(p).ToList());
						}
						return perms;
					}
				}
				All_various_rankings.ClearRankings();
				if (n == 1)
					All_various_rankings.Rankings.Add(new Ranking(ALL_RANKINGS, new List<int> { 0 }));
				else if (n == 2)
				{
					All_various_rankings.Rankings.Add(new Ranking(ALL_RANKINGS, new List<int> { 0, 1 }));
					All_various_rankings.Rankings.Add(new Ranking(ALL_RANKINGS, new List<int> { 1, 0 }));
				}
				else
				{
					for (int i = 0; i < n; i++)
						for (int j = 0; j < n; j++)
							if (i != j)
							{
								List<int> middle_vetrices = new List<int> { };
								for (int v = 0; v < n; v++)
									if (v != i && v != j)
										middle_vetrices.Add(v);
								foreach (List<int> p in permutations_of_elements(middle_vetrices))
								{
									List<int> r = new List<int> { i }.Concat(p).Concat(new List<int> { j }).ToList();
									All_various_rankings.Rankings.Add(new Ranking(ALL_RANKINGS, r));
								}
							}
				}
			}

			/// <summary>
			/// вычисляет все Гамильтоновы пути и сохраняет в метод в словаре
			/// </summary>
			/// <param name="HP"></param>
			/// <param name="Weights_matrix"></param>
			static public void Set_All_Hamiltonian_paths()
			{
				List<List<int>>[,] HP = Hamiltonian_paths_through_matrix_degree(C);
				All_Hamiltonian_paths.ClearRankings();
				for (int i = 0; i < HP.GetLength(0); i++)
					for (int j = 0; j < HP.GetLength(1); j++)
						foreach (List<int> r in HP[i, j])
							All_Hamiltonian_paths.Rankings.Add(new Ranking(ALL_HP, r));
			}

			/// <summary>
			/// линейные медианы для ранжирований экспертов
			/// </summary>
			/// <param name="R_list"></param>
			/// <returns></returns>
			static public void Set_Linear_medians()
			{
				if (All_various_rankings.Rankings.Count == 0)
					Set_All_various_rankings();
				MinHammingDistance = All_various_rankings.Rankings.Select(x => x.PathHammingDistance).Min();
				Linear_medians.ClearRankings();
				foreach (Ranking r in All_various_rankings.Rankings)
					if (r.PathHammingDistance == MinHammingDistance)
						Linear_medians.Rankings.Add(r);
			}

			/// <summary>
			/// Гамильтоновы пути максимальной длины
			/// </summary>
			/// <param name="Weights_matrix"></param>
			/// <returns></returns>
			static public void Set_Hp_max_length()
			{
				if (All_Hamiltonian_paths.Rankings.Count == 0)
					Set_All_Hamiltonian_paths();
				MaxLength = All_Hamiltonian_paths.Rankings.Select(x => x.PathLength).Max();
				Hp_max_length.ClearRankings();
				foreach (Ranking r in All_Hamiltonian_paths.Rankings)
					if (r.PathLength == MaxLength)
						Hp_max_length.Rankings.Add(r);
			}

			/// <summary>
			/// Гамильтоновы пути наибольшей силы
			/// </summary>
			/// <param name="Weights_matrix"></param>
			/// <returns></returns>
			static public void Set_Hp_max_strength()
			{
				if (All_Hamiltonian_paths.Rankings.Count == 0)
					Set_All_Hamiltonian_paths();
				MaxStrength = Enumerable.Select(All_Hamiltonian_paths.Rankings, x => x.PathStrength).Max();
				Hp_max_strength.ClearRankings();
				foreach (Ranking r in All_Hamiltonian_paths.Rankings)
					if (r.PathStrength == MaxStrength)
						Hp_max_strength.Rankings.Add(r);
			}

			/// <summary>
			/// Нахождение ранжирования и победителей методом Шульце
			/// </summary>
			/// <param name="Weights_matrix"></param>
			/// <returns></returns>
			static public void Set_Schulze_method()
			{
				double[,] PD = new double[n, n];//strength of the strongest path from alternative i to alternative j
				int[,] pred = new int[n, n];//is the predecessor of alternative j in the strongest path from alternative i to alternative j
				List<(int, int)> O = new List<(int, int)>();
				bool[] winner = Enumerable.Repeat(false, n).ToArray();

				//initialization
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
					{
						PD[i, j] = C[i, j];
						pred[i, j] = i;
					}
				//calculation of the strengths of the strongest paths
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
						if (i != j)
						{
							for (int k = 0; k < n; k++)
							{
								if (i != k && j != k)
								{
									if (PD[j, k] < Math.Min(PD[j, i], PD[i, k]))
									{
										PD[j, k] = Math.Min(PD[j, i], PD[i, k]);
										pred[j, k] = pred[i, k];
									}
								}
							}
						}
				//calculation of the binary relation O and the set of potential winners
				for (int i = 0; i < n; i++)
				{
					winner[i] = true;
					for (int j = 0; j < n; j++)
						if (i != j)
						{
							if (PD[j, i] > PD[i, j])
							{
								O.Add((j, i));
								winner[i] = false;
							}
							else
								O.RemoveAll(x => x == (j, i));
						}
				}
				SchulzeWinners = Enumerable.Range(0, n).Where(i => winner[i] == true).ToList();

				bool compare_MORETHAN(int A, int B)
				{
					if (PD[A, B] > PD[B, A])
						return true;  // побеждает A. (A > B) т.е. morethan
					return false;
				}

				bool compare_EQUIV(int A, int B)
				{
					if (!compare_MORETHAN(A, B) && !compare_MORETHAN(B, A))
						return true;
					return false;
				}
				// матрица строгих сравнений A>B <=> comparsion[A,B]=true
				// должна быть транзитивна A>B && B>C => A>C
				bool[,] comparsion = new bool[n, n];
				int true_cnt = 0; // количество единиц при транзитивности
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
						if (compare_MORETHAN(i, j))
						{
							comparsion[i, j] = true;
							true_cnt++;
						}
						else
							comparsion[i, j] = false;
				for (int i = 0; i < n; i++)
					for (int j = 0; i < n; i++)
						if (i != j && PD[i, j] == 0)//сила == 0 <=> несравнимость
							for (int k = 0; k < n; k++)
								if (compare_MORETHAN(i, k) && compare_MORETHAN(k, j))
								{
									comparsion[i, j] = true;
									true_cnt++;
								}

				Schulze_method.ClearRankings();
				if (true_cnt == n * (n - 1) / 2)
				{
					int[] r = Enumerable.Range(0, n).Reverse().ToArray();
					for (int i = 0; i < n; i++)
					{
						var j = i;
						while (j > 0 && comparsion[r[j], r[j - 1]])
						{
							var temp = r[j];
							r[j] = r[j - 1];
							r[j - 1] = temp;
							j--;
						}

					}
					Schulze_method.Rankings.Add(new Ranking(SCHULZE_METHOD, r));
				}
			}
		}
		/*
				def all_simple_paths_betweenAB(Adjacency_list, idA, idB):
				   Paths = []  # simple - без циклов
				   if idA != idB:
					   n = len(Adjacency_list)
					   cur_path = []
					   visited = [False for i in range(n)]

					   def enter_in_vertex(v):
						   cur_path.append(v)
						   visited[v] = True  # зашли в вершину

					   def leave_vertex(v):
						   cur_path.pop()
						   visited[v] = False  # вышли - поднялись выше по цепочке

					   def dfs(v):
						   enter_in_vertex(v)
						   if v == idB:  # нашли путь
							   Paths.append(cur_path.copy())
						   else:
							   for next_v in Adjacency_list[v]:
								   if visited[next_v] == False:
									   dfs(next_v)  # идём туда, куда ещё не входили
						   leave_vertex(v)
						   return 0
					   dfs(idA)
				   return Paths

			   def strongest_paths_betweenAB(Weights_matrix,
											 All_paths_betweenAB, idA, idB):
				   Paths = [path for path in All_paths_betweenAB]
				   Weights = [weights_of_path(path, Weights_matrix) for path in Paths]
				   Strongest_paths = []
				   max_strength = -math.inf
				   l = len(Paths)
				   if l > 0:
					   strengths = []
					   for i in range(l):
						   strengths.append(min(Weights[i]))
					   max_strength = max(strengths)
					   for i in range(l):
						   if max_strength == strengths[i]:
							   Strongest_paths.append(Paths[i])
				   return (max_strength, Strongest_paths)

			   Adjacency_list = matrix2adjacency_list(Weights_matrix)
			   Paths_matrix = [[all_simple_paths_betweenAB(Adjacency_list, i, j)
								for j in range(n)] for i in range(n)]
			   # матрица сильнейших путей - strongest paths (SP)
			   SP_matrix = [[0 for j in range(n)] for i in range(n)]
			   # матрица сил сильнейших путей - Power
			   Power = [[0 for j in range(n)] for i in range(n)]
			   for i in range(n):
				   for j in range(n):
					   strength, S_paths = strongest_paths_betweenAB(
						   Weights_matrix, Paths_matrix[i][j], i, j)
					   SP_matrix[i][j] = S_paths
					   Power[i][j] = strength

			   def compare_MORETHAN(A, B):
				   if Power[A][B] > Power[B][A]:
					   return True  # побеждает A. (A > B) т.е. morethan
				   return False

			   def compare_EQUIV(A, B):
				   if not compare_MORETHAN(A, B) && not compare_MORETHAN(B, A):
					   return True
				   return False

			   def is_winner(A):
				   for B in range(n):
					   if B != A && Power[A][B] < Power[B][A]:
						   return False
				   return True
			   S = [A for A in range(n) if is_winner(A)]  # set of winners
			   # матрица строгих сравнений A>B <=> comparsion[A][B]=1
			   # должна быть транзитивна A>B && B>C => A>C
			   comparsion = [[1 if compare_MORETHAN(i, j) else 0
							  for j in range(n)] for i in range(n)]
			   for i in range(n):
				   for j in range(n):
					   if i != j && Power[i][j] == 0:
						   for k in range(n):
							   if compare_MORETHAN(i, k) && compare_MORETHAN(k, j):
								   comparsion[i][j] = 1
			   ranking = null
			   if sum([sum(c) for c in comparsion]) == (n**2 - n)/2:  # количество единиц при транзитивности
				   # результирующее ранжирование по Шульце
				   ranking = [i for i in range(n)]
				   ranking.reverse()
				   for i in range(n):
					   j = i
					   while j > 0 && comparsion[ranking[j]][ranking[j-1]]:
						   ranking[j], ranking[j-1] = ranking[j-1], ranking[j]
						   j -= 1
			   return S, ranking
				*/

		/// <summary>
		/// для каждого метода существуют выдаваемые им ранжирования и др. атрибуты
		/// </summary>
		public class Method
		{
			public Method(int name)
			{
				Name = name;
				Rankings = new List<Ranking>();
			}
			public Method(int name, CheckBox checkBox, DataGridView frame)
			{
				Name = name;
				connectedCheckBox = checkBox;
				connectedFrame = frame;
				Rankings = new List<Ranking>();
			}
			public int Name;//название метода
			private CheckBox connectedCheckBox = null;//чекбокс - будем ли запускать метод
			public DataGridView connectedFrame = null;//в какой контейнер выводить результаты работы метода
			private Label connectedLabel = null;//в какой контейнер выводить текстовые пояснения к методу
			public List<Ranking> Rankings = null;//выдаваемые методом ранжирования
			public double LengthsMin;
			public double LengthsMax;
			public double StrengthsMin;
			public double StrengthsMax;
			public double DistancesMin;
			public double DistancesMax;
			public bool IsExecute
			{
				get { return connectedCheckBox != null ? connectedCheckBox.Checked : false; }
			}
			public string ConnectedLabel
			{
				set
				{
					if (value == "" || value == null)
					{
						connectedLabel?.ResetText();
						connectedLabel?.Hide();
					}
					else
					{
						if (connectedLabel == null)
						{
							connectedLabel = new Label();
							connectedLabel.Dock = DockStyle.Bottom;
							connectedFrame?.Parent?.Controls.Add(connectedLabel);
						}
						connectedLabel.Text = value;
						connectedLabel.Show();
					}

				}
				get { return connectedLabel != null ? connectedLabel.Text : ""; }
			}
			public List<List<int>> Ranks2Lists
			{
				set { Rankings = Enumerable.Select(value, x => new Ranking(x)).ToList(); }
				get { return Enumerable.Select(this.Rankings, x => x.Rank2List).ToList(); }
			}
			public List<int[]> Ranks2Arrays
			{
				set { Rankings = Enumerable.Select(value, x => new Ranking(x)).ToList(); }
				get { return Enumerable.Select(this.Rankings, x => x.Rank2Array).ToList(); }
			}
			public List<string> Ranks2Strings
			{
				get { return this.Rankings.Select(x => x.Rank2String).ToList(); }
			}
			public void ClearRankings()
			{
				Rankings = new List<Ranking>();
				LengthsMin = INF;
				LengthsMax = INF;
				StrengthsMin = INF;
				StrengthsMax = INF;
				DistancesMin = INF;
				DistancesMax = INF;
			}
			/// <summary>
			/// находит минимум характеристики ранжирований
			/// </summary>
			/// <param name="list"></param>
			/// <returns></returns>
			private double min(List<double> list)
			{
				List<double> L = list.Where(x => Math.Abs(x) != INF).ToList();
				if (L.Count == 0)
					return -INF;
				return Enumerable.Min(L);
			}
			/// <summary>
			/// находит максимум характеристики ранжирований
			/// </summary>
			/// <param name="list"></param>
			/// <returns></returns>
			private double max(List<double> list)
			{
				List<double> L = list.Where(x => Math.Abs(x) != INF).ToList();
				if (L.Count == 0)
					return -INF;
				return Enumerable.Max(L);
			}
			/// <summary>
			/// находит минимумы и максимумы по каждой характеристике ранжирований
			/// </summary>
			public void SetCharacteristicsMinsMaxes()
			{
				LengthsMin = min(Rankings.Select(x => x.PathLength).ToList());
				LengthsMax = max(Rankings.Select(x => x.PathLength).ToList());
				StrengthsMin = min(Rankings.Select(x => x.PathStrength).ToList());
				StrengthsMax = max(Rankings.Select(x => x.PathStrength).ToList());
				DistancesMin = min(Rankings.Select(x => x.PathHammingDistance).ToList());
				DistancesMax = max(Rankings.Select(x => x.PathHammingDistance).ToList());
			}
			/// <summary>
			/// очищает весь вывод метода
			/// </summary>
			public void ClearMethodOutput()
			{
				connectedFrame?.Rows.Clear();
				connectedFrame?.Columns.Clear();
				connectedFrame?.Hide();
				connectedFrame?.Parent.Hide();
				ConnectedLabel = "";
			}
			/// <summary>
			/// отображает весь вывод метода
			/// </summary>
			public void ShowMethodOutput()
			{
				if (IsExecute)
				{
					connectedFrame?.Show();
					connectedFrame?.Parent.Show();
				}
			}

		}

		/// <summary>
		/// одно какое-то ранжирование (или путь) со всеми его свойствами
		/// </summary>
		public class Ranking
		{
			public Ranking(int[] rank) { Rank2Array = rank; }
			public Ranking(List<int> rank) { Rank2List = rank; }
			public Ranking(int name, object rank)
			{
				MethodName = name;
				if (rank as List<int> != null)
					Rank2List = rank as List<int>;
				else if (rank as int[] != null)
					Rank2Array = rank as int[];
			}
			public int MethodName;//каким методом получено
			public double PathLength;//длина пути
			public double PathStrength;//сила пути
			public double PathHammingDistance;//суммарное расстояние Хэмминга до всех остальных входных ранжирований
			public List<int> Rank = new List<int>();//список вершин в пути-ранжировании
			public List<int> Rank2List
			{
				set
				{
					Rank = value;
					SetRankingParams();
				}
				get { return Rank; }
			}
			public int[] Rank2Array
			{
				set
				{
					Rank = value.ToList();
					SetRankingParams();
				}
				get { return Rank.ToArray(); }
			}
			public string Rank2String
			{
				get
				{
					return string.Join("", Rank.Select(x => ind2sym[x]).ToList());
				}
			}
			public int Count
			{
				get { return Rank.Count; }
			}
			public List<int> String2List(string s)
			{
				return s.Split(mark).ToList()
					.Where(x => int.TryParse(x, out var _))
					.Select(x => int.Parse(x)).ToList();
			}
			private void SetRankingParams()
			{
				if (Rank != null)
				{
					if (Rank.Count == 0)
					{
						PathLength = INF;
						PathStrength = INF;
						PathHammingDistance = INF;
					}
					else
					{
						PathLength = path_length(Rank2List, C);
						PathStrength = path_strength(Rank2List, C);
						PathHammingDistance = path_sum_Hamming_distance(Rank2List, R_list);
					}
				}
			}
			public override int GetHashCode() => Rank.GetHashCode();
		}

		public class Fuzzy
		{
			public class FuzzySet
			{
				public FuzzySet() { isin = new Dictionary<object, double>(); }
				public FuzzySet(Dictionary<object, double> func) { isin = func; }
				private Dictionary<object, double> isin;//функция принадлжености
				public double this[object i]
				{
					get
					{
						if (!isin.Keys.Contains(i))
							isin[i] = 0;
						return isin[i];
					}
					set { isin[i] = value; }
				}
				/// <summary>
				/// все элементы, в том числе с принадлежностью 0
				/// </summary>
				/// <returns></returns>
				public List<object> Elements()
				{
					return isin.Keys.ToList();
				}
				public FuzzySet Negotate()//not A
				{
					var S = new FuzzySet();
					foreach (var x in Elements())
						S[x] = 1 - this[x];
					return S;
				}
				public FuzzySet Intersect(FuzzySet other)//A \intersect B
				{
					var S = new FuzzySet();
					foreach (var x in Elements().Concat(other.Elements()))
						S[x] = Math.Min(this[x], other[x]);
					return S;
				}
				public FuzzySet Union(FuzzySet other)//A U B
				{
					var S = new FuzzySet();
					foreach (var x in Elements().Concat(other.Elements()))
						S[x] = Math.Max(this[x], other[x]);
					return S;
				}
				public FuzzySet SetMinus(FuzzySet other)//A \ B
				{
					var S = new FuzzySet();
					foreach (var x in Elements().Concat(other.Elements()))
						S[x] = Math.Max(this[x] - other[x], 0);
					return S;
				}
			}
			public class FuzzyRelation : Matrix
			{// base - матрица нечёткого бинарного отношения
			 //полагаем квадратными
				public FuzzyRelation() : base() { }
				public FuzzyRelation(int n) : base(n, n) { }
				public FuzzyRelation(double[,] M) : base(M) { }
				public FuzzyRelation(int[,] M) : base(M) { }
				public FuzzyRelation(Matrix M) : base(M) { }

				public double this[Tuple<int, int> pair]
				{
					get { return base[pair.Item1, pair.Item2]; }
					set { base[pair.Item1, pair.Item2] = value; }
				}
				/// <summary>
				/// матрица принадлежности (= м. предпочтений)
				/// </summary>
				public Matrix M
				{
					get { return base.Self; }
				}
				public Tuple<int, int>[] Elements()
				{
					Tuple<int, int>[] ans = new Tuple<int, int>[this.n * this.m];
					for (int i = 0; i < this.n; i++)
						for (int j = 0; j < this.m; j++)
							ans[this.m * i + j] = new Tuple<int, int>(i, j);
					return ans;
				}
				/// <summary>
				/// дополнение
				/// </summary>
				/// <returns></returns>
				public FuzzyRelation Negotate()//not A
				{
					var S = new FuzzyRelation();
					foreach (var x in Elements())
						S[x] = 1 - this[x];
					return S;
				}
				/// <summary>
				/// пересечение
				/// </summary>
				/// <param name="other"></param>
				/// <returns></returns>
				public FuzzyRelation Intersect(FuzzyRelation other)//A \intersect B
				{
					var S = new FuzzyRelation();
					foreach (var x in Elements().Concat(other.Elements()))
						S[x] = Math.Min(this[x], other[x]);
					return S;
				}
				/// <summary>
				/// объединение
				/// </summary>
				/// <param name="other"></param>
				/// <returns></returns>
				public FuzzyRelation Union(FuzzyRelation other)//A U B
				{
					var S = new FuzzyRelation();
					foreach (var x in Elements().Concat(other.Elements()))
						S[x] = Math.Max(this[x], other[x]);
					return S;
				}
				/// <summary>
				/// симметрическая разность
				/// </summary>
				/// <param name="other"></param>
				/// <returns></returns>
				public FuzzyRelation SetMinus(FuzzyRelation other)//A \ B
				{
					var S = new FuzzyRelation();
					foreach (var x in Elements().Concat(other.Elements()))
						S[x] = Math.Max(this[x] - other[x], 0);
					return S;
				}
				/// <summary>
				/// обратное отношение
				/// </summary>
				/// <returns></returns>
				public FuzzyRelation Inverse()
				{
					var R = new FuzzyRelation();
					for (int i = 0; i < this.n; i++)
						for (int j = i + 1; j < this.m; j++)
						{
							var t = R[i, j];
							R[i, j] = R[j, i];
							R[j, i] = t;
						}
					return new FuzzyRelation(Transpose());
				}
				/// <summary>
				/// композиция нечётких бинарных отношений
				/// </summary>
				/// <returns></returns>
				public FuzzyRelation Compose(FuzzyRelation other)
				{
					var R = new FuzzyRelation();
					int[] Z = Enumerable.Range(0, this.n).ToArray();
					for (int i = 0; i < this.n; i++)
						for (int j = i + 1; j < this.m; j++)
						{
							R[i, j] = Z.Select(z => Math.Min(this[i, z], other[z, j])).Max();
						}
					return R;
				}
				/// <summary>
				/// возведение отношения в степень
				/// </summary>
				public new FuzzyRelation Pow(int p)
				{
					var R = this;
					for (int k = 1; k < p; k++)
						R = R.Compose(R);
					return R;
				}
			}
		}

		#endregion CLASSES


		#region COMPUTATION FUNCS		

		/// <summary>
		/// веса данного пути
		/// </summary>
		/// <param name="vertices_list"></param>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		static List<double> weights_of_path(List<int> vertices_list, Matrix Weights_matrix)
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
		/// длина пути
		/// </summary>
		/// <param name="vertices_list"></param>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		static double path_length(List<int> vertices_list, Matrix Weights_matrix)
		{
			return weights_of_path(vertices_list, Weights_matrix).Sum();
		}

		/// <summary>
		/// сила пути
		/// </summary>
		/// <param name=""></param>
		/// <param name=""></param>
		/// <returns></returns>
		static double path_strength(List<int> vertices_list, Matrix Weights_matrix)
		{
			var wp = weights_of_path(vertices_list, Weights_matrix);
			if (wp.Count == 0)
				return -INF;
			return Enumerable.Min(wp);
		}

		/// <summary>
		/// суммарное расстояние Хэмминга от некоторого пути до всех остальных матриц профилей экспертов
		/// </summary>
		/// <param name="vertices_list"></param>
		/// <param name="R_list"></param>
		/// <returns></returns>
		static double path_sum_Hamming_distance(List<int> vertices_list, List<Matrix> R_list)
		{
			return make_single_R_profile_matrix(vertices_list.ToArray())
				.SumDistance(R_list, Matrix.DistanceHamming);
		}

		/// <summary>
		/// создаёт матрицу смежности (порядок) из профиля эксперта
		/// </summary>
		/// <param name="single_profile"></param>
		/// <returns></returns>
		static Matrix make_single_R_profile_matrix(int[] single_profile)
		{
			var l = single_profile.Length;
			if (l != n || l != Enumerable.Distinct(single_profile).ToArray().Length)
				throw new ArgumentException("Некорректный профиль эксперта");
			int[,] Rj = new int[l, l];
			for (int i = 0; i < l; i++)
				for (int j = 0; j < l; j++)
				{
					var candidate1 = single_profile[i];
					var candidate2 = single_profile[j];
					if (i < j)
						Rj[candidate1, candidate2] = 1;
					else
						Rj[candidate1, candidate2] = 0;
				}
			return new Matrix(Rj); // adjacency_matrix
		}

		/// <summary>
		/// создаёт матрицу смежности
		/// </summary>
		/// <param name="weight_C_matrix"></param>
		/// <returns></returns>
		static Matrix make_sum_R_profile_matrix(Matrix weight_C_matrix)
		{  // adjacency_matrix	
			R = new Matrix(n, n);
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
				{
					if (Math.Abs(weight_C_matrix[i, j]) != INF)
						R[i, j] = 1;
					else
						R[i, j] = 0;
				}
			return R;
		}

		/// <summary>
		/// создаёт матрицу весов
		/// </summary>
		/// <param name="summarized_P_matrix"></param>
		/// <returns></returns>
		static Matrix make_weight_C_matrix(Matrix summarized_P_matrix)
		{
			C = new Matrix(n, n);
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
				{
					if (i == j || summarized_P_matrix[i, j] < summarized_P_matrix[j, i])
						C[i, j] = -INF;
					else
						C[i, j] = summarized_P_matrix[i, j] - summarized_P_matrix[j, i];
				}
			return C;
		}

		/// <summary>
		/// нахождение Гамильтоновых путей
		/// </summary>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		static List<List<int>>[,] Hamiltonian_paths_through_matrix_degree(Matrix Weights_matrix)
		{
			int n = Weights_matrix.GetLength(0);

			string[,] matrices_mult_sym(string[,] M1, string[,] M2)
			{
				int n1 = M1.GetLength(0);
				int n2 = M2.GetLength(1);
				int m = M1.GetLength(1); // = M2.GetLength(0);
				var sep = new char[] { mark };
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
								var inds_str = (ind2sym[i] + sym_path + ind2sym[j]).TrimStart(mark).Split(mark);
								Paths_matrix[i, j].Add(inds_str.Select(x => int.Parse(x)).ToList());
							}
						}
						else if (n == 2)
							Paths_matrix[i, j].Add(new List<int> { i, j });
					}
				}
			return Paths_matrix;
		}

		#endregion


		#region AUXILIARY FUNCS

		/// <summary>
		/// установка дефолтных значений переменных
		/// </summary>
		void refresh_variables()
		{
			R_list = new List<Matrix>();
			P = new Matrix { };
			C = new Matrix { };
			R = new Matrix { };
			foreach (Method M in Methods.GetMethods())
				M.ClearRankings();
			Methods.MinHammingDistance = 0;
			Methods.MaxLength = 0;
			Methods.MaxStrength = 0;
			Methods.SchulzeWinners = null;

			for (int i = 0; i < n; i++)
			{
				sym2ind[$"{mark}{i}"] = i;
				ind2sym[i] = $"{mark}{i}";
			}
		}

		/// <summary>
		/// переводит индекс альтернативы в её буквенное обозначение, если возможно (букв всего 26)
		/// </summary>
		/// <param name="index"></param>
		/// <param name="max_index"></param>
		/// <returns></returns>
		string index2symbol(int index, int max_index)
		{
			if (max_index > 25)
			{
				return (index + 1).ToString();
			}
			else
			{
				string symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
				return symbols[index].ToString();
			}
		}

		/// <summary>
		/// переводит буквенное обозначение альтернативы в её индекс (букв всего 26)
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns></returns>
		int symbol2index(string symbol)
		{
			char s = symbol[0];
			bool is_int = int.TryParse(symbol, out var index);
			if (symbol.Length == 1 && char.IsLetter(s))
			{
				string symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
				//Dictionary<char, int> correspondence = new Dictionary<char, int>();
				//for (int i = 0; i < symbol.Length; i++) { correspondence[symbols[i]] = i; }
				return symbols.IndexOf(char.ToUpper(s));     //(int)correspondence[s];
			}
			else if (is_int && index > 0)
			{
				return index - 1;
			}
			else
			{
				throw new ArgumentException("Неверный символ");
			}
		}

		void clear_output()
		{
			try
			{
				label_info.Visible = false;
				foreach (var m in Methods.GetMethods())
					m.ClearMethodOutput();
			}
			catch (Exception ex) { }
		}

		void show_output()
		{
			try
			{
				label_info.Visible = true;
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
				foreach (Control dgv in flowLayoutPanel_input.Controls)
				{
					//dgv.Rows.Clear();
					//dgv.Columns.Clear();
					dgv.Dispose();
				}
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
		/// для удобства печати матриц
		/// </summary>
		/// <param name="Matrix"></param>
		/// <returns></returns>
		string matrix2string(double[,] Matrix)
		{
			/// удаляет последние cnt символов из строки
			string trim_end(string s, int cnt)
			{
				return s.Remove(s.Length - cnt, cnt);
			}
			var n = Matrix.GetLength(0);
			var m = Matrix.GetLength(1);
			int[] max_widths = new int[m];
			for (int j = 0; j < m; j++)
				max_widths[j] = 3;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					if (Matrix[i, j].ToString().Length > max_widths[j])
						max_widths[j] = Matrix[i, j].ToString().Length;
			var str = "";
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					//var fill = "_";
					//var align = "^";
					int width = m > 5 ? max_widths[j] + 2 : max_widths.Max() + 2;
					//str += string.Format("[{0:{fill}{align}{width}}]", Matrix[i, j], fill, align, width);
					str += string.Format($"[{{0,{width}}}]", Matrix[i, j], width);
				}
				str += "\n";
			}
			return trim_end(str, 1);
		}

		#endregion


		#region INTERFACE FUNCS

		/// <summary>
		/// считать профили экспертов из файла
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void read_profiles_file(object sender, EventArgs e)
		{
			try
			{
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
							throw new ArgumentException("Неверное количество альтернатив профиля");
						for (int j = 0; j < numbers.Length; j++)
							cur_matrix[l % nn, j] = numbers[j];
					}
					if (l % nn == nn - 1)
						matrices.Add(new Matrix(cur_matrix));
				}
				if (matrices.Count == 0)
					throw new ArgumentException("Пустой файл");
				m = matrices.Count;
				n = nn;
				set_input_datagrids_matrices(sender, e, matrices);
			}
			catch (FileNotFoundException ex)
			{
				throw new Exception($"Файл не найден\n{ex.Message}");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Файл некорректен\n{ex.Message}");
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
					if (n <= 0 || n <= 0)
						throw new ArgumentException("Должно выполняться n > 0, m > 0");
					clear_input();
					clear_output();
					refresh_variables();
					if (n > max_number_for_spinbox || m > max_number_for_spinbox || n * m > max_number_for_cells)
						throw new ArgumentException(
							"Число альтернатив n и/или число экспертов m слишком большое\n" +
							$"n максимальное = {max_number_for_spinbox}\n" +
							$"m максимальное = {max_number_for_spinbox}\n" +
							$"n*m максимальное = {max_number_for_cells}");
					else
						set_input_datagrids_matrices(sender, e, null);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Введите кооректные данные\n{ex.Message}");
			}
		}

		/// <summary>
		/// размещение таблицы для ввода профилей
		/// </summary>
		private void set_input_datagrids_matrices(object sender, EventArgs e, List<Matrix> list_of_matrices)
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
						dgv[j, i].Value = fill_values[i,j];
						dgv[j, i].ValueType = typeof(double);
					}
			}
			Form1_SizeChanged(sender, e);
			activate_input();
		}

		/// <summary>
		/// чтение входных профилей и запуск работы программы на выбранных алгоритмах
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void read_profiles_input_table(object sender, EventArgs e)
		{
			try
			{
				if (n == 0 || m == 0)
					throw new ArgumentException("Пустой ввод");
				if (n > max_count_of_alternatives)
					throw new ArgumentException(
						"Количество альтернатив n слишком велико\nПрограмма может зависнуть");
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
				MessageBox.Show($"Введите кооректные данные\n{ex.Message}");
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
					throw new Exception("Выберите метод");

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
			label_info.Text = $"Минимальное суммарное расстояние Хэмминга для мажоритарного графа: " +
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
						met.connectedFrame[j, n + 2].Value = met.Rankings[j].PathHammingDistance;
						if (met.Rankings[j].PathHammingDistance == Methods.MinHammingDistance)
							met.connectedFrame[j, n + 2].Value += "\nМедиана";

						for (int k = 0; k < 3; k++)
							met.connectedFrame[j, n + k].Style.BackColor = output_characteristics_bg_color;

						color_characteristics(j, met.LengthsMin, met.LengthsMax, met.Rankings[j].PathLength);
						color_characteristics(j, met.StrengthsMin, met.StrengthsMax, met.Rankings[j].PathStrength);
						color_characteristics(j, met.DistancesMin, met.DistancesMax, met.Rankings[j].PathHammingDistance);

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

		#endregion

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
