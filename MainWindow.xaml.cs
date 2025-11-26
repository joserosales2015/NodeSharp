using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using NodeSharp.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

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
			
			MainGrid.IsTabStop = true;
			MainGrid.Focus(FocusState.Programmatic);
		}
		#region Metodos

		private void UpdateColumnWidth()
		{
			double halfWidth = MainGrid.ActualWidth / 2.0;
			MainGrid.ColumnDefinitions[2].Width = new GridLength(halfWidth);
		}
		#endregion

		#region Eventos
		private void MainGrid_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateColumnWidth();
			
			MyNodeCanvas.AddNode("InicioProceso", "Inicia el flujo del proceso de Suma", new Windows.Foundation.Point(96, 96), 0);
			MyNodeCanvas.AddNode("LeerDatos", "Solicita al usuario los datos de entrada", new Windows.Foundation.Point(96, 208), 0);
			MyNodeCanvas.AddNode("HacerOperacion", "Realiza la operación de suma", new Windows.Foundation.Point(96, 320), 0);
			MyNodeCanvas.AddNode("MostrarResultado", "Muestra los resultados en pantalla", new Windows.Foundation.Point(96, 432), 0);
		}

		private void MainGrid_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			// Verificar teclas modificadoras
			bool isCtrlPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control)
				.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
			bool isShiftPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift)
				.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
			bool isAltPressed = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu)
				.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

			// Ctrl + S: Guardar
			if (e.Key == VirtualKey.S && isCtrlPressed)
			{
				System.Diagnostics.Debug.WriteLine($"Guardar");
				e.Handled = true;
				return;
			}

			// Delete: Eliminar selección
			if (e.Key == VirtualKey.Delete)
			{
				MyNodeCanvas.DeleteSelectedConnection();
				e.Handled = true;
				return;
			}
		}
		
		private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateColumnWidth();
		}

		private void MyNodeCanvas_ClickNode(object? sender, Classes.ClickNodeEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"Nodo clickeado: {e.Node.Name}");
		}

		private void TxtDescripcionNodo_LostFocus(object sender, RoutedEventArgs e)
		{
			TxtDescripcionNodo.Text = TxtDescripcionNodo.Text.Trim();
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
