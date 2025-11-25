var editor;

require.config({ paths: { 'vs': 'min/vs' } });

require(['vs/editor/editor.main'], function () {
    editor = monaco.editor.create(document.getElementById('container'), {
        value: "",
        language: 'csharp',       // Activa Highlighting y Autocompletado básico
        //theme: 'vs-dark',       // Este parametro se cambia dinámicamente desde C#
        automaticLayout: true,    // Se ajusta al cambiar tamaño de ventana
        folding: true,            // Activa Code Folding
        minimap: { enabled: true }
    });

    // 2. Iniciar conexión WebSocket
    // Asegúrate que este puerto (5000) sea el mismo que pusiste en MonacoEditor.xaml.cs
    const socket = new WebSocket("ws://localhost:5000");

    socket.onopen = () => {
        console.log("JS: Conectado al servidor WebSocket");
    };

    socket.onerror = (err) => {
        console.error("JS: Error de WebSocket", err);
    };

    socket.onmessage = (event) => {
        const msg = JSON.parse(event.data);
        const requestId = msg.requestId;

        if (msg.type === "completion_response") {
            // Aquí recibimos las sugerencias de C# y las guardamos
            // Nota: Monaco requiere un "CompletionItemProvider" registrado para mostrar esto.
            // Para simplificar, guardamos esto en una variable global que el Provider leerá.
            window.latestSuggestions = msg.data;
        }
        else if (msg.type === "diagnostics") {
            // Convertimos los datos que vienen de C# al formato de Monaco
            var markers = msg.data.map(err => ({
                severity: err.Severity,
                startLineNumber: err.StartLine,
                startColumn: err.StartColumn,
                endLineNumber: err.EndLine,
                endColumn: err.EndColumn,
                message: err.Message
            }));

            // Aplicamos los marcadores al modelo actual
            monaco.editor.setModelMarkers(monaco.editor.getModels()[0], "Roslyn", markers);
        }
        else if (msg.type === "signature_response") {
            // Guardamos la respuesta temporalmente
            window.latestSignature = msg.data;
        }
        
    };

    // 3. Registrar Autocompletado (AHORA ESTÁ DENTRO DEL BLOQUE)
    monaco.languages.registerCompletionItemProvider('csharp', {
        triggerCharacters: ['.'],
        provideCompletionItems: function (model, position) {

            const code = model.getValue();
            const offset = model.getOffsetAt(position);

            if (socket.readyState === WebSocket.OPEN) {
                const request = {
                    type: "completion",
                    code: code,
                    position: offset
                };
                socket.send(JSON.stringify(request));
            }

            // Devolver sugerencias cacheadas (Hack temporal para visualización)
            var suggestions = (window.latestSuggestions || []).map(text => ({
                label: text,
                kind: monaco.languages.CompletionItemKind.Method,
                insertText: text
            }));

            return { suggestions: suggestions };
        }
    });

    monaco.languages.registerSignatureHelpProvider('csharp', {
        // Caracteres que disparan la ayuda: paréntesis de apertura y coma
        signatureHelpTriggerCharacters: ['(', ','],

        provideSignatureHelp: function (model, position, token) {
            const code = model.getValue();
            const offset = model.getOffsetAt(position);

            if (socket.readyState === WebSocket.OPEN) {
                socket.send(JSON.stringify({
                    type: "signature",
                    code: code,
                    position: offset
                }));
            }

            // Hack para esperar un poco la respuesta del socket (solo visual)
            // En un mundo ideal usaríamos Promesas/Async real
            var data = window.latestSignature || { signatures: [] };

            return {
                value: {
                    signatures: data.signatures || [],
                    activeSignature: data.activeSignature || 0,
                    activeParameter: data.activeParameter || 0
                },
                dispose: () => { }
            };
        }
    });

    // 4. Escuchar cambios en el texto

    // Enviar actualizaciones de código para mantener el servidor sincronizado
    editor.onDidChangeModelContent((e) => {
        if (socket.readyState === WebSocket.OPEN) {
            const request = {
                type: "update",
                code: editor.getValue()
            };
            socket.send(JSON.stringify(request));
        }
    });
});

// --- Funciones API para ser llamadas desde C# ---

function setCode(code) {
    if (editor) editor.setValue(code);
}

function getCode() {
    return editor ? editor.getValue() : "";
}

// Función para subrayar errores
function setMarkers(jsonErrors) {
    if (!editor) return;

    var parsedErrors = JSON.parse(jsonErrors);
    var markers = [];

    parsedErrors.forEach(err => {
        markers.push({
            startLineNumber: err.Line,
            startColumn: err.Column,
            endLineNumber: err.Line,
            endColumn: err.Column + err.Length,
            message: err.Message,
            severity: monaco.MarkerSeverity.Error // Puede ser Warning o Info
        });
    });

    monaco.editor.setModelMarkers(editor.getModel(), "owner", markers);
}

function setMonacoTheme(themeName) {
    if (monaco) {
        // themeName debe ser 'vs' (claro) o 'vs-dark' (oscuro)
        monaco.editor.setTheme(themeName);
    }
}
