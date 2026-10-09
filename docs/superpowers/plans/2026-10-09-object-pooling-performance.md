# 🩸 BloodSeal: Nesne Havuzu ve Performans Optimizasyonu Uygulama Planı

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Savaş alanında sürekli `Instantiate()` ve `QueueFree()` ile yaratılıp silinen Yüzen Metinler (FloatingText), Pet Mermileri (BloodProjectile), Görsel Efektler (Slash, Kan, Patlama, Ripple) ve Minyon Düşmanlar için Generic, tip güvenli bir `NodePool<T>` nesne havuzu kurarak bellek tahsisatı (allocation) ve Garbage Collection (GC) takılmalarını sıfırlamak.

**Architecture:** Saf C# ve Godot'dan bağımsız `ObjectPool<T>` çekirdeği ile xUnit testlenebilirliği; Godot `PackedScene` ve `Node` yaşam döngüsünü (`ProcessMode`, `Visible`, `IPoolable`) yöneten `NodePool<T>` sarmalayıcısı; alt sistemlerde (`FloatingTextManager`, `PetCompanion`, `FXManager`, `WaveSpawner`) pre-warm edilmiş havuzlar.

**Tech Stack:** Godot 4.7 Mono (C#), .NET 10.0, xUnit, C# 12+.

## Global Constraints
- Her C# dosyası kesinlikle 250 satır sınırının altında tutulacaktır (`AGENTS.md`).
- `Engine.TimeScale`'e asla dokunulmayacaktır.
- Ebeveynler alt nesneleri doğrudan çağırır, alt nesneler event/callback yayar ("Call Down, Signal Up").
- `dotnet build` 0 uyarı ve 0 hata ile derlenmelidir.
- Tüm xUnit birim testleri yeşil olmalıdır.

---

### Task 1: Çekirdek Havuz Modelleri (`ObjectPool<T>`, `NodePool<T>`, `IPoolable`) ve Birim Testleri
- [ ] Adım 1: `Tests/BloodSeal.Tests.csproj` içine `Scripts/Core/ObjectPool.cs` dosyasını derleme bağlantısı olarak ekle.
- [ ] Adım 2: `Tests/ObjectPoolTests.cs` dosyasını oluştur ve başarısız olan xUnit testlerini yaz (`Prewarm`, `Acquire`, `Release`, `DoubleReleasePrevention`, `IPoolableCallbacks`).
- [ ] Adım 3: Testlerin başarısız olduğunu doğrula (`dotnet test Tests/BloodSeal.Tests.csproj`).
- [ ] Adım 4: `Scripts/Core/ObjectPool.cs` sınıfını ve `IPoolable` arayüzünü kodla.
- [ ] Adım 5: Godot `PackedScene` ve `Node` desteği sunan `Scripts/Core/NodePool.cs` sınıfını kodla.
- [ ] Adım 6: Testlerin başarıyla geçtiğini doğrula (`dotnet test Tests/BloodSeal.Tests.csproj`).
- [ ] Adım 7: Task 1 için Git commit (`feat: implement generic ObjectPool and NodePool with unit tests`).

---

### Task 2: Yüzen Metinler Havuzu (`FloatingText.cs` & `FloatingTextManager.cs`)
- [ ] Adım 1: `Scripts/UI/FloatingText.cs` sınıfına `IPoolable` uygula, `OnReturnToPool` ve `OnSpawnFromPool` ile tween/opaklık durumlarını sıfırla.
- [ ] Adım 2: `FloatingText.cs` içinde tween bitişinde `QueueFree` yerine `FloatingTextManager.Instance.ReleaseText(this)` çağır.
- [ ] Adım 3: `Scripts/Combat/FloatingTextManager.cs` içinde `NodePool<FloatingText>` tanımla, `_Ready`'de 30 adet pre-warm et ve `Spawn...` metodlarını havuzdan alacak şekilde güncelle.
- [ ] Adım 4: `dotnet build` ve `dotnet test` ile doğrula.
- [ ] Adım 5: Task 2 için Git commit (`feat: integrate FloatingText with NodePool`).

---

### Task 3: Pet Kan Mermileri Havuzu (`BloodProjectile.cs` & `PetCompanion.cs`)
- [ ] Adım 1: `Scripts/Combat/BloodProjectile.cs` sınıfına `IPoolable` uygula, `OnReturnToPool` ile hedef/hız durumlarını temizle.
- [ ] Adım 2: `BloodProjectile.cs` içinde hedef yok olduğunda veya vuruş gerçekleştiğinde `QueueFree` yerine `PetCompanion.Instance.ReleaseProjectile(this)` çağır.
- [ ] Adım 3: `Scripts/Combat/PetCompanion.cs` içinde `NodePool<BloodProjectile>` tanımla, 15 adet pre-warm et ve saldırı anında mermiyi havuzdan edin.
- [ ] Adım 4: `dotnet build` ve `dotnet test` ile doğrula.
- [ ] Adım 5: Task 3 için Git commit (`feat: integrate PetCompanion BloodProjectile with NodePool`).

---

### Task 4: Savaş Görsel Efektleri Havuzu (`FXManager.cs`, `SlashEffect.cs`, `OneShotParticle.cs`, `TapRipple.cs`)
- [ ] Adım 1: `SlashEffect.cs` ve `TapRipple.cs` sınıflarına `IPoolable` uygula, tween bitiminde `FXManager` iade çağrılarını bağla.
- [ ] Adım 2: `OneShotParticle.cs` sınıfına `IPoolable` uygula, parçacık süresi bittiğinde havuzuna dönmesini sağla.
- [ ] Adım 3: `Scripts/Combat/FXManager.cs` içinde Slash (10), BloodSplatter (15), DeathExplosion (10) ve TapRipple (10) için havuzları kur ve oynatma metodlarını güncelle.
- [ ] Adım 4: `dotnet build` ve `dotnet test` ile doğrula.
- [ ] Adım 5: Task 4 için Git commit (`feat: pool combat visual effects in FXManager`).

---

### Task 5: Minyon Düşman Havuzu (`WaveSpawner.cs` & `Enemy.cs`)
- [ ] Adım 1: `Scripts/Combat/Enemy.cs` sınıfına `IPoolable` uygula; `OnReturnToPool` içinde can, sağlık çubuğu ve ölüm bayrağını temizle.
- [ ] Adım 2: `Enemy.Die()` içinde `QueueFree` yerine `WaveSpawner.Instance.ReleaseEnemy(this)` çağır.
- [ ] Adım 3: `Scripts/Combat/WaveSpawner.cs` içinde 15 adet minyon için `NodePool<Enemy>` kur, `SpawnEnemy` ve `ClearAllEnemies` metodlarını havuza uyarla.
- [ ] Adım 4: `dotnet build` ve `dotnet test` ile doğrula.
- [ ] Adım 5: Task 5 için Git commit (`feat: pool minion enemies in WaveSpawner`).

---

### Task 6: Tam Doğrulama, Headless Test ve Rapor Güncellemesi (Verification Gate)
- [ ] Adım 1: Tüm xUnit testlerini çalıştır (`dotnet test Tests/BloodSeal.Tests.csproj`).
- [ ] Adım 2: `dotnet build` ile sıfır uyarı ve sıfır hata olduğunu doğrula.
- [ ] Adım 3: Godot headless smoke testini çalıştır (`--headless --quit-after 60`).
- [ ] Adım 4: Tüm C# dosyalarının 250 satır sınırının altında olduğunu doğrula.
- [ ] Adım 5: `Reports/PROJECT_STATE.md` dosyasında `performance-optimization` yeteneğini "Kullanılıyor" olarak güncelle.
- [ ] Adım 6: Task 6 için Git commit (`docs: update PROJECT_STATE.md with completed Object Pooling`).
