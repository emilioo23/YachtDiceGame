# Passkeys en Yacht Dice

Documento de decisiones: qué se hizo, cómo funciona, por qué se hizo así y por qué no de otra forma.

## 1. Qué es una passkey y qué se quería lograr

Una passkey es una credencial de WebAuthn (FIDO2). El dispositivo del jugador crea un par de llaves: la llave **privada** se queda en el dispositivo (Windows Hello, o el celular) y nunca sale; el servidor guarda solo la llave **pública**. Para entrar, el servidor manda un reto (challenge) aleatorio, el dispositivo lo firma después de verificar al jugador (cara, huella o PIN) y el servidor comprueba la firma con la llave pública.

Lo que se pidió: poder entrar al juego usando Face ID del celular, sin escribir contraseña.

Lo que se implementó:

- Entrar con passkey desde la pantalla de login.
- Agregar una passkey desde Ajustes → seguridad (máximo 5 por jugador).
- Ver la lista de passkeys y quitar una (pide la contraseña actual y confirmación).
- Todo con las mismas protecciones del login normal: sesión única, bloqueos temporales, bloqueo diario y suspensión por reportes.

## 2. Cómo funciona (visión general)

```
Cliente WPF                         Servidor WCF (NetTcp)                     BD
-----------                         ---------------------                     --
BeginPasskeyLogin()          --->   genera reto de 32 bytes (5 min, 1 uso)
webauthn.dll (Windows)       <---
  muestra el diálogo de
  Windows: Windows Hello o
  celular por QR (Face ID)
CompletePasskeyLogin(resp)   --->   valida: reto, tipo, origen, rpIdHash,
                                    UP/UV, firma, contador, userHandle   --->  Passkey
                             <---   inicia la sesión (Sessions.Start)
```

Capas (la misma arquitectura que ya tenía el proyecto):

| Capa | Qué se agregó |
|---|---|
| Contracts | `PasskeyChallengeData`, `PasskeyAttestationData`, `PasskeyAssertionData`, `PasskeyInfoData`, códigos de resultado, 7 operaciones nuevas en `ITwoFactorManagementService` |
| Services | Las operaciones en `TwoFactorManagementService` y `PasskeyMapper` |
| Logic/Passkeys | `PasskeyService` (reglas), `PasskeyChallengeStore`, lectura de CBOR, de `authenticatorData`, de la llave COSE y verificación de firma |
| Data | Columnas `CredentialId` y `SignCount` en la tabla `Passkey`, `PasskeyRepository` |
| Cliente | `WindowsPasskeyAuthenticator` (llama a `webauthn.dll`), botón en el login, bloque en Ajustes |

## 3. Decisiones y por qué no otra opción

### 3.1 ¿App de escritorio o página web?

**Se eligió:** hacerlo dentro de la app WPF usando la API de WebAuthn de Windows (`webauthn.dll`).

**Por qué no una página web:** WebAuthn en un navegador solo funciona en un origen seguro con un dominio real (HTTPS y un nombre de dominio; `localhost` es la única excepción). El servidor del juego se conecta por IP y no tiene dominio ni certificado de una autoridad. Una página web obligaría a comprar un dominio, montar HTTPS, un servidor web y su despliegue, además de una segunda forma de autenticar que el resto del juego no usa. La app WPF ya es el cliente del juego; la sesión que se inicia es la misma que usa el resto.

**Por qué `webauthn.dll` y no una librería NuGet (Fido2NetLib u otra):** en Windows la propia API del sistema ya muestra el diálogo oficial (Windows Hello, llaves de seguridad y celular por QR con Bluetooth). No agrega paquetes de terceros al cliente. En el servidor tampoco se pudo agregar un paquete (el entorno donde se desarrolló no podía descargar de NuGet), y esto obligó a verificar a mano; ver 3.3.

### 3.2 ¿Dónde vive el servicio en el servidor?

**Se eligió:** agregar las operaciones a `ITwoFactorManagementService`, que ya existe, ya recibe el token de sesión y ya está registrado en el host.

**Por qué no un servicio WCF nuevo:** un servicio nuevo significa otro endpoint, otro puerto, otra regla de firewall, otra referencia de servicio completa en el cliente y otro punto donde configurar el certificado. Las passkeys son otro "segundo factor" de la cuenta, así que caben en el mismo servicio. El costo es que ese servicio crece; la lógica real está en `PasskeyService`, que es una clase aparte, así que el servicio WCF solo traduce.

### 3.3 Verificación de la firma hecha a mano en el servidor

Se implementó en el servidor: Base64url, un lector mínimo de CBOR, lectura de `authenticatorData`, llaves COSE (ES256 con P-256 y RS256), conversión de firma DER a formato fijo y verificación con `ECDsa` / `RSA` de .NET.

**Por qué no una librería:** no se podía instalar paquetes (ver 3.1) y el proyecto es .NET Framework 4.7.2, donde las librerías modernas de FIDO2 ya no son compatibles o exigen versiones nuevas. Lo que hay que verificar es corto y bien definido por la especificación (W3C WebAuthn, nivel 2).

**Cómo se redujo el riesgo de hacerlo a mano:**

- Solo se aceptan dos algoritmos (ES256 y RS256), los que usan Windows Hello y los celulares.
- No se pide "attestation": se usa `none`. El servidor no necesita saber el modelo del dispositivo; solo guarda la llave pública. Verificar cadenas de certificados de fabricantes sería mucho más código y nada que el juego necesite.
- El lector de CBOR tiene límites de profundidad (6) y de elementos (64), rechaza llaves nulas y datos truncados, para que una respuesta mal formada no pueda causar un cierre del servidor ni consumir memoria.
- Se probó con un **autenticador falso** (`FakeAuthenticator`) que genera llaves ECDSA y firmas reales, de modo que las pruebas ejercitan la misma ruta que un dispositivo de verdad.

### 3.4 Qué valida el servidor en cada respuesta

1. El reto existe, no se usó antes, no expiró (5 minutos) y es del mismo propósito (registro o login). **Un reto sirve una sola vez.**
2. En el registro, el reto pertenece al jugador de la sesión.
3. El `clientDataJSON` tiene el tipo correcto (`webauthn.create` o `webauthn.get`), el mismo reto y el origen esperado.
4. `rpIdHash` del `authenticatorData` es el SHA-256 del `RelyingPartyId`.
5. Las banderas **UP** (usuario presente) y **UV** (usuario verificado) están encendidas. Sin UV no se acepta: es lo que garantiza que se pidió Face ID, huella o PIN.
6. La firma es válida sobre `authenticatorData || SHA256(clientDataJSON)`.
7. El contador de firmas no retrocede (detección de passkey clonada).
8. Si la respuesta trae `userHandle`, debe ser el del dueño de la credencial.
9. Después se aplican los bloqueos de cuenta del login normal y se inicia la sesión única.

### 3.5 Login con passkey: ¿cuenta como "dos factores"?

**Se eligió:** que la passkey inicia la sesión directamente, sin pedir además el código del correo.

**Por qué:** una passkey con verificación de usuario ya es multifactor por sí misma: algo que tienes (el dispositivo con la llave) y algo que eres o sabes (cara, huella o PIN). Pedir encima el código por correo sería repetir un factor más débil. Es la práctica de las aplicaciones que usan passkeys.

### 3.6 Reto y registro en memoria

Los retos se guardan en memoria del servidor (`PasskeyChallengeStore`), con tope de 500 pendientes y expiración, no en la base de datos.

**Por qué:** viven 5 minutos y se usan una vez. Guardarlos en BD agregaría escrituras y una tabla para algo que se descarta casi de inmediato. **Limitación conocida:** si el servidor se reinicia mientras alguien está a mitad de la ceremonia, ese intento falla y se repite. El tope evita que alguien llene la memoria pidiendo retos sin parar.

### 3.7 `RelyingPartyId`: `yachtdice.localhost`

WebAuthn exige un identificador de la parte confiable (RP ID). Como no hay dominio, se usa `yachtdice.localhost` y el origen `https://yachtdice.localhost`, el mismo en cliente y servidor. **Esto no está probado contra Windows ni contra un iPhone**; es el punto que más puede necesitar ajuste (ver 5).

### 3.8 Quitar una passkey

Se pide la **contraseña actual** y una confirmación, y el servidor revisa que la passkey sea del jugador de la sesión (una persona no puede quitar la de otra mandando un identificador ajeno).

**Por qué la contraseña y no otra passkey:** si alguien tiene la computadora abierta con la sesión iniciada, no debe poder quitarte la passkey (o dejarte solo con la suya) sin saber la contraseña. Es lo que hacen las aplicaciones que manejan passkeys (re-autenticar antes de un cambio sensible). Es el mismo patrón que ya usan los códigos de respaldo.

### 3.9 Límite de 5 passkeys

Cubre computadora, celular, tableta y una llave de repuesto sin permitir un número ilimitado de filas por jugador. Al crear una nueva se mandan las ya registradas como "excluidas" para que el mismo dispositivo no registre dos veces la misma passkey.

## 4. Seguridad y estándar de código

- No hay secretos en el código ni en variables de entorno: el servidor solo guarda llaves públicas.
- No hay mensajes engañosos: los mensajes del usuario dicen qué pasó y no filtran el motivo exacto del rechazo (un login fallido siempre dice que la passkey no se pudo verificar; el motivo se registra en el log del servidor).
- Ninguna excepción se traga sin registrarse; la cancelación del diálogo de Windows no es error y no muestra mensaje.
- Estándar v20: un solo `return`, máximo 3 parámetros, sin números mágicos, constantes para los textos de guarda, XML docs en inglés, pruebas con una sola aserción y nombres `Metodo_Escenario_Resultado`.
- **Excepción conocida:** las dos funciones de `webauthn.dll` (`WebAuthNAuthenticatorMakeCredential` y `WebAuthNAuthenticatorGetAssertion`) tienen más de 3 parámetros porque así las define Windows; no se pueden cambiar. Están aisladas en `WebAuthnNative.cs` y documentadas.
- Script SQL `AddPasskeyCredentialData.sql`: idempotente, borra las filas viejas de la tabla `Passkey` (que no tenían credencial y no servían) y crea el índice único de `CredentialId`.

## 5. Qué se probó y qué falta (ser honestos con el profe)

**Probado automáticamente:** lectura de Base64url, CBOR, `authenticatorData`, validación del `clientDataJSON`, almacén de retos (uso único, expiración, tope), `PasskeyService` completo (registro, login, quitar, listar) y los mensajes del cliente.

**No se ha probado en un Windows real:**

1. La llamada a `webauthn.dll` (estructuras escritas según `webauthn.h`, versión 1 de la API).
2. Que Windows y el iPhone acepten `yachtdice.localhost` como RP ID.
3. Que el celular se conecte por QR (necesita internet en la PC y en el celular).
4. Las ventanas WPF nuevas (XAML) y el arranque del host con las clases nuevas.

Las pruebas que firman con ECDSA se ejecutan en Visual Studio con .NET Framework 4.7.2; el entorno en que se desarrolló (Mono) no implementa esa parte de la API.

## 6. Preguntas que puede hacer el profe

**¿Por qué no usaron una librería?** No había forma de instalar paquetes y la versión de .NET es antigua; la verificación son pocas reglas bien definidas y se cubrió con un autenticador falso que firma de verdad.

**¿Dónde se guarda la llave privada?** Nunca llega al servidor ni al juego: se queda en Windows Hello o en el celular.

**¿Y si roban la base de datos?** Solo hay llaves públicas y contadores; no sirven para entrar.

**¿Y si pierdo el celular?** La cuenta sigue teniendo contraseña (y el código por correo si el segundo paso está activo); desde Ajustes se quita la passkey perdida.

**¿Qué evita que alguien grabe la respuesta y la repita?** El reto se usa una sola vez y expira; además la firma cubre el reto y el contador no puede retroceder.

**¿Por qué UV obligatorio?** Sin verificación de usuario, quien tenga el dispositivo desbloqueado entraría sin Face ID o PIN.
