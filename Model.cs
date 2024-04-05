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
using static Group_choice_algos_fuzzy.GraphDrawingFuncs;
using System.IO;

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


		/// <summary>
		/// матрицы нечетких отношений экспертов
		/// </summary>
		public static class ExpertRelations
		{
			public class ConnectedControls : ConnectedControlsBase
			{
				public ConnectedControls(
					CheckBox cb_show, CheckBox cb_doTransClos,
					NumericUpDown n, NumericUpDown m, FlowLayoutPanel flp)
				{
					ConnectedCheckBox_ToShow = cb_show;
					ConnectedCheckBox_DoTransClosure = cb_doTransClos;
					numericUpDown_n = n;
					numericUpDown_m = m;
					ConnectedflowLayoutPanel = flp;
				}
				private CheckBox ConnectedCheckBox_ToShow;//выводить ли таблички для ввода
				private CheckBox ConnectedCheckBox_DoTransClosure;//делать ли транз. замыкание после ввода
				private FlowLayoutPanel ConnectedflowLayoutPanel;//куда кладутся все datagridview экспертов
				private NumericUpDown numericUpDown_n;
				private NumericUpDown numericUpDown_m;
				public Control.ControlCollection ConnectedTables
				{
					get { return ConnectedflowLayoutPanel?.Controls; }
				}
				override public void UI_Show()
				{
					if (ConnectedCheckBox_ToShow.Checked)
					{
						if (ConnectedTables?.Count == 0)
						{
							set_input_datagrids();
						}
						foreach (DataGridView dgv in ConnectedTables)
						{
							dgv?.Show();
							dgv?.Parent.Show();
						}
					}
				}
				override public void UI_Clear()
				{
					ConnectedLabelText = "";
					foreach (DataGridView dgv in ConnectedTables)
					{
						dgv?.Rows.Clear();
						dgv?.Columns.Clear();
						dgv?.Hide();
						dgv?.Parent?.Hide();
						dgv?.Dispose();
					}
					ConnectedflowLayoutPanel.Controls.Clear();
				}
				public void UI_Activate()
				{
					foreach (DataGridView dgv in UI_Controls.ConnectedTables)
					{
						for (int i = 0; i < dgv.RowCount; i++)
						{
							for (int j = 0; j < dgv.ColumnCount; j++)
							{
								color_input_cell(dgv, i, j, input_bg_color);
							}
						}
						dgv.ReadOnly = false;
					}
				}
				public void UI_Deactivate()
				{
					foreach (DataGridView dgv in UI_Controls.ConnectedTables)
					{
						for (int i = 0; i < dgv.RowCount; i++)
						{
							for (int j = 0; j < dgv.ColumnCount; j++)
							{
								color_input_cell(dgv, i, j, input_bg_color_disabled);
							}
						}
						dgv.ReadOnly = true;
					}
				}
				/// <summary>
				/// размещение таблицы для ввода профилей
				/// </summary>
				private void set_input_datagrids()
				{
					UI_Clear();
					if (numericUpDown_n.Minimum <= n && n <= numericUpDown_n.Maximum &&
						numericUpDown_m.Minimum <= m && m <= numericUpDown_m.Maximum)
					{
						numericUpDown_n.Value = n;
						numericUpDown_m.Value = m;
					}
					try
					{
						bool some_matrices_have_cycle = false;
						for (int expert = 0; expert < m; expert++)
						{
							int comparsion_trials = 0;//сколько отредактированных ячеек уже было
							bool[] is_compared_alternative = new bool[n];//сравнима ли альтернатива

							FuzzyRelation PerformTransClosure(FuzzyRelation matrix)
							{
								if (ConnectedCheckBox_DoTransClosure.Checked &&
								(comparsion_trials == n - 1 || is_compared_alternative.All(x => x == true)))
								{
									if (!matrix.IsTransitive())
									{
										matrix = matrix.TransitiveClosure();
									}
								}
								return matrix;
							}
							void CheckCellWhenValueChanged(object sender, DataGridViewCellEventArgs e)
							{//что должно происходить при завершении редактирования ячейки
								//if (!ConnectedCheckBox_ToShow.Checked)
								//	return;
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
									is_compared_alternative[i] = new_matrix.IsAlternativeCompared(i);
									is_compared_alternative[j] = new_matrix.IsAlternativeCompared(j);
									//транзитивное замыкание не должно содержать циклов
									if (new_matrix.IsHasCycle())
										throw new MyException(EX_contains_cycle);
									new_matrix = PerformTransClosure(new_matrix);
									UpdateExpertMatrices_wrapper(sender, exp_index, new_matrix);
								}
								catch (MyException ex) { ex.Info(); }
							}

							FuzzyRelation input_matrix = RListFuzzyRel[expert];
							if (input_matrix.IsHasCycle())
							{
								some_matrices_have_cycle = true;
							}
							else
							{
								input_matrix = PerformTransClosure(input_matrix);
								UpdateExpertMatrices_wrapper(this, expert, input_matrix);
							}


							if (ConnectedCheckBox_ToShow.Checked)
							{
								double[,] fill_values = input_matrix.matrix_base;
								DataGridView dgv = new DataGridView();
								SetDataGridViewDefaults(dgv);
								dgv.CellEndEdit += CheckCellWhenValueChanged;
								dgv.CellEndEdit += DeactivateSymmetricCell;
								dgv.CellEndEdit += UpdateExpertGraph;
								ConnectedflowLayoutPanel.Controls.Add(dgv);

								for (int j = 0; j < n; j++)
								{
									DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
									column.Name = j.ToString();
									column.HeaderText = $"{ind2letter[j]}";
									column.SortMode = DataGridViewColumnSortMode.NotSortable;
									dgv.Columns.Add(column);
								}
								for (int i = 0; i < n; i++)
								{
									dgv.Rows.Add();
									dgv.Rows[i].HeaderCell.Value = $"{ind2letter[i]}";
								}
								for (int i = 0; i < dgv.Rows.Count; i++)
								{
									for (int j = 0; j < dgv.Columns.Count; j++)
									{
										dgv[j, i].ValueType = typeof(double);
										dgv[j, i].Value = fill_values[i, j];
										DeactivateSymmetricCell(dgv, new DataGridViewCellEventArgs(j, i));
									}
								}
								for (int i = 0; i < n; i++)
								{
									is_compared_alternative[i] = Matrix.GetFromDataGridView(dgv).IsAlternativeCompared(i);
								}

							}

							UpdateExpertMatrices_wrapper(this, expert, input_matrix);
							UpdateExpertGraph(null, null);

						}
						if (some_matrices_have_cycle)
							throw new MyException(EX_contains_cycle);
					}
					catch (MyException ex) { ex.Info(); }
					UI_Controls.UI_Activate();
					form1.set_controls_size();
				}
			}
			private static List<Matrix> _RList;
			public static ConnectedControls UI_Controls;
			public static List<FuzzyRelation> RListFuzzyRel
			{
				get { return FuzzyRelation.ToFuzzyList(RListMatrix); }
				set { RListMatrix = FuzzyRelation.ToMatrixList(value); }
			}
			public static List<Matrix> RListMatrix
			{
				get
				{
					if (_RList is null)
						_RList = new List<Matrix>();
					for (int i = 0; i < _RList.Count; i++)
					{
						if (!FuzzyRelation.IsFuzzyRelationMatrix(_RList[i]))
							_RList[i] = _RList[i].NormalizeAndCast2Fuzzy;
					}
					return _RList;
				}
				set
				{
					if (value is null)
					{
						UI_Controls.UI_Clear();
					}
					_RList = value;
					if(_RList?.Count > 0)
						UI_Controls.UI_Show();
				}
			}
			public static void Clear()
			{
				RListMatrix = null;
			}
			public static void UpdateExpertDGV(int expert_index)
			{
				try
				{
				DataGridView DGV = (DataGridView)UI_Controls.ConnectedTables[expert_index];
				Matrix fill_values = RListMatrix[expert_index];
				Matrix.SetToDataGridView(fill_values, DGV);
				for (int i = 0; i < n; i++)
					for (int j = 0; j < n; j++)
						DeactivateSymmetricCell(DGV, new DataGridViewCellEventArgs(i, j));
				}
				catch { }
			}
			public static void UpdateExpertMatrix(object sender, ExpertRelationsEventArgs e)
			{
				RListMatrix[e.expert_index] = e.fill_values;
				UpdateExpertDGV(e.expert_index);
			}
			/// <summary>
			/// обновить рисунки графов - матриц экспертов
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			static public void UpdateExpertGraph(object sender, DataGridViewCellEventArgs e)
			{
				var M = RListMatrix;
				var L = new List<string>();
				for (int i = 0; i < M.Count; i++)
				{
					L.Add($"Expert{i}:");
				}
				OrgraphsPics_update(Form1.form3_input_expert_matrices, M, L);
			}
			static public void UpdateExpertMatrices_wrapper(object sender, int expert_index, Matrix fill_values)
			{
				var args = new ExpertRelationsEventArgs();
				args.expert_index = expert_index;
				args.fill_values = fill_values;
				ExpertRelations_EventHandler?.Invoke(sender, args);
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
					ConnectedLabelControl = lbl;
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
						for_print_matrices("Агрегированное отношение R",
							R);
						for_print_matrices("Асимметричная часть As(R) агрегированного отношения R",
							R.Asymmetric);
						for_print_matrices("Транзитивное замыкание Tr(R) агрегированного отношения R",
							R.TransClosured);
						for_print_matrices("Отношение с разбитыми циклами Acyc(R) агрегированного отношения R",
							R.DestroyedCycles);
						for_print_matrices("Транзитивное замыкание Tr(Acyc(R)) отношения с разбитыми циклами Acyc(R) агрегированного отношения R",
							R.DestroyedCycles.TransClosured);
					}
					ConnectedLabelText = tex;
					ConnectedLabelControl.Show();
				}
				override public void UI_Clear()
				{
					ConnectedLabelText = "";
					ConnectedLabelControl.Hide();
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
			public static void Set(List<FuzzyRelation> experts_relations)
			{
				Avg = Matrix.Average(FuzzyRelation.ToMatrixList(experts_relations)).Cast2Fuzzy;
				Med = Matrix.Median(FuzzyRelation.ToMatrixList(experts_relations)).Cast2Fuzzy;
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
				var M = new List<Matrix>{
					R, R.TransClosured,
					R.DestroyedCycles, R.DestroyedCycles.TransClosured,
					R.Asymmetric};
				var L = new List<string>{
					"R", "Tr(R)",
					"Acyclic(R)", "Tr(Acyclic(R))",
					"Asymmetric(R)"};
				var ans = (M, L);
				return ans;
			}
		}


		/// <summary>
		/// все методы
		/// </summary>
		public static class Methods
		{
			public static Method All_various_rankings = new Method(ALL_RANKINGS);
			public static Method All_Hamiltonian_paths = new Method(ALL_HP);
			private static Method Hp_max_length = new Method(HP_MAX_LENGTH);
			private static Method Hp_max_strength = new Method(HP_MAX_STRENGTH);
			public static Method Schulze_method = new Method(SCHULZE_METHOD);//имеет результирующее ранжирование по методу Шульце (единственно)
			public static Method Smerchinskaya_Yashina_method = new Method(SMERCHINSKAYA_YASHINA_METHOD);
			public struct MethodsCharacteristics
			{
				private static Characteristic _MaxHamPathCost;//длина пути длиннейших Гаммильтоновых путей
				private static Characteristic _MaxHamPathStrength;//сила пути сильнейших Гаммильтоновых путей
				public static Characteristic MaxHamPathCost
				{
					get
					{
						if (Method.IsMethodExistWithRanks(All_Hamiltonian_paths) && !Characteristic.IsInitialized(_MaxHamPathCost))
						{
							_MaxHamPathCost = new Characteristic("самая большая стоимость гамильтоновых путей",
								All_Hamiltonian_paths.Rankings.Select(x => x.Cost.Value).Max());
						}
						return _MaxHamPathCost;
					}
				}
				public static Characteristic MaxHamPathStrength
				{
					get
					{
						if (Method.IsMethodExistWithRanks(All_Hamiltonian_paths) && !Characteristic.IsInitialized(_MaxHamPathStrength))
						{
							_MaxHamPathStrength = new Characteristic("самая большая сила гамильтоновых путей",
								All_Hamiltonian_paths.Rankings.Select(x => x.Strength.Value).Max());
						}
						return _MaxHamPathStrength;
					}
				}
				public static void Clear()
				{
					_MaxHamPathCost = null;
					_MaxHamPathStrength = null;
				}
			}
			/// <summary>
			/// показывает результаты выполнения методов
			/// </summary>
			public static void UI_Show()
			{
				foreach (Method M in GetMethods())
				{
					M.UI_Controls.UI_Show();
				}
			}
			public static void UI_Clear()
			{
				foreach (Method M in GetMethods())
				{
					M.UI_Controls.UI_Clear();
				}
			}
			/// <summary>
			/// очищает результаты методов и характеристики этих результатов
			/// </summary>
			public static void Clear()
			{
				try
				{
					foreach (Method M in GetMethods())
					{
						M.Clear();
					}
					MethodsCharacteristics.Clear();
				}
				catch (MyException ex) { }
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
					All_various_rankings.Rankings.Add(new Ranking(ALL_RANKINGS, new List<int> { 0 }));
				else if (n == 2)
				{
					All_various_rankings.Rankings.Add(new Ranking(ALL_RANKINGS, new List<int> { 0, 1 }));
					All_various_rankings.Rankings.Add(new Ranking(ALL_RANKINGS, new List<int> { 1, 0 }));
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
									All_various_rankings.Rankings.Add(new Ranking(ALL_RANKINGS, r));
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
							All_Hamiltonian_paths.Rankings.Add(new Ranking(ALL_HP, path_from_i_to_j));
				Hp_max_length.Clear();
				Hp_max_strength.Clear();
				foreach (Ranking r in All_Hamiltonian_paths.Rankings)
				{
					if (r.Cost.Value == MethodsCharacteristics.MaxHamPathCost.Value)
						Hp_max_length.Rankings.Add(r);
					if (r.Strength.Value == MethodsCharacteristics.MaxHamPathStrength.Value)
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
						Schulze_method.Rankings.Add(new Ranking(SCHULZE_METHOD, r));
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
						Smerchinskaya_Yashina_method.Rankings.Add(new Ranking(SMERCHINSKAYA_YASHINA_METHOD, rr));
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
					if (ExpertRelations.RListFuzzyRel.Count == 0)
						throw new MyException(EX_bad_expert_profile);
					AggregatedMatrix.Set(ExpertRelations.RListFuzzyRel);
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

					var is_rankings_of_method_exist = new List<Method>();
					foreach (Method m in GetMethods())
					{
						if (m.IsExecute && (m.HasRankings))
							is_rankings_of_method_exist.Add(m);
					}
					if (is_rankings_of_method_exist.Count() > 1)
					{
						bool processing_first_method = true;
						foreach (Method met in is_rankings_of_method_exist)
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
				return Intersect;
			}
			/// <summary>
			/// вывести на экран результирующие ранжирования
			/// </summary>
			/// <param name="Mutual_rankings"></param>
			public static async void set_output_results(List<string> Mutual_rankings)
			{
				UI_Clear();
				ExpertRelations.UI_Controls.UI_Deactivate();
				AggregatedMatrix.UI_Controls.UI_Show();
				try
				{
					//создание чистого файла для вывода ранжирований в виде матриц
					FileOperations.WriteToFile("",OUT_FILE, false);
					UI_Show();
					foreach (Method met in GetMethods())
					{
						if (met.IsExecute && met.HasRankings)
						{							
							for (int j = 0; j < met.Rankings.Count; j++)
							{
								if (Mutual_rankings.Contains(met.Rankings[j].Rank2String))
								{
									for (int i = 0; i < n; i++)
										met.UI_Controls.ConnectedTableFrame[j, i].Style.BackColor = output_characteristics_mutual_color;
								}								
							}
						}
					}
					form1.set_controls_size();
				}
				catch (MyException ex) { ex.Info(); }
			}
		}

		#region Обновление матриц экспертов (Model)
		//public delegate void ExpertMatricesInUI_EventHandler(object sender, ExpertMatricesEventArgs e);
		static public event EventHandler<ExpertRelationsEventArgs> ExpertRelations_EventHandler;

		public virtual void ExpertRelations_Update(object sender, ExpertRelationsEventArgs e)
		{
			EventHandler<ExpertRelationsEventArgs> handler = ExpertRelations_EventHandler;
			handler?.Invoke(this, e);
		}
		public class ExpertRelationsEventArgs : EventArgs
		{
			public int expert_index { get; set; }
			public Matrix fill_values { get; set; }
		}
		static public void color_input_cell(DataGridView dgv, int row, int col, System.Drawing.Color color)
		{
			try
			{
				dgv[col, row].Style.BackColor = color;
			}
			catch (MyException ex) { }
		}
		static public void DeactivateSymmetricCell(object sender, DataGridViewCellEventArgs e)
		{
			try
			{
				var dd = sender as DataGridView;
				int i = e.RowIndex;
				int j = e.ColumnIndex;
				if (i == j)
					color_input_cell(dd, i, j, input_bg_color_disabled);
				else
				{
					double Mij, Mji;
					double.TryParse(dd[j, i]?.Value?.ToString(), out Mij);
					double.TryParse(dd[i, j]?.Value?.ToString(), out Mji);
					if (Mij == 0 && Mji != 0)
					{
						color_input_cell(dd, i, j, input_bg_color_disabled);
						color_input_cell(dd, j, i, input_bg_color);
					}
					else if (Mij != 0 && Mji == 0)
					{
						color_input_cell(dd, i, j, input_bg_color);
						color_input_cell(dd, j, i, input_bg_color_disabled);
					}
					else
					{
						color_input_cell(dd, i, j, input_bg_color);
						color_input_cell(dd, j, i, input_bg_color);
					}
				}
			}
			catch (MyException ex) { ex.Info(); }
		}

		#endregion Обновление матриц экспертов (Model)

	}
}
