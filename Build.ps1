$ErrorActionPreference = 'Stop'
$filTjekCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
Push-Location -LiteralPath $PSScriptRoot
try {
    & $filTjekCompiler /nologo /target:winexe /optimize+ /platform:anycpu /out:FilTjek.exe /win32manifest:FilTjek.manifest /reference:System.Windows.Forms.dll /reference:System.Drawing.dll FilTjek.cs
    if ($LASTEXITCODE -ne 0) { throw 'Build mislykkedes.' }
} finally { Pop-Location }
