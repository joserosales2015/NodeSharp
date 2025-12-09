using NodeSharp.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeSharp.Models
{
	public class MainNode : FlowNode
	{
		#region Variables y Propiedades
		public string CodigoMetodo { get; set; }
		public string TipoRetorno { get; set; }
		public List<ParametroMetodo> Parametros { get; set; }
		#endregion

		public MainNode(string name, string description, Windows.Foundation.Point position, int iconIndex = -1) : base(name, description, position, iconIndex)
		{
			Size = new Size(192, 64);
			Parametros = new List<ParametroMetodo>();
			TipoRetorno = "V"; //Por defecto Void
			CodigoMetodo = @"
using Microsoft.VisualBasic;
using System.Windows.Forms;
using System;

namespace NodeSharp
{
    public static class Calcular
    {
        public static void Suma()
        {
			//Declaracion de variables


			// --------------------------------------------------------------
			// Código generado automáticamente de aquí en adelante
			// No modifique este código manualmente

		}
	}
}";
		}

		#region Metodos
		// Genera el método completo con firma
		public string GenerarMetodoCompleto()
		{
			var parametrosStr = string.Join(", ", Parametros.Select(p => $"{p.Tipo} {p.Nombre}"));

			string script =
				$@"public {TipoRetorno} {Name}({parametrosStr})" +
				$@"{{" +
				$@"\t{CodigoMetodo}" +
				$@"}}";

			return script;
		}
		#endregion
	}
}

