# Hướng Dẫn Setup Death Manager cho Game Scenes

## Vấn Đề

Khi player chết trong các scene game (Dungeon1, Dungeon2, Dungeon3...), game không dừng và không hiện death scene.

## Nguyên Nhân

Các scene game thiếu **DeathManager** GameObject, nên `DeathManager.Instance` là `null` khi player chết.

## Giải Pháp

Có 2 cách để fix:

### Cách 1: Tự Động (Khuyến Nghị)

Script `PlayerHealth` đã được cập nhật để tự động tạo DeathManager nếu chưa có. Nhưng để đảm bảo, bạn có thể:

1. **Thêm DeathManagerAutoSetup vào scene**:
   - Mở scene game (Dungeon1, Dungeon2, v.v.)
   - Tạo GameObject trống: Right-click Hierarchy > Create Empty
   - Đặt tên: "DeathManagerAutoSetup"
   - Add Component > `DeathManagerAutoSetup`
   - Trong Inspector:
     - **Auto Create If Missing**: ✓ (checked)
     - **Death Scene Name**: "DeathScene"
     - **Main Menu Scene Name**: "Menu"

### Cách 2: Thủ Công (Chắc Chắn Hơn)

1. **Thêm DeathManager vào mỗi scene game**:
   - Mở scene game (Dungeon1, Dungeon2, v.v.)
   - Tạo GameObject trống: Right-click Hierarchy > Create Empty
   - Đặt tên: "DeathManager"
   - Add Component > `DeathManager`
   - Trong Inspector, cấu hình:
     - **Death Scene Name**: "DeathScene"
     - **Main Menu Scene Name**: "Menu"
     - **Pause Game On Death**: ✓ (checked)
     - **Allow Restart**: ✓ (checked)

2. **Lặp lại cho tất cả scene game** (Dungeon1, Dungeon2, Dungeon3, v.v.)

## Kiểm Tra

1. Chạy game và chơi đến khi player chết
2. Kiểm tra Console:
   - Phải thấy: "DeathManager: ShowDeathScreen() được gọi!"
   - Phải thấy: "DeathManager: Đã pause game (Time.timeScale = 0)"
   - Phải thấy: "DeathManager: Đang load death scene: DeathScene"
3. Game phải dừng ngay khi chết
4. Death scene phải được load

## Lưu Ý

- DeathManager sử dụng `DontDestroyOnLoad`, nên chỉ cần có 1 DeathManager trong scene đầu tiên, nó sẽ tồn tại qua các scene
- Nhưng để đảm bảo, nên thêm DeathManager vào mỗi scene game
- Hoặc dùng DeathManagerAutoSetup để tự động tạo nếu chưa có

## Nếu Vẫn Không Hoạt Động

1. Kiểm tra Console log:
   - Có thấy "DeathManager.Instance is null" không?
   - Có lỗi gì về scene không tồn tại không?

2. Kiểm tra Build Settings:
   - File > Build Settings
   - Đảm bảo scene "DeathScene" có trong danh sách

3. Kiểm tra DeathManager trong scene:
   - Chọn GameObject "DeathManager"
   - Trong Inspector, kiểm tra:
     - Death Scene Name = "DeathScene"
     - Main Menu Scene Name = "Menu"
     - Pause Game On Death = checked

