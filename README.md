# Age Tower Defense

Un juego de estrategia en tiempo real / defensa de torres construido en **Unity 6** usando el paquete de pixel art **Tiny Sword**.

Defiende tu castillo contra oleadas interminables de tropas enemigas. Recolecta recursos, construye estructuras defensivas, entrena tu propio ejército y sobrevive el mayor tiempo posible.

---

## Cómo jugar

### 1. Elige tu facción
Al iniciar, selecciona la facción **Azul** o **Roja**. Tu castillo, edificios, peones y tropas adoptarán el color elegido. Los enemigos siempre serán del color contrario.

### 2. Recolecta recursos
Haz clic en un **nodo de recursos** del mapa para enviar un Peón a recolectarlo:

- 🪵 **Madera** — de árboles (hacha)
- ⛏️ **Oro** — de piedras minerales (pico)
- 🥩 **Comida** — de ovejas (cuchillo)

Los Peones llevan los recursos de regreso al castillo automáticamente y repiten el ciclo hasta que el nodo se agote. La Comida aumenta tu límite de tropas; el Oro desbloquea mejoras.

### 3. Construye estructuras
Haz clic en un **espacio de construcción** vacío en el mapa para abrir el menú de construcción. Cada edificio cuesta Madera y genera tropas automáticamente con el tiempo:

| Edificio | Costo | Genera | Enfriamiento |
|---|---|---|---|
| Cuartel | 80 Madera | Guerrero | 8s (4s mejorado) |
| Torre de Arquería | 60 Madera | Arquero | 8s (4s mejorado) |
| Torre | 100 Madera | Lancero | 8s (4s mejorado) |
| Monasterio | 70 Madera + 30 Comida | Monje | 8s (4s mejorado) |
| Casa | 50 Madera | +1 ranura de Peón | — |

Gasta **100 de Oro** para mejorar un edificio y reducir a la mitad su tiempo de generación de tropas.

### 4. Gestiona tus tropas
Tus tropas patrullan automáticamente alrededor del castillo y atacan a cualquier enemigo cercano:

- ⚔️ **Guerrero** — combatiente cuerpo a cuerpo equilibrado
- 🏹 **Arquero** — ataque a larga distancia, dispara flechas
- 🗡️ **Lancero** — cuerpo a cuerpo rápido, mantiene la línea
- 🙏 **Monje** — cura a las tropas aliadas cercanas

### 5. Defiende el castillo
Las oleadas enemigas marchan desde los bordes del mapa hacia tu castillo. Tienes **30 segundos** antes de la primera oleada y **10 segundos** entre oleadas siguientes. Cada oleada trae más enemigos y más fuertes. Si el HP del castillo llega a cero — **Fin del juego**.

El castillo muestra efectos de fuego cuando está por debajo del 30% de HP. Asigna un Peón a un edificio dañado haciendo clic sobre él para repararlo.

### 6. Habilidades especiales
- **Impulso de tropas** — gasta 150 de Oro para dar a todas las tropas activas +50% de estadísticas durante 2 minutos
- **Contratar Peón** — gasta 50 de Oro para reclutar un Peón adicional

---

## Sistema de oleadas

| Oleada | Enemigos | Composición |
|---|---|---|
| 1 | 5 | Solo guerreros |
| 2 | 8 | Guerreros + Arqueros |
| 3 | 10 | Guerreros, Arqueros, Lanceros |
| 4 | 12 | Mezcla completa |
| 7+ | 10 + oleada×2 | Mezcla completa con Monjes, +25% estadísticas por oleada |

Un banner de cuenta regresiva anuncia cada oleada entrante. ¡Sobrevive el mayor tiempo posible!

---

## Controles

| Acción | Entrada |
|---|---|
| Enviar Peón a recurso | Clic en nodo de recurso |
| Cancelar tarea del Peón | Clic en el nodo asignado nuevamente |
| Construir edificio | Clic en espacio vacío → elegir tipo |
| Reparar edificio | Clic en edificio dañado (el Peón debe estar libre) |
| Mover guerrero del jugador | WASD / Teclas de dirección |
| Atacar (guerrero del jugador) | Clic izquierdo |
| Pausar / Reanudar | Esc |
| Volver al menú principal | Esc (en pantalla de Game Over) |

---

## Tecnología utilizada

- **Unity 6000.3.9f1**
- **C#** — IA basada en corrutinas, movimiento físico, UI orientada a eventos
- **Rigidbody2D** cinemático con `MovePosition` para física fluida
- **Sistema Y-Sort** para capas correctas en perspectiva 2D top-down
- **Wave Manager** con composición de enemigos por oleada y escalado de estadísticas
- **Sistema de proyectiles** para flechas de arqueros

---

## Créditos

**Creado por [Laszlo Sierra](https://github.com/LaszloSM)**
Diseño del juego, programación y todos los sistemas construidos desde cero en Unity 6.

**Assets de pixel art** — [Tiny Sword](https://pixelfrog-assets.itch.io/) por **Pixel Frog**
→ https://pixelfrog-assets.itch.io/

Sprites, animaciones, tilemaps, elementos de UI, efectos de partículas y audio del paquete Tiny Sword. ¡Apoya su increíble trabajo!

---

## Licencia

© 2026 Laszlo Sierra. Todos los derechos reservados.
Los assets visuales y de audio pertenecen a sus respectivos creadores (ver Créditos).
