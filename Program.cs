using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Group_choice_algos_fuzzy
{
	static class Program
	{
		/// <summary>
		/// Главная точка входа для приложения.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			var Main_form = new Form1();
			Main_form.ExpertMatricesInUI_EventHandler += Main_form.UpdateExpertMatrices;

			Application.Run(Main_form);

		}
	}
}
