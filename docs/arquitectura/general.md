# 🗺️ Visión General de la Arquitectura

El sistema **SIGEC** (Sistema de Gestión de Clínica) para la **Clínica Santa Mónica** está diseñado e implementado bajo los fundamentos de la **Arquitectura Hexagonal** (también conocida como el patrón de *Puertos y Adaptadores*) y los principios del **Diseño Guiado por el Dominio (DDD)**. 

El objetivo principal de esta arquitectura es la **separación estricta de responsabilidades**, aislando por completo la lógica de negocio pura (ginecobstétrica y recaudación) de cualquier acoplamiento con frameworks, bases de datos o clientes web.



---

## 🧅 La Regla de Dependencia Dependiente

La arquitectura se visualiza como una serie de capas concéntricas (estilo cebolla) donde **las dependencias fluyen estrictamente hacia adentro**. Las capas externas conocen a las internas, pero las internas jamás tienen conocimiento, referencia directa o dependencia de las capas que las rodean.

```text
  ┌─────────────────────────────────────────────────────────────┐
  │                   ADAPTADORES DE ENTRADA                    │
  │          [Clinica.WASM] ➔ [Clinica.API (Controllers)]      │
  └──────────────────────────────┬──────────────────────────────┘
                                 │ (Invocación)
                                 ▼
                    ┌──────────────────────────┐
                    │     PUERTOS DE ENTRADA   │
                    │   (Interfaces de Application/Domain)
                    └────────────┬─────────────┘
                                 │
                                 ▼
                  ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
                  ┃   NÚCLEO DE DOMINIO (CORE)   ┃
                  ┃    Entidades, Enums, Reglas  ┃
                  ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
                                 ▲
                                 │
                    ┌────────────┴─────────────┐
                    │     PUERTOS DE SALIDA    │
                     (Interfaces de Repositorio)
                    └────────────┬─────────────┘
                                 │ (Inversión de Control)
                                 ▼
  ┌─────────────────────────────────────────────────────────────┐
  │                    ADAPTADORES DE SALIDA                    │
  │     [Clinica.Infrastructure (EF Core 9, Neon, QuestPDF)]    │
  └─────────────────────────────────────────────────────────────┘