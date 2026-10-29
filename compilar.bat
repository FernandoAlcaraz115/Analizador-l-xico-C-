@echo off
setlocal enabledelayedexpansion
title Compilar Analizador Lexico en C#

echo =======================================================
echo          COMPILADOR DEL ANALIZADOR LEXICO (C#)
echo =======================================================
echo.

set CSC_PATH=

if exist "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" (
    set "CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
) else if exist "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
    set "CSC_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
) else (
    where csc >nul 2>&1
    if !errorlevel! equ 0 (
        set "CSC_PATH=csc"
    )
)

if "%CSC_PATH%"=="" (
    echo [ERROR] No se encontro el compilador csc.exe de .NET Framework en el sistema.
    echo Asegurese de tener instalado .NET Framework 4.0 o superior.
    echo.
    pause
    exit /b 1
)

echo Usando compilador: %CSC_PATH%
echo Compilando archivos de codigo fuente...
echo.

"%CSC_PATH%" /target:winexe /optimize+ /nologo /r:System.Windows.Forms.dll,System.Drawing.dll,System.dll,System.Core.dll /out:AnalizadorLexico.exe src\Model\*.cs src\Core\*.cs src\UI\*.cs src\Program.cs

if %errorlevel% equ 0 (
    echo.
    echo =======================================================
    echo   [EXITO] AnalizadorLexico.exe compilado correctamente.
    echo =======================================================
    echo.
    echo Puede ejecutarlo mediante:
    echo   - Doble clic en 'ejecutar.bat' (Modo Grafico)
    echo   - O ejecutar 'ejecutar_consola.bat' (Modo Consola)
    echo.
) else (
    echo.
    echo [ERROR] Hubo fallos durante la compilacion.
    echo.
)

pause
