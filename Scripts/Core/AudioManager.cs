using Godot;
using System;
using BloodSeal.Core;
using BloodSeal.Combat;

namespace BloodSeal.Core
{
    public partial class AudioManager : Node
    {
        public static AudioManager Instance { get; private set; }

        private AudioStreamPlayer _bgmPlayer;
        private AudioStreamPlayer _sfxPlayer;
        private AudioStreamPlayer _slashPlayer;
        private AudioStreamPlayer _impactPlayer;

        // Cached AudioStreams
        private AudioStream _slashStream;
        private AudioStream _hitStream;
        private AudioStream _critStream;
        private AudioStream _coinStream;
        private AudioStream _tapStream;
        private AudioStream _enrageStream;
        private AudioStream _bgmStream;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
                return;
            }
        }

        public override void _Ready()
        {
            SetupPlayers();
            LoadStreams();
            PlayBGM();
        }

        private void SetupPlayers()
        {
            _bgmPlayer = new AudioStreamPlayer { Bus = "BGM", Autoplay = false };
            AddChild(_bgmPlayer);

            _sfxPlayer = new AudioStreamPlayer { Bus = "SFX" };
            AddChild(_sfxPlayer);

            _slashPlayer = new AudioStreamPlayer { Bus = "SFX" };
            AddChild(_slashPlayer);

            _impactPlayer = new AudioStreamPlayer { Bus = "SFX" };
            AddChild(_impactPlayer);
        }

        private void LoadStreams()
        {
            _slashStream = GD.Load<AudioStream>("res://Audio/SFX/slash.wav");
            _hitStream = GD.Load<AudioStream>("res://Audio/SFX/hit.wav");
            _critStream = GD.Load<AudioStream>("res://Audio/SFX/crit_hit.wav");
            _coinStream = GD.Load<AudioStream>("res://Audio/SFX/coin.wav");
            _tapStream = GD.Load<AudioStream>("res://Audio/SFX/tap.wav");
            _enrageStream = GD.Load<AudioStream>("res://Audio/SFX/boss_enrage.wav");
            _bgmStream = GD.Load<AudioStream>("res://Audio/BGM/gothic_ambient.wav");
        }

        public void PlayBGM()
        {
            if (_bgmStream != null && _bgmPlayer != null)
            {
                _bgmPlayer.Stream = _bgmStream;
                _bgmPlayer.VolumeDb = -6f;
                _bgmPlayer.Play();
            }
        }

        public void PlaySlash()
        {
            PlayWithPitch(_slashPlayer, _slashStream, -3f, 0.92f, 1.08f);
        }

        public void PlayHit(bool isCrit = false)
        {
            var stream = isCrit ? _critStream : _hitStream;
            float vol = isCrit ? 0f : -2f;
            PlayWithPitch(_impactPlayer, stream, vol, 0.95f, 1.05f);
        }

        public void PlayCoin()
        {
            PlayWithPitch(_sfxPlayer, _coinStream, -2f, 0.96f, 1.04f);
        }

        public void PlayTap()
        {
            PlayWithPitch(_sfxPlayer, _tapStream, -4f, 0.95f, 1.05f);
        }

        public void PlayEnrage()
        {
            PlayWithPitch(_sfxPlayer, _enrageStream, 2f, 0.98f, 1.02f);
        }

        private void PlayWithPitch(AudioStreamPlayer player, AudioStream stream, float volumeDb, float minPitch, float maxPitch)
        {
            if (player == null || stream == null) return;

            player.Stream = stream;
            player.VolumeDb = volumeDb;
            player.PitchScale = (float)GD.RandRange(minPitch, maxPitch);
            player.Play();
        }

        public void SetBusVolume(string busName, float linearVolume)
        {
            int busIdx = AudioServer.GetBusIndex(busName);
            if (busIdx >= 0)
            {
                float clamped = Mathf.Max(0.0001f, linearVolume);
                AudioServer.SetBusVolumeDb(busIdx, Mathf.LinearToDb(clamped));
            }
        }

        public float MasterVolume { get; private set; } = 1.0f;
        public float BgmVolume { get; private set; } = 0.8f;
        public float SfxVolume { get; private set; } = 1.0f;
        public bool IsMuted { get; private set; } = false;

        public void ApplySettings(float master, float bgm, float sfx, bool isMuted)
        {
            MasterVolume = master;
            BgmVolume = bgm;
            SfxVolume = sfx;
            IsMuted = isMuted;

            SetBusVolume("Master", isMuted ? 0.0001f : master);
            SetBusVolume("BGM", bgm);
            SetBusVolume("SFX", sfx);
            SetBusVolume("UI", sfx);
            SetBusMute("Master", isMuted);
        }

        public void SetBusMute(string busName, bool isMuted)
        {
            int busIdx = AudioServer.GetBusIndex(busName);
            if (busIdx >= 0)
            {
                AudioServer.SetBusMute(busIdx, isMuted);
            }
        }
    }
}
