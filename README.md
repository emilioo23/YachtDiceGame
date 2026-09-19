# Yacht Dice Multiplayer

Yacht Dice Multiplayer es un videojuego de dados basado en las reglas originales de Yacht Dice de la década de 1940. Usa una arquitectura cliente-servidor con C# / .NET y ASP.NET Core SignalR para la comunicación en tiempo real entre jugadores, con una interfaz gráfica nativa en WPF.

## Características del Sistema

* **Modos de Partida**: Partidas locales y en red de 2 a 4 jugadores, con dos variantes de tiempo por turno: Ritmo Normal (60 segundos) y Ritmo Rápido (30 segundos).

* **Autenticación y Seguridad**: Registro de usuarios, inicio de sesión, doble factor de autenticación (2FA) por correo electrónico, acceso como invitado y soporte para Passkeys mediante FIDO2/WebAuthn vinculadas al dispositivo.

* **Motor de Juego**: Hasta 3 lanzamientos por turno para 5 dados y validación de las 12 categorías de puntuación oficiales, con cálculo del Bonus Superior (+35 puntos si la sección superior suma ≥63) y el Yacht Bonus (+100 puntos acumulables, con aplicación de la Regla Joker).

* **Multijugador y Social**: Salas de espera (lobby) públicas o privadas con código de acceso, sistema de amistades y chat colaborativo con filtro automático de palabras ofensivas.

* **Moderación**: Reporte de jugadores y expulsión de la sala por parte del anfitrión, ya sea manual o mediante confirmación tras 3 infracciones de chat acumuladas (el sistema notifica al anfitrión, pero la expulsión siempre requiere su confirmación).

* **Progresión**: Marcador Top Global según los puntos históricos acumulados y sistema de experiencia (XP) que otorga niveles progresivos a los jugadores registrados según su desempeño.

* **Internacionalización**: Interfaz con soporte en tiempo real para Español (es-MX) e Inglés (en-US), mediante enlace de datos estático hacia diccionarios de recursos `.resx`.

* **Personalización**: Temas visuales claro y oscuro, y modificación del perfil del jugador (nombre para mostrar, avatar de un catálogo y color de fondo).

## Estándares de Código

El proyecto sigue un estándar de desarrollo propio del equipo, basado en los principios de Clean Code y las directrices oficiales de Microsoft para C#. El documento completo también define la organización del proyecto, el estilo y formato del código, la estructura interna de clases, los principios de POO, la gestión de recursos y el proceso de revisión de código; aquí solo se resumen los puntos que más afectan la lectura diaria del código:

* **Idiomas del Código**: El código fuente se redacta en inglés; la documentación XML, los comentarios de implementación y los registros del sistema (logs) se redactan en español.

* **Nomenclatura C#**: PascalCase para clases, métodos e interfaces (con prefijo `I`); camelCase para parámetros y variables; `_camelCase` para campos privados. Los delegados de eventos llevan el sufijo `EventHandler` y las excepciones personalizadas el sufijo `Exception`.

* **Diseño Orientado a Objetos**: Principio de Responsabilidad Única (SRP) y DRY (Don't Repeat Yourself). Los métodos se limitan a 3 parámetros y una complejidad ciclomática ≤10, usando objetos DTO para transferir información compleja.

* **Manejo Asíncrono**: Toda operación de red o de entrada/salida usa `async/await`, y los métodos correspondientes terminan con el sufijo `Async`.

* **Trazabilidad (Logging)**: Eventos clasificados en Trace, Debug, Info, Warning y Error, con la captura de pila limitada a 3 niveles en excepciones críticas. `Console.WriteLine` está prohibido.

* **Pruebas Unitarias**: Patrón Arrange, Act, Assert (AAA), con nombres que describen el método probado, el escenario y el comportamiento esperado. Las pruebas no dependen de la base de datos, el sistema de archivos ni la red.

## Base de Datos

La persistencia se maneja en SQL Server Express, en la base de datos `YachtDiceDB`. El esquema normalizado incluye:

* **Gestión de Identidad**: Tablas `PLAYER` (identidad y progreso), `PLAYER_SETTINGS` (preferencias de interfaz), y control de acceso en `TWO_FACTOR_AUTH`, `PASSKEY`, `PWD_RESET_TOKEN` y `BACKUP_CODE`.

* **Catálogos**: `AVATAR` (imágenes disponibles para el perfil) y `CATEGORY` (las 12 categorías de puntuación).

* **Multijugador**: Tablas `ROOM` y `ROOM_PLAYER` para el manejo de salas y cupos. Las interacciones sociales usan `FRIENDSHIP`, `ROOM_INVITE`, `KICK_LOG` y `PLAYER_REPORT`.

* **Historial y Puntuación**: Estado por turno en `TURN_STATE` (valores de los dados), desglose de jugadas en `SCORECARD` y `SCORECARD_ENTRY`, y resultados finales en `GAME_HISTORY` y `GAME_HISTORY_PLAYER`.

> El diseño de la base de datos especifica la intercalación `Latin1_General_100_CI_AS_SC_UTF8` para admitir caracteres acentuados y emojis de forma nativa. El script `CREATE DATABASE` actual no la aplica explícitamente (queda en `DATABASE_DEFAULT`), así que conviene revisar si falta agregar el `COLLATE` correspondiente o si el servidor ya la tiene como colación por defecto.

## Tecnologías Utilizadas

* C# / .NET
* Windows Presentation Foundation (WPF)
* ASP.NET Core SignalR
* SQL Server Express
* Visual Studio Code

## Equipo de Desarrollo

Universidad Veracruzana - Licenciatura en Ingeniería de Software.
**Equipo 1:**

* Emilio Álvarez Villalobos
* Gabriel Hernández Martínez
