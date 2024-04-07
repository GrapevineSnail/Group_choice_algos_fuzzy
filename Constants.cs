﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Group_choice_algos_fuzzy
{
	public class Constants
	{
		public class MyException : Exception
		{
			public MyException(string message) : base(message) { }
			#region EXCEPTIONS and INFO
			public static string EX_bad_expert_profile = "Введите корректные профили экспертов";
			public static string EX_matrix_not_square = "Матрица должна быть квадратной";
			public static string EX_bad_dimensions = "Размерности двух объектов не совпадают";
			public static string EX_bad_matrix = "Некорректная матрица";
			public static string EX_bad_fuzzy_relation_matrix = "Некорректная матрица принадлежности нечёткого отношения";
			public static string EX_matrix_was_normalized = "Элементы матрицы были нормализованы (значения приведены в интервал [0;1])";
			public static string EX_bad_file = "Некорректный файл";
			public static string EX_n_m_too_big =
				$"Число альтернатив n и/или число экспертов m слишком большое. Программа может зависнуть{CR_LF}" +
				$"n максимальное = {max_count_of_alternatives}{CR_LF}" +
				$"m максимальное = {max_count_of_experts}{CR_LF}";
			public static string EX_choose_method = "Выберите метод агрегирования";
			public static string EX_choose_distance_func = "Выберите способ подсчёта расстояния между отношениями";
			public static string EX_bad_symbol = "Неверный символ";
			public static string EX_contains_cycle = "Введённое отношение содержит цикл";
			public static string INF_ranking_unavailable = "Строгое ранжирование невозможно. ";
			#endregion EXCEPTIONS and INFO
			/// <summary>
			/// выводить при catch 
			/// </summary>
			public void Info()
			{
				MessageBox.Show(this.Message.ToString());
			}
		}

		#region FEILDS
		#region COMMON
		public const int max_count_of_alternatives = 9;
		public const int max_count_of_experts = 50;
		#endregion COMMON

		#region SYMBOLS
		public const string CR_LF = "\r\n";//вариант перевода строки - carriage return, line feed
		public const double INF = double.PositiveInfinity;
		public const double NO_EDGE = 0;//если стоит в ячейке матрицы, то в соотв. графе нет этого ребра
		public const int DIGITS_PRECISION = 12;//насколько точными будут вычисления на double
		public const string ZER = "0";
		public const string ONE = "1";
		public const char PLS = '+';
		public const char MARK = 'a';
		private const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		/// <summary>
		/// символ альтернативы (a1,a2,A,B,a,b...) в индекс 
		/// </summary>
		public static Dictionary<string, int> sym2ind = new Dictionary<string, int>();
		/// <summary>
		/// индекс альтернативы в её символ a1, a2 и т.д.
		/// </summary>
		public static Dictionary<int, string> ind2sym = new Dictionary<int, string>();
		/// <summary>
		/// индекс альтернативы в её буквенное обозначение, если возможно (букв всего 26)
		/// </summary>
		public static Dictionary<int, string> ind2letter = new Dictionary<int, string>();
		#endregion SYMBOLS

		#region VISUAL_INTERFACE
		public const string font = "Book Antiqua";
		public const string font_mono = "Courier New";
		public const float font_size = 10;
		public const float button_borderwidth = 5;
		public static Color font_color = Color.Black;
		public static Color error_color = ColorTranslator.FromHtml("#FFBBBB");
		public static Color input_bg_color = Color.White;
		public static Color input_bg_color_disabled = ColorTranslator.FromHtml("#FFCCCCCC");
		public static Color output_characteristics_bg_color = ColorTranslator.FromHtml("#FFDFBFFA");
		public static Color output_characteristics_min_color = ColorTranslator.FromHtml("#BBEEFF");
		public static Color output_characteristics_max_color = ColorTranslator.FromHtml("#FFEEBB");
		public static Color output_characteristics_mutual_color = ColorTranslator.FromHtml("#D0FFBB");
		public static Color window_bg_color = Color.AntiqueWhite; //ColorTranslator.FromHtml("#FAEBD7");
		public static Color button_bg_color = Color.Bisque; //ColorTranslator.FromHtml("#FFE4C4");
		public static Color button_bg_color_disabled = ColorTranslator.FromHtml("#D9D9D9");
		public static Color datagridview_bg_color = System.Drawing.Color.Gray;
		public static Microsoft.Msagl.Drawing.Color node_color = Microsoft.Msagl.Drawing.Color.PaleGreen;
		#endregion VISUAL_INTERFACE

		#region FILE_OPERATIONS
		public static string PROJECT_DIRECTORY = new DirectoryInfo(
			AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
		public static string DIRECTORY_WITH_TESTS = "Manual_tests";
		public static string OUT_FILE = "result.txt";
		public const string MAINTAINED_EXTENSION = ".txt";
		#endregion FILE_OPERATIONS

		#region METHODS' IDs
		public const int MET_ALL_RANKINGS = 0;
		public const int MET_ALL_HP = 1;
		public const int MET_HP_MAX_LENGTH = 2;
		public const int MET_HP_MAX_STRENGTH = 3;
		public const int MET_SCHULZE_METHOD = 4;
		public const int MET_SMERCHINSKAYA_YASHINA_METHOD = 5;
		/// <summary>
		/// название на ествественном языке для вывода в интерфейс
		/// </summary>
		public static Dictionary<int, string> MethodName = new Dictionary<int, string>{
			{ MET_ALL_RANKINGS, "Всевозможные ранжирования" },
			{ MET_ALL_HP, "Гамильтоновы пути" },
			{ MET_HP_MAX_LENGTH, "Гамильтоновы пути максимальной стоимости" },
			{ MET_HP_MAX_STRENGTH, "Гамильтоновы пути максимальной силы" },
			{ MET_SCHULZE_METHOD, "Ранжирование и победители по Алгоритму Шульце" },
			{ MET_SMERCHINSKAYA_YASHINA_METHOD, "Ранжирования, агрегированные по расстоянию, с разбиением контуров" }
		};
		#endregion METHODS' IDs

		#region NAMING
		public const string CH_COST = "стоимость";
		public const string CH_STRENGTH = "сила";
		public const string CH_DIST_SQUARE = "cумм. расст. 'квадрат разности'";
		public const string CH_DIST_MODULUS = "cумм. расст. 'модуль разности'";
		public const string CH_COST_EXPERTS = "стоимость по каждому эксперту-характеристике";
		public const string CH_STRENGTH_EXPERTS = "сила по каждому эксперту-характеристике";
		public const string RE_R = "Агрегированное отношение R";
		public const string RE_R_Asym = "Асимметричная часть Asymetric(R) агрегированного отношения R";
		public const string RE_R_Tr = "Транзитивное замыкание Tr(R) агрегированного отношения R";
		public const string RE_R_Acyc = "Отношение с разбитыми циклами Acyclic(R) агрегированного отношения R";
		public const string RE_R_Acyc_Tr = "Транзитивное замыкание Tr(Acyclic(R)) отношения с разбитыми циклами Acyclic(R) агрегированного отношения R";
		#endregion NAMING

		#endregion FEILDS		

		#region FUNCTIONS
		public static bool DoubleMORETHAN(double A, double B)
		{
			return Math.Round(A - B, DIGITS_PRECISION) > 0;//A > B +- epsilon
		}
		/// <summary>
		/// задание символов, обозначающих альтернативы
		/// </summary>
		/// <param name="n">количество альтернатив (размерность квадратной матрицы предпочтений)</param>
		public static void SetSymbolsForAlternatives(int n)
		{
			for (int i = 0; i < n; i++)
			{
				sym2ind[$"{MARK}{i}"] = i;
				ind2sym[i] = $"{MARK}{i}";
				if (n <= 26)
				{
					sym2ind[$"{letters[i]}"] = i;
					sym2ind[$"{letters[i]}{i:00}"] = i;
					sym2ind[$"{char.ToLower(letters[i])}"] = i;
				}
				ind2letter[i] = n > 26 ? ind2sym[i] : string.Format("{0}{1:00}", letters[i], i);
			}
		}
		#endregion FUNCTIONS

	}
}