using NodeSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeSharp.Models
{
	public class CodeNode : FlowNode
	{
		#region Variables y Propiedades
		public string CodigoMetodo { get; set; }
		public string TipoRetorno { get; set; }
		public List<ParametroMetodo> Parametros { get; set; }
		#endregion

		public CodeNode(string name, string description, Windows.Foundation.Point position, int iconIndex = -1) : base(name, description, position, iconIndex)
		{
			Parametros = new List<ParametroMetodo>();
			TipoRetorno = "V"; //Por defecto Void
			CodigoMetodo = "// Escribe tu código aquí";
		}

		#region Metodos
		// Genera el método completo con firma
		public string GenerarMetodoCompleto()
		{
			var parametrosStr = string.Join(", ", Parametros.Select(p => $"{p.Tipo} {p.Nombre}"));

			string script =
				$@"private {TipoRetorno} {Name}({parametrosStr})" +
				$@"{{" +
				$@"\t{CodigoMetodo}" +
				$@"}}";

			return script;
		}
		#endregion
	}
}

