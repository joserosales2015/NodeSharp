using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Text;
using System;
using System.Numerics;
using Windows.UI;

namespace NodeSharp.Utils
{
    public static class GraphicsExtensions
	{
		public static void FillRoundedRectangle(this CanvasDrawingSession g, ICanvasBrush brush, float x, float y, float width, float height, float radius)
		{
             
			using (var geometry = CreateRoundedRectangle(g, x, y, width, height, radius))
			{
				g.FillGeometry(geometry, brush);
			}
		}

		public static void FillRoundedRectangle(this CanvasDrawingSession g, ICanvasBrush brush, Windows.Foundation.Rect rect, float radius)
		{

			using (var geometry = CreateRoundedRectangle(g, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, radius))
			{
				g.FillGeometry(geometry, brush);
			}
		}

		public static void FillPolygon(this CanvasDrawingSession g, ICanvasBrush brush, Vector2[] points)
		{
			if (points == null || points.Length < 3)
				throw new ArgumentException("Se necesitan al menos 3 puntos para formar un polígono");

			using (var pathBuilder = new CanvasPathBuilder(g.Device))
			{
				pathBuilder.BeginFigure(points[0]);

				for (int i = 1; i < points.Length; i++)
				{
					pathBuilder.AddLine(points[i]);
				}

				pathBuilder.EndFigure(CanvasFigureLoop.Closed);

				using (var geometry = CanvasGeometry.CreatePath(pathBuilder))
				{
					g.FillGeometry(geometry, brush);
				}
			}
		}

		public static void DrawRoundedRectangle(this CanvasDrawingSession g, ICanvasBrush brush, Windows.Foundation.Rect rect, float radius, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
		{
			using (var geometry = CreateRoundedRectangle(g, (float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, radius))
			{
				if (strokeStyle != null)
					g.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
				else
					g.DrawGeometry(geometry, brush, strokeWidth);				
			}
		}

		public static void DrawBezier(this CanvasDrawingSession g, ICanvasBrush brush, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
		{
			using (var pathBuilder = new CanvasPathBuilder(g.Device))
			{
				pathBuilder.BeginFigure(x1, y1);
				pathBuilder.AddCubicBezier(
					new Vector2(cx1, cy1),
					new Vector2(cx2, cy2),
					new Vector2(x2, y2)
				);
				pathBuilder.EndFigure(CanvasFigureLoop.Open);

				using (var geometry = CanvasGeometry.CreatePath(pathBuilder))
				{
					if (strokeStyle != null)
						g.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
					else
						g.DrawGeometry(geometry, brush, strokeWidth);
				}
			}
		}

		public static void DrawPolygon(this CanvasDrawingSession g, ICanvasBrush brush, Vector2[] points, float strokeWidth = 1f, CanvasStrokeStyle strokeStyle = null)
		{
			if (points == null || points.Length < 3)
				throw new ArgumentException("Se necesitan al menos 3 puntos para formar un polígono");

			using (var pathBuilder = new CanvasPathBuilder(g.Device))
			{
				pathBuilder.BeginFigure(points[0]);

				for (int i = 1; i < points.Length; i++)
				{
					pathBuilder.AddLine(points[i]);
				}

				pathBuilder.EndFigure(CanvasFigureLoop.Closed);

				using (var geometry = CanvasGeometry.CreatePath(pathBuilder))
				{
					if (strokeStyle != null)
						g.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
					else
						g.DrawGeometry(geometry, brush, strokeWidth);
				}
			}
		}

		public static void DrawIcon(this CanvasDrawingSession g, string glyphCode, float x, float y, float size, Color color)
		{
			var textFormat = new CanvasTextFormat
			{
				FontFamily = "Segoe MDL2 Assets",
				FontSize = size,
				FontWeight = FontWeights.ExtraLight,
				HorizontalAlignment = CanvasHorizontalAlignment.Left,
				VerticalAlignment = CanvasVerticalAlignment.Top
			};

			g.DrawText(glyphCode, new Vector2(x, y), color, textFormat);
		}

		public static void DrawTextIcon(this CanvasDrawingSession g, char character, float x, float y, float size, Color color)
		{
			string text = character.ToString();
			var textFormat = new CanvasTextFormat
			{
				FontFamily = "Segoe UI",
				FontSize = size,
				FontWeight = FontWeights.Bold,
				HorizontalAlignment = CanvasHorizontalAlignment.Left,
				VerticalAlignment = CanvasVerticalAlignment.Top
			};

			g.DrawText(text, new Vector2(x, y), color, textFormat);
		}

		private static CanvasGeometry CreateRoundedRectangle(CanvasDrawingSession g, float x, float y, float width, float height, float radius)
		{
			var rect = new Windows.Foundation.Rect(x, y, width, height);

			return CanvasGeometry.CreateRoundedRectangle(g.Device, rect, radius, radius);
		}
	}
}



