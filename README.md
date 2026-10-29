# ⚡ Analizador Léxico (Scanner) en C#

Un analizador léxico completo, profesional y modular desarrollado en **C# (.NET)**, diseñado para asignaturas de **Compiladores, Teoría de Lenguajes Formales y Autómatas**, o como módulo inicial para un compilador o intérprete de programación.

El proyecto cuenta con **soporte dual**:
1. **Modo Gráfico (GUI)**: Una interfaz moderna en modo oscuro (*Dark Slate Theme*) con editor de código, visor de tokens clasificados por color, tabla de símbolos, lista de errores y métricas de rendimiento.
2. **Modo Consola (CLI)**: Permite procesar archivos de código fuente directamente por terminal con salida formateada, tablas alineadas y colores sintácticos.

---

## 🚀 Características Principales

- **Clasificación Exhaustiva de Tokens**:
  - **Palabras Reservadas de Control**: `if`, `else`, `while`, `for`, `do`, `switch`, `case`, `break`, `continue`, `return`, `default`.
  - **Tipos de Datos**: `int`, `float`, `double`, `char`, `string`, `bool`, `void`.
  - **Estructura y POO**: `class`, `function`, `public`, `private`, `static`, `new`, `main`.
  - **Entrada / Salida**: `print`, `println`, `read`.
  - **Constantes y Literales**: Enteros (`42`), Decimales (`3.1416`), Cadenas con escapes (`"texto\n"`), Caracteres (`'a'`), Booleanos (`true`, `false`) y Nulos (`null`).
  - **Identificadores de Usuario**: Validados bajo la regla `[a-zA-Z_][a-zA-Z0-9_]*`.
  - **Operadores Aritméticos**: `+`, `-`, `*`, `/`, `%`, `++`, `--`.
  - **Operadores Relacionales**: `==`, `!=`, `<`, `<=`, `>`, `>=`.
  - **Operadores Lógicos**: `&&`, `||`, `!`.
  - **Operadores de Asignación**: `=`, `+=`, `-=`, `*=`, `/=`, `%=`.
  - **Delimitadores y Puntuación**: `(`, `)`, `{`, `}`, `[`, `]`, `;`, `,`, `:`, `.`, `->`.
  - **Comentarios**: De una línea (`// ...`) y de bloque (`/* ... */`).
- **Manejo Robusto de Errores Léxicos**:
  - Detección de caracteres no permitidos (`@`, `#`, `$`, etc.).
  - Números mal formados (e.g. `12.34.56` o `99variable`).
  - Cadenas y caracteres no cerrados antes del fin de línea o fin de archivo.
  - Comentarios de bloque sin cerrar (`/*`).
  - Operadores lógicos incompletos (`&` o `|`).
  - No detiene el análisis ante un error: recolecta todos los errores con su ubicación exacta (**Línea** y **Columna**) y proporciona sugerencias de solución.
- **Tabla de Símbolos Automática**:
  - Identificación de identificadores únicos.
  - Conteo de ocurrencias / apariciones.
  - Inferencia básica de tipos a partir del contexto previo (`int x;` -> `x: int`).
  - Historial de coordenadas `(L:C)` de cada aparición.
- **Exportación de Resultados**:
  - Posibilidad de exportar la lista de tokens generada a formato `.csv` para su análisis posterior.
- **Sin Dependencias Externas**:
  - Compila directamente con el compilador nativo de Windows `csc.exe` (.NET Framework 4.0+) sin necesidad de instalar SDKs adicionales.

---

## 📁 Estructura del Proyecto

```text
analizador_lexico/
│
├── AnalizadorLexico.exe        # Ejecutable compilado listo para usar
├── AnalizadorLexico.csproj     # Archivo de solución para Visual Studio / VS Code
├── compilar.bat                # Script para compilar el código con csc.exe
├── ejecutar.bat                # Script para abrir la interfaz gráfica
├── ejecutar_consola.bat        # Script para ejecutar el análisis por consola
├── README.md                   # Documentación detallada
│
├── src/                        # Código fuente modular en C#
│   ├── Model/
│   │   ├── TokenType.cs        # Enumeración de tipos de tokens
│   │   ├── TokenCategory.cs    # Categorías generales para filtrado y estilo
│   │   ├── Token.cs            # Estructura del componente léxico
│   │   ├── LexicalError.cs     # Estructura de errores detectados
│   │   └── SymbolItem.cs       # Elemento de la tabla de símbolos
│   │
│   ├── Core/
│   │   ├── Lexer.cs            # Motor del analizador léxico (Scanner / AFD)
│   │   ├── SymbolTable.cs      # Gestor de la tabla de símbolos
│   │   └── LexerStats.cs       # Estadísticas y métricas de rendimiento
│   │
│   ├── UI/
│   │   ├── MainForm.cs         # Ventana principal en Windows Forms
│   │   └── Theme.cs            # Paleta de colores Dark Theme y estilos
│   │
│   └── Program.cs              # Punto de entrada dual (GUI o CLI)
│
└── ejemplos/                   # Códigos fuente de prueba
    ├── ejemplo_correcto.txt    # Código con sintaxis válida
    ├── ejemplo_con_errores.txt # Código con errores léxicos intencionados
    └── ejemplo_algoritmo.txt   # Algoritmo de ordenamiento y factorial
```

---

## 🛠️ Cómo Compilar

El proyecto ya incluye el ejecutable `AnalizadorLexico.exe` generado, pero si realizas cambios puedes recompilarlo de las siguientes maneras:

### Opción 1: Mediante el script automático
Haz doble clic en `compilar.bat`. El script detectará el compilador de C# de Windows (`csc.exe`) y compilará todos los archivos en un nuevo ejecutable.

### Opción 2: Desde la terminal de comandos (PowerShell / CMD)
```cmd
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:exe /optimize+ /nologo /r:System.Windows.Forms.dll,System.Drawing.dll,System.dll,System.Core.dll /out:AnalizadorLexico.exe src\Model\*.cs src\Core\*.cs src\UI\*.cs src\Program.cs
```

### Opción 3: Con Visual Studio
Abre el archivo `AnalizadorLexico.csproj` en **Visual Studio** o **Visual Studio Code** y presiona `F5` o `Build Solution`.

---

## 💻 Cómo Ejecutar

### 1. Interfaz Gráfica (Modo Visual)
- Haz doble clic en `ejecutar.bat` o directamente en `AnalizadorLexico.exe`.
- Puedes pegar cualquier código fuente o elegir uno de los ejemplos predefinidos desde el desplegable de la barra superior.
- Haz clic en **▶ Analizar** para obtener los resultados al instante.
- Pestañas disponibles:
  - **Tokens**: Tabla interactiva con número, línea, columna, lexema, tipo, categoría y descripción.
  - **Tabla de Símbolos**: Identificadores únicos, tipo inferido, total de apariciones y coordenadas.
  - **Errores Léxicos**: Lista detallada de fallos léxicos y sugerencias.
  - **Métricas & Resumen**: Total de tokens, distribución por categoría y tiempo en milisegundos.
  - **Exportar CSV**: Guarda los tokens resultantes en formato de hoja de cálculo.

### 2. Modo Consola (Línea de Comandos)
Para procesar un archivo desde la terminal:
```cmd
AnalizadorLexico.exe ejemplos\ejemplo_correcto.txt
```
O ejecutando el archivo por lotes:
```cmd
ejecutar_consola.bat
```

Para ver el análisis de un código con errores:
```cmd
AnalizadorLexico.exe ejemplos\ejemplo_con_errores.txt
```

---

## 🔬 Especificación de la Gramática Léxica

| Componente | Expresión Regular / Regla | Ejemplo |
| :--- | :--- | :--- |
| **Identificadores** | `[a-zA-Z_][a-zA-Z0-9_]*` | `totalVenta`, `_contador`, `nodo1` |
| **Enteros** | `[0-9]+` | `0`, `450`, `1001` |
| **Decimales** | `[0-9]+\.[0-9]+([eE][+-]?[0-9]+)?` | `3.14159`, `0.5`, `1.2e-3` |
| **Cadenas** | `"([^"\n\\]|\\.)*"` | `"Hola Mundo"`, `"Salto\n"` |
| **Caracteres** | `'([^'\n\\]|\\.)'` | `'a'`, `'\t'`, `'Z'` |
| **Operadores** | `+`, `-`, `*`, `/`, `%`, `++`, `--`, `==`, `!=`, `<`, `<=`, `>`, `>=`, `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `&&`, `||`, `!` | `x += 5;`, `a && b` |
| **Delimitadores**| `(`, `)`, `{`, `}`, `[`, `]`, `;`, `,`, `:`, `.`, `->` | `(a, b);` |
| **Comentarios** | `//[^\n]*` ó `/\*[\s\S]*?\*/` | `// nota`, `/* bloque */` |
