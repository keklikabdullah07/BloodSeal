#nullable enable
using Godot;
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public partial class AudioManager : Node
    {
        public static AudioManager? Instance { get; private set; }

        private AudioStreamPlayer _bgmTrackA = null!;
        private AudioStreamPlayer _bgmTrackB = null!;
        private bool _isUsingTrackA = true;
        private BgmTrackType? _currentTrack = null;

        private AudioStreamPlayer[] _sfxPool = new AudioStreamPlayer[6];
        private int _poolIndex = 0;
        private AudioStreamPlayer _priorityPlayer = null!;

        private readonly Dictionary<AudioCueType, AudioStream> _cueStreams = new();
        private readonly Dictionary<BgmTrackType, AudioStream> _bgmStreams = new();

        public float MasterVolume { get; private set; } = 1.0f;
        public float BgmVolume { get; private set; } = 0.8f;
        public float SfxVolume { get; private set; } = 1.0f;
        public bool IsMuted { get; private set; } = false;

        public override void _EnterTree()
        {
            if (Instance == null) Instance = this;
            else { QueueFree(); return; }
        }

        public override void _Ready()
        {
            SetupAudioPlayers();
            LoadAudioStreams();
            PlayBGM(BgmTrackType.GothicAmbient, 0.5f);
        }

        private void SetupAudioPlayers()
        {
            _bgmTrackA = new AudioStreamPlayer { Bus = "BGM" };
            _bgmTrackB = new AudioStreamPlayer { Bus = "BGM" };
            AddChild(_bgmTrackA);
            AddChild(_bgmTrackB);

            for (int i = 0; i < _sfxPool.Length; i++)
            {
                _sfxPool[i] = new AudioStreamPlayer { Bus = "SFX" };
                AddChild(_sfxPool[i]);
            }

            _priorityPlayer = new AudioStreamPlayer { Bus = "SFX" };
            AddChild(_priorityPlayer);
        }

        private void LoadAudioStreams()
        {
            LoadCue(AudioCueType.Slash, "res://Audio/SFX/slash.wav");
            LoadCue(AudioCueType.Hit, "res://Audio/SFX/hit.wav");
            LoadCue(AudioCueType.CritHit, "res://Audio/SFX/crit_hit.wav");
            LoadCue(AudioCueType.Tap, "res://Audio/SFX/tap.wav");
            LoadCue(AudioCueType.RageBurst, "res://Audio/SFX/rage_burst.wav");
            LoadCue(AudioCueType.BossEnrage, "res://Audio/SFX/boss_enrage.wav");
            LoadCue(AudioCueType.Coin, "res://Audio/SFX/coin.wav");
            LoadCue(AudioCueType.BossVictory, "res://Audio/SFX/boss_victory.wav");
            LoadCue(AudioCueType.HeroDeath, "res://Audio/SFX/hero_death.wav");
            LoadCue(AudioCueType.RelicUnlock, "res://Audio/SFX/relic_unlock.wav");
            LoadCue(AudioCueType.AwakeningRitual, "res://Audio/SFX/awakening_ritual.wav");
            LoadCue(AudioCueType.ButtonClick, "res://Audio/SFX/button_click.wav");
            LoadCue(AudioCueType.ModalOpen, "res://Audio/SFX/modal_open.wav");
            LoadCue(AudioCueType.ModalClose, "res://Audio/SFX/modal_close.wav");

            LoadBgm(BgmTrackType.GothicAmbient, "res://Audio/BGM/gothic_ambient.wav");
            LoadBgm(BgmTrackType.BossCombat, "res://Audio/BGM/boss_combat.wav");
        }

        private void LoadCue(AudioCueType cue, string path)
        {
            if (ResourceLoader.Exists(path))
            {
                var s = GD.Load<AudioStream>(path);
                if (s != null) _cueStreams[cue] = s;
            }
        }

        private void LoadBgm(BgmTrackType track, string path)
        {
            if (ResourceLoader.Exists(path))
            {
                var s = GD.Load<AudioStream>(path);
                if (s != null) _bgmStreams[track] = s;
            }
        }

        public void PlayBGM(BgmTrackType track, float duration = 1.2f)
        {
            if (_currentTrack == track) return;
            if (!_bgmStreams.TryGetValue(track, out var stream)) return;

            var activePlayer = _isUsingTrackA ? _bgmTrackA : _bgmTrackB;
            var nextPlayer = _isUsingTrackA ? _bgmTrackB : _bgmTrackA;
            _isUsingTrackA = !_isUsingTrackA;
            _currentTrack = track;

            nextPlayer.Stream = stream;
            nextPlayer.VolumeDb = -40f;
            nextPlayer.Play();

            var tween = CreateTween().SetParallel(true);
            tween.TweenProperty(nextPlayer, "volume_db", -4.0f, duration);
            if (activePlayer.Playing)
            {
                tween.TweenProperty(activePlayer, "volume_db", -40.0f, duration);
                tween.Chain().TweenCallback(Callable.From(activePlayer.Stop));
            }
        }

        public void PlaySFX(AudioCueType cue, float pitchMin = 0.95f, float pitchMax = 1.05f)
        {
            if (!_cueStreams.TryGetValue(cue, out var stream)) return;

            bool isPriority = cue is AudioCueType.CritHit or AudioCueType.RageBurst or AudioCueType.BossVictory or AudioCueType.AwakeningRitual;
            var player = isPriority ? _priorityPlayer : GetNextPoolPlayer();

            player.Stream = stream;
            player.VolumeDb = cue switch
            {
                AudioCueType.CritHit => 1.5f,
                AudioCueType.RageBurst => 2.0f,
                AudioCueType.BossVictory => 1.0f,
                AudioCueType.ButtonClick => -2.0f,
                AudioCueType.Tap => -3.0f,
                _ => 0.0f
            };
            player.PitchScale = (float)GD.RandRange(pitchMin, pitchMax);
            player.Play();
        }

        private AudioStreamPlayer GetNextPoolPlayer()
        {
            var p = _sfxPool[_poolIndex];
            _poolIndex = (_poolIndex + 1) % _sfxPool.Length;
            return p;
        }

        // Convenience & backwards-compatibility methods
        public void PlaySlash() => PlaySFX(AudioCueType.Slash, 0.92f, 1.08f);
        public void PlayHit(bool isCrit = false) => PlaySFX(isCrit ? AudioCueType.CritHit : AudioCueType.Hit, 0.95f, 1.05f);
        public void PlayCoin() => PlaySFX(AudioCueType.Coin, 0.96f, 1.04f);
        public void PlayTap() => PlaySFX(AudioCueType.Tap, 0.95f, 1.05f);
        public void PlayEnrage() => PlaySFX(AudioCueType.BossEnrage, 0.98f, 1.02f);
        public void PlayHeroDeath() => PlaySFX(AudioCueType.HeroDeath, 0.98f, 1.02f);
        public void PlayBossVictory() => PlaySFX(AudioCueType.BossVictory, 1.0f, 1.0f);
        public void PlayRageBurst() => PlaySFX(AudioCueType.RageBurst, 0.98f, 1.02f);
        public void PlayRelicUnlock() => PlaySFX(AudioCueType.RelicUnlock, 0.98f, 1.02f);
        public void PlayAwakeningRitual() => PlaySFX(AudioCueType.AwakeningRitual, 1.0f, 1.0f);
        public void PlayButtonClick() => PlaySFX(AudioCueType.ButtonClick, 0.98f, 1.02f);
        public void PlayModalOpen() => PlaySFX(AudioCueType.ModalOpen, 0.98f, 1.02f);
        public void PlayModalClose() => PlaySFX(AudioCueType.ModalClose, 0.98f, 1.02f);

        public void ApplySettings(float master, float bgm, float sfx, bool isMuted)
        {
            MasterVolume = master;
            BgmVolume = bgm;
            SfxVolume = sfx;
            IsMuted = isMuted;

            SetBusVolume("Master", isMuted ? 0.0f : master);
            SetBusVolume("BGM", bgm);
            SetBusVolume("SFX", sfx);
            SetBusVolume("UI", sfx);
            SetBusMute("Master", isMuted);
        }

        public void SetMasterVolume(float vol) => ApplySettings(vol, BgmVolume, SfxVolume, IsMuted);
        public void SetBgmVolume(float vol) => ApplySettings(MasterVolume, vol, SfxVolume, IsMuted);
        public void SetSfxVolume(float vol) => ApplySettings(MasterVolume, BgmVolume, vol, IsMuted);
        public void SetMuted(bool muted) => ApplySettings(MasterVolume, BgmVolume, SfxVolume, muted);

        public void SetBusVolume(string busName, float linearVolume)
        {
            int busIdx = AudioServer.GetBusIndex(busName);
            if (busIdx >= 0)
            {
                AudioServer.SetBusVolumeDb(busIdx, AudioData.LinearToDb(linearVolume));
            }
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
