# 🩸 BloodSeal: Antigravity & Agent Directive

Bu dosya Antigravity ve yapay zeka ajanları için doğrudan bağlayıcı ana kuralları içerir.
Detaylı mimari el kitabı için ek kaynak: [`AGENTS.md`](file:///c:/Users/Partridge/Desktop/blood-seal/AGENTS.md).

## Teknik Temeller
- **Motor:** Godot 4.7.x Mono (C#)
- **Hedef Framework:** .NET 10.0 (`net10.0`, C# 12+)
- **Renderer:** Compatibility (GL Compatibility / OpenGL 3)
- **Çözünürlük:** 1920x1080 (16:9 Yatay), `stretch/mode = "canvas_items"`, `stretch/aspect = "expand"`

## 6 Kritik Mimari & Oynanış Kuralı (Doğrudan Bağlayıcı)
1. **"Call Down, Signal Up" & Sınıf Sınırı:** Ebeveynler alt nesneleri çağırır, alt nesneler C# event (`event Action`) veya signal yayar (`GetParent().GetParent()` yasaktır). Her sınıf maksimum **250 satır** ile sınırlıdır.
2. **Game Feel & Asla Engine.TimeScale Değiştirme:** `Engine.TimeScale`'e asla dokunulmaz. Hit-freeze yalnızca yerel aktör görselini ~40 ms dondurur. Yalnızca boss kritiği/ölümünde (min 0.4s bekleme) çalışır, Berserk'te kapalıdır.
3. **Kamera Sarsıntısı (Trauma):** Trauma `Clamp(0, 1)` ile sınırlandırılır. Sönüm hızı sabit `1.5/sn`'dir. Normal vuruş 0.10, kritik 0.20, hasar alma 0.25, boss ölümü 0.50. Berserk modunda vuruş başı shake eklenmez, sabit taban 0.15 tutulur.
4. **Gerçek Zamanlı Sayaçlar:** Enrage (5s), Berserk (10s), gelir ve offline sayaçları gerçek delta ile hesaplanır; görsel efektler bunları durduramaz. Gelir için `Timer` node'u kullanılmaz.
5. **Ekonomi & Sayı Hijyeni:** Para ve hasar hesaplarında `double` kullanılır (taşma önleme). Yükseltme maliyet formülü $Cost = BaseCost \times 1.15^{(Level - 1)}$'dir. Değerler `Data/BalanceConfig.json` üzerinden yönetilir. Çevrimdışı altın en son temizlenen güvenli farm dalgası ($Wave - 1$) üzerinden ve max 6 saat (21.600s) korumalı hesaplanır.
6. **Güvenli Node Referansları:** Dinamik aktörlerde daima `IsInstanceValid(node)` kontrolü yapılır. Sahne düğümleri `[Export]` veya `_Ready()` içinde `GetNodeOrNull<T>()` ile alınır.

## Doğrulama Kapısı (Verification Gate)
Herhangi bir görev tamamlandı sayılmadan önce şu 3 adım geçilmelidir:
1. `dotnet build` (0 uyarı, 0 hata).
2. Denge ve formül C# birim testleri (unit tests) başarıyla geçmeli.
3. Godot headless testi (`--headless --quit-after 60`) ve Android export ön doğrulaması (`godot --export-debug "Android"` veya template kontrolü).
