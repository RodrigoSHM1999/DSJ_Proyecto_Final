# DSJ_Proyecto_Final

**Versión de Unity:** 2022.3.62f1 (LTS)  
**Plataforma:** Unity 3D (Windows)  
**Tipo de proyecto:** Juego con navegación mediante IA (AI Navigation)  
**Repositorio:** [GitHub](https://github.com/RodrigoSHM1999/DSJ_Proyecto_Final)

---

## Descripción

**DSJ_Proyecto_Final** es un proyecto desarrollado en **Unity 3D** que utiliza la librería **AI Navigation** para implementar agentes inteligentes y trayectorias de movimiento en entornos 3D.  
El proyecto incluye **múltiples escenas**, scripts personalizados y está configurado para trabajo colaborativo en equipo a través de **Git y GitHub**.

---

## Requisitos del entorno

| Requisito | Versión recomendada | Descripción |
|-----------|---------------------|-------------|
| **Unity Hub** | Última versión estable | Para gestionar proyectos y versiones de Unity |
| **Unity Editor** | **2022.3.62f1 (LTS)** | Versión exacta usada por el equipo |
| **Git** | 2.40 o superior | Para clonar y sincronizar el repositorio |
| **Visual Studio Code / Visual Studio 2022** | Opcional | Para editar scripts en C# |

> **IMPORTANTE:** Todos los miembros del equipo **deben usar exactamente la misma versión de Unity** para evitar conflictos de escenas o meta archivos.

---

## Librerías y dependencias

El proyecto utiliza los siguientes paquetes de Unity (ya configurados en `/Packages/manifest.json`):

- `com.unity.ai.navigation` - Control de navegación por malla (NavMesh)
- `com.unity.inputsystem` - Nuevo sistema de entrada (si aplica)
- `com.unity.textmeshpro` - Textos y UI

> Unity descargará automáticamente estos paquetes al abrir el proyecto por primera vez.

---

## Estructura del proyecto

```
DSJ_Proyecto_Final/
├── Assets/              # Escenas, scripts, materiales, prefabs, etc.
│   ├── Scenes/          # Todas las escenas del proyecto
│   ├── Scripts/         # Código C# del juego
│   ├── Prefabs/         # Prefabs reutilizables
│   └── Materials/       # Materiales y texturas
├── Packages/            # Dependencias de Unity
├── ProjectSettings/     # Configuración del proyecto
├── UserSettings/        # Configuración local del usuario
├── .gitignore           # Exclusiones de Git
└── README.md            # Este archivo
```

---

## Instalación del proyecto

### 1. Clonar el repositorio

Abre **Git Bash**, **PowerShell** o **GitHub Desktop** y ejecuta:

```bash
git clone https://github.com/RodrigoSHM1999/DSJ_Proyecto_Final.git
```

---

### 2. Abrir el proyecto en Unity Hub

1. Abre **Unity Hub**
2. Selecciona **Projects → Add project from disk**
3. Busca la carpeta clonada `DSJ_Proyecto_Final`
4. Unity detectará automáticamente la versión **2022.3.62f1**
5. Haz clic en **Open**

> **NOTA:** La primera vez que abras el proyecto, Unity reconstruirá la carpeta `Library` (puede tardar unos minutos).

---

### 3. Verificar paquetes

1. En Unity, ve a **Window → Package Manager**
2. Asegúrate de que los siguientes paquetes estén instalados:
   - **AI Navigation**
   - **TextMeshPro**
   - **Input System** (si se usa)
3. Si falta alguno, presiona **Add package by name…** e introduce el nombre del paquete.

---

### 4. Ejecutar el juego

1. Abre la escena principal desde:
   ```
   Assets/Scenes/
   ```
2. Haz doble clic en la escena inicial (por ejemplo, `MenuPrincipal.unity` o `Nivel1.unity`)
3. Presiona **Play** en la parte superior de Unity

---

## Buenas prácticas para trabajo en equipo

### Antes de trabajar:

```bash
git pull origin main
```

> Trae los cambios más recientes del equipo.

### Después de hacer cambios:

```bash
git add .
git commit -m "Descripción breve del cambio realizado"
git push origin main
```

> Sube tus cambios al repositorio remoto.

### Recomendaciones:

- No modificar escenas que otro miembro esté editando sin coordinar
- No subir carpetas pesadas (Unity las regenera)
- Verificar que el proyecto **compila sin errores** antes de hacer push
- Mantener los nombres de archivos y carpetas en inglés y sin espacios

---

## Archivo .gitignore

El proyecto incluye un `.gitignore` configurado para evitar archivos innecesarios:

```gitignore
# Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
UserSettings/
MemoryCaptures/
Recordings/

# IDEs
.vscode/
.idea/
*.csproj
*.sln
*.user
*.unityproj

# OS
.DS_Store
Thumbs.db
```

---

