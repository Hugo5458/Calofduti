# 🎮 Guía Paso a Paso - FPS Zombies

## 📋 Resumen de Scripts Creados

| Script | Función |
|--------|---------|
| `PlayerHealth.cs` | Vida del jugador, daño y muerte |
| `Weapon.cs` | Sistema de disparo y munición |
| `ZombieAI.cs` | Comportamiento del zombie (sin IA compleja) |
| `ZombieHealth.cs` | Vida del zombie y puntuación |
| `ZombieSpawner.cs` | Generador de oleadas de zombies |
| `GameManager.cs` | Control del juego, puntuación, pausa |
| `Crosshair.cs` | Mira en pantalla |
| `PickupItem.cs` | Items recogibles (vida, munición) |
| `MainMenu.cs` | Menú principal |
| `Door.cs` | Puertas interactivas |
| `SimpleZombie.cs` | Zombie alternativo simplificado |

---

## 🚀 PASO 1: Configurar el Jugador

### 1.1 Añadir el FPSController
1. Ve a la carpeta: `Assets/Standard Assets/Standard Assets/Standard Assets/Characters/FirstPersonCharacter/Prefabs/`
2. Arrastra **FPSController** a tu escena "Zombies"
3. Posiciónalo sobre el terreno (Y = 2 aproximadamente)

### 1.2 Configurar el Tag del Jugador
1. Selecciona el **FPSController** en la jerarquía
2. En el Inspector, busca **Tag** (arriba)
3. Selecciona **"Player"**
   - Si no existe, crea uno: Add Tag > + > escribe "Player" > Save > vuelve y selecciónalo

### 1.3 Añadir Scripts al Jugador
1. Con **FPSController** seleccionado
2. En el Inspector, click en **Add Component**
3. Busca y añade: **PlayerHealth**
4. Busca y añade: **Crosshair**

---

## 🔫 PASO 2: Configurar el Arma

### 2.1 Crear el Objeto del Arma
1. En la jerarquía, expande: `FPSController > FirstPersonCharacter`
2. Click derecho en **FirstPersonCharacter** > Create Empty
3. Renómbralo a **"Arma"**
4. Posición: X=0.5, Y=-0.3, Z=0.8 (ajusta para que se vea bien)

### 2.2 Añadir Modelo del Arma (temporal)
1. Click derecho en **"Arma"** > 3D Object > Cube
2. Escala: X=0.1, Y=0.1, Z=0.5 (simula una pistola)
3. Quítale el Collider (Box Collider) - click derecho > Remove Component

### 2.3 Añadir Script de Arma
1. Selecciona el objeto **"Arma"**
2. Add Component > busca **Weapon**
3. Configura en el Inspector:
   - **Fps Cam**: Arrastra "FirstPersonCharacter" aquí
   - **Damage**: 25
   - **Range**: 100
   - **Fire Rate**: 0.25
   - **Max Ammo**: 30
   - **Reserve Ammo**: 90

### 2.4 Crear Punto de Disparo
1. Click derecho en **"Arma"** > Create Empty
2. Renómbralo a **"FirePoint"**
3. Posición: Z=0.5 (en la punta del arma)
4. Vuelve al componente Weapon y arrastra **FirePoint** al campo "Fire Point"

---

## 🧟 PASO 3: Crear el Zombie

### 3.1 Crear el Modelo del Zombie
1. En la jerarquía: Click derecho > 3D Object > Capsule
2. Renómbralo a **"Zombie"**
3. Escala: X=1, Y=1, Z=1
4. Añade color (opcional):
   - Create > Material > Color verde/gris
   - Arrastra el material al Zombie

### 3.2 Añadir Componentes al Zombie
1. Selecciona **"Zombie"**
2. Add Component > **Rigidbody**
   - Freeze Rotation: ✓ X, ✓ Y, ✓ Z (marca los tres)
3. Add Component > **ZombieHealth**
   - Max Health: 100
   - Score Value: 100
4. Add Component > **ZombieAI**
   - Damage: 10
   - Attack Rate: 1
   - Detection Range: 30
   - Attack Range: 2
   - Move Speed: 3

### 3.3 Guardar como Prefab
1. Crea la carpeta si no existe: `Assets/Prefabs`
2. Arrastra el **"Zombie"** desde la jerarquía a la carpeta `Assets/Prefabs`
3. Ahora tienes un prefab reutilizable
4. **Elimina** el zombie de la escena (el spawner lo creará)

---

## 🎯 PASO 4: Configurar el Spawner de Zombies

### 4.1 Crear Puntos de Spawn
1. Click derecho en jerarquía > Create Empty
2. Renómbralo a **"SpawnPoints"** (contenedor)
3. Click derecho en SpawnPoints > Create Empty
4. Renómbralo a **"SpawnPoint1"**
5. Posiciónalo en el mapa donde quieres que aparezcan zombies
6. Repite para crear **SpawnPoint2**, **SpawnPoint3**, etc. (mínimo 3-4)

### 4.2 Crear el Spawner
1. Click derecho en jerarquía > Create Empty
2. Renómbralo a **"ZombieSpawner"**
3. Add Component > **ZombieSpawner**
4. Configura:
   - **Zombie Prefab**: Arrastra el prefab Zombie desde Assets/Prefabs
   - **Spawn Points**: 
     - Size: (número de spawn points)
     - Arrastra cada SpawnPoint al array
   - **Initial Spawn Delay**: 3
   - **Spawn Interval**: 5
   - **Max Zombies Alive**: 10
   - **Zombies Per Wave**: 3

---

## 🎮 PASO 5: Crear el Game Manager

### 5.1 Crear el Objeto
1. Click derecho en jerarquía > Create Empty
2. Renómbralo a **"GameManager"**
3. Add Component > **GameManager**

---

## 🖥️ PASO 6: Crear la UI

### 6.1 Crear Canvas
1. Click derecho en jerarquía > UI > Canvas
2. Selecciona el Canvas:
   - Canvas Scaler > UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: 1920 x 1080

### 6.2 Crear Texto de Puntuación
1. Click derecho en Canvas > UI > Legacy > Text
2. Renómbralo a **"ScoreText"**
3. Rect Transform:
   - Anchor: Top Left
   - Pos X: 150, Pos Y: -30
   - Width: 300, Height: 50
4. Text component:
   - Text: "Puntuación: 0"
   - Font Size: 24
   - Color: Blanco

### 6.3 Crear Texto de Oleada
1. Click derecho en Canvas > UI > Legacy > Text
2. Renómbralo a **"WaveText"**
3. Anchor: Top Center
4. Text: "Oleada: 1"
5. Font Size: 28

### 6.4 Crear Texto de Munición
1. Click derecho en Canvas > UI > Legacy > Text
2. Renómbralo a **"AmmoText"**
3. Anchor: Bottom Right
4. Pos X: -100, Pos Y: 30
5. Text: "30 / 90"
6. Font Size: 28

### 6.5 Crear Barra de Vida
1. Click derecho en Canvas > UI > Slider
2. Renómbralo a **"HealthSlider"**
3. Anchor: Bottom Left
4. Pos X: 150, Pos Y: 30
5. Width: 200
6. Desactiva "Interactable"
7. En Fill Area > Fill: Color Rojo
8. Borra el "Handle Slide Area" (no lo necesitas)

### 6.6 Crear Panel de Daño
1. Click derecho en Canvas > UI > Image
2. Renómbralo a **"DamageImage"**
3. Anchor: Stretch (todo el Canvas)
4. Color: Rojo con Alpha 0 (transparente)

### 6.7 Conectar UI al GameManager
1. Selecciona **GameManager**
2. Arrastra los textos a los campos correspondientes:
   - Score Text: ScoreText
   - Wave Text: WaveText

### 6.8 Conectar UI al PlayerHealth
1. Selecciona **FPSController**
2. En el componente PlayerHealth:
   - Health Slider: HealthSlider
   - Damage Image: DamageImage

### 6.9 Conectar UI al Weapon
1. Selecciona el objeto **Arma**
2. En el componente Weapon:
   - Ammo Text: AmmoText

---

## ⏸️ PASO 7: Crear Menú de Pausa (Opcional)

### 7.1 Crear Panel de Pausa
1. Click derecho en Canvas > UI > Panel
2. Renómbralo a **"PauseMenu"**
3. Color: Negro con Alpha 200
4. **Desactívalo** (desmarca el checkbox en el Inspector)

### 7.2 Añadir Botones
1. Click derecho en PauseMenu > UI > Legacy > Button
2. Renómbralo a **"ResumeButton"**
3. Texto: "Continuar"
4. Repite para crear **"QuitButton"** con texto "Salir"

### 7.3 Conectar al GameManager
1. Selecciona GameManager
2. Arrastra PauseMenu al campo "Pause Menu"

---

## 📦 PASO 8: Crear Items Recogibles (Opcional)

### 8.1 Crear Botiquín
1. Click derecho > 3D Object > Cube
2. Renómbralo a **"HealthPack"**
3. Escala: 0.5, 0.5, 0.5
4. Añade material rojo/verde
5. Add Component > **PickupItem**
   - Pickup Type: Health
   - Amount: 25
6. En el Collider (Box Collider):
   - ✓ Is Trigger
7. Guarda como prefab

### 8.2 Crear Munición
1. Igual que el botiquín pero:
   - Renómbralo a **"AmmoPack"**
   - Material amarillo
   - Pickup Type: Ammo
   - Ammo Amount: 30

---

## 🏠 PASO 9: Añadir la Casa (Opcional)

### 9.1 Colocar la Casa
1. Ve a `Assets/country house01/Prefabs/`
2. Arrastra **House_Prefab** a la escena
3. Posiciónalo en el terreno

### 9.2 Añadir Puertas Interactivas
1. Selecciona una puerta de la casa
2. Add Component > **Door**
3. Añade un Box Collider grande
   - ✓ Is Trigger
4. Configura el script Door

---

## ✅ PASO 10: Verificación Final

### Checklist
- [ ] FPSController tiene tag "Player"
- [ ] FPSController tiene PlayerHealth y Crosshair
- [ ] Arma tiene Weapon con FpsCam asignada
- [ ] Zombie prefab tiene ZombieHealth y ZombieAI
- [ ] ZombieSpawner tiene el prefab y spawn points asignados
- [ ] GameManager existe en la escena
- [ ] UI conectada a los scripts
- [ ] Escena guardada

### Probar el Juego
1. **Ctrl + S** para guardar la escena
2. **Play** (▶️) para probar
3. Controles:
   - **WASD**: Mover
   - **Mouse**: Mirar
   - **Click Izquierdo**: Disparar
   - **R**: Recargar
   - **ESC**: Pausa

---

## 🐛 Solución de Problemas

### "El zombie no persigue al jugador"
- Verifica que el FPSController tenga el tag **"Player"**

### "No puedo disparar"
- Verifica que **Fps Cam** esté asignada en el Weapon
- Verifica que **Current Ammo** sea mayor a 0

### "El zombie atraviesa paredes"
- Añade Colliders a las paredes
- El zombie simple no evita obstáculos (es intencional, sin IA)

### "La UI no se actualiza"
- Verifica que los textos estén conectados en los scripts

### "El jugador cae del mapa"
- Añade colliders al terreno
- Verifica la posición Y del FPSController

---

## 🎨 Mejoras Opcionales

1. **Añadir sonidos**: Asigna AudioClips en los scripts
2. **Añadir efectos de partículas**: Para disparos e impactos
3. **Crear más tipos de armas**: Duplica y modifica el Weapon
4. **Añadir skybox**: Usa los de `Assets/AllSkyFree`
5. **Decorar el mapa**: Añade árboles, rocas, etc.

---

¡Buena suerte con tu juego! 🎮🧟
