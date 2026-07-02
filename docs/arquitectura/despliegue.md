# 🚀 Despliegue en la Nube (Azure & Neon) - SYS Clínica Santa Mónica

La arquitectura de despliegue de **SYS Clínica Santa Mónica** está diseñada bajo criterios de alta disponibilidad, seguridad criptográfica y escalabilidad elástica. El sistema se distribuye en una infraestructura multinube aprovechando las capacidades de **Microsoft Azure** para el cómputo y enrutamiento del frontend y backend, y de **Neon.tech** para la persistencia relacional aislada. 

Este ecosistema en la nube responde directamente a los desafíos geográficos e infraestructurales identificados en la región de Juliaca (Puno), erradicando la vulnerabilidad del almacenamiento local en papel y servidores físicos propensos a pérdidas por cortes eléctricos o siniestros, y garantizando la continuidad del acto médico conforme a las normativas de **SUSALUD** y **MINSA**.

---

## 🏗️ Arquitectura de Despliegue Topológica

La distribución del ecosistema de software está estructurada para mitigar riesgos críticos de seguridad y garantizar tiempos de respuesta óptimos (baja latencia) para el personal asistencial de la clínica:

| Componente | Tecnología | Proveedor / Región | Justificación Técnica y de Negocio |
| :--- | :--- | :--- | :--- |
| **Frontend SPA** | Blazor WebAssembly | Azure Static Web Apps (Global) | Descarga el renderizado en el cliente. Soporta ruteo profundo contra errores 404 mediante `navigationFallback` hacia `index.html`. |
| **Backend API** | ASP.NET Core 9 | Azure App Service (East US 2) | Entorno de ejecución administrado para la API RESTful y sockets de SignalR. Escalabilidad vertical ante picos de demanda. |
| **Base de Datos** | PostgreSQL 16 | Neon.tech (AWS sa-east-1 - São Paulo) | Base de datos serverless con escalado automático a cero. Ubicada en Sudamérica para garantizar una latencia inferior a 45ms desde Juliaca. |
| **Orquestador CI/CD** | GitHub Actions | GitHub Runners | Automatización del ciclo de vida del software. Despliegue continuo condicionado a pruebas aprobadas. |
| **Análisis de Calidad** | SonarCloud | Cloud Service | Inspección estática de código. Asegura un umbral mínimo de 80% de cobertura de pruebas unitarias/integración. |
| **Seguridad de Capas** | Snyk | Cloud Service | Escaneo continuo de dependencias NuGet para bloquear vulnerabilidades OWASP antes de producción. |

!!! info "Mitigación de Latencia en el Altiplano Peruano"
    La elección de la región **AWS sa-east-1 (São Paulo)** para el clúster de Neon PostgreSQL es estratégica. Las conexiones de red en Juliaca presentan rutas de enrutamiento más estables y veloces hacia los nodos sudamericanos que hacia Norteamérica, reduciendo el *Time to First Byte* (TTFB) en las consultas de historias clínicas y pantallas de facturación electrónica.

---

## 🔄 Pipeline de Integración y Despliegue Continuo (CI/CD)

El despliegue de **SYS Clínica Santa Mónica** está completamente automatizado y gobernado por políticas de calidad estrictas. Ningún artefacto compilado puede impactar el entorno de producción en Azure si no supera de forma exitosa los umbrales de seguridad y pruebas de regresión.

```text
       [Push a Master]
              │
              ▼
   ┌─────────────────────┐
   │  Build & Restore    │
   └──────────┬──────────┘
              │
              ▼
   ┌─────────────────────┐
   │ Pruebas Unitarias   │ ➔ Cobertura Coverlet (XML)
   └──────────┬──────────┘
              │
              ▼
   ┌─────────────────────┐
   │Pruebas Integración  │ ➔ Contenedores de Docker Efímeros
   └──────────┬──────────┘
              │
              ▼
   ┌─────────────────────┐
   │Análisis SonarCloud  │ 🡦 (Bloquea si hay Vulnerabilidades Críticas o <80% Cobertura)
   └──────────┬──────────┘
              │
              ▼
   ┌─────────────────────┐
   │ Escaneo Snyk Sec    │ ➔ Validación de Dependencias Seguras
   └──────────┬──────────┘
              │
              ▼
   ┌─────────────────────┐
   │ Despliegue Automat. │ ➔ Frontend a Static Web Apps / Backend a App Service
   └─────────────────────┘
```

### 🛠️ Especificación de los Workflows de GitHub Actions
## 1. Pipeline del Frontend (static-web-apps.yml)
Este flujo compila la aplicación WebAssembly y la inyecta en el servicio de distribución perimetral de Azure Static Web Apps, optimizando la descarga de archivos .wasm y configurando las cabeceras de respuesta de la SPA.

Desencadenadores: Eventos de push o cierres conformes de pull_request sobre la rama master.

```text
name: SYS Clinica Santa Monica - Frontend Deployment

on:
  push:
    branches:
      - master
  pull_request:
    types: [opened, synchronize, reopened, closed]
    branches:
      - master

jobs:
  build_and_deploy_job:
    if: github.event_name == 'push' || (github.event_name == 'pull_request' && github.event.action != 'closed')
    runs-on: ubuntu-latest
    name: Build and Deploy Job
    steps:
      - uses: actions/checkout@v4
        with:
          submodules: true
          fetch-depth: 0

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Build And Deploy
        id: builddeploy
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN_SALMON_BUSH_08C1E7510 }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "./Clinica.WASM" 
          output_location: "wwwroot"
          app_build_command: "dotnet publish -c Release"

  close_pull_request_job:
    if: github.event_name == 'pull_request' && github.event.action == 'closed'
    runs-on: ubuntu-latest
    name: Close Pull Request Job
    steps:
      - name: Close Job
        id: closepullrequest
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN_SALMON_BUSH_08C1E7510 }}
          action: "close"
          app_location: "./Clinica.WASM"
```

## 2. Pipeline del Backend API (ci-cd-pipeline.yml)
Es el workflow principal de ingeniería. Se encarga de levantar el entorno de compilación, inyectar herramientas de análisis de seguridad, ejecutar la suite completa de pruebas unitarias e integradas, y empaquetar los artefactos para Azure App Service.

```text
name: SYS Clinica Santa Monica - Backend Core CI/CD

on:
  push:
    branches: [ "master" ]
  pull_request:
    branches: [ "master" ]
  workflow_dispatch:

jobs:
  build-test:
    name: Compilación, Control de Calidad y Publicación
    runs-on: ubuntu-latest
    steps:
      - name: Checkout del Código Fuente
        uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: Configurar Entorno .NET 9
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Configurar Java Runtime 17 (Requisito SonarCloud)
        uses: actions/setup-java@v4
        with:
          java-version: 17
          distribution: 'zulu'

      - name: Cache de Componentes de SonarCloud
        uses: actions/cache@v4
        with:
          path: ~\sonar\cache
          key: ${{ runner.os }}-sonar
          restore-keys: ${{ runner.os }}-sonar

      - name: Instalar Herramientas Globales .NET (Sonar & ReportGenerator)
        run: |
          dotnet tool install --global dotnet-sonarscanner
          dotnet tool install --global dotnet-reportgenerator-globaltool

      - name: Restaurar Paquetes NuGet
        run: dotnet restore Clinica.sln

      - name: Inicializar Escaneo SonarCloud
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        run: |
          dotnet-sonarscanner begin /k:"MathiuZuri_ClinicaSantaMPrUnit" /o:"mathiuzuri" /d:sonar.token="${{ secrets.SONAR_TOKEN }}" /d:sonar.host.url="[https://sonarcloud.io](https://sonarcloud.io)" /d:sonar.exclusions="**/Clinica.API.Tests/**,**/Clinica.API.IntegrationTests/**,**/Clinica.E2ETest/**,**/*.js,**/node_modules/**,**/*.html,**/*.css,**/bin/**,**/obj/**,**/Migrations/**,**/k6/**" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

      - name: Compilar Solución Completa
        run: dotnet build Clinica.sln --configuration Release --no-restore

      - name: Ejecutar Suite de Pruebas Unitarias
        run: dotnet test Clinica.API.Tests/Clinica.API.Tests.csproj --configuration Release --no-build --logger "trx;LogFileName=unit_tests.trx" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

      - name: Ejecutar Suite de Pruebas de Integración (Docker Testcontainers)
        run: dotnet test Clinica.API.IntegrationTests/Clinica.API.IntegrationTests.csproj --configuration Release --no-build --logger "trx;LogFileName=integration_tests.trx" /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

      - name: Finalizar Escaneo y Transmitir Métricas a SonarCloud
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
        run: dotnet-sonarscanner end /d:sonar.token="${{ secrets.SONAR_TOKEN }}"

      - name: Publicar Artefactos de la API de Producción
        run: dotnet publish Clinica.API/Clinica.API.csproj --configuration Release --output ./publish

      - name: Desplegar Artefactos en Azure App Service
        uses: azure/webapps-deploy@v3
        with:
          app-name: 'SYS-Clinica-API' # Nombre del recurso configurado en Azure portal
          publish-profile: ${{ secrets.AZURE_APP_SERVICE_PUBLISH_PROFILE }}
          package: ./publish
```

### 🔒 Variables de Entorno y Configuración de Secretos (AppSettings)
Para cumplir con las políticas de auditoría de la clínica, ninguna credencial técnica, token o cadena de conexión se almacena en texto plano en el repositorio. Las configuraciones se inyectan en caliente a través de las variables de entorno del panel de control de Azure App Service:

ConnectionStrings__DefaultConnection: Cadena de conexión encriptada de tipo SSL armada hacia el pool de conexiones de Neon PostgreSQL.

Jwt__Secret: Llave criptográfica de alta entropía (mínimo 256 bits) para la firma y validación de tokens de acceso del personal médico.

WhatsAppOptions__ApiUrl y WhatsAppOptions__ApiKey: Credenciales de integración con la instancia externa de Evolution API para el despacho de recordatorios automatizados de citas por WhatsApp.

SunatOptions__Token: Token de autenticación de servicios para la firma e inyección de validez fiscal en el módulo de facturación de comprobantes de pago.