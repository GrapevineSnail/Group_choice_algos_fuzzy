using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
namespace Pathfinding
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public int vertecesCount = 0;
        public int cellsNumber = 0;
        public int minVertecesCount = 0;

        
        public int[,] AdjMatrix;
        public double[,] FzMatrix; //матрица нечетких соотношений
        public double[,] _FzMatrix;
        public int[,] _AdjMatrix;
        public ArrayList graphComponents = new ArrayList();
        static int d = (int)DateTime.Now.Ticks;
        Random rnd = new Random(d);
        public string Line = Environment.NewLine;
        int H, W;


        private static int Nmax;

        private int[,] A;
        private double[,] F;
        private int[,] Alg_P;
        private int[,] Rand_m;
        private int[,] _Rand_m;
        private int[] Exc; //массив эксцентриситетов вершин по расстояниям
        private double[] Exc_f; //массив эксцентриситетов вершин по нечетким отношениям
        private int[,] Fl; //массив 
        private double[,] Ff;
        private double[] Cent; // массив центров графа по расстояниям
        private double[] Cent_f; // массив центров по нечетким отношениям
        private double[] vs; //сравнение центров
        int rad = 10000; // радиус графа
        double rad_f = 0; // радиус нечеткого графа
        private int[] Stack;
        int[][] Ribs;
        List<Edge> edgeArchive = new List<Edge>();
        List<FzEdge> fzedgeArchive = new List<FzEdge>();
        public int PointOfDeparture;
        public List<int> Price = new List<int>();
        private void Form1_Load(object sender, EventArgs e)
        {
            increaseVertex_ValueChanged(sender, e);
            W = drawField.Width;
            H = drawField.Height;
        }

        private void increaseVertex_ValueChanged(object sender, EventArgs e)
        {

            vertecesCount = (int)increaseVertex.Value;
            cellsNumber = (int)increaseVertex.Value + 1;

            Nmax = vertecesCount;
            int N_St = Nmax * (Nmax - 1) / 2;
            Ribs = new int[Nmax - 1][];
            Alg_P = new int[Nmax, Nmax];
            Stack = new int[0];

            gridMatrix.ColumnCount = cellsNumber;
            gridMatrix.RowCount = cellsNumber;

            gridFuzzyMatrix.ColumnCount = cellsNumber;
            gridFuzzyMatrix.RowCount = cellsNumber;

            AdjMatrix = new int[cellsNumber, cellsNumber];
            FzMatrix = new double[cellsNumber, cellsNumber];

            gridMatrix.Rows[0].Cells[0].Style.BackColor = SystemColors.AppWorkspace;
            gridMatrix.Rows[0].Cells[0].Style.ForeColor = SystemColors.AppWorkspace;

            gridFuzzyMatrix.Rows[0].Cells[0].Style.BackColor = SystemColors.AppWorkspace;
            gridFuzzyMatrix.Rows[0].Cells[0].Style.ForeColor = SystemColors.AppWorkspace;

            for (int i = 1; i < cellsNumber; i++)
            {
                gridMatrix[0, i].Value = i;
                gridMatrix[0, i].Style.BackColor = Color.LightGray;
                gridMatrix[i, 0].Value = gridMatrix[0, i].Value;
                gridMatrix[i, 0].Style.BackColor = gridMatrix[0, i].Style.BackColor;

                gridFuzzyMatrix[0, i].Value = i;
                gridFuzzyMatrix[0, i].Style.BackColor = Color.LightGray;
                gridFuzzyMatrix[i, 0].Value = gridFuzzyMatrix[0, i].Value;
                gridFuzzyMatrix[i, 0].Style.BackColor = gridFuzzyMatrix[0, i].Style.BackColor;
            }

            //заполнение диагонали
            for (int i = 1; i < cellsNumber; i++)
            {
                gridMatrix[i, i].Value = 0;
                gridFuzzyMatrix[i, i].Value = 1;

                gridMatrix[i, i].Style.BackColor = Color.Bisque;
                gridFuzzyMatrix[i, i].Style.BackColor = Color.Bisque;
            }
        }

        private void drawField_Paint(object sender, PaintEventArgs e)
        {

        }

        private void drawingGraph_Click(object sender, EventArgs e)
        {
            btnClear.Enabled = true;
            btnPathFind.Enabled = true;
            Gamilton.Enabled = true;

            Applying();
            drawField.Refresh();

            Graphics gr = drawField.CreateGraphics();
            Vertex[] verteces = new Vertex[vertecesCount]; //массив вершин
            Edge[] edges = new Edge[vertecesCount]; //массив рёбер

            int size = 30; //размер вершины
            int x = rnd.Next(size, drawField.Width - size);
            int y = rnd.Next(size, drawField.Height - size);

            for (int i = 0; i < vertecesCount; i++)
            {
                switch (i)
                {
                    case 0:
                        x = rnd.Next(W / 4 - size);
                        y = rnd.Next(H / 2 - size);
                        break;
                    case 1:
                        x = rnd.Next(W / 4 - size, W / 2 - size);
                        y = rnd.Next(H / 2 - size);
                        break;
                    case 2:
                        x = rnd.Next(W / 2 - size, 3 * W / 4 - size);
                        y = rnd.Next(H / 2 - size);
                        break;
                    case 3:
                        x = rnd.Next(3 * W / 4 - size, W - size);
                        y = rnd.Next(H / 2 - size);
                        break;
                    case 4:
                        x = rnd.Next(W / 4 - size);
                        y = rnd.Next(H / 2 - size, H - size);
                        break;
                    case 5:
                        x = rnd.Next(W / 4 - size, W / 2 - size);
                        y = rnd.Next(H / 2 - size, H - size);
                        break;
                    case 6:
                        x = rnd.Next(W / 2 - size, 3 * W / 4 - size);
                        y = rnd.Next(H / 2 - size, H - size);
                        break;
                    case 7:
                        x = rnd.Next(3 * W / 4 - size, W - size);
                        y = rnd.Next(H / 2 - size, H - size);
                        break;
                }
                verteces[i] = new Vertex(x, y, size, size, (i + 1).ToString());
            }
            
            //создание, отрисовка рёбер
            for (int i = 0; i < vertecesCount; i++)
            {
                for (int j = i + 1; j < vertecesCount; j++)
                {
                    if (_AdjMatrix[i, j] != 0)
                    {
                        edges[i] = new Edge(verteces[i].X + size / 2, verteces[i].Y + size / 2, verteces[j].X + size / 2, verteces[j].Y + size / 2, _AdjMatrix[i, j].ToString());
                        edges[i].Draw(gr);
                        edgeArchive.Add(edges[i]);
                    }
                }
            }

            foreach (var v in verteces)
                v.Draw(gr);
            Init();


        }

        private void DrawingFzGraph_Click_1(object sender, EventArgs e)
        {
            btnClear.Enabled = true;
            btnPathFind.Enabled = true;

            Applying();
            drawField.Refresh();

            Graphics fzgr = drawField.CreateGraphics();
            FzVertex[] fzverteces = new FzVertex[vertecesCount]; //массив вершин
            FzEdge[] fzedges = new FzEdge[vertecesCount]; //массив рёбер

            int size = 30; //размер вершины
            int x = rnd.Next(size, drawField.Width - size);
            int y = rnd.Next(size, drawField.Height - size);

            for (int i = 0; i < vertecesCount; i++)
            {
                switch (i)
                {
                    case 0:
                        x = rnd.Next(W / 4 - size);
                        y = rnd.Next(H / 2 - size);
                        break;
                    case 1:
                        x = rnd.Next(W / 4 - size, W / 2 - size);
                        y = rnd.Next(H / 2 - size);
                        break;
                    case 2:
                        x = rnd.Next(W / 2 - size, 3 * W / 4 - size);
                        y = rnd.Next(H / 2 - size);
                        break;
                    case 3:
                        x = rnd.Next(3 * W / 4 - size, W - size);
                        y = rnd.Next(H / 2 - size);
                        break;
                    case 4:
                        x = rnd.Next(W / 4 - size);
                        y = rnd.Next(H / 2 - size, H - size);
                        break;
                    case 5:
                        x = rnd.Next(W / 4 - size, W / 2 - size);
                        y = rnd.Next(H / 2 - size, H - size);
                        break;
                    case 6:
                        x = rnd.Next(W / 2 - size, 3 * W / 4 - size);
                        y = rnd.Next(H / 2 - size, H - size);
                        break;
                    case 7:
                        x = rnd.Next(3 * W / 4 - size, W - size);
                        y = rnd.Next(H / 2 - size, H - size);
                        break;
                }
                fzverteces[i] = new FzVertex(x, y, size, size, (i + 1).ToString());
            }

            //создание, отрисовка рёбер
            for (int i = 0; i < vertecesCount; i++)
            {
                for (int j = i + 1; j < vertecesCount; j++)
                {
                    if (_FzMatrix[i, j] != 0)
                    {
                        fzedges[i] = new FzEdge(fzverteces[i].X + size / 2, fzverteces[i].Y + size / 2, fzverteces[j].X + size / 2, fzverteces[j].Y + size / 2, _FzMatrix[i, j].ToString());
                        fzedges[i].Draw(fzgr);
                        fzedgeArchive.Add(fzedges[i]);
                    }
                }
            }

            foreach (var v in fzverteces)
                v.Draw(fzgr);
            Init();


        }

        //инициализация вершин и массива
        private void Init()
        {
            Nmax = vertecesCount;
            A = _AdjMatrix;
            F = _FzMatrix;
        }

        // Строит каркас минимального веса
        private void FindTree(int[,] Alg_P)
        {
            Set Sp = new Set();
            int min = 100;
            int l = 0, t = 0;
            for (int i = 0; i < Nmax - 1; i++)
                for (int j = 1; j < Nmax; j++)
                    if ((A[i, j] < min) && (A[i, j] != 0))
                    {
                        min = A[i, j];
                        l = i;
                        t = j;
                    }
            Alg_P[l, t] = A[l, t];
            Alg_P[t, l] = A[t, l];
            Sp.Add(l + 1);
            Sp.Add(t + 1);

            int ribIndex = 0;
            Ribs[ribIndex] = new int[2];
            Ribs[ribIndex][0] = l + 1;
            Ribs[ribIndex][1] = t + 1;
            ribIndex++;

            while (!Sp.Contains(1, Nmax))
            {
                min = 100;
                l = 0; t = 0;
                for (int i = 0; i < Nmax; i++)
                    if (Sp.Contains(i + 1))
                        for (int j = 0; j < Nmax; j++)
                            if (!Sp.Contains(j + 1) && (A[i, j] < min) && (A[i, j] != 0))
                            {
                                min = A[i, j];
                                l = i;
                                t = j;
                            }
                Alg_P[l, t] = A[l, t];
                Alg_P[t, l] = A[t, l];
                Sp.Add(l + 1);
                Sp.Add(t + 1);

                Ribs[ribIndex] = new int[2];
                Ribs[ribIndex][0] = l + 1;
                Ribs[ribIndex][1] = t + 1;
                ribIndex++;
            }
        }

        //Поиск пути
        private void FindWay(int v)
        {
            for (int i = 0; i < Nmax; i++)
                if (Alg_P[v, i] != 0)
                {
                    Alg_P[v, i] = 0;
                    FindWay(i);
                }
            int[] temp = (int[])Stack.Clone();
            Stack = new int[Stack.Length + 1];
            for (int i = 0; i < temp.Length; i++)
                Stack[i] = temp[i];
            Stack[Stack.Length - 1] = v + 1;
        }

        int v = 0;
        private void Centre()
        {
            Fl = new int[Nmax, Nmax];
            Ff = new double[Nmax, Nmax];
            Exc = new int[9];
            Exc_f = new double[9];
            Cent = new double[9];
            Cent_f = new double[9];
            vs = new double[9];

            for (int i = 0; i < Nmax; i++)
            {
                for (int j = 0; j < Nmax; j++)
                {
                        Fl[i, j] = A[i, j];
                        Ff[i, j] = F[i, j];
                }
            }

            for (int i = 0; i < Nmax; i++)
            {
                for (int j = 0; j < Nmax; j++) 
                {
                    if (Fl[i, j] > Exc[i])
                        Exc[i] = Fl[i, j];
                }
            }

            for (int i = 0; i < Nmax; i++)
            {
                for (int j = 0; j < Nmax; j++)
                {
                    if ((i != j) && (Ff[i, j] != 0))
                    {
                        Exc_f[i] = Ff[i, j];
                    }
                }
            }

            for (int i = 0; i < Nmax; i++)
            {
                for (int j = 0; j < Nmax; j++)
                {
                    if ((i != j) && (Exc_f[i] > Ff[i, j]))
                    {
                        Exc_f[i] = Ff[i, j];
                    }
                }
            }

            for (int i = 0; i < Nmax; i++)
            {
                if (Exc[i] < rad) 
                {
                    rad = Exc[i];
                }
            }

            for (int i = 0; i < Nmax; i++)
            {
                if (Exc_f[i] > rad_f)
                {
                    rad_f = Exc_f[i];
                }
            }

            for (int i = 0; i < Nmax; i++)
            {
                if (Exc[i] == rad)
                {
                    Cent[i] = 1; // массив центров графа, потом для каждого индекса единицы перебрать фзматрицу
                                  // ищем в подходящих строках минимальное значение степени достоверности, заменяем единицу на него
                                     // после этого сравниваем минимумы и находим максимальное из них, ответом является его индекс в массиве
                }
            }

            for (int i = 0; i < Nmax; i++)
            {
                if (Exc_f[i] == rad_f)
                {
                    Cent_f[i] = 1;      
                }
            }

            for (int i = 0; i < Nmax; i++)
            {
                vs[i] = Cent[i] + Cent_f[i]; //поиск совпадений центров
                if (vs[i] == 2)
                {
                    v += 1;
                }
            }
        }

        // Вывод результата
        private void OutPut()
        {
            int i;
            textBox1.Text += Line;
            textBox1.Text += "Вершины, входящие в центр графа по расстояниям: ";
            int check = 0;
            for (i = 0; i < Nmax; i++)
                if (Exc[i] == rad)
                {
                    check++;
                }
            int z = 0;       
            for (i = 0; i < Nmax; i++)
                if (Exc[i] == rad)
                {
                    z++;
                    textBox1.Text += i+1;
                    if ((check > 1) && (check != z))
                        textBox1.Text += "; ";
                }

            textBox1.Text += Line;
            textBox1.Text += Line;

            textBox1.Text += "Радиус графа по расстояниям: " + rad;

            textBox1.Text += Line;
            textBox1.Text += Line;

            textBox1.Text += "Вершины, входящие в центр графа нечетких отношений: ";
            check = 0;
            for (i = 0; i < Nmax; i++)
                if (Exc_f[i] == rad_f)
                {
                    check++;
                }
            z = 0;
            for (i = 0; i < Nmax; i++)
                if (Exc_f[i] == rad_f)
                {
                    z++;
                    textBox1.Text += i + 1;
                    if ((check > 1) && (check != z))
                        textBox1.Text += "; ";
                }

            textBox1.Text += Line;
            textBox1.Text += Line;

            textBox1.Text += "Радиус графа нечетких отношений: " + rad_f;

            textBox1.Text += Line;
            textBox1.Text += Line;

            textBox1.Text += "Рекомендация для ЛПР: ";
            textBox1.Text += Line;
            if (v == 0)
            {
                textBox1.Text += "Центры графов не совпали. Попробуйте поменять значения нечетких отношений";
            }
            else
            {
                textBox1.Text += "Найдены совпадения центров. Совпадают вершины под номером ";
            }
            z = 0;
            for (i = 0; i < Nmax; i++)
            { 
                if (vs[i] == 2)
                {
                    z++;
                    textBox1.Text += i + 1;
                    if ((v > 1) && (v != z))
                        textBox1.Text += "; ";
                }
            }

            
            textBox1.Text += Line;
            textBox1.Text += Line;
            textBox1.Text += "*******************************************************";
            //textBox1.Text += "exc_f: ";
            //for (i = 0; i < Nmax; i++)
            //{
            //    textBox1.Text += Exc_f[i];
            //    textBox1.Text += "; ";              
            //}
        }

        private void gridMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //зеркальное отображение значений
            if (SymmetryCheck.Checked == false)
                gridMatrix[e.RowIndex, e.ColumnIndex].Value = gridMatrix[e.ColumnIndex, e.RowIndex].Value;
        }

        private void gridFuzzyMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //зеркальное отображение значений
            if (SymmetryCheck.Checked == false)
                gridFuzzyMatrix[e.RowIndex, e.ColumnIndex].Value = gridFuzzyMatrix[e.ColumnIndex, e.RowIndex].Value;

        }

        private void gridMatrix_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //запрет на изменение значений по диагонали
            if (e.RowIndex == e.ColumnIndex)
                e.Cancel = true;


            //запрет на редактирование ячеек
            //отвечающих за нумерацию строк и столбцов
            for (int i = 1; i < cellsNumber; i++)
            {
                if (e.RowIndex == 0 && e.ColumnIndex == i || (e.RowIndex == i && e.ColumnIndex == 0))
                    e.Cancel = true;
            }
        }

        private void gridMatrix_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (SymmetryCheck.Checked == false)
                return;

            if (e.RowIndex > e.ColumnIndex)
            {
                if (Convert.ToInt32(gridMatrix[e.RowIndex, e.ColumnIndex].Value) != 0)
                    gridMatrix[e.RowIndex, e.ColumnIndex].Value = gridMatrix[e.ColumnIndex, e.RowIndex].Value;
            }
            else
            {
                if (Convert.ToInt32(gridMatrix[e.ColumnIndex, e.RowIndex].Value) != 0)
                {
                    gridMatrix[e.RowIndex, e.ColumnIndex].Value = gridMatrix[e.ColumnIndex, e.RowIndex].Value;
                    gridFuzzyMatrix[e.ColumnIndex, e.RowIndex].Value = gridFuzzyMatrix[e.RowIndex, e.ColumnIndex].Value;
                }
                else
                {
                    while (gridFuzzyMatrix[e.ColumnIndex, e.RowIndex].Value == gridFuzzyMatrix[e.RowIndex, e.ColumnIndex].Value)
                    {
                        gridFuzzyMatrix[e.ColumnIndex, e.RowIndex].Value = Math.Round(rnd.NextDouble(), 1);
                    }
                }
            }

        }

        private void gridFuzzyMatrix_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (SymmetryCheck.Checked == false)
                return;

            if (gridMatrix[e.ColumnIndex, e.RowIndex].Value == gridMatrix[e.RowIndex, e.ColumnIndex].Value)
                gridFuzzyMatrix[e.RowIndex, e.ColumnIndex].Value = gridFuzzyMatrix[e.ColumnIndex, e.RowIndex].Value;
        }

        private void gridFuzzyMatrix_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            //запрет на изменение значений по диагонали
            if ((e.RowIndex == e.ColumnIndex))
                e.Cancel = true;


            //запрет на редактирование ячеек
            //отвечающих за нумерацию строк и столбцов
            for (int i = 1; i < cellsNumber; i++)
            {
                if (e.RowIndex == 0 && e.ColumnIndex == i || (e.RowIndex == i && e.ColumnIndex == 0))
                    e.Cancel = true;
            }
        }

        private void gridMatrix_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ((DataGridView)sender).SelectedCells[0].Selected = false;
            }
            catch { }


        }

        private void gridFuzzyMatrix_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ((DataGridView)sender).SelectedCells[0].Selected = false;
            }
            catch { }


        }

        private void gridMatrix_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            gridMatrix.BeginEdit(true);
        }

        private void gridFuzzyMatrix_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            gridFuzzyMatrix.BeginEdit(true);
        }

        private void btnRandomlyFuzz_Click(object sender, EventArgs e)
        {
            vertecesCount = (int)increaseVertex.Value;
            cellsNumber = (int)increaseVertex.Value + 1;

            gridFuzzyMatrix.ColumnCount = cellsNumber;
            gridFuzzyMatrix.RowCount = cellsNumber;

            gridFuzzyMatrix.Rows[0].Cells[0].Style.BackColor = SystemColors.AppWorkspace;
            gridFuzzyMatrix.Rows[0].Cells[0].Style.ForeColor = SystemColors.AppWorkspace;

            //нумерация строк и столбцов
            for (int i = 1; i < cellsNumber; i++)
            {
                gridFuzzyMatrix[0, i].Value = i;
                gridFuzzyMatrix[0, i].Style.BackColor = Color.LightGray;
                gridFuzzyMatrix[i, 0].Value = gridFuzzyMatrix[0, i].Value;
                gridFuzzyMatrix[i, 0].Style.BackColor = gridFuzzyMatrix[0, i].Style.BackColor;
            }

            //заполнение диагонали
            for (int i = 1; i < cellsNumber; i++)
            {
                gridFuzzyMatrix[i, i].Value = 1;
                gridFuzzyMatrix[i, i].Style.BackColor = Color.Bisque;
            }

            //случайное заполнение матрицы ниже главной диагонали
            for (int i = 1; i < cellsNumber; i++)
            {
                for (int j = i + 1; j < cellsNumber; j++)
                {
                    if (i == j)
                        gridFuzzyMatrix[i, i].Value = 0;
                    else
                        gridFuzzyMatrix[i, j].Value = Math.Round(rnd.NextDouble(), 1);
                }
            }

            for (int i = 1; i < cellsNumber; i++)
            {
                for (int j = 1; j < cellsNumber; j++)
                {
                    FzMatrix[i, j] = Convert.ToDouble(gridFuzzyMatrix[i, j].Value);
                }
            }
        }
        private void btnRandomly_Click(object sender, EventArgs e)
        {
            vertecesCount = (int)increaseVertex.Value;
            cellsNumber = (int)increaseVertex.Value + 1;

            gridMatrix.ColumnCount = cellsNumber;
            gridMatrix.RowCount = cellsNumber;

            AdjMatrix = new int[cellsNumber, cellsNumber];

            gridMatrix.Rows[0].Cells[0].Style.BackColor = SystemColors.AppWorkspace;
            gridMatrix.Rows[0].Cells[0].Style.ForeColor = SystemColors.AppWorkspace;

            //нумерация строк и столбцов
            for (int i = 1; i < cellsNumber; i++)
            {
                gridMatrix[0, i].Value = i;
                gridMatrix[0, i].Style.BackColor = Color.LightGray;
                gridMatrix[i, 0].Value = gridMatrix[0, i].Value;
                gridMatrix[i, 0].Style.BackColor = gridMatrix[0, i].Style.BackColor;
            }

            //заполнение диагонали
            for (int i = 1; i < cellsNumber; i++)
            {
                gridMatrix[i, i].Value = 0;
                gridMatrix[i, i].Style.BackColor = Color.Bisque;
            }

            //случайное заполнение матрицы ниже главной диагонали
            for (int i = 1; i < cellsNumber; i++)
            {
                for (int j = i + 1; j < cellsNumber; j++)
                {
                    if (i == j)
                        gridMatrix[i, i].Value = 99;
                    else
                        gridMatrix[i, j].Value = rnd.Next(0, 15);
                }
            }

            ////заполнение матрицы выше главной диагонали происходит с помощью метода gridMatrix_CellValueChanged()
            //в котором реализовано зеркальное отображение значений

            //матрица смежности без учета нумерации строк и столбцов
            for (int i = 1; i < cellsNumber; i++)
            {
                for (int j = 1; j < cellsNumber; j++)
                {
                    AdjMatrix[i, j] = Convert.ToInt32(gridMatrix[i, j].Value);
                }
            }
            Applying();
        }

        //сохранение введённых значений в таблице
        //запись данных в матрицу смежности
        private void Applying()
        {
            //матрица смежности без учета нумерации строк и столбцов
            for (int i = 1; i < cellsNumber; i++)
            {
                for (int j = 1; j < cellsNumber; j++)
                {
                    AdjMatrix[i, j] = Convert.ToInt32(gridMatrix[i, j].Value);
                }
            }

            _AdjMatrix = new int[vertecesCount, vertecesCount];

            for (int i = 0; i < AdjMatrix.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < AdjMatrix.GetLength(1) - 1; j++)
                {
                    _AdjMatrix[i, j] = AdjMatrix[i + 1, j + 1];
                }
            }

            for (int i = 1; i < cellsNumber; i++)
            {
                for (int j = 1; j < cellsNumber; j++)
                {
                    FzMatrix[i, j] = Convert.ToDouble(gridFuzzyMatrix[i, j].Value);
                }
            }

            _FzMatrix = new double[vertecesCount, vertecesCount];

            for (int i = 0; i < FzMatrix.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < FzMatrix.GetLength(1) - 1; j++)
                {
                    _FzMatrix[i, j] = FzMatrix[i + 1, j + 1];
                }
            }
        }

        private void cmbPointOfDeparture_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnPathFind.Enabled = true;
        }

        private void btnPathFind_Click(object sender, EventArgs e)
        {
            
            //int start = (int)nudPointOfDeparture.Value - 1;
            FindTree(Alg_P);
            //FindWay(start);
            Centre();
            OutPut();

            Price.Clear();

            Nmax = vertecesCount;
            int N_St = Nmax * (Nmax - 1) / 2;
            Ribs = new int[Nmax - 1][];
            Alg_P = new int[Nmax, Nmax];
            Stack = new int[0];
            rad = 10000;
            Exc = new int[8];
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void нечеткаяЛогикаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FuzzyLogic fuzzyLogic = new FuzzyLogic();
            fuzzyLogic.ShowDialog();
        }

        private void алгоритмToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Algorythm algorythm = new Algorythm();
            algorythm.ShowDialog();
        }

        private void строительствоДорогToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Roads roads = new Roads();
            roads.ShowDialog();
        }

        private void логистикаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logistics logistics = new Logistics();
            logistics.ShowDialog();
        }

        private void SymmetryCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (SymmetryCheck.Checked == true)
            {
                RandomOr.Enabled = true;
                btnRandomly.Enabled = false;
                btnRandomlyFuzz.Enabled = false;
            }
            else
            {
                RandomOr.Enabled = false;
                btnRandomly.Enabled = true;
                btnRandomlyFuzz.Enabled = true;
            }
        }

        private void RandomOr_Click(object sender, EventArgs e)
        {
            vertecesCount = (int)increaseVertex.Value;
            cellsNumber = (int)increaseVertex.Value + 1;
            Nmax = vertecesCount;

            gridMatrix.ColumnCount = cellsNumber;
            gridMatrix.RowCount = cellsNumber;

            AdjMatrix = new int[cellsNumber, cellsNumber];

            gridMatrix.Rows[0].Cells[0].Style.BackColor = SystemColors.AppWorkspace;
            gridMatrix.Rows[0].Cells[0].Style.ForeColor = SystemColors.AppWorkspace;

            //нумерация строк и столбцов
            for (int i = 1; i < cellsNumber; i++)
            {
                gridMatrix[0, i].Value = i;
                gridMatrix[0, i].Style.BackColor = Color.LightGray;
                gridMatrix[i, 0].Value = gridMatrix[0, i].Value;
                gridMatrix[i, 0].Style.BackColor = gridMatrix[0, i].Style.BackColor;
            }

            // Заполнение диагонали
            for (int i = 1; i < cellsNumber; i++)
            {
                gridMatrix[i, i].Value = 0;
                gridMatrix[i, i].Style.BackColor = Color.Bisque;
            }

            // Заполнение ниже диагонали
            for (int i = 1; i < cellsNumber - 1; i++)
            {
                for (int j = i + 1; j < cellsNumber; j++)
                {
                    if (i == j)
                    {
                        gridMatrix[i, i].Value = 0;
                        gridFuzzyMatrix[i, i].Value = 0;
                    }
                    else
                    {
                        gridMatrix[i, j].Value = rnd.Next(4, 15);
                        gridFuzzyMatrix[i, j].Value = Math.Round(rnd.NextDouble(), 1);
                    }
                }
            }

            // Заполнение выше диагонали
            for (int i = 2; i < cellsNumber; i++)
            {
                for (int j = 1; j < i; j++)
                {
                    if (i == j)
                    {
                        gridMatrix[i, i].Value = 0;
                        gridFuzzyMatrix[i, i].Value = 0;
                    }
                    else
                    {
                        int rand_val = rnd.Next(0, 2);
                        if (rand_val == 0)
                        {
                            gridMatrix[i, j].Value = 0;
                            gridFuzzyMatrix[i, j].Value = gridFuzzyMatrix[j, i].Value;
                            while (gridFuzzyMatrix[i, j].Value == gridFuzzyMatrix[j, i].Value)
                                gridFuzzyMatrix[i, j].Value = Math.Round(rnd.NextDouble(), 1);
                        }
                        else
                        {
                            gridMatrix[i, j].Value = gridMatrix[j, i].Value;
                            gridFuzzyMatrix[i, j].Value = gridFuzzyMatrix[j, i].Value;
                        }
                    }
                }
            }

            for (int i = 1; i < cellsNumber; i++)
            {
                for (int j = 1; j < cellsNumber; j++)
                {
                    AdjMatrix[i, j] = Convert.ToInt32(gridMatrix[i, j].Value);
                }
            }

            Applying();
        }

        private void ShWay_Click(object sender, EventArgs e)
        {
            Applying();

            vertecesCount = (int)increaseVertex.Value;

            for (int i = 0; i < vertecesCount; i++)
            {
                for (int j = 0; j < vertecesCount; j++)
                {
                    Console.Write(_AdjMatrix[i, j]);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }

            for (int i = 1; i < vertecesCount; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (_AdjMatrix[i, j] == 0)
                    {
                        gridMatrix[i + 1, j + 1].Value = Dijkstra(_AdjMatrix, i, vertecesCount)[j];
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            drawField.Invalidate();
            textBox1.Text = string.Empty;
        }

        private static int MinimumDistance(int[] distance, bool[] shortestPathTreeSet, int verticesCount)
        {
            int min = int.MaxValue;
            int minIndex = 0;

            for (int v = 0; v < verticesCount; ++v)
            {
                if (shortestPathTreeSet[v] == false && distance[v] <= min)
                {
                    min = distance[v];
                    minIndex = v;
                }
            }

            return minIndex;
        }

        public static int[] Dijkstra(int[,] graph, int source, int verticesCount)
        {
            int[] distance = new int[verticesCount];
            bool[] shortestPathTreeSet = new bool[verticesCount];

            for (int i = 0; i < verticesCount; ++i)
            {
                distance[i] = int.MaxValue;
                shortestPathTreeSet[i] = false;
            }

            distance[source] = 0;

            for (int count = 0; count < verticesCount - 1; ++count)
            {
                int u = MinimumDistance(distance, shortestPathTreeSet, verticesCount);
                shortestPathTreeSet[u] = true;

                for (int v = 0; v < verticesCount; ++v)
                    if (!shortestPathTreeSet[v] && Convert.ToBoolean(graph[u, v]) && distance[u] != int.MaxValue && distance[u] + graph[u, v] < distance[v])
                        distance[v] = distance[u] + graph[u, v];
            }

            return (distance);
        }
    }
}
