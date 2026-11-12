# Funko API

API REST desarrollada con **ASP.NET Core y .NET 10** para la gestión de una colección de figuras Funko. El proyecto implementa una arquitectura por capas, diferentes sistemas de almacenamiento, autenticación mediante JWT e integraciones adicionales para ampliar sus funcionalidades.

Desarrollado por **Kevin Zelaya y Sergio Burda** como práctica de la asignatura Desarrollo Web en Entorno Servidor del IES Luis Vives.

## Tecnologías utilizadas

| Tecnología | Uso |
|---|---|
| .NET 10 | Framework principal |
| ASP.NET Core | Desarrollo de la API |
| Entity Framework Core | Acceso y persistencia de datos |
| PostgreSQL | Base de datos relacional |
| MongoDB | Almacenamiento de proveedores |
| Redis | Sistema de caché |
| Cassandra | Almacenamiento del historial de precios |
| JWT | Autenticación y autorización |
| BCrypt | Protección de contraseñas |
| NUnit | Pruebas automatizadas |
| Moq | Simulación de dependencias |
| Testcontainers | Pruebas de integración |
| Serilog | Registro de eventos y errores |
| Docker | Contenerización |

## Funcionalidades principales

- **Autenticación y autorización:** registro e inicio de sesión mediante JWT, con roles `ADMIN` y `USER`.
- **Gestión de Funkos:** operaciones CRUD, búsqueda, filtrado y paginación.
- **Gestión de categorías:** creación, consulta, actualización, eliminación y restauración.
- **Gestión de proveedores:** administración de proveedores almacenados en MongoDB.
- **Control de concurrencia:** utilización de `RowVersion` para evitar sobrescrituras de datos.
- **Almacenamiento de imágenes:** gestión de imágenes mediante el sistema de archivos.
- **Caché distribuida:** Redis para reducir consultas a las bases de datos.
- **Historial de precios:** almacenamiento y consulta de la evolución de precios mediante Cassandra.

## Integraciones adicionales

### GraphQL

Implementación mediante **HotChocolate** para realizar consultas personalizadas sobre Funkos, categorías, proveedores e historial de precios.

Incluye consultas, paginación, filtrado, ordenación y resolvers para recuperar información relacionada. Se utiliza para operaciones de lectura, manteniendo las modificaciones mediante REST.

### SignalR / WebSockets

Comunicación bidireccional en tiempo real mediante SignalR.

Componentes principales:
- `FunkoHub`: gestiona las conexiones de los clientes.
- `NotificationService`: centraliza el envío de notificaciones.

Permite notificar a los administradores sobre creaciones, actualizaciones, eliminaciones, restauraciones y errores no controlados.

Endpoint del Hub:

`/hubs/funkos`

### Servicio de correo electrónico

Integración mediante **MailKit** para enviar correos electrónicos a través de un servidor SMTP.

Componentes principales:
- `MailKitEmailService`: conexión SMTP y envío de mensajes.
- `SmtpSettings`: configuración del servidor SMTP.
- `EmailQueueService`: gestión de la cola de correos.
- `EmailBackgroundWorker`: procesamiento de correos en segundo plano.

La configuración se centraliza en la sección `Smtp` de `appsettings.json`.

### Swagger / OpenAPI

Documentación interactiva de la API que permite consultar endpoints, parámetros, modelos y respuestas.

Incluye autenticación JWT Bearer para probar los endpoints protegidos desde Swagger UI.

## Arquitectura del proyecto

El proyecto sigue una arquitectura por capas basada en la separación de responsabilidades:

- **Controllers:** reciben las peticiones HTTP y devuelven las respuestas.
- **Services:** contienen la lógica de negocio.
- **Repositories:** gestionan el acceso a las fuentes de datos.
- **Models:** representan las entidades del sistema.
- **DTOs:** definen los datos de entrada y salida.
- **Mappers:** realizan conversiones entre modelos y DTOs.
- **Middleware:** gestiona aspectos transversales, como las excepciones.
- **Storage:** gestiona el almacenamiento de imágenes.
- **Infrastructure:** contiene la configuración e inyección de dependencias.
- **Entities:** contiene los contextos y componentes de persistencia.

## Persistencia de datos

La aplicación utiliza cuatro sistemas de almacenamiento:

| Sistema | Responsabilidad |
|---|---|
| PostgreSQL | Usuarios, Funkos y categorías |
| MongoDB | Proveedores |
| Redis | Caché de consultas |
| Cassandra | Historial de precios |

## Seguridad

La API utiliza autenticación basada en JWT.

Los endpoints protegidos requieren un token Bearer en la cabecera `Authorization`. Además, se aplica autorización por roles para restringir determinadas operaciones a usuarios administradores.

Las contraseñas se protegen mediante BCrypt.

## Pruebas

El proyecto incorpora pruebas unitarias y de integración.

Herramientas utilizadas:

- **NUnit:** estructura y ejecución de pruebas.
- **Moq:** simulación de dependencias.
- **FluentAssertions:** comprobación de resultados.
- **Testcontainers:** ejecución de servicios y bases de datos en contenedores.

Se utiliza el patrón AAA (Arrange, Act, Assert) para organizar las pruebas.

También se utiliza Bruno para realizar pruebas de los endpoints HTTP y automatizar su ejecución mediante Bruno CLI.

## Ejecución del proyecto

### Requisitos previos

- .NET 10 SDK.
- Docker y Docker Compose.
- Git.
- Configuración de las conexiones a las bases de datos y servicios externos mediante los appsettings para cada entorno.

### Clonar el repositorio

```bash
git clone https://github.com/Kevin-Zelaya/DAW-FunkoApi.git
cd FunkoApi
```

### Configurar la aplicación

Configura las conexiones a PostgreSQL, MongoDB, Redis y Cassandra, así como los parámetros de autenticación JWT y el servidor SMTP mediante los appsettings para cada entorno.

Las secciones de los archiovs de configuración se obtienen mediante configuración tipada y se transmiten a toda la aplicación mediante inyeccion de dependencias.


### Ejecutar con Docker Compose

```bash
docker compose up -d --build
```

Para detener los servicios:

```bash
docker compose down
```

### Ejecutar desde .NET

```bash
dotnet restore
dotnet build
$env:ASPNETCORE_ENVIRONMENT="Develpment"; dotnet run
```

La dirección y el puerto de ejecución dependerán de la configuración del proyecto.

## Documentación

La API dispone de documentación interactiva mediante Swagger/OpenAPI.

Desde Swagger es posible consultar los endpoints, revisar los modelos de petición y respuesta y probar las operaciones protegidas mediante JWT.

## Autores

- Kevin Zelaya
- Sergio Burda

**IES Luis Vives — Desarrollo Web en Entorno Servidor**