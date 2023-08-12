
namespace Group_choice_algos_fuzzy
{
	partial class Form1
	{
		/// <summary>
		/// Обязательная переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Требуемый метод для поддержки конструктора — не изменяйте 
		/// содержимое этого метода с помощью редактора кода.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.flowLayoutPanel_output_tables = new System.Windows.Forms.FlowLayoutPanel();
			this.groupBox_HP_max_length = new System.Windows.Forms.GroupBox();
			this.dg_HP_max_length = new System.Windows.Forms.DataGridView();
			this.groupBox_HP_max_strength = new System.Windows.Forms.GroupBox();
			this.dg_HP_max_strength = new System.Windows.Forms.DataGridView();
			this.groupBox_Schulze_method = new System.Windows.Forms.GroupBox();
			this.dg_Schulze_method = new System.Windows.Forms.DataGridView();
			this.groupBox_All_rankings = new System.Windows.Forms.GroupBox();
			this.dg_All_rankings = new System.Windows.Forms.DataGridView();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.cb_All_rankings = new System.Windows.Forms.CheckBox();
			this.cb_Schulze_method = new System.Windows.Forms.CheckBox();
			this.cb_HP_max_strength = new System.Windows.Forms.CheckBox();
			this.cb_HP_max_length = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textBox_file = new System.Windows.Forms.TextBox();
			this.button_read_file = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.numericUpDown_m = new System.Windows.Forms.NumericUpDown();
			this.button_n_m = new System.Windows.Forms.Button();
			this.numericUpDown_n = new System.Windows.Forms.NumericUpDown();
			this.button_run_program = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.flowLayoutPanel_input = new System.Windows.Forms.FlowLayoutPanel();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.rb_dist_square = new System.Windows.Forms.RadioButton();
			this.rb_dist_modulus = new System.Windows.Forms.RadioButton();
			this.flowLayoutPanel_output_info = new System.Windows.Forms.FlowLayoutPanel();
			this.label3 = new System.Windows.Forms.Label();
			this.button_for_tests = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.flowLayoutPanel_output_tables.SuspendLayout();
			this.groupBox_HP_max_length.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dg_HP_max_length)).BeginInit();
			this.groupBox_HP_max_strength.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dg_HP_max_strength)).BeginInit();
			this.groupBox_Schulze_method.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dg_Schulze_method)).BeginInit();
			this.groupBox_All_rankings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dg_All_rankings)).BeginInit();
			this.groupBox4.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown_m)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown_n)).BeginInit();
			this.groupBox3.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.flowLayoutPanel_output_info.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoScroll = true;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel_output_tables, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.groupBox4, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.button_run_program, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.groupBox5, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel_output_info, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.button_for_tests, 1, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(944, 491);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// flowLayoutPanel_output_tables
			// 
			this.flowLayoutPanel_output_tables.AutoScroll = true;
			this.flowLayoutPanel_output_tables.Controls.Add(this.groupBox_HP_max_length);
			this.flowLayoutPanel_output_tables.Controls.Add(this.groupBox_HP_max_strength);
			this.flowLayoutPanel_output_tables.Controls.Add(this.groupBox_Schulze_method);
			this.flowLayoutPanel_output_tables.Controls.Add(this.groupBox_All_rankings);
			this.flowLayoutPanel_output_tables.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel_output_tables.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel_output_tables.Location = new System.Drawing.Point(328, 143);
			this.flowLayoutPanel_output_tables.Name = "flowLayoutPanel_output_tables";
			this.tableLayoutPanel1.SetRowSpan(this.flowLayoutPanel_output_tables, 3);
			this.flowLayoutPanel_output_tables.Size = new System.Drawing.Size(613, 345);
			this.flowLayoutPanel_output_tables.TabIndex = 24;
			this.flowLayoutPanel_output_tables.WrapContents = false;
			this.flowLayoutPanel_output_tables.MouseEnter += new System.EventHandler(this.flowLayoutPanel_output_tables_MouseEnter);
			// 
			// groupBox_HP_max_length
			// 
			this.groupBox_HP_max_length.Controls.Add(this.dg_HP_max_length);
			this.groupBox_HP_max_length.Location = new System.Drawing.Point(3, 3);
			this.groupBox_HP_max_length.MaximumSize = new System.Drawing.Size(1000, 500);
			this.groupBox_HP_max_length.MinimumSize = new System.Drawing.Size(10, 10);
			this.groupBox_HP_max_length.Name = "groupBox_HP_max_length";
			this.groupBox_HP_max_length.Size = new System.Drawing.Size(450, 250);
			this.groupBox_HP_max_length.TabIndex = 19;
			this.groupBox_HP_max_length.TabStop = false;
			this.groupBox_HP_max_length.Text = "Гамильтоновы пути максимальной стоимости";
			// 
			// dg_HP_max_length
			// 
			this.dg_HP_max_length.AllowUserToAddRows = false;
			this.dg_HP_max_length.AllowUserToDeleteRows = false;
			this.dg_HP_max_length.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dg_HP_max_length.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dg_HP_max_length.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
			this.dg_HP_max_length.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle17.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle17.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dg_HP_max_length.DefaultCellStyle = dataGridViewCellStyle17;
			this.dg_HP_max_length.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dg_HP_max_length.Location = new System.Drawing.Point(3, 16);
			this.dg_HP_max_length.MinimumSize = new System.Drawing.Size(100, 100);
			this.dg_HP_max_length.Name = "dg_HP_max_length";
			this.dg_HP_max_length.ReadOnly = true;
			this.dg_HP_max_length.RowHeadersWidth = 100;
			this.dg_HP_max_length.ShowEditingIcon = false;
			this.dg_HP_max_length.Size = new System.Drawing.Size(444, 231);
			this.dg_HP_max_length.TabIndex = 3;
			// 
			// groupBox_HP_max_strength
			// 
			this.groupBox_HP_max_strength.Controls.Add(this.dg_HP_max_strength);
			this.groupBox_HP_max_strength.Location = new System.Drawing.Point(3, 259);
			this.groupBox_HP_max_strength.MaximumSize = new System.Drawing.Size(1000, 500);
			this.groupBox_HP_max_strength.MinimumSize = new System.Drawing.Size(10, 10);
			this.groupBox_HP_max_strength.Name = "groupBox_HP_max_strength";
			this.groupBox_HP_max_strength.Size = new System.Drawing.Size(450, 250);
			this.groupBox_HP_max_strength.TabIndex = 20;
			this.groupBox_HP_max_strength.TabStop = false;
			this.groupBox_HP_max_strength.Text = "Гамильтоновы пути наибольшей силы";
			// 
			// dg_HP_max_strength
			// 
			this.dg_HP_max_strength.AllowUserToAddRows = false;
			this.dg_HP_max_strength.AllowUserToDeleteRows = false;
			this.dg_HP_max_strength.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dg_HP_max_strength.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dg_HP_max_strength.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
			this.dg_HP_max_strength.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dg_HP_max_strength.DefaultCellStyle = dataGridViewCellStyle18;
			this.dg_HP_max_strength.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dg_HP_max_strength.Location = new System.Drawing.Point(3, 16);
			this.dg_HP_max_strength.MinimumSize = new System.Drawing.Size(100, 100);
			this.dg_HP_max_strength.Name = "dg_HP_max_strength";
			this.dg_HP_max_strength.ReadOnly = true;
			this.dg_HP_max_strength.RowHeadersWidth = 100;
			this.dg_HP_max_strength.ShowEditingIcon = false;
			this.dg_HP_max_strength.Size = new System.Drawing.Size(444, 231);
			this.dg_HP_max_strength.TabIndex = 1;
			// 
			// groupBox_Schulze_method
			// 
			this.groupBox_Schulze_method.Controls.Add(this.dg_Schulze_method);
			this.groupBox_Schulze_method.Location = new System.Drawing.Point(3, 515);
			this.groupBox_Schulze_method.MaximumSize = new System.Drawing.Size(1000, 500);
			this.groupBox_Schulze_method.MinimumSize = new System.Drawing.Size(10, 10);
			this.groupBox_Schulze_method.Name = "groupBox_Schulze_method";
			this.groupBox_Schulze_method.Size = new System.Drawing.Size(450, 250);
			this.groupBox_Schulze_method.TabIndex = 21;
			this.groupBox_Schulze_method.TabStop = false;
			this.groupBox_Schulze_method.Text = "Ранжирование по Алгоритму Шульце";
			// 
			// dg_Schulze_method
			// 
			this.dg_Schulze_method.AllowUserToAddRows = false;
			this.dg_Schulze_method.AllowUserToDeleteRows = false;
			this.dg_Schulze_method.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dg_Schulze_method.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dg_Schulze_method.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
			this.dg_Schulze_method.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle19.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle19.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle19.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle19.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dg_Schulze_method.DefaultCellStyle = dataGridViewCellStyle19;
			this.dg_Schulze_method.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dg_Schulze_method.Location = new System.Drawing.Point(3, 16);
			this.dg_Schulze_method.MinimumSize = new System.Drawing.Size(100, 100);
			this.dg_Schulze_method.Name = "dg_Schulze_method";
			this.dg_Schulze_method.ReadOnly = true;
			this.dg_Schulze_method.RowHeadersWidth = 100;
			this.dg_Schulze_method.ShowEditingIcon = false;
			this.dg_Schulze_method.Size = new System.Drawing.Size(444, 231);
			this.dg_Schulze_method.TabIndex = 1;
			// 
			// groupBox_All_rankings
			// 
			this.groupBox_All_rankings.Controls.Add(this.dg_All_rankings);
			this.groupBox_All_rankings.Location = new System.Drawing.Point(3, 771);
			this.groupBox_All_rankings.MaximumSize = new System.Drawing.Size(1000, 500);
			this.groupBox_All_rankings.MinimumSize = new System.Drawing.Size(10, 10);
			this.groupBox_All_rankings.Name = "groupBox_All_rankings";
			this.groupBox_All_rankings.Size = new System.Drawing.Size(450, 250);
			this.groupBox_All_rankings.TabIndex = 23;
			this.groupBox_All_rankings.TabStop = false;
			this.groupBox_All_rankings.Text = "Всевозможные ранжирования";
			// 
			// dg_All_rankings
			// 
			this.dg_All_rankings.AllowUserToAddRows = false;
			this.dg_All_rankings.AllowUserToDeleteRows = false;
			this.dg_All_rankings.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dg_All_rankings.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dg_All_rankings.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
			this.dg_All_rankings.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle20.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			dataGridViewCellStyle20.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle20.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle20.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle20.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dg_All_rankings.DefaultCellStyle = dataGridViewCellStyle20;
			this.dg_All_rankings.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dg_All_rankings.Location = new System.Drawing.Point(3, 16);
			this.dg_All_rankings.MinimumSize = new System.Drawing.Size(100, 100);
			this.dg_All_rankings.Name = "dg_All_rankings";
			this.dg_All_rankings.ReadOnly = true;
			this.dg_All_rankings.RowHeadersWidth = 100;
			this.dg_All_rankings.ShowEditingIcon = false;
			this.dg_All_rankings.Size = new System.Drawing.Size(444, 231);
			this.dg_All_rankings.TabIndex = 1;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.cb_All_rankings);
			this.groupBox4.Controls.Add(this.cb_Schulze_method);
			this.groupBox4.Controls.Add(this.cb_HP_max_strength);
			this.groupBox4.Controls.Add(this.cb_HP_max_length);
			this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox4.Location = new System.Drawing.Point(3, 143);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(259, 84);
			this.groupBox4.TabIndex = 5;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Выбор метода";
			// 
			// cb_All_rankings
			// 
			this.cb_All_rankings.AutoEllipsis = true;
			this.cb_All_rankings.Dock = System.Windows.Forms.DockStyle.Top;
			this.cb_All_rankings.Location = new System.Drawing.Point(3, 67);
			this.cb_All_rankings.Name = "cb_All_rankings";
			this.cb_All_rankings.Size = new System.Drawing.Size(253, 17);
			this.cb_All_rankings.TabIndex = 4;
			this.cb_All_rankings.Text = "Всевозможные ранжирования";
			this.cb_All_rankings.UseVisualStyleBackColor = true;
			// 
			// cb_Schulze_method
			// 
			this.cb_Schulze_method.AutoEllipsis = true;
			this.cb_Schulze_method.Dock = System.Windows.Forms.DockStyle.Top;
			this.cb_Schulze_method.Location = new System.Drawing.Point(3, 50);
			this.cb_Schulze_method.Name = "cb_Schulze_method";
			this.cb_Schulze_method.Size = new System.Drawing.Size(253, 17);
			this.cb_Schulze_method.TabIndex = 2;
			this.cb_Schulze_method.Text = "Ранжирование по Алгоритму Шульце";
			this.cb_Schulze_method.UseVisualStyleBackColor = true;
			// 
			// cb_HP_max_strength
			// 
			this.cb_HP_max_strength.AutoEllipsis = true;
			this.cb_HP_max_strength.Dock = System.Windows.Forms.DockStyle.Top;
			this.cb_HP_max_strength.Location = new System.Drawing.Point(3, 33);
			this.cb_HP_max_strength.Name = "cb_HP_max_strength";
			this.cb_HP_max_strength.Size = new System.Drawing.Size(253, 17);
			this.cb_HP_max_strength.TabIndex = 1;
			this.cb_HP_max_strength.Text = "Гамильтоновы пути наибольшей силы";
			this.cb_HP_max_strength.UseVisualStyleBackColor = true;
			// 
			// cb_HP_max_length
			// 
			this.cb_HP_max_length.AutoEllipsis = true;
			this.cb_HP_max_length.Dock = System.Windows.Forms.DockStyle.Top;
			this.cb_HP_max_length.Location = new System.Drawing.Point(3, 16);
			this.cb_HP_max_length.Name = "cb_HP_max_length";
			this.cb_HP_max_length.Size = new System.Drawing.Size(253, 17);
			this.cb_HP_max_length.TabIndex = 0;
			this.cb_HP_max_length.Text = "Гамильтоновы пути максимальной стоимости";
			this.cb_HP_max_length.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textBox_file);
			this.groupBox2.Controls.Add(this.button_read_file);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox2.Location = new System.Drawing.Point(3, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(259, 44);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Импорт из txt-файла";
			// 
			// textBox_file
			// 
			this.textBox_file.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_file.Location = new System.Drawing.Point(6, 17);
			this.textBox_file.Name = "textBox_file";
			this.textBox_file.Size = new System.Drawing.Size(155, 20);
			this.textBox_file.TabIndex = 1;
			this.textBox_file.Text = "test.txt";
			// 
			// button_read_file
			// 
			this.button_read_file.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button_read_file.Location = new System.Drawing.Point(160, 16);
			this.button_read_file.Name = "button_read_file";
			this.button_read_file.Size = new System.Drawing.Size(93, 22);
			this.button_read_file.TabIndex = 0;
			this.button_read_file.Text = "Ввод из файла";
			this.button_read_file.UseVisualStyleBackColor = true;
			this.button_read_file.Click += new System.EventHandler(this.button_read_file_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.numericUpDown_m);
			this.groupBox1.Controls.Add(this.button_n_m);
			this.groupBox1.Controls.Add(this.numericUpDown_n);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(3, 53);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(259, 84);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Выбор n и m";
			// 
			// label2
			// 
			this.label2.AutoEllipsis = true;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(106, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Число m экспертов";
			// 
			// label1
			// 
			this.label1.AutoEllipsis = true;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(115, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Число n альтернатив";
			// 
			// numericUpDown_m
			// 
			this.numericUpDown_m.Location = new System.Drawing.Point(127, 33);
			this.numericUpDown_m.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDown_m.Name = "numericUpDown_m";
			this.numericUpDown_m.Size = new System.Drawing.Size(41, 20);
			this.numericUpDown_m.TabIndex = 2;
			this.numericUpDown_m.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// button_n_m
			// 
			this.button_n_m.Location = new System.Drawing.Point(9, 56);
			this.button_n_m.Name = "button_n_m";
			this.button_n_m.Size = new System.Drawing.Size(159, 22);
			this.button_n_m.TabIndex = 1;
			this.button_n_m.Text = "Ввод n и m";
			this.button_n_m.UseVisualStyleBackColor = true;
			this.button_n_m.Click += new System.EventHandler(this.button_n_m_Click);
			// 
			// numericUpDown_n
			// 
			this.numericUpDown_n.Location = new System.Drawing.Point(127, 12);
			this.numericUpDown_n.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDown_n.Name = "numericUpDown_n";
			this.numericUpDown_n.Size = new System.Drawing.Size(41, 20);
			this.numericUpDown_n.TabIndex = 0;
			this.numericUpDown_n.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// button_run_program
			// 
			this.button_run_program.AutoSize = true;
			this.button_run_program.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.button_run_program.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_run_program.Location = new System.Drawing.Point(268, 3);
			this.button_run_program.Name = "button_run_program";
			this.button_run_program.Size = new System.Drawing.Size(54, 44);
			this.button_run_program.TabIndex = 6;
			this.button_run_program.Text = "Пуск!\r\n→";
			this.button_run_program.UseVisualStyleBackColor = true;
			this.button_run_program.Click += new System.EventHandler(this.button_run_program_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.groupBox3, 2);
			this.groupBox3.Controls.Add(this.flowLayoutPanel_input);
			this.groupBox3.Location = new System.Drawing.Point(3, 298);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(319, 190);
			this.groupBox3.TabIndex = 3;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Ввод матриц предпочтений экспертов";
			// 
			// flowLayoutPanel_input
			// 
			this.flowLayoutPanel_input.AutoScroll = true;
			this.flowLayoutPanel_input.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel_input.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPanel_input.Location = new System.Drawing.Point(3, 16);
			this.flowLayoutPanel_input.Name = "flowLayoutPanel_input";
			this.flowLayoutPanel_input.Size = new System.Drawing.Size(313, 171);
			this.flowLayoutPanel_input.TabIndex = 25;
			this.flowLayoutPanel_input.WrapContents = false;
			this.flowLayoutPanel_input.MouseEnter += new System.EventHandler(this.flowLayoutPanel_input_MouseEnter);
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.rb_dist_square);
			this.groupBox5.Controls.Add(this.rb_dist_modulus);
			this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox5.Location = new System.Drawing.Point(3, 233);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(259, 59);
			this.groupBox5.TabIndex = 25;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Выбор расстояния";
			// 
			// rb_dist_square
			// 
			this.rb_dist_square.AutoSize = true;
			this.rb_dist_square.Checked = true;
			this.rb_dist_square.Location = new System.Drawing.Point(4, 34);
			this.rb_dist_square.Name = "rb_dist_square";
			this.rb_dist_square.Size = new System.Drawing.Size(117, 17);
			this.rb_dist_square.TabIndex = 1;
			this.rb_dist_square.TabStop = true;
			this.rb_dist_square.Text = "Квадрат разности";
			this.rb_dist_square.UseVisualStyleBackColor = true;
			// 
			// rb_dist_modulus
			// 
			this.rb_dist_modulus.AutoSize = true;
			this.rb_dist_modulus.Location = new System.Drawing.Point(4, 17);
			this.rb_dist_modulus.Name = "rb_dist_modulus";
			this.rb_dist_modulus.Size = new System.Drawing.Size(113, 17);
			this.rb_dist_modulus.TabIndex = 0;
			this.rb_dist_modulus.TabStop = true;
			this.rb_dist_modulus.Text = "Модуль разности";
			this.rb_dist_modulus.UseVisualStyleBackColor = true;
			// 
			// flowLayoutPanel_output_info
			// 
			this.flowLayoutPanel_output_info.AutoScroll = true;
			this.flowLayoutPanel_output_info.Controls.Add(this.label3);
			this.flowLayoutPanel_output_info.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel_output_info.Location = new System.Drawing.Point(328, 3);
			this.flowLayoutPanel_output_info.Name = "flowLayoutPanel_output_info";
			this.tableLayoutPanel1.SetRowSpan(this.flowLayoutPanel_output_info, 2);
			this.flowLayoutPanel_output_info.Size = new System.Drawing.Size(613, 134);
			this.flowLayoutPanel_output_info.TabIndex = 3;
			this.flowLayoutPanel_output_info.MouseEnter += new System.EventHandler(this.flowLayoutPanel_output_info_MouseEnter);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label3.Location = new System.Drawing.Point(3, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 14);
			this.label3.TabIndex = 7;
			this.label3.Text = "Info...";
			// 
			// button_for_tests
			// 
			this.button_for_tests.AutoSize = true;
			this.button_for_tests.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button_for_tests.Font = new System.Drawing.Font("Calibri", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.button_for_tests.Location = new System.Drawing.Point(268, 53);
			this.button_for_tests.Name = "button_for_tests";
			this.button_for_tests.Size = new System.Drawing.Size(54, 84);
			this.button_for_tests.TabIndex = 26;
			this.button_for_tests.Text = "ПРОГНАТЬ ВСЕ ТЕСТЫ В ПАПКЕ (для разработчика)";
			this.button_for_tests.UseVisualStyleBackColor = true;
			this.button_for_tests.Click += new System.EventHandler(this.button_for_tests_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(944, 491);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MinimumSize = new System.Drawing.Size(800, 500);
			this.Name = "Form1";
			this.Text = "Алгоритмы группового выбора, использующие пути в орграфе";
			this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.flowLayoutPanel_output_tables.ResumeLayout(false);
			this.groupBox_HP_max_length.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dg_HP_max_length)).EndInit();
			this.groupBox_HP_max_strength.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dg_HP_max_strength)).EndInit();
			this.groupBox_Schulze_method.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dg_Schulze_method)).EndInit();
			this.groupBox_All_rankings.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dg_All_rankings)).EndInit();
			this.groupBox4.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown_m)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown_n)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.flowLayoutPanel_output_info.ResumeLayout(false);
			this.flowLayoutPanel_output_info.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button button_run_program;
		private System.Windows.Forms.Button button_n_m;
		private System.Windows.Forms.Button button_read_file;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox cb_All_rankings;
		private System.Windows.Forms.CheckBox cb_Schulze_method;
		private System.Windows.Forms.CheckBox cb_HP_max_strength;
		private System.Windows.Forms.CheckBox cb_HP_max_length;
		private System.Windows.Forms.TextBox textBox_file;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_output_tables;
		private System.Windows.Forms.GroupBox groupBox_HP_max_length;
		private System.Windows.Forms.DataGridView dg_HP_max_length;
		private System.Windows.Forms.GroupBox groupBox_HP_max_strength;
		private System.Windows.Forms.DataGridView dg_HP_max_strength;
		private System.Windows.Forms.GroupBox groupBox_Schulze_method;
		private System.Windows.Forms.DataGridView dg_Schulze_method;
		private System.Windows.Forms.GroupBox groupBox_All_rankings;
		private System.Windows.Forms.DataGridView dg_All_rankings;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_input;
		private System.Windows.Forms.NumericUpDown numericUpDown_n;
		private System.Windows.Forms.NumericUpDown numericUpDown_m;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.RadioButton rb_dist_square;
		private System.Windows.Forms.RadioButton rb_dist_modulus;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_output_info;
		private System.Windows.Forms.Button button_for_tests;
	}
}

