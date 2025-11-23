using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeSharp.Models
{
	public class EditorError
	{
		public int Line { get; set; }
		public int Column { get; set; }
		public int Length { get; set; }
		public string Message { get; set; }
	}
}