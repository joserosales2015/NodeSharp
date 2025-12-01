using Windows.UI;

namespace NodeSharp.Constants
{
	public static class DataTypeColors
	{
		// Tipos Numéricos Enteros (Azules)
		public static readonly Color Byte = Color.FromArgb(255, 0, 170, 255);       // #00AAFF
		public static readonly Color Integer = Color.FromArgb(255, 0, 170, 255);    // #00AAFF
		public static readonly Color Long = Color.FromArgb(255, 0, 170, 255);       // #00AAFF

		// Tipos Numéricos Decimales (Verdes)
		public static readonly Color Decimal = Color.FromArgb(255, 47, 237, 102);  // #2FED66
		public static readonly Color Float = Color.FromArgb(255, 47, 237, 102);    // #2FED66
		public static readonly Color Double = Color.FromArgb(255, 47, 237, 102);   // #2FED66

		// Tipos de Texto (Naranjas/Amarillos)
		public static readonly Color String = Color.FromArgb(255, 255, 183, 77);    // #FFB74D
		public static readonly Color Char = Color.FromArgb(255, 255, 183, 77);      // #FFB74D

		// Tipos Lógicos/Especiales
		public static readonly Color Boolean = Color.FromArgb(255, 243, 91, 255);	// #FF00FF
		public static readonly Color DateTime = Color.FromArgb(255, 237, 73, 76);   // #ED494C
		public static readonly Color Void = Color.FromArgb(255, 114, 191, 209);     // #78909C
		public static readonly Color Object = Color.FromArgb(255, 255, 235, 59);    // #FFEB3B

		// Método helper para obtener color por nombre de tipo
		public static Color GetColorByTypeCode(string typeCode)
		{
			return typeCode switch
			{
				"BY" => Byte,
				"I" => Integer,
				"L" => Long,
				"D" => Decimal,
				"F" => Float,
				"DB" => Double,
				"S" => String,
				"C" => Char,
				"B" => Boolean,
				"DT" => DateTime,
				"V" => Void,
				"O" => Object,
				_ => Color.FromArgb(255, 255, 255, 255) // Blanco por defecto
			};
		}

		// Versiones en formato Hex string (útil para XAML)
		public static class Hex
		{
			public const string Byte = "#4A9EFF";
			public const string Integer = "#4A9EFF";
			public const string Long = "#4A9EFF";
			public const string Decimal = "#4CAF50";
			public const string Float = "#66BB6A";
			public const string Double = "#81C784";
			public const string String = "#FFA726";
			public const string Char = "#FFB74D";
			public const string Boolean = "#6A5ACD";
			public const string DateTime = "#EC407A";
			public const string Void = "#78909C";
			public const string Object = "#FFEB3B";

			public static string GetHexByTypeName(string typeName)
			{
				return typeName switch
				{
					"BY" => Byte,
					"I" => Integer,
					"L" => Long,
					"D" => Decimal,
					"F" => Float,
					"DB" => Double,
					"S" => String,
					"C" => Char,
					"B" => Boolean,
					"DT" => DateTime,
					"V" => Void,
					"O" => Object,
					_ => "#FFFFFF"
				};
			}
		}
	}
}

// Ejemplo de uso:
// var color = DataTypeColors.GetColorByTypeName("Integer");
// var hexColor = DataTypeColors.Hex.GetHexByTypeName("String");