using Godot;
using System.Collections.Generic;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class WaveSpawner : Node2D
    {
        [Export] public PackedScene EnemyScene;
        [Export] public PackedScene BossScene;
        [Export] public Hero TargetHero;
        [Export] public Vector2 SpawnPosition = new Vector2(2050, 700);

        public static WaveSpawner Instance { get; private set; }

        private NodePool<Enemy> _enemyPool;
        private int _enemiesRemainingToSpawn = 0;
        private double _spawnCooldown = 0.0;
        private bool _isWaveActive = false;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            Instance = this;
            if (EnemyScene != null)
            {
                _enemyPool = new NodePool<Enemy>(EnemyScene, this, 15);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveChanged += StartWave;
                GameManager.Instance.OnHeroDied += ClearAllEnemies;
                StartWave(GameManager.Instance.CurrentWave, GameManager.Instance.CurrentWave % 10 == 0);
            }
        }

        public void ReleaseEnemy(Enemy enemy)
        {
            if (enemy == null) return;
            if (enemy is not BossEnemy && _enemyPool != null)
            {
                _enemyPool.Release(enemy);
            }
            else
            {
                enemy.QueueFree();
            }
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveChanged -= StartWave;
                GameManager.Instance.OnHeroDied -= ClearAllEnemies;
            }
        }

        public void StartWave(int wave, bool isBoss)
        {
            ClearAllEnemies();
            _isWaveActive = true;

            if (isBoss)
            {
                _enemiesRemainingToSpawn = 1;
                SpawnBoss(wave);
            }
            else
            {
                _enemiesRemainingToSpawn = 5;
                _spawnCooldown = 0.2;
            }
        }

        public override void _Process(double delta)
        {
            if (!_isWaveActive) return;

            if (_enemiesRemainingToSpawn > 0)
            {
                _spawnCooldown -= delta;
                if (_spawnCooldown <= 0.0)
                {
                    _spawnCooldown = 1.3;
                    SpawnEnemy(GameManager.Instance.CurrentWave);
                    _enemiesRemainingToSpawn--;
                }
            }
            else
            {
                // Check if all enemies defeated
                var activeEnemies = GetTree().GetNodesInGroup("Enemies");
                if (activeEnemies.Count == 0)
                {
                    _isWaveActive = false;
                    int curWave = GameManager.Instance.CurrentWave;
                    GameManager.Instance.AdvanceWave();

                    // Wave Leap (Dalga Sıçraması - non-boss only)
                    if (curWave % 10 != 0 && (curWave + 1) % 10 != 0)
                    {
                        float leapChance = AwakeningManager.Instance?.GetWaveLeapChance() ?? 0.0f;
                        if (leapChance > 0f && GD.Randf() < leapChance)
                        {
                            GameManager.Instance.AdvanceWave();
                        }
                    }
                }
            }
        }

        private void SpawnEnemy(int wave)
        {
            if (TargetHero == null) return;
            var enemy = _enemyPool != null ? _enemyPool.Acquire() : EnemyScene?.Instantiate<Enemy>();
            if (enemy == null) return;
            enemy.GlobalPosition = SpawnPosition + new Vector2(0, (float)GD.RandRange(-25, 25));
            enemy.Setup(wave, TargetHero);
            if (_enemyPool == null)
            {
                AddChild(enemy);
            }
        }

        private void SpawnBoss(int wave)
        {
            if (BossScene == null || TargetHero == null) return;
            var boss = BossScene.Instantiate<BossEnemy>();
            boss.GlobalPosition = SpawnPosition;
            boss.Setup(wave, TargetHero);
            AddChild(boss);
            _enemiesRemainingToSpawn = 0;
        }

        private void ClearAllEnemies()
        {
            var enemies = GetTree().GetNodesInGroup("Enemies");
            foreach (var node in enemies)
            {
                if (node is Enemy enemy)
                {
                    ReleaseEnemy(enemy);
                }
                else if (node is Node n)
                {
                    n.QueueFree();
                }
            }
            _enemiesRemainingToSpawn = 0;
        }
    }
}
