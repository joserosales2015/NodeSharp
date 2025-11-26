
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using NodeSharp.Models;
using NodeSharp.Utils;

namespace NodeSharp.Rendering
{
	public class NodeRenderer
	{
		public void DrawNode(Graphics g, FlowNode node, bool isSelected, ImageList iconList = null)
		{
			var bounds = node.GetBounds();

			// Sombra
			using (var shadowBrush = new SolidBrush(Color.FromArgb(isSelected ? 80 : 0, 195, 201, 213)))
			{
				g.FillRoundedRectangle(shadowBrush, bounds.X - 6, bounds.Y - 6, bounds.Width + 12, bounds.Height + 12, 18);
			}

			// Fondo del nodo
			using (var brush = new SolidBrush(Color.FromArgb(65, 66, 68)))
			{
				g.FillRoundedRectangle(brush, bounds, 10);
			}

			// Borde
			var borderColor = Color.FromArgb(195, 201, 213);
			var borderWidth = 1f;

			using (var pen = new Pen(borderColor, borderWidth))
			{
				g.DrawRoundedRectangle(pen, bounds, 10);
			}

			// Dibujar icono si existe
			if (iconList != null && node.IconIndex >= 0 && node.IconIndex < iconList.Images.Count)
			{
				var iconSize = 32; // Tamaño del icono
				var iconX = bounds.X + 15; // Margen izquierdo
				var iconY = bounds.Y + (bounds.Height - iconSize) / 2; // Centrado verticalmente

				g.DrawImage(iconList.Images[node.IconIndex], iconX, iconY, iconSize, iconSize);
			}

			// Nombre del nodo
			using (var font = new Font("Segoe UI", 11, FontStyle.Bold))
			using (var brush = new SolidBrush(Color.White))
			{
				var format = new StringFormat
				{
					Alignment = StringAlignment.Near,
					LineAlignment = StringAlignment.Near
				};

				var textBounds = new RectangleF(bounds.X + 64, bounds.Y + 6, bounds.Width - 64 - 4, bounds.Height / 3);

				g.DrawString(node.Name, font, brush, textBounds, format);
			}

			using (var font = new Font("Segoe UI", 7, FontStyle.Regular))
			using (var brush = new SolidBrush(Color.SkyBlue))
			{
				var format = new StringFormat
				{
					Alignment = StringAlignment.Near,
					LineAlignment = StringAlignment.Near
				};

				var boundsDescription = new RectangleF(bounds.X + 64, bounds.Y + bounds.Height / 3 + 8, bounds.Width - 64 - 4, bounds.Height / 3 * 2 - 12);

				g.DrawString(node.Summary, font, brush, boundsDescription, format);
			}

			// Puntos de conexión
			DrawInputPoint(g, node.GetInputPoint(), Color.FromArgb(195, 201, 213));
			DrawOutputPoint(g, node.GetOutputPoint(), Color.FromArgb(195, 201, 213));
		}

		private void DrawInputPoint(Graphics g, Point point, Color color)
		{
			using (var brush = new SolidBrush(color))
			using (var pen = new Pen(color, 2))
			{
				g.FillRectangle(brush, point.X - 5, point.Y - 3, 10, 5);
				g.DrawRectangle(pen, point.X - 5, point.Y - 3, 10, 5);
			}
		}

		private void DrawOutputPoint(Graphics g, Point point, Color color)
		{
			using (var brush = new SolidBrush(color))
			using (var pen = new Pen(color, 2))
			{
				g.FillEllipse(brush, point.X - 5, point.Y - 5, 10, 10);
				g.DrawEllipse(pen, point.X - 5, point.Y - 5, 10, 10);
			}
		}

		public void DrawConnection(Graphics g, Point start, Point end, bool isTemporary = false, bool isSelected = false)
		{
			Color color;
			float width;

			if (isTemporary)
			{
				color = Color.FromArgb(255, 140, 0);
				width = 2f;
			}
			else if (isSelected)
			{
				color = Color.FromArgb(255, 140, 0); // Naranja para conexión seleccionada
				width = 4f;
			}
			else
			{
				color = Color.FromArgb(195, 201, 213);
				width = 1.5f;
			}

			using (var pen = new Pen(color, width))
			{
				var controlOffset = Math.Abs(end.Y - start.Y) / 2;
				var cp1 = new Point(start.X, start.Y + controlOffset);
				var cp2 = new Point(end.X, end.Y - controlOffset);

				g.DrawBezier(pen, start, cp1, cp2, new Point(end.X, end.Y - 10));
			}

			if (!isTemporary)
			{
				DrawArrow(g, end, isSelected ? Color.FromArgb(255, 140, 0) : color);
			}
		}

		private void DrawArrow(Graphics g, Point point, Color color)
		{
			using (var brush = new SolidBrush(color))
			{
				var arrowPoints = new Point[]
				{
					new Point(point.X, point.Y - 4),
					new Point(point.X - 5, point.Y - 10),
					new Point(point.X + 5, point.Y - 10)
				};
				g.FillPolygon(brush, arrowPoints);
			}
		}
	}
}