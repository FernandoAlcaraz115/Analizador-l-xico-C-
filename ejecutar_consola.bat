@echo off
title Analizador Lexico - Modo Consola

if not exist "AnalizadorLexico.exe" (
    echo [INFO] AnalizadorLexico.exe no encontrado. Compilando primero...
    call compilar.bat
)

if not exist "AnalizadorLexico.exe" (
    echo [ERROR] No se pudo encontrar ni compilar AnalizadorLexico.exe
    pause
    exit /b 1
)

set TARGET_FILE=%1
if "%TARGET_FILE%"=="" (
    set "TARGET_FILE=ejemplos\ejemplo_correcto.txt"
)

echo Ejecutando analisis lexico sobre: %TARGET_FILE%
echo.

AnalizadorLexico.exe "%TARGET_FILE%"

echo.
echo Presione cualquier tecla para salir...
pause >nul
