# Turn-Based Combat System Implementation Prompt

## Overview
Implement a turn-based combat system that triggers when the player touches an enemy. The combat alternates between player and enemy attacks until one dies, or the player chooses to run away.

## Requirements

### 1. Combat Trigger
- When the player's collider touches an enemy's collider, initiate combat
- Disable player movement during combat
- Show a combat UI panel with combat controls

### 2. Turn-Based Combat Flow
- **Turn 1**: Player attacks enemy → show damage dealt on UI
- **Turn 2**: Enemy attacks player → show damage dealt on UI
- **Turn 3**: Player attacks enemy → show damage dealt on UI
- **Turn 4**: Enemy attacks player → show damage dealt on UI
- Continue alternating until:
  - One entity dies (health <= 0), OR
  - Player clicks "Run Away" button

### 3. Damage Display System
- When damage is dealt, display a damage number on the UI (e.g., "-50" or "-100")
- Damage text should be visible and readable
- Each damage number automatically disappears after 2 seconds
- Multiple damage numbers can stack if attacks happen quickly

### 4. Combat UI Elements
- Combat panel that appears when combat starts
- Display current health for both player and enemy
- Show damage numbers for each attack
- "Run Away" button that allows player to escape combat
- Auto-hide combat UI when combat ends

### 5. Combat State Management
- Prevent player movement during combat
- Prevent enemy movement during combat
- Queue attacks properly (wait for damage display before next attack)
- Handle combat end conditions (death or escape)

## Technical Implementation Notes

### Existing Codebase Context
- `PlayerHealth.cs`: Has `TakeDamage(float damage)` method and `OnHealthChanged` event
- `HealthBarUI.cs`: Updates health bar when health changes
- `PlayerController.cs`: Handles player movement
- `EnemyController.cs`: Basic enemy controller (may need health component added)
- Unity 2D project with Input System

### Suggested Components to Create
1. **CombatManager.cs**: Singleton or static manager to handle combat state
2. **EnemyHealth.cs**: Similar to PlayerHealth for enemy entities
3. **CombatUI.cs**: UI controller for combat panel and damage displays
4. **DamageNumber.cs**: Component for floating damage text that auto-destroys after 2s
5. **CombatTrigger.cs**: Component to detect player-enemy collision and start combat

### Implementation Steps
1. Add health system to enemies (create `EnemyHealth.cs` similar to `PlayerHealth.cs`)
2. Create combat detection system (collision trigger between player and enemy)
3. Implement combat state machine (idle → combat → player turn → enemy turn → repeat)
4. Create combat UI with damage number display system
5. Add "Run Away" functionality
6. Handle combat end conditions (death, escape)
7. Re-enable movement when combat ends

## UI/UX Considerations
- Damage numbers should be visually distinct (red for damage, maybe larger font)
- Combat UI should not obstruct gameplay view
- Smooth transitions when entering/exiting combat
- Clear visual feedback for whose turn it is
- "Run Away" button should be clearly visible and accessible

## Testing Checklist
- [ ] Combat triggers when player touches enemy
- [ ] Player movement disabled during combat
- [ ] Turn alternates correctly (player → enemy → player...)
- [ ] Damage numbers appear and disappear after 2 seconds
- [ ] Health bars update correctly during combat
- [ ] Combat ends when player dies
- [ ] Combat ends when enemy dies
- [ ] "Run Away" button exits combat
- [ ] Movement re-enabled after combat ends
- [ ] Multiple damage numbers can display simultaneously

