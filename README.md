# Euphoria Essence

Aplicación de perfumería desarrollada como proyecto académico de Programación Móvil. Permite consultar un catálogo de perfumes, administrar un carrito, realizar pedidos y gestionar productos y pedidos desde una cuenta de administrador.

La solución integra una aplicación **.NET MAUI Blazor Hybrid**, una **API REST con ASP.NET Core** y una base de datos relacional **SQL Server**. Las pantallas toman como referencia el prototipo diseñado en Figma.

## Estado del proyecto

Prototipo funcional en etapa de pruebas y ajustes visuales. Incluye los módulos de autenticación, catálogo, carrito, pedidos, perfil y administración. La documentación académica y la entrega de la aplicación Android se completarán junto con las pruebas finales.

La plataforma utilizada durante las pruebas es Android. El proyecto también declara destinos para Windows, iOS y Mac Catalyst; su presencia en la configuración no implica que hayan sido probados.

## Tecnologías

| Componente | Tecnología |
| --- | --- |
| Aplicación | .NET MAUI Blazor Hybrid, .NET 10 |
| Interfaz | Componentes Razor, HTML, CSS y SVG |
| Comunicación | HttpClient y JSON sobre HTTP/HTTPS |
| Backend | ASP.NET Core Web API, .NET 10 |
| Acceso a datos | Entity Framework Core 10, enfoque Database First |
| Base de datos | SQL Server |
| Autenticación | JWT Bearer y autorización por roles |
| Contraseñas | PasswordHasher de ASP.NET Core Identity |
| Sesión móvil | SecureStorage de .NET MAUI |
| Documentación de API | OpenAPI y Swagger UI en Development |
| Hosting | MonsterASP |
| Diseño de referencia | Figma |

## Funcionalidades

### Cuenta y sesión

- Registro de clientes e inicio de sesión con correo y contraseña.
- Autenticación mediante token JWT con vencimiento.
- Restauración de la sesión guardada en el dispositivo y cierre de sesión.
- Acceso a las funciones correspondientes al rol Cliente o Administrador.
- Consulta del perfil y edición de nombre, apellido, teléfono y dirección.

El correo y la contraseña no se modifican desde el formulario de edición del perfil actual.

### Cliente

- Consultar el catálogo y buscar perfumes por nombre o marca.
- Filtrar por marca y categoría mediante opciones con desplazamiento horizontal.
- Consultar imágenes, descripción, concentración, tamaño, precio y disponibilidad.
- Agregar productos al carrito, cambiar cantidades y eliminar artículos.
- Consultar subtotales y total de la compra.
- Confirmar la dirección de envío y realizar un pedido.
- Consultar los pedidos propios y sus detalles.

Las categorías utilizadas son **Masculino, Femenino y Unisex**. Las referencias a perfumes árabes, de diseñador o de nicho se incluyen en la descripción.

### Administrador

- Consultar y filtrar el listado administrativo de perfumes.
- Crear perfumes y editar sus datos.
- Cargar imágenes JPG o PNG desde el dispositivo.
- Retirar perfumes del catálogo mediante eliminación lógica.
- Actualizar el stock desde la pantalla de detalle del perfume.
- Consultar pedidos, revisar sus detalles y actualizar su estado.

En el detalle, el cliente dispone de **Agregar al carrito** y el administrador de **Actualizar stock**. El stock se guarda con su operación específica, separada de la edición general del producto.

## Reglas de negocio

- El catálogo público muestra productos activos cuya marca y categoría también estén activas.
- Retirar un perfume establece su estado como inactivo y conserva el registro en la base de datos.
- Actualizar el stock no reactiva un producto retirado.
- Los productos retirados pueden aparecer en el listado administrativo sin imagen.
- El carrito y los pedidos del cliente requieren autenticación con el rol correspondiente.
- Al realizar un pedido, la API valida disponibilidad y stock, descuenta las unidades y vacía el carrito dentro de una transacción.
- El precio y el stock se validan en el servidor; el frontend no es la autoridad de esos datos.
- La API publicada ofrece consulta de marcas y categorías. Su alta y mantenimiento se realizan actualmente en SQL Server, sin una pantalla administrativa ni endpoints de escritura para esos catálogos.

Los estados de pedido siguen esta secuencia:

**Aprobado → Preparando pedido → Despachado → En camino → Llegó a sucursal → Entregado**

## Organización de la solución

| Ruta | Contenido |
| --- | --- |
| `EuphoriaEssence.slnx` | Solución de Visual Studio |
| `EuphoriaEssence.App/Components/Pages/` | Pantallas Razor y sus estilos aislados |
| `EuphoriaEssence.App/Components/Layout/` | Contenedor visual compartido |
| `EuphoriaEssence.App/Models/` | Modelos utilizados por la aplicación |
| `EuphoriaEssence.App/Services/SesionService.cs` | Inicio, restauración y cierre de sesión |
| `EuphoriaEssence.App/MauiProgram.cs` | Servicios y dirección base de la API |
| `EuphoriaEssence.App/wwwroot/` | HTML anfitrión, CSS y recursos web |
| `EuphoriaEssence.Api/Controllers/` | Endpoints de la API |
| `EuphoriaEssence.Api/DTOs/` | Contratos y validaciones de entrada/salida |
| `EuphoriaEssence.Api/Models/` | Entidades de la base de datos |
| `EuphoriaEssence.Api/Data/` | DbContext y configuración de EF Core |
| `EuphoriaEssence.Api/Services/` | JWT, manejo de errores y configuración OpenAPI |
| `EuphoriaEssence.Api/wwwroot/uploads/` | Imágenes cargadas de los productos |

## Base de datos

El modelo contiene once entidades:

| Entidad | Responsabilidad |
| --- | --- |
| Roles | Roles de acceso |
| Usuarios | Cuentas y datos de perfil |
| Marcas | Marcas de perfumes |
| Categorias | Clasificación de productos |
| Productos | Datos, precio, presentación, stock y estado |
| ImagenesProducto | Imágenes y selección de imagen principal |
| Carritos | Carrito asociado al usuario |
| DetalleCarrito | Productos y cantidades del carrito |
| EstadosPedido | Estados disponibles para los pedidos |
| Pedidos | Usuario, fecha, dirección y estado del pedido |
| DetallePedido | Productos, cantidades y precios de la compra |

En este modelo, el tamaño y la concentración forman parte del producto. La documentación debe reflejar este esquema y no el de otros proyectos web de la misma marca.

El código incluye las entidades y el mapeo de EF Core, pero **el ZIP revisado no incluye un script SQL de creación ni una copia de respaldo de la base**. Para instalar el backend desde cero debe incorporarse un script que reproduzca el esquema, las restricciones y los datos iniciales necesarios. No se debe asumir que las tablas se crean automáticamente al ejecutar la API.

## Ejecutar la aplicación con la API publicada

Esta opción permite probar el frontend sin ejecutar el backend localmente.

1. Instalar una versión de Visual Studio compatible con .NET 10 y la carga de trabajo de .NET MAUI, incluido el SDK de Android.
2. Clonar el repositorio y abrir `EuphoriaEssence.slnx`.
3. Restaurar los paquetes NuGet.
4. Establecer `EuphoriaEssence.App` como proyecto de inicio.
5. Seleccionar el destino Android y un emulador o dispositivo físico con depuración USB.
6. Ejecutar la aplicación con acceso a Internet.

La dirección configurada en `MauiProgram.cs` es:

```csharp
BaseAddress = new Uri("https://euphoriaessence-api.runasp.net/")
```

El proyecto establece Android 7.0 / API 24 como versión mínima. La disponibilidad de la API depende del servicio de hosting.

Para las pruebas como cliente puede utilizarse el registro de la aplicación. La cuenta de administrador debe prepararse en la base de datos con el rol correspondiente; el registro público crea clientes.

Las credenciales de las cuentas de prueba se entregan por separado y no forman parte de este README.

## Ejecutar la API localmente

### Requisitos

- SDK de .NET 10.
- SQL Server y una herramienta de administración, como SQL Server Management Studio.
- Base de datos con el esquema del proyecto y datos iniciales de roles, marcas, categorías y estados de pedido.

Los siguientes comandos se ejecutan desde la carpeta que contiene `EuphoriaEssence.slnx`.

### Configuración

La API utiliza estas claves:

| Clave | Valor esperado |
| --- | --- |
| `ConnectionStrings:EuphoriaEssenceDB` | Cadena de conexión a SQL Server |
| `Jwt:Key` | Clave privada de firma de al menos 32 bytes UTF-8 |
| `Jwt:Issuer` | Emisor de los tokens |
| `Jwt:Audience` | Audiencia de los tokens |

En desarrollo, guardar los valores privados mediante User Secrets. Reemplazar los textos de ejemplo antes de ejecutar:

```powershell
dotnet user-secrets set "ConnectionStrings:EuphoriaEssenceDB" "Server=SERVIDOR;Database=BASE_DE_DATOS;Trusted_Connection=True;TrustServerCertificate=True;" --project EuphoriaEssence.Api

dotnet user-secrets set "Jwt:Key" "REEMPLAZAR_POR_UNA_CLAVE_PRIVADA_ALEATORIA_DE_AL_MENOS_32_BYTES" --project EuphoriaEssence.Api

dotnet user-secrets set "Jwt:Issuer" "EuphoriaEssence.Api" --project EuphoriaEssence.Api

dotnet user-secrets set "Jwt:Audience" "EuphoriaEssence.App" --project EuphoriaEssence.Api
```

El proyecto ya declara un `UserSecretsId`. Los valores del ejemplo son parámetros de configuración, no credenciales de la base hosteada.

Para un servidor, configurar los valores privados mediante el mecanismo del hosting; las claves equivalentes para variables de entorno son `ConnectionStrings__EuphoriaEssenceDB`, `Jwt__Key`, `Jwt__Issuer` y `Jwt__Audience`.

### Inicio

```powershell
dotnet restore EuphoriaEssence.Api/EuphoriaEssence.Api.csproj
dotnet run --project EuphoriaEssence.Api --launch-profile https
```

Direcciones del perfil HTTPS incluido:

- API local: `https://localhost:7266/`
- Swagger UI: `https://localhost:7266/swagger`
- Documento OpenAPI: `https://localhost:7266/openapi/v1.json`

Swagger UI y OpenAPI se habilitan únicamente en el entorno `Development`.

La app está configurada para consumir la API hosteada. Si se cambia a una API local, hay que ajustar `BaseAddress` y configurar la conectividad y la confianza del certificado desde el dispositivo. `localhost` dentro de Android se refiere al propio dispositivo, no a la computadora que ejecuta la API.

## Endpoints principales

`{id}` representa el identificador del recurso y `{idProducto}` el del perfume.

| Método | Ruta | Acceso / función |
| --- | --- | --- |
| POST | `/api/auth/registro` | Registro de cliente |
| POST | `/api/auth/login` | Inicio de sesión |
| GET | `/api/catalogo/marcas` | Marcas activas |
| GET | `/api/catalogo/categorias` | Categorías activas |
| GET | `/api/productos` | Catálogo público paginado |
| GET | `/api/productos/{id}` | Detalle público e imágenes |
| GET / PUT | `/api/perfil` | Consultar o editar el perfil autenticado |
| GET / DELETE | `/api/carrito` | Consultar o vaciar el carrito del cliente |
| POST | `/api/carrito/items` | Agregar un producto |
| PUT / DELETE | `/api/carrito/items/{idProducto}` | Cambiar cantidad o eliminar un artículo |
| POST / GET | `/api/pedidos` | Crear un pedido o consultar pedidos propios |
| GET | `/api/pedidos/{id}` | Detalle de un pedido propio |
| GET / POST | `/api/admin/productos` | Listar o crear productos como administrador |
| GET / PUT / DELETE | `/api/admin/productos/{id}` | Consultar, editar o retirar un producto |
| PATCH | `/api/admin/productos/{id}/stock` | Actualizar stock |
| POST | `/api/admin/productos/{idProducto}/imagenes` | Subir una imagen |
| GET | `/api/admin/pedidos` | Listar pedidos como administrador |
| GET | `/api/admin/pedidos/estados` | Consultar estados de pedido |
| GET | `/api/admin/pedidos/{id}` | Detalle administrativo del pedido |
| PATCH | `/api/admin/pedidos/{id}/estado` | Actualizar estado del pedido |
| GET | `/api/estado` | Estado general de la API |

Las operaciones protegidas reciben el encabezado `Authorization: Bearer <token>`. Los contratos completos y validaciones pueden consultarse en los DTO y en Swagger durante el desarrollo.

## Documentación académica

Se prevé incorporar la documentación en una carpeta `docs/`. Esta organización es una propuesta para la entrega; esos archivos todavía no forman parte del ZIP revisado.

| Material | Contenido |
| --- | --- |
| Descripción del proyecto | Problema, objetivos, alcance y tecnologías |
| Modelo de datos | DER, tablas, relaciones y restricciones |
| Casos de uso | Acciones del cliente y del administrador |
| Prototipo Figma | Pantallas y navegación entre ellas |
| Arquitectura | Relación entre app, API y SQL Server |
| API | Autenticación, endpoints, contratos y respuestas |
| Pruebas | Casos ejecutados, resultados y evidencias |
| Despliegue | Hosting, configuración y proceso de publicación |
| Manual de usuario | Uso de la aplicación por rol |

Las capturas y los diagramas deben corresponder a la versión final implementada. El material teórico de la cátedra puede incorporarse como referencia, diferenciado de la documentación propia del sistema y con su fuente identificada.

## Pruebas para la entrega

La siguiente lista sirve para registrar la validación final; no implica que todas las pruebas estén completadas:

- Registro, login, cierre y restauración de sesión.
- Acceso por rol y manejo de sesión vencida.
- Búsqueda, filtros y detalle de productos.
- Imágenes y presentación visual respecto del Figma.
- Carrito: agregar, cambiar cantidades, eliminar y vaciar.
- Compra con stock disponible y rechazo cuando no alcanza.
- Creación y consulta de pedidos, y actualización de estados por el administrador.
- Creación, edición, carga de imágenes y retiro lógico de perfumes.
- Actualización de stock desde el detalle administrativo.
- Consulta y edición del perfil.
- Mensajes ante errores de conexión y respuestas de la API.
- Separación del contenido respecto de la barra de estado y navegación de Android.

## Preparación del repositorio

El ZIP de trabajo contiene archivos de compilación y configuración local que no deben subirse como parte del código fuente. Antes del primer commit:

- Excluir `bin/`, `obj/`, `.vs/`, `*.user` y perfiles de publicación con información privada.
- Revisar `appsettings.json`, `appsettings.Development.json` y archivos `.http` para retirar contraseñas, tokens o cadenas de conexión privadas.
- Conservar en el repositorio únicamente configuración de ejemplo sin secretos.
- Incorporar el script SQL del esquema y los datos iniciales necesarios, sin datos personales ni credenciales reales.
- Incorporar las imágenes de ejemplo solo cuando se cuente con autorización para utilizarlas.
- Publicar el APK o AAB de entrega como un artefacto de una versión de GitHub cuando esté generado y probado.

## Alcance y limitaciones actuales

- La app consume por defecto la API publicada en MonsterASP.
- No incluye pagos en línea ni integración con una empresa de envíos.
- No incluye mantenimiento de marcas y categorías desde la app.
- La API actual no ofrece una operación de reactivación de productos retirados; actualizar stock o editar datos no cambia ese estado.
- No se incluyen en esta entrega del código un script SQL de instalación, documentación académica final ni un APK/AAB de distribución preparado para la entrega.
- El aspecto visual y el comportamiento en dispositivos siguen sujetos a la validación final del prototipo.

## Autor

**Luca Fanjul** — Proyecto académico de Programación Móvil.
