# Plugins de Oxide.

## RacePlugin - Plugin de Carreras para Rust con Oxide

### Descripción
RacePlugin es un plugin para servidores de Rust basado en Oxide, que permite la creación y gestión de eventos de carreras de coches. Incluye soporte para múltiples posiciones de inicio, checkpoints y una meta configurable con radio de detección. Además, ofrece comandos de administración para gestionar y editar eventos.

### Características
- Creación y edición de eventos de carrera.
- Configuración de posiciones de inicio, meta y checkpoints con radio de detección.
- Comandos avanzados con flags para configuración granular.
- Posibilidad de visualizar hitboxes para cada checkpoint (futuro desarrollo).

### Instalación
1. Descarga el archivo `RacePlugin.cs`.
2. Copia el archivo en la carpeta `oxide/plugins` de tu servidor de Rust.
3. Reinicia el servidor o usa el comando `oxide.reload RacePlugin` en la consola.

### Comandos Disponibles

#### **Creación y Edición de Eventos**
- `/race create` - Inicia la creación de una nueva carrera.
- `/race edit {nombre}` - Edita una carrera guardada.
- `/race save` - Guarda la carrera en edición.
- `/race cancel` - Cancela la creación o edición de una carrera.

#### **Configuración de la Carrera**
- `/race set finish radius {valor}` - Configura la meta con un radio determinado.
- `/race add checkpoint radius {valor}` - Añade un checkpoint con un radio determinado.
- `/race edit finish radius {valor}` - Edita el radio de la meta.
- `/race edit checkpoint {índice} radius {valor}` - Edita el radio de un checkpoint específico.

#### **Gestión de Eventos**
- `/race list` - Lista todas las carreras guardadas.
- `/race delete {nombre}` - Elimina una carrera guardada.
- `/race start {nombre}` - Inicia una carrera guardada.
- `/race join` - Permite unirse a la carrera activa.
- `/race positions` - Muestra las posiciones actuales de los participantes.

### Configuración Adicional
El plugin usa un archivo de configuración en `oxide/config/RacePlugin.json` donde se pueden personalizar:
- Duración de la visualización de hitboxes.
- Radio predeterminado de los checkpoints.

### Requisitos
- Servidor de Rust con Oxide instalado.

### Futuras Mejoras
- Implementación de hitboxes visibles para checkpoints y meta.
- Integración con ZoneManager para visualización avanzada.
- Lógica de detección automática de vueltas.