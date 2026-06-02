# Code analysis
## Clinica Santa Monica 
#### Branch main
#### Version not provided 

**By: default**

*Date: 2026-06-02*

*Analyzed the: 2026-05-31*

## Introduction
This document contains results of the code analysis of Clinica Santa Monica



## Configuration

- Quality Profiles
    - Names: Sonar way [C#]; Sonar way [CSS]; Sonar way [Docker]; Sonar way [JavaScript]; Sonar way [HTML]; 
    - Files: 1d2cfb83-a2b5-4622-9b8a-27bbfa08c993.json; 2c1ec84c-6232-4ea1-8746-7c48c44dccbf.json; c98aef37-a5d6-4a99-bb05-18247310e950.json; 5905777f-1fea-4b5f-bd99-4dad7cd49443.json; 44225260-8deb-4e20-8ed5-c5611dde0e19.json; 


 - Quality Gate
    - Name: Sonar way
    - File: Sonar way.xml

## Synthesis

### Analysis Status

Reliability | Security | Security Review | Maintainability |
:---:|:---:|:---:|:---:
A | A | A | A |

### Quality gate status

| Quality Gate Status | OK |
|-|-|

Metric|Value
---|---
Coverage on New Code|OK
Duplicated Lines (%) on New Code|OK
Security Hotspots Reviewed on New Code|OK
New Issues|OK


### Metrics

Coverage | Duplications | Comment density | Median number of lines of code per file | Adherence to coding standard |
:---:|:---:|:---:|:---:|:---:
0.0 % | 4.6 % | 1.6 % | 31.0 | 99.1 %

### Tests

Total | Success Rate | Skipped | Errors | Failures |
:---:|:---:|:---:|:---:|:---:
0 | 0 % | 0 | 0 | 0

### Detailed technical debt

Reliability|Security|Maintainability|Total
---|---|---|---
-|-|1d 1h 42min|1d 1h 42min


### Metrics Range

\ | Cyclomatic Complexity | Cognitive Complexity | Lines of code per file | Coverage | Comment density (%) | Duplication (%)
:---|:---:|:---:|:---:|:---:|:---:|:---:
Min | 0.0 | 0.0 | 2.0 | 0.0 | 0.0 | 0.0
Max | 2433.0 | 827.0 | 10703.0 | 0.0 | 24.7 | 93.1

### Volume

Language|Number
---|---
C#|15479
CSS|3200
Docker|22
JavaScript|51
HTML|214
Total|18966


## Issues

### Issues count by severity and types

Type / Severity|INFO|MINOR|MAJOR|CRITICAL|BLOCKER
---|---|---|---|---|---
BUG|0|0|0|0|0
VULNERABILITY|0|0|0|0|0
CODE_SMELL|5|50|39|2|0


### Issues List

Name|Description|Type|Severity|Number
---|---|---|---|---
Cognitive Complexity of methods should not be too high||CODE_SMELL|CRITICAL|1
Unread "private" fields should be removed||CODE_SMELL|CRITICAL|1
Mergeable "if" statements should be combined||CODE_SMELL|MAJOR|1
Methods should not have too many parameters||CODE_SMELL|MAJOR|1
Nested blocks of code should not be left empty||CODE_SMELL|MAJOR|2
Utility classes should not have public constructors||CODE_SMELL|MAJOR|1
Unused private types or members should be removed||CODE_SMELL|MAJOR|1
Custom attributes should be marked with "System.AttributeUsageAttribute"||CODE_SMELL|MAJOR|1
Use a format provider when parsing date and time||CODE_SMELL|MAJOR|1
Text and background colors should have sufficient contrast||CODE_SMELL|MAJOR|1
URIs should not be hardcoded||CODE_SMELL|MINOR|12
String literals should not be duplicated||CODE_SMELL|MINOR|21
Unused local variables should be removed||CODE_SMELL|MINOR|2
Classes should not be empty||CODE_SMELL|MINOR|5
Methods and properties that don't access instance data should be static||CODE_SMELL|MINOR|1
Generic exceptions should not be ignored||CODE_SMELL|MINOR|2
Loops should be simplified with "LINQ" expressions||CODE_SMELL|MINOR|1
Prefer indexing instead of "Enumerable" methods on types implementing "IList"||CODE_SMELL|MINOR|1
Use "globalThis" instead of "window", "self", or "global"||CODE_SMELL|MINOR|5
external_roslyn:CS1998|El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica. Puede usar el operador 'await' para esperar llamadas API que no sean de bloqueo o 'await Task.Run(...)' para hacer tareas enlazadas a la CPU en un subproceso en segundo plano.|CODE_SMELL|MAJOR|11
external_roslyn:CS0105|La directiva using para 'Clinica.WASM.DTOs.Permisos' aparece previamente en este espacio de nombres|CODE_SMELL|MAJOR|1
external_roslyn:CA1862|Prefiera usar "string.Equals(string, StringComparison)" para realizar una comparación sin distinción de mayúsculas y minúsculas, pero tenga en cuenta que esto puede provocar cambios sutiles en el comportamiento, por lo que asegúrese de realizar pruebas exhaustivas después de aplicar la sugerencia, o si no se requiere una comparación culturalmente confidencial, considere la posibilidad de usar "StringComparison.OrdinalIgnoreCase"|CODE_SMELL|INFO|1
external_roslyn:CA1860|Es preferible comparar "Count" con 0 en lugar de usar "Any()", tanto por claridad como por rendimiento.|CODE_SMELL|INFO|1
external_roslyn:CS8602|Desreferencia de una referencia posiblemente NULL.|CODE_SMELL|MAJOR|12
external_roslyn:CS0414|El campo 'PermisosPage.EstaCargando' está asignado pero su valor nunca se usa|CODE_SMELL|MAJOR|1
external_roslyn:MUD0002|Illegal Attribute 'Align' on 'MudTh' using pattern 'LowerCase' source location '(525,16)-(525,76)'|CODE_SMELL|MAJOR|5
external_roslyn:CA1822|El miembro "ObtenerMensajeErrorAsync" no tiene acceso a los datos de la instancia y se puede marcar como static.|CODE_SMELL|INFO|1
external_roslyn:CA1869|Evite crear una nueva instancia de "JsonSerializerOptions" para cada operación de serialización. En su lugar, almacene en caché y reutilice instancias.|CODE_SMELL|INFO|2


## Security Hotspots

### Security hotspots count by category and priority

Category / Priority|LOW|MEDIUM|HIGH
---|---|---|---
LDAP Injection|0|0|0
Object Injection|0|0|0
Server-Side Request Forgery (SSRF)|0|0|0
XML External Entity (XXE)|0|0|0
Insecure Configuration|0|0|0
XPath Injection|0|0|0
Authentication|0|0|0
Weak Cryptography|0|0|0
Denial of Service (DoS)|0|0|0
Log Injection|0|0|0
Cross-Site Request Forgery (CSRF)|0|0|0
Open Redirect|0|0|0
Permission|0|0|0
SQL Injection|0|0|0
Encryption of Sensitive Data|0|0|0
Traceability|0|0|0
Buffer Overflow|0|0|0
File Manipulation|0|0|0
Code Injection (RCE)|0|0|0
Cross-Site Scripting (XSS)|0|0|0
Command Injection|0|0|0
Path Traversal Injection|0|0|0
HTTP Response Splitting|0|0|0
Others|0|0|0


### Security hotspots


