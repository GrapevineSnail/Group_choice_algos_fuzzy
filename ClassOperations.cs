using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Constants.MyException;
using static Group_choice_algos_fuzzy.ClassOperations.OPS_String;
using System.Xml;
using System.Text.RegularExpressions;


namespace Group_choice_algos_fuzzy
{
	/// <summary>
	/// самописные операции для разных классов
	/// </summary>
	public static class ClassOperations
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
				return MORETHAN(B, A);
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
			public static double Divis(double A, double B)
			{
				return Math.Round(A / B, DIGITS_PRECISION);
			}
			public static double Trunc(double A)
			{
				return Math.Round(A, DIGITS_PRECISION);
			}
		}
		/// <summary>
		/// операции со string, с regular expressions
		/// </summary>
		public static class OPS_String
		{
			/// <summary>
			/// удаляет последние cnt символов из строки
			/// </summary>
			/// <param name="s"></param>
			/// <param name="cnt"></param>
			/// <returns></returns>
			public static string TrimEnd(string s, int cnt)
			{
				var start = s.Length - cnt;
				if (start < 0)
					return "";
				return s.Remove(s.Length - cnt, cnt);
			}
			public static string Clean_RepeatingTabsAndCRLF(string str)
			{
				var ans = Regex.Replace(str, $"{TAB}{{2,}}", TAB);
				ans = Regex.Replace(ans, $"{CR_LF}{{2,}}", CR_LF);
				return ans;
			}
			public static string Clean_StringBeginAndEnd(string str)
			{
				var ans = Regex.Replace(str, $"({TAB}| ){CR_LF}", CR_LF);
				ans = Regex.Replace(ans, $"^({TAB}| )", "");
				return ans;
			}
			public static string[] Clean_DeleteSpasyLines(string[] lines)
			{
				return lines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
			}
			public static string Clean_Commas2Dots(string str)
			{
				return Regex.Replace(str, ",", ".");
			}
			public static string Clean_ShortenAlternativeNumber(string str)
			{
				var num = Regex.Match(str, @"\d+").Value;
				return MARK + int.Parse(num).ToString();
			}
			public static string[] SplitWithowtEmptyEntries(string str, char[] delimiters)
			{
				return str.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			}
			public static bool TryParseRankingAlternatives(List<string> signs, out List<int> alters)
			{
				alters = new List<int>();
				SetSymbolsForAlternatives(signs.Count);
				for (int i = 0; i < signs.Count; i++)
				{
					var alt = Clean_StringBeginAndEnd(signs[i]);
					alt = Clean_ShortenAlternativeNumber(alt);
					if (!sym2ind.ContainsKey(alt))
						return false;
					alters.Add(sym2ind[alt]);
				}
				return true;
			}
			public static bool TryParseDoubles(string[] input, out double[] numbers)
			{
				var str = input.Select(x => Clean_Commas2Dots(x));
				numbers = str.Select(x => double.TryParse(x, out double res) ? res : INF).ToArray();
				if (numbers.Any(x => x == INF))
					return false;
				return true;
			}
			public static List<string> GetRankingAlternatives(string str)
			{
				MatchCollection matches = REG_pattern_alternative.Matches(str);
				var ans = new List<string>();
				for (int i = 0; i < matches.Count; i++)
				{
					ans.Add(matches[i].Groups[REG_SYM_GROUPNAME].Value);
				}
				return ans;
			}
			public static bool GetRankingAlternatives(string str, out List<int> alters)
			{
				return TryParseRankingAlternatives(GetRankingAlternatives(str), out alters);
			}
			public static bool GetWeights(string str, out double[] weights)
			{
				var str_without_alters = REG_pattern_alternative
					.Replace(str, SEP_IN_LINE_SPLIT[0].ToString());
				return TryParseDoubles(
					SplitWithowtEmptyEntries(str_without_alters, SEP_IN_LINE_SPLIT), out weights);
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
			public static bool FindFile(string file_name, 
				out string absolute_file_name, out string error_text)
			{
				absolute_file_name = "";
				error_text = "";
				try
				{
					string directory_with_file = Path.GetDirectoryName(file_name);
					bool emptydirname = new object[] { null, "" }.Contains(directory_with_file);
					file_name = Path.GetFileName(file_name);
					directory_with_file = Path.Combine(PROJECT_DIRECTORY, directory_with_file);
					error_text += $"PROJECT_DIRECTORY = {PROJECT_DIRECTORY}{CR_LF}";
					error_text += $"directory_with_file = {directory_with_file}{CR_LF}";
					error_text += $"file_name = {file_name}{CR_LF}";
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
			/// <param name="add">добавить ли в конец (иначе - перезаписать)</param>
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
						writer.WriteLine(text);
					}
				}
				else
				{
					File.WriteAllText(absolute_file_name, text);
				}
				return absolute_file_name;
			}
			/// <summary>
			/// читает файл и выдает матрицы
			/// </summary>
			/// <param name="filename"></param>
			/// <param name="absolute_file_name">если файл найден, возвращает полный путь к файлу</param>
			/// <param name="one_test_matrices">одна группа матриц - одно экспертное совещание</param>
			/// <returns></returns>
			public static bool ReadFileOneTest(string filename, out string absolute_file_name,
				out List<Matrix> one_test_matrices, out string errtext_bad_string)
			{
				one_test_matrices = null;
				errtext_bad_string = "";
				if (!FindFile(filename, out absolute_file_name, out errtext_bad_string))
					return false;
				string[] lines = File.ReadAllLines(absolute_file_name);//ReadAllLines вызывает FileNotFoundException
				if (ReadMatrices(lines, out one_test_matrices))
					return true;
				else if (ReadRankings(lines, out one_test_matrices, out errtext_bad_string))
					return true;
				return true;
			}
			/// <summary>
			/// читает файл и выдает несколько групп матриц
			/// </summary>
			/// <param name="filename"></param>
			/// <param name="absolute_file_name">если файл найден, возвращает полный путь к файлу</param>
			/// <param name="several_tests">несколько групп матриц - несколько экспертных совещаний</param>
			/// <returns></returns>
			public static bool ReadFileSeveralTests(string filename, out string absolute_file_name,
				out List<List<Matrix>> several_tests, out List<string[]> unparsedtext_several_tests, 
				out string error_text)
			{
				several_tests = new List<List<Matrix>>();
				unparsedtext_several_tests = new List<string[]>();
				error_text = "";
				if (!FindFile(filename, out absolute_file_name, out error_text))
					return false;
				XmlDocument xDoc = new XmlDocument();
				try
				{
					xDoc.Load(absolute_file_name);
				}
				catch(Exception ex)
				{
					error_text = EX_bad_file + CR_LF + ex.Message;
					return false;
				}
				XmlElement xRoot = xDoc.DocumentElement;
				if (xRoot is null)
					return false;
				List<bool> is_test_correct = new List<bool>();
				foreach (XmlNode childnode in xRoot.ChildNodes)
				{
					string[] lines = SplitWithowtEmptyEntries(childnode.InnerText, SEP_IN_TEXT_SPLIT);
					unparsedtext_several_tests.Add(lines);
					bool is_OK = ReadMatrices(lines, out List<Matrix> single_test_matrices);
					is_test_correct.Add(is_OK);
					several_tests.Add(single_test_matrices);
				}
				return is_test_correct.Any();
			}
			/// <summary>
			/// считывает матрицы из строк
			/// </summary>
			/// <param name="absolute_file_name"></param>
			/// <param name="matrices"></param>
			/// <returns>возвращает - удалось ли считать набор матриц (1 тест)</returns>
			private static bool ReadMatrices(string[] lines, out List<Matrix> matrices)
			{
				matrices = new List<Matrix>();
				lines = Clean_DeleteSpasyLines(lines);
				if (lines.Count() == 0)
					return false;
				int read_n = SplitWithowtEmptyEntries(lines.First(), SEP_IN_LINE_SPLIT).Count();
				Matrix cur_matrix = new Matrix(read_n);
				for (int l = 0; l < lines.Length; l++)
				{
					if (lines[l].Length > 0)
					{
						bool success = TryParseDoubles(SplitWithowtEmptyEntries(lines[l], SEP_IN_LINE_SPLIT),
							out double[] numbers);
						if (numbers.Length != read_n || !success)
							return false;
						for (int j = 0; j < numbers.Length; j++)
							cur_matrix[l % read_n, j] = numbers[j];
					}
					if (l % read_n == read_n - 1)
						matrices.Add(new Matrix(cur_matrix));
				}
				if (matrices.Count == 0)
					return false;
				return true;
			}
			/// <summary>
			/// считывает ранжирования экспертов из строк
			/// </summary>
			/// <param name="lines">строки формата "a0 1 a3 0.5 a1 0.99 a4 0 a2 ..."</param>
			/// <param name="matrices"></param>
			/// <returns></returns>
			private static bool ReadRankings(string[] lines,
				out List<Matrix> matrices, out string bad_string)
			{
				matrices = new List<Matrix>();
				bad_string = "";
				lines = Clean_DeleteSpasyLines(lines);
				if (lines.Count() == 0)
					return false;
				int read_n = GetRankingAlternatives(lines.First()).Count();
				for (int l = 0; l < lines.Length; l++)
				{
					Matrix cur_matrix = Matrix.Zeros(read_n, read_n);
					if (lines[l].Length > 0)
					{
						if (!GetRankingAlternatives(lines[l], out var alters)
							|| alters.Count != read_n
							|| alters.Count != alters.Distinct().Count())
						{
							bad_string = lines[l];
							return false;
						}
						if (!GetWeights(lines[l], out var weights)
							|| weights.Count() != alters.Count - 1)
						{
							bad_string = lines[l];
							return false;
						}
						for (int i = 0; i < alters.Count - 1; i++)
						{
							cur_matrix[alters[i], alters[i + 1]] = weights[i];
						}
						matrices.Add(new Matrix(cur_matrix));
					}
				}
				if (matrices.Count == 0)
					return false;
				return true;
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
					(int)drawing_field.Width, (int)drawing_field.Height,
					System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
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
			public static void UpdateOrgraphPics(IFromGraphsDraw form, Dictionary<string, Matrix> Lab_Mat)
			{
				if (form != null && !((Form)form).IsDisposed)
				{
					form.Redraw(Lab_Mat.ToDictionary(x => x.Key, x => x.Value.matrix_base));
				}
			}
		}
		/// <summary>
		/// операции с DataGridView
		/// </summary>
		public static class OPS_DataGridView
		{
			public static int SetColumn(DataGridView dgv)
			{
				int j = dgv.Columns.Count;
				DataGridViewColumn column = new DataGridViewColumn();
				column.CellTemplate = new DataGridViewTextBoxCell();
				column.SortMode = DataGridViewColumnSortMode.NotSortable;
				column.HeaderCell.Style.BackColor = window_bg_color;
				column.MinimumWidth = row_min_height;
				column.FillWeight = 1;
				dgv.Columns.Add(column);
				return j;
			}
			public static int SetColumn(DataGridView dgv, string header)
			{
				int j = SetColumn(dgv);
				dgv.Columns[j].HeaderText = header;
				return j;
			}
			public static int SetRow(DataGridView dgv)
			{
				int i = dgv.Rows.Count;
				DataGridViewRow row = new DataGridViewRow();
				row.MinimumHeight = row_min_height;
				dgv.Rows.Add(row);
				return i;
			}
			public static int SetRow(DataGridView dgv, string header)
			{
				int i = SetRow(dgv);
				dgv.Rows[i].HeaderCell.Value = header;
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
				dgv?.Rows?.Clear();
				dgv?.Columns?.Clear();
				//Dispose не делать, вредно
			}
			/// <summary>
			/// достаёт матрицу из DataGridView
			/// </summary>
			public static Matrix GetFromDGV(DataGridView dgv)
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
			public static void PutValues2DGVCells(DataGridView dgv, Matrix M)
			{
				for (int i = 0; i < M.n; i++)
					for (int j = 0; j < M.m; j++)
						SetDoubleCell(dgv, i, j, M[i, j]);
			}
			private static void SetDGVDefaults_FontAndColors(DataGridView dgv)
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
			public static void SetDGVDefaults_experts(DataGridView dgv)
			{
				dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
				dgv.AllowUserToAddRows = false;
				dgv.AllowUserToDeleteRows = false;
				dgv.AllowUserToResizeRows = true;
				dgv.AllowUserToResizeColumns = true;
				dgv.AllowUserToOrderColumns = false;
				dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
				dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
				dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
				dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
				dgv.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
				dgv.ShowEditingIcon = true;
				dgv.DefaultCellStyle.Format = $"0.{'0' + new string('#', DIGITS_PRECISION - 1)}";
				dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
				//dgv.ShowCellToolTips = true;
				//for (int j = 0; j < dgv.Columns.Count; j++)
				//	for (int i = 0; i < dgv.Rows.Count; i++)
				//		dgv[j, i].ToolTipText = dgv.Rows[i].HeaderCell.Value.ToString();
				SetDGVDefaults_FontAndColors(dgv);
				dgv.DataError += (object ss, DataGridViewDataErrorEventArgs anError) => { dgv.CancelEdit(); };
			}
			/// <summary>
			/// настрйки для вывода DataGridView
			/// </summary>
			/// <param name="dgv"></param>
			public static void SetDGVDefaults_methods(DataGridView dgv)
			{
				dgv.AllowUserToAddRows = false;
				dgv.AllowUserToDeleteRows = false;
				dgv.AllowUserToResizeRows = true;
				dgv.AllowUserToResizeColumns = true;
				dgv.AllowUserToOrderColumns = false;
				dgv.RowHeadersWidth = row_headers_width;
				dgv.ShowEditingIcon = true;
				dgv.DefaultCellStyle.Format = $"0.{new string('#', DIGITS_PRECISION)}";
				dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
				//dgv.ShowCellToolTips = true;
				//for (int j = 0; j < dgv.Columns.Count; j++)
				//	for (int i = 0; i < dgv.Rows.Count; i++)
				//		dgv[j, i].ToolTipText = dgv.Rows[i].HeaderCell.Value.ToString();
				SetDGVDefaults_FontAndColors(dgv);
				dgv.DataError += (object ss, DataGridViewDataErrorEventArgs anError) => { dgv.CancelEdit(); };
			}
			public static void AddDGVColumnsAndRows(DataGridView dgv, int col_cnt, int row_cnt)
			{
				for (int j = 0; j < col_cnt; j++)
				{
					SetColumn(dgv);
				}
				for (int i = 0; i < row_cnt; i++)
				{
					SetRow(dgv);
				}
			}
			public static void SetDGVHeaders(DataGridView dgv, string[] col_headers, string[] row_headers)
			{
				for (int j = 0; j < dgv.Columns.Count; j++)
				{
					dgv.Columns[j].HeaderText = col_headers?[j];
				}
				for (int i = 0; i < dgv.Rows.Count; i++)
				{
					dgv.Rows[i].HeaderCell.Value = row_headers?[i];
				}
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
			public static void ColorSymmetricCells(object sender, DataGridViewCellEventArgs e)
			{
				var dgv = sender as DataGridView;
				for (int i = 0; i < dgv.Rows.Count; i++)
					for (int j = 0; j < dgv.Columns.Count; j++)
						ColorSymmetricCell(dgv, new DataGridViewCellEventArgs(j, i));
			}
			public static System.Drawing.Size GetTableSize(DataGridView dgv)
			{
				var Width = dgv.Columns.GetColumnsWidth(DataGridViewElementStates.Visible)
					+ dgv.RowHeadersWidth + row_min_height;
				var Height = dgv.Rows.GetRowsHeight(DataGridViewElementStates.Visible)
					+ dgv.ColumnHeadersHeight + row_min_height;
				return new System.Drawing.Size(Width, Height);
			}
		}
	}
}
