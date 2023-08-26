using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Group_choice_algos_fuzzy
{
	public class Constants
	{
		public const float button_borderwidth = 5;
		public const string font = "Book Antiqua";
		public const string font_mono = "Courier New";
		public const float font_size = 10;
		public static Color error_color = ColorTranslator.FromHtml("#FFBBBB");
		public static Color input_bg_color = Color.White;
		public static Color output_characteristics_bg_color = ColorTranslator.FromHtml("#FFDFBFFA");
		public static Color disabled_input_bg_color = ColorTranslator.FromHtml("#FFCCCCCC");
		public static Color color_min = ColorTranslator.FromHtml("#BBEEFF");
		public static Color color_max = ColorTranslator.FromHtml("#FFEEBB");
		public static Color color_mutual = ColorTranslator.FromHtml("#D0FFBB");
		public static Color window_background = Color.AntiqueWhite; //ColorTranslator.FromHtml("#FAEBD7");
		public static Color button_background = Color.Bisque; //ColorTranslator.FromHtml("#FFE4C4");
		public static Color disabled_button_background = ColorTranslator.FromHtml("#D9D9D9");

		public const string out_file = "result.txt";
		public const string CR_LF = "\r\n";//вариант перевода строки - carriage return, line feed
		public const int max_count_of_alternatives = 9;
		public const int max_count_of_experts = 50;
		public const double INF = double.PositiveInfinity;
		public const int digits_precision = 12;//насколько точными будут вычисления на double
		public const string ZER = "0";
		public const string ONE = "1";
		public const char PLS = '+';
		public const char mark = 'a';
		//IDs
		public const int ALL_RANKINGS = 0;
		public const int ALL_HP = 1;
		public const int HP_MAX_LENGTH = 2;
		public const int HP_MAX_STRENGTH = 3;
		public const int SCHULZE_METHOD = 4;
		public const int SMERCHINSKAYA_YASHINA_METHOD = 5;
		//название на ествественном языке для вывода в интерфейс
		public static Dictionary<int, string> MethodsInfo = new Dictionary<int, string>{
			{ ALL_RANKINGS, "Всевозможные ранжирования" },
			{ HP_MAX_LENGTH, "Гамильтоновы пути максимальной стоимости" },
			{ HP_MAX_STRENGTH, "Гамильтоновы пути максимальной силы" },
			{ SCHULZE_METHOD, "Ранжирование и победители по Алгоритму Шульце" },
			{ SMERCHINSKAYA_YASHINA_METHOD, "Ранжирования, агрегированные по расстоянию (Смерчинская-Яшина)" }
		};
		public const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		//символ альтернативы (a1,a2,A,B,a,b...) в индекс 
		public static Dictionary<string, int> sym2ind = new Dictionary<string, int>();
		//индекс альтернативы в её символ a1, a2 и т.д.
		public static Dictionary<int, string> ind2sym = new Dictionary<int, string>();
		//индекс альтернативы в её буквенное обозначение, если возможно(букв всего 26)
		public static Dictionary<int, string> ind2letter = new Dictionary<int, string>();
		/// <summary>
		/// задание констант (при инициализации формы)
		/// </summary>
		/// <param name="n">количество альтернатив (размерность квадратной матрицы предпочтений)</param>
		public static void SetConstants(int n)
		{
			for (int i = 0; i < n; i++)
			{
				sym2ind[$"{mark}{i}"] = i;
				ind2sym[i] = $"{mark}{i}";
				sym2ind[$"{letters[i]}"] = i;
				sym2ind[$"{char.ToLower(letters[i])}"] = i;
				ind2letter[i] = n > 26 ? ind2sym[i] : letters[i].ToString();
			}
		}

		#region EXCEPTIONS
		public static string EX_bad_expert_profile = "Введите корректные профили экспертов";
		public static string EX_matrix_not_square = "Матрица должна быть квадратной";
		public static string EX_bad_dimensions = "Размерности двух объектов не совпадают";
		public static string EX_bad_matrix = "Некорректная матрица";
		public static string EX_bad_file = "Некорректный файл";
		public static string EX_n_m_too_big = 
			"Число альтернатив n и/или число экспертов m слишком большое. Программа может зависнуть\n" +
			$"n максимальное = {max_count_of_alternatives}\n" +
			$"m максимальное = {max_count_of_experts}\n";
		public static string EX_choose_method = "Выберите метод агрегирования";
		public static string EX_choose_distance_func = "Выберите способ подсчёта расстояния между отношениями";
		public static string EX_bad_symbol = "Неверный символ";
		public static string EX_not_transitive_profile = "Введённое отношение было не транзитивно (содержание цикла)";
		public class MyException : Exception
		{
			public MyException(string message) : base(message) { }
			public void Info() //выводить при catch
			{ MessageBox.Show(this.Message.ToString()); }
		}
		#endregion EXCEPTIONS


	}
}