
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
			using (var shadowBrush = new CanvasSolidColorBrush(g, Windows.UI.Color.FromArgb((byte)(isSelected ? 80 : 0), 195, 201, 213)))
			{
				g.FillRoundedRectangle(shadowBrush, bounds._x - 6, bounds._y - 6, bounds._width + 12, bounds._height + 12, 16f);
			}

			// Fondo del nodo
			using (var brush = new CanvasSolidColorBrush(g, Windows.UI.Color.FromArgb(255, 65, 66, 68)))
			{
				g.FillRoundedRectangle(brush, bounds, 10f);
			}

			// Borde
			var borderColor = Windows.UI.Color.FromArgb(255, 195, 201, 213);
			var borderWidth = 1f;

			using (var brush = new CanvasSolidColorBrush(g, borderColor))
			{
				g.DrawRoundedRectangle(brush, bounds, 10f, borderWidth);
			}

			// Iconos
			if (isSelected) // Eliminar
				g.DrawIcon("\uE74D", bounds._x + bounds._width - 16, bounds._y + 6, 11, Colors.DarkGray);

			// TipoDato
			if (node is CodeNode)
			{
				g.DrawTextIcon(((CodeNode)node).TipoRetorno, bounds._x + 8, bounds._y + 8, 12, DataTypeColors.GetColorByTypeCode(((CodeNode)node).TipoRetorno));
			}
			
			// Nombre del nodo
			g.DrawText(
				text: node.Name,
				x: bounds._x + (node is CodeNode ? 25 : 8),
				y: bounds._y + 6,
				w: bounds._width - 10,
				h: bounds._height / 3,
				color: Colors.White,
				new CanvasTextFormat()
				{
					FontFamily = "Consolas",
					FontSize = 14,
					FontWeight = FontWeights.Medium,
					HorizontalAlignment = CanvasHorizontalAlignment.Left,
					VerticalAlignment = CanvasVerticalAlignment.Top,
					WordWrapping = CanvasWordWrapping.NoWrap,
					TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
					TrimmingSign = CanvasTrimmingSign.Ellipsis
				}
			);

			// Descripción del nodo
			g.DrawText(
				text: node.Summary,
				x: bounds._x + 8,
				y: bounds._y + bounds._height / 3 + 4,
				w: bounds._width - 10,
				h: bounds._height / 3 * 2 - 12,
				color: Windows.UI.Color.FromArgb(255, 148, 148, 148),
				new CanvasTextFormat()
				{
					FontFamily = "Segoe UI",
					FontSize = 11,
					FontWeight = FontWeights.Normal,
					FontStyle = Windows.UI.Text.FontStyle.Italic,
					HorizontalAlignment = CanvasHorizontalAlignment.Left,
					VerticalAlignment = CanvasVerticalAlignment.Top,
					TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
					TrimmingSign = CanvasTrimmingSign.Ellipsis
				}
			);

			// Puntos de conexión
			DrawInputPoint(g, node.GetInputPoint(), Windows.UI.Color.FromArgb(255, 195, 201, 213));
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