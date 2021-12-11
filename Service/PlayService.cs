using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Core;
using HyPlayer.Casper.Annotations;

namespace HyPlayer.Casper.Service;

public abstract class PlayService
{
    public PlayServiceAbility Abilities;
    public bool Available;
    public PlayServiceEvents Events;
    public string Id;
    public string LastError; // Win32 时代的产物
    public string Name;
    public PlayServiceStatus Status;
    public ObservableCollection<PlayServiceOutgoingDeviceInfo> OutgoingDevices;

    public abstract Task<bool> InitializeService();
    public abstract Task<bool> Load(MediaSource mediaSource);
    public abstract Task Play();
    public abstract Task Pause();
    public abstract Task Stop();
    public abstract Task Seek(TimeSpan timeSpan);
    public abstract Task<bool> ChangeOutputDevice(PlayServiceOutgoingDeviceInfo device);
    public abstract Task<bool> RefreshOutputDevice();
    public abstract Task<bool> SwitchBackground();
}

public abstract class EffectivePlayService : PlayService // 这个命名好好想想
{
    public EchoEffectDefinition EchoEffectDefinition;
    public EqualizerEffectDefinition EqEffectDefinition;
    public LimiterEffectDefinition LimiterEffectDefinition;
    public ReverbEffectDefinition ReverbEffectDefinition;
    public abstract Task<bool> InitializeEffect();
}

public class PlayServiceAbility
{
    public bool ChangeOutputDevice; // 更改输出设备
    public bool ChangePlaybackRate; // 更改倍速
    public bool ChangeVolume; // 更改音量
    public bool Load; // 加载歌曲
    public bool Pause; // 暂停歌曲
    public bool Play; // 播放歌曲
    public bool PlayBackground; // 后台播放
    public bool Seek; // 切换时长
    public bool Stop; // 停止歌曲 (卸载歌曲)
}

public class PlayServiceOutgoingDeviceInfo
{
    public string DeviceId;
    public string Name;
    public DeviceInformation NativeDevice;
}

public enum PlayingStatus
{
    None,
    Loading,
    Loaded,
    Paused,
    Playing,
    Failed
}

public class PlayServiceStatus : INotifyPropertyChanged
{
    private bool _buffering;
    private TimeSpan _duration;
    private int _playbackRate;

    private PlayingStatus _playStatus;
    private TimeSpan _position;
    private int _volume;

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
            OnVolumeChanged?.Invoke(value);
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
            OnPlayBackRateChanged?.Invoke(value);
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public delegate void VolumeChangedEventHandler(int volume);

    public event VolumeChangedEventHandler OnVolumeChanged;

    public delegate void PlaybackRateChangedEventHandler(int playbackRate);

    public event PlaybackRateChangedEventHandler OnPlayBackRateChanged;
}