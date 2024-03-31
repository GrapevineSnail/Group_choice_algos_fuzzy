using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Model;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Group_choice_algos_fuzzy
{
	/// <summary>
	/// матрицы
	/// </summary>
	public class Matrix
	{
		#region CONSTRUCTORS
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
		#endregion CONSTRUCTORS

		#region FIELDS
		/// <summary>
		/// "основа" матрицы - двумерный массив
		/// </summary>
		public double[,] matrix_base = new double[,] { };
		#endregion FIELDS

		#region PROPERTIES
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
		public FuzzyRelation ToFuzzy { get { return new FuzzyRelation(this); } }
		#endregion PROPERTIES

		#region OPERATORS
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
			{
				if (R1 is null && R2 is null)
					return false;
				else
					return true;
			}
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
		#endregion OPERATORS

		#region FUNCTIONS
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
			var lef_bnd = use_separator ? "[" : "";
			var rig_bnd = use_separator ? "]" : "";
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
		public double ElemSum()
		{
			double ans = 0;
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					ans += this[i, j];
			return ans;
		}
		/// <summary>
		/// возвращает множество всех элементов матрицы (каждый элемент в одном экземпляре)
		/// </summary>
		/// <returns></returns>
		public HashSet<double> GetElemValues()
		{
			HashSet<double> ans = new HashSet<double>();
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					ans.Add(this[i, j]);
			return ans;
		}
		/// <summary>
		/// минимальный ненулевой элемент матрицы
		/// </summary>
		public double MinElemButNotZero()
		{
			var elems = this.GetElemValues();
			double ans = INF;
			foreach (var e in elems)
				if (e < ans && e != 0)
					ans = e;
			return ans;
		}
		/// <summary>
		/// минимальный элемент матрицы
		/// </summary>
		/// <returns></returns>
		public double MinElem()
		{
			return this.GetElemValues().Min();
		}
		/// <summary>
		/// максимальный элемент матрицы
		/// </summary>
		public double MaxElem()
		{
			return this.GetElemValues().Max();
		}
		/// <summary>
		/// расстояние между матрицами на основании выбранной функции расстояния для отдельных элементов
		/// </summary>
		/// <param name="M1"></param>
		/// <param name="M2"></param>
		/// <param name="elem_diff">функция расстояния для отдельных элементов</param>
		/// <returns></returns>
		private static double Distance(Matrix M1, Matrix M2, Func<double, double, double> elem_diff)
		{// вход: матрицы одинаковой размерности
			if (M1.n != M2.n || M1.m != M2.m)
				throw new MyException(EX_bad_dimensions);
			double ans = 0;
			for (int i = 0; i < M1.n; i++)
				for (int j = 0; j < M1.m; j++)
				{
					ans += elem_diff(M1[i, j], M2[i, j]);
				}
			return ans;
		}
		/// <summary>
		/// вычисляет расстояние между матрицами из модулей разностей элементов
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
		/// вычисляет расстояние между матрицами из квадратов разностей элементов
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
				for (int i = 0; i < AdjacencyList[v].Count; ++i)//перебрать исходящие рёбра
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
		/// достаёт матрицу из DataGridView
		/// </summary>
		public static Matrix GetFromDataGridView(DataGridView dgv)
		{
			var input_matrix = new Matrix(dgv.Rows.Count, dgv.Columns.Count);
			for (int i = 0; i < input_matrix.n; i++)
				for (int j = 0; j < input_matrix.m; j++)
					input_matrix[i, j] = dgv[j, i].Value == null ? 0 : (double)dgv[j, i].Value;
			return input_matrix;
		}
		/// <summary>
		/// кладёт матрицу в DataGridView
		/// </summary>
		public static void SetToDataGridView(Matrix M, DataGridView dgv)
		{
			for (int i = 0; i < M.n; i++)
				for (int j = 0; j < M.m; j++)
					dgv[j, i].Value = M[i, j];
		}
		/// <summary>
		/// делает матрицу с элементами, нормированными на 1 (принадлежащими от 0 до 1 включительно)
		/// </summary>
		/// <returns></returns>
		public Matrix NormalizeElems(out bool IsNormalized)
		{
			Matrix R = new Matrix(this);
			//была ли матрица уже нормализованной
			IsNormalized = 0 <= this.MinElem() && this.MaxElem() <= 1 ? true : false;
			double shift = this.MinElem();
			double squeeze = this.GetElemValues().Select(x => x - shift).Max();
			if (!IsNormalized)
			{
				for (int i = 0; i < n; i++)
					for (int j = 0; j < m; j++)
						R[i, j] = (R[i, j] - shift) / squeeze;
			}
			return R;
		}
		#endregion FUNCTIONS
	}


	/// <summary>
	/// матрицы нечётких отношений со специфичными для нечёткости операциями
	/// </summary>
	public class FuzzyRelation : Matrix
	{// base - матрица нечёткого бинарного отношения
	 //матрица принадлежности (= м. предпочтений, функция принадлжености)
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
		private bool is_fuzzy_relation_matrix(double[,] M)
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
			if (!is_fuzzy_relation_matrix(M))
				throw new MyException(EX_bad_fuzzy_relation_matrix);
		}
		public FuzzyRelation(Matrix M) : base(M)
		{
			if (!is_fuzzy_relation_matrix(M.matrix_base))
				throw new MyException(EX_bad_fuzzy_relation_matrix);
		}
		#endregion CONSTRUCTORS

		#region FIELDS
		private FuzzyRelation _Asymmetric;
		private FuzzyRelation _Symmetric;
		private FuzzyRelation _DestroyedCycles;
		private FuzzyRelation _TransClosured;
		#endregion FIELDS

		#region PROPERTIES
		public new double this[int i, int j]
		{
			get { return base[i, j]; }
			set
			{
				if (!is_value_of_membership_func(value))
					throw new MyException(EX_bad_fuzzy_relation_matrix);
				base[i, j] = value;
				ClearDerivativeFields();
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
				ClearDerivativeFields();
			}
		}
		public Matrix ToMatrix
		{
			get { return base.Self; }
		}
		public FuzzyRelation Asymmetric
		{
			get
			{
				if (_Asymmetric is null)
				{
					_Asymmetric = this.AsymmetricPart.ToFuzzy;
				}
				return _Asymmetric;
			}
			set { _Asymmetric = value; }
		}
		public FuzzyRelation Symmetric
		{
			get
			{
				if (_Symmetric is null)
				{
					_Symmetric = this.SymmetricPart.ToFuzzy;
				}
				return _Symmetric;
			}
			set { _Symmetric = value; }
		}
		public FuzzyRelation DestroyedCycles
		{
			get
			{
				if (_DestroyedCycles is null)
				{
					_DestroyedCycles = this.DestroyCycles();
				}
				return _DestroyedCycles;
			}
			set { _DestroyedCycles = value; }
		}
		public FuzzyRelation TransClosured
		{
			get
			{
				if (_TransClosured is null)
				{
					_TransClosured = this.TransitiveClosure();
				}
				return _TransClosured;
			}
			set { _TransClosured = value; }
		}
		#endregion PROPERTIES

		#region FUNCTIONS
		public void ClearDerivativeFields()
		{
			Asymmetric = null;
			Symmetric = null;
			DestroyedCycles = null;
			TransClosured = null;
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
					ans[(this.m * i) + j] = new Tuple<int, int>(i, j);
			return ans;
		}
		/// <summary>
		/// возвращает альфа-срез нечёткого отношения
		/// </summary>
		/// <param name="alpha">уровень, на котором отсекаем принадлежность (mu(x)>=alpha ? mu(x) : 0)</param>
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
		/// <returns>not A</returns>
		public FuzzyRelation Negotate()
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
		/// <returns>A \intersect B</returns>
		public FuzzyRelation Intersect1(FuzzyRelation other)
		{
			var S = new FuzzyRelation(this.n);
			foreach (var x in Elements().Union(other.Elements()))
				S[x] = Math.Min(this[x], other[x]);
			return S;
		}
		/// <summary>
		/// пересечение
		/// </summary>
		/// <param name="other"></param>
		/// <returns>A \intersect B</returns>
		public FuzzyRelation Intersect2(FuzzyRelation other)
		{
			var S = new FuzzyRelation(this.n);
			foreach (var x in Elements().Union(other.Elements()))
				S[x] = Math.Max(0, this[x] + other[x] - 1);
			return S;
		}
		/// <summary>
		/// объединение
		/// </summary>
		/// <param name="other"></param>
		/// <returns>A U B</returns>
		public FuzzyRelation Union(FuzzyRelation other)
		{
			var S = new FuzzyRelation(this.n);
			foreach (var x in Elements().Union(other.Elements()))
				S[x] = Math.Max(this[x], other[x]);
			return S;
		}
		/// <summary>
		/// объединение (множественное)
		/// </summary>
		/// <param name="list_rels"></param>
		/// <returns></returns>
		public static FuzzyRelation Union(List<FuzzyRelation> list_rels)
		{
			var R = new FuzzyRelation(list_rels.First().n);//base матрица заполнена нулями
			foreach (var r in list_rels)
			{
				if (r.n != R.n || r.m != R.m)
					throw new MyException(EX_bad_dimensions);
				R = R.Union(r);
			}
			return R;
		}
		/// <summary>
		/// разность
		/// </summary>
		/// <param name="other"></param>
		/// <returns>A \ B</returns>
		public FuzzyRelation SetMinus1(FuzzyRelation other)
		{
			return this.Intersect1(other.Negotate());
		}
		/// <summary>
		/// разность
		/// </summary>
		/// <param name="other"></param>
		/// <returns>A \ B</returns>
		public FuzzyRelation SetMinus2(FuzzyRelation other)
		{
			return this.Intersect2(other.Negotate());
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
		/// <returns>A \circ B</returns>
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
			if (p == 0)
				return Eye(n).ToFuzzy;
			var R = new FuzzyRelation(this);
			for (int k = 1; k < p; k++)
				R = R.Compose(R);
			return R;
		}
		/// <summary>
		/// является ли левый операнд подмножеством (нестрого) правого операнда
		/// </summary>
		/// <returns></returns>
		public static bool SubsetOrEqual(FuzzyRelation R1, FuzzyRelation R2)
		{
			if (R1.n != R2.n)
				throw new MyException(EX_bad_dimensions);
			for (int i = 0; i < R1.n; i++)
			{
				for (int j = 0; j < R1.n; j++)
				{
					if (R1[i, j] > R2[i, j])
						return false;
				}
			}
			return true;
		}
		/// <summary>
		/// является ли отношение транзитивным
		/// </summary>
		public bool IsTransitive()
		{
			var r2 = this.Compose(this);
			if (SubsetOrEqual(r2, this))
				return true;
			return false;
		}
		/// <summary>
		/// транзитивное замыкание нечеткого отношения
		/// </summary>
		public FuzzyRelation TransitiveClosure()
		{//алгоритм можно ускорить - можно алг Флойда-Уоршалла?
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
		public bool IsHasCycle()
		{
			return Matrix.IsHasCycle(this.AdjacencyList(0.0));
		}
		/// <summary>
		/// разбить циклы
		/// </summary>
		/// <returns></returns>
		public FuzzyRelation DestroyCycles()
		{
			var R = new FuzzyRelation(this);
			var R_AM = R.AdjacencyMatrix.ToFuzzy;
			Matrix RK = Matrix.Eye(1); //матрица контуров
			while (true)
			{//если ещё остались контуры
				var RKTC = R_AM.TransitiveClosure();
				RK = R_AM.Intersect1(RKTC).Intersect1(RKTC.Transpose().ToFuzzy);
				if (RK.ElemSum() == 0)
					break;
				for (int i = 0; i < RK.n; i++)
					for (int j = 0; j < RK.m; j++)
						if (RK[i, j] != 0)
							RK[i, j] = R[i, j];

				var min_elem = RK.MinElemButNotZero();
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
						if (RK[i, j] != 0 && R[i, j] == min_elem)
						{
							R[i, j] = 0;
							R_AM[i, j] = 0;
						}
			}
			return R;
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
		#endregion FUNCTIONS
	}


	/// <summary>
	/// одно какое-то ранжирование (или путь) со всеми его свойствами
	/// </summary>
	public class Ranking
	{
		/// <summary>
		///каждая характеристика ранжирования имеет числовое значение и осмысленное наименование 
		/// </summary>
		public class RankingCharacteristic
		{
			public RankingCharacteristic(string label) { Label = label; }
			public RankingCharacteristic(string label, double value) { Label = label; Value = value; }
			public string Label = "";
			public double Value = INF;
			public List<double> ValuesList = new List<double>();
		}
		/// <summary>
		/// суммарное расстояние до всех остальных входных ранжирований
		/// </summary>
		public class PathSummaryDistanceClass
		{
			public RankingCharacteristic modulus = new RankingCharacteristic("Сумм. расстояние 'модуль разности'");
			public RankingCharacteristic square = new RankingCharacteristic("Сумм. расстояние 'квадрат разности'");
		}

		#region CONSTRUCTORS
		public Ranking(int[] rank)
		{
			Rank2Array = rank;
		}
		public Ranking(List<int> rank)
		{
			Rank2List = rank;
		}
		public Ranking(int methodID, object rank)
		{
			MethodID = methodID;
			if (rank as List<int> != null)
				Rank2List = rank as List<int>;
			else if (rank as int[] != null)
				Rank2Array = rank as int[];
			else if (rank as Ranking != null)
				Rank2List = (rank as Ranking)._Path;
		}
		#endregion CONSTRUCTORS

		#region FIELDS
		private List<int> _Path;//список вершин в пути-ранжировании
		public int MethodID = -1;//каким методом получено
		public RankingCharacteristic Cost;//общая стоимость пути
		public RankingCharacteristic CostsExperts; //вектор стоимостей по каждому эксперту-характеристике
		public RankingCharacteristic Strength; //сила пути (пропускная способность)
		public PathSummaryDistanceClass SummaryDistance;//суммарное расстояние до всех остальных входных ранжирований
		#endregion FIELDS

		#region PROPERTIES
		public List<int> Rank2List
		{
			get { return _Path; }
			set
			{
				_Path = value;
				UpdateRankingParams(AggregatedMatrix.R, FuzzyRelation.ToMatrixList(R_list));
			}
		}
		public int[] Rank2Array
		{
			get { return _Path.ToArray(); }
			set
			{
				_Path = value.ToList();
				UpdateRankingParams(AggregatedMatrix.R, FuzzyRelation.ToMatrixList(R_list));
			}
		}
		/// <summary>
		/// создаёт матрицу смежности (порядок) из профиля эксперта
		/// </summary>
		public Matrix Rank2Matrix
		{//ранжирование - как полный строгий порядок (полное, антирефлексивное, асимметричное, транзитивное)
		 //но может быть не полным - тогда будут нули в матрице смежности
			get
			{
				var node_list = Rank2Array;
				var l = Rank2Array.Length;
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
		public string Rank2String
		{
			get
			{
				return string.Join("", _Path.Select(x => ind2sym[x]).ToList());
			}
		}
		public int Count
		{
			get { return _Path.Count(); }
		}
		#endregion PROPERTIES

		#region FUNCTIONS
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
		/// вычисление всех параметров ранжирования
		/// </summary>
		private void UpdateRankingParams(Matrix weight_matrix, List<Matrix> other_matrices)
		{
			if (Rank2List != null)
			{
				Cost = new RankingCharacteristic("Стоимость", PathCost(Rank2List, weight_matrix));
				Strength = new RankingCharacteristic("Сила", PathStrength(Rank2List, weight_matrix));
				CostsExperts = new RankingCharacteristic("Вектор стоимостей по экспертам");
				if (other_matrices != null)
				{
					foreach (var expert_matrix in other_matrices)
						CostsExperts.ValuesList.Add(PathCost(Rank2List, expert_matrix));
					if (Rank2List.Count == n)
					{
						SummaryDistance = new PathSummaryDistanceClass();
						SummaryDistance.modulus.Value = Rank2Matrix.SumDistance(other_matrices, Matrix.DistanceModulus);
						SummaryDistance.square.Value = Rank2Matrix.SumDistance(other_matrices, Matrix.DistanceSquare);
					}
				}
			}
		}
		/// <summary>
		/// создаёт ранжирование на основе матрицы
		/// алг. Демукрона, начиная с конца - со стока
		/// </summary>
		public static bool Matrix2RanksDemukron(Matrix M, out List<List<int>> levels, out List<Ranking> rankings)
		{
			levels = new List<List<int>>();
			rankings = new List<Ranking>();
			//если матрица не асимметричная, то она имеет цикл из двух вершин. Поэтому матрица будет асимметрична
			if (Matrix.IsHasCycle(M.AdjacencyList(x => x == 0.0 || Math.Abs(x) == INF)))
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
					rankings.Add(new Ranking(constructed_ranking));
				}
			}
			if (levels_cnt < n)
				return false;//если это не ранжирование, а разбиение на уровни
			else
				return true;
		}
		public static Ranking List2Rank(List<int> path)
		{
			return new Ranking(path);
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
		private RankingsCharacteristics _RanksCharacteristics;
		private List<int> _Winners;//победители - недоминируемые альтернативы
		private List<List<int>> _Levels;//разбиение графа отношения на уровни (алг. Демукрона, начиная с конца - со стока)
		private ConnectedControls _VisualFormConnectedControls;
		#endregion FIELDS

		#region SUBCLASSES
		/// <summary>
		/// связанные с методом элементы управления (control-ы) на форме
		/// </summary>
		public class ConnectedControls //: Method
		{
			//public ConnectedControls(int id) : base(id) { }//фиктивный конструктор
			public ConnectedControls(Method m) { parent_method = m; }
			private readonly Method parent_method;
			private CheckBox connectedCheckBox;//чекбокс - будем ли запускать метод
			private Label connectedLabel;//в какой контейнер выводить текстовые пояснения к методу
			private DataGridView connectedTableFrame;//в какой контейнер выводить результаты работы метода
			public CheckBox ConnectedCheckBox
			{
				get { return connectedCheckBox; }
				set
				{
					connectedCheckBox = value;
					connectedCheckBox.Text = MethodsInfo[parent_method.ID];
				}
			}
			public string ConnectedLabel
			{
				get { return connectedLabel != null ? connectedLabel.Text : ""; }
				set
				{
					if (value == "" || value is null)
					{
						connectedLabel?.ResetText();
						connectedLabel?.Hide();
						//connectedLabel?.Dispose();
					}
					else
					{
						if (connectedLabel is null)
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
			}
			public Label ConnectedLabel_control
			{
				get { return connectedLabel; }
				set { connectedLabel = value; }
			}
			public DataGridView ConnectedTableFrame
			{
				get { return connectedTableFrame; }
				set
				{
					connectedTableFrame = value;
					((GroupBox)connectedTableFrame?.Parent).Text = MethodsInfo[parent_method.ID];
				}
			}
			public void Set(CheckBox checkBox, DataGridView dgv)
			{
				ConnectedCheckBox = checkBox;
				ConnectedTableFrame = dgv;
			}
			public void ShowOutput()
			{
				if (parent_method.IsExecute)
				{
					ConnectedTableFrame?.Show();
					ConnectedTableFrame?.Parent.Show();
				}
			}
			public void ClearOutput()
			{
				ConnectedTableFrame?.Rows.Clear();
				ConnectedTableFrame?.Columns.Clear();
				ConnectedLabel = "";
				ConnectedTableFrame?.Hide();
				ConnectedTableFrame?.Parent?.Hide();
			}
		}
		/// <summary>
		/// характеристики совокупности ранжирований метода
		/// </summary>
		public class RankingsCharacteristics //: Method
		{
			//public MethodCharacteristics(int id) : base(id) { }//фиктивный конструктор
			public RankingsCharacteristics(Method m) { parent_method = m; }
			private readonly Method parent_method;
			private double _MinCost;//минимальная длина среди ранжирований метода
			private double _MaxCost;//максимальная длина среди ранжирований метода
			private double _MinStrength;//минимальная сила среди ранжирований метода
			private double _MaxStrength;//максимальная сила среди ранжирований метода
			private Ranking.PathSummaryDistanceClass _MinDistance;//минимальное суммарн. расстояние среди ранжирований метода
			private Ranking.PathSummaryDistanceClass _MaxDistance;//максимальное суммарн. расстояние среди ранжирований метода
			private bool[] _IsInPareto;//входит ли ранжирование по индексу i в Парето-множество по векторам-характеристикам экспертов
			public double MinCost
			{
				get
				{
					if (!IsInitialized(_MinCost))
						_MinCost = min(parent_method.Rankings.Select(x => x.Cost.Value).ToList());
					return _MinCost;
				}
			}
			public double MaxCost
			{
				get
				{
					if (!IsInitialized(_MaxCost))
						_MaxCost = max(parent_method.Rankings.Select(x => x.Cost.Value).ToList());
					return _MaxCost;
				}
			}
			public double MinStrength
			{
				get
				{
					if (!IsInitialized(_MinStrength))
						_MinStrength = min(parent_method.Rankings.Select(x => x.Strength.Value).ToList());
					return _MinStrength;
				}
			}
			public double MaxStrength
			{
				get
				{
					if (!IsInitialized(_MaxStrength))
						_MaxStrength = max(parent_method.Rankings.Select(x => x.Strength.Value).ToList());
					return _MaxStrength;
				}
			}
			public Ranking.PathSummaryDistanceClass MinDistance
			{
				get
				{
					if (!IsInitialized(_MinDistance))
					{
						_MinDistance = new Ranking.PathSummaryDistanceClass();
						_MinDistance.modulus.Value = min(parent_method.Rankings.Select(x => x.SummaryDistance.modulus.Value).ToList());
						_MinDistance.square.Value = min(parent_method.Rankings.Select(x => x.SummaryDistance.square.Value).ToList());
					}
					return _MinDistance;
				}
			}
			public Ranking.PathSummaryDistanceClass MaxDistance
			{
				get
				{
					if (!IsInitialized(_MaxDistance))
					{
						_MaxDistance = new Ranking.PathSummaryDistanceClass();
						_MaxDistance.modulus.Value = max(parent_method.Rankings.Select(x => x.SummaryDistance.modulus.Value).ToList());
						_MaxDistance.square.Value = max(parent_method.Rankings.Select(x => x.SummaryDistance.square.Value).ToList());
					}
					return _MaxDistance;
				}
			}
			public bool[] IsInPareto
			{
				get
				{
					if (_IsInPareto is null)
						_IsInPareto = QualeEInParetoSet(parent_method.Rankings.Select(x => x.CostsExperts).ToList());
					return _IsInPareto;
				}
			}
			private bool IsInitialized(double t)
			{
				return t != 0 && Math.Abs(t) != INF;
			}
			private bool IsInitialized(Ranking.PathSummaryDistanceClass t)
			{
				return t != null;
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
			private bool[] QualeEInParetoSet(List<Ranking.RankingCharacteristic> R)
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
			public static bool ParetoMORETHAN(List<double> R1, List<double> R2)
			{
				bool ans = false;
				if (R1.Count != R2.Count)
					throw new MyException(EX_bad_dimensions);
				for (int i = 0; i < R1.Count; i++)
				{
					if (R1[i] < R2[i])
						return false;//тогда уже не строго больше
					if (R1[i] > R2[i])//если есть хотя бы один элемент, который больше
						ans = true;
				}
				return ans;
			}
			public void Clear()
			{
				_MinCost = INF;
				_MaxCost = INF;
				_MinStrength = INF;
				_MaxStrength = INF;
				_MinDistance = null;
				_MaxDistance = null;
			}
		}
		#endregion SUBCLASSES

		#region PROPERTIES
		public bool IsExecute
		{
			get
			{
				return (VisualFormConnectedControls.ConnectedCheckBox != null) ?
					VisualFormConnectedControls.ConnectedCheckBox.Checked : false;
			}
		}
		public List<List<int>> Ranks2Lists
		{
			set { Rankings = value.Select(x => new Ranking(x)).ToList(); }
			get { return this.Rankings.Select(x => x.Rank2List).ToList(); }
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
		public List<Ranking> Rankings
		{
			get
			{
				if (_Rankings is null)
				{
					_Rankings = new List<Ranking>();
					RanksCharacteristics = null;
				}
				return _Rankings;
			}
			set
			{
				_Rankings = value;
				if (value is null)
					RanksCharacteristics = null;
			}
		}
		public RankingsCharacteristics RanksCharacteristics
		{
			get
			{
				if (_RanksCharacteristics is null)
				{
					_RanksCharacteristics = new RankingsCharacteristics(this);
				}
				return _RanksCharacteristics;
			}
			set { _RanksCharacteristics = value; }
		}
		public List<int> Winners
		{
			get
			{
				if (_Winners is null)
					_Winners = new List<int>();
				return _Winners;
			}
			set { _Winners = value; }
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
		public ConnectedControls VisualFormConnectedControls
		{
			get
			{
				if (_VisualFormConnectedControls is null)
				{
					_VisualFormConnectedControls = new ConnectedControls(this);
				}
				return _VisualFormConnectedControls;
			}
			set { _VisualFormConnectedControls = value; }
		}
		#endregion PROPERTIES

		#region FUNCTIONS
		/// <summary>
		/// удаление ранжирований и их характеристик
		/// </summary>
		public void ClearResults()
		{
			Rankings = null;
			Winners = null;
			Levels = null;
		}
		#endregion FUNCTIONS
	}

	/// <summary>
	/// операции с файлами
	/// </summary>
	public static class FileOperations
	{
		/// <summary>
		/// поиск файлов в директориях
		/// </summary>
		/// <param name="directory_with_file"></param>
		/// <param name="file_name"></param>
		/// <returns></returns>
		public static bool FindFile(string file_name, out string absolute_file_name)
		{
			string directory_with_file = Path.GetDirectoryName(file_name);
			bool emptydirname = new object[] { null, "" }.Contains(directory_with_file);
			file_name = Path.GetFileName(file_name);
			Console.WriteLine($"PROJECT_DIRECTORY = {PROJECT_DIRECTORY}");
			directory_with_file = Path.Combine(PROJECT_DIRECTORY, directory_with_file);
			string[] allFoundFiles;
			if (!emptydirname)
			{
				allFoundFiles = Directory.GetFiles(directory_with_file, $"{file_name}*",
					SearchOption.TopDirectoryOnly);
			}
			else
			{
				allFoundFiles = Directory.GetFiles(directory_with_file, $"{file_name}*",
					SearchOption.AllDirectories);
			}
			if (allFoundFiles.Length > 0)
			{
				absolute_file_name = allFoundFiles.First();
				return true;
			}
			absolute_file_name = "";
			return false;
		}

		/// <summary>
		/// создать файл с переданным текстом
		/// </summary>
		/// <param name="text"></param>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static string WriteToFile(string text, string filename, bool add)
		{
			string directory_with_file = Path.GetDirectoryName(filename);
			filename = Path.GetFileName(filename);
			directory_with_file = Path.Combine(PROJECT_DIRECTORY, directory_with_file);
			bool emptydirname = new object[] { null, "" }.Contains(directory_with_file);
			//достать название папки, если есть указание папки
			var absolute_file_name = Path.Combine(directory_with_file, filename);
			if (add)
			{
				using (StreamWriter writer = new StreamWriter(absolute_file_name, true))
				{
					writer.WriteLineAsync(CR_LF);
					writer.WriteLineAsync(text);
				}
			}
			else
			{
				File.WriteAllText(absolute_file_name, text);
			}
			return absolute_file_name;
		}
	}
}
