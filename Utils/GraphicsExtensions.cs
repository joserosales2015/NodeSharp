using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
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

		private static CanvasGeometry CreateRoundedRectangle(CanvasDrawingSession g, float x, float y, float width, float height, float radius)
		{
			var rect = new Windows.Foundation.Rect(x, y, width, height);

			return CanvasGeometry.CreateRoundedRectangle(g.Device, rect, radius, radius);
		}
	}
}



