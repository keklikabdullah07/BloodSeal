# 🩸 BloodSeal: Nesne Havuzu ve Performans Optimizasyonu Tasarım Belgesi (Spec)

## 1. Genel Bakış ve Amaç
BloodSeal, yüksek vuruş frekansına, pet mermilerine, yüzen hasar/altın metinlerine ve sürekli dalga döngüsüne sahip karanlık bir idle RPG oyunudur. Mevcut sistemde her vuruşta, mermi fırlatılışında, efekt patlamasında ve minyon ölümünde Godot'nun `PackedScene.Instantiate()` ve `Node.QueueFree()` çağrıları çalıştırılmaktadır.

Bu durum özellikle mobil cihazlarda (Android GL Compatibility) sık çalışan Managed Heap tahsisatlarına (allocation) ve akabinde Garbage Collector (GC) duraksamalarına (frame hitch / stutter) neden olmaktadır.

Bu belgenin amacı:
- Generic, tip güvenli ve `IPoolable` arayüzüne dayalı bir **`NodePool<T>`** yapısı kurmak.
- **FloatingTextManager** (hasar/altın/enrage metinleri), **PetCompanion** (kan mermileri), **FXManager** (savurma, kan sıçraması, ölüm patlaması, tıklama dalgası) ve **WaveSpawner** (minyon düşmanlar) bileşenlerini nesne havuzuna bağlamak.
- `AGENTS.md` 250 satır kuralına, "Call Down, Signal Up" prensibine ve 0 warning/0 error derleme standartlarına %100 uymak.

---

## 2. Mimari Tasarım ve Bileşenler

### A. Çekirdek Havuz Yapısı (`Scripts/Core/NodePool.cs`)
Tip güvenli, sahneye bağlı ve dinamik genişleyebilir generic havuz sınıfı:

```csharp
namespace BloodSeal.Core
{
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }

    public class NodePool<T> where T : Node
    {
        private readonly PackedScene _scene;
        private readonly Node _parent;
        private readonly Stack<T> _available = new();
        private readonly HashSet<T> _active = new();
        public int TotalCount => _available.Count + _active.Count;
        public int ActiveCount => _active.Count;

        public NodePool(PackedScene scene, Node parent, int initialCapacity = 0)
        {
            _scene = scene;
            _parent = parent;
            if (initialCapacity > 0) Prewarm(initialCapacity);
        }

        public void Prewarm(int count);
        public T Acquire();
        public void Release(T item);
        public void Clear();
    }
}
```

- **Uyku Durumu Yönetimi:** Havuzdaki nesneler `Visible = false`, `ProcessMode = ProcessModeEnum.Disabled` (veya `SetPhysicsProcess(false)`) durumuna alınır; bu sayede dinlenen nesneler GPU/CPU döngüsü tüketmez.
- **Uyanma Durumu Yönetimi:** `Acquire()` ile çağrılan nesneler `Visible = true`, `ProcessMode = ProcessModeEnum.Inherit` yapılır ve `IPoolable.OnSpawnFromPool()` tetiklenir.

---

### B. Havuzlanacak Alt Sistemler ve Kapasiteleri

| Alt Sistem | Hedef Sınıf / Sahne | Başlangıç Kapasitesi (`Prewarm`) | Tetikleyici & İade Noktası |
| :--- | :--- | :---: | :--- |
| **Yüzen Metinler** | [`FloatingText`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/UI/FloatingText.cs)<br>[`FloatingTextManager`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/FloatingTextManager.cs) | 30 | Tween tamamlandığında `QueueFree` yerine `FloatingTextManager.Instance.ReleaseText(this)` |
| **Pet Kan Mermileri** | [`BloodProjectile`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/BloodProjectile.cs)<br>[`PetCompanion`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/PetCompanion.cs) | 15 | Hedefe ulaştığında veya hedef geçersizleştiğinde `PetCompanion.Instance.ReleaseProjectile(this)` |
| **VFX: Kılıç Savurma** | [`SlashEffect`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/SlashEffect.cs)<br>[`FXManager`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/FXManager.cs) | 10 | Tween tamamlandığında `FXManager.Instance.ReleaseSlash(this)` |
| **VFX: Kan Sıçraması** | [`OneShotParticle`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/OneShotParticle.cs)<br>[`FXManager`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/FXManager.cs) | 15 | Parçacık ömrü bittiğinde havuzuna döner |
| **VFX: Ölüm Patlaması** | [`OneShotParticle`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/OneShotParticle.cs)<br>[`FXManager`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/FXManager.cs) | 10 | Parçacık ömrü bittiğinde havuzuna döner |
| **VFX: Tıklama Dalgası** | [`TapRipple`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/TapRipple.cs)<br>[`FXManager`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/FXManager.cs) | 10 | Tween tamamlandığında `FXManager.Instance.ReleaseRipple(this)` |
| **Minyon Düşmanlar** | [`Enemy`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/Enemy.cs)<br>[`WaveSpawner`](file:///c:/Users/Partridge/Desktop/blood-seal/Scripts/Combat/WaveSpawner.cs) | 15 | Öldüğünde (`Die()`) veya dalga temizliğinde `WaveSpawner.Instance.ReleaseEnemy(this)` |

---

### C. Yaşam Döngüsü ve Durum Sıfırlama Kuralları

1. **`FloatingText`**:
   - `OnReturnToPool`: Aktif tween varsa `Kill()`, `Modulate = Colors.White`.
   - `OnSpawnFromPool`: Pozisyon ve metin `Setup` ile atanır, opaklık (`modulate:a = 1`) sıfırlanır.
2. **`BloodProjectile`**:
   - `OnReturnToPool`: `Target = null`, `GlobalPosition = Vector2.Zero`.
   - `OnSpawnFromPool`: Yeni hedef ve hasar atanır, görünürlük açılır.
3. **`Enemy` (Minyon)**:
   - `OnReturnToPool`: `IsDead = false`, `CurrentHp = MaxHp`, `TargetHero = null`, sağlık çubuğu gizlenir, `Enemies` grubundaki durumu yönetilir.
   - `OnSpawnFromPool`: `Setup(wave, hero)` ile statlar dalga seviyesine göre yeniden başlatılır.
4. **VFX Öğeleri**:
   - Tween ve parçacıklar yeniden başlatılır (`Emitting = true`, `Restart()`).

---

## 3. Kod Dosyası Boyutları & AGENTS.md Disiplini
Her sınıfın satır sayısı 250 limitinin altında tutulacaktır:
- `Scripts/Core/NodePool.cs`: ~90 satır (< 250)
- `Scripts/Combat/FloatingTextManager.cs`: ~80 satır (< 250)
- `Scripts/UI/FloatingText.cs`: ~55 satır (< 250)
- `Scripts/Combat/PetCompanion.cs`: ~110 satır (< 250)
- `Scripts/Combat/BloodProjectile.cs`: ~45 satır (< 250)
- `Scripts/Combat/FXManager.cs`: ~150 satır (< 250)
- `Scripts/Combat/WaveSpawner.cs`: ~145 satır (< 250)
- `Scripts/Combat/Enemy.cs`: ~145 satır (< 250)

---

## 4. Test ve Doğrulama Planı

1. **Birim Testleri (`Tests/ObjectPoolTests.cs`):**
   - Havuz oluşturma ve `Prewarm` kapasite doğrulaması.
   - `Acquire()` ile nesne alma ve `Release()` ile geri bırakma döngüsü.
   - Havuz tükendiğinde dinamik genişleme (`Auto-expand`) doğrulaması.
   - `IPoolable` arayüzünün (`OnSpawnFromPool`, `OnReturnToPool`) doğru sırada çağrıldığının testi.
2. **Derleme Doğrulaması:**
   - `dotnet build` (0 uyarı, 0 hata).
   - `dotnet test Tests/BloodSeal.Tests.csproj` (tüm testler yeşil).
3. **Godot Headless Smoke Test:**
   - Godot konsol ile `--headless --quit-after 60` testi.
   - Memory sızıntısı veya sahne ağacı kopukluğu olmadığını doğrulama.
