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

        private int _enemiesRemainingToSpawn = 0;
        private double _spawnCooldown = 0.0;
        private bool _isWaveActive = false;

        public override void _Ready()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveChanged += StartWave;
                GameManager.Instance.OnHeroDied += ClearAllEnemies;
                StartWave(GameManager.Instance.CurrentWave, GameManager.Instance.CurrentWave % 10 == 0);
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
                    GameManager.Instance.AdvanceWave();
                }
            }
        }

        private void SpawnEnemy(int wave)
        {
            if (EnemyScene == null || TargetHero == null) return;
            var enemy = EnemyScene.Instantiate<Enemy>();
            enemy.GlobalPosition = SpawnPosition + new Vector2(0, (float)GD.RandRange(-25, 25));
            enemy.Setup(wave, TargetHero);
            AddChild(enemy);
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
                if (node is Node n) n.QueueFree();
            }
            _enemiesRemainingToSpawn = 0;
        }
    }
}
