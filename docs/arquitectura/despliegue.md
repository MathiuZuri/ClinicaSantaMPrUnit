# 🚀 Despliegue en la Nube (Azure & Neon)

El sistema SIGEC de la Clínica Santa Mónica se despliega en la nube de **Microsoft Azure** utilizando dos servicios principales: **Azure Static Web Apps** para el frontend (Blazor WASM) y **Azure App Service** para el backend (API RESTful). La base de datos PostgreSQL se encuentra en **Neon.tech** (serverless), proporcionando una solución escalable y de bajo mantenimiento.

El pipeline de despliegue está completamente automatizado mediante **GitHub Actions**, garantizando que cada cambio en la rama `master` pase por un riguroso proceso de compilación, pruebas, análisis de calidad (SonarCloud), verificación de vulnerabilidades (Snyk) y despliegue continuo.

---

## 🏗️ Arquitectura de Despliegue

| Componente | Tecnología | Ubicación | Propósito |
| :--- | :--- | :--- | :--- |
| **Frontend** | Blazor WebAssembly | Azure Static Web Apps | Interfaz de usuario SPA |
| **Backend API** | ASP.NET Core 9 | Azure App Service | API RESTful y SignalR |
| **Base de Datos** | PostgreSQL | Neon.tech (AWS sa-east-1) | Persistencia de datos |
| **CI/CD** | GitHub Actions | Repositorio GitHub | Automatización de builds, tests y despliegues |
| **Calidad de Código** | SonarCloud | Servicio en la nube | Análisis estático y cobertura |
| **Seguridad** | Snyk | Servicio en la nube | Escaneo de vulnerabilidades |

---

## 🔄 Workflow de GitHub Actions

El repositorio contiene dos workflows principales:

1. **Azure Static Web Apps CI/CD** (`static-web-apps.yml`): Responsable del despliegue del frontend Blazor WASM a Azure Static Web Apps.
2. **CI/CD Pipeline - Clinica Santa Monica** (`ci-cd-pipeline.yml`): Responsable del build, pruebas, análisis de calidad y despliegue del backend API a Azure App Service.

Ambos workflows se activan en `push` y `pull_request` a la rama `master`.

### 1. Workflow de Azure Static Web Apps (Frontend)

Este workflow se encarga del despliegue continuo del frontend Blazor WASM. Utiliza la acción oficial `Azure/static-web-apps-deploy@v1`.

* **Desencadenadores:**
    * `push` a `master`
    * `pull_request` a `master`

* **Pasos principales:**
    1. **Checkout** del código fuente.
    2. **Configuración de .NET 9**.
    3. **Build y despliegue** a Azure Static Web Apps:
        * `app_location`: `./Clinica.WASM`
        * `output_location`: `wwwroot`
        * `app_build_command`: `dotnet publish -c Release`

* **Configuración de secrets:**
    * `AZURE_STATIC_WEB_APPS_API_TOKEN_SALMON_BUSH_08C1E7510`: Token de despliegue generado por Azure Static Web Apps.

---

### 2. Workflow CI/CD Pipeline (Backend API)

Este es el workflow principal que gestiona el backend. Incluye build, pruebas unitarias y de integración, análisis de calidad con SonarCloud, escaneo de vulnerabilidades con Snyk y despliegue a Azure App Service.

* **Desencadenadores:**
    * `push` a `master`
    * `pull_request` a `master`
    * `workflow_dispatch`: Ejecución manual desde la interfaz de GitHub.

#### Etapa de Integración Continua (`build-test`)

| Paso | Herramienta | Propósito |
| :--- | :--- | :--- |
| **Checkout** | `actions/checkout@v4` | Obtener el código fuente con historial completo. |
| **Setup .NET 9** | `actions/setup-dotnet@v4` | Instalar .NET 9 SDK en el agente runner. |
| **Setup Java 17** | `actions/setup-java@v4` | Requisito obligatorio para la ejecución de SonarScanner. |
| **Cache Sonar** | `actions/cache@v4` | Almacenar paquetes de Sonar para acelerar análisis futuros. |
| **Instalar SonarScanner** | Tool nativa | Herramienta global de análisis de código de SonarCloud. |
| **Instalar ReportGenerator** | Tool nativa | Generación consolidada de reportes de cobertura en formato HTML. |
| **Restore** | `dotnet restore` | Restaurar dependencias NuGet de la solución `Clinica.sln`. |
| **Sonar Begin** | `sonarscanner begin` | Iniciar análisis de SonarCloud parametrizando las exclusiones. |
| **Build** | `dotnet build` | Compilar la solución en modo Release forzando la bandera `--no-restore`. |
| **Unit Tests** | `dotnet test` | Ejecutar pruebas unitarias y recoger cobertura técnica (Opencover). |
| **Integration Tests** | `dotnet test` | Ejecutar pruebas de integración acopladas a contenedores de Docker. |
| **Generate Report** | `reportgenerator` | Generar reporte unificado a partir de los archivos XML de cobertura. |
| **Snyk Test** | `snyk test` | Escanear vulnerabilidades de alta gravedad en librerías externas. |
| **Sonar End** | `sonarscanner end` | Finalizar análisis estático y transmitir métricas a SonarCloud. |
| **Publish API** | `dotnet publish` | Publicar los artefactos compilados de la API de producción. |

#### Configuración Técnica de Análisis Estático (SonarCloud)

A continuación se detalla la inyección de exclusiones de archivos e indexación de trazas de cobertura utilizadas en el pipeline automatizado:

```yaml
/d:sonar.exclusions="**/Clinica.API.Tests/**,**/Clinica.API.IntegrationTests/**,**/Clinica.E2ETest/**,**/*.js,**/node_modules/**,**/*.html,**/*.css,**/bin/**,**/obj/**,**/Migrations/**,**/k6/**,**/infra/sonarqube/**,**/CoverageReport/**"
/d:sonar.cs.opencover.reportsPaths="$GITHUB_WORKSPACE/cobertura/Unit/coverage.opencover.xml,$GITHUB_WORKSPACE/cobertura/Integration/coverage.opencover.xml"
/d:sonar.coverage.exclusions="**/Clinica.WASM/**,**/Clinica.Infrastructure/**,**/Clinica.API/Program.cs,**/Clinica.API/Configurations/**,**/Clinica.API/Middlewares/**"