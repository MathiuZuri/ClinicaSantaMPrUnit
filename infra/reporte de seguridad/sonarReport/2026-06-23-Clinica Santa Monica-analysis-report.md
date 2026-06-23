# Code analysis
## Clinica Santa Monica 
#### Branch main
#### Version not provided 

**By: default**

*Date: 2026-06-23*

*Analyzed the: 2026-06-23*

## Introduction
This document contains results of the code analysis of Clinica Santa Monica



## Configuration

- Quality Profiles
    - Names: Sonar way [C#]; Sonar way [Docker]; 
    - Files: 1d2cfb83-a2b5-4622-9b8a-27bbfa08c993.json; c98aef37-a5d6-4a99-bb05-18247310e950.json; 


 - Quality Gate
    - Name: Sonar way
    - File: Sonar way.xml

## Synthesis

### Analysis Status

Reliability | Security | Security Review | Maintainability |
:---:|:---:|:---:|:---:
A | A | A | A |

### Quality gate status

| Quality Gate Status | ERROR |
|-|-|

Metric|Value
---|---
Coverage on New Code|OK
Duplicated Lines (%) on New Code|ERROR (7.1% is greater than 3%)
Security Hotspots Reviewed on New Code|OK
New Issues|ERROR (129 is greater than 0)


### Metrics

Coverage | Duplications | Comment density | Median number of lines of code per file | Adherence to coding standard |
:---:|:---:|:---:|:---:|:---:
82.1 % | 7.4 % | 5.6 % | 29.0 | 98.9 %

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
Max | 3730.0 | 1493.0 | 10789.0 | 100.0 | 41.1 | 95.6

### Volume

Language|Number
---|---
C#|23151
Docker|22
Total|23173


## Issues

### Issues count by severity and types

Type / Severity|INFO|MINOR|MAJOR|CRITICAL|BLOCKER
---|---|---|---|---|---
BUG|0|0|0|0|0
VULNERABILITY|0|0|0|0|0
CODE_SMELL|52|59|74|17|0


### Issues List

Name|Description|Type|Severity|Number
---|---|---|---|---
Properties should not make collection or array copies||CODE_SMELL|CRITICAL|1
Cognitive Complexity of methods should not be too high||CODE_SMELL|CRITICAL|5
Unread "private" fields should be removed||CODE_SMELL|CRITICAL|11
Track uses of "TODO" tags||CODE_SMELL|INFO|3
Mergeable "if" statements should be combined||CODE_SMELL|MAJOR|1
Nested blocks of code should not be left empty||CODE_SMELL|MAJOR|2
Utility classes should not have public constructors||CODE_SMELL|MAJOR|1
Unused private types or members should be removed||CODE_SMELL|MAJOR|2
Sections of code should not be commented out||CODE_SMELL|MAJOR|4
Ternary operators should not be nested||CODE_SMELL|MAJOR|4
Custom attributes should be marked with "System.AttributeUsageAttribute"||CODE_SMELL|MAJOR|1
Always set the "DateTimeKind" when creating new "DateTime" instances||CODE_SMELL|MAJOR|1
Use a format provider when parsing date and time||CODE_SMELL|MAJOR|1
Controllers should not have mixed responsibilities||CODE_SMELL|MAJOR|1
URIs should not be hardcoded||CODE_SMELL|MINOR|1
String literals should not be duplicated||CODE_SMELL|MINOR|24
Unused local variables should be removed||CODE_SMELL|MINOR|2
Classes should not be empty||CODE_SMELL|MINOR|5
Methods and properties that don't access instance data should be static||CODE_SMELL|MINOR|20
Generic exceptions should not be ignored||CODE_SMELL|MINOR|2
Loops should be simplified with "LINQ" expressions||CODE_SMELL|MINOR|1
Jump statements should not be redundant||CODE_SMELL|MINOR|1
Method overloads should be grouped together||CODE_SMELL|MINOR|1
Prefer indexing instead of "Enumerable" methods on types implementing "IList"||CODE_SMELL|MINOR|2
external_roslyn:CS1998|El método asincrónico carece de operadores "await" y se ejecutará de forma sincrónica. Puede usar el operador 'await' para esperar llamadas API que no sean de bloqueo o 'await Task.Run(...)' para hacer tareas enlazadas a la CPU en un subproceso en segundo plano.|CODE_SMELL|MAJOR|14
external_roslyn:CS0105|La directiva using para 'Clinica.WASM.DTOs.Permisos' aparece previamente en este espacio de nombres|CODE_SMELL|MAJOR|1
external_roslyn:CA1862|Prefiera la sobrecarga de método de comparación de cadenas de "string.Contains(string)" que toma un valor de enumeración "StringComparison" para realizar una comparación sin distinción de mayúsculas y minúsculas, pero tenga en cuenta que esto puede provocar cambios sutiles en el comportamiento, por lo que asegúrese de realizar pruebas exhaustivas después de aplicar la sugerencia, o si no se requiere una comparación culturalmente confidencial, considere la posibilidad de usar "StringComparison.OrdinalIgnoreCase"|CODE_SMELL|INFO|15
external_roslyn:CA1860|Es preferible comparar "Count" con 0 en lugar de usar "Any()", tanto por claridad como por rendimiento.|CODE_SMELL|INFO|18
external_roslyn:CS8625|No se puede convertir un literal NULL en un tipo de referencia que no acepta valores NULL.|CODE_SMELL|MAJOR|1
external_roslyn:CS8604|Posible argumento de referencia nulo para el parámetro "logo" en "void CertificadoTrabajoPdfService.ConstruirEncabezado(IContainer container, byte[] logo)".|CODE_SMELL|MAJOR|5
external_roslyn:CS8602|Desreferencia de una referencia posiblemente NULL.|CODE_SMELL|MAJOR|17
external_roslyn:CS8600|Se va a convertir un literal nulo o un posible valor nulo en un tipo que no acepta valores NULL|CODE_SMELL|MAJOR|5
external_roslyn:CS0414|El campo 'PagosPage.EstaCargandoPacientes' está asignado pero su valor nunca se usa|CODE_SMELL|MAJOR|2
external_roslyn:MUD0002|Illegal Attribute 'Loading' on 'MudStepper' using pattern 'LowerCase' source location '(424,20)-(432,21)'|CODE_SMELL|MAJOR|11
external_roslyn:CA1822|El miembro "TasaIgvActiva" no tiene acceso a los datos de la instancia y se puede marcar como static.|CODE_SMELL|INFO|14
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


