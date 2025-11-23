using Microsoft.UI.Xaml.Controls;
using NodeSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NodeSharp.Controls
{
	public sealed partial class MonacoEditor : UserControl
	{
		private bool _isEditorLoaded = false;

		public MonacoEditor()
		{
			this.InitializeComponent();
			EditorWebView = this.FindName("EditorWebView") as WebView2;
			InitializeAsync();
		}

		private async void InitializeAsync()
		{
			// 1. Inicializar entorno WebView2
			await EditorWebView.EnsureCoreWebView2Async();

			// 2. Mapear la carpeta local para que el HTML pueda cargar los scripts
			// IMPORTANTE: Asegúrate de que los archivos en Assets tengan "Build Action: Content"
			EditorWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
				"monaco-editor", "Assets/Monaco",
				Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

			// 3. Navegar al index.html usando el host virtual
			EditorWebView.Source = new Uri("http://monaco-editor/index.html");

			// 4. Esperar a que cargue la página
			EditorWebView.NavigationCompleted += (s, e) => _isEditorLoaded = true;
		}

		// --- Métodos Públicos para usar desde fuera ---

		public async Task SetCodeAsync(string code)
		{
			if (!_isEditorLoaded) return;

			// Serializamos el string a JSON para escapar caracteres especiales automáticamente
			string jsonCode = JsonSerializer.Serialize(code);
			await EditorWebView.ExecuteScriptAsync($"setCode({jsonCode});");
		}

		public async Task<string> GetCodeAsync()
		{
			if (!_isEditorLoaded) return "";

			// JS devuelve el string entre comillas, hay que limpiarlo
			string result = await EditorWebView.ExecuteScriptAsync("getCode();");
			return JsonSerializer.Deserialize<string>(result);
		}

		public async Task ShowErrorsAsync(List<EditorError> errors)
		{
			if (!_isEditorLoaded) return;

			string jsonErrors = JsonSerializer.Serialize(errors);
			// Pasamos el JSON como string al método JS
			// Note: Serializamos dos veces o escapamos para pasar el JSON string dentro de la llamada JS
			string safeJsonArgument = JsonSerializer.Serialize(jsonErrors);

			await EditorWebView.ExecuteScriptAsync($"setMarkers({safeJsonArgument});");
		}
	}
}
