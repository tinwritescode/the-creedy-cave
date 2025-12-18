# Unity Layers Explanation

## What are Layers?

Unity has **two different layer systems**:

### 1. **Physics Layers** (just called "Layers")
- Located in: **Edit > Project Settings > Tags and Layers**
- Used for: **Physics collisions, raycasting, camera culling**
- 32 layers available (0-31)
- Common layers: Default, TransparentFX, Ignore Raycast, Water, UI

### 2. **Sorting Layers** (for 2D rendering)
- Located in: **Edit > Project Settings > Tags and Layers > Sorting Layers**
- Used for: **Rendering order of sprites** (which sprites appear in front/behind)
- Your project has: Background, Wall, Foreground, Coin, Player, UI

---

## Why Assign a Player Layer?

### **1. Collision Filtering** ⚡
Control which objects the player can collide with:

```
Example Setup:
- Player Layer: Collides with Enemy, Coin, Wall
- Enemy Layer: Collides with Player, Wall (but NOT other enemies)
- UI Layer: Collides with NOTHING (ignored by physics)
```

**How to set up:**
1. Edit > Project Settings > Physics 2D
2. Configure "Layer Collision Matrix"
3. Uncheck collisions you don't want (e.g., Enemy vs Enemy)

### **2. Raycasting** 🎯
Filter raycasts to only hit specific objects:

```csharp
// Only hit objects on "Enemy" layer
int enemyLayer = LayerMask.NameToLayer("Enemy");
RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, 1 << enemyLayer);

// Hit everything EXCEPT UI layer
int uiLayer = LayerMask.NameToLayer("UI");
int layerMask = ~(1 << uiLayer); // ~ means "NOT"
RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, layerMask);
```

### **3. Camera Culling** 📷
Control what each camera renders:

```
Main Camera: Renders Default, Player, Enemy, Coin layers
UI Camera: Renders ONLY UI layer
```

**Benefits:**
- Better performance (don't render unnecessary layers)
- Separate UI rendering (UI always on top)
- Multiple cameras for different views

### **4. Physics Interactions** 🔧
Control which objects interact with physics:

```csharp
// Only check collisions with specific layers
Collider2D[] hits = Physics2D.OverlapCircleAll(
    position, 
    radius, 
    1 << LayerMask.NameToLayer("Enemy") // Only enemies
);
```

---

## Common Layer Setup for Your Game

### **Recommended Physics Layers:**
1. **Default** (0) - Walls, floors, environment
2. **Player** (8) - Player character
3. **Enemy** (9) - Enemy characters
4. **Coin** (10) - Collectible items
5. **UI** (5) - UI elements (usually doesn't collide)

### **Layer Collision Matrix:**
```
        | Default | Player | Enemy | Coin | UI
--------|---------|--------|-------|------|----
Default |   ✓     |   ✓    |   ✓   |  ✓   | ✗
Player  |   ✓     |   ✗    |   ✓   |  ✓   | ✗
Enemy   |   ✓     |   ✓    |   ✗   |  ✗   | ✗
Coin    |   ✓     |   ✓    |   ✗   |  ✗   | ✗
UI      |   ✗     |   ✗    |   ✗   |  ✗   | ✗
```

**Legend:**
- ✓ = These layers collide
- ✗ = These layers don't collide

**Why:**
- Player doesn't collide with other players (multiplayer)
- Enemies don't collide with each other (can overlap)
- Coins don't collide with enemies (enemies pass through)
- UI never collides (it's just visual)

---

## Sorting Layers vs Physics Layers

### **Sorting Layers** (Rendering Order):
- Controls **visual order** (what appears in front/behind)
- Used by SpriteRenderer component
- Example: Player sprite appears in front of Background

### **Physics Layers** (Collision Filtering):
- Controls **physics interactions** (what collides with what)
- Used by Collider2D, Rigidbody2D, Raycast
- Example: Player collides with Enemy but not UI

**They are completely separate!** You can have:
- Player on "Player" Physics Layer
- Player on "Foreground" Sorting Layer
- These work independently!

---

## Practical Example: Enemy Detection

**Without Layers:**
```csharp
// Hits EVERYTHING (walls, coins, UI, etc.)
Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 5f);
```

**With Layers:**
```csharp
// Only hits Player layer
int playerLayer = LayerMask.NameToLayer("Player");
Collider2D[] hits = Physics2D.OverlapCircleAll(
    transform.position, 
    5f, 
    1 << playerLayer
);
```

**Result:** More efficient, more accurate, no false positives!

---

## How to Set Up Layers

### **Step 1: Create Physics Layers**
1. Edit > Project Settings > Tags and Layers
2. Under "Layers", assign names to empty slots:
   - Layer 8: "Player"
   - Layer 9: "Enemy"
   - Layer 10: "Coin"

### **Step 2: Assign Layers to GameObjects**
1. Select GameObject in scene
2. In Inspector, top-right corner
3. Change "Layer" dropdown to your layer (e.g., "Player")

### **Step 3: Configure Collision Matrix**
1. Edit > Project Settings > Physics 2D
2. Find "Layer Collision Matrix"
3. Uncheck collisions you don't want

### **Step 4: Use in Code**
```csharp
// Get layer by name
int playerLayer = LayerMask.NameToLayer("Player");

// Check if object is on specific layer
if (gameObject.layer == playerLayer) { ... }

// Create layer mask
int layerMask = 1 << playerLayer; // Only player layer
int layerMask = (1 << playerLayer) | (1 << enemyLayer); // Player OR enemy
```

---

## Summary

**Layers are essential for:**
- ✅ Performance (fewer unnecessary collision checks)
- ✅ Organization (group similar objects)
- ✅ Control (precise collision filtering)
- ✅ Flexibility (different cameras, different views)

**You don't NEED a Player layer, but it's highly recommended** for:
- Cleaner code
- Better performance
- Easier debugging
- Professional game development practices
