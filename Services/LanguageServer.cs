// Archivo: Services/LanguageServer.cs
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.CodeAnalysis.SignatureHelp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.FindSymbols; // Necesario para SymbolFinder.FindSymbolAtPositionAsync

namespace NodeSharp.Services // Asegúrate que este namespace coincida con tu proyecto
{
	public class LanguageServer
	{
		private readonly WebSocket _socket;
		private readonly AdhocWorkspace _workspace;
		private readonly ProjectId _projectId;
		private readonly DocumentId _documentId;

		public LanguageServer(WebSocket socket)
		{
			_socket = socket;
			_workspace = new AdhocWorkspace();

			// 1. Configuración inicial de Roslyn
			_projectId = ProjectId.CreateNewId();

			var allReferences = GetAllReferences();

			var projectInfo = ProjectInfo.Create(
				_projectId, 
				VersionStamp.Create(), 
				"MiProyectoVirtual", 
				"MiProyectoVirtual", 
				LanguageNames.CSharp
			).WithMetadataReferences(allReferences);

			_workspace.AddProject(projectInfo);

			// Creamos un documento vacío inicial
			var doc = _workspace.AddDocument(_projectId, "Program.cs", SourceText.From(""));
			_documentId = doc.Id;
		}

		private IEnumerable<MetadataReference> GetAllReferences()
		{
			// Obtenemos la ruta de todas las DLLs que está usando tu aplicación (.NET Runtime)
			var trustedAssembliesPaths = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");

			var references = new List<MetadataReference>();

			if (trustedAssembliesPaths != null)
			{
				var paths = trustedAssembliesPaths.Split(Path.PathSeparator);

				foreach (var path in paths)
				{
					// Filtramos para cargar solo DLLs (evitamos .exe o archivos nativos)
					// y nos aseguramos de que el archivo exista.
					if (Path.GetExtension(path) == ".dll" && File.Exists(path))
					{
						try
						{
							// Creamos la referencia para Roslyn
							references.Add(MetadataReference.CreateFromFile(path));
						}
						catch
						{
							// Si alguna DLL falla (por permisos o formato), la ignoramos y seguimos
						}
					}
				}
			}

			return references;
		}

		// Bucle principal que escucha mensajes desde Javascript
		public async Task StartListening()
		{
			var buffer = new byte[1024 * 4];
			while (_socket.State == WebSocketState.Open)
			{
				var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

				if (result.MessageType == WebSocketMessageType.Close) break;

				string jsonReceived = Encoding.UTF8.GetString(buffer, 0, result.Count);
				await ProcessMessage(jsonReceived);
			}
		}

		private async Task ProcessMessage(string json)
		{
			// Parseamos el mensaje simple que enviaremos desde JS
			// Formato esperado: { "type": "completion", "code": "...", "position": 123 }
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;
			string type = root.GetProperty("type").GetString();

			if (type == "update")
			{
				// Solo actualizamos el código en memoria
				string code = root.GetProperty("code").GetString();
				UpdateCode(code);

				await PushDiagnostics();
			}
			else if (type == "completion")
			{
				string code = root.GetProperty("code").GetString();
				int position = root.GetProperty("position").GetInt32();

				UpdateCode(code);

				var suggestions = await GetCompletions(position);

				// Enviamos respuesta al JS
				string response = JsonSerializer.Serialize(new { type = "completion_response", data = suggestions });
				await SendString(response);
			}
			else if (type == "signature")
			{
				string code = root.GetProperty("code").GetString();
				UpdateCode(code); // Importante actualizar antes de pedir firma
				int position = root.GetProperty("position").GetInt32();

				var signatures = await GetSignatureHelp(position);
				string response = JsonSerializer.Serialize(new { type = "signature_response", data = signatures });
				await SendString(response);
			}
			else if (type == "hover")
			{
				string code = root.GetProperty("code").GetString();
				UpdateCode(code);

				int requestId = root.GetProperty("requestId").GetInt32();
				int position = root.GetProperty("position").GetInt32();
				var hoverData = await GetHoverInfo(position);

				string response = JsonSerializer.Serialize(new 
				{ 
					type = "hover_response", 
					data = hoverData,
					requestId = requestId
				});

				await SendString(response);
			}
		}

		private void UpdateCode(string code)
		{
			var sourceText = SourceText.From(code);
			var doc = _workspace.CurrentSolution.GetDocument(_documentId);
			var newSolution = doc.WithText(sourceText).Project.Solution;
			_workspace.TryApplyChanges(newSolution);
		}

		private async Task<string[]> GetCompletions(int position)
		{
			// Aquí usamos Roslyn para obtener inteligencia real
			var document = _workspace.CurrentSolution.GetDocument(_documentId);
			var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(document);

			if (service == null) return Array.Empty<string>();

			var results = await service.GetCompletionsAsync(document, position);

			// Devolvemos solo los nombres de las sugerencias (para simplificar)
			return results.ItemsList.Select(x => x.DisplayText).ToArray();
		}

		private async Task<object> GetSignatureHelp(int position)
		{
			var document = _workspace.CurrentSolution.GetDocument(_documentId);
			var tree = await document.GetSyntaxTreeAsync();
			var root = await tree.GetRootAsync();
			var semanticModel = await document.GetSemanticModelAsync();

			// 1. Encontrar el nodo donde estamos escribiendo
			// Buscamos el token a la izquierda del cursor (probablemente '(' o ',')
			var token = root.FindToken(position - 1);

			// 2. Buscar hacia arriba el "InvocationExpression" (llamada a método) 
			// o "ObjectCreationExpression" (new Clase(...)) más cercano.
			var node = token.Parent;
			while (node != null &&
				   !(node is InvocationExpressionSyntax) &&
				   !(node is ObjectCreationExpressionSyntax) &&
				   !(node is ConstructorInitializerSyntax))
			{
				node = node.Parent;
			}

			if (node == null) return null;

			// 3. Obtener información del símbolo (el método que se intenta llamar)
			// GetSymbolInfo devuelve el símbolo si es exacto, o "CandidateSymbols" si hay sobrecargas
			var symbolInfo = semanticModel.GetSymbolInfo(node);

			var methods = new List<IMethodSymbol>();

			if (symbolInfo.Symbol is IMethodSymbol exactMethod)
			{
				methods.Add(exactMethod);
			}
			else if (symbolInfo.CandidateSymbols.Any())
			{
				methods.AddRange(symbolInfo.CandidateSymbols.OfType<IMethodSymbol>());
			}

			if (!methods.Any()) return null;

			// 4. Determinar qué parámetro estamos escribiendo (índice activo)
			int activeParameterIndex = 0;
			if (node is InvocationExpressionSyntax invocation)
			{
				var args = invocation.ArgumentList;
				var spanStart = args.OpenParenToken.SpanStart;

				if (position > spanStart)
				{
					var sourceText = await document.GetTextAsync();

					// 2. Definir el RANGO (TextSpan) a extraer
					var length = position - spanStart;
					var textSpanToAnalyze = TextSpan.FromBounds(spanStart, position);

					// 3. Obtener el subtexto usando la sobrecarga correcta: ToString(TextSpan span)
					var textInParens = sourceText.ToString(textSpanToAnalyze);

					// Contamos cuántas comas hay en el texto escrito entre '(' y el cursor
					activeParameterIndex = textInParens.Count(c => c == ',');
				}
			}
			// (Se podría agregar lógica similar para ObjectCreationExpression)

			// 5. Formatear la respuesta para Monaco
			var signaturesList = methods.Select(m => new
			{
				// Etiqueta: "void WriteLine(string value)"
				label = m.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),

				// Documentación XML (si existe)
				documentation = m.GetDocumentationCommentXml(),

				// Parámetros
				parameters = m.Parameters.Select(p => new
				{
					label = p.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
					documentation = p.GetDocumentationCommentXml()
				}).ToArray()
			}).ToArray();

			return new
			{
				signatures = signaturesList,
				activeSignature = 0, // Por defecto mostramos la primera sobrecarga
				activeParameter = activeParameterIndex
			};
		}

		private async Task<object> GetHoverInfo(int position)
		{
			var document = _workspace.CurrentSolution.GetDocument(_documentId);

			// Buscar el símbolo (variable, método, clase) en la posición dada
			var symbol = await SymbolFinder.FindSymbolAtPositionAsync(document, position);

			if (symbol == null) return null;

			// Obtener la firma del símbolo (ej: "class System.Console" o "int x")
			var signature = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

			// Obtener la documentación XML asociada (si existe, ej. documentacion de .NET)
			var documentation = symbol.GetDocumentationCommentXml();

			return new
			{
				signature = signature,
				documentation = documentation
			};
		}

		private async Task SendString(string data)
		{
			var bytes = Encoding.UTF8.GetBytes(data);
			await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
		}

		private async Task PushDiagnostics()
		{
			var document = _workspace.CurrentSolution.GetDocument(_documentId);

			// Obtenemos el Modelo Semántico (es el que entiende el significado y errores)
			var semanticModel = await document.GetSemanticModelAsync();

			// Pedimos todos los diagnósticos (Errores y Advertencias)
			var diagnostics = semanticModel.GetDiagnostics();

			// Filtramos y transformamos a un objeto simple para enviar a JS
			var simplifiedErrors = diagnostics
				.Where(d => d.Severity != DiagnosticSeverity.Hidden) // Solo enviamos errores rojos por ahora
				.Select(d => {
					var lineSpan = d.Location.GetLineSpan();

					// Mapeo manual de severidad de Roslyn a Monaco
					int monacoSeverity = 8; // Default Error

					switch (d.Severity)
					{
						case DiagnosticSeverity.Error: monacoSeverity = 8; break;
						case DiagnosticSeverity.Warning: monacoSeverity = 4; break;
						case DiagnosticSeverity.Info: monacoSeverity = 2; break;
					}

					return new
					{
						Message = d.GetMessage(),
						// Importante: Roslyn usa índice 0, Monaco usa índice 1. Sumamos 1.
						StartLine = lineSpan.StartLinePosition.Line + 1,
						StartColumn = lineSpan.StartLinePosition.Character + 1,
						EndLine = lineSpan.EndLinePosition.Line + 1,
						EndColumn = lineSpan.EndLinePosition.Character + 1,
						Severity = monacoSeverity // Enviamos el nivel correcto
					};
				})
				.ToArray();

			// Serializamos y enviamos con el tipo "diagnostics"
			string response = JsonSerializer.Serialize(new { type = "diagnostics", data = simplifiedErrors });
			await SendString(response);
		}
	}
}
