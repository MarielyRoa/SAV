# 📊 SAV – Sistema de Análisis de Ventas

Proyecto ETL desarrollado en C# (.NET 8) para cargar información en un Data Warehouse orientado al análisis de ventas.  
El proceso integra datos desde APIs, archivos CSV y una base de datos externa.

---

## 🚀 Funcionalidad Principal
- Carga automática de **dimensiones**: Clientes, Productos, Tiempo y Fuentes.  
- Procesamiento y carga de la tabla de **hechos FactVentas**.  
- Manejo de errores y logs detallados.  
- Worker Service programado para ejecutarse cada 60 minutos.

---

## 📦 Tablas Principales del Data Warehouse

### **DimClientes**
Información maestra de clientes.

### **DimProductos**
Catálogo de productos proveniente de API o datos mock.

### **DimTiempos**
Tabla calendario generada automáticamente.

### **DimFuentes**
Identifica la procedencia de los datos cargados.

### **FactVentas**
Consolidación de ventas unidas con información de clientes, productos y fechas.

---

## 🔄 Flujo ETL
1. **Extracción**: APIs, CSV (`OrderDetails.csv`), base externa.  
2. **Transformación**: Limpieza, validación y mapeo a dimensiones.  
3. **Carga**: Inserción en tablas de dimensiones y hechos, con resumen final.

---

## 🛠 Tecnologías
- C# .NET 8  
- SQL Server  
- Entity Framework Core  
- Worker Service  
- Git / GitHub

---

## 👩‍💻 Autor
**Mariely Roa**  
GitHub: https://github.com/MarielyRoa
