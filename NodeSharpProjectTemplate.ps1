param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectName
)

Write-Host "Creando proyecto Windows Forms: $ProjectName" -ForegroundColor Green

# Crear proyecto
dotnet new winforms -n $ProjectName -f net8.0

# Cambiar al directorio
Set-Location $ProjectName

# Eliminar archivos del formulario predeterminado
Remove-Item "Form1.cs" -ErrorAction SilentlyContinue
Remove-Item "Form1.Designer.cs" -ErrorAction SilentlyContinue
Remove-Item "Form1.resx" -ErrorAction SilentlyContinue

Write-Host "`nArchivos Form1 eliminados" -ForegroundColor Yellow

# Modificar Program.cs
$programContent = @"
namespace $ProjectName
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            // Aquí puedes iniciar tu propio formulario
            // Application.Run(new MiFormulario());
            
            MessageBox.Show("Proyecto $ProjectName creado sin formulario principal", "Info");
        }
    }
}
"@

Set-Content -Path "Program.cs" -Value $programContent

Write-Host "Program.cs actualizado" -ForegroundColor Green
Write-Host "`nProyecto listo en: $(Get-Location)" -ForegroundColor Cyan
Write-Host "`nComandos útiles:" -ForegroundColor White
Write-Host "  dotnet build   - Compilar" -ForegroundColor Gray
Write-Host "  dotnet run     - Ejecutar" -ForegroundColor Gray