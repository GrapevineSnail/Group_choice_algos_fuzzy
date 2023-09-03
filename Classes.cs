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
	/// <summary>
	/// матрицы
	/// </summary>
	public class Matrix
	{
		public Matrix(int n) { matrix_base = new double[n, n]; }
		public Matrix(int n, int m) { matrix_base = new double[n, m]; }
		public Matrix(double[,] M) { matrix_base = (double[,])M.Clone(); }
		public Matrix(int[,] M)
		{
			matrix_base = new double[M.GetLength(0), M.GetLength(1)];
			for (int i = 0; i < this.n; i++)
				for (int j = 0; j < this.m; j++)
					matrix_base[i, j] = M[i, j];
		}
		public Matrix(Matrix M) { matrix_base = (double[,])M.matrix_base.Clone(); }

		public double[,] matrix_base = new double[,] { }; //"основа" матрицы - двумерный массив
		public double this[int i, int j]
		{
			get { return matrix_base[i, j]; }
			set { matrix_base[i, j] = value; }
		}
		/// <summary>
		/// количество строк матрицы
		/// </summary>
		public int n
		{
			get { return matrix_base.GetLength(0); }
		}
		/// <summary>
		/// количество столбцов матрицы
		/// </summary>
		public int m
		{
			get { return matrix_base.GetLength(1); }
		}
		public Matrix Self { get { return this; } }
		/// <summary>
		/// из любой матрицы весов достаёт её асимметричную часть
		/// </summary>
		/// <param name="M"></param>
		/// <returns></returns>
		public Matrix AsymmetricPart
		{//только для квадратных
			get
			{
				if (n != m)
					throw new MyException(EX_matrix_not_square);
				Matrix ans = new Matrix(n);
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
					{// выдаёт 0.099999999999999978 вместо 0.1
						ans[i, j] = Math.Max(Math.Round(this[i, j] - this[j, i], DIGITS_PRECISION), 0);
					}
				return ans;
			}
		}
		/// <summary>
		/// из любой матрицы весов достаёт её симметричную часть
		/// </summary>
		/// <param name="M"></param>
		/// <returns></returns>
		public Matrix SymmetricPart
		{//только для квадратных
			get
			{
				if (n != m)
					throw new MyException(EX_matrix_not_square);
				Matrix Sym = new Matrix(n);
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
						Sym[i, j] = Math.Min(this[i, j], this[j, i]);
				return Sym;
			}
		}
		/// <summary>
		/// из любой матрицы весов создаёт матрицу смежности
		/// </summary>
		/// <param name="M"></param>
		/// <returns></returns>
		public Matrix AdjacencyMatrix
		{
			get
			{
				Matrix R = new Matrix(n, m);
				for (int i = 0; i < n; i++)
					for (int j = 0; j < m; j++)
						R[i, j] = (Math.Abs(this[i, j]) == INF || Math.Abs(this[i, j]) == 0) ? 0 : 1;
				return R;
			}
		}
		/// <summary>
		/// превращает матрицу в нечёткое отношение
		/// </summary>
		/// <returns></returns>
		public FuzzyRelation ToFuzzy { get { return new FuzzyRelation(this); } }
		public static Matrix operator *(double c, Matrix R1)
		{
			var R = new Matrix(R1);
			for (int i = 0; i < R.n; i++)
				for (int j = 0; j < R.m; j++)
					R[i, j] = Math.Round(c * R[i, j], DIGITS_PRECISION);
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
					R[i, j] = Math.Round(a_ij, DIGITS_PRECISION);
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
					R[i, j] = Math.Round(R[i, j] + R2[i, j], DIGITS_PRECISION);
			return R;
		}
		public static Matrix operator -(Matrix R1, Matrix R2)
		{
			return R1 + (-1) * R2;
		}
		public static bool operator !=(Matrix R1, Matrix R2)
		{
			if (R1 is null || R2 is null)
				return true;
			if (R1.n != R2.n || R1.m != R2.m)
				return true;
			for (int i = 0; i < R1.n; i++)
				for (int j = 0; j < R1.m; j++)
				{
					if (R1[i, j] != R2[i, j])
						return true;
				}
			return false;//если одинаковые размерности и все элементы
		}
		public static bool operator ==(Matrix R1, Matrix R2)
		{
			return !(R1 != R2);
		}

		/// <summary>
		/// выводит список смежности матрицы на основании того, 
		/// какое значение элемента матрицы считать отсутствием ребра
		/// </summary>
		/// <param name="condition_of_no_edge_symbol">
		/// какое значение элемента матрицы считать отсутствием ребра - условие, лямбда-функция
		/// </param>
		/// <returns></returns>
		public List<List<int>> AdjacencyList(Func<double, bool> condition_of_no_edge_symbol)
		{
			var ans = new List<List<int>>();
			for (int i = 0; i < n; i++)
			{
				ans.Add(new List<int>());
				for (int j = 0; j < m; j++)
					if (!condition_of_no_edge_symbol(this[i, j]))
						ans[i].Add(j);
			}
			return ans;
		}
		/// <summary>
		/// выводит список смежности матрицы на основаноо того, какое значение элемента матрицы считать отсутствием ребра
		/// </summary>
		/// <param name="no_edge_symbol">
		/// какое значение элемента матрицы считать отсутствием ребра (0, INF, -INF и т.д.)
		/// </param>
		/// <returns></returns>
		public List<List<int>> AdjacencyList(double no_edge_symbol)
		{
			return AdjacencyList(x => x == no_edge_symbol);
		}
		/// <summary>
		///  для удобства печати матриц
		/// </summary>
		/// <param name="use_separator"></param>
		/// <returns></returns>
		public string Matrix2String(bool use_separator)
		{
			/// удаляет последние cnt символов из строки
			string trim_end(string s, int cnt)
			{
				var start = s.Length - cnt;
				if (start < 0)
					return "";
				return s.Remove(s.Length - cnt, cnt);
			}
			int[] max_widths = new int[m];
			for (int j = 0; j < m; j++)
				max_widths[j] = 5;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					if (this[i, j].ToString().Length > max_widths[j])
						max_widths[j] = this[i, j].ToString().Length;
			var str = "";
			var lef_bnd = "";
			var rig_bnd = "";
			if (use_separator)
			{
				lef_bnd = "[";
				rig_bnd = "]";
			}
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					//var fill = "_";
					//var align = "^";
					int width = m > 5 ? max_widths[j] + 2 : max_widths.Max() + 2;
					//str += string.Format("[{0:{fill}{align}{width}}]", Matrix[i, j], fill, align, width);
					str += string.Format($"{lef_bnd}{{0,{width}}}{rig_bnd}", this[i, j]);
				}
				str += CR_LF;
			}
			return trim_end(str, 1);
		}
		public int GetLength(int dimension)
		{
			return matrix_base.GetLength(dimension);
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
		public static Matrix Eye(int n)
		{
			var R = new Matrix(n);
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
					R[i, j] = (i == j) ? 1 : 0;
			return R;
		}
		/// <summary>
		/// возведение в степень
		/// </summary>
		public Matrix Pow(int p)
		{//квадратная матрица
			if (this.n != this.m)
				throw new MyException(EX_matrix_not_square);
			var R = Eye(this.n);
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
			int med_index = M_list.Count / 2; // так как нумерация с 0
			for (int i = 0; i < R.n; i++)
				for (int j = 0; j < R.m; j++)
				{
					var Rij_list = Enumerable.Select(M_list, x => x[i, j]).OrderBy(y => y).ToArray();
					R[i, j] = M_list.Count % 2 == 1 ? Rij_list[med_index] :
						(Rij_list[med_index - 1] + Rij_list[med_index]) / 2;
				}
			return R;
		}
		/// <summary>
		/// матрица из модулей её элементов
		/// </summary>
		/// <param name="M"></param>
		/// <returns></returns>
		public Matrix Abs()
		{
			var R = new Matrix(n, m);
			for (int i = 0; i < R.n; i++)
				for (int j = 0; j < R.m; j++)
					R[i, j] = Math.Abs(this[i, j]);
			return R;
		}
		/// <summary>
		/// сумма всех элементов матрицы
		/// </summary>
		/// <param name="M"></param>
		/// <returns></returns>
		public double ElemSum()
		{
			double ans = 0;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					ans += this[i, j];
			return ans;
		}
		/// <summary>
		/// минимальный ненулевой элемент матрицы
		/// </summary>
		/// <param name="M"></param>
		/// <returns></returns>
		public double MinElemNotZero()
		{
			double ans = INF;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					if (this[i, j] < ans && this[i, j] != 0)
						ans = this[i, j];
			return ans;
		}
		/// <summary>
		/// максимальный элемент матрицы
		/// </summary>
		/// <param name="M"></param>
		/// <returns></returns>
		public double MaxElem()
		{
			double ans = -INF;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					if (this[i, j] > ans)
						ans = this[i, j];
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
				throw new MyException(EX_bad_dimensions);
			double ans = 0;
			for (int i = 0; i < M1.n; i++)
				for (int j = 0; j < M1.m; j++)
				{
					if (M1[i, j] < 0 || M1[i, j] > 1 || M2[i, j] < 0 || M2[i, j] > 1)
						throw new MyException(EX_bad_matrix);
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
		/// является ли асимметричной (предполагает антирефлексивность)
		/// </summary>
		/// <returns></returns>
		public bool IsAsymmetric()
		{
			for (int i = 0; i < n; i++)
				for (int j = i; j < n; j++)
					if (this[i, j] != 0 && this[j, i] != 0)
						return false;
			return true;
		}
		/// <summary>
		/// есть ли в графе цикл
		/// </summary>
		/// <param name="AdjacencyList">список смежности</param>
		/// <returns></returns>
		public static bool IsHasCycle(List<List<int>> AdjacencyList)
		{
			int n = AdjacencyList.Count;//количество вершин
			bool is_cycle(int start_vertex_for_search)
			{
				int[] color = Enumerable.Repeat(0, n).ToArray();
				return dfs(start_vertex_for_search, color);
			}
			bool dfs(int v, int[] color)
			{
				color[v] = 1;//зашли в вершину
				for (int i = 0; i < AdjacencyList[v].Count; ++i)//перебрать исхоящие рёбра
				{
					int to = AdjacencyList[v][i];
					if (color[to] == 0)//not visited
					{
						if (dfs(to, color))
							return true;
					}
					else if (color[to] == 1)//уже заходили в to
						return true;//нашли цикл
				}
				color[v] = 2;//вышли из вершины
				return false;
			}
			var vertices = Enumerable.Range(0, n).ToArray();
			return vertices.Any(v => is_cycle(v));
		}
		/// <summary>
		/// достаёт матрицу из элемента DataGridView
		/// </summary>
		/// <param name="dgv"></param>
		/// <returns></returns>
		public static Matrix GetFromDataGridView(DataGridView dgv)
		{
			var input_matrix = new Matrix(dgv.Rows.Count, dgv.Columns.Count);
			for (int i = 0; i < input_matrix.n; i++)
				for (int j = 0; j < input_matrix.m; j++)
					input_matrix[i, j] = (double)dgv[j, i].Value;
			return input_matrix;
		}
		/// <summary>
		/// кладёт матрицу в DataGridView
		/// </summary>
		/// <param name="M"></param>
		/// <param name="dgv"></param>
		public static void SetToDataGridView(Matrix M, DataGridView dgv)
		{
			for (int i = 0; i < M.n; i++)
				for (int j = 0; j < M.m; j++)
					dgv[j, i].Value = M[i, j];
		}
	}


	/// <summary>
	/// матрицы нечётких отношений со специфичными для нечёткости операциями
	/// </summary>
	public class FuzzyRelation : Matrix
	{// base - матрица нечёткого бинарного отношения
	 //полагаем квадратными

		#region CONSTRAINTS
		/// <summary>
		/// ограничение на элементы матрицы принадлежности
		/// </summary>
		/// <param name="mu_ij"></param>
		/// <returns></returns>
		private bool is_value_of_membership_func(double mu_ij)
		{
			if (0 <= mu_ij && mu_ij <= 1)
				return true;
			return false;
		}
		/// <summary>
		/// проверка - является ли это функцией принадлежности нечёткого отношения
		/// </summary>
		/// <param name="M"></param>
		/// <returns></returns>
		private bool is_fuzzy_relation(double[,] M)
		{
			var n = M.GetLength(0);
			var m = M.GetLength(1);
			if (n != m)
				return false;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					if (!is_value_of_membership_func(M[i, j]))
						return false;
			return true;
		}
		#endregion CONSTRAINTS

		#region CONSTRUCTORS
		public FuzzyRelation(int n) : base(n) { }
		public FuzzyRelation(double[,] M) : base(M)
		{
			if (!is_fuzzy_relation(M))
				throw new MyException(EX_bad_fuzzy_relation_matrix);
		}
		public FuzzyRelation(Matrix M) : base(M)
		{
			if (!is_fuzzy_relation(M.matrix_base))
				throw new MyException(EX_bad_fuzzy_relation_matrix);
		}
		#endregion CONSTRUCTORS

		public new double this[int i, int j]
		{
			get { return base[i, j]; }
			set
			{
				if (!is_value_of_membership_func(value))
					throw new MyException(EX_bad_fuzzy_relation_matrix);
				base[i, j] = value;
			}
		}
		public double this[Tuple<int, int> pair]
		{
			get { return base[pair.Item1, pair.Item2]; }
			set
			{
				if (!is_value_of_membership_func(value))
					throw new MyException(EX_bad_fuzzy_relation_matrix);
				base[pair.Item1, pair.Item2] = value;
			}
		}
		/// <summary>
		/// матрица принадлежности (= м. предпочтений, функция принадлжености)
		/// </summary>
		public Matrix ToMatrix
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
		/// возвращает альфа-срез нечёткого отношения
		/// </summary>
		/// <param name="alpha">уровень, на котором отсекаем принадлежность (\mu(x)>alpha => 1, иначе 0)</param>
		/// <returns></returns>
		public FuzzyRelation AlphaSlice(double alpha)
		{
			var R = new FuzzyRelation(this.n);
			for (int i = 0; i < this.n; i++)
				for (int j = 0; j < this.n; j++)
					R[i, j] = this[i, j] >= alpha ? this[i, j] : 0;
			return R;
		}
		/// <summary>
		/// дополнение
		/// </summary>
		/// <returns></returns>
		public FuzzyRelation Negotate()//not A
		{
			var S = new FuzzyRelation(this.n);
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
			var S = new FuzzyRelation(this.n);
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
			var S = new FuzzyRelation(this.n);
			foreach (var x in Elements().Union(other.Elements()))
				S[x] = Math.Max(this[x], other[x]);
			return S;
		}
		public static FuzzyRelation Union(List<FuzzyRelation> list_rels)
		{
			var R = new FuzzyRelation(list_rels.First().n);//base матрица заполнена нулями
			foreach (var r in list_rels)
				R = R.Union(r);
			return R;
		}
		/// <summary>
		/// симметрическая разность
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public FuzzyRelation SetMinus1(FuzzyRelation other)//A \ B
		{
			var S = new FuzzyRelation(this.n);
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
			var S = new FuzzyRelation(this.n);
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
			return this.Transpose().ToFuzzy;
		}
		/// <summary>
		/// композиция нечётких бинарных отношений
		/// </summary>
		/// <returns></returns>
		public FuzzyRelation Compose(FuzzyRelation other)
		{
			var R = new FuzzyRelation(this.n);
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
			//if (p == 0)
			//	return Eye(n).ToFuzzy;
			var R = new FuzzyRelation(this);
			for (int k = 1; k < p; k++)
				R = R.Compose(R);
			return R;
		}
		/// <summary>
		/// транзитивное замыкание нечеткого отношения
		/// </summary>
		public FuzzyRelation TransitiveClosure()
		{
			var ans = new List<FuzzyRelation>();
			ans.Add(new FuzzyRelation(this));
			for (int i = 1; i < n; i++)
			{
				ans.Add(ans.Last().Compose(ans[0]));
				if (ans[i] == ans[i - 1])
					break;
			}
			return Union(ans);
		}
		/// <summary>
		/// есть ли цикл в матрице принадлежности отношения
		/// </summary>
		/// <param name="trans_closured_matrix"></param>
		/// <returns></returns>
		public bool IsHasCycle()
		{
			return Matrix.IsHasCycle(this.AdjacencyList(0.0));
		}
		/// <summary>
		/// преобразование списка FuzzyRelation в список Matrix
		/// </summary>
		/// <param name="R_list"></param>
		/// <returns></returns>
		public static List<Matrix> ToMatrixList(List<FuzzyRelation> R_list)
		{
			return R_list.Select(x => x.ToMatrix).ToList();
		}
		/// <summary>
		/// индексы недоминируемых альтернатив
		/// </summary>
		/// <returns></returns>
		public HashSet<int> UndominatedAlternatives()
		{
			var ans = Enumerable.Range(0, n).ToHashSet();
			for (int i = 0; i < n; i++)
				for (int j = 0; j < n; j++)
					if (i != j && this[j, i] > this[i, j])
					{
						ans.Remove(i);
						break;
					}
			return ans;
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
			public List<double> ValuesList;
			public string Label;
		}
		public class PathSummaryDistanceType
		{//суммарное расстояние до всех остальных входных ранжирований
			public PathSummaryDistanceType() { }
			public Characteristic modulus = new Characteristic("Сумм. расстояние 'модуль разности'");
			public Characteristic square = new Characteristic("Сумм. расстояние 'квадрат разности'");
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
			else if (rank as Ranking != null)
				Rank2List = (rank as Ranking).Path;
		}
		#endregion CONSTRUCTORS

		public int MethodID;//каким методом получено
		private List<int> Path = new List<int>();//список вершин в пути-ранжировании
		public Characteristic PathCost = new Characteristic("Стоимость"); //общая стоимость пути
		public Characteristic PathStrength = new Characteristic("Сила"); //сила пути (пропускная способность)
		public PathSummaryDistanceType PathSummaryDistance = new PathSummaryDistanceType();//суммарное расстояние до всех остальных входных ранжирований
		public Characteristic PathExpertCosts = new Characteristic("Вектор стоимостей по экспертам"); //вектор стоимостей по каждому эксперту-характеристике

		#region PROPERTIES
		public List<int> Rank2List
		{
			set
			{
				Path = value;
				SetRankingParams(FuzzyRelation.ToMatrixList(R_list), R.aggregated);
			}
			get { return Path; }
		}
		public int[] Rank2Array
		{
			set
			{
				Path = value.ToList();
				SetRankingParams(FuzzyRelation.ToMatrixList(R_list), R.aggregated);
			}
			get { return Path.ToArray(); }
		}
		/// <summary>
		/// создаёт матрицу смежности (порядок) из профиля эксперта
		/// </summary>
		public Matrix Rank2Matrix
		{//ранжирование - как полный строгий порядок (полное, антирефлексивное, асимметричное, транзитивное)
			get
			{
				var node_list = Rank2Array;
				var l = Rank2Array.Length;
				if (l != Rank2Array.Distinct().Count())
					throw new MyException(EX_bad_expert_profile);
				Matrix Rj = new Matrix(l);
				for (int i = 0; i < l; i++)
					for (int j = 0; j < l; j++)
					{
						var candidate1 = node_list[i];
						var candidate2 = node_list[j];
						Rj[candidate1, candidate2] = (i < j) ? 1 : 0;//дуга i->j, левый лучше
					}
				return Rj;
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
		/// создаёт строгое ранжирование на основе матрицы: полного транзитивного отношения, выделяя асимметричную часть
		/// </summary>
		public static bool Matrix2RanksDemukron(Matrix M, out List<Ranking> ans)
		{
			//если матрица не асимметричная, то она имеет цикл из двух вершин. Поэтому матрица будет асимметрична
			if (Matrix.IsHasCycle(M.AdjacencyList(x => x == 0.0 || Math.Abs(x) == INF)))
				throw new MyException(EX_bad_matrix);
			var AM = M.AdjacencyMatrix;
			List<List<int>> levels = new List<List<int>>();
			var wins = new double[M.n];//инициализирован нулями
			int mark = -1;
			while (true)
			{
				for (int i = 0; i < AM.n; i++)
				{
					if (wins[i] != mark)
					{
						wins[i] = 0;
						for (int j = 0; j < AM.m; j++)
							wins[i] += AM[i, j];
					}
				}
				var last_level_vertices = new List<int>();
				for (int i = 0; i < AM.n; i++)
				{
					if (wins[i] == 0)
					{
						last_level_vertices.Add(i);
						wins[i] = mark;
						for (int k = 0; k < AM.n; k++)
							AM[k, i] = 0;
					}
				}
				levels.Add(last_level_vertices);
				if (wins.Where(x => x == mark).Count() == wins.Count())
					break;
			}
			levels.Reverse();
			/*
			ans = new List<Ranking>();
			void add_next_variants(int next_lvl, List<int> cur_path)
			{
				if(next_lvl == levels.Count-1)
				foreach (int v in levels[next_lvl])
				{
					var ordering = new List<int>(cur_path);
					ordering.Add(v);
					ans.Add(new Ranking(ordering));
				}
				else
				{
					foreach (int v in levels[next_lvl])
					{
						var ordering = new List<int>(cur_path);
						ordering.Add(v);
						add_next_variants(next_lvl + 1, ordering);
					}
				}
			}
			for(int lev = 0; lev<levels.Count; lev++)
			{
				foreach (int v in levels[lev])
				{
					cur_path.Add(v);

				}

				cur_path.Add(levels[lev]);
				ans.Add
			}
			*/
			var ordering = Enumerable.Repeat(-1, n).ToArray();
			for (int i = 0; i < AM.n; i++)
			{
				double cwins = 0;
				for (int j = 0; j < AM.n; j++)
				{
					cwins += AM[i, j];
				}
				if (cwins < n && ordering[(int)cwins] == -1)
					ordering[(int)cwins] = i;
				else
				{
					ans = null;
					return false;
				}
			}
			ans = new List<Ranking> { new Ranking(ordering.Reverse().ToArray()) };
			return true;
		}

		/// <summary>
		/// a0a1a2 -> 0, 1, 2
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public List<int> String2List(string s)
		{
			return s.Split(MARK).ToList()
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
					PathCost.Value = INF;
					PathStrength.Value = INF;
					PathSummaryDistance.modulus.Value = INF;
					PathSummaryDistance.square.Value = INF;
					PathExpertCosts.ValuesList = Enumerable.Repeat(INF, other_matrices.Count).ToList();
				}
				else
				{
					PathCost.Value = PathCost(Rank2List, weight_matrix);
					PathStrength.Value = PathStrength(Rank2List, weight_matrix);
					PathSummaryDistance.modulus.Value = Rank2Matrix.SumDistance(other_matrices, Matrix.DistanceModulus);
					PathSummaryDistance.square.Value = Rank2Matrix.SumDistance(other_matrices, Matrix.DistanceSquare);
					PathExpertCosts.ValuesList = new List<double>();
					foreach (var expert_matrix in other_matrices)
						PathExpertCosts.ValuesList.Add(PathCost(Rank2List, expert_matrix));
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
		public DataGridView connectedTableFrame = null;//в какой контейнер выводить результаты работы метода
		public Label connectedLabel = null;//в какой контейнер выводить текстовые пояснения к методу

		public List<Ranking> Rankings = null;//выдаваемые методом ранжирования
		public bool[] IsInPareto = null;//входит ли ранжирование по индексу i в Парето-множество по векторам-характеристикам экспертов
		public List<int> Winners = null;//победители - недоминируемые альтернативы

		public double MinLength;//минимальная длина среди ранжирований метода
		public double MaxLength;//максимальная длина среди ранжирований метода
		public double MinStrength;//минимальная сила среди ранжирований метода
		public double MaxStrength;//максимальная сила среди ранжирований метода
								  //минимальное и максимальное суммарные расстояние среди ранжирований метода
		public Ranking.PathSummaryDistanceType MinDistance = new Ranking.PathSummaryDistanceType();
		public Ranking.PathSummaryDistanceType MaxDistance = new Ranking.PathSummaryDistanceType();

		#region PROPERTIES
		public bool IsExecute
		{
			get
			{
				return (connectedCheckBox != null) ? connectedCheckBox.Checked : false;
			}
		}
		public string ConnectedLabel
		{
			set
			{
				if (value == "" || value == null)
				{
					connectedLabel?.ResetText();
					connectedLabel?.Hide();
					//connectedLabel?.Dispose();
				}
				else
				{
					if (connectedLabel == null)
					{
						connectedLabel = new Label();
						connectedLabel.AutoSize = true;
						connectedLabel.Location = new System.Drawing.Point(
							0, connectedTableFrame.Location.Y + connectedTableFrame.Height);
						connectedTableFrame?.Parent?.Controls.Add(connectedLabel);
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
			set { Rankings = value.Select(x => new Ranking(x)).ToList(); }
			get { return this.Rankings.Select(x => x.Rank2Array).ToList(); }
		}
		public List<string> Ranks2Strings
		{
			get { return this.Rankings.Select(x => x.Rank2String).ToList(); }
		}
		#endregion PROPERTIES

		/// <summary>
		/// отношение Парето
		/// </summary>
		/// <param name="R1"></param>
		/// <param name="R2"></param>
		/// <returns></returns>
		public static bool ParetoMORETHAN(List<double> R1, List<double> R2)
		{
			bool ans = false;
			if (R1.Count != R2.Count)
				throw new MyException(EX_bad_dimensions);
			for (int i = 0; i < R1.Count; i++)
			{
				if (R1[i] < R2[i])
					return false;
				if (R1[i] > R2[i])//если есть хотя бы один элемент, который больше
					ans = true;
			}
			return ans;
		}
		public static bool ParetoEQUALS(List<double> R1, List<double> R2)
		{
			if (!ParetoMORETHAN(R1, R2) && !ParetoMORETHAN(R2, R1))
				return true;
			return false;
		}

		/// <summary>
		/// задаёт связанные с методом элементы управления
		/// </summary>
		/// <param name="checkBox"></param>
		/// <param name="dgv"></param>
		public void SetConnectedControls(CheckBox checkBox, DataGridView dgv)
		{
			connectedCheckBox = checkBox;
			connectedTableFrame = dgv;
			connectedCheckBox.Text = MethodsInfo[ID];
			((GroupBox)connectedTableFrame?.Parent).Text = MethodsInfo[ID];
		}
		/// <summary>
		/// удаление ранжирований и их характеристик
		/// </summary>
		public void ClearRankings()
		{
			Rankings = new List<Ranking>();
			Winners = new List<int>();
			IsInPareto = new bool[] { };
			MinLength = INF;
			MaxLength = INF;
			MinStrength = INF;
			MaxStrength = INF;
			MinDistance.modulus.Value = INF;
			MaxDistance.modulus.Value = INF;
			MinDistance.square.Value = INF;
			MaxDistance.square.Value = INF;
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
		/// задаёт индексы ранжирований, входящих в Парето-множество
		/// </summary>
		/// <param name="R"></param>
		/// <returns></returns>
		private bool[] set_Pareto_signs(List<Ranking.Characteristic> R)
		{
			var r = R.Count;
			var ans = new bool[r];
			var Pareto_indices = Enumerable.Range(0, r).ToHashSet();

			for (int i = 0; i < r; i++)
				foreach (int j in Pareto_indices)//(int j = 0; j < r; j++)
				{
					var Vj = R[j].ValuesList;
					var Vi = R[i].ValuesList;
					var K = Vj.Count;
					if (i != j && ParetoMORETHAN(Vj, Vi))
					{
						Pareto_indices.Remove(i);
						break;
					}
				}
			foreach (int i in Pareto_indices)
				ans[i] = true;
			return ans;
		}
		/// <summary>
		/// выбирает лучшие по каждой характеристике ранжирований
		/// </summary>
		public void SetCharacteristicsBestWorst()
		{
			MinLength = min(Rankings.Select(x => x.PathCost.Value).ToList());
			MaxLength = max(Rankings.Select(x => x.PathCost.Value).ToList());
			MinStrength = min(Rankings.Select(x => x.PathStrength.Value).ToList());
			MaxStrength = max(Rankings.Select(x => x.PathStrength.Value).ToList());
			MinDistance.modulus.Value = min(Rankings.Select(x => x.PathSummaryDistance.modulus.Value).ToList());
			MaxDistance.modulus.Value = max(Rankings.Select(x => x.PathSummaryDistance.modulus.Value).ToList());
			MinDistance.square.Value = min(Rankings.Select(x => x.PathSummaryDistance.square.Value).ToList());
			MaxDistance.square.Value = max(Rankings.Select(x => x.PathSummaryDistance.square.Value).ToList());
			IsInPareto = set_Pareto_signs(Rankings.Select(x => x.PathExpertCosts).ToList());
		}
		/// <summary>
		/// очищает весь вывод метода
		/// </summary>
		public void ClearMethodOutput()
		{
			connectedTableFrame?.Rows.Clear();
			connectedTableFrame?.Columns.Clear();
			ConnectedLabel = "";
			connectedTableFrame?.Hide();
			connectedTableFrame?.Parent?.Hide();
		}
		/// <summary>
		/// отображает весь вывод метода
		/// </summary>
		public void ShowMethodOutput()
		{
			if (IsExecute)
			{
				connectedTableFrame?.Show();
				connectedTableFrame?.Parent.Show();
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
		public static Method Smerchinskaya_Yashina_method = new Method(SMERCHINSKAYA_YASHINA_METHOD);

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
			MinSummaryModulusDistance = INF;
			MinSummarySquareDistance = INF;
			MaxHamPathLength = INF;
			MaxHamPathStrength = INF;
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
		public static List<Method> GetMethodsExecutedWhithResult()
		{
			var is_rankings_of_method_exists = new List<Method>();
			foreach (Method m in Methods.GetMethods())
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
			All_various_rankings.ClearRankings();
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

			MinSummaryModulusDistance = All_various_rankings.Rankings
				.Select(x => x.PathSummaryDistance.modulus.Value).Min();
			MinSummarySquareDistance = All_various_rankings.Rankings
				.Select(x => x.PathSummaryDistance.square.Value).Min();
		}

		/// <summary>
		/// вычисляет все Гамильтоновы пути и сохраняет в метод в словаре
		/// </summary>
		/// <param name="HP"></param>
		/// <param name="Weights_matrix"></param>
		public static void Set_All_Hamiltonian_paths(Matrix weight_matrix)
		{
			All_Hamiltonian_paths.ClearRankings();
			List<List<int>>[,] HP = Hamiltonian_paths_through_matrix_degree(weight_matrix);
			for (int i = 0; i < HP.GetLength(0); i++)
				for (int j = 0; j < HP.GetLength(1); j++)
					foreach (List<int> path_from_i_to_j in HP[i, j])
						All_Hamiltonian_paths.Rankings.Add(new Ranking(ALL_HP, path_from_i_to_j));
		}

		/// <summary>
		/// Гамильтоновы пути максимальной длины
		/// </summary>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		public static void Set_Hp_max_length(Matrix weight_matrix)
		{
			Hp_max_length.ClearRankings();
			if (All_Hamiltonian_paths.Rankings.Count == 0)
				Set_All_Hamiltonian_paths(weight_matrix);
			MaxHamPathLength = All_Hamiltonian_paths.Rankings.Select(x => x.PathCost.Value).Max();
			foreach (Ranking r in All_Hamiltonian_paths.Rankings)
				if (r.PathCost.Value == MaxHamPathLength)
					Hp_max_length.Rankings.Add(r);
		}

		/// <summary>
		/// Гамильтоновы пути наибольшей силы
		/// </summary>
		/// <param name="Weights_matrix"></param>
		/// <returns></returns>
		public static void Set_Hp_max_strength(Matrix weight_matrix)
		{
			Hp_max_strength.ClearRankings();
			if (All_Hamiltonian_paths.Rankings.Count == 0)
				Set_All_Hamiltonian_paths(weight_matrix);
			MaxHamPathStrength = All_Hamiltonian_paths.Rankings.Select(x => x.PathStrength.Value).Max();
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
			Schulze_method.ClearRankings();
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
			Schulze_method.Winners = Enumerable.Range(0, n).Where(i => winner[i] == true).ToList();//индексы победителей
			if (Ranking.Matrix2RanksDemukron(new Matrix(PD), out var rank))
				Schulze_method.Rankings.Add(new Ranking(SCHULZE_METHOD, rank));
		}

		/// <summary>
		/// Нахождение ранжирований из агрегированной матрицы - минимальное расстояние
		/// </summary>
		/// <param name="weight_matrix"></param>
		public static void Set_Smerchinskaya_Yashina_method(Matrix weight_matrix)
		{
			Smerchinskaya_Yashina_method.ClearRankings();
			var R = new FuzzyRelation(weight_matrix);
			var r = weight_matrix.AdjacencyMatrix.ToFuzzy;
			Matrix RK = Matrix.Eye(1); //матрица контуров
			while (true)
			{//если ещё остались контуры
				RK = r.TransitiveClosure().Intersect(r.TransitiveClosure().Transpose().ToFuzzy).Intersect(r);
				if (RK.ElemSum() == 0)
					break;
				for (int i = 0; i < RK.n; i++)
					for (int j = 0; j < RK.m; j++)
						if (RK[i, j] != 0)
							RK[i, j] = R[i, j];

				var min_elem = RK.MinElemNotZero();
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
						if (RK[i, j] != 0 && R[i, j] == min_elem)
						{
							R[i, j] = 0;
							r[i, j] = 0;
						}
			}
			R = R.TransitiveClosure();
			Smerchinskaya_Yashina_method.Winners = R.UndominatedAlternatives().ToList();
			if (Ranking.Matrix2RanksDemukron(R, out var rank))
				Smerchinskaya_Yashina_method.Rankings.Add(new Ranking(SMERCHINSKAYA_YASHINA_METHOD, rank));
		}

	}
}
