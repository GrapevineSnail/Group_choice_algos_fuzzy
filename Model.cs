using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using static Group_choice_algos_fuzzy.Constants;
using static Group_choice_algos_fuzzy.Constants.MyException;
using static Group_choice_algos_fuzzy.DataGridViewOperations;
using static Group_choice_algos_fuzzy.GraphDrawingFuncs;
using System.IO;
using System.Drawing;

namespace Group_choice_algos_fuzzy
{
	class Model
	{
		#region FIELDS
		private static int _n;//количество альтернатив
		private static int _m;//количество экспертов
		public static Form1 form1;
		#endregion FIELDS

		#region PROPERTIES
		public static int n
		{
			set
			{
				_n = value;
				SetSymbolsForAlternatives(n);
			}
			get { return _n; }
		}
		public static int m
		{
			set
			{
				_m = value;
			}
			get { return _m; }
		}
		#endregion PROPERTIES

		/*
		public virtual void ExpertRelations_Update(object sender, ExpertRelations.ExpertRelationsEventArgs e)
		{
			EventHandler<ExpertRelations.ExpertRelationsEventArgs> handler = ExpertRelations.ExpertRelations_EventHandler;
			handler?.Invoke(this, e);
		}
		*/

		/// <summary>
		/// матрицы нечетких отношений экспертов
		/// </summary>
		public static class ExpertRelations
		{
			#region SUBCLASSES
			public class ConnectedControls : ConnectedControlsBase
			{
				public ConnectedControls(CheckBox cb_show, CheckBox cb_doTransClos,
					NumericUpDown n, NumericUpDown m, FlowLayoutPanel flp)
				{
					connectedCheckBox_ToShow = cb_show;
					connectedCheckBox_DoTransClosure = cb_doTransClos;
					numericUpDown_n = n;
					numericUpDown_m = m;
					connectedFlowLayoutPanel = flp;
				}
				public readonly CheckBox connectedCheckBox_ToShow;//выводить ли таблички для ввода
				public readonly CheckBox connectedCheckBox_DoTransClosure;//делать ли транз. замыкание после ввода
				public readonly NumericUpDown numericUpDown_n;
				public readonly NumericUpDown numericUpDown_m;
				private readonly FlowLayoutPanel connectedFlowLayoutPanel;//куда кладутся все datagridview экспертов
				public Control.ControlCollection GetOutputControls
				{
					get
					{
						return connectedFlowLayoutPanel?.Controls;
					}
				}
				public List<DataGridView> GetOutputDataGridViews
				{
					get
					{
						return GetOutputControls.OfType<DataGridView>().ToList();
						//	List<DataGridView> ans = new List<DataGridView>();
						//	foreach (Control c in connectedFlowLayoutPanel?.Controls)
						//	{
						//		if (c as DataGridView != null)
						//		{
						//			ans.Add(c as DataGridView);
						//		}
						//	}
						//	return ans;
					}
				}
				public List<Label> GetOutputLabels
				{
					get
					{
						return GetOutputControls.OfType<Label>().ToList();
					}
				}
				public bool IsNoConnectedTables()
				{
					if (GetOutputDataGridViews is null || GetOutputDataGridViews.Count == 0)
						return true;
					return false;
				}
				public bool IsNoConnectedLabels()
				{
					if (GetOutputLabels is null || GetOutputLabels.Count == 0)
						return true;
					return false;
				}
				public bool IsNoConnectedOutputs()
				{
					if (IsNoConnectedTables() && IsNoConnectedLabels())
						return true;
					return false;
				}
				public void UpdateViewNM()
				{
					if (UI_Controls.numericUpDown_n.Minimum <= n && n <= UI_Controls.numericUpDown_n.Maximum &&
						UI_Controls.numericUpDown_m.Minimum <= m && m <= UI_Controls.numericUpDown_m.Maximum)
					{
						UI_Controls.numericUpDown_n.Value = n;
						UI_Controls.numericUpDown_m.Value = m;
					}
				}
				public void UpdateModelNM()
				{
					if (form1.cb_All_rankings.Checked && (
					(int)numericUpDown_n.Value > max_count_of_alternatives ||
					(int)numericUpDown_m.Value > max_count_of_experts
					))
						throw new MyException(EX_n_m_too_big);
					n = (int)numericUpDown_n.Value;
					m = (int)numericUpDown_m.Value;
				}
				public DataGridView UpdateView(int expert_index)
				{
					var new_martix = GetModelMatrix(expert_index);
					if (new_martix is null)
						return null;
					DataGridView dgv = UI_Controls.GetOutputDataGridViews.ElementAtOrDefault(expert_index);
					dgv = new DataGridView();
					SetDataGridViewDefaults(dgv);
					dgv.CellEndEdit += CheckCellWhenValueChanged;
					dgv.CellEndEdit += ColorSymmetricCell;
					for (int j = 0; j < new_martix.m; j++)
					{
						SetColumn(dgv, $"{ind2letter[j]}");
					}
					for (int i = 0; i < new_martix.n; i++)
					{
						SetRow(dgv, $"{ind2letter[i]}");
					}
					Matrix.SetToDataGridView(new_martix, dgv);
					for (int i = 0; i < dgv.Rows.Count; i++)
						for (int j = 0; j < dgv.Columns.Count; j++)
							ColorSymmetricCell(dgv, new DataGridViewCellEventArgs(j, i));
					form1.set_controls_size();
					return dgv;
				}
				public void UpdateViewAll()
				{
					UI_Clear();
					try
					{
						UpdateViewNM();
						if (connectedCheckBox_ToShow.Checked)
						{
							for (int ex = 0; ex < m; ex++)
							{
								var dgv = UpdateView(ex);
								if (!(dgv is null))
									GetOutputControls.Add(dgv);
							}
							form1.set_controls_size();
						}
						UI_Show();
					}
					catch (MyException ex) { ex.Info(); }
				}
				override public void UI_Show()
				{
					if (connectedCheckBox_ToShow.Checked)
					{
						foreach (DataGridView dgv in GetOutputDataGridViews)
						{
							dgv?.Show();
							dgv?.Parent.Show();
						}
						foreach (Label lbl in GetOutputLabels)
						{
							lbl?.Show();
						}
					}
				}
				override public void UI_Clear()
				{
					ConnectedLabel.Dispose();
					ConnectedLabel = null;
					foreach (DataGridView dgv in GetOutputDataGridViews)
					{
						dgv?.Rows.Clear();
						dgv?.Columns.Clear();
						dgv?.Hide();
						dgv?.Parent?.Hide();
						dgv?.Dispose();
					}
					foreach (Label lbl in GetOutputLabels)
					{
						//lbl.Text = "";
						lbl?.Hide();
						lbl?.Dispose();
					}
					GetOutputControls.Clear();
				}
				public void UI_Activate()
				{
					foreach (DataGridView dgv in GetOutputDataGridViews)
					{
						for (int i = 0; i < dgv.RowCount; i++)
						{
							for (int j = 0; j < dgv.ColumnCount; j++)
							{
								ColorCell(dgv, i, j, input_bg_color);
							}
						}
						dgv.ReadOnly = false;
					}
				}
				public void UI_Deactivate()
				{
					foreach (DataGridView dgv in GetOutputDataGridViews)
					{
						for (int i = 0; i < dgv.RowCount; i++)
						{
							for (int j = 0; j < dgv.ColumnCount; j++)
							{
								ColorCell(dgv, i, j, input_bg_color_disabled);
							}
						}
						dgv.ReadOnly = true;
					}
				}
			}
			public class ExpertRelationsEventArgs : EventArgs
			{
				public ExpertRelationsEventArgs() { }
				public ExpertRelationsEventArgs(int exp_ind, Matrix mat)
				{
					expert_index = exp_ind;
					fill_values = mat;
				}
				public ExpertRelationsEventArgs(int exp_ind, Matrix mat, List<Matrix> exp_mats)
				{
					expert_index = exp_ind;
					fill_values = mat;
					expert_matrices = exp_mats;
				}
				public ExpertRelationsEventArgs(int row, int col)
				{
					this.cell_args = new DataGridViewCellEventArgs(col, row);
				}
				/// <summary>
				/// в какого эксперта
				/// </summary>
				public int expert_index { get; set; }
				/// <summary>
				/// что положить
				/// </summary>
				public Matrix fill_values { get; set; }
				/// <summary>
				/// что положить вообще все матрицы экспертов
				/// </summary>
				public List<Matrix> expert_matrices { get; set; }
				public DataGridViewCellEventArgs cell_args { get; set; }
			}
			#endregion SUBCLASSES

			#region FIELDS
			//public delegate void ExpertMatricesInUI_EventHandler(object sender, ExpertMatricesEventArgs e);
			public static event EventHandler<ExpertRelationsEventArgs> ExpertRelations_InputRelChanged;
			public static event EventHandler<ExpertRelationsEventArgs> ExpertRelations_InputRelsChanged;
			public static event EventHandler<ExpertRelationsEventArgs> ExpertRelations_ModelRelChanged;
			public static event EventHandler<ExpertRelationsEventArgs> ExpertRelations_ModelRelsChanged;
			private static List<Matrix> _RList;
			//public static List<Matrix> RListMatrix;
			public static ConnectedControls UI_Controls;
			//
			/// 
			///// 
			/*static public int comparsion_trials = -1;//сколько отредактированных ячеек уже было
			private static void comparsion_trials_upd()
			{
				if (comparsion_trials < 0)
					comparsion_trials = 0;
				comparsion_trials++;
				if (comparsion_trials >= n - 1)
					comparsion_trials = 0;//= n-1
			}*/


			#endregion FIELDS

			/*
			public static List<Matrix> RListMatrix
			{
				get
				{
					if (_RList is null || _RList.Count == 0)
					{
						_RList = new List<Matrix>(m);
						for(int ex = 0; ex<m; ex++)
						{
							_RList.Add(new Matrix(n));
						}
					}
					return _RList;
				}
				set
				{
					_RList = value;
					bool some_matrices_have_cycle = false;
					for (int i = 0; i < _RList?.Count; i++)
					{
						if (!FuzzyRelation.IsFuzzyRelationMatrix(_RList[i]))
						{
							_RList[i] = _RList[i].NormalizeAndCast2Fuzzy;
						}
						if (_RList[i].Cast2Fuzzy.IsHasCycle())
						{
							some_matrices_have_cycle = true;
						}
					}
					if (some_matrices_have_cycle)
					{
						throw new MyException(EX_contains_cycle);
					}
					ExpertRelations_ModelRelationsChanged?.Invoke(null,
						new ExpertRelationsEventArgs(-1, null, _RList));
				}
			}
			*/
			private static Matrix NewModelMatrix(Matrix new_matrix)
			{
				new_matrix = new_matrix.NormalizeAndCast2Fuzzy;
				if (new_matrix.Cast2Fuzzy.IsHasCycle())
				{
					throw new MyException(EX_contains_cycle);
				}
				else
				{
					if (UI_Controls.connectedCheckBox_DoTransClosure.Checked)
					{
						var L = new List<int>();
						L.Add(n-1);
						for(int i =n-2; i>0; i--)
						{
							L.Add(L.Last() + i);
						}

						if (new_matrix.ComparedAlternatives().All(x => x == true) ||
							L.Contains(new_matrix.EdgesCount(false)) )//||comparsion_trials == 0
						{
							if (!new_matrix.Cast2Fuzzy.IsTransitive())
							{
								new_matrix = new_matrix.Cast2Fuzzy.TransitiveClosure();
							}
						}
					}
				}
				return new_matrix;
			}
			public static void UpdateModelMatrix(int index, Matrix new_matrix)
			{
				if (GetModelMatrix(index) is null)
					return;
				_RList[index] = NewModelMatrix(new_matrix);
				ExpertRelations_ModelRelChanged?.Invoke(null,
					new ExpertRelationsEventArgs(index, GetModelMatrix(index), null));
			}
			public static void UpdateModelMatrices(List<Matrix> new_matrices)
			{
				_RList = new_matrices;
				if (_RList is null)
					return;
				_RList = _RList.Select(x => NewModelMatrix(x)).ToList();
				if (_RList.Any(x => x.Cast2Fuzzy.IsHasCycle()))
				{
					throw new MyException(EX_contains_cycle);
				}
				ExpertRelations_ModelRelsChanged?.Invoke(null,
					new ExpertRelationsEventArgs(-1, null, GetModelMatrices()));
			}
			public static Matrix GetModelMatrix(int index)
			{
				if (GetModelMatrices().Count == 0)
					return null;
				return GetModelMatrices()[index];
			}
			public static List<Matrix> GetModelMatrices()
			{
				if (_RList is null)
					_RList = new List<Matrix>();
				return new List<Matrix>(_RList);
			}
			public static void Clear()
			{
				//RListMatrix = null;
				UpdateModelMatrices(null);
			}
			private static void ColorCell(DataGridView dgv, int row, int col, System.Drawing.Color color)
			{
				dgv[col, row].Style.BackColor = color;
			}
			static void ColorSymmetricCell(object sender, DataGridViewCellEventArgs e)
			{
				var dd = sender as DataGridView;
				int i = e.RowIndex;
				int j = e.ColumnIndex;
				if (i == j)
					ColorCell(dd, i, j, input_bg_color_disabled);
				else
				{
					double Mij, Mji;
					double.TryParse(dd[j, i]?.Value?.ToString(), out Mij);
					double.TryParse(dd[i, j]?.Value?.ToString(), out Mji);
					ColorCell(dd, i, j, input_bg_color);
					ColorCell(dd, j, i, input_bg_color);
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
			static void CheckCellWhenValueChanged(object sender, DataGridViewCellEventArgs e)
			{//что должно происходить при завершении редактирования ячейки
				try
				{
					var dd = sender as DataGridView;
					///////
					//int exp_index = flowLayoutPanel_input_tables.Controls.GetChildIndex(dd);
					int exp_index = UI_Controls.GetOutputDataGridViews.IndexOf(dd);
					///////
					///									
					int i = e.RowIndex;
					int j = e.ColumnIndex;
					double Mij, Mji;
					var p = double.TryParse(dd[j, i]?.Value?.ToString(), out Mij);
					double.TryParse(dd[i, j]?.Value?.ToString(), out Mji);
					if (!p || Mij > 1 || Mij < 0 || i == j)
					{
						dd[j, i].Value = 0.0;
					}
					else
					{
						//comparsion_trials_upd();
					}
					ExpertRelations_InputRelChanged?.Invoke(
						sender, new ExpertRelationsEventArgs(exp_index, Matrix.GetFromDataGridView(dd)));
				}
				catch (MyException ex) { ex.Info(); }
			}
			public static void UpdateExpertDataGridView(object sender, ExpertRelationsEventArgs e)
			{
				UI_Controls.UpdateView(e.expert_index);
			}
			public static void UpdateExpertDataGridViews(object sender, ExpertRelationsEventArgs e)
			{
				UI_Controls.UpdateViewAll();
			}
			public static void UpdateExpertMatrix(object sender, ExpertRelationsEventArgs e)
			{
				try
				{
					UpdateModelMatrix(e.expert_index, e.fill_values);
				}
				catch (MyException ex) { ex.Info(); }
			}
			public static void UpdateExpertMatrices(object sender, ExpertRelationsEventArgs e)
			{				
				try
				{
					UpdateModelMatrices(e.expert_matrices);
				}
				catch (MyException ex) { ex.Info(); }
			}
			/// <summary>
			/// обновить рисунки графов - матриц экспертов
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			public static void UpdateExpertGraph(object sender, ExpertRelationsEventArgs e)
			{
				var M = GetModelMatrices();
				var L = new List<string>();
				for (int i = 0; i < M.Count; i++)
				{
					L.Add($"Expert{i}:");
				}
				OrgraphsPics_update(Form1.form3_input_expert_matrices, M, L);
			}

		}

		/// <summary>
		/// ResultRelation - агрегированная матрица матриц профилей
		/// </summary>
		public static class AggregatedMatrix
		{
			public class ConnectedControls : ConnectedControlsBase
			{
				public ConnectedControls(RadioButton rb_square, RadioButton rb_modulus, Label lbl)
				{
					Connected_rb_dist_square = rb_square;
					Connected_rb_dist_modulus = rb_modulus;
					ConnectedLabel = lbl;
				}
				private RadioButton connected_rb_dist_square;
				private RadioButton connected_rb_dist_modulus;
				public RadioButton Connected_rb_dist_square
				{
					get
					{
						if (connected_rb_dist_square is null)
							throw new MyException(EX_choose_distance_func);
						return connected_rb_dist_square;
					}
					set { connected_rb_dist_square = value; }//set text
				}
				public RadioButton Connected_rb_dist_modulus
				{
					get
					{
						if (connected_rb_dist_square is null)
							throw new MyException(EX_choose_distance_func);
						return connected_rb_dist_modulus;
					}
					set { connected_rb_dist_modulus = value; }//set text
				}
				override public void UI_Show()
				{
					var tex = "";
					void for_print_matrices(string name, Matrix M)
					{
						tex += $"{CR_LF}{name}:{CR_LF}";
						tex += M?.Matrix2String(true);
					}
					if (R != null)
					{
						for_print_matrices(RE_R, R);
						for_print_matrices(RE_R_Asym, R.Asymmetric);
						for_print_matrices(RE_R_Tr, R.TransClosured);
						for_print_matrices(RE_R_Acyc, R.DestroyedCycles);
						for_print_matrices(RE_R_Acyc_Tr, R.DestroyedCycles.TransClosured);
					}
					ConnectedLabel.Text = tex;
					ConnectedLabel.Show();
				}
				override public void UI_Clear()
				{
					ConnectedLabel.Text = "";
					ConnectedLabel.Dispose();
					ConnectedLabel = null;
				}
			}
			public delegate void MyEventHandler();//сигнатура
			public static event MyEventHandler R_Changed;//для изменения картинки графа
			public static ConnectedControls UI_Controls;//связанные control-ы на форме
			public static FuzzyRelation Avg;//агрегированная матрица матриц профилей (среднее)
			public static FuzzyRelation Med;//агрегированная матрица матриц профилей (медианные)
			private static FuzzyRelation _R;//текущая используемая агрегированная матрица
			public static FuzzyRelation R
			{
				get { return _R; }
				set
				{
					_R = value;
					R_Changed();
				}
			}
			public static void Set(List<Matrix> experts_relations)
			{
				Avg = Matrix.Average(experts_relations).Cast2Fuzzy;
				Med = Matrix.Median(experts_relations).Cast2Fuzzy;
				if (UI_Controls.Connected_rb_dist_square.Checked)
					R = Avg;
				else if (UI_Controls.Connected_rb_dist_modulus.Checked)
					R = Med;
				else
					throw new MyException(EX_choose_distance_func);
			}
			public static void Clear()
			{
				Avg = new FuzzyRelation(n);
				Med = new FuzzyRelation(n);
				R = new FuzzyRelation(n);
			}
			public static (List<Matrix> Matrices, List<string> Labels) GetRelations2Draw()
			{
				var M = new List<Matrix>{R, R.TransClosured, R.DestroyedCycles, R.DestroyedCycles.TransClosured,
					R.Asymmetric};
				var L = new List<string>{RE_R, RE_R_Tr, RE_R_Acyc, RE_R_Acyc_Tr,
					RE_R_Asym};
				var ans = (M, L);
				return ans;
			}
		}

		/// <summary>
		/// все методы
		/// </summary>
		public static class Methods
		{
			public static Method All_various_rankings = new Method(MET_ALL_RANKINGS);
			public static Method All_Hamiltonian_paths = new Method(MET_ALL_HP);
			private static Method Hp_max_length = new Method(MET_HP_MAX_LENGTH);
			private static Method Hp_max_strength = new Method(MET_HP_MAX_STRENGTH);
			public static Method Schulze_method = new Method(MET_SCHULZE_METHOD);//имеет результирующее ранжирование по методу Шульце (единственно)
			public static Method Smerchinskaya_Yashina_method = new Method(MET_SMERCHINSKAYA_YASHINA_METHOD);
			public static List<string> MutualRankings;//ранжирования, которые принадлежат всем выбранным к выполнению (IsExecute) методам
			public static void UI_Show()
			{
				FileOperations.WriteToFile("", OUT_FILE, false);
				foreach (Method M in GetMethods())
				{
					if (M.IsExecute)
						M.UI_Controls.UI_Show();
				}
			}
			public static void UI_Clear()
			{
				foreach (Method M in GetMethods())
				{
					M.UI_Controls.UI_Clear();
				}
				FileOperations.WriteToFile("", OUT_FILE, false);
			}
			/// <summary>
			/// очищает результаты методов и характеристики этих результатов
			/// </summary>
			public static void Clear()
			{
				foreach (Method M in GetMethods())
				{
					M.Clear();
				}
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
			/// создание всех возможных ранжирований данных альтернатив
			/// </summary>
			/// <returns></returns>
			public static void Set_All_various_rankings(int n)
			{
				All_various_rankings.Clear();
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
					All_various_rankings.Rankings.Add(new Ranking(MET_ALL_RANKINGS, new List<int> { 0 }));
				else if (n == 2)
				{
					All_various_rankings.Rankings.Add(new Ranking(MET_ALL_RANKINGS, new List<int> { 0, 1 }));
					All_various_rankings.Rankings.Add(new Ranking(MET_ALL_RANKINGS, new List<int> { 1, 0 }));
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
									All_various_rankings.Rankings.Add(new Ranking(MET_ALL_RANKINGS, r));
								}
							}
				}
			}
			/// <summary>
			/// вычисляет все Гамильтоновы пути
			/// </summary>
			/// <param name="HP"></param>
			/// <param name="Weights_matrix"></param>
			public static void Set_All_Hamiltonian_paths(Matrix weight_matrix)
			{
				All_Hamiltonian_paths.Clear();
				List<List<int>>[,] HP = Hamiltonian_paths_through_matrix_degree(weight_matrix, NO_EDGE);
				for (int i = 0; i < HP.GetLength(0); i++)
					for (int j = 0; j < HP.GetLength(1); j++)
						foreach (List<int> path_from_i_to_j in HP[i, j])
							All_Hamiltonian_paths.Rankings.Add(new Ranking(MET_ALL_HP, path_from_i_to_j));
				Hp_max_length.Clear();
				Hp_max_strength.Clear();
				foreach (Ranking r in All_Hamiltonian_paths.Rankings)
				{
					if (r.Cost.Value == All_Hamiltonian_paths.RankingsCharacteristics.MinMaxCost.ValueMax)
						Hp_max_length.Rankings.Add(r);
					if (r.Strength.Value == All_Hamiltonian_paths.RankingsCharacteristics.MinMaxStrength.ValueMax)
						Hp_max_strength.Rankings.Add(r);
				}
				/// <summary>
				/// нахождение Гамильтоновых путей
				/// </summary>
				List<List<int>>[,] Hamiltonian_paths_through_matrix_degree(Matrix Weights_matrix, double no_edge_symbol)
				{
					int n = Weights_matrix.GetLength(0);

					string[,] matrices_mult_sym(string[,] M1, string[,] M2)
					{
						int n1 = M1.GetLength(0);
						int n2 = M2.GetLength(1);
						int m = M1.GetLength(1); // = M2.GetLength(0);
						var sep = new char[] { MARK };
						string[,] ans = new string[n1, n2];
						for (int i = 0; i < n1; i++)
							for (int j = 0; j < n2; j++)
							{
								if (i == j)
									ans[i, j] = ZER;//обнулене петель
								else
								{
									string a_ij = "";
									for (int k = 0; k < m; k++)
									{
										var lhs = M1[i, k];
										var rhs = M2[k, j];
										if (lhs == ZER || rhs == ZER)
											a_ij += "";
										else//перемножение двух скобок, каждая скобка - сумма нескольких слагаемых
										{
											var parts1 = lhs.Split(PLS);
											var parts2 = rhs.Split(PLS);
											foreach (string l in parts1)
											{
												foreach (string r in parts2)
												{
													if (!l.Contains(ind2sym[i]) && !l.Contains(ind2sym[j])//если старт или конец пути циклят
														&& !r.Contains(ind2sym[i]) && !r.Contains(ind2sym[j]))
													{
														if (l == ONE)
															a_ij += PLS + r;
														else if (r == ONE)
															a_ij += PLS + l;
														else if (l.Split(sep, StringSplitOptions.RemoveEmptyEntries)
															.Intersect(r.Split(sep, StringSplitOptions.RemoveEmptyEntries))
															.ToArray().Length == 0)//если внутри тоже нет циклов
															a_ij += PLS + l + r;
													}
												}
											}
										}
									}
									if (a_ij == "")
										a_ij = ZER;
									ans[i, j] = a_ij.TrimStart(PLS);
								}
							}
						return ans;
					}

					Matrix Q_int = new Matrix(n, n);
					string[,] Q = new string[n, n];
					string[,] H = new string[n, n];
					for (int i = 0; i < n; i++)
						for (int j = 0; j < n; j++)
						{
							if (!Weights_matrix.HasEdge((i, j), new double[] { no_edge_symbol, INF, -INF })
								|| i == j)// с занулением диагонали
							{
								Q_int[i, j] = 0;
								Q[i, j] = ZER;
								H[i, j] = ZER;
							}
							else
							{
								Q_int[i, j] = 1;
								Q[i, j] = ONE;
								H[i, j] = ind2sym[j];
							}
						}
					Matrix Q_paths_cnt = Q_int.Pow(n - 1); //Paths count between vertices			
					int paths_cnt = 0; //Total paths count
					for (int i = 0; i < n; i++)
						for (int j = 0; j < n; j++)
							if (i != j)
								paths_cnt += (int)Math.Truncate(Q_paths_cnt[i, j]);

					for (int i = 2; i < n; i++)
						Q = matrices_mult_sym(H, Q);

					List<List<int>>[,] Paths_matrix = new List<List<int>>[n, n];
					if (n == 1)
					{
						Paths_matrix[0, 0] = new List<List<int>> { new List<int> { 0 } };
						return Paths_matrix;
					}
					for (int i = 0; i < n; i++)
						for (int j = 0; j < n; j++)
						{
							Paths_matrix[i, j] = new List<List<int>>();
							if (Q[i, j] != ZER)
							{
								if (n > 2)
								{
									List<string> some_sym_paths = new List<string>();
									some_sym_paths = some_sym_paths.Concat(Q[i, j].Split(PLS)).ToList();
									foreach (string sym_path in some_sym_paths)
									{
										var inds_str = (ind2sym[i] + sym_path + ind2sym[j]).TrimStart(MARK).Split(MARK);
										Paths_matrix[i, j].Add(inds_str.Select(x => int.Parse(x)).ToList());
									}
								}
								else if (n == 2)
									Paths_matrix[i, j].Add(new List<int> { i, j });
							}
						}
					return Paths_matrix;
				}
			}
			/// <summary>
			/// нахождение ранжирования и победителей методом Шульце
			/// </summary>
			public static void Set_Schulze_method(int n, Matrix weight_matrix)
			{
				Schulze_method.Clear();
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
				//победители - это, буквально, недоминируемые альтернативы
				Schulze_method.Winners = Enumerable.Range(0, n).Where(i => winner[i] == true).ToList();//индексы победителей																								   
				var pair_dominant_matrix = new Matrix(PD);
				var is_ = Ranking.Matrix2RanksDemukron(pair_dominant_matrix, out var levels, out var ranks);
				Schulze_method.Levels = levels;
				if (is_)
				{
					foreach (var r in ranks)
						Schulze_method.Rankings.Add(new Ranking(MET_SCHULZE_METHOD, r));
				}
			}
			/// <summary>
			/// нахождение ранжирований из агрегированной матрицы - используется минимальное расстояние и разбиение контуров
			/// </summary>
			public static void Set_Smerchinskaya_Yashina_method()
			{
				Smerchinskaya_Yashina_method.Clear();
				Smerchinskaya_Yashina_method.Winners = AggregatedMatrix.R.DestroyedCycles.TransClosured.UndominatedAlternatives().ToList();
				var is_ = Ranking.Matrix2RanksDemukron(AggregatedMatrix.R.DestroyedCycles.TransClosured, out var levels, out var ranks);
				Smerchinskaya_Yashina_method.Levels = levels;
				if (is_)
				{
					foreach (var rr in ranks)
						Smerchinskaya_Yashina_method.Rankings.Add(new Ranking(MET_SMERCHINSKAYA_YASHINA_METHOD, rr));
				}
			}
			/// <summary>
			/// запускает выполнение выбранных алгоритмов
			/// </summary>
			/// <param name="list_of_profiles"></param>
			public static List<string> ExecuteAlgorythms()
			{
				List<string> Intersect = new List<string>();//общие ранжирования для использованных методов
				try
				{
					if (ExpertRelations.GetModelMatrices().Count == 0)
						throw new MyException(EX_bad_expert_profile);
					AggregatedMatrix.Set(ExpertRelations.GetModelMatrices());
					var checkbuttons = GetMethods().Select(x => x.IsExecute);
					if (checkbuttons.All(x => x == false))
						throw new MyException(EX_choose_method);

					if (All_various_rankings.IsExecute)
						Set_All_various_rankings(n);
					if (All_Hamiltonian_paths.IsExecute)
						Set_All_Hamiltonian_paths(AggregatedMatrix.R);
					if (Schulze_method.IsExecute)
						Set_Schulze_method(n, AggregatedMatrix.R);
					if (Smerchinskaya_Yashina_method.IsExecute)
						Set_Smerchinskaya_Yashina_method();

					var methods_has_rankings = new List<Method>();
					foreach (Method m in GetMethods())
					{
						if (m.IsExecute && (m.HasRankings))
							methods_has_rankings.Add(m);
					}
					if (methods_has_rankings.Count() > 1)
					{
						bool processing_first_method = true;
						foreach (Method met in methods_has_rankings)
						{
							if (processing_first_method)
							{
								Intersect = met.Ranks2Strings;
								processing_first_method = false;
							}
							else
								Intersect = Enumerable.Intersect(Intersect, met.Ranks2Strings).ToList();
						}
					}
				}
				catch (MyException ex) { ex.Info(); }
				MutualRankings = Intersect;
				return Intersect;
			}
		}
	}
}
