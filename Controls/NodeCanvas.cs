using ABI.Windows.Foundation;
using ABI.Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using NodeSharp.Classes;
using NodeSharp.Models;
using NodeSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI;

namespace NodeSharp.Controls
{
	public sealed class NodeCanvas : UserControl
	{
		#region Variables y Propiedades
		private List<FlowNode> nodes = new List<FlowNode>();
		private FlowNode selectedNode = null;
		private FlowNode draggedNode = null;
		private Windows.Foundation.Point dragOffset;
		private FlowNode connectionSourceNode = null;
		private Windows.Foundation.Point currentMousePos;
		private bool isConnecting = false;
		private NodeRenderer renderer;
		private NodeConnection selectedConnection = null;
		private const int GridSize = 16; // Tamaño del grid en píxeles
		private readonly CanvasControl _canvas;

		// Eventos personalizados (equivalentes a Windows Forms)
		public event EventHandler<NodeCanvasMouseEventArgs> NodeMouseDown;
		public event EventHandler<NodeCanvasMouseEventArgs> NodeMouseMove;
		public event EventHandler<NodeCanvasMouseEventArgs> NodeMouseUp;
		public event EventHandler<NodeCanvasMouseEventArgs> NodeMouseDoubleClick;
		public event EventHandler<NodeCanvasKeyEventArgs> NodeKeyDown;
		public event EventHandler<NodeCanvasPaintEventArgs> NodePaint;
		public event EventHandler<ClickNodeEventArgs> ClickNode;
		public event EventHandler<NodeCanvasMouseEventArgs> ClickCanvas;

		// Estado del mouse
		private Vector2 _lastMousePosition;
		private bool _isMousePressed;

		// Propiedades públicas para configurar el canvas
		public Windows.UI.Color BackgroundColor
		{
			get => _canvas.ClearColor;
			set => _canvas.ClearColor = value;
		}

		public List<FlowNode> Nodes
		{
			get { return nodes; }
		}
		#endregion

		public NodeCanvas()
		{
			_canvas = new CanvasControl
			{
				ClearColor = Windows.UI.Color.FromArgb(255, 45, 46, 46)
			};

			// Establecer el canvas como contenido del UserControl
			this.Content = _canvas;

			renderer = new NodeRenderer();

			// Suscribirse a eventos nativos del CanvasControl
			_canvas.PointerPressed += OnPointerPressed;
			_canvas.PointerMoved += OnPointerMoved;
			_canvas.PointerReleased += OnPointerReleased;
			_canvas.DoubleTapped += OnDoubleTapped;
			_canvas.Draw += OnDraw;
			
			// Suscribirse a eventos del UserControl para capturar teclas
			this.KeyDown += OnKeyDown;
			this.PreviewKeyDown += OnPreviewKeyDown;

			// Habilitar eventos de teclado
			this.IsTabStop = true;
			this.UseSystemFocusVisuals = false;
			this.AllowFocusOnInteraction = true;
			this.TabNavigation = KeyboardNavigationMode.Local;

			// Habilitar eventos de mouse/pointer en el canvas
			_canvas.IsTapEnabled = true;
			_canvas.IsDoubleTapEnabled = true;
			_canvas.IsRightTapEnabled = true;

			// Capturar foco cuando se carga el control
			this.Loaded += (s, e) => this.Focus(FocusState.Programmatic);
		}

		#region Métodos
		public void AddCodeNode(string name, string description, Windows.Foundation.Point position, int iconIndex = -1)
		{
			var node = new CodeNode(name, description, SnapToGrid(position), iconIndex);
			nodes.Add(node);
			Refresh();
		}

		public void AddNode(string name, string description, Windows.Foundation.Point position, int iconIndex = -1)
		{
			var node = new FlowNode(name, description, SnapToGrid(position), iconIndex);
			nodes.Add(node);
			Refresh();
		}

		public void DeleteSelectedConnection()
		{
			// Buscar y eliminar la conexión
			if (selectedConnection != null && selectedConnection.SourceNode.Connections.Contains(selectedConnection.TargetNode))
			{
				selectedConnection.SourceNode.Connections.Remove(selectedConnection.TargetNode);
				selectedConnection = null;
				Refresh();
			}
		}

		private void DrawGrid(CanvasDrawingSession g)
		{
			using (var brush = new CanvasSolidColorBrush(g, Windows.UI.Color.FromArgb(128, 150, 150, 150)))
			{
				for (int x = 0; x < this.ActualWidth; x += GridSize)
				{
					for (int y = 0; y < this.ActualHeight; y += GridSize)
					{
						var rect = new Windows.Foundation.Rect(x, y, 1.5, 1.5);
						g.FillRectangle(rect, brush);
					}
				}
			}
		}

		private Windows.Foundation.Point SnapToGrid(Windows.Foundation.Point point)
		{
			int x = (int)Math.Round(point.X / (double)GridSize) * GridSize;
			int y = (int)Math.Round(point.Y / (double)GridSize) * GridSize;
			return new Windows.Foundation.Point(x, y);
		}

		private NodeMouseButton GetMouseButton(PointerPoint point)
		{
			if (point.Properties.IsLeftButtonPressed)
				return NodeMouseButton.Left;
			if (point.Properties.IsRightButtonPressed)
				return NodeMouseButton.Right;
			if (point.Properties.IsMiddleButtonPressed)
				return NodeMouseButton.Middle;

			return NodeMouseButton.None;
		}

		/// <summary>
		/// Forzar el redibujado del canvas (equivalente a Invalidate en Windows Forms)
		/// </summary>
		public void Refresh()
		{
			_canvas.Invalidate();
		}

		/// <summary>
		/// Obtener la posición actual del mouse
		/// </summary>
		public Vector2 GetMousePosition()
		{
			return _lastMousePosition;
		}

		/// <summary>
		/// Verificar si el mouse está presionado
		/// </summary>
		public bool IsMousePressed()
		{
			return _isMousePressed;
		}

		/// <summary>
		/// Acceso al CanvasControl interno (para configuraciones avanzadas)
		/// </summary>
		public CanvasControl Canvas => _canvas;

		#endregion

		#region Eventos

		private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			// Capturar el foco para recibir eventos de teclado
			this.Focus(FocusState.Programmatic);

			var point = e.GetCurrentPoint(_canvas);
			_lastMousePosition = point.Position.ToVector2();
			_isMousePressed = true;

			var button = GetMouseButton(point);
			var args = new NodeCanvasMouseEventArgs
			{
				Position = _lastMousePosition,
				Button = button,
				IsLeftButton = point.Properties.IsLeftButtonPressed,
				IsRightButton = point.Properties.IsRightButtonPressed,
				IsMiddleButton = point.Properties.IsMiddleButtonPressed,
				Delta = Vector2.Zero
			};

			bool nodeClicked = false;

			foreach (var node in nodes.AsEnumerable().Reverse())
			{
				if (node.GetBounds().Contains(new Windows.Foundation.Point(args.Position.X, args.Position.Y)))
				{
					selectedNode = node;
					selectedConnection = null; // Deseleccionar conexión
					nodeClicked = true;
					ClickNode?.Invoke(this, new ClickNodeEventArgs(node));

					if (args.Button == NodeMouseButton.Right)
					{
						connectionSourceNode = node;
						isConnecting = true;
						currentMousePos = new Windows.Foundation.Point(args.Position.X, args.Position.Y);
						return;
					}

					if (args.Button == NodeMouseButton.Left)
					{
						draggedNode = node;
						dragOffset = new Windows.Foundation.Point(args.Position.X - node.Position.X, args.Position.Y - node.Position.Y);
					}

					Refresh();
					return;
				}
			}

			// Si no se hizo clic en un nodo, verificar si se hizo clic en una conexión
			if (!nodeClicked && args.Button == NodeMouseButton.Left)
			{
				foreach (var node in nodes)
				{
					foreach (var connectedNode in node.Connections)
					{
						var connection = new NodeConnection(node, connectedNode);
						if (connection.IsNearConnection(new Windows.Foundation.Point(args.Position.X, args.Position.Y)))
						{
							selectedConnection = connection;
							selectedNode = null; // Deseleccionar nodo
							Refresh();
							return;
						}
					}
				}
			}

			// Si no se hizo clic en nada, deseleccionar todo
			selectedNode = null;
			selectedConnection = null;
			Refresh();
			ClickCanvas?.Invoke(this, args);
			NodeMouseDown?.Invoke(this, args);

			if (args.Handled)
			{
				e.Handled = true;
			}
		}

		private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(_canvas);
			var currentPosition = point.Position.ToVector2();
			var delta = currentPosition - _lastMousePosition;

			var button = GetMouseButton(point);
			var args = new NodeCanvasMouseEventArgs
			{
				Position = currentPosition,
				Button = button,
				IsLeftButton = point.Properties.IsLeftButtonPressed,
				IsRightButton = point.Properties.IsRightButtonPressed,
				IsMiddleButton = point.Properties.IsMiddleButtonPressed,
				Delta = delta
			};

			if (draggedNode != null && args.Button == NodeMouseButton.Left)
			{
				Windows.Foundation.Point newPosition = new Windows.Foundation.Point(args.Position.X - dragOffset._x, args.Position.Y - dragOffset._y);
				draggedNode.Position = SnapToGrid(newPosition);
				Refresh();
			}

			if (isConnecting)
			{
				currentMousePos = new Windows.Foundation.Point(args.Position.X, args.Position.Y);
				Refresh();
			}

			NodeMouseMove?.Invoke(this, args);

			_lastMousePosition = currentPosition;

			if (args.Handled)
			{
				e.Handled = true;
			}
		}

		private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(_canvas);
			_lastMousePosition = point.Position.ToVector2();
			_isMousePressed = false;

			var button = GetMouseButton(point);
			var args = new NodeCanvasMouseEventArgs
			{
				Position = _lastMousePosition,
				Button = button,
				IsLeftButton = false,
				IsRightButton = false,
				IsMiddleButton = false,
				Delta = Vector2.Zero
			};

			if (isConnecting && connectionSourceNode != null)
			{
				foreach (var node in nodes)
				{
					if (node != connectionSourceNode && node.GetBounds().Contains(new Windows.Foundation.Point(args.Position.X, args.Position.Y)))
					{
						if (!connectionSourceNode.Connections.Contains(node))
						{
							connectionSourceNode.Connections.Add(node);
						}
						break;
					}
				}

				isConnecting = false;
				connectionSourceNode = null;
				Refresh();
			}

			draggedNode = null;

			NodeMouseUp?.Invoke(this, args);

			if (args.Handled)
			{
				e.Handled = true;
			}
		}

		private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var position = e.GetPosition(_canvas).ToVector2();
			_lastMousePosition = position;

			var args = new NodeCanvasMouseEventArgs
			{
				Position = position,
				Button = NodeMouseButton.Left,
				IsLeftButton = true,
				IsRightButton = false,
				IsMiddleButton = false,
				Delta = Vector2.Zero
			};

			bool nodeClicked = false;

			foreach (var node in nodes.AsEnumerable().Reverse())
			{
				if (node.GetBounds().Contains(new Windows.Foundation.Point(args.Position.X, args.Position.Y)))
				{
					selectedNode = node;
					selectedConnection = null; // Deseleccionar conexión
					nodeClicked = true;

					ClickNode?.Invoke(this, new ClickNodeEventArgs(node));
					break;
				}
			}

			NodeMouseDoubleClick?.Invoke(this, args);

			if (args.Handled)
			{
				e.Handled = true;
			}
		}


		private void OnKeyDown(object sender, KeyRoutedEventArgs e)
		{
			base.OnKeyDown(e);
			var args = new NodeCanvasKeyEventArgs
			{
				Key = e.Key,
				IsCtrlPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down),
				IsShiftPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down),
				IsAltPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)
			};

			// Eliminar conexión seleccionada con la tecla Supr
			if (args.Key == VirtualKey.Delete)
			{
				DeleteSelectedConnection();
				e.Handled = true;
			}

			NodeKeyDown?.Invoke(this, args);

			if (args.Handled)
			{
				e.Handled = true;
			}
		}

		private void OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
		{
			// PreviewKeyDown se ejecuta antes que KeyDown
			// Útil para capturar teclas que normalmente son consumidas por el sistema
			// como Tab, Arrow keys, etc.

			// Evitar que ciertas teclas sean manejadas por el sistema
			if (e.Key == VirtualKey.Tab ||
				e.Key == VirtualKey.Up ||
				e.Key == VirtualKey.Down ||
				e.Key == VirtualKey.Left ||
				e.Key == VirtualKey.Right ||
				e.Key == VirtualKey.Delete ||
				e.Key == VirtualKey.Back)
			{
				e.Handled = false; // Permitir que llegue a KeyDown
			}
		}

		private void OnDraw(CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
		{
			var paintArgs = new NodeCanvasPaintEventArgs
			{
				DrawingSession = args.DrawingSession,
				CanvasSize = new Vector2((float)_canvas.ActualWidth, (float)_canvas.ActualHeight)
			};

			args.DrawingSession.Antialiasing = CanvasAntialiasing.Antialiased;

			// Dibujar la cuadrícula de fondo
			DrawGrid(args.DrawingSession);

			foreach (var node in nodes)
			{
				foreach (var connectedNode in node.Connections)
				{
					// Verificar si esta conexión está seleccionada
					bool isSelected = selectedConnection != null &&
									 selectedConnection.SourceNode == node &&
									 selectedConnection.TargetNode == connectedNode;

					renderer.DrawConnection(args.DrawingSession, node.GetOutputPoint(), connectedNode.GetInputPoint(), false, isSelected);
				}
			}

			if (isConnecting && connectionSourceNode != null)
			{
				renderer.DrawConnection(args.DrawingSession, connectionSourceNode.GetOutputPoint(), currentMousePos, true);
			}

			foreach (var node in nodes)
			{
				renderer.DrawNode(args.DrawingSession, node, node == selectedNode);//, NodeIcons);
			}

			NodePaint?.Invoke(this, paintArgs);
		}
		#endregion
	}

	#region Clases de argumentos de eventos
	public enum NodeMouseButton
	{
		None,
		Left,
		Right,
		Middle
	}

	public class NodeCanvasMouseEventArgs : EventArgs
	{
		public Vector2 Position { get; set; }
		public NodeMouseButton Button { get; set; }
		public bool IsLeftButton { get; set; }
		public bool IsRightButton { get; set; }
		public bool IsMiddleButton { get; set; }
		public Vector2 Delta { get; set; }
		public bool Handled { get; set; }
	}

	public class NodeCanvasKeyEventArgs : EventArgs
	{
		public VirtualKey Key { get; set; }
		public bool IsCtrlPressed { get; set; }
		public bool IsShiftPressed { get; set; }
		public bool IsAltPressed { get; set; }
		public bool Handled { get; set; }
	}

	public class NodeCanvasPaintEventArgs : EventArgs
	{
		public Microsoft.Graphics.Canvas.CanvasDrawingSession DrawingSession { get; set; }
		public Vector2 CanvasSize { get; set; }
	}
	#endregion
}