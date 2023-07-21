using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Algorithms;
using static Group_choice_algos_fuzzy.Form1;


namespace Group_choice_algos_fuzzy
{
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
				for (int j = 0; j < M1.m; j++) // почему j = i+1??
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
		{// вход: матрицы одинаковой размерности только из чисел \in [0;1]
			for (int i = 0; i < M1.n; i++)
				for (int j = 0; j < M1.m; j++)
					if (M1[i, j] < 0 || M1[i, j] > 1 || M2[i, j] < 0 || M2[i, j] > 1)
						throw new ArgumentException("Некорректные матрицы");
			return DistanceModulus(M1, M2);
		}
		/// <summary>
		/// суммарное расстояние от заданной матрицы до всех остальных матриц из списка
		/// </summary>
		public double SumDistance(List<Matrix> List_of_other_R, Func<Matrix, Matrix, double> distance_function)
		{
			// параметром - список матриц смежности
			double sum_dist = 0;
			foreach (Matrix other_R in List_of_other_R)
				sum_dist += distance_function(this, other_R);
			return sum_dist;
		}
	}

	public class Fuzzy
	{
		public class FuzzySet
		{
			public FuzzySet() { membershipFunction = new Dictionary<object, double>(); }
			public FuzzySet(Dictionary<object, double> func) { membershipFunction = func; }
			private Dictionary<object, double> membershipFunction;//функция принадлжености
			public double this[object i]
			{
				get
				{
					if (!membershipFunction.Keys.Contains(i))
						membershipFunction[i] = 0;
					return membershipFunction[i];
				}
				set { membershipFunction[i] = value; }
			}
			/// <summary>
			/// все элементы, в том числе с принадлежностью 0
			/// </summary>
			/// <returns></returns>
			public List<object> Elements()
			{
				return membershipFunction.Keys.ToList();
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
				foreach (var x in Elements().Union(other.Elements()))
					S[x] = Math.Min(this[x], other[x]);
				return S;
			}
			public FuzzySet Union(FuzzySet other)//A U B
			{
				var S = new FuzzySet();
				foreach (var x in Elements().Union(other.Elements()))
					S[x] = Math.Max(this[x], other[x]);
				return S;
			}
			public FuzzySet SetMinus(FuzzySet other)//A \ B
			{
				var S = new FuzzySet();
				foreach (var x in Elements().Union(other.Elements()))
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
		public double PathSummaryDistance;//суммарное расстояние до всех остальных входных ранжирований
		public List<int> Path = new List<int>();//список вершин в пути-ранжировании
		public List<int> Rank2List
		{
			set
			{
				Path = value;
				SetRankingParams();
			}
			get { return Path; }
		}
		public int[] Rank2Array
		{
			set
			{
				Path = value.ToList();
				SetRankingParams();
			}
			get { return Path.ToArray(); }
		}
		/// <summary>
		/// создаёт матрицу смежности (порядок) из профиля эксперта
		/// </summary>
		public Matrix Rank2Matrix
		{
			get
			{
				var single_profile = Rank2Array;
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
		}
		public string Rank2String
		{
			get
			{
				return string.Join("", Path.Select(x => ind2sym[x]).ToList());
			}
		}
		public int Count
		{
			get { return Path.Count; }
		}


		public List<int> String2List(string s)
		{
			return s.Split(mark).ToList()
				.Where(x => int.TryParse(x, out var _))
				.Select(x => int.Parse(x)).ToList();
		}
		private void SetRankingParams()
		{
			if (Path != null)
			{
				if (Path.Count == 0)
				{
					PathLength = INF;
					PathStrength = INF;
					PathSummaryDistance = INF;
				}
				else
				{
					PathLength = path_length(Rank2List, C);
					PathStrength = path_strength(Rank2List, C);
					PathSummaryDistance = Rank2Matrix.SumDistance(R_list, Matrix.DistanceHamming);
				}
			}
		}
		public override int GetHashCode() => Path.GetHashCode();
	}

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
		public double LengthsMin;//минимальная длина среди ранжирований метода
		public double LengthsMax;//максимальная длина среди ранжирований метода
		public double StrengthsMin;//минимальная сила среди ранжирований метода
		public double StrengthsMax;//максимальная сила среди ранжирований метода
		public double DistancesMin;//минимальное суммарное расстояние среди ранжирований метода
		public double DistancesMax;//максимальное суммарное расстояние среди ранжирований метода
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
		/// <summary>
		/// удаление ранжирований и их характеристик
		/// </summary>
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
			DistancesMin = min(Rankings.Select(x => x.PathSummaryDistance).ToList());
			DistancesMax = max(Rankings.Select(x => x.PathSummaryDistance).ToList());
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
			MinHammingDistance = All_various_rankings.Rankings.Select(x => x.PathSummaryDistance).Min();
			Linear_medians.ClearRankings();
			foreach (Ranking r in All_various_rankings.Rankings)
				if (r.PathSummaryDistance == MinHammingDistance)
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


}
