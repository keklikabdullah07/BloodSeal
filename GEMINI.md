# 🩸 BloodSeal: Antigravity & Agent Directive

Bu dosya Antigravity ve yapay zeka ajanları için proje yönergesidir. 
Tüm bağlayıcı mimari ve mühendislik standartları için **tek gerçek kaynak (Single Source of Truth)**: [`AGENTS.md`](file:///c:/Users/Partridge/Desktop/blood-seal/AGENTS.md)'dir.

## Teknik Temeller
- **Motor:** Godot 4.7.x Mono (C#)
- **Hedef Framework:** .NET 10.0 (`net10.0`, C# 12+)
- **Renderer:** Compatibility (GL Compatibility / OpenGL 3)
- **Çözünürlük:** 1920x1080 (16:9 Yatay), `stretch/mode = "canvas_items"`, `stretch/aspect = "expand"`

## Temel Prensipler Özeti
1. **"Call Down, Signal Up":** Ebeveynler alt nesneleri çağırır, alt nesneler C# event (`event Action`) veya Godot signal fırlatır. Asla `GetParent().GetParent()` yapılmaz.
2. **Bileşim Öncelikli (Composition over Inheritance):** Sınıflar 250 satır sınırında tutulur, god object oluşturulmaz.
3. **Güvenli Node Referansları:** `[Export]` veya `_Ready()` içinde `GetNodeOrNull<T>()`. Dinamik aktörlerde daima `IsInstanceValid()`.
4. **Yükseltme Matematiği:** GDD standardı gereği $Cost = BaseCost \times 1.15^{(Level - 1)}$ formülü esastır. Ayarlar `Data/BalanceConfig.json` üzerinden yönetilir.
5. **Kural ve Beceri Haritası (`.agents/skills/`):**
   - `bloodseal-balance`: GDD yükseltme maliyetleri, dalga büyümesi, Boss enrage, 6 saatlik korumalı offline ilerleme.
   - `bloodseal-art-style`: Gotik renk paleti, 3 katmanlı paralaks, ekran sarsıntısı ve bağımsız VFX ömrü.
   - `godot-csharp`: C# .NET döngüsü, tipler ve sinyaller.
   - `game-feel`: Vuruş dondurma (hitstop), ekran sarsıntısı, görsel tepkiler.
   - `godot-resources`: Veri odaklı konfigürasyonlar.
   - `save-systems`: Güvenli serileştirme ve durum kaydı.
   - `create-game-assets`: Görsel üretim hattı ve asset yönetimi.
   - `godot-audio`: Ses mimarisi ve SFX/BGM mikseri.
   - `godot-export`: Mobil ve masaüstü derleme/paketleme.

## Doğrulama Kapısı (Verification Gate)
Herhangi bir görev tamamlanmadan önce:
1. `dotnet build` çalıştırılır (0 uyarı, 0 hata zorunludur).
2. Godot headless testi çalıştırılır: `--headless --quit-after 60`.
