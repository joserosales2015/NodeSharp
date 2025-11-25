using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NodeSharp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
		#region Variables y Propiedades

		#endregion
		public MainWindow()
        {
            InitializeComponent();
		}


		#region Metodos
	
		#endregion

		#region Eventos
		private void MainGrid_Loaded(object sender, RoutedEventArgs e)
		{

		}

		private void TxtDescripcionNodo_LostFocus(object sender, RoutedEventArgs e)
		{
			TxtDescripcionNodo.Text = TxtDescripcionNodo.Text.Trim();
			System.Diagnostics.Debug.WriteLine("Perdio enfoque");

		}
		private void BtnCargarCodigo_Click(object sender, RoutedEventArgs e)
		{
			TxtEditorCodigo.SetCodeAsync(@"public class Main {
    
    public Main() {
        Concatenar(""a"", ""b"");
        Sumar(2, 3);
    }

	/// <summary>
    /// Concatena dos strings en una sola.
    /// </summary>
    private string Concatenar (string s1, string s2) {
		string res = string.Empty;
		if (s1 != null && s2 != null)
		{
			res = s1 + s2;
			return res;
		}
		else
		{
			return ""error"";
		}
	}

    public int Sumar (int a, int b)
    {
        return a+b;
    }
}");
		}

		#endregion

	}
}
