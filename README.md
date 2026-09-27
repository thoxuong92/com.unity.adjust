# Unity Adjust Service (UPM Package)

Package module tích hợp giải pháp phân bổ cài đặt người dùng (**Attribution Tracking**), sự kiện trong ứng dụng (**In-App Events**), liên kết sâu (**Deep Linking**), SKAdNetwork và theo dõi doanh thu quảng cáo (**Impression-Level Ad Revenue**) từ **Adjust** cho **Unity Core Framework**.

Được tích hợp sẵn với kiến trúc **Pluggable Service Bridge** (`Unity.Core`), tự động bắt doanh thu quảng cáo từ `Unity.Core.Services.Ads` (AppLovin MAX / AdMob) và bắn sang Adjust.

---

## 🚀 Các Tính Năng Nổi Bật

1. **Attribution, Deep Linking & SKAdNetwork**:
   - Tự động tích hợp với `Unity.Core.Services.Analytics.AnalyticsService` và `Unity.Core.Services.Tracking.TrackingService`.
   - Tự động xử lý Cold Start & Runtime Deep Links (`Adjust.ProcessDeeplink`).
   - Tích hợp sẵn SKAdNetwork, Apple AdServices và IDFA Reading.

2. **Impression-Level Ad Revenue (ILR) Tracking**:
   - Tự động bắt `AdRevenueInfo` từ `TrackingService` và bắn `Adjust.TrackAdRevenue()` với nguồn `applovin_max_sdk` hoặc `admob_sdk`.

3. **Unity Editor Setup Tool**:
   - Menu: **`Unity Core > Adjust > Setup & Configuration`**.
   - Quản lý và lưu `AppToken`, `Environment` (Production / Sandbox) vào `Assets/Resources/Info.json`.

4. **IL2CPP Stripping Safe**:
   - Kèm file `link.xml` bảo vệ các symbol của Adjust SDK khỏi bị strip khi build release với Managed Stripping Level = High.

---

## 📦 Cài Đặt Vào Dự Án

### Cách 1: Cài đặt qua Git URL trong Unity Package Manager
1. Mở Unity Editor: **Window** > **Package Manager**.
2. Nhấn vào dấu **`+`** (góc trên bên trái) > chọn **Add package from git URL...**
3. Nhập:
   ```text
   https://github.com/thoxuong92/com.unity.adjust.git
   ```

### Cách 2: Qua file `Packages/manifest.json`
Thêm dependency trỏ tới kho lưu trữ GitHub:
```json
{
  "dependencies": {
    "com.unity.core": "https://github.com/thoxuong92/com.unity.core.git",
    "com.unity.adjust": "https://github.com/thoxuong92/com.unity.adjust.git"
  }
}
```

---

## 🛠️ Hướng Dẫn Sử Dụng Code

Gameplay và UI gọi thông qua `Unity.Core.Services.Analytics` hoặc `Unity.Core.Services.Tracking`:

```csharp
using System.Collections.Generic;
using Unity.Core.Services.Analytics;
using Unity.Core.Services.Tracking;

// 1. Gửi sự kiện In-App Event
AnalyticsService.LogEvent("event_token_abc123", new Dictionary<string, object>
{
    ["level"] = 10,
    ["score"] = 5000
});

// 2. Theo dõi chuyển đổi với doanh thu
TrackingService.TrackEvent("purchase_token_xyz", 4.99, "USD");
```

---

## 👨‍💻 Tác Giả & Bản Quyền
- **Tác giả**: **joukyuu**
- **Repository**: [thoxuong92/com.unity.adjust](https://github.com/thoxuong92/com.unity.adjust.git)
