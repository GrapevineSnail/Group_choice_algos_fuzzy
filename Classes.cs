using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Algorithms;
using static Group_choice_algos_fuzzy.Constants;
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

		private double[,] matrix = new double[,] { }; //"основа" матрицы - двумерный массив
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

		/// <summary>
		/// для удобства печати матриц
		/// </summary>
		/// <param name="Matrix"></param>
		/// <returns></returns>
		public static string Matrix2String(double[,] Matrix)
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
		public int GetLength(int dimension)
		{
			return matrix.GetLength(dimension);
		}
		/// <summary>
		/// возвращает транспонированую матрицу
		/// </summary>
		/// <returns></returns>
		public Matrix Transpose()
		{
			var R = new Matrix(this.n, this.m);
			for (int i = 0; i < R.n; i++)
				for (int j = 0; j < R.m; j++)
				{
					R[i, j] = this[j, i];
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
				throw new Exception(EX_matrix_not_square);
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
		/// <param name="M_list"></param>
		/// <returns></returns>
		public static Matrix Average(List<Matrix> M_list)
		{
			return Sum(M_list) / M_list.Count();
		}
		/// <summary>
		/// матрица из медианы для каждого элемента, из списка матриц
		/// </summary>
		/// <param name="M_list"></param>
		/// <returns></returns>
		public static Matrix Median(List<Matrix> M_list)
		{
			var R = Zeros(M_list.Last().n, M_list.Last().m);
			int med_index = M_list.Count / 2 + 1;
			for(int i =0; i < R.n; i++)
				for(int j = 0; j < R.m; j++)
				{
					var Rij_list = Enumerable.Select(M_list, x => x[i, j]).OrderBy(y => y).ToArray();
					R[i,j] = M_list.Count % 2 == 1 ? Rij_list[med_index]: 
						(Rij_list[med_index-1] + Rij_list[med_index])/2;
				}
			return R;
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
		/// расстояние между матрицами на основании выбранной функции расстояния для отдельных элементов
		/// </summary>
		/// <param name="M1"></param>
		/// <param name="M2"></param>
		/// <param name="elem_diff">функция расстояния для отдельных элементов</param>
		/// <returns></returns>
		private static double Distance(Matrix M1, Matrix M2, Func<double, double, double> elem_diff)
		{// вход: матрицы одинаковой размерности только из чисел \in [0;1]
			if (M1.n != M2.n || M1.m != M2.m)
				throw new ArgumentException(EX_matrix_multing_dim);
			double ans = 0;
			for (int i = 0; i < M1.n; i++)
				for (int j = 0; j < M1.m; j++)
				{
					if (M1[i, j] < 0 || M1[i, j] > 1 || M2[i, j] < 0 || M2[i, j] > 1)
						throw new ArgumentException(EX_bad_matrix);
					ans += elem_diff(M1[i, j], M2[i, j]);
				}
			return ans;
		}
		/// <summary>
		/// вычисляет расстояние из модулей разностей элементов (между двумя матрицами)
		/// </summary>
		/// <param name="M1"></param>
		/// <param name="M2"></param>
		/// <returns></returns>
		public static double DistanceModulus(Matrix M1, Matrix M2)
		{
			Func<double, double, double> f = (x, y) => Math.Abs(x - y);
			return Distance(M1, M2, f);
		}
		/// <summary>
		 /// вычисляет расстояние из квадратов разностей элементов (между двумя матрицами)
		 /// </summary>
		 /// <param name="M1"></param>
		 /// <param name="M2"></param>
		 /// <returns></returns>
		public static double DistanceSquare(Matrix M1, Matrix M2)
		{
			Func<double, double, double> f = (x, y) => Math.Pow(x - y, 2);
			return Distance(M1, M2, f);
		}
		/// <summary>
		/// вычисляет евклидово расстояние между двумя матрицами
		/// </summary>
		/// <param name="M1"></param>
		/// <param name="M2"></param>
		/// <returns></returns>
		public static double DistanceEuclidean(Matrix M1, Matrix M2)
		{
			return Math.Sqrt(DistanceSquare(M1, M2));
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
		/// <summary>
		/// из любой матрицы весов делает матрицу смежности
		/// </summary>
		/// <param name="M"></param>
		/// <returns></returns>
		public static Matrix MakeAdjacencyMatrix(Matrix M)
		{
			Matrix R = new Matrix(M.n, M.m);
			for (int i = 0; i < M.n; i++)
				for (int j = 0; j < M.m; j++)
				{
					if (Math.Abs(M[i, j]) == INF)// && Math.Abs(M[i, j]) == 0)
						R[i, j] = 0;
					else
						R[i, j] = 1;
				}
			return R;
		}
	}

	public class Fuzzy
	{
		/* public class FuzzySet
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
		*/
		public class FuzzyRelation : Matrix
		{// base - матрица нечёткого бинарного отношения
		 //полагаем квадратными
			public FuzzyRelation() : base() { }
			public FuzzyRelation(int n) : base(n, n) { }
			public FuzzyRelation(double[,] M) : base(M)
			{
				if (M.GetLength(0) != M.GetLength(1))
					throw new Exception(EX_matrix_not_square);
			}
			public FuzzyRelation(Matrix M) : base(M)
			{
				if (M.GetLength(0) != M.GetLength(1))
					throw new Exception(EX_matrix_not_square);
			}

			public double this[Tuple<int, int> pair]
			{
				get { return base[pair.Item1, pair.Item2]; }
				set { base[pair.Item1, pair.Item2] = value; }
			}
			/// <summary>
			/// матрица принадлежности (= м. предпочтений, функция принадлжености)
			/// </summary>
			public Matrix MembershipFunction
			{
				get { return base.Self; }
			}
			/// <summary>
			/// все элементы (пары), в том числе с принадлежностью 0
			/// </summary>
			/// <returns></returns>
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
				foreach (var x in Elements().Union(other.Elements()))
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
				foreach (var x in Elements().Union(other.Elements()))
					S[x] = Math.Max(this[x], other[x]);
				return S;
			}
			/// <summary>
			/// симметрическая разность
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public FuzzyRelation SetMinus1(FuzzyRelation other)//A \ B
			{
				var S = new FuzzyRelation();
				foreach (var x in Elements().Union(other.Elements()))
					S[x] = Math.Min(this[x], 1 - other[x]);
				return S;
			}
			/// <summary>
			/// симметрическая разность
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public FuzzyRelation SetMinus2(FuzzyRelation other)//A \ B
			{
				var S = new FuzzyRelation();
				foreach (var x in Elements().Union(other.Elements()))
					S[x] = Math.Max(this[x] - other[x], 0);
				return S;
			}
			/// <summary>
			/// обратное отношение
			/// </summary>
			/// <returns></returns>
			public FuzzyRelation Inverse()
			{
				return new FuzzyRelation(this.Transpose());
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
					for (int j = 0; j < this.m; j++)
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
			/// <summary>
			/// возвращает матрицу четкого отношения из нечёткого
			/// </summary>
			/// <param name="alpha">уровень, на котором отсекаем принадлежность (\mu(x)>alpha => 1, иначе 0)</param>
			/// <returns></returns>
			public Matrix DefuzzyfyingAlphaSlice(double alpha)
			{
				var R = new Matrix(this.n, this.m);
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
						R[i, j] = this[i, j] >= alpha ? 1 : 0;
				return R;
			}
		}
	}

	/// <summary>
	/// одно какое-то ранжирование (или путь) со всеми его свойствами
	/// </summary>
	public class Ranking
	{
		/// <summary>
		///каждая характеристика ранжирования имеет числовое значение и осмысленное наименование 
		/// </summary>
		public class Characteristic
		{
			public Characteristic(string label) { Label = label; }
			public double Value;
			public string Label;
		}
		#region CONSTRUCTORS
		public Ranking(int[] rank) { Rank2Array = rank; }
		public Ranking(List<int> rank) { Rank2List = rank; }
		public Ranking(int methodID, object rank)
		{
			MethodID = methodID;
			if (rank as List<int> != null)
				Rank2List = rank as List<int>;
			else if (rank as int[] != null)
				Rank2Array = rank as int[];
		}
		#endregion CONSTRUCTORS

		public int MethodID;//каким методом получено
		private List<int> Path = new List<int>();//список вершин в пути-ранжировании
		public Characteristic PathLength = new Characteristic("Стоимость"); //общая стоимость пути
		public Characteristic PathStrength = new Characteristic("Сила"); //сила пути (пропускная способность)
		public Characteristic PathSummaryDistance_modulus = new Characteristic(
			"Сумм. расстояние 'модуль разности'"); //суммарное расстояние до всех остальных входных ранжирований
		public Characteristic PathSummaryDistance_square = new Characteristic(
			"Сумм. расстояние 'квадрат разности'"); //суммарное расстояние до всех остальных входных ранжирований


		#region PROPERTIES
		public List<int> Rank2List
		{
			set
			{
				Path = value;
				SetRankingParams(R_list, R);
			}
			get { return Path; }
		}
		public int[] Rank2Array
		{
			set
			{
				Path = value.ToList();
				SetRankingParams(R_list, R);
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
				if (l != Enumerable.Distinct(single_profile).ToArray().Length)
					throw new ArgumentException(EX_bad_expert_profile);
				Matrix Rj = new Matrix(l, l);
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
				return Rj; //полный строгий порядок (антиреф. асимм. транзитивное)	
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
		#endregion PROPERTIES

		/// <summary>
		/// a0a1a2 -> 0, 1, 2
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public List<int> String2List(string s)
		{
			return s.Split(mark).ToList()
				.Where(x => int.TryParse(x, out var _))
				.Select(x => int.Parse(x)).ToList();
		}
		/// <summary>
		/// вычисление всех параметров ранжирования
		/// </summary>
		private void SetRankingParams(List<Matrix> other_matrices, Matrix weight_matrix)
		{
			if (Path != null)
			{
				if (Path.Count == 0)
				{
					PathLength.Value = INF;
					PathStrength.Value = INF;
					PathSummaryDistance_modulus.Value = INF;
					PathSummaryDistance_square.Value = INF;
				}
				else
				{
					PathLength.Value = path_cost(Rank2List, weight_matrix);
					PathStrength.Value = path_strength(Rank2List, weight_matrix);
					PathSummaryDistance_modulus.Value = Rank2Matrix.SumDistance(other_matrices, Matrix.DistanceModulus);
					PathSummaryDistance_square.Value = Rank2Matrix.SumDistance(other_matrices, Matrix.DistanceSquare);
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
		public Method(int id)
		{
			ID = id;
			Rankings = new List<Ranking>();
		}
		public int ID;//обозначение метода
		private CheckBox connectedCheckBox = null;//чекбокс - будем ли запускать метод
		public DataGridView connectedFrame = null;//в какой контейнер выводить результаты работы метода
		private Label connectedLabel = null;//в какой контейнер выводить текстовые пояснения к методу
		public List<Ranking> Rankings = null;//выдаваемые методом ранжирования
		public double MinLength;//минимальная длина среди ранжирований метода
		public double MaxLength;//максимальная длина среди ранжирований метода
		public double MinStrength;//минимальная сила среди ранжирований метода
		public double MaxStrength;//максимальная сила среди ранжирований метода
		//минимальное и максимальное суммарные расстояние среди ранжирований метода
		public double MinDistance_modulus;
		public double MaxDistance_modulus;
		public double MinDistance_square;
		public double MaxDistance_square;
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
		/// задаёт связанные с методом элементы управления
		/// </summary>
		/// <param name="checkBox"></param>
		/// <param name="frame"></param>
		public void SetConnectedControls(CheckBox checkBox, DataGridView frame)
		{
			connectedCheckBox = checkBox;
			connectedFrame = frame;
		}
		/// <summary>
		/// удаление ранжирований и их характеристик
		/// </summary>
		public void ClearRankings()
		{
			Rankings = new List<Ranking>();
			MinLength = INF;
			MaxLength = INF;
			MinStrength = INF;
			MaxStrength = INF;
			MinDistance_modulus = INF;
			MaxDistance_modulus = INF;
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
				return INF;
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
				return INF;
			return Enumerable.Max(L);
		}
		/// <summary>
		/// находит минимумы и максимумы по каждой характеристике ранжирований
		/// </summary>
		public void SetCharacteristicsMinsMaxes()
		{
			MinLength = min(Rankings.Select(x => x.PathLength.Value).ToList());
			MaxLength = max(Rankings.Select(x => x.PathLength.Value).ToList());
			MinStrength = min(Rankings.Select(x => x.PathStrength.Value).ToList());
			MaxStrength = max(Rankings.Select(x => x.PathStrength.Value).ToList());
			MinDistance_modulus = min(Rankings.Select(x => x.PathSummaryDistance_modulus.Value).ToList());
			MaxDistance_modulus = max(Rankings.Select(x => x.PathSummaryDistance_modulus.Value).ToList());
			MinDistance_square = min(Rankings.Select(x => x.PathSummaryDistance_square.Value).ToList());
			MaxDistance_square = max(Rankings.Select(x => x.PathSummaryDistance_square.Value).ToList());
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
	public static class Methods
	{
		public static Method All_various_rankings = new Method(ALL_RANKINGS);
		public static Method All_Hamiltonian_paths = new Method(ALL_HP);
		public static Method Hp_max_length = new Method(HP_MAX_LENGTH);
		public static Method Hp_max_strength = new Method(HP_MAX_STRENGTH);
		public static Method Schulze_method = new Method(SCHULZE_METHOD);//имеет результирующее ранжирование по методу Шульце (единственно)

		public static List<int> SchulzeWinners = null;//победители по методу Шульце

		public static double MinSummaryModulusDistance;//расстояние наиближайшего ко всем агрегированного ранжирования (модули) - лин. медианы
		public static double MinSummarySquareDistance;//расстояние наиближайшего ко всем агрегированного ранжирования (квадраты)
		public static double MaxHamPathLength;//длина пути длиннейших Гаммильтоновых путей
		public static double MaxHamPathStrength;//сила пути сильнейших Гаммильтоновых путей

		/// <summary>
		/// очищает результаты методов и характеристики этих результатов
		/// </summary>
		public static void ClearMethods()
		{
			foreach (Method M in GetMethods())
				M.ClearRankings();
			MinSummaryModulusDistance = 0;
			MinSummarySquareDistance = 0;
			MaxHamPathLength = 0;
			MaxHamPathStrength = 0;
			SchulzeWinners = null;
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
		/// выдаёт все методы, имеющие ранжирования и отмеченные к выполнению в текущей программе
		/// </summary>
		/// <returns></returns>
		public static List<Method> GetMethodsExecutedWhithResult() {
			var is_rankings_of_method_exists = new List<Method>();
			foreach(Method m in Methods.GetMethods())
			{
				if (m.IsExecute && m.Rankings != null && m.Rankings.Count > 0)
					is_rankings_of_method_exists.Add(m);
			}
			return is_rankings_of_method_exists;
		}

		/// <summary>
		/// создание всех возможных ранжирований данных альтернатив
		/// </summary>
		/// <returns></returns>
		public static void Set_All_various_rankings(int n)
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
		public static void Set_All_Hamiltonian_paths(Matrix weight_matrix)
		{
			List<List<int>>[,] HP = Hamiltonian_paths_through_matrix_degree(weight_matrix);
			All_Hamiltonian_paths.ClearRankings();
			for (int i = 0; i < HP.GetLength(0); i++)
				for (int j = 0; j < HP.GetLength(1); j++)
					foreach (List<int> r in HP[i, j])
						All_Hamiltonian_paths.Rankings.Add(new Ranking(ALL_HP, r));
		}

		/// <summary>
		/// Гамильтоновы пути максимальной длины
		/// </summary>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		public static void Set_Hp_max_length(Matrix weight_matrix)
		{
			if (All_Hamiltonian_paths.Rankings.Count == 0)
				Set_All_Hamiltonian_paths(weight_matrix);
			MaxHamPathLength = All_Hamiltonian_paths.Rankings.Select(x => x.PathLength.Value).Max();
			Hp_max_length.ClearRankings();
			foreach (Ranking r in All_Hamiltonian_paths.Rankings)
				if (r.PathLength.Value == MaxHamPathLength)
					Hp_max_length.Rankings.Add(r);
		}

		/// <summary>
		/// Гамильтоновы пути наибольшей силы
		/// </summary>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		public static void Set_Hp_max_strength(Matrix weight_matrix)
		{
			if (All_Hamiltonian_paths.Rankings.Count == 0)
				Set_All_Hamiltonian_paths(weight_matrix);
			MaxHamPathStrength = Enumerable.Select(All_Hamiltonian_paths.Rankings, x => x.PathStrength.Value).Max();
			Hp_max_strength.ClearRankings();
			foreach (Ranking r in All_Hamiltonian_paths.Rankings)
				if (r.PathStrength.Value == MaxHamPathStrength)
					Hp_max_strength.Rankings.Add(r);
		}

		/// <summary>
		/// Нахождение ранжирования и победителей методом Шульце
		/// </summary>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		public static void Set_Schulze_method(int n, Matrix weight_matrix)
		{
			double[,] PD = new double[n, n];//strength of the strongest path from alternative i to alternative j
			int[,] pred = new int[n, n];//is the predecessor of alternative j in the strongest path from alternative i to alternative j
			List<(int, int)> O = new List<(int, int)>();
			bool[] winner = Enumerable.Repeat(false, n).ToArray();

			//initialization
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
				{
					PD[i, j] = weight_matrix[i, j];
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
	/* def all_simple_paths_betweenAB(Adjacency_list, idA, idB):
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
