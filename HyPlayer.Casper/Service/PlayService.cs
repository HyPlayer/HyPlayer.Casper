using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Media.Audio;
using Windows.Media.Core;
using HyPlayer.Casper.Properties;

namespace HyPlayer.Casper.Service
{
    public abstract class PlayService
    {
        public string Id;
        public string Name;
        public bool Available;
        public string LastError; // Win32 时代的产物
        public PlayServiceAbility Abilities;
        public PlayServiceEvents Events;
        public PlayServiceStatus Status;

        public abstract void InitializeService();
        public abstract void Load(MediaSource mediaSource);
        public abstract void Play();
        public abstract void Pause();
        public abstract void Stop();
        public abstract void Seek(TimeSpan timeSpan);
        public abstract void ChangeOutputDevice(AudioDeviceOutputNode audioDeviceOutputNode);
        public abstract void ChangeVolume(int volume);
        public abstract void ChangePlaybackRate(int rate);
        public abstract void SwitchBackground();
    }

    public abstract class EffectivePlayService : PlayService // 这个命名好好想想
    {
        public EchoEffectDefinition EchoEffectDefinition;
        public ReverbEffectDefinition ReverbEffectDefinition;
        public EqualizerEffectDefinition EqEffectDefinition;
        public LimiterEffectDefinition LimiterEffectDefinition;
        public abstract void InitializeEffect();
    }

    public class PlayServiceAbility
    {
        public bool Play; // 播放歌曲
        public bool Pause; // 暂停歌曲
        public bool Stop; // 停止歌曲 (卸载歌曲)
        public bool Load; // 加载歌曲
        public bool Seek; // 切换时长
        public bool ChangeVolume; // 更改音量
        public bool ChangePlaybackRate; // 更改倍速
        public bool PlayBackground; // 后台播放
        public bool ChangeOutputDevice; // 更改输出设备
    }

    public enum PlayingStatus
    {
        Paused,
        Playing,
        Failed,
        None
    }

    public class PlayServiceStatus : INotifyPropertyChanged
    {
        public PlayingStatus PlayStatus
        {
            get => _playStatus;
            set
            {
                _playStatus = value;
                OnPropertyChanged();
            }
        }


        public TimeSpan Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged();
            }
        }
        
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }

        public int Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged();
            }
        }

        public bool Buffering
        {
            get => _buffering;
            set
            {
                _buffering = value;
                OnPropertyChanged();
            }
        }

        public int PlaybackRate
        {
            get => _playbackRate;
            set
            {
                _playbackRate = value;
                OnPropertyChanged();
            }
        }

        private PlayingStatus _playStatus;
        private TimeSpan _position;
        private int _volume;
        private bool _buffering;
        private int _playbackRate;
        private TimeSpan _duration;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}