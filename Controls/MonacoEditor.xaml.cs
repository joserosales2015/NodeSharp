using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using NodeSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Net; 
using NodeSharp.Services;

namespace NodeSharp.Controls
{
	public sealed partial class MonacoEditor : UserControl
	{
		private bool _isEditorLoaded = false;
		public event EventHandler<string> CodeChanged;
		private HttpListener _httpListener;

		public MonacoEditor()
		{
			this.InitializeComponent();
			EditorWebView = this.FindName("EditorWebView") as WebView2;
			InitializeAsync();
			this.Unloaded += MonacoEditor_Unloaded;
		}

	

		private void MonacoEditor_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			// Verificamos que el motor CoreWebView2 haya sido inicializado antes de intentar detenerlo.
			if (EditorWebView.CoreWebView2 != null)
			{
				try
				{
					// 1. Intenta detener el proceso de Chromium asociado.
					// Si el proceso ya no existe, lanzará la COMException, que capturaremos.
					EditorWebView.CoreWebView2.Stop();
				}
				catch (System.Runtime.InteropServices.COMException)
				{
					// 2. Ignoramos la excepción. 
					// Esto significa que el proceso ya fue liberado y no necesitamos hacer nada más.
				}
			}

			// 3. Buena práctica: Remueve el contenido del control.
			EditorWebView.Source = null;
		}

		private async void InitializeAsync()
		{
			// 1. Inicializar entorno WebView2
			await EditorWebView.EnsureCoreWebView2Async();

			// Suscripción al listener JS -> C#
			EditorWebView.WebMessageReceived += OnMonacoMessageReceived;

			// 2. Mapear la carpeta local para que el HTML pueda cargar los scripts
			// IMPORTANTE: Asegúrate de que los archivos en Assets tengan "Build Action: Content"
			EditorWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
				"monaco-editor", "Assets/Monaco",
				Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

			// 3. Navegar al index.html usando el host virtual
			EditorWebView.Source = new Uri("http://monaco-editor/index.html");

			// 4. Esperar a que cargue la página
			EditorWebView.NavigationCompleted += (s, e) =>
			{
				_isEditorLoaded = true;
				ApplyThemeBasedOnSystem(); // Llamada al método de configuración
			};

			StartSocketServer();
		}

		// --- Métodos Públicos para usar desde fuera ---
		private async void ApplyThemeBasedOnSystem()
		{
			// Obtener el tema actual de WinUI 3
			var winUITheme = Application.Current.RequestedTheme;

			// Mapear el tema de WinUI al tema de Monaco
			string monacoTheme = (winUITheme == ApplicationTheme.Dark) ? "vs-dark" : "vs";

			// Llamar a la función JS
			await EditorWebView.ExecuteScriptAsync($"setMonacoTheme('{monacoTheme}');");
		}

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

		private void OnMonacoMessageReceived(WebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
		{
			// El mensaje es el texto que Monaco nos envía (ej. "Texto actualizado")
			string updatedCode = args.WebMessageAsJson;

			// Desencapsulamos el JSON string que viene del JS
			try
			{
				var jsonDoc = JsonDocument.Parse(args.TryGetWebMessageAsString());
				var root = jsonDoc.RootElement;

				if (root.TryGetProperty("type", out var typeProp) &&
					typeProp.GetString() == "ui_codeChanged")
				{
					string content = root.GetProperty("content").GetString();

					// Disparamos el evento hacia afuera
					CodeChanged?.Invoke(this, content);
				}
			}
			catch 
			{
				/* Ignorar si el mensaje no es el código esperado */ 
			}
		}

		private void StartSocketServer()
		{
			Task.Run(async () =>
			{
				_httpListener = new HttpListener();
				_httpListener.Prefixes.Add("http://localhost:5000/"); // Puerto 5000
				_httpListener.Start();

				while (true)
				{
					var context = await _httpListener.GetContextAsync();
					if (context.Request.IsWebSocketRequest)
					{
						var wsContext = await context.AcceptWebSocketAsync(null);
						var server = new LanguageServer(wsContext.WebSocket);
						await server.StartListening(); // Esto bloquea el hilo por cliente, está bien en Task.Run
					}
				}
			});
		}
	}
}
