
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using Microsoft.UI.Text;
using NodeSharp.Models;
using NodeSharp.Utils;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using NodeSharp.Constants;

namespace NodeSharp.Rendering
{
	public class NodeRenderer
	{
		public void DrawNode(CanvasDrawingSession g, FlowNode node, bool isSelected)
		{
			var bounds = node.GetBounds();

			// Sombra
			if (node is MainNode)
			{
				using (var brush = new CanvasSolidColorBrush(g, Windows.UI.Color.FromArgb((byte)(isSelected ? 80 : 0), 195, 201, 213)))
				{
					g.FillRoundedTop(brush, bounds._x - 6, bounds._y - 6, bounds._width + 12, bounds._height + 12, 46f, 16f);
				}
			}
			else 
			{
				using (var shadowBrush = new CanvasSolidColorBrush(g, Windows.UI.Color.FromArgb((byte)(isSelected ? 80 : 0), 195, 201, 213)))
				{
					g.FillRoundedRectangle(shadowBrush, bounds._x - 6, bounds._y - 6, bounds._width + 12, bounds._height + 12, 16f);
				}
			}

			//Fondo del nodo
			if (node is MainNode)
			{
				using (var brush = new CanvasSolidColorBrush(g, Windows.UI.Color.FromArgb(255, 65, 66, 68)))
				{
					g.FillRoundedTop(brush, bounds._x, bounds._y, bounds._width, bounds._height, 40f, 10f);
				}
			}
			else
			{
				using (var brush = new CanvasSolidColorBrush(g, Windows.UI.Color.FromArgb(255, 65, 66, 68)))
				{
					g.FillRoundedRectangle(brush, bounds, 10f);
				}
			}

			// Borde
			var borderColor = Windows.UI.Color.FromArgb(255, 195, 201, 213);
			var borderWidth = 1f;

			if (node is MainNode)
			{
				using (var brush = new CanvasSolidColorBrush(g, Colors.DodgerBlue))
				{
					g.DrawRoundedTop(brush, bounds._x, bounds._y, bounds._width, bounds._height, 40f, 10f, 2);
				}
			}
			else 
			{
				using (var brush = new CanvasSolidColorBrush(g, borderColor))
				{
					g.DrawRoundedRectangle(brush, bounds, 10f, borderWidth);
				}
			}

			// Iconos
			if (isSelected && node is CodeNode) // Eliminar
				g.DrawIcon("\uE74D", bounds._x + bounds._width - 16, bounds._y + 6, 11, Colors.DarkGray);

			// TipoDato
			if (node is CodeNode)
			{
				g.DrawTextIcon(((CodeNode)node).TipoRetorno, bounds._x + 7, bounds._y + 4, 12, DataTypeColors.GetColorByTypeCode(((CodeNode)node).TipoRetorno));
			}
			else if (node is MainNode)
			{
				g.DrawTextIcon(((MainNode)node).TipoRetorno, bounds._x + 7, bounds._y + 4, 12, DataTypeColors.GetColorByTypeCode(((MainNode)node).TipoRetorno));
			}

			// Nombre del nodo
			g.DrawText(
				text: node.Name + "()",
				x: bounds._x,
				y: bounds._y,
				w: bounds._width,
				h: bounds._height / 3,
				color: node is MainNode ? Colors.Gold : Colors.White,
				
				new CanvasTextFormat()
				{
					FontFamily = "Consolas",
					FontSize = 14,
					FontWeight = FontWeights.Medium,
					HorizontalAlignment = CanvasHorizontalAlignment.Center,
					VerticalAlignment = CanvasVerticalAlignment.Center,
					WordWrapping = CanvasWordWrapping.NoWrap,
					TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
					TrimmingSign = CanvasTrimmingSign.Ellipsis					
				}
			);

			// Separador
			CanvasStrokeStyle canvasStrokeStyle = new CanvasStrokeStyle()
			{
				DashStyle = CanvasDashStyle.Dash
			};
			using (var brush = new CanvasSolidColorBrush(g, node is MainNode ? Colors.SteelBlue : Colors.DimGray))
			{
				g.DrawLine(
					new Vector2(bounds._x + 1, bounds._y + bounds._height / 3 + 1), 
					new Vector2(bounds._x + bounds._width - (node is MainNode ? 3 : 1), bounds._y + bounds._height / 3 + 1), 
					brush, 
					1f, 
					canvasStrokeStyle
				);
			}
			
			// Descripción del nodo
			g.DrawText(
				text: node.Summary,
				x: bounds._x + 1,
				y: bounds._y + bounds._height / 3,
				w: bounds._width - 2,
				h: bounds._height / 3 * 2 - 1,
				color: Windows.UI.Color.FromArgb(255, 148, 148, 148),
				new CanvasTextFormat()
				{
					FontFamily = "Segoe UI",
					FontSize = 11,
					FontWeight = FontWeights.Normal,
					FontStyle = Windows.UI.Text.FontStyle.Italic,
					HorizontalAlignment = CanvasHorizontalAlignment.Center,
					VerticalAlignment = CanvasVerticalAlignment.Center,
					TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
					TrimmingSign = CanvasTrimmingSign.Ellipsis
				}
			);

			// Puntos de conexión
			if (node is CodeNode)
			{
				DrawInputPoint(g, node.GetInputPoint(), Windows.UI.Color.FromArgb(255, 195, 201, 213));
			}
				
			DrawOutputPoint(g, node.GetOutputPoint(), Windows.UI.Color.FromArgb(255, 195, 201, 213));
		}

		private void DrawInputPoint(CanvasDrawingSession g, Windows.Foundation.Point point, Windows.UI.Color color)
		{
			using (var brush = new CanvasSolidColorBrush(g, color))
			{
				var rect = new Windows.Foundation.Rect(point.X - 5, point.Y - 3, 10, 5);
				g.FillRectangle(rect, brush);
				g.DrawRectangle(rect, brush, 2);
			}
		}

		private void DrawOutputPoint(CanvasDrawingSession g, Windows.Foundation.Point point, Windows.UI.Color color)
		{
			using (var brush = new CanvasSolidColorBrush(g, color))
			{
				g.FillEllipse(point._x, point._y, 5, 5, brush);
				g.DrawEllipse(point._x, point._y, 5, 5, brush);
			}
		}

		public void DrawConnection(CanvasDrawingSession g, Windows.Foundation.Point start, Windows.Foundation.Point end, bool isTemporary = false, bool isSelected = false)
		{
			Windows.UI.Color color;
			float width;
			CanvasStrokeStyle? canvasStrokeStyle = null;

			if (isTemporary)
			{
				color = Colors.Orange; //Windows.UI.Color.FromArgb(255, 255, 140, 0);
				width = 2f;
			}
			else if (isSelected)
			{
				color = Colors.Gold; // Naranja para conexión seleccionada
				canvasStrokeStyle = new CanvasStrokeStyle()
				{
					DashStyle = CanvasDashStyle.Dash
				};
				width = 2f;
			}
			else
			{
				color = Windows.UI.Color.FromArgb(255, 195, 201, 213);
				width = 1.5f;
			}

			using (var brush = new CanvasSolidColorBrush(g, color))
			{
				var controlOffset = Math.Abs(end.Y - start.Y) / 2;
				var cp1 = new Windows.Foundation.Point(start.X, start.Y + controlOffset);
				var cp2 = new Windows.Foundation.Point(end.X, end.Y - controlOffset);

				g.DrawBezier(brush, start._x, start._y, cp1._x, cp1._y, cp2._x, cp2._y, end._x, end._y - 10, width, canvasStrokeStyle);
			}

			if (!isTemporary)
			{
				DrawArrow(g, end, color);
			}
		}

		private void DrawArrow(CanvasDrawingSession g, Windows.Foundation.Point point, Windows.UI.Color color)
		{
			using (var brush = new CanvasSolidColorBrush(g, color))
			{
				var arrowPoints = new Vector2[]
				{
					new Vector2(point._x, point._y - 4),
					new Vector2(point._x - 5, point._y - 10),
					new Vector2(point._x + 5, point._y - 10)
				};

				g.FillPolygon(brush, arrowPoints);
			}
		}
	}
}