# Hướng Dẫn Setup Death Scene

## Tổng Quan

Death Scene là một scene riêng biệt được load khi player chết. Scene này có 2 button:
1. **Return to Menu** - Quay về main menu
2. **Play Again** - Chơi lại level vừa chết

## Các Script

### 1. DeathManager.cs
- **Vị trí**: `Assets/Scripts/DeathManager.cs`
- **Chức năng**: Quản lý việc load death scene khi player chết
- **Cách hoạt động**:
  - Khi player chết, `ShowDeathScreen()` được gọi
  - Tự động lưu tên scene hiện tại để có thể restart
  - Load scene "DeathScene"

### 2. DeathSceneManager.cs
- **Vị trí**: `Assets/Scripts/DeathSceneManager.cs`
- **Chức năng**: Quản lý death scene và các button
- **Cách sử dụng**:
  - Thêm script này vào một GameObject trong DeathScene
  - Gán các button vào Inspector hoặc để script tự tìm

## Setup Death Scene

### Bước 1: Tạo Scene Mới

1. Trong Unity Editor: **File > New Scene**
2. Chọn **2D** template
3. Lưu scene với tên **"DeathScene"** (quan trọng: tên phải khớp với `deathSceneName` trong DeathManager)
4. Đảm bảo scene được thêm vào **Build Settings**:
   - **File > Build Settings**
   - Kéo scene "DeathScene" vào danh sách

### Bước 2: Tạo UI

1. **Tạo Canvas**:
   - Right-click trong Hierarchy > **UI > Canvas**
   - Đặt tên: "Canvas"

2. **Tạo Background Panel**:
   - Right-click Canvas > **UI > Panel**
   - Đặt tên: "BackgroundPanel"
   - Trong Inspector:
     - **Image > Color**: Đen với alpha 0.8-0.9
     - **RectTransform**: Fill toàn màn hình (Anchor: stretch-stretch)

3. **Tạo Title Text**:
   - Right-click Canvas > **UI > Text - TextMeshPro** (hoặc Text)
   - Đặt tên: "TitleText"
   - Trong Inspector:
     - **Text**: "YOU DIED" hoặc "GAME OVER"
     - **Font Size**: 72
     - **Alignment**: Center
     - **Color**: Đỏ hoặc trắng
   - **RectTransform**: Đặt ở giữa trên (Anchor: center-top)

4. **Tạo Button "Return to Menu"**:
   - Right-click Canvas > **UI > Button - TextMeshPro**
   - Đặt tên: "ReturnToMenuButton"
   - Trong Inspector:
     - **Button > On Click()**: 
       - Kéo GameObject có `DeathSceneManager` vào
       - Chọn method: `DeathSceneManager.ReturnToMainMenu()`
   - **RectTransform**: Đặt ở giữa dưới (Anchor: center-bottom, Y: -100)

5. **Tạo Button "Play Again"**:
   - Right-click Canvas > **UI > Button - TextMeshPro**
   - Đặt tên: "PlayAgainButton"
   - Trong Inspector:
     - **Button > On Click()**: 
       - Kéo GameObject có `DeathSceneManager` vào
       - Chọn method: `DeathSceneManager.PlayAgain()`
   - **RectTransform**: Đặt ở giữa dưới (Anchor: center-bottom, Y: -200)

### Bước 3: Setup DeathSceneManager

1. **Tạo GameObject**:
   - Right-click trong Hierarchy > **Create Empty**
   - Đặt tên: "DeathSceneManager"

2. **Thêm Component**:
   - Chọn GameObject "DeathSceneManager"
   - **Add Component > DeathSceneManager**

3. **Cấu hình trong Inspector**:
   - **Main Menu Scene Name**: "Menu" (hoặc tên scene menu của bạn)
   - **Allow Restart**: ✓ (checked)

### Bước 4: Setup DeathManager

1. Tìm GameObject có component `DeathManager` trong scene game (Dungeon1, Dungeon2, v.v.)
2. Trong Inspector, tìm `DeathManager` component:
   - **Death Scene Name**: "DeathScene" (phải khớp với tên scene bạn đã tạo)
   - **Main Menu Scene Name**: "Menu" (hoặc tên scene menu của bạn)

## Flow Hoạt Động

1. **Player chết**:
   - `PlayerHealth.Die()` được gọi
   - `PlayerDeath.HandleDeath()` được gọi
   - Death animation chạy
   - Sau khi animation xong, `DeathManager.ShowDeathScreen()` được gọi

2. **Load Death Scene**:
   - `DeathManager` lưu tên scene hiện tại
   - Load scene "DeathScene"
   - `DeathSceneManager` được khởi tạo

3. **Player chọn**:
   - **Return to Menu**: Load scene "Menu"
   - **Play Again**: Load lại scene đã chơi (đã được lưu)

## Layout Gợi Ý

```
DeathScene
├── Main Camera
├── Canvas
│   ├── BackgroundPanel (fill toàn màn hình, màu đen mờ)
│   ├── TitleText ("YOU DIED")
│   ├── ReturnToMenuButton
│   │   └── Text ("Return to Menu")
│   └── PlayAgainButton
│       └── Text ("Play Again")
└── DeathSceneManager (GameObject với DeathSceneManager component)
```

## Lưu Ý

- Tên scene "DeathScene" phải khớp với `deathSceneName` trong DeathManager
- Tên scene menu phải khớp với `mainMenuSceneName` trong cả DeathManager và DeathSceneManager
- Đảm bảo tất cả scene đều được thêm vào Build Settings
- DeathManager sử dụng DontDestroyOnLoad nên sẽ tồn tại qua các scene

## Testing

1. Chạy game và chơi đến khi player chết
2. Kiểm tra xem death scene có được load không
3. Test button "Return to Menu" → Phải quay về menu
4. Test button "Play Again" → Phải load lại level vừa chơi

