# 🔍 BloodSeal: Claude İçin Açık Kaynak Skill & Araç Araştırma İstemi

Bu istem, **BloodSeal** projesindeki doğrulanmış teknik eksiklikler ve riskler için harici yapay zekadan (Claude) açık kaynak beceri (skill), repo, MCP ve araç önerileri almak üzere hazırlanmıştır.

---

### Proje Özeti (5 Satır)
1. **Motor & Çerçeve:** Godot 4.7.2 Mono, C# (.NET 10.0 / `net10.0`), GL Compatibility Renderer.
2. **Tür & Format:** 2D Karanlık Gotik Idle RPG, 16:9 Yatay (1920x1080), yarı otomatik savaş döngüsü.
3. **Hedef Platform:** Çapraz platform (PC Windows + Mobil Android dokunmatik).
4. **Bağlayıcı Mimari:** "Call Down, Signal Up", sınıf başına maks 250 satır, `Engine.TimeScale` dokunulmazlığı, `double` ekonomi matematiği.
5. **Mevcut Kurulu Skill'ler:** `bloodseal-art-style`, `bloodseal-balance`, `game-feel`, `godot-csharp`, `godot-shaders`, `godot-signals-groups`, `godot-ui-control`, `save-systems`, `create-game-assets`, `game-ui-ux`, `performance-optimization`, `godot-audio`, `godot-export`, `camera-systems`, `godot-resources`.

---

### Araştırılması İstenen Teknik Sorunlar & Araç İhtiyaçları

Aşağıdaki her madde için açık kaynak bir **agent skill'i, GitHub reposu, CLI aracı veya MCP sunucusu** var mıdır?

1. **Android C# TargetFramework Uyuşmazlığı (`net10.0` vs `net9.0`):**
   - *Sorun:* Godot 4.7 Mono yerleşik Android export şablonu `net10.0` projesinde `C# project targets 'net10.0' but the export template only supports 'net9.0'` hatası vermektedir.
   - *Soru:* Godot 4.7 C# projelerini Android için Gradle build veya özel export hattı ile sorunsuz derleyen açık kaynak bir araç, CLI veya iş akışı skill'i var mı?

2. **Karanlık Gotik 2D Görsel Varlık Üretim ve Normalizasyon Hattı:**
   - *Sorun:* Projedeki tüm aktörler ve harabeler geçici `Polygon2D`'dir.
   - *Soru:* 2D sprite sheet, katmanlı paralaks arka plan ve gotik karakter/düşman piksellerini veya raster görsellerini otomatik boyutlandırıp, palet kontrolü yaparak Godot'ya aktaran açık kaynak bir asset pipeline skill'i / CLI aracı var mı?

3. **Mobil Dokunmatik UI Ölçekleme & Safe-Area Yönetimi:**
   - *Sorun:* 1920x1080 UI düğümleri farklı mobil ekran oranlarında (çentikli ekranlar, dar tabletler) dokunma alanı ve yazı boyutu sıkışması riski taşımaktadır.
   - *Soru:* Godot 4 Control düğümleri için dinamik ekran ölçekleme, çentik (notch/safe area) koruması ve dokunmatik geri bildirim sağlayan açık kaynak bir Godot C# UI kütüphanesi veya skill'i var mı?

4. **Mobil Performans & 2D Nesne Havuzu (Object Pooling):**
   - *Sorun:* Sürekli doğup ölen minyonlar, mermiler ve parçacıklar `Instantiate` / `QueueFree` ile mobilde çöp toplayıcı (GC) takılmalarına yol açabilir.
   - *Soru:* Godot 4 (.NET/C#) uyumlu, generic tip destekli, yüksek performanslı açık kaynak bir 2D Object Pool kütüphanesi veya hazır bileşen var mı?

5. **2D Ses & SFX/BGM Dinamik Mikser Yönetimi:**
   - *Sorun:* Projede şu an hiçbir ses mimarisi veya ses varlığı entegrasyonu bulunmamaktadır.
   - *Soru:* Godot 4 C# için prosedürel veya dosya tabanlı ses çalma, pitch varyasyonu (vuruşlarda sesin monotonlaşmasını önleme) ve audio bus mikserini otomatikleştiren açık kaynak bir ses yönetim kütüphanesi/skill'i var mı?

6. **Idle RPG Matematiksel İlerleme & Denge Simülatörü:**
   - *Sorun:* Doğrusal dalga ödülü ile üstel yükseltme maliyetinin ilerleyen aşamalarda tıkanıp tıkanmayacağını ölçen otomatikleştirilmiş simülasyon araçlarına ihtiyaç vardır.
   - *Soru:* Idle RPG ekonomisi, dalga eğrileri ve zaman analizini test eden açık kaynak bir C#/.NET test/simülasyon kütüphanesi veya CLI aracı var mı?

---

### İstenen Cevap Biçimi

Lütfen önerdiğin her araç için yalnızca şu şablonu kullan:

```markdown
- **Araç / Repo Adı:** [GitHub Linki veya Paket Adı]
- **Ne İşe Yarar:** [Kısa ve somut 1-2 cümlelik açıklama]
- **Godot 4.7 + C# Desteği:** [Destekliyor / Kısmen / Desteklemiyor / Doğrulanmadı]
- **Kurulum / Entegrasyon Komutu:** [Örn: dotnet add package ... veya npx skills add ...]
- **Doğrulama Notu:** [Emin olunmayan kısımlar açıkça belirtilecek]
```

### Kesin Yasaklar (Kurallar)
- **YASAK:** Mekanik veya oynanış tasarım önerisi verme.
- **YASAK:** Denge formülü veya katsayı tavsiyesi verme.
- **YASAK:** Proje için hazır C# gameplay kodu yazma.
- Yalnızca doğrulanabilir açık kaynak araçları, repoları ve kurulum adımlarını listele.
