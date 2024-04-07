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
				public CheckBox connectedCheckBox_ToShow;//выводить ли таблички для ввода
				public CheckBox connectedCheckBox_DoTransClosure;//делать ли транз. замыкание после ввода
				public FlowLayoutPanel connectedFlowLayoutPanel;//куда кладутся все datagridview экспертов
				public NumericUpDown numericUpDown_n;
				public NumericUpDown numericUpDown_m;
				public Control.ControlCollection ConnectedTables
				{
					get
					{
						Control.ControlCollection ans = new Control.ControlCollection(connectedFlowLayoutPanel);
						foreach (Control c in connectedFlowLayoutPanel?.Controls)
						{
							if (c as DataGridView != null)
							{
								ans.Add(c);
							}
						}
						return ans;
					}
				}
				override public void UI_Show()
				{
					if (connectedCheckBox_ToShow.Checked)
					{
						foreach (DataGridView dgv in ConnectedTables)
						{
							dgv?.Show();
							dgv?.Parent.Show();
						}
					}
				}
				override public void UI_Clear()
				{
					ConnectedLabel.Dispose();
					ConnectedLabel = null;
					foreach (DataGridView dgv in ConnectedTables)
					{
						dgv?.Rows.Clear();
						dgv?.Columns.Clear();
						dgv?.Hide();
						dgv?.Parent?.Hide();
						dgv?.Dispose();
					}
					connectedFlowLayoutPanel.Controls.Clear();
				}
				public void UI_Activate()
				{
					foreach (DataGridView dgv in ConnectedTables)
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
					foreach (DataGridView dgv in ConnectedTables)
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
			}
			#endregion SUBCLASSES

			#region FIELDS
			//public delegate void ExpertMatricesInUI_EventHandler(object sender, ExpertMatricesEventArgs e);
			public static event EventHandler<ExpertRelationsEventArgs> ExpertRelations_InputViewChanged;
			public static event EventHandler<ExpertRelationsEventArgs> ExpertRelations_InputViewsChanged;
			public static event EventHandler<ExpertRelationsEventArgs> ExpertRelations_ModelRelChanged;
			public static event EventHandler<ExpertRelationsEventArgs> ExpertRelations_ModelRelsChanged;
			//private static List<Matrix> _RList;
			public static List<Matrix> RListMatrix;
			public static ConnectedControls UI_Controls;
			//
			/// 
			///// 
			static public int comparsion_trials = 0;//сколько отредактированных ячеек уже было
													//// ///
			/// 
			//
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
			static FuzzyRelation PerformTransClosure(FuzzyRelation matrix)
			{
				if (UI_Controls.connectedCheckBox_DoTransClosure.Checked &&
				(comparsion_trials == n - 1 || matrix.ComparedAlternatives().All(x => x == true)))
				{
					if (!matrix.IsTransitive())
					{
						matrix = matrix.TransitiveClosure();
					}
				}
				return matrix;
			}
			public static void Clear()
			{
				RListMatrix = null;
			}
			private static void ColorCell(DataGridView dgv, int row, int col, System.Drawing.Color color)
			{
				dgv[col, row].Style.BackColor = color;
			}
			private static void ColorSymmetricCell(object sender, DataGridViewCellEventArgs e)
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
					FuzzyRelation new_matrix;
					var dd = sender as DataGridView;
					///////
					//int exp_index = flowLayoutPanel_input_tables.Controls.GetChildIndex(dd);
					int exp_index = UI_Controls.ConnectedTables.IndexOf(dd);
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
						comparsion_trials++;
					}
					new_matrix = Matrix.GetFromDataGridView(dd).NormalizeAndCast2Fuzzy;
					ExpertRelations_InputViewChanged?.Invoke(sender, new ExpertRelationsEventArgs(exp_index, new_matrix));

					//транзитивное замыкание не должно содержать циклов
					if (new_matrix.IsHasCycle())
						throw new MyException(EX_contains_cycle);
					new_matrix = PerformTransClosure(new_matrix);

				}
				catch (MyException ex) { ex.Info(); }
			}
			public static void UpdateExpertDataGridView(object sender, ExpertRelationsEventArgs e)
			{
				if (UI_Controls.connectedCheckBox_ToShow.Checked)
				{
					DataGridView dgv = new DataGridView();
					SetDataGridViewDefaults(dgv);
					dgv.CellEndEdit += CheckCellWhenValueChanged;
					dgv.CellEndEdit += ColorSymmetricCell;
					UI_Controls.connectedFlowLayoutPanel.Controls.Add(dgv);
					for (int j = 0; j < n; j++)
					{
						SetColumn(dgv, $"{ind2letter[j]}");
					}
					for (int i = 0; i < n; i++)
					{
						SetRow(dgv, $"{ind2letter[i]}");
					}

					Matrix.SetToDataGridView(e.fill_values, dgv);
					for (int i = 0; i < dgv.Rows.Count; i++)
						for (int j = 0; j < dgv.Columns.Count; j++)
							ColorSymmetricCell(dgv, new DataGridViewCellEventArgs(j, i));
				}
			}
			public static void UpdateExpertDataGridViews(object sender, ExpertRelationsEventArgs e)
			{
				UI_Controls.UI_Clear();
				if (UI_Controls.numericUpDown_n.Minimum <= n && n <= UI_Controls.numericUpDown_n.Maximum &&
						UI_Controls.numericUpDown_m.Minimum <= m && m <= UI_Controls.numericUpDown_m.Maximum)
				{
					UI_Controls.numericUpDown_n.Value = n;
					UI_Controls.numericUpDown_m.Value = m;
				}
				try
				{
					for (int expert = 0; expert < e.expert_matrices?.Count; expert++)
					{
						UpdateExpertDataGridView(sender, new ExpertRelationsEventArgs(
							expert, e.expert_matrices[expert]));						
					}
				}
				catch (MyException ex) { ex.Info(); }
				form1.set_controls_size();
			}
			public static void UpdateExpertMatrix(object sender, ExpertRelationsEventArgs e)
			{
				Matrix new_M = e.fill_values;
				if (!FuzzyRelation.IsFuzzyRelationMatrix(new_M))
				{
					new_M = new_M.NormalizeAndCast2Fuzzy;
				}
				RListMatrix[e.expert_index] = new_M;
			}
			public static void UpdateExpertMatrices(object sender, ExpertRelationsEventArgs e)
			{
				try
				{
					for (int expert = 0; expert < e.expert_matrices?.Count; expert++)
					{
						UpdateExpertMatrix(sender, new ExpertRelationsEventArgs(
							expert, e.expert_matrices[expert]));
					}
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
				var M = RListMatrix;
				var L = new List<string>();
				for (int i = 0; i < M.Count; i++)
				{
					L.Add($"Expert{i}:");
				}
				OrgraphsPics_update(Form1.form3_input_expert_matrices, M, L);
			}

			public static void ModelChanged()
			{
				ExpertRelations_ModelRelsChanged?.Invoke(
					null, new ExpertRelationsEventArgs(-1, null,RListMatrix));
			}
			public static void ViewChanged(List<Matrix> input)
			{
				ExpertRelations_InputViewChanged?.Invoke(
					null, new ExpertRelationsEventArgs(-1, null, input));
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
				//try
				//{
				foreach (Method M in GetMethods())
				{
					M.Clear();
				}
				//}
				//catch (MyException ex) { }
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
					if (ExpertRelations.RListMatrix.Count == 0)
						throw new MyException(EX_bad_expert_profile);
					AggregatedMatrix.Set(ExpertRelations.RListMatrix);
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
