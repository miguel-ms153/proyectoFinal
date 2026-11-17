# EcoScan Hogar

Aplicación .NET MAUI para gestionar alimentos del hogar, reducir desperdicios y recibir alertas de vencimiento.

## Arquitectura

- `ProyectoFinal`: cliente MAUI con cinco pantallas, persistencia local, cámara y GPS.
- `EcoScanHogar.Api`: servicio REST con ASP.NET Core, Entity Framework Core y SQLite.
- Comunicación: HTTP/JSON. El cliente puede apuntar a la API local en `http://localhost:5188` durante las pruebas de Windows.

## Ejecutar la API

```powershell
cd EcoScanHogar.Api
dotnet run
```

La base `ecoscan.db` se crea automáticamente. Endpoints: `GET/POST /api/products`, `PUT /api/products/{id}/consume`, `DELETE /api/products/{id}` y `GET /api/health`.

## Google Maps

La pantalla de estadísticas abre una búsqueda de Google Maps de supermercados cercanos usando la ubicación actual. No necesita una clave API para esta primera integración.
