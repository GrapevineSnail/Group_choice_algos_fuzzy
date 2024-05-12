using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.ClassOperations;
using static Group_choice_algos_fuzzy.ClassOperations.OPS_DataGridView;
using static Group_choice_algos_fuzzy.ClassOperations.OPS_GraphDrawing;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Constants.MyException;

namespace Group_choice_algos_fuzzy
{
	class Model
	{
		#region FIELDS
		private static int _n;//количество альтернатив
		private static int _m;//количество экспертов
		public static Form1 form1;
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
			set { _m = value; }
			get { return _m; }
		}
		#endregion PROPERTIES

		/// <summary>
		/// название/текстовые пояснения какой-то характеристики
		/// </summary>
		abstract public class WithTextDescription
		{
			public WithTextDescription(string description) { Description = description; }
			private string _Description;
			/// <summary>
			/// название какой-то характеристики
			/// </summary>
			public string Description
			{
				get
				{
					if (_Description is null)
						_Description = "";
					return _Description;
				}
				set { _Description = value; }
			}
		}
		/// <summary>
		/// связанные с сущностью control-ы на форме
		/// </summary>
		abstract public class WithConnectedLabel
		{
			/// <summary>
			/// в какой контейнер выводить текстовые пояснения
			/// </summary>
			private Label _ConnectedLabel;
			/// <summary>
			/// в какой контейнер выводить текстовые пояснения
			/// </summary>
			public Label ConnectedLabel
			{
				get
				{
					if (_ConnectedLabel is null)
					{
						_ConnectedLabel = new Label();
						_ConnectedLabel.Text = "";
						_ConnectedLabel.AutoSize = true;
					}
					return _ConnectedLabel;
				}
				set
				{
					if (value is null)
					{
						if (!(_ConnectedLabel is null))
							_ConnectedLabel.Text = "";
						_ConnectedLabel?.Hide();
						//_ConnectedLabel?.Dispose();//этого делать не надо!
					}
					else
					{
						_ConnectedLabel = value;
						_ConnectedLabel?.Show();
					}
				}
			}
			protected void DisposeLabel()
			{
				_ConnectedLabel?.Dispose();
				_ConnectedLabel = null;
			}
			abstract public void UI_Show();
			abstract public void UI_Clear();
		}
		/// <summary>
		/// каждая характеристика (ранжирования, метода) имеет числовое значение и осмысленное наименование 
		/// </summary>
		public class Characteristic : WithTextDescription
		{
			public Characteristic(string label) : base(label) { }
			public Characteristic(string label, double value) : base(label) { Value = value; }
			private double _Value = INF;
			private List<double> _ValuesList;
			private double _ValueMin;
			private double _ValueMax;
			/// <summary>
			/// числовое значение хар.
			/// </summary>
			public double Value
			{
				get { return _Value; }
				set { _Value = value; }
			}
			/// <summary>
			/// векторное значение хар.
			/// </summary>
			public List<double> ValuesList
			{
				get
				{
					if (_ValuesList is null)
						_ValuesList = new List<double>();
					return _ValuesList;
				}
				set { _ValuesList = value; }
			}
			/// <summary>
			/// если есть список характеристик, в который данная хар. входит, то будут вычислены min и max
			/// </summary>
			public double ValueMin { get { return _ValueMin; } }
			/// <summary>
			/// если есть список характеристик, в который данная хар. входит, то будут вычислены min и max
			/// </summary>
			public double ValueMax { get { return _ValueMax; } }
			/// <summary>
			/// проверка значения характеристики на существование
			/// </summary>
			public bool IsInitializedValue()
			{
				return Math.Abs(Value) != INF;
			}
			/// <summary>
			/// проверка значения характеристики на существование
			/// </summary>
			public bool IsInitializedValuesList()
			{
				return ValuesList != null && ValuesList.Count > 0;
			}
			public static (double min, double max) CalcMinMax(List<double> valuelist)
			{
				double min = INF;
				double max = INF;
				List<double> L = valuelist.Where(x => Math.Abs(x) != INF).ToList();
				if (L.Count != 0)
				{
					min = valuelist.Min();
					max = valuelist.Max();
				}
				return (min, max);
			}
			public void SetMinMax(List<double> valuelist)
			{
				(_ValueMin, _ValueMax) = CalcMinMax(valuelist);
			}
			public void SumValuesListAndPutToValue()
			{
				Value = ValuesList.Sum();
			}
		}
		/// <summary>
		/// суммарное расстояние до всех остальных входных ранжирований
		/// </summary>
		public class CharacteristicDistance : WithTextDescription
		{
			public CharacteristicDistance(string description) : base(description)
			{
				modulus = new Characteristic(CH_DIST_MODULUS + description);
				square = new Characteristic(CH_DIST_SQUARE + description);
			}
			public Characteristic modulus;
			public Characteristic square;
		}
		public class CharacteristicRankingDistances
		{
			public CharacteristicRankingDistances(Method method)
			{
				parent_method = method;
				NonFuzzyRanking = new CharacteristicDistance(_CH_NON_FUZZY);
				FuzzyRankingBy_RAcycTr = new CharacteristicDistance(_CH_FUZZY_BY_RAcycTr);
			}
			private Method parent_method;
			public CharacteristicDistance NonFuzzyRanking;
			public CharacteristicDistance FuzzyRankingBy_RAcycTr;
			public CharacteristicDistance MethodDist
			{
				get
				{
					if (parent_method is null)
						return null;
					if (parent_method.ID == MET_SMERCHINSKAYA_YASHINA_METHOD)
						return FuzzyRankingBy_RAcycTr;
					else
						return NonFuzzyRanking;
				}
			}
		}
		/// <summary>
		/// характеристики совокупности ранжирований метода
		/// </summary>
		public class MethodRankingsCharacteristics
		{
			public MethodRankingsCharacteristics(Method m) { parent_method = m; }
			private readonly Method parent_method;
			private Characteristic _MinMaxCost;//мин и макс стоимость среди ранжирований метода
			private Characteristic _MinMaxStrength;//мин и макс сила среди ранжирований метода
			private CharacteristicRankingDistances _MinMaxDistance;//мин и макс суммарн. расстояние среди ранжирований метода
			private bool[] _IsInPareto_Cost;//входит ли ранжирование по индексу i в оптимальное множество по векторам экспертов-критериев
			private bool[] _IsInPareto_Strength;
			private bool[] _IsInPareto_DistRankModulus;
			private bool[] _IsInPareto_DistRankSquare;
			public Characteristic MinMaxCost
			{
				get
				{
					if (_MinMaxCost is null)
						_MinMaxCost = new Characteristic(CH_COST);
					if (Method.IsMethodExistWithRanks(parent_method))
					{
						_MinMaxCost.SetMinMax(parent_method.Rankings.Select(x => x.Cost.Value).ToList());
					}
					return _MinMaxCost;
				}
			}
			public Characteristic MinMaxStrength
			{
				get
				{
					if (_MinMaxStrength is null)
						_MinMaxStrength = new Characteristic(CH_STRENGTH);
					if (Method.IsMethodExistWithRanks(parent_method))
					{
						_MinMaxStrength.SetMinMax(parent_method.Rankings.Select(x => x.Strength.Value).ToList());
					}
					return _MinMaxStrength;
				}
			}
			/// <summary>
			/// мин и макс суммарн. расстояние среди ранжирований метода
			/// </summary>
			public CharacteristicRankingDistances MinMaxDistance
			{
				get
				{
					if (Method.IsMethodExistWithRanks(parent_method))
					{
						_MinMaxDistance = new CharacteristicRankingDistances(parent_method);
						_MinMaxDistance.NonFuzzyRanking.square.SetMinMax(
							parent_method.Rankings.Select(x => x.Distance.NonFuzzyRanking.square.Value).ToList()
							);
						_MinMaxDistance.NonFuzzyRanking.modulus.SetMinMax(
							parent_method.Rankings.Select(x => x.Distance.NonFuzzyRanking.modulus.Value).ToList()
							);
						_MinMaxDistance.FuzzyRankingBy_RAcycTr.square.SetMinMax(
							parent_method.Rankings.Select(x => x.Distance.FuzzyRankingBy_RAcycTr.square.Value).ToList()
							);
						_MinMaxDistance.FuzzyRankingBy_RAcycTr.modulus.SetMinMax(
							parent_method.Rankings.Select(x => x.Distance.FuzzyRankingBy_RAcycTr.modulus.Value).ToList()
							);
					}
					return _MinMaxDistance;
				}
			}
			public bool[] IsInPareto_Cost
			{
				get
				{
					IfNullThenUpdatePareto(ref
						_IsInPareto_Cost,
						parent_method.Rankings.Select(x => x.CostsExperts.ValuesList).ToList(),
						MAX_SIGN
						);
					return _IsInPareto_Cost;
				}
			}
			public bool[] IsInPareto_Strength
			{
				get
				{
					IfNullThenUpdatePareto(ref
						_IsInPareto_Strength,
						parent_method.Rankings.Select(x => x.StrengthsExperts.ValuesList).ToList(),
						MAX_SIGN
						);
					return _IsInPareto_Strength;
				}
			}
			public bool[] IsInPareto_DistRankModulus
			{
				get
				{
					IfNullThenUpdatePareto(ref
						_IsInPareto_DistRankModulus,
						parent_method.Rankings.Select(x => x.Distance.MethodDist.modulus.ValuesList).ToList(),
						MIN_SIGN);
					return _IsInPareto_DistRankModulus;
				}
			}
			public bool[] IsInPareto_DistRankSquare
			{
				get
				{
					IfNullThenUpdatePareto(ref
						_IsInPareto_DistRankSquare,
						parent_method.Rankings.Select(x => x.Distance.MethodDist.square.ValuesList).ToList(),
						MIN_SIGN);
					return _IsInPareto_DistRankSquare;
				}
			}
			private bool[] IfNullThenUpdatePareto(ref bool[] rankings_ch_which_canbe_null,
				List<List<double>> all_rankings_vectors, string min_or_max)
			{
				if (rankings_ch_which_canbe_null is null)
					rankings_ch_which_canbe_null = QualeEInParetoSet(all_rankings_vectors, min_or_max);
				return rankings_ch_which_canbe_null;
			}
			/// <summary>
			/// задаёт индексы ранжирований, входящих в Парето-множество
			/// </summary>
			/// <param name="vectors"></param>
			/// <param name="min_or_max">минимизирующий или максимизирющий критери: "min"/"max"</param>
			/// <returns></returns>
			private bool[] QualeEInParetoSet(List<List<double>> vectors, string min_or_max)
			{
				bool ParetoMORETHAN(List<double> R1, List<double> R2)
				{
					if (R1.Count != R2.Count)
						throw new MyException(EX_bad_dimensions);
					bool one_morethan = false;
					bool one_lessthan = false;
					for (int i = 0; i < R1.Count; i++)
					{
						if (OPS_Double.MORETHAN(R1[i], R2[i]))//R1[i] > R2[i]
							one_morethan = true;
						if (OPS_Double.MORETHAN(R2[i], R1[i]))//R1[i] < R2[i]
							one_lessthan = true;
					}
					if (one_morethan && !one_lessthan)
						return true;
					return false;
				}
				bool ParetoLESSTHAN(List<double> R1, List<double> R2)
				{
					return ParetoMORETHAN(R2, R1);
				}
				var r = vectors.Count;
				var isInPareto = new bool[r];
				for (int i = 0; i < r; i++)
				{
					isInPareto[i] = true;
					for (int j = 0; j < r; j++)
					{
						var Vj = vectors[j];
						var Vi = vectors[i];
						if (min_or_max == MIN_SIGN)
						{
							if (i != j && ParetoLESSTHAN(Vj, Vi))
							{
								isInPareto[i] = false;
								break;
							}
						}
						else
						{
							if (i != j && ParetoMORETHAN(Vj, Vi))
							{
								isInPareto[i] = false;
								break;
							}
						}
					}
				}
				return isInPareto;
			}
		}
		/// <summary>
		/// одно какое-то ранжирование (или путь) со всеми его свойствами
		/// </summary>
		public class Ranking
		{
			#region CONSTRUCTORS
			public Ranking(Method method, object rank)
			{
				parent_method = method;
				if (rank as List<int> != null)
					Rank2List = rank as List<int>;
				else if (rank as Ranking != null)
					Rank2List = (rank as Ranking)._Path;
			}
			#endregion CONSTRUCTORS

			#region FIELDS
			private List<int> _Path;//список вершин в пути-ранжировании
			public Method parent_method;//каким методом получено
			public Characteristic Cost;//общая стоимость пути
			public Characteristic CostsExperts; //вектор стоимостей по каждому эксперту-характеристике
			public Characteristic Strength; //сила пути (пропускная способность)
			public Characteristic StrengthsExperts; //вектор сил по каждому эксперту-характеристике
			public CharacteristicRankingDistances Distance;//расстояние входного ранжирования каждого эксперта и суммарное
			#endregion FIELDS

			#region PROPERTIES
			public List<int> Rank2List
			{
				get { return _Path; }
				set
				{
					_Path = value;
					UpdateRankingParams(AggregatedMatrix.R, ExpertRelations.Model.GetMatrices());
				}
			}
			/// <summary>
			/// создаёт матрицу смежности (порядок) из профиля эксперта
			/// ранжирование - как полный строгий порядок (полное, антирефлексивное, асимметричное, транзитивное)
			/// </summary>
			public Matrix Rank2AdjacencyMatrixTransitive
			{
				get
				{
					var node_list = Rank2List;
					var l = Rank2List.Count;
					if (l != node_list.Distinct().Count() || node_list.Max() >= n)
						throw new MyException(EX_bad_expert_profile);
					Matrix AM = Matrix.Zeros(n, n);//матрица смежности инициализирована нулями 
					for (int i = 0; i < l - 1; i++)
						for (int j = i + 1; j < l; j++)
						{
							var candidate1 = node_list[i];
							var candidate2 = node_list[j];
							AM[candidate1, candidate2] = 1;//левый лучше
						}
					return AM;
				}
			}
			/// <summary>
			/// создаёт не транзитивную матрицу смежности из профиля эксперта (просто обозначение дуг единицами)
			/// </summary>
			public Matrix Rank2AdjacencyMatrixNonTransitive
			{
				get
				{
					var node_list = Rank2List;
					var l = Rank2List.Count;
					if (l != node_list.Distinct().Count() || node_list.Max() >= n)
						throw new MyException(EX_bad_expert_profile);
					Matrix AM = Matrix.Zeros(n, n);//матрица смежности инициализирована нулями
					for (int i = 0; i < l - 1; i++)
					{
						var candidate1 = node_list[i];
						var candidate2 = node_list[i + 1];
						AM[candidate1, candidate2] = 1;//левый лучше
					}
					return AM;
				}
			}
			public string Rank2String
			{
				get
				{
					return string.Join(",", _Path.Select(x => ind2sym[x]).ToList());
				}
			}
			public int Count
			{
				get { return _Path.Count(); }
			}
			#endregion PROPERTIES

			#region FUNCTIONS		
			/// <summary>
			/// вычисление всех параметров ранжирования
			/// </summary>
			private void UpdateRankingParams(Matrix weight_matrix, List<Matrix> experts_matrices)
			{
				Cost = null;
				CostsExperts = null;
				Strength = null;
				StrengthsExperts = null;
				Distance = null;
				if (!(_Path is null))
				{
					Cost = new Characteristic(CH_COST + _CH_ON_R, PathCost(Rank2List, weight_matrix));
					Strength = new Characteristic(CH_STRENGTH + _CH_ON_R, PathStrength(Rank2List, weight_matrix));
					if (experts_matrices != null)
					{
						CostsExperts = new Characteristic(CH_COST + _CH_ON_EACH_EXPERT);
						StrengthsExperts = new Characteristic(CH_STRENGTH + _CH_ON_EACH_EXPERT);
						foreach (var expert_matrix in experts_matrices)
						{
							CostsExperts.ValuesList.Add(PathCost(Rank2List, expert_matrix));
							StrengthsExperts.ValuesList.Add(PathStrength(Rank2List, expert_matrix));
						}
						if (Rank2List.Count == n)
						{
							Distance = new CharacteristicRankingDistances(parent_method);
							var Rank_AdjTrans = Rank2AdjacencyMatrixTransitive;
							var Rank_AdjTransFuzzy_by_RAcyTr = Matrix.MultElementwise(Rank_AdjTrans, AggregatedMatrix.R.DestroyedCycles.TransClosured);
							foreach (var expert_matrix in experts_matrices)
							{
								Distance.NonFuzzyRanking.modulus.ValuesList.Add(
									Matrix.DistanceModulus(Rank_AdjTrans, expert_matrix));
								Distance.NonFuzzyRanking.square.ValuesList.Add(
									Matrix.DistanceSquare(Rank_AdjTrans, expert_matrix));
								Distance.FuzzyRankingBy_RAcycTr.modulus.ValuesList.Add(
									Matrix.DistanceModulus(Rank_AdjTransFuzzy_by_RAcyTr, expert_matrix));
								Distance.FuzzyRankingBy_RAcycTr.square.ValuesList.Add(
									Matrix.DistanceSquare(Rank_AdjTransFuzzy_by_RAcyTr, expert_matrix));
							}
							Distance.NonFuzzyRanking.modulus.SumValuesListAndPutToValue();
							Distance.NonFuzzyRanking.square.SumValuesListAndPutToValue();
							Distance.FuzzyRankingBy_RAcycTr.modulus.SumValuesListAndPutToValue();
							Distance.FuzzyRankingBy_RAcycTr.square.SumValuesListAndPutToValue();
						}
					}
				}
			}
			/// <summary>
			/// веса данного пути
			/// </summary>
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
			public static double PathCost(List<int> vertices_list, Matrix Weights_matrix)
			{
				return WeightsOfPath(vertices_list, Weights_matrix).Sum();
			}
			/// <summary>
			/// сила пути (пропускная способность)
			/// </summary>
			public static double PathStrength(List<int> vertices_list, Matrix Weights_matrix)
			{
				var wp = WeightsOfPath(vertices_list, Weights_matrix);
				return wp.Count == 0 ? INF : wp.Min();
			}
			/// <summary>
			/// создаёт ранжирование на основе матрицы
			/// алг. Демукрона, начиная с конца - со стока
			/// </summary>
			public static bool Matrix2RanksDemukron(Matrix M, out List<List<int>> levels, out List<List<int>> rankings)
			{
				levels = new List<List<int>>();
				rankings = new List<List<int>>();
				//если матрица не асимметричная, то она имеет цикл из двух вершин. Поэтому матрица будет асимметрична
				if (M.IsHasCycle(new double[] { NO_EDGE, INF }))
					return false;
				var AM = M.AdjacencyMatrix;//из нулей и единиц
				int n = AM.n;
				var wins = new double[n];//инициализирован нулями
				int mark = -1;//отметка о том, что этот уровень больше не рассматриваем
				while (true)
				{
					var last_level_vertices = new List<int>();
					for (int i = 0; i < n; i++)
					{
						if (wins[i] != mark)
						{
							wins[i] = 0;
							for (int j = 0; j < AM.m; j++)//сумма элементов строки
								wins[i] += AM[i, j];
						}
					}
					for (int i = 0; i < n; i++)
					{
						if (wins[i] == 0)
						{
							last_level_vertices.Add(i);
							wins[i] = mark;
							for (int k = 0; k < n; k++)//удалим все рёбра к нему
								AM[k, i] = 0;
						}
					}
					levels.Add(last_level_vertices);
					if (wins.Where(x => x == mark).Count() == n)
						break;
				}
				levels.Reverse();
				var levels_cnt = levels.Count;
				Queue<List<int>> Q = new Queue<List<int>>();
				foreach (var v in levels.First())
					Q.Enqueue(new List<int> { v });
				while (Q.Count > 0)
				{
					List<int> constructed_ranking = Q.Dequeue();
					int next_lvl_index = constructed_ranking.Count;
					if (next_lvl_index < levels_cnt)//последний возможный индекс уровня - это (n-1)
					{
						foreach (int v in levels[next_lvl_index])
						{
							Q.Enqueue(constructed_ranking.Append(v).ToList());
						}
					}
					else
					{
						rankings.Add(constructed_ranking);
					}
				}
				if (levels_cnt < n)
					return false;//если это не ранжирование, а разбиение на уровни
				else
					return true;
			}
			#endregion FUNCTIONS
		}
		/// <summary>
		/// для каждого метода существуют выдаваемые им ранжирования и др. атрибуты
		/// </summary>
		public class Method
		{
			public Method(int id) { ID = id; }

			#region FIELDS
			public int ID = -1;//обозначение метода
			private List<Ranking> _Rankings;//выдаваемые методом ранжирования
			private MethodRankingsCharacteristics _RankingsCharacteristics;//характеристики ранжирований
			private List<int> _UndominatedAlternatives;//победители - недоминируемые альтернативы
			private List<List<int>> _Levels;//разбиение графа отношения на уровни (алг. Демукрона, начиная с конца - со стока)
			public ConnectedControls UI_Controls;
			#endregion FIELDS

			#region SUBCLASSES
			/// <summary>
			/// связанные с методом элементы управления (control-ы) на форме
			/// </summary>
			public class ConnectedControls : WithConnectedLabel
			{
				public ConnectedControls(Method m, CheckBox checkBox, DataGridView dgv, Label lbl)
				{
					parent_method = m;
					ConnectedCheckBox = checkBox;
					ConnectedTableFrame = dgv;
					ConnectedLabel = lbl;
				}
				private readonly Method parent_method;
				private CheckBox connectedCheckBox;//чекбокс - будем ли запускать метод
				private DataGridView connectedTableFrame;//в какой контейнер выводить результаты работы метода
				public CheckBox ConnectedCheckBox
				{
					get { return connectedCheckBox; }
					set
					{
						connectedCheckBox = value;
						connectedCheckBox.Text = MethodName[parent_method.ID];
					}
				}
				public DataGridView ConnectedTableFrame
				{
					get { return connectedTableFrame; }
					set
					{
						connectedTableFrame = value;
						((GroupBox)connectedTableFrame?.Parent).Text = MethodName[parent_method.ID];
					}
				}
				/// <summary>
				/// запись в файл всех полученных ранжирований метода
				/// </summary>
				public void WriteRankingsToFile()
				{
					if (parent_method.HasRankings)
					{
						var text = string.Join(CR_LF + CR_LF,
								parent_method.Rankings
								.Where(x => x.Count == n)
								.Select(x => x.Rank2AdjacencyMatrixTransitive.Matrix2String(false)).ToArray());
						OPS_File.WriteToFile(text, OUT_FILE, true);
					}
				}
				private void SetRankingsToDataGridView()
				{
					if (!parent_method.IsExecute)
						return;

					string RowHeaderForRankingAndLevel(int i)
					{
						return $"Место {i + 1}";
					}

					SetDGVDefaults_methods(parent_method.UI_Controls.ConnectedTableFrame);

					if (!parent_method.HasRankings)
					{
						parent_method.UI_Controls.ConnectedLabel.Text = INF_ranking_unavailable;
						if (parent_method.HasLevels)
						{//ранжирований нет, но можно задать разбиение на уровни
							var col_headers = new string[] { $"Разбиение{CR_LF}на уровни" };
							var row_headers = Enumerable.Range(0, parent_method.Levels.Count)
								 .Select(x => RowHeaderForRankingAndLevel(x)).ToArray();
							AddDGVColumnsAndRows(parent_method.UI_Controls.ConnectedTableFrame,
								col_headers.Count(), row_headers.Count());
							SetDGVHeaders(parent_method.UI_Controls.ConnectedTableFrame,
								col_headers, row_headers);
							for (int i = 0; i < parent_method.Levels.Count; i++)
							{
								SetReadonlyCell(parent_method.UI_Controls.ConnectedTableFrame,
									i, 0, parent_method.Levels2Strings[i], Color.Empty);
							}
						}
					}
					else
					{
						//задать значение характеристики ранжирования и раскрасить
						void display_vector_characteristic(int i, int j, string min_or_max,
							Characteristic Characteristic)
						{
							string cell_text = "";
							Color cell_colour = output_characteristics_bg_color;
							if (Characteristic.IsInitializedValuesList())
							{
								cell_text += string.Join(CR_LF, Characteristic.ValuesList);
								if (min_or_max == MIN_SIGN)
									cell_colour = output_characteristics_min_color;
								else if (min_or_max == MAX_SIGN)
									cell_colour = output_characteristics_max_color;
							}
							SetReadonlyCell(parent_method.UI_Controls.ConnectedTableFrame,
								i, j, cell_text, cell_colour);
						}
						//задать значение характеристики ранжирования и раскрасить
						void display_scalar_characteristic(int i, int j, double min, double max,
							Characteristic Characteristic)
						{
							string cell_text = "";
							Color cell_colour = output_characteristics_bg_color;
							if (Characteristic.IsInitializedValue())
							{
								cell_text += Characteristic.Value.ToString();
								if (min < max)
								{
									if (OPS_Double.EQUALS(Characteristic.Value, min))
										cell_colour = output_characteristics_min_color;
									else if (OPS_Double.EQUALS(Characteristic.Value, max))
										cell_colour = output_characteristics_max_color;
								}
							}
							SetReadonlyCell(parent_method.UI_Controls.ConnectedTableFrame,
								i, j, cell_text, cell_colour);
						}

						Ranking some_rank = parent_method.Rankings.First();
						var col_headers = Enumerable.Range(0, parent_method.Rankings.Count)
								 .Select(x => $"Ранжиро-{CR_LF}вание {x + 1}").ToArray();
						var row_headers = Enumerable.Range(0, some_rank.Count)
								 .Select(x => RowHeaderForRankingAndLevel(x)).ToArray();
						AddDGVColumnsAndRows(parent_method.UI_Controls.ConnectedTableFrame,
								col_headers.Count(), row_headers.Count());
						SetDGVHeaders(parent_method.UI_Controls.ConnectedTableFrame,
							col_headers, row_headers);
						SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.Cost.Description);
						SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.Strength.Description);
						SetRow(parent_method.UI_Controls.ConnectedTableFrame, _CH_WHOLE_SUM +
							some_rank.Distance.MethodDist.square.Description);
						SetRow(parent_method.UI_Controls.ConnectedTableFrame, _CH_WHOLE_SUM +
							some_rank.Distance.MethodDist.modulus.Description);
						SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.CostsExperts.Description);
						SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.StrengthsExperts.Description);
						SetRow(parent_method.UI_Controls.ConnectedTableFrame, _CH_TO_EACH_EXPERT +
							some_rank.Distance.MethodDist.square.Description);
						SetRow(parent_method.UI_Controls.ConnectedTableFrame, _CH_TO_EACH_EXPERT +
							some_rank.Distance.MethodDist.modulus.Description);
						for (int j = 0; j < parent_method.Rankings.Count; j++)
						{
							Ranking rank = parent_method.Rankings[j];
							Color cell_colour = Color.Empty;
							if (Methods.MutualRankings.Contains(rank.Rank2String))
							{
								cell_colour = output_characteristics_mutual_color;
							}
							for (int i = 0; i < rank.Count; i++)
							{
								SetReadonlyCell(parent_method.UI_Controls.ConnectedTableFrame,
									i, j, ind2letter[rank.Rank2List[i]], cell_colour);
							}
							var N = rank.Count;
							var MethodCh = parent_method.RankingsCharacteristics;
							display_scalar_characteristic(N, j,
								MethodCh.MinMaxCost.ValueMin,
								MethodCh.MinMaxCost.ValueMax,
								parent_method.Rankings[j].Cost);
							display_scalar_characteristic(N + 1, j,
								MethodCh.MinMaxStrength.ValueMin,
								MethodCh.MinMaxStrength.ValueMax,
								parent_method.Rankings[j].Strength);
							display_scalar_characteristic(N + 2, j,
								MethodCh.MinMaxDistance.MethodDist.square.ValueMin,
								MethodCh.MinMaxDistance.MethodDist.square.ValueMax,
								parent_method.Rankings[j].Distance.MethodDist.square);
							display_scalar_characteristic(N + 3, j,
								MethodCh.MinMaxDistance.MethodDist.modulus.ValueMin,
								MethodCh.MinMaxDistance.MethodDist.modulus.ValueMax,
								parent_method.Rankings[j].Distance.MethodDist.modulus);
							display_vector_characteristic(N + 4, j,
								MethodCh.IsInPareto_Cost[j] ? MAX_SIGN : "",
								parent_method.Rankings[j].CostsExperts);
							display_vector_characteristic(N + 5, j,
								MethodCh.IsInPareto_Strength[j] ? MAX_SIGN : "",
								parent_method.Rankings[j].StrengthsExperts);
							display_vector_characteristic(N + 6, j,
								MethodCh.IsInPareto_DistRankSquare[j] ? MIN_SIGN : "",
								parent_method.Rankings[j].Distance.MethodDist.square);
							display_vector_characteristic(N + 7, j,
								MethodCh.IsInPareto_DistRankModulus[j] ? MIN_SIGN : "",
								parent_method.Rankings[j].Distance.MethodDist.modulus);
						}
					}
				}
				override public void UI_Show()
				{
					SetRankingsToDataGridView();
					ConnectedTableFrame?.Show();
					ConnectedTableFrame?.Parent.Show();
					ConnectedLabel.Show();
					ConnectedLabel.Text = parent_method.Info();
					ConnectedTableFrame?.Parent.Controls.Add(ConnectedLabel);
				}
				override public void UI_Clear()
				{
					ConnectedLabel = null;
					ConnectedTableFrame?.Rows.Clear();
					ConnectedTableFrame?.Columns.Clear();
					ConnectedTableFrame?.Hide();
					ConnectedTableFrame?.Parent?.Hide();
				}
			}
			#endregion SUBCLASSES

			#region PROPERTIES
			public bool IsExecute
			{
				get
				{
					return (UI_Controls.ConnectedCheckBox != null) ?
						UI_Controls.ConnectedCheckBox.Checked : false;
				}
			}
			public bool HasRankings
			{
				get
				{
					if (Rankings is null || Rankings.Count == 0)
						return false;
					return true;
				}
			}
			public bool HasLevels
			{
				get
				{
					if (Levels is null || Levels.Count == 0)
						return false;
					return true;
				}
			}
			public bool HasWinners
			{
				get
				{
					if (UndominatedAlternatives is null || UndominatedAlternatives.Count == 0)
						return false;
					return true;
				}
			}
			public List<string> Ranks2Strings
			{
				get { return this.Rankings.Select(x => x.Rank2String).ToList(); }
			}
			public List<Ranking> Rankings
			{
				get
				{
					if (_Rankings is null)
					{
						_Rankings = new List<Ranking>();
					}
					return _Rankings;
				}
				set { _Rankings = value; }
			}
			public MethodRankingsCharacteristics RankingsCharacteristics
			{
				get
				{
					if (_RankingsCharacteristics is null || Rankings is null || Rankings?.Count == 0)
					{
						_RankingsCharacteristics = new MethodRankingsCharacteristics(this);
					}
					return _RankingsCharacteristics;
				}
				set { _RankingsCharacteristics = value; }
			}
			public List<int> UndominatedAlternatives
			{
				get
				{
					if (_UndominatedAlternatives is null && Levels?.Count > 0)
						_UndominatedAlternatives = Levels?.First();
					return _UndominatedAlternatives;
				}
				set { _UndominatedAlternatives = value; }
			}
			public List<List<int>> Levels
			{
				get
				{
					if (_Levels is null)
						_Levels = new List<List<int>>();
					return _Levels;
				}
				set { _Levels = value; }
			}
			public string UndominatedAlternatives2String
			{
				get
				{
					if (UndominatedAlternatives is null)
						return "";
					return string.Join(",", UndominatedAlternatives?.Select(x => ind2letter[x]));
				}
			}
			public List<string> Levels2Strings
			{
				get
				{
					List<string> ans = new List<string>();
					foreach (var level in Levels)
					{
						ans.Add(string.Join(",", level.Select(x => ind2letter[x])));
					}
					return ans;
				}
			}
			#endregion PROPERTIES

			#region FUNCTIONS
			public static bool IsMethodExistWithRanks(Method met)
			{
				if (!(met is null) && met.HasRankings)
					return true;
				return false;
			}
			/// <summary>
			/// удаление ранжирований и их характеристик
			/// </summary>
			public void Clear()
			{
				Rankings = null;
				UndominatedAlternatives = null;
				Levels = null;
				RankingsCharacteristics = null; //очищено в свойстве
			}
			public string Info()
			{
				string text = "";
				string TextTemplateAmong(Characteristic ch)
				{
					return $"мин. и макс. {ch?.Description}: [{ch?.ValueMin}; {ch?.ValueMax}];{CR_LF}";
				}
				text += $"Недоминируемые альтернативы: {UndominatedAlternatives2String}{CR_LF}";
				text += $"Среди ранжирований метода: {CR_LF}";
				text += TextTemplateAmong(RankingsCharacteristics.MinMaxCost);
				text += TextTemplateAmong(RankingsCharacteristics.MinMaxStrength);
				text += TextTemplateAmong(RankingsCharacteristics.MinMaxDistance?.MethodDist.square);
				text += TextTemplateAmong(RankingsCharacteristics.MinMaxDistance?.MethodDist.modulus);
				return text;
			}
			#endregion FUNCTIONS
		}
		/// <summary>
		/// матрицы нечетких отношений экспертов
		/// </summary>
		public static class ExpertRelations
		{
			#region SUBCLASSES
			public class ConnectedControlsAndView : WithConnectedLabel
			{
				public ConnectedControlsAndView(CheckBox cb_show, CheckBox cb_doTransClos,
					NumericUpDown n, NumericUpDown m, FlowLayoutPanel flp)
				{
					connectedCheckBox_ToShow = cb_show;
					connectedCheckBox_DoTransClosure = cb_doTransClos;
					numericUpDown_n = n;
					numericUpDown_m = m;
					connectedFlowLayoutPanel = flp;
				}
				public readonly CheckBox connectedCheckBox_ToShow;//выводить ли таблички для ввода
				public readonly CheckBox connectedCheckBox_DoTransClosure;//делать ли транз. замыкание после ввода
				private readonly NumericUpDown numericUpDown_n;
				private readonly NumericUpDown numericUpDown_m;
				private readonly FlowLayoutPanel connectedFlowLayoutPanel;//куда кладутся все datagridview экспертов
				public Control.ControlCollection GetOutputControls
				{
					get
					{
						return connectedFlowLayoutPanel?.Controls;
					}
				}
				public List<DataGridView> GetOutputDataGridViews
				{
					get
					{
						return GetOutputControls.OfType<DataGridView>().ToList();
					}
				}
				public List<Label> GetOutputLabels
				{
					get
					{
						return GetOutputControls.OfType<Label>().ToList();
					}
				}
				public bool HasNoConnectedTables()
				{
					if (GetOutputDataGridViews is null || GetOutputDataGridViews.Count == 0)
						return true;
					return false;
				}
				public bool HasNoConnectedLabels()
				{
					if (GetOutputLabels is null || GetOutputLabels.Count == 0)
						return true;
					return false;
				}
				public bool HasNoConnectedOutputs()
				{
					if (HasNoConnectedTables() && HasNoConnectedLabels())
						return true;
					return false;
				}
				public void UpdateView_n_m()
				{
					if (UI_ControlsAndView.numericUpDown_n.Minimum <= n && n <= UI_ControlsAndView.numericUpDown_n.Maximum &&
						UI_ControlsAndView.numericUpDown_m.Minimum <= m && m <= UI_ControlsAndView.numericUpDown_m.Maximum)
					{
						UI_ControlsAndView.numericUpDown_n.Value = n;
						UI_ControlsAndView.numericUpDown_m.Value = m;
					}
				}
				public void UpdateModel_n_m()
				{
					if ((int)numericUpDown_n.Value > max_count_of_alternatives ||
						(int)numericUpDown_m.Value > max_count_of_experts)
						throw new MyException(EX_n_m_too_big);
					n = (int)numericUpDown_n.Value;
					m = (int)numericUpDown_m.Value;
				}
				/// <summary>
				/// что должно происходить при завершении редактирования ячейки
				/// </summary>
				public void CheckCellWhenValueChanged(object sender, DataGridViewCellEventArgs e)
				{
					try
					{
						var dd = sender as DataGridView;
						int exp_index = this.GetOutputDataGridViews.IndexOf(dd);
						int i = e.RowIndex;
						int j = e.ColumnIndex;
						double Mij, Mji;
						var p = double.TryParse(dd[j, i]?.Value?.ToString(), out Mij);
						double.TryParse(dd[i, j]?.Value?.ToString(), out Mji);
						if (!p || Mij < 0 || i == j)
						{
							Mij = 0.0;
						}
						//Mij = Mij > 1 ? 1.0 : Mij;
						if (Mij > 1)
						{
							double.TryParse($"0.{Math.Truncate(Mij)}", out Mij);
						}
						dd[j, i].Value = Mij;
						Model.CheckAndSetMatrixElement(exp_index, i, j, Mij);
					}
					catch (MyException ex) { ex.Info(); }
				}
				public DataGridView NewTable(Matrix new_martix)
				{
					if (new_martix is null)
						return null;
					DataGridView dgv = new DataGridView();
					SetDGVDefaults_experts(dgv);
					dgv.CellEndEdit += CheckCellWhenValueChanged;
					dgv.CellEndEdit += ColorSymmetricCell;
					SetNewMatrixToDGV(dgv, new_martix);
					var col_headers = Enumerable
						.Range(0, new_martix.m).Select(x => $"{ind2letter[x]}").ToArray();
					var row_headers = Enumerable
						.Range(0, new_martix.n).Select(x => $"{ind2letter[x]}").ToArray();
					SetDGVHeaders(dgv, col_headers, row_headers);
					return dgv;
				}
				public void UpdateTable(int expert_index)
				{
					if (HasNoConnectedOutputs())
						return;
					UpdateDGVCells(GetOutputDataGridViews[expert_index], Model.GetMatrix(expert_index));
					ColorSymmetricCells(GetOutputDataGridViews[expert_index]);
				}
				public void NewTables()
				{
					UI_Clear();
					UpdateView_n_m();
					if (connectedCheckBox_ToShow.Checked)
					{
						for (int ex = 0; ex < m; ex++)
						{
							var dgv = NewTable(Model.GetMatrix(ex));
							GetOutputControls.Add(dgv);
							ColorSymmetricCells((DataGridView)GetOutputControls[GetOutputControls.Count - 1]);
						}
					}
					UI_Show();
				}
				override public void UI_Show()
				{
					if (connectedCheckBox_ToShow.Checked)
					{
						foreach (DataGridView dgv in GetOutputDataGridViews)
						{
							dgv?.Parent.Show();
							dgv?.Show();
						}
						foreach (Label lbl in GetOutputLabels)
						{
							lbl?.Show();
						}
					}
				}
				override public void UI_Clear()
				{
					ConnectedLabel = null;
					foreach (DataGridView dgv in GetOutputDataGridViews)
					{
						dgv?.Hide();
						dgv?.Parent?.Hide();
						ClearDGV(dgv);
					}
					foreach (Label lbl in GetOutputLabels)
					{
						lbl?.Hide();
						//lbl.Text = "";
						//Dispose ne delat, vredno
					}
					GetOutputControls.Clear();
				}
				public void UI_Activate()
				{
					foreach (DataGridView dgv in GetOutputDataGridViews)
					{
						for (int i = 0; i < dgv.RowCount; i++)
						{
							for (int j = 0; j < dgv.ColumnCount; j++)
							{
								ColorCell(dgv, i, j, input_bg_color);
							}
						}
						dgv.ReadOnly = false;
					}
				}
				public void UI_Deactivate()
				{
					foreach (DataGridView dgv in GetOutputDataGridViews)
					{
						for (int i = 0; i < dgv.RowCount; i++)
						{
							for (int j = 0; j < dgv.ColumnCount; j++)
							{
								ColorCell(dgv, i, j, input_bg_color_disabled);
							}
						}
						dgv.ReadOnly = true;
					}
				}
				/// <summary>
				/// обновить рисунки графов - матриц экспертов
				/// </summary>
				/// <param name="sender"></param>
				/// <param name="e"></param>
				public void UpdateExpertGraphs()
				{
					var M = Model.GetMatrices();
					var pairs = new Dictionary<string, Matrix>(M.Count);
					for (int i = 0; i < M.Count; i++)
					{
						pairs.Add($"Expert{i}:", M[i]);
					}
					UpdateOrgraphPics(Form1.form3_input_expert_matrices, pairs);
				}
			}
			public class ExpertRelationsEventArgs : EventArgs
			{
				public ExpertRelationsEventArgs(int exp_ind, Matrix mat)
				{
					expert_index = exp_ind;
					fill_values = mat;
				}
				public ExpertRelationsEventArgs(int exp_ind, Matrix mat, List<Matrix> exp_mats)
				{
					expert_index = exp_ind;
					fill_values = mat;
					expert_matrices = exp_mats;
				}
				/// <summary>
				/// в какого эксперта
				/// </summary>
				public int expert_index { get; set; }
				/// <summary>
				/// что положить
				/// </summary>
				public Matrix fill_values { get; set; }
				/// <summary>
				/// что положить вообще все матрицы экспертов
				/// </summary>
				public List<Matrix> expert_matrices { get; set; }
				//public DataGridViewCellEventArgs cell_args { get; set; }
			}
			public static class Model
			{
				private static List<Matrix> _RList;
				private static FuzzyRelation CheckComparedAlternativesAndDoTransClosure(FuzzyRelation M)
				{
					var comparsions_cnt = new List<int>();//количество сравнений
					comparsions_cnt.Add(n - 1);
					for (int i = n - 2; i > 0; i--)
					{
						comparsions_cnt.Add(comparsions_cnt.Last() + i);
					}
					if (M.ComparedAlternatives().All(x => x == true) ||
						comparsions_cnt.Contains(M.EdgesCount(false, NO_EDGE)))
					{
						M = M.TransitiveClosure();
					}
					return M;
				}
				public static (bool is_norm, bool has_cycle) CheckMatrix(
					Matrix matrix, out FuzzyRelation new_matrix)
				{
					new_matrix = matrix.DeleteSolitaryLoops(out var solloops_cnt, NO_EDGE)
						.NormalizeElems(out bool is_norm).Cast2Fuzzy;
					bool has_cycle = new_matrix.IsHasCycle(NO_EDGE);
					if (!(new_matrix).IsTransitive() && UI_ControlsAndView.connectedCheckBox_DoTransClosure.Checked)
					{
						new_matrix = CheckComparedAlternativesAndDoTransClosure(new_matrix);
					}
					return (is_norm, has_cycle);
				}
				public static (bool some_not_norm, bool some_has_cycle) CheckMatrices(
					List<Matrix> matrices, out List<Matrix> new_matrices)
				{
					List<(bool is_norm, bool has_cycle)> answers = new List<(bool, bool)>();
					new_matrices = new List<Matrix>(matrices.Count);
					for (int k = 0; k < matrices.Count; k++)
					{
						answers.Add(CheckMatrix(matrices[k], out var M));
						new_matrices.Add(M);
					}
					return (answers.Any(x => !x.is_norm), answers.Any(x => x.has_cycle));
				}
				/// <summary>
				/// без обновления DataGridView
				/// </summary>
				/// <param name="matrix_index"></param>
				/// <param name="i"></param>
				/// <param name="j"></param>
				/// <param name="value"></param>
				private static void SetMatrixElement(int matrix_index, int i, int j, double value)
				{
					if (GetMatrix(matrix_index) is null)
						return;
					_RList[matrix_index][i, j] = value;
				}
				private static void SetMatrix(int matrix_index, Matrix new_matrix)
				{
					if (GetMatrix(matrix_index) is null)
						return;
					_RList[matrix_index] = new_matrix;
					UI_ControlsAndView.UpdateTable(matrix_index);
					UI_ControlsAndView.UpdateExpertGraphs();
				}
				public static void SetMatrices(List<Matrix> new_matrices)
				{
					_RList = new_matrices;
					UI_ControlsAndView.NewTables();
					UI_ControlsAndView.UpdateExpertGraphs();
				}
				public static Matrix GetMatrix(int index)
				{
					if (GetMatrices().Count == 0)
						return null;
					return GetMatrices()[index];
				}
				public static List<Matrix> GetMatrices()
				{
					if (_RList is null)
						_RList = new List<Matrix>();
					return new List<Matrix>(_RList);
				}
				/// <summary>
				/// только при вводе с таблицы руками - DataGridView не обновляются
				/// </summary>
				/// <param name="matrix_index"></param>
				/// <param name="i"></param>
				/// <param name="j"></param>
				/// <param name="value"></param>
				/// <param name="form3"></param>
				public static void CheckAndSetMatrixElement(int matrix_index, int i, int j, double value)
				{
					SetMatrixElement(matrix_index, i, j, value);
					CheckAndSetMatrix(matrix_index, _RList[matrix_index]);
				}
				public static void CheckAndSetMatrix(int matrix_index, Matrix matrix)
				{
					var ans = CheckMatrix(matrix, out var M);
					if (matrix != M)
						SetMatrix(matrix_index, M);
					try
					{
						if (!ans.is_norm)
						{
							throw new MyException(INF_matrix_was_normalized);
						}
					}
					catch (MyException ex) { ex.Info(); }
					try
					{
						if (ans.has_cycle)
						{
							throw new MyException(EX_contains_cycle);
						}
					}
					catch (MyException ex) { ex.Info(); }
				}
				public static void CheckAndSetMatrices(List<Matrix> matrices)
				{
					var ans = CheckMatrices(matrices, out var M);
					SetMatrices(M);
					try
					{
						if (ans.some_not_norm)
						{
							throw new MyException(INF_matrix_was_normalized);
						}
					}
					catch (MyException ex) { ex.Info(); }
					try
					{
						if (ans.some_has_cycle)
						{
							throw new MyException(EX_contains_cycle);
						}
					}
					catch (MyException ex) { ex.Info(); }
				}
			}
			#endregion SUBCLASSES

			public static ConnectedControlsAndView UI_ControlsAndView;
		}
		/// <summary>
		/// ResultRelation - агрегированная матрица матриц профилей
		/// </summary>
		public static class AggregatedMatrix
		{
			public class ConnectedControls : WithConnectedLabel
			{
				public ConnectedControls(RadioButton rb_square, RadioButton rb_modulus, Label lbl)
				{
					Connected_rb_dist_square = rb_square;
					Connected_rb_dist_modulus = rb_modulus;
					ConnectedLabel = lbl;
				}
				private RadioButton connected_rb_dist_square;
				private RadioButton connected_rb_dist_modulus;
				public RadioButton Connected_rb_dist_square
				{
					get
					{
						if (connected_rb_dist_square is null)
							throw new MyException(EX_choose_distance_func);
						return connected_rb_dist_square;
					}
					set { connected_rb_dist_square = value; }//set text
				}
				public RadioButton Connected_rb_dist_modulus
				{
					get
					{
						if (connected_rb_dist_square is null)
							throw new MyException(EX_choose_distance_func);
						return connected_rb_dist_modulus;
					}
					set { connected_rb_dist_modulus = value; }//set text
				}
				override public void UI_Show()
				{
					var tex = "";
					if (R != null)
					{
						foreach (var r in GetRelations2Show())
						{
							tex += $"{CR_LF}{r.Key}:{CR_LF}";
							tex += r.Value?.Matrix2String(true);
						}
					}
					ConnectedLabel.Text = tex;
					ConnectedLabel.Show();
				}
				override public void UI_Clear()
				{
					ConnectedLabel = null;
				}
			}
			public delegate void MyEventHandler();//сигнатура
			public static event MyEventHandler R_Changed;//для изменения картинки графа
			public static ConnectedControls UI_Controls;//связанные control-ы на форме
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
			public static void Set(List<Matrix> experts_relations)
			{
				Avg = Matrix.Average(experts_relations).Cast2Fuzzy;
				Med = Matrix.Median(experts_relations).Cast2Fuzzy;
				if (UI_Controls.Connected_rb_dist_square.Checked)
					R = Avg;
				else if (UI_Controls.Connected_rb_dist_modulus.Checked)
					R = Med;
				else
					throw new MyException(EX_choose_distance_func);
			}
			public static void Clear()
			{
				Avg = new FuzzyRelation(n);
				Med = new FuzzyRelation(n);
				R = new FuzzyRelation(n);
			}
			public static Dictionary<string, Matrix> GetRelations2Show()
			{
				Dictionary<string, Matrix> ans = new Dictionary<string, Matrix>
				{
					[RE_FullName_R] = R,
					[RE_FullName_R_Asym] = R.Asymmetric,
					[RE_FullName_R_Acyc] = R.DestroyedCycles,
					[RE_FullName_R_Acyc_Tr] = R.DestroyedCycles.TransClosured
				};
				return ans;
			}
		}
		/// <summary>
		/// все методы
		/// </summary>
		public static class Methods
		{
			public static Method All_various_rankings = new Method(MET_ALL_RANKINGS);
			public static Method All_Hamiltonian_paths = new Method(MET_ALL_HP);
			private static Method Hp_max_length = new Method(MET_HP_MAX_LENGTH);
			private static Method Hp_max_strength = new Method(MET_HP_MAX_STRENGTH);
			public static Method Schulze_method = new Method(MET_SCHULZE_METHOD);//имеет результирующее ранжирование по методу Шульце (единственно)
			public static Method Smerchinskaya_Yashina_method = new Method(MET_SMERCHINSKAYA_YASHINA_METHOD);
			public static List<string> MutualRankings;//ранжирования, которые принадлежат всем выбранным к выполнению (IsExecute) методам
			public static void UI_ShowMethods()
			{
				OPS_File.WriteToFile("", OUT_FILE, false);
				foreach (Method M in GetMethods())
				{
					if (M.IsExecute)
						M.UI_Controls.UI_Show();
				}
			}
			public static void UI_ClearMethods()
			{
				foreach (Method M in GetMethods())
				{
					M.UI_Controls.UI_Clear();
				}
				OPS_File.WriteToFile("", OUT_FILE, false);
			}
			/// <summary>
			/// очищает результаты методов и характеристики этих результатов
			/// </summary>
			public static void Clear()
			{
				foreach (Method M in GetMethods())
				{
					M.Clear();
				}
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
			/// создание всех возможных ранжирований данных альтернатив
			/// </summary>
			/// <returns></returns>
			public static void Set_All_various_rankings(int n)
			{
				All_various_rankings.Clear();
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
					All_various_rankings.Rankings.Add(new Ranking(All_various_rankings, new List<int> { 0 }));
				else if (n == 2)
				{
					All_various_rankings.Rankings.Add(new Ranking(All_various_rankings, new List<int> { 0, 1 }));
					All_various_rankings.Rankings.Add(new Ranking(All_various_rankings, new List<int> { 1, 0 }));
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
									All_various_rankings.Rankings.Add(new Ranking(All_various_rankings, r));
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
				All_Hamiltonian_paths.Clear();
				List<List<int>>[,] HP = Hamiltonian_paths_through_matrix_degree(weight_matrix, NO_EDGE);
				for (int i = 0; i < HP.GetLength(0); i++)
					for (int j = 0; j < HP.GetLength(1); j++)
						foreach (List<int> path_from_i_to_j in HP[i, j])
							All_Hamiltonian_paths.Rankings.Add(new Ranking(All_Hamiltonian_paths, path_from_i_to_j));
				Hp_max_length.Clear();
				Hp_max_strength.Clear();
				foreach (Ranking r in All_Hamiltonian_paths.Rankings)
				{
					if (r.Cost.Value == All_Hamiltonian_paths.RankingsCharacteristics.MinMaxCost.ValueMax)
						Hp_max_length.Rankings.Add(r);
					if (r.Strength.Value == All_Hamiltonian_paths.RankingsCharacteristics.MinMaxStrength.ValueMax)
						Hp_max_strength.Rankings.Add(r);
				}
				/// <summary>
				/// нахождение Гамильтоновых путей
				/// </summary>
				List<List<int>>[,] Hamiltonian_paths_through_matrix_degree(
					Matrix Weights_matrix, double no_edge_symbol)
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
						{
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
						}
						return ans;
					}

					Matrix Q_int = new Matrix(n, n);
					string[,] Q = new string[n, n];
					string[,] H = new string[n, n];
					for (int i = 0; i < n; i++)
					{
						for (int j = 0; j < n; j++)
						{
							if (!Weights_matrix.HasEdge((i, j), new double[] { no_edge_symbol, INF, -INF })
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
					{
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
								{
									Paths_matrix[i, j].Add(new List<int> { i, j });
								}
							}
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
				Schulze_method.Clear();
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
				Schulze_method.UndominatedAlternatives = Enumerable.Range(0, n).Where(i => winner[i] == true).ToList();//индексы победителей																								   
				var pair_dominant_matrix = new Matrix(PD);
				var is_ = Ranking.Matrix2RanksDemukron(pair_dominant_matrix, out var levels, out var ranks);
				Schulze_method.Levels = levels;
				if (is_)
				{
					foreach (var r in ranks)
						Schulze_method.Rankings.Add(new Ranking(Schulze_method, r));
				}
			}
			/// <summary>
			/// нахождение ранжирований из агрегированной матрицы - используется минимальное расстояние и разбиение контуров
			/// </summary>
			public static void Set_Smerchinskaya_Yashina_method()
			{
				Smerchinskaya_Yashina_method.Clear();
				Smerchinskaya_Yashina_method.UndominatedAlternatives = AggregatedMatrix.R.DestroyedCycles.TransClosured.UndominatedAlternatives().ToList();
				var is_ = Ranking.Matrix2RanksDemukron(AggregatedMatrix.R.DestroyedCycles.TransClosured, out var levels, out var ranks);
				Smerchinskaya_Yashina_method.Levels = levels;
				if (is_)
				{
					foreach (var rr in ranks)
						Smerchinskaya_Yashina_method.Rankings.Add(new Ranking(Smerchinskaya_Yashina_method, rr));
				}
			}
			/// <summary>
			/// запускает выполнение выбранных алгоритмов
			/// </summary>
			/// <param name="list_of_profiles"></param>
			public static List<string> ExecuteAlgorythms()
			{
				List<string> Intersect = new List<string>();//общие ранжирования для использованных методов
				try
				{
					if (ExpertRelations.Model.GetMatrices().Count == 0)
						throw new MyException(EX_bad_expert_profile);
					AggregatedMatrix.Set(ExpertRelations.Model.GetMatrices());
					var checkbuttons = GetMethods().Select(x => x.IsExecute);
					if (checkbuttons.All(x => x == false))
						throw new MyException(EX_choose_method);

					if (All_various_rankings.IsExecute)
						Set_All_various_rankings(n);
					if (All_Hamiltonian_paths.IsExecute)
						Set_All_Hamiltonian_paths(AggregatedMatrix.R);
					if (Schulze_method.IsExecute)
						Set_Schulze_method(n, AggregatedMatrix.R);
					if (Smerchinskaya_Yashina_method.IsExecute)
						Set_Smerchinskaya_Yashina_method();

					var methods_has_rankings = new List<Method>();
					foreach (Method m in GetMethods())
					{
						if (m.IsExecute && (m.HasRankings))
							methods_has_rankings.Add(m);
					}
					if (methods_has_rankings.Count() > 1)
					{
						bool processing_first_method = true;
						foreach (Method met in methods_has_rankings)
						{
							if (processing_first_method)
							{
								Intersect = met.Ranks2Strings;
								processing_first_method = false;
							}
							else
								Intersect = Enumerable.Intersect(Intersect, met.Ranks2Strings).ToList();
						}
					}
				}
				catch (MyException ex) { ex.Info(); }
				MutualRankings = Intersect;
				return Intersect;
			}
		}
	}
}
