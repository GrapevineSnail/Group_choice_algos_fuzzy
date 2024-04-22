using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Group_choice_algos_fuzzy
{
	public interface IFromGraphsDraw 
	{
		void Redraw(Dictionary<string, double[,]> labeled_matrices);
	}
}
