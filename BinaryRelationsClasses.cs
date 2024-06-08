using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Constants.MyException;
using static Group_choice_algos_fuzzy.ClassOperations;
using System.Text.RegularExpressions;

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
					{
						ans[i, j] = Math.Max(OPS_Double.Minus(this[i, j], this[j, i]), 0);
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
		public FuzzyRelation Cast2Fuzzy { get { return new FuzzyRelation(this); } }
		#endregion PROPERTIES

		#region OPERATORS
		public static Matrix operator *(double c, Matrix R1)
		{
			var R = new Matrix(R1);
			for (int i = 0; i < R.n; i++)
				for (int j = 0; j < R.m; j++)
					R[i, j] = OPS_Double.Mult(c, R[i, j]);
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
					R[i, j] = OPS_Double.Trunc(a_ij);
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
					R[i, j] = OPS_Double.Plus(R[i, j], R2[i, j]);
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
		{//решить, что матрицы не равны - легче,
		 //потому что надо найти первый несовпадающий элемент и выйти из цикла
			return !(R1 != R2);
		}
		#endregion OPERATORS

		#region FUNCTIONS
		/// <summary>
		/// есть ли ребро на основании символов, обозначающих отсутствие ребра
		/// </summary>
		public bool HasEdge((int i, int j) edge, double[] no_edge_symbols)
		{
			return !no_edge_symbols.Contains(this[edge.i, edge.j]);
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
		/// выводит список смежности матрицы на основании того, какое значение элемента матрицы считать отсутствием ребра
		/// </summary>
		/// <param name="no_edge_symbol">
		/// какое значение элемента матрицы считать отсутствием ребра (0, INF, -INF и т.д.)
		/// </param>
		/// <returns></returns>
		public List<List<int>> AdjacencyList(double[] no_edge_symbols)
		{
			return AdjacencyList(x => no_edge_symbols.Contains(x));
		}
		/// <summary>
		/// выводит список смежности матрицы на основании того, какое значение элемента матрицы считать отсутствием ребра
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
		/// для удобства печати матриц
		/// </summary>
		/// <param name="separator_type">0 - пробел, 1 - "[" и "]", 2 - TAB, default - ""</param>
		/// <param name="to_level">выравнивать ли</param>
		/// <returns></returns>
		public string Matrix2String(uint separator_type, bool to_level)
		{
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
			switch (separator_type)
			{
				case 0:
					rig_bnd = " ";
					break;
				case 1:
					lef_bnd = "[";
					rig_bnd = "]";
					break;
				case 2:
					rig_bnd = TAB;
					break;
				default:
					lef_bnd =  "";
					rig_bnd =  "";
					break;
			}
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					if (to_level)
					{
						//var fill = "_";
						//var align = "^";
						int width = m > 5 ? max_widths[j] + 2 : max_widths.Max() + 2;
						//str += string.Format("[{0:{fill}{align}{width}}]", Matrix[i, j], fill, align, width);
						str += string.Format($"{lef_bnd}{{0,{width}}}{rig_bnd}", this[i, j]);
					}
					else
					{
						str += string.Format($"{lef_bnd}{this[i, j]}{rig_bnd}");
					}
				}
				str += CR_LF;
			}
			str = OPS_String.Clean_RepeatingTabsAndCRLF(str);
			str = OPS_String.Clean_StringBeginAndEnd(str);
			return str;
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
			{
				for (int j = 0; j < R.m; j++)
				{
					R[i, j] = this[j, i];
				}
			}
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
			int med_index = M_list.Count / 2; // так как нумерация с 0, деление целочисленное
			for (int i = 0; i < R.n; i++)
			{
				for (int j = 0; j < R.m; j++)
				{
					var Rij_list = M_list.Select(x => x[i, j]).OrderBy(y => y).ToArray();
					R[i, j] = M_list.Count % 2 == 1 ? Rij_list[med_index] :
						OPS_Double.Divis(
							OPS_Double.Plus(Rij_list[med_index - 1], Rij_list[med_index]), 2.0);
				}
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
			return OPS_Double.Trunc(ans);
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
		public double MinElemExistEdge(double no_edge_symbol)
		{
			var elems = this.GetElemValues();
			double ans = INF;
			foreach (var e in elems)
				if (e < ans && e != no_edge_symbol)
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
			Func<double, double, double> f = (x, y) => Math.Abs(OPS_Double.Minus(x, y));
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
			Func<double, double, double> f = (x, y) => Math.Pow(OPS_Double.Minus(x, y), 2);
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
		public bool IsAdjacency()
		{
			for (int i = 0; i < n; i++)
				for (int j = i; j < n; j++)
					if (!(this[i, j] == 0 || this[i, j] == 1))
						return false;
			return true;
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
		/// есть ли в графе цикл, учитывая символы, обозначающие отсутствие дуги
		/// </summary>
		/// <param name="no_edge_symbol"></param>
		/// <returns></returns>
		public bool IsHasCycle(double[] no_edge_symbols)
		{
			return IsHasCycle(this.AdjacencyList(no_edge_symbols));
		}
		/// <summary>
		/// есть ли в графе цикл, учитывая символ, обозначающий отсутствие дуги
		/// </summary>
		/// <param name="no_edge_symbol"></param>
		/// <returns></returns>
		public bool IsHasCycle(double no_edge_symbol)
		{
			return IsHasCycle(this.AdjacencyList(no_edge_symbol));
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
			double squeeze = this.GetElemValues().Select(x => OPS_Double.Minus(x, shift)).Max();
			if (!IsNormalized)
			{
				for (int i = 0; i < n; i++)
					for (int j = 0; j < m; j++)
						R[i, j] = OPS_Double.Divis(OPS_Double.Minus(R[i, j], shift), squeeze);
			}
			return R;
		}
		/// <summary>
		/// удаляет ребраЮ соответствующие петлям
		/// </summary>
		/// <returns></returns>
		public Matrix DeleteSolitaryLoops(out int SolitaryLoopsCnt, double no_edge_symbol)
		{
			Matrix R = new Matrix(this);
			SolitaryLoopsCnt = 0;
			for (int i = 0; i < n; i++)
				if (R[i, i] != no_edge_symbol)
				{
					SolitaryLoopsCnt++;
					R[i, i] = no_edge_symbol;
				}
			return R;
		}
		/// <summary>
		/// сравнивалась ли альтернатива с какой-либо другой
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool IsAlternativeCompared(int i)
		{
			for (int j = 0; j < m; j++)
			{
				if (i != j)
				{
					if (this[i, j] != 0 || this[j, i] != 0)
						return true;
				}
			}
			return false;
		}
		/// <summary>
		/// альтернативы, которые имеют сравнение с какой-либо другой альтернативой
		/// </summary>
		/// <returns></returns>
		public bool[] ComparedAlternatives()
		{
			bool[] ans = new bool[n];
			for (int i = 0; i < n; i++)
			{
				ans[i] = IsAlternativeCompared(i);
			}
			return ans;
		}
		/// <summary>
		/// заполняет 1 присутствующие ребра, 0 - отсутствие ребра
		/// </summary>
		/// <param name="no_edge_symbols"></param>
		/// <returns></returns>
		public int[,] EdgesMask(double[] no_edge_symbols)
		{
			int[,] ans = new int[n, m];
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					ans[i, j] = this.HasEdge((i, j), no_edge_symbols) ? 1 : 0;
				}
			}
			return ans;
		}
		/// <summary>
		/// количество ребер в графе
		/// </summary>
		/// <param name="count_solitary_loop">подсчитывать ли петли (ребро из вершины в неё саму)</param>
		/// <returns></returns>
		public int EdgesCount(bool count_solitary_loop, double[] no_edge_symbols)
		{
			int ans = 0;
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					if (!count_solitary_loop && i == j)
					{ }
					else
					{
						if (this.HasEdge((i, j), no_edge_symbols))
						{
							ans++;
						}
					}
				}
			}
			return ans;
		}
		/// <summary>
		/// количество ребер в графе
		/// </summary>
		/// <param name="count_solitary_loop">подсчитывать ли петли (ребро из вершины в неё саму)</param>
		/// <returns></returns>
		public int EdgesCount(bool count_solitary_loop, double no_edge_symbol)
		{
			return EdgesCount(count_solitary_loop, new double[] { no_edge_symbol });
		}
		/// <summary>
		/// количество ребер в графе
		/// </summary>
		/// <param name="count_solitary_loop">подсчитывать ли петли (ребро из вершины в неё саму)</param>
		/// <returns></returns>
		public int EdgesCount(bool count_solitary_loop)
		{
			return EdgesCount(count_solitary_loop, new double[] { NO_EDGE, INF });
		}
		/// <summary>
		/// поэлементное умножение матриц
		/// </summary>
		/// <param name="M1"></param>
		/// <param name="M2"></param>
		/// <returns></returns>
		public static Matrix MultElementwise(Matrix M1, Matrix M2)
		{
			var M = new Matrix(M1);
			for (int i = 0; i < M.n; i++)
				for (int j = 0; j < M.m; j++)
					M[i, j] = OPS_Double.Mult(M[i, j], M2[i, j]);
			return M;
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
		/// <summary>
		/// оставить ненулевыми элементы, соответствующие единицам маски
		/// </summary>
		/// <param name="Mask"></param>
		/// <returns></returns>
		public Matrix SelectByMask01(Matrix Mask)
		{
			if(!Mask.IsAdjacency())
				throw new MyException(EX_bad_matrix);
			var M = new Matrix(this);
			for (int i = 0; i < M.n; i++)
				for (int j = 0; j < M.m; j++)
					if (Mask[i, j] == 0)
						M[i, j] = 0;
			return M;
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
		private static bool is_value_of_membership_func(double mu_ij)
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
		private static bool is_fuzzy_relation_matrix(double[,] M)
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
					_Asymmetric = this.AsymmetricPart.Cast2Fuzzy;
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
					_Symmetric = this.SymmetricPart.Cast2Fuzzy;
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
			return this.Transpose().Cast2Fuzzy;
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
				return Eye(n).Cast2Fuzzy;
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
		public new bool IsHasCycle(double no_edge_symbol)
		{
			return base.IsHasCycle(no_edge_symbol);
		}
		/// <summary>
		/// разбить циклы
		/// </summary>
		/// <returns></returns>
		public FuzzyRelation DestroyCycles()
		{
			var R = new FuzzyRelation(this);
			var RA = R.AdjacencyMatrix.Cast2Fuzzy;
			Matrix RK = Matrix.Eye(1); //матрица контуров
			while (true)
			{//если ещё остались контуры
				var RAT = RA.TransitiveClosure();
				RK = RA.Intersect1(RAT).Intersect1(RAT.Transpose().Cast2Fuzzy);
				if (RK.ElemSum() == 0)
					break;
				for (int i = 0; i < RK.n; i++)
					for (int j = 0; j < RK.m; j++)
						if (RK[i, j] != 0)
							RK[i, j] = R[i, j];

				var min_elem = RK.MinElemExistEdge(NO_EDGE);
				for (int i = 0; i < R.n; i++)
					for (int j = 0; j < R.m; j++)
						if (RK[i, j] != 0 && R[i, j] == min_elem)
						{
							R[i, j] = NO_EDGE;
							RA[i, j] = 0;
						}
			}
			return R;
		}
		/// <summary>
		/// является ли матрица матрицей нечеткого отношения
		/// </summary>
		/// <returns></returns>
		public static bool IsFuzzyRelationMatrix(Matrix M)
		{
			if (is_fuzzy_relation_matrix(M.matrix_base))
				return true;
			return false;
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
		/// преобразование списка Matrix в список FuzzyRelation
		/// </summary>
		/// <param name="R_list"></param>
		/// <returns></returns>
		public static List<FuzzyRelation> ToFuzzyList(List<Matrix> R_list)
		{
			return R_list.Select(x => x.Cast2Fuzzy).ToList();
		}		
		#endregion FUNCTIONS
	}
}
