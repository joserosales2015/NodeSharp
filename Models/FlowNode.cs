using System;
using System.Collections.Generic;
using System.Drawing;

namespace NodeSharp.Models
{
	public class FlowNode
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Summary { get; set; }
		public Point Position { get; set; }
		public Size Size { get; set; }
		public List<FlowNode> Connections { get; set; }
		public Dictionary<string, object> Properties { get; set; }
		public int IconIndex { get; set; }

		public FlowNode(string name, string description, Point position, int iconIndex = -1)
		{
			Id = Guid.NewGuid().ToString();
			Name = name;
			Summary = description;
			Position = position;
			Size = new Size(192, 64);
			Connections = new List<FlowNode>();
			Properties = new Dictionary<string, object>();
			IconIndex = iconIndex;
		}

		public Rectangle GetBounds()
		{
			return new Rectangle(Position, Size);
		}

		public Point GetOutputPoint()
		{
			return new Point(Position.X + Size.Width / 2, Position.Y + Size.Height);
		}

		public Point GetInputPoint()
		{
			return new Point(Position.X + Size.Width / 2, Position.Y);
		}
	}

	public class NodeConnection
	{
		public FlowNode SourceNode { get; set; }
		public FlowNode TargetNode { get; set; }

		public NodeConnection(FlowNode source, FlowNode target)
		{
			SourceNode = source;
			TargetNode = target;
		}

		// Verifica si un punto está cerca de la línea de conexión
		public bool IsNearConnection(Point point, float threshold = 10f)
		{
			var start = SourceNode.GetOutputPoint();
			var end = TargetNode.GetInputPoint();

			// Calcular distancia del punto a la curva Bézier
			var controlOffset = Math.Abs(end.Y - start.Y) / 2;
			var cp1 = new PointF(start.X, start.Y + controlOffset);
			var cp2 = new PointF(end.X, end.Y - controlOffset);

			// Aproximación: verificar múltiples puntos en la curva
			for (float t = 0; t <= 1; t += 0.05f)
			{
				var curvePoint = GetBezierPoint(start, cp1, cp2, end, t);
				var distance = Math.Sqrt(Math.Pow(point.X - curvePoint.X, 2) + Math.Pow(point.Y - curvePoint.Y, 2));

				if (distance <= threshold)
					return true;
			}

			return false;
		}

		private PointF GetBezierPoint(PointF p0, PointF p1, PointF p2, PointF p3, float t)
		{
			float u = 1 - t;
			float tt = t * t;
			float uu = u * u;
			float uuu = uu * u;
			float ttt = tt * t;

			PointF point = new PointF();
			point.X = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X;
			point.Y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y;

			return point;
		}
	}
}