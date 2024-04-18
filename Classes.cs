using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Constants.MyException;
using static Group_choice_algos_fuzzy.OPS_DataGridView;
using static Group_choice_algos_fuzzy.Model;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.GraphViewerGdi;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using static System.IO.DirectoryInfo;
using System.ComponentModel;

namespace Group_choice_algos_fuzzy
{
	/// <summary>
	/// операции с double - с округлением до нужной точности
	/// </summary>
	public static class OPS_Double
	{// если выдаёт 0.099999999999999978 вместо 0.1
		/// <summary>
		/// A > B +- epsilon
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns></returns>
		public static bool MORETHAN(double A, double B)
		{
			return Math.Round(A - B, DIGITS_PRECISION) > 0;
		}
		/// <summary>
		/// A == B +- epsilon
		/// </summary>
		/// <param name="A"></param>
		/// <param name="B"></param>
		/// <returns></returns>
		public static bool EQUALS(double A, double B)
		{
			return Math.Round(A - B, DIGITS_PRECISION) == 0;
		}
		public static bool LESSTHAN(double A, double B)
		{
			return MORETHAN(B,A);
		}
		public static double Plus(double A, double B)
		{
			return Math.Round(A + B, DIGITS_PRECISION);
		}
		public static double Minus(double A, double B)
		{
			return Math.Round(A - B, DIGITS_PRECISION);
		}
		public static double Mult(double A, double B)
		{
			return Math.Round(A * B, DIGITS_PRECISION);
		}
		public static double Trunc(double A)
		{
			return Math.Round(A, DIGITS_PRECISION);
		}
	}
	/// <summary>
	/// операции с файлами
	/// </summary>
	public static class OPS_File
	{
		/// <summary>
		/// поиск файлов в директориях
		/// </summary>
		/// <param name="directory_with_file"></param>
		/// <param name="file_name"></param>
		/// <returns></returns>
		public static bool FindFile(string file_name, out string absolute_file_name)
		{
			absolute_file_name = "";
			try
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
			}
			catch (DirectoryNotFoundException ex)
			{
				throw new MyException($"{ex.Message}");
			}
			catch { }
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
					writer.WriteLine(CR_LF);
					writer.WriteLine(text);
				}
			}
			else
			{
				File.WriteAllText(absolute_file_name, text);
			}
			return absolute_file_name;
		}

		public static List<Matrix> ReadFileWithMatrices(string filename, out string absolute_file_name)
		{
			List<Matrix> matrices = new List<Matrix>();
			FindFile(filename, out absolute_file_name);
			try
			{
				if (absolute_file_name == "")
					throw new FileNotFoundException();
				string[] lines = File.ReadAllLines(absolute_file_name)
					.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();//ReadAllLines вызывает FileNotFoundException
				filename = absolute_file_name;
				char[] chars_for_split = new char[] { ' ', '	' };
				int _n = lines.First().Split(chars_for_split, StringSplitOptions.RemoveEmptyEntries).Count();
				Matrix cur_matrix = new Matrix(_n, _n);
				for (int l = 0; l < lines.Length; l++)
				{
					if (lines[l].Length != 0)
					{
						double res;
						double[] numbers = lines[l].Split(chars_for_split, StringSplitOptions.RemoveEmptyEntries)
							.Select(x => double.TryParse(x, out res) ? res : INF).ToArray();
						if (numbers.Any(x => x == INF) || numbers.Length != _n)
							throw new MyException(EX_bad_file);
						for (int j = 0; j < numbers.Length; j++)
							cur_matrix[l % _n, j] = numbers[j];
					}
					if (l % _n == _n - 1)
						matrices.Add(new Matrix(cur_matrix));
				}
				if (matrices.Count == 0)
					throw new MyException(EX_bad_file);
				//m = matrices.Count;
				//n = _n;
			}
			catch (FileNotFoundException ex)
			{
				throw new MyException($"{ex.Message}");
			}
			return matrices;
		}
	}
	/// <summary>
	/// операции с рисованием графов на форме
	/// </summary>
	public static class OPS_GraphDrawing
	{
		/// <summary>
		/// отрисовать граф по матрице в PictureBox
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="pictureBox"></param>
		/// <returns></returns>
		public static Bitmap DrawGraph(double[,] matrix, PictureBox pictureBox)
		{
			try
			{
				var G = GenerateGraph(matrix);
				Bitmap bitmap = DrawBitmap(G, pictureBox);
				//bitmap.Save("graph_visualizing_output.png");
				pictureBox.SizeChanged += (object sender, EventArgs e) =>
				{
					DrawBitmap(G, (PictureBox)sender);
				};
				return bitmap;
			}
			catch { }
			return null;
		}

		/// <summary>
		/// создадим орграф
		/// </summary>
		/// <param name="M">матрица весов орграфа</param>
		/// <returns></returns>
		public static Microsoft.Msagl.Drawing.Graph GenerateGraph(double[,] M)
		{
			if (M.GetLength(0) != M.GetLength(1))
				throw new MyException(EX_matrix_not_square);
			int n = M.GetLength(0);
			Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("");
			for (int i = 0; i < n; i++)
			{
				Microsoft.Msagl.Drawing.Node node = graph.AddNode(ind2letter[i]);
				node.Attr.LabelMargin = 1;
				node.Attr.FillColor = node_color;
				node.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Circle;
				for (int j = 0; j < n; j++)
				{
					if (M[i, j] != 0 && Math.Abs(M[i, j]) != INF)
					{
						Microsoft.Msagl.Drawing.Edge edge = graph.AddEdge(ind2letter[i], string.Format("{0:0.####}", M[i, j]), ind2letter[j]);
						edge.Label.FontSize = node.Label.FontSize * 0.75;
					}
				}
			}
			return graph;
		}

		/// <summary>
		/// создать картинку графа (битмап)
		/// </summary>
		/// <param name="g"></param>
		/// <param name="drawing_field"></param>
		/// <returns></returns>
		public static Bitmap DrawBitmap(Microsoft.Msagl.Drawing.Graph g, PictureBox drawing_field)
		{
			Microsoft.Msagl.GraphViewerGdi.GraphRenderer renderer = new Microsoft.Msagl.GraphViewerGdi.GraphRenderer(g);
			renderer.CalculateLayout();
			Bitmap bitmap = new Bitmap(
				(int)drawing_field.Width, (int)drawing_field.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
			renderer.Render(bitmap);
			drawing_field.Image = (Image)bitmap;
			return bitmap;
		}

		/// <summary>
		/// обновить рисунки графов
		/// </summary>
		/// <param name="form"></param>
		/// <param name="M"></param>
		/// <param name="L"></param>
		public static void OrgraphsPics_update(IFromGraphsDraw form, List<Matrix> M, List<string> L)
		{
			if (form != null && !((Form)form).IsDisposed)
			{
				form.Redraw(M.Select(x => x.matrix_base).ToList(), L);
			}
		}
	}
	/// <summary>
	/// операции с DataGridView
	/// </summary>
	public static class OPS_DataGridView
	{
		public static int SetColumn(DataGridView dgv, string header)
		{
			int j = dgv.Columns.Count;
			DataGridViewColumn column = new DataGridViewColumn();
			column.CellTemplate = new DataGridViewTextBoxCell();
			column.SortMode = DataGridViewColumnSortMode.NotSortable;
			column.HeaderCell.Style.BackColor = window_bg_color;
			column.HeaderText = header;
			column.MinimumWidth = 15;
			column.FillWeight = 1;
			dgv.Columns.Add(column);
			return j;
		}
		public static int SetRow(DataGridView dgv, string header)
		{
			int i = dgv.Rows.Count;
			DataGridViewRow row = new DataGridViewRow();
			row.HeaderCell.Value = header;
			row.MinimumHeight = 15;
			dgv.Rows.Add(row);
			return i;
		}
		public static void SetReadonlyCell(DataGridView dgv, int i, int j, string value, Color clr)
		{
			dgv[j, i].ReadOnly = true;
			dgv[j, i].Style.BackColor = clr;
			dgv[j, i].Value = value;
		}
		public static void SetDoubleCell(DataGridView dgv, int i, int j, double value)
		{
			dgv[j, i].ValueType = typeof(double);
			dgv[j, i].Value = value;
		}
		public static void ClearDGV(DataGridView dgv)
		{
			dgv?.Rows.Clear();
			dgv?.Columns.Clear();
			//Dispose не делать, вредно
		}
		/// <summary>
		/// достаёт матрицу из DataGridView
		/// </summary>
		public static Matrix GetFromDataGridView(DataGridView dgv)
		{
			var input_matrix = new Matrix(dgv.Rows.Count, dgv.Columns.Count);
			for (int i = 0; i < input_matrix.n; i++)
				for (int j = 0; j < input_matrix.m; j++)
				{
					input_matrix[i, j] =
						double.TryParse(dgv[j, i]?.Value?.ToString(), out var Mij) ?
						Mij : 0;
				}
			return input_matrix;
		}
		/// <summary>
		/// кладёт матрицу в DataGridView
		/// </summary>
		public static void SetToDataGridView(Matrix M, DataGridView dgv)
		{
			for (int i = 0; i < M.n; i++)
				for (int j = 0; j < M.m; j++)
					SetDoubleCell(dgv, i, j, M[i, j]);
		}		
		private static void SetDataGridViewDefaults_FontAndColors(DataGridView dgv)
		{
			//dgv.DefaultCellStyle.Font = new Font(font, font_size);
			dgv.DefaultCellStyle.ForeColor = font_color;
			dgv.DefaultCellStyle.BackColor = input_bg_color;
			dgv.DefaultCellStyle.SelectionForeColor = font_color;
			dgv.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.Empty;
		}
		/// <summary>
		/// настрйки для вывода DataGridView
		/// </summary>
		/// <param name="dgv"></param>
		public static void SetDataGridViewDefaults(DataGridView dgv)
		{
			dgv.AllowUserToAddRows = false;
			dgv.AllowUserToDeleteRows = false;
			dgv.AllowUserToResizeRows = true;
			dgv.AllowUserToResizeColumns = true;
			dgv.AllowUserToOrderColumns = false;
			dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
			dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
			dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			dgv.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			dgv.ShowEditingIcon = true;
			dgv.DefaultCellStyle.Format = $"0.{new string('#', DIGITS_PRECISION)}";
			dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
			SetDataGridViewDefaults_FontAndColors(dgv);
			dgv.DataError += (object ss, DataGridViewDataErrorEventArgs anError) => { dgv.CancelEdit(); };
		}
		public static void ColorCell(DataGridView dgv, int row, int col, System.Drawing.Color color)
		{
			dgv[col, row].Style.BackColor = color;
		}
		public static void ColorSymmetricCell(object sender, DataGridViewCellEventArgs e)
		{
			var dd = sender as DataGridView;
			int i = e.RowIndex;
			int j = e.ColumnIndex;
			if (i == j)
				ColorCell(dd, i, j, input_bg_color_disabled);
			else
			{
				double Mij, Mji;
				var p1 = double.TryParse(dd[j, i]?.Value?.ToString(), out Mij);
				var p2 = double.TryParse(dd[i, j]?.Value?.ToString(), out Mji);
				ColorCell(dd, i, j, input_bg_color);
				ColorCell(dd, j, i, input_bg_color);
				if (p1 && p2)
				{
					if (Mij == 0 && Mji != 0)
					{
						ColorCell(dd, i, j, input_bg_color_disabled);
					}
					else if (Mij != 0 && Mji == 0)
					{
						ColorCell(dd, j, i, input_bg_color_disabled);
					}
				}
			}
		}
		public static void ColorSymmetricCells(DataGridView dgv)
		{
			for (int i = 0; i < dgv.Rows.Count; i++)
				for (int j = 0; j < dgv.Columns.Count; j++)
					ColorSymmetricCell(dgv, new DataGridViewCellEventArgs(j, i));
		}
	}


	/// <summary>
	/// связанные с сущностью control-ы на форме
	/// </summary>
	abstract public class ConnectedControlsBase
	{
		private Label connectedLabel;//в какой контейнер выводить текстовые пояснения
		public Label ConnectedLabel
		{
			get
			{
				if (connectedLabel is null)
				{
					connectedLabel = new Label();
					connectedLabel.AutoSize = true;
				}
				return connectedLabel;
			}
			set
			{
				if (value is null)
				{
					if (!(connectedLabel is null))
						connectedLabel.Text = "";
					connectedLabel?.Hide();
					//connectedLabel?.Dispose();//этого делать не надо!
				}
				else
				{
					connectedLabel = value;
					connectedLabel?.Show();
				}
			}
		}
		abstract public void UI_Show();
		abstract public void UI_Clear();
	}

	/// <summary>
	/// каждая характеристика (ранжирования, метода) имеет числовое значение и осмысленное наименование 
	/// </summary>
	public class Characteristic
	{
		public Characteristic(string label) { Label = label; }
		public Characteristic(string label, double value) { Label = label; Value = value; }
		private string _Label;//название характеристики
		private double _Value = INF;//числовое значение х.
		private List<double> _ValuesList;//векторное значение х.
		private double _ValueMin;//если есть список характеристик, в который данная х. входит
		private double _ValueMax;//будут вычислены min и max
		public string Label
		{
			get
			{
				if (_Label is null)
					_Label = "";
				return _Label;
			}
			set { _Label = value; }
		}
		public double Value
		{
			get { return _Value; }
			set { _Value = value; }
		}
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
		public double ValueMin { get { return _ValueMin; } }
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
	}

	/// <summary>
	/// суммарное расстояние до всех остальных входных ранжирований
	/// </summary>
	public class RankingSummaryDistanceClass
	{
		public Characteristic modulus = new Characteristic(CH_DIST_MODULUS);
		public Characteristic square = new Characteristic(CH_DIST_SQUARE);
	}

	/// <summary>
	/// одно какое-то ранжирование (или путь) со всеми его свойствами
	/// </summary>
	public class Ranking
	{
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
		public int MethodID;//каким методом получено
		public Characteristic Cost;//общая стоимость пути
		public Characteristic CostsExperts; //вектор стоимостей по каждому эксперту-характеристике
		public Characteristic Strength; //сила пути (пропускная способность)
		public Characteristic StrengthsExperts; //вектор сил по каждому эксперту-характеристике
		public RankingSummaryDistanceClass SummaryDistance;//суммарное расстояние до всех остальных входных ранжирований
		public RankingSummaryDistanceClass SummaryDistanceExperts;//расстояние входного ранжирования каждого эксперта - вектор
		#endregion FIELDS

		#region PROPERTIES
		public List<int> Rank2List
		{
			get { return _Path; }
			set
			{
				_Path = value;
				UpdateRankingParams(AggregatedMatrix.R, ExpertRelations.GetModelMatrices());
			}
		}
		public int[] Rank2Array
		{
			get { return _Path.ToArray(); }
			set
			{
				_Path = value.ToList();
				UpdateRankingParams(AggregatedMatrix.R, ExpertRelations.GetModelMatrices());
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
		/// <summary>
		/// создаёт не транзитивную матрицу смежности из профиля эксперта
		/// </summary>
		public Matrix Rank2NontransitiveMatrix
		{
			get
			{
				var node_list = Rank2Array;
				var l = Rank2Array.Length;
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
		private void UpdateRankingParams(Matrix weight_matrix, List<Matrix> other_matrices)
		{
			MethodID = -1;
			Cost = null;
			CostsExperts = null;
			Strength = null;
			StrengthsExperts = null;
			SummaryDistance = null;
			SummaryDistanceExperts = null;
			if (!(_Path is null))
			{
				Cost = new Characteristic(CH_COST, PathCost(Rank2List, weight_matrix));
				Strength = new Characteristic(CH_STRENGTH, PathStrength(Rank2List, weight_matrix));
				if (other_matrices != null)
				{
					CostsExperts = new Characteristic(CH_COST_EXPERTS);
					StrengthsExperts = new Characteristic(CH_STRENGTH_EXPERTS);
					foreach (var expert_matrix in other_matrices)
					{
						CostsExperts.ValuesList.Add(PathCost(Rank2List, expert_matrix));
						StrengthsExperts.ValuesList.Add(PathStrength(Rank2List, expert_matrix));
					}
					if (Rank2List.Count == n)
					{
						SummaryDistance = new RankingSummaryDistanceClass();
						SummaryDistance.modulus.Value = Rank2Matrix.SumDistance(other_matrices, Matrix.DistanceModulus);
						SummaryDistance.square.Value = Rank2Matrix.SumDistance(other_matrices, Matrix.DistanceSquare);
						SummaryDistance.modulus.Label += _CH_WHOLE_SUM;
						SummaryDistance.square.Label += _CH_WHOLE_SUM;

						SummaryDistanceExperts = new RankingSummaryDistanceClass();
						SummaryDistanceExperts.modulus.ValuesList = other_matrices
							.Select(x => Matrix.DistanceModulus(Rank2Matrix, x)).ToList();
						SummaryDistanceExperts.square.ValuesList = other_matrices
							.Select(x => Matrix.DistanceSquare(Rank2Matrix, x)).ToList();
						SummaryDistanceExperts.modulus.Label += _CH_ON_EACH_EXPERT;
						SummaryDistanceExperts.square.Label += _CH_ON_EACH_EXPERT;

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
		private List<int> _Winners;//победители - недоминируемые альтернативы
		private List<List<int>> _Levels;//разбиение графа отношения на уровни (алг. Демукрона, начиная с конца - со стока)
		public ConnectedControls UI_Controls;
		#endregion FIELDS

		#region SUBCLASSES
		/// <summary>
		/// связанные с методом элементы управления (control-ы) на форме
		/// </summary>
		public class ConnectedControls : ConnectedControlsBase
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
							.Select(x => x.Rank2Matrix.Matrix2String(false)).ToArray());
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

				if (!parent_method.HasRankings)
				{
					parent_method.UI_Controls.ConnectedLabel.Text = INF_ranking_unavailable;
					if (parent_method.HasLevels)
					{//ранжирований нет, но можно задать разбиение на уровни
						SetColumn(parent_method.UI_Controls.ConnectedTableFrame, $"Разбиение{CR_LF}на уровни");
						for (int i = 0; i < parent_method.Levels.Count; i++)
						{
							SetRow(parent_method.UI_Controls.ConnectedTableFrame, RowHeaderForRankingAndLevel(i));
						}
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
					void display_characteristic(int i, int j, double min, double max,
						bool highlight_this_best, Characteristic Characteristic)
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
						if (Characteristic.IsInitializedValuesList())
						{
							cell_text += string.Join(CR_LF, Characteristic.ValuesList);
							if (highlight_this_best)
								cell_colour = output_characteristics_Pareto_color;
						}
						SetReadonlyCell(parent_method.UI_Controls.ConnectedTableFrame,
							i, j, cell_text, cell_colour);
					}
					for (int j = 0; j < parent_method.Rankings.Count; j++)
					{
						SetColumn(parent_method.UI_Controls.ConnectedTableFrame, $"Ранжиро-{CR_LF}вание {j + 1}");
					}
					var some_rank = parent_method.Rankings.First();
					for (int i = 0; i < some_rank.Count; i++)
					{
						SetRow(parent_method.UI_Controls.ConnectedTableFrame, RowHeaderForRankingAndLevel(i));
					}
					SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.Cost.Label);
					SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.Strength.Label);
					SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.SummaryDistance.modulus.Label);
					SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.SummaryDistance.square.Label);
					SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.CostsExperts.Label);
					SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.StrengthsExperts.Label);
					SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.SummaryDistanceExperts.modulus.Label);
					SetRow(parent_method.UI_Controls.ConnectedTableFrame, some_rank.SummaryDistanceExperts.square.Label);
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
						display_characteristic(N, j,
							MethodCh.MinMaxCost.ValueMin,
							MethodCh.MinMaxCost.ValueMax,
							false,
							parent_method.Rankings[j].Cost);
						display_characteristic(N + 1, j,
							MethodCh.MinMaxStrength.ValueMin,
							MethodCh.MinMaxStrength.ValueMax,
							false,
							parent_method.Rankings[j].Strength);
						display_characteristic(N + 2, j,
							MethodCh.MinMaxDistance.modulus.ValueMin,
							MethodCh.MinMaxDistance.modulus.ValueMax,
							false,
							parent_method.Rankings[j].SummaryDistance.modulus);
						display_characteristic(N + 3, j,
							MethodCh.MinMaxDistance.square.ValueMin,
							MethodCh.MinMaxDistance.square.ValueMax,
							false,
							parent_method.Rankings[j].SummaryDistance.square);
						display_characteristic(N + 4, j,
							INF,
							INF,
							MethodCh.IsInPareto_Cost[j],
							parent_method.Rankings[j].CostsExperts);
						display_characteristic(N + 5, j,
							INF,
							INF,
							MethodCh.IsInPareto_Strength[j],
							parent_method.Rankings[j].StrengthsExperts);

						display_characteristic(N + 6, j,
							INF,
							INF,
							MethodCh.IsInPareto_DistModulus[j],
							parent_method.Rankings[j].SummaryDistanceExperts.modulus);
						display_characteristic(N + 7, j,
							INF,
							INF,
							MethodCh.IsInPareto_DistSquare[j],
							parent_method.Rankings[j].SummaryDistanceExperts.square);
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
		/// <summary>
		/// характеристики совокупности ранжирований метода
		/// </summary>
		public class MethodRankingsCharacteristics
		{
			public MethodRankingsCharacteristics(Method m) { parent_method = m; }
			private readonly Method parent_method;
			private Characteristic _MinMaxCost;//мин и макс стоимость среди ранжирований метода
			private Characteristic _MinMaxStrength;//мин и макс сила среди ранжирований метода
			private RankingSummaryDistanceClass _MinMaxDistance;//мин и макс суммарн. расстояние среди ранжирований метода
			private bool[] _IsInPareto_Cost;//входит ли ранжирование по индексу i в Парето-множество по векторам-характеристикам экспертов
			private bool[] _IsInPareto_Strength;
			private bool[] _IsInPareto_DistModulus;
			private bool[] _IsInPareto_DistSquare;
			public Characteristic MinMaxCost
			{
				get
				{
					if (_MinMaxCost is null)
						_MinMaxCost = new Characteristic(CH_COST);
					if (IsMethodExistWithRanks(parent_method))
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
					if (IsMethodExistWithRanks(parent_method))
					{
						_MinMaxStrength.SetMinMax(parent_method.Rankings.Select(x => x.Strength.Value).ToList());
					}
					return _MinMaxStrength;
				}
			}
			public RankingSummaryDistanceClass MinMaxDistance
			{
				get
				{
					if (IsMethodExistWithRanks(parent_method))
					{
						_MinMaxDistance = new RankingSummaryDistanceClass();
						_MinMaxDistance.square.SetMinMax(
							parent_method.Rankings.Select(x => x.SummaryDistance.square.Value).ToList()
							);
						_MinMaxDistance.modulus.SetMinMax(
							parent_method.Rankings.Select(x => x.SummaryDistance.modulus.Value).ToList()
							);
						_MinMaxDistance.square.Label += _CH_WHOLE_SUM;
						_MinMaxDistance.modulus.Label += _CH_WHOLE_SUM;
					}
					return _MinMaxDistance;
				}
			}
			public bool[] IsInPareto_Cost
			{
				get
				{
					if (_IsInPareto_Cost is null)
						_IsInPareto_Cost = QualeEInParetoSet(parent_method.Rankings
							.Select(x => x.CostsExperts).ToList(), "max");
					return _IsInPareto_Cost;
				}
			}
			public bool[] IsInPareto_Strength
			{
				get
				{
					if (_IsInPareto_Strength is null)
						_IsInPareto_Strength = QualeEInParetoSet(parent_method.Rankings
							.Select(x => x.StrengthsExperts).ToList(), "max");
					return _IsInPareto_Strength;
				}
			}
			public bool[] IsInPareto_DistModulus
			{
				get
				{
					if (_IsInPareto_DistModulus is null)
						_IsInPareto_DistModulus = QualeEInParetoSet(parent_method.Rankings
							.Select(x => x.SummaryDistanceExperts.modulus).ToList(), "min");
					return _IsInPareto_DistModulus;
				}
			}
			public bool[] IsInPareto_DistSquare
			{
				get
				{
					if (_IsInPareto_DistSquare is null)
						_IsInPareto_DistSquare = QualeEInParetoSet(parent_method.Rankings
							.Select(x => x.SummaryDistanceExperts.square).ToList(), "min");
					return _IsInPareto_DistSquare;
				}
			}
			/// <summary>
			/// задаёт индексы ранжирований, входящих в Парето-множество
			/// </summary>
			/// <param name="R"></param>
			/// <param name="min_or_max">минимизирующий или максимизирющий критери: "min"/"max"</param>
			/// <returns></returns>
			private bool[] QualeEInParetoSet(List<Characteristic> R, string min_or_max)
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
				var r = R.Count;
				var isInPareto = new bool[r];
				for (int i = 0; i < r; i++)
				{
					isInPareto[i] = true;
					for (int j = 0; j < r; j++)
					{
						var Vj = R[j].ValuesList;
						var Vi = R[i].ValuesList;
						if (min_or_max == "min")
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
				if (Winners is null || Winners.Count == 0)
					return false;
				return true;
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
		public List<int> Winners
		{
			get
			{
				if (_Winners is null && Levels?.Count > 0)
					_Winners = Levels?.First();
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
		public string Winners2String
		{
			get
			{
				if (Winners is null)
					return "";
				return string.Join(",", Winners?.Select(x => ind2letter[x]));
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
			if (met is null || !met.HasRankings)
				return false;
			return true;
		}
		/// <summary>
		/// удаление ранжирований и их характеристик
		/// </summary>
		public void Clear()
		{
			Rankings = null;
			Winners = null;
			Levels = null;
			RankingsCharacteristics = null; //очищено в свойстве
		}
		public string Info()
		{
			string text = "";
			string TextTemplateAmong(Characteristic ch)
			{
				return $"Мин. и макс. {ch?.Label} среди ранжирований метода: " +
					$"[{ch?.ValueMin}; {ch?.ValueMax}];{CR_LF}";
			}
			text += $"Недоминируемые альтернативы: {Winners2String}{CR_LF}";
			text += TextTemplateAmong(RankingsCharacteristics.MinMaxCost);
			text += TextTemplateAmong(RankingsCharacteristics.MinMaxStrength);
			text += TextTemplateAmong(RankingsCharacteristics.MinMaxDistance?.square);
			text += TextTemplateAmong(RankingsCharacteristics.MinMaxDistance?.modulus);
			return text;
		}
		#endregion FUNCTIONS
	}

}
