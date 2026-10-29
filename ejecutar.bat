@echo off
title Analizador Lexico - Modo Grafico

if not exist "AnalizadorLexico.exe" (
    echo [INFO] AnalizadorLexico.exe no encontrado. Compilando primero...
    call compilar.bat
)

if exist "AnalizadorLexico.exe" (
    start "" AnalizadorLexico.exe
) else (
    echo [ERROR] No se pudo encontrar ni compilar AnalizadorLexico.exe
    pause
)
