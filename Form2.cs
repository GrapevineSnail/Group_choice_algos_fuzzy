using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuikGraph;
using GraphX.Controls;
using GraphX.Common.Models;
using System.Windows;
using GraphX.Common.Enums;
using GraphX.Logic.Algorithms.OverlapRemoval;
using GraphX.Logic.Models;
using GraphX.Controls.Models;
using static Group_choice_algos_fuzzy.Constants;
using static Microsoft.Msagl.Drawing.Graph;
using System.Drawing.Imaging;
/// <summary>
/// /////////////
/// </summary>


namespace Group_choice_algos_fuzzy
{
	public partial class Form2 : Form
	{
		public Form2(double[,] matrix)
		{
			InitializeComponent();
			//Load += Form1_Load;
			weight_matrix = matrix;



			Microsoft.Msagl.Drawing.Graph graph = new
Microsoft.Msagl.Drawing.Graph("");
			graph.AddEdge("A", "B");
			graph.AddEdge("A", "B");
			graph.FindNode("A").Attr.FillColor =
			Microsoft.Msagl.Drawing.Color.Red;
			graph.FindNode("B").Attr.FillColor =
			Microsoft.Msagl.Drawing.Color.Blue;
			Microsoft.Msagl.GraphViewerGdi.GraphRenderer renderer
			= new Microsoft.Msagl.GraphViewerGdi.GraphRenderer
			(graph);
			renderer.CalculateLayout();
			int width = 50;
			Bitmap bitmap = new Bitmap(width, (int)(graph.Height *
			(width / graph.Width)), PixelFormat.Format32bppPArgb);
			renderer.Render(bitmap);
			bitmap.Save("test.png");


		}

		private ZoomControl _zoomctrl;
		private GraphAreaExample _gArea;
		private double[,] weight_matrix;

		void Form1_Load(object sender, EventArgs e)
		{
			elementHost1.Child = GenerateWpfVisuals();
			_gArea.GenerateGraph(true);
			_zoomctrl.ZoomToFill();
		}

		public class DataVertex : VertexBase
		{
			public string Text { get; set; }
			public override string ToString() { return Text; }
			public DataVertex(string text = "") { Text = text; }
		}

		public class DataEdge : EdgeBase<DataVertex>
		{
			/// <summary>
			/// Default constructor. We need to set at least Source and Target properties of the edge.
			/// </summary>
			/// <param name="source">Source vertex data</param>
			/// <param name="target">Target vertex data</param>
			/// <param name="weight">Optional edge weight</param>
			public DataEdge(DataVertex source, DataVertex target, double weight = 1)
				: base(source, target, weight) { }
			public string Text { get; set; }
			public override string ToString() { return Text; }
		}

		public class GraphExample : BidirectionalGraph<DataVertex, DataEdge> { }

		public class GraphAreaExample : GraphArea<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>> { }

		private UIElement GenerateWpfVisuals()
		{
			_zoomctrl = new ZoomControl();
			ZoomControl.SetViewFinderVisibility(_zoomctrl, Visibility.Visible);
			var logic = new GXLogicCore<DataVertex, DataEdge, BidirectionalGraph<DataVertex, DataEdge>>();
			_gArea = new GraphAreaExample
			{
				LogicCore = logic,
				EdgeLabelFactory = new DefaultEdgelabelFactory()
			};
			_gArea.ShowAllEdgesLabels(true);
			logic.Graph = GenerateGraph(weight_matrix);
			logic.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.FR;
			logic.DefaultLayoutAlgorithmParams = logic.AlgorithmFactory.CreateLayoutParameters(
				LayoutAlgorithmTypeEnum.LinLog);
			logic.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.PathFinder;
			logic.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
			logic.DefaultOverlapRemovalAlgorithmParams = logic.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
			((OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
			((OverlapRemovalParameters)logic.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;
			logic.AsyncAlgorithmCompute = false;
			_zoomctrl.Content = _gArea;
			_gArea.RelayoutFinished += (object sender, EventArgs e) => { _zoomctrl.ZoomToFill(); };

			var myResourceDictionary = new ResourceDictionary { Source = new Uri("template.xaml", UriKind.Relative) };
			_zoomctrl.Resources.MergedDictionaries.Add(myResourceDictionary);

			return _zoomctrl;
		}

		/// <summary>
		/// создадим орграф
		/// </summary>
		/// <param name="M">матрица весов орграфа</param>
		/// <returns></returns>
		private GraphExample GenerateGraph(double[,] M)
		{
			if (M.GetLength(0) != M.GetLength(1))
				throw new MyException(EX_matrix_not_square);
			int n = M.GetLength(0);
			var dataGraph = new GraphExample();
			for (int i = 0; i < n; i++)
			{
				var dataVertex = new DataVertex(ind2letter[i]);
				dataGraph.AddVertex(dataVertex);
			}
			var vlist = dataGraph.Vertices.ToList();
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					if (M[i, j] != 0 && Math.Abs(M[i, j]) != INF)
					{
						var dataEdge = new DataEdge(vlist[i], vlist[j]) { Text = string.Format("{0:0.####}", M[i, j]) };
						dataGraph.AddEdge(dataEdge);
					}
				}
			}
			return dataGraph;
		}
	}
}
