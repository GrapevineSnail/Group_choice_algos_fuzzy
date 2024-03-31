using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;

namespace Group_choice_algos_fuzzy
{
	class Model
	{
		#region FIELDS
		private static int _n;//количество альтернатив
		private static int _m;//количество экспертов
		public static List<FuzzyRelation> R_list; //список матриц нечётких предпочтений экспертов
		#endregion FIELDS

		#region PROPERTIES
		public static int n
		{
			set
			{
				_n = value;
				SetSymbolsForAlternatives(n);
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
		#endregion PROPERTIES


		/// <summary>
		/// ResultRelation - агрегированная матрица матриц профилей
		/// </summary>
		public static class AggregatedMatrix
		{
			public delegate void MyEventHandler();//сигнатура
			public static event MyEventHandler R_Changed;//для изменения картинки графа
			public static FuzzyRelation Avg;//агрегированная матрица матриц профилей (среднее)
			public static FuzzyRelation Med;//агрегированная матрица матриц профилей (медианные)
			private static FuzzyRelation _R;//текущая используемая агрегированная матрица
			public static FuzzyRelation R
			{
				get { return _R; }
				set
				{
					_R = value;
					R_Changed();
				}
			}
			public static void ClearAggregatedMatrices()
			{
				Avg = new FuzzyRelation(n);
				Med = new FuzzyRelation(n);
				R = new FuzzyRelation(n);
			}
			public static (List<Matrix> Matrices, List<string> Labels) GetRelations2Draw()
			{
				var M = new List<Matrix>{
					R, R.TransClosured,
					R.DestroyedCycles, R.DestroyedCycles.TransClosured,
					R.Asymmetric};
				var L = new List<string>{
					"R", "Tr(R)",
					"Acyclic(R)", "Tr(Acyclic(R))",
					"Asymmetric(R)"};
				var ans = (M, L);
				return ans;
			}
		}


		/// <summary>
		/// все методы
		/// </summary>
		public static class Methods
		{
			public static Method All_various_rankings = new Method(ALL_RANKINGS);
			public static Method All_Hamiltonian_paths = new Method(ALL_HP);
			private static Method Hp_max_length = new Method(HP_MAX_LENGTH);
			private static Method Hp_max_strength = new Method(HP_MAX_STRENGTH);
			public static Method Schulze_method = new Method(SCHULZE_METHOD);//имеет результирующее ранжирование по методу Шульце (единственно)
			public static Method Smerchinskaya_Yashina_method = new Method(SMERCHINSKAYA_YASHINA_METHOD);
			public static class MethodsCharacteristics
			{
				private static double _MaxHamPathLength;//длина пути длиннейших Гаммильтоновых путей
				private static double _MaxHamPathStrength;//сила пути сильнейших Гаммильтоновых путей
				private static Ranking.PathSummaryDistanceClass _MinSummaryDistance;//расстояние наиближайшего ко всем агрегированного ранжирования
				public static double MaxHamPathLength
				{
					get
					{
						if (!IsInitialized(_MaxHamPathLength))
							_MaxHamPathLength = All_Hamiltonian_paths.Rankings.Select(x => x.Cost.Value).Max();
						return _MaxHamPathLength;
					}
				}
				public static double MaxHamPathStrength
				{
					get
					{
						if (!IsInitialized(_MaxHamPathStrength))
							_MaxHamPathStrength = All_Hamiltonian_paths.Rankings.Select(x => x.Strength.Value).Max();
						return _MaxHamPathStrength;
					}
				}
				public static Ranking.PathSummaryDistanceClass MinSummaryDistance
				{
					get
					{
						if (!IsInitialized(_MinSummaryDistance))
						{
							_MinSummaryDistance = new Ranking.PathSummaryDistanceClass();
							if (All_various_rankings.HasResults)
							{
								_MinSummaryDistance.modulus.Value = All_various_rankings.Rankings
									.Select(x => x.SummaryDistance.modulus.Value).Min();
								_MinSummaryDistance.square.Value = All_various_rankings.Rankings
									.Select(x => x.SummaryDistance.square.Value).Min();
							}
						}
						return _MinSummaryDistance;
					}
				}
				private static bool IsInitialized(double t)
				{
					return t != 0 && Math.Abs(t) != INF;
				}
				private static bool IsInitialized(Ranking.PathSummaryDistanceClass t)
				{
					return t != null;
				}
				public static void Clear()
				{
					_MaxHamPathLength = INF;
					_MaxHamPathStrength = INF;
					_MinSummaryDistance = null;
				}
			}
			/// <summary>
			/// очищает результаты методов и характеристики этих результатов
			/// </summary>
			public static void ClearMethods()
			{
				foreach (Method M in GetMethods())
					M.ClearResults();
				MethodsCharacteristics.Clear();
			}
			/// <summary>
			/// выдаёт все используемые методы
			/// </summary>
			/// <returns></returns>
			public static Method[] GetMethods()
			{
				Type t = typeof(Methods);
				return t.GetFields().Select(x => x.GetValue(t) as Method).Where(x => x != null).ToArray();
			}
			/// <summary>
			/// выдаёт все методы, имеющие ответ и отмеченные к выполнению в текущей программе
			/// </summary>
			/// <returns></returns>
			public static List<Method> GetMethodsExecutedWhithResult()
			{
				var is_method_results_exist = new List<Method>();
				foreach (Method m in Methods.GetMethods())
				{
					if (m.IsExecute && m.HasResults)
						is_method_results_exist.Add(m);
				}
				return is_method_results_exist;
			}
			/// <summary>
			/// создание всех возможных ранжирований данных альтернатив
			/// </summary>
			/// <returns></returns>
			public static void Set_All_various_rankings(int n)
			{
				All_various_rankings.ClearResults();
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
						{//выделяем 0-ой едет по всем i-тым местам и вокруг него крутим все возможные перестановки
							List<int> elems = new List<int> { };
							elems.AddRange(elements);
							elems[i] = elements[0];//меняем местами 0-ой и i-ый (0-ое место скипается)
							foreach (List<int> p in permutations_of_elements(elems.Skip(1).ToList()))
								perms.Add(new List<int> { elements[i] }.Concat(p).ToList());
						}
						return perms;
					}
				}
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
			/// вычисляет все Гамильтоновы пути
			/// </summary>
			/// <param name="HP"></param>
			/// <param name="Weights_matrix"></param>
			public static void Set_All_Hamiltonian_paths(Matrix weight_matrix)
			{
				All_Hamiltonian_paths.ClearResults();
				List<List<int>>[,] HP = Hamiltonian_paths_through_matrix_degree(weight_matrix, NO_EDGE);
				for (int i = 0; i < HP.GetLength(0); i++)
					for (int j = 0; j < HP.GetLength(1); j++)
						foreach (List<int> path_from_i_to_j in HP[i, j])
							All_Hamiltonian_paths.Rankings.Add(new Ranking(ALL_HP, path_from_i_to_j));
				Hp_max_length.ClearResults();
				Hp_max_strength.ClearResults();
				foreach (Ranking r in All_Hamiltonian_paths.Rankings)
				{
					if (r.Cost.Value == MethodsCharacteristics.MaxHamPathLength)
						Hp_max_length.Rankings.Add(r);
					if (r.Strength.Value == MethodsCharacteristics.MaxHamPathStrength)
						Hp_max_strength.Rankings.Add(r);
				}
				/// <summary>
				/// нахождение Гамильтоновых путей
				/// </summary>
				List<List<int>>[,] Hamiltonian_paths_through_matrix_degree(Matrix Weights_matrix, double no_edge_symbol)
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
											{
												foreach (string r in parts2)
												{
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
							if (!Weights_matrix.HasEdge((i, j), new double[] { no_edge_symbol , INF, -INF}) 
								|| i == j)// с занулением диагонали
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
			/// <summary>
			/// нахождение ранжирования и победителей методом Шульце
			/// </summary>
			public static void Set_Schulze_method(int n, Matrix weight_matrix)
			{
				Schulze_method.ClearResults();
				var PD = new double[n, n];//strength of the strongest path from alternative i to alternative j
				var pred = new int[n, n];//is the predecessor of alternative j in the strongest path from alternative i to alternative j
				var O = new HashSet<(int, int)>();//множество пар - отношение доминирования
				bool[] winner = Enumerable.Repeat(false, n).ToArray();
				//initialization
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
					{
						PD[i, j] = weight_matrix[i, j];
						pred[i, j] = i;
					}
				//calculation of the strengths of the strongest paths
				for (int j = 0; j < n; j++)
					for (int i = 0; i < n; i++)
						if (j != i)//петли не смотрим
						{
							for (int k = 0; k < n; k++)
							{
								if (j != k && i != k)//петли не смотрим
								{
									var tok = Math.Min(PD[i, j], PD[j, k]);
									if (PD[i, k] < tok)
									{
										PD[i, k] = tok;//увеличиваем силу
										pred[i, k] = pred[j, k];//записываем узел в пути от i до k
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
								O.Remove((j, i));
						}
				}
				//победители - это, буквально, недоминируемые альтернативы
				Schulze_method.Winners = Enumerable.Range(0, n).Where(i => winner[i] == true).ToList();//индексы победителей																								   
				var pair_dominant_matrix = new Matrix(PD);
				var is_ = Ranking.Matrix2RanksDemukron(pair_dominant_matrix, out var levels, out var ranks);
				Schulze_method.Levels = levels;
				if (is_)
				{
					foreach (var r in ranks)
						Schulze_method.Rankings.Add(new Ranking(SCHULZE_METHOD, r));
				}
			}
			/// <summary>
			/// нахождение ранжирований из агрегированной матрицы - используется минимальное расстояние и разбиение контуров
			/// </summary>
			public static void Set_Smerchinskaya_Yashina_method()
			{
				Smerchinskaya_Yashina_method.ClearResults();
				Smerchinskaya_Yashina_method.Winners = AggregatedMatrix.R.DestroyedCycles.TransClosured.UndominatedAlternatives().ToList();
				var is_ = Ranking.Matrix2RanksDemukron(AggregatedMatrix.R.DestroyedCycles.TransClosured, out var levels, out var ranks);
				Smerchinskaya_Yashina_method.Levels = levels;
				if (is_)
				{
					foreach (var rr in ranks)
						Smerchinskaya_Yashina_method.Rankings.Add(new Ranking(SMERCHINSKAYA_YASHINA_METHOD, rr));
				}
			}

		}
	}
}
