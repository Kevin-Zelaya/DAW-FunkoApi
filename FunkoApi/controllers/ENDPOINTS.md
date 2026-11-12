# Endpoints — Funkos API

Documentación de los endpoints REST definidos en los controladores `AuthController`, `CategoryController`, `SupplierController`, `UserController` y `FunkoController`.

## Autenticación y permisos

La API utiliza autenticación mediante **JWT**. Los endpoints marcados como **Autenticado** requieren un token válido; los marcados como **ADMIN** requieren el rol `ADMIN`. Los endpoints **Públicos** no tienen el atributo `[Authorize]` en los controladores facilitados.

Para acceder a un endpoint protegido, enviar la cabecera:

```http
Authorization: Bearer <token_jwt>
```

## 1. Autenticación (`/api/auth`)

| Método | Endpoint | Descripción | Acceso |
|---|---|---|---|
| POST | `/api/auth/signup` | Registrar un usuario. | Público |
| POST | `/api/auth/signup/admin` | Registrar un nuevo administrador. | ADMIN |
| POST | `/api/auth/login` | Iniciar sesión y obtener un token JWT. | Público |

## 2. Usuarios (`/api/users`)

| Método | Endpoint | Descripción | Acceso |
|---|---|---|---|
| GET | `/api/users` | Obtener todos los usuarios activos. | ADMIN |
| GET | `/api/users/paginated` | Obtener usuarios paginados. | ADMIN |
| GET | `/api/users/{id:long}` | Consultar un usuario por ID. | ADMIN |
| GET | `/api/users/email/{email}` | Buscar un usuario por correo electrónico. | ADMIN |
| GET | `/api/users/me` | Consultar el perfil propio. | Autenticado |
| PUT | `/api/users/{id:long}` | Actualizar los datos de un usuario. | ADMIN |
| PUT | `/api/users/me` | Actualizar el perfil propio. | Autenticado |
| PATCH | `/api/users/{id:long}/password` | Cambiar contraseña: la propia o cualquiera si es ADMIN. | Autenticado* |
| PATCH | `/api/users/{id:long}/role` | Cambiar el rol de un usuario. | ADMIN |
| DELETE | `/api/users/{id:long}` | Eliminar lógicamente un usuario. | ADMIN |
| DELETE | `/api/users/me` | Eliminar la propia cuenta. | USER |
| PATCH | `/api/users/{id:long}/restore` | Restaurar un usuario eliminado. | ADMIN |

**Paginación:** `pageNumber=1` y `pageSize=10` por defecto.

\* En el cambio de contraseña, un usuario autenticado solo puede modificar la suya, mientras que un administrador puede modificar la de otros usuarios.

## 3. Categorías (`/api/categories`)

| Método | Endpoint | Descripción | Acceso |
|---|---|---|---|
| GET | `/api/categories/all` | Obtener todas las categorías. | Público |
| GET | `/api/categories` | Obtener categorías paginadas. | Público |
| GET | `/api/categories/{id}` | Consultar una categoría por ID. | Público |
| POST | `/api/categories` | Crear una categoría. | ADMIN |
| PUT | `/api/categories/{id}` | Actualizar una categoría. | ADMIN |
| DELETE | `/api/categories/{id:guid}` | Eliminar lógicamente una categoría. | ADMIN |
| PATCH | `/api/categories/{id:guid}/restore` | Restaurar una categoría eliminada. | ADMIN |

**Paginación:** `pageNumber=1` y `pageSize=10` por defecto. Ambos deben ser mayores que cero.

## 4. Proveedores (`/api/suppliers`)

| Método | Endpoint | Descripción | Acceso |
|---|---|---|---|
| GET | `/api/suppliers/all` | Obtener todos los proveedores. | ADMIN |
| GET | `/api/suppliers` | Obtener proveedores paginados. | ADMIN |
| GET | `/api/suppliers/{id}` | Consultar un proveedor por ID. | ADMIN |
| POST | `/api/suppliers` | Crear un proveedor. | ADMIN |
| PUT | `/api/suppliers/{id}` | Actualizar un proveedor. | ADMIN |
| DELETE | `/api/suppliers/{id}` | Eliminar lógicamente un proveedor. | ADMIN |
| PATCH | `/api/suppliers/{id}/restore` | Restaurar un proveedor eliminado. | ADMIN |

**Paginación:** `pageNumber=1` y `pageSize=10` por defecto. Ambos deben ser mayores que cero.


## 5. Funkos (`/api/funkos`)

| Método | Endpoint | Descripción | Acceso |
|---|---|---|---|
| GET | `/api/funkos/all` | Obtener todos los Funkos. | Público |
| GET | `/api/funkos` | Obtener Funkos paginados. | Público |
| GET | `/api/funkos/{id}` | Consultar un Funko por ID. | Público |
| POST | `/api/funkos` | Crear un Funko. | ADMIN |
| PUT | `/api/funkos/{id}` | Actualizar los datos de un Funko. | ADMIN |
| PATCH | `/api/funkos/{id}/category` | Cambiar la categoría de un Funko. | ADMIN |
| PATCH | `/api/funkos/{id}/supplier` | Cambiar el proveedor de un Funko. | ADMIN |
| PATCH | `/api/funkos/{id}/price` | Actualizar el precio y su historial. | ADMIN |
| DELETE | `/api/funkos/{id}` | Eliminar lógicamente un Funko. | ADMIN |
| PATCH | `/api/funkos/{id}/restore` | Restaurar un Funko eliminado. | ADMIN |
| POST | `/api/funkos/{id}/image` | Subir o actualizar la imagen de un Funko. | ADMIN |
| GET | `/api/funkos/{id}/price-history` | Consultar el historial de precios en Cassandra. | ADMIN |
| DELETE | `/api/funkos/{id}/historial-precios` | Eliminar el historial de precios. | ADMIN |
| PATCH | `/api/funkos/{id}/historial-precios` | Restaurar el historial de precios. | ADMIN |

**Paginación:** `pageNumber=1` y `pageSize=5` por defecto. Ambos deben ser mayores que cero.

**Imágenes:** `POST /api/funkos/{id}/image` consume `multipart/form-data` y recibe un archivo en el campo `file`.

## Resumen

| Controlador | Endpoints |
|---|---:|
| Auth | 3 |
| Categories | 7 |
| Suppliers | 7 |
| Users | 12 |
| Funkos | 14 |
| **Total** | **43** |

## Códigos HTTP utilizados

| Código | Significado |
|---|---|
| `200 OK` | Operación completada correctamente. |
| `201 Created` | Recurso creado. |
| `204 No Content` | Operación correcta sin contenido en la respuesta. |
| `400 Bad Request` | Solicitud o datos inválidos. |
| `401 Unauthorized` | Falta autenticación o es inválida. |
| `403 Forbidden` | El usuario no tiene permisos suficientes. |
| `404 Not Found` | Recurso no encontrado. |
| `409 Conflict` | Conflicto al realizar la operación. |

