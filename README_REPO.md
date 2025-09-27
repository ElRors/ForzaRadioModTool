# 🚗 Forza Radio Mod Tool v2.0

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Windows](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](#)

> **Una herramienta profesional y moderna para modificar estaciones de radio en juegos Forza, completamente refactorizada con arquitectura Clean Code.**

![ForzaRadioModTool](https://img.shields.io/badge/Forza_Radio-Mod_Tool-red?style=for-the-badge&logo=data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjQiIGhlaWdodD0iMjQiIHZpZXdCb3g9IjAgMCAyNCAyNCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTEyIDJMMTMuMDkgOC4yNkwyMCA5TDEzLjA5IDE1Ljc0TDEyIDIyTDEwLjkxIDE1Ljc0TDQgOUwxMC45MSA4LjI2TDEyIDJaIiBmaWxsPSJ3aGl0ZSIvPgo8L3N2Zz4K)

## ✨ Características Principales

### 🏗️ **Arquitectura Moderna**
- **Clean Architecture** con separación clara de responsabilidades
- **Dependency Injection** con Microsoft.Extensions
- **Async/Await** para operaciones no bloqueantes
- **SOLID Principles** aplicados consistentemente

### 🎵 **Funcionalidades de Audio**
- **Modificación de estaciones** de radio de Forza Horizon
- **Gestión completa** de bancos de audio FMOD
- **Conversión automática** de formatos de audio
- **Backup automático** de archivos originales
- **Validación de archivos** de audio

### 🎨 **Interfaz de Usuario**
- **Windows Forms modernizado** con diseño responsivo
- **Icono personalizado** con temática automotriz
- **Soporte multi-idioma** (Inglés/Español)
- **Progress bars** para operaciones largas
- **Tooltips informativos** y mensajes de estado

### 📊 **Logging y Monitoreo**
- **Structured logging** con Serilog
- **Múltiples outputs** (consola y archivos)
- **Configuración flexible** de niveles de log
- **Diagnóstico detallado** de errores

---

## 🚀 Instalación Rápida

### Prerrequisitos
- **Windows 10/11** (64-bit)
- **.NET 8.0 Runtime** ([Descargar](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Forza Horizon 4/5** instalado

### Descarga
1. Ve a [Releases](../../releases) y descarga la última versión
2. Extrae el archivo ZIP
3. Ejecuta `ForzaRadioModTool.exe`

---

## � Tutorial

### 🎥 **Video Tutorial Completo**

[![ForzaRadioModTool v2.0 Tutorial](https://img.youtube.com/vi/q01OBJYeMWc/maxresdefault.jpg)](https://www.youtube.com/watch?v=q01OBJYeMWc)

**[▶️ Ver Tutorial en YouTube](https://www.youtube.com/watch?v=q01OBJYeMWc)**

En este tutorial aprenderás:
- ✅ Instalación y configuración inicial
- ✅ Navegación de la interfaz
- ✅ Reemplazo de canciones en estaciones de radio
- ✅ Aplicación de cambios al juego
- ✅ Solución de problemas comunes
- ✅ Tips y mejores prácticas

---

## �📖 Uso

### 1. **Configuración Inicial**
```
1. Abrir ForzaRadioModTool
2. Hacer clic en "Buscar Carpeta" 
3. Seleccionar la carpeta del juego Forza
4. Seleccionar idioma preferido
```

### 2. **Modificar Estaciones**
```
1. Las estaciones se cargan automáticamente
2. Seleccionar una estación de la lista
3. Ver/modificar canciones en la tabla
4. Usar "Reemplazar Audio" para nuevas canciones
```

### 3. **Aplicar Cambios**
```
1. Hacer clic en "Aplicar Cambios"
2. Usar "Extraer Bank" si es necesario
3. "Insertar en Juego" para aplicar modificaciones
```

---

## 🏗️ Arquitectura del Proyecto

```
ForzaRadioModTool/
├── 📦 Core/
│   ├── Models/          # Modelos de datos y excepciones
│   ├── Services/        # Lógica de negocio
│   └── Interfaces/      # Contratos de servicios
├── 🎨 UI/Forms/         # Interfaz de usuario
├── ⚙️ Configuration/    # Configuración de la aplicación  
├── 🔧 Common/          # Utilidades compartidas
├── 🎵 Helpers/         # Helpers de audio (legacy)
└── 🛠️ Tools/           # Herramientas externas (FFmpeg, FMOD)
```

### Servicios Principales
- **🎵 RadioManager**: Orquestador principal de operaciones de radio
- **📄 XmlProcessor**: Manejo de archivos de configuración XML
- **📁 FileManager**: Operaciones de archivos y directorios
- **🎧 AudioProcessor**: Conversión y validación de audio
- **⚙️ ConfigurationService**: Gestión de configuración de la aplicación

---

## 🔧 Tecnologías Utilizadas

### **Framework Principal**
- **.NET 8.0** - Framework principal
- **C# 12** - Lenguaje con características modernas
- **Windows Forms** - Interfaz gráfica nativa

### **Dependencias**
```xml
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog" Version="4.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

### **Herramientas Externas**
- **FFmpeg** - Conversión de formatos de audio
- **FMOD Bank Tools** - Manipulación de bancos de audio

---

## ⚙️ Configuración

### `appsettings.json`
```json
{
  "Audio": {
    "DefaultVolumeDb": -13.0,
    "SampleRate": 44100,
    "AllowedFormats": ["wav", "mp3", "flac"]
  },
  "UI": {
    "DefaultLanguage": "EN",
    "MinWindowWidth": 800,
    "MinWindowHeight": 600,
    "ShowTooltips": true
  },
  "Logging": {
    "LogLevel": "Information",
    "EnableFileLogging": true,
    "MaxLogFiles": 10
  }
}
```

---

## 📻 Estaciones Soportadas

| Estación | Archivo Bank | Descripción |
|----------|--------------|-------------|
| **Radio Rock** | `RadioStation_Rock.bank` | Rock clásico y alternativo |
| **Radio Hip Hop** | `RadioStation_Hip_Hop.bank` | Hip hop y R&B |
| **Radio Hospital** | `RadioStation_Hospital.bank` | Drum & Bass |
| **Radio Elektronika** | `RadioStation_Elektronika.bank` | Música electrónica |
| **Radio Levante** | `RadioStation_Levante.bank` | Música latina |
| **Radio Vagrant** | `RadioStation_Vagrant.bank` | Indie y alternativo |
| **Radio Block Party** | `RadioStation_Block.bank` | Party y dance |
| **Radio Eterna** | `RadioStation_Eterna.bank` | Clásica y ambiental |
| **Radio XS** | `RadioStation_XS.bank` | Variedad musical |

---

## 🛠️ Desarrollo

### **Compilar desde Código Fuente**
```bash
# Clonar repositorio
git clone https://github.com/tu-usuario/ForzaRadioModTool.git
cd ForzaRadioModTool

# Restaurar paquetes
dotnet restore

# Compilar
dotnet build --configuration Release

# Ejecutar
dotnet run
```

### **Estructura de Commits**
```
feat: nueva funcionalidad
fix: corrección de bug  
docs: cambios en documentación
style: formateo de código
refactor: refactorización
test: pruebas
chore: tareas de mantenimiento
```

---

## 📊 Métricas de Calidad

| Métrica | Valor | Estado |
|---------|-------|--------|
| **Líneas de código** | ~2,500 | ✅ Óptimo |
| **Complejidad ciclomática** | < 10 | ✅ Excelente |
| **Cobertura de código** | TBD | ⏳ Pendiente |
| **Deuda técnica** | Muy baja | ✅ Excelente |
| **Mantenibilidad** | Alta | ✅ Excelente |

---

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Por favor:

1. **Fork** el proyecto
2. **Crear branch** para tu feature (`git checkout -b feature/AmazingFeature`)
3. **Commit** tus cambios (`git commit -m 'Add: AmazingFeature'`)
4. **Push** al branch (`git push origin feature/AmazingFeature`)
5. **Abrir Pull Request**

### **Pautas de Contribución**
- Seguir principios **SOLID** y **Clean Code**
- Incluir **tests unitarios** para nuevas funcionalidades
- Actualizar **documentación** según corresponda
- Usar **conventional commits** para mensajes

---

## 📄 Licencia

Este proyecto está bajo la licencia **MIT**. Ver [LICENSE](LICENSE) para más detalles.

---

## 🙏 Agradecimientos

- **Forza Community** por el soporte y feedback
- **FMOD Technologies** por las herramientas de audio
- **Microsoft** por el framework .NET
- **Serilog** por el excelente sistema de logging

---

## 📞 Soporte

- **🐛 Bug Reports**: [Issues](../../issues)
- **💡 Feature Requests**: [Issues](../../issues)
- **📖 Wiki**: [Wiki](../../wiki)
- **💬 Discusiones**: [Discussions](../../discussions)

---

<div align="center">

**⭐ Si este proyecto te ayudó, considera darle una estrella ⭐**

**Hecho con ❤️ para la comunidad de Forza**

![Forza Logo](https://img.shields.io/badge/Powered_by-Forza_Community-orange?style=for-the-badge)

</div>