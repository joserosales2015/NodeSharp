var editor;

require.config({ paths: { 'vs': 'min/vs' } });

require(['vs/editor/editor.main'], function () {
    editor = monaco.editor.create(document.getElementById('container'), {
        value: "",
        language: 'csharp',       // Activa Highlighting y Autocompletado básico
        theme: 'vs-dark',         // Tema Oscuro
        automaticLayout: true,    // Se ajusta al cambiar tamaño de ventana
        folding: true,            // Activa Code Folding
        minimap: { enabled: true }
    });

    // Notificar a C# cuando el texto cambia (opcional)
    editor.onDidChangeModelContent((e) => {
        // Si usas WebView2, puedes enviar mensajes así:
        // window.chrome.webview.postMessage({ type: 'textChanged', content: editor.getValue() });
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