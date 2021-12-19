using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Core;
using Windows.Media.Devices;
using Windows.Media.Render;

namespace HyPlayer.Casper.Service.PlayServices;

public sealed class AudioGraphService : EffectivePlayService
{
    private static readonly Dictionary<AudioGraphCreationStatus, string> AudioGraphCreationStatusStrings =
        new()
        {
            { AudioGraphCreationStatus.Success, "成功" },
            { AudioGraphCreationStatus.UnknownFailure, "未知错误" },
            { AudioGraphCreationStatus.DeviceNotAvailable, "设备不存在" },
            { AudioGraphCreationStatus.FormatNotSupported, "格式不支持" }
        };

    private static readonly Dictionary<AudioDeviceNodeCreationStatus, string> AudioDeviceNodeCreationStatusStrings =
        new()
        {
            { AudioDeviceNodeCreationStatus.Success, "成功" },
            { AudioDeviceNodeCreationStatus.AccessDenied, "拒绝访问" },
            { AudioDeviceNodeCreationStatus.UnknownFailure, "未知错误" },
            { AudioDeviceNodeCreationStatus.DeviceNotAvailable, "设备不存在" },
            { AudioDeviceNodeCreationStatus.FormatNotSupported, "格式不支持" }
        };

    private static readonly Dictionary<MediaSourceAudioInputNodeCreationStatus, string>
        MediaSourceAudioInputNodeCreationStatusStrings =
            new()
            {
                { MediaSourceAudioInputNodeCreationStatus.Success, "成功" },
                { MediaSourceAudioInputNodeCreationStatus.NetworkError, "网络错误" },
                { MediaSourceAudioInputNodeCreationStatus.UnknownFailure, "未知错误" },
                { MediaSourceAudioInputNodeCreationStatus.FormatNotSupported, "格式不支持" }
            };

    private readonly Timer _timer = new(500);
    private AudioGraph _graph;
    private MediaSourceAudioInputNode _inputNode;
    private AudioDeviceOutputNode _outputNode;
    private List<MediaSourceAudioInputNode> _nodes = new List<MediaSourceAudioInputNode>();
    private readonly double previousPositionMilliseconds = double.Epsilon;

    private int _sortNodeCountDown = 4;


    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        // 检测进度变更
        if (Math.Abs(_inputNode.Position.TotalMilliseconds - previousPositionMilliseconds) > 0.01)
        {
            Status.InternalPosition = _inputNode.Position;
            Events.RaisePositionChangedEvent();
        }
        // 播放整流
        if (_sortNodeCountDown-- <= 0)
        {
            _sortNodeCountDown = 4;
            SortNodes();
        }
    }


    private async Task CreateAudioGraph(DeviceInformation device = null)
    {
        // Create Audio Graph By Using The Default
        if (_graph != null)
        {
            _graph.Dispose();
            _graph = null;
        }

        var settings = new AudioGraphSettings(AudioRenderCategory.Media);
        if (device != null)
        {
            settings.PrimaryRenderDevice = device;
        }

        var result = await AudioGraph.CreateAsync(settings);
        if (result.Status != AudioGraphCreationStatus.Success)
        {
            Available = false;
            LastError = AudioGraphCreationStatusStrings[result.Status];
            return;
        }

        _graph = result.Graph;
        var outputDeviceNodeResult = await _graph.CreateDeviceOutputNodeAsync();
        if (outputDeviceNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
        {
            Available = false;
            LastError = AudioDeviceNodeCreationStatusStrings[outputDeviceNodeResult.Status];
            return;
        }

        _outputNode = outputDeviceNodeResult.DeviceOutputNode;
        await InitializeEffect();
        _graph.Start();
    }

    public override async Task<bool> InitializeService()
    {
        Abilities = new PlayServiceAbility
        {
            Play = true,
            Pause = true,
            Stop = true,
            Load = true,
            Seek = true,
            ChangeVolume = true,
            ChangePlaybackRate = true,
            PlayBackground = true,
            ChangeOutputDevice = true
        };
        Status = new PlayServiceStatus
        {
            PlayStatus = PlayingStatus.None,
            InternalPosition = TimeSpan.Zero,
            Position = TimeSpan.Zero,
            Volume = 50,
            Buffering = false,
            PlaybackRate = 10
        };
        Status.OnVolumeChanged += (volume) => { _outputNode.OutgoingGain = (double)volume / 50; };
        Status.OnPlayBackRateChanged += (rate) =>
        {
            if (_inputNode != null) _inputNode.PlaybackSpeedFactor = (double)rate / 10;
        };
        Status.OnPositionChanged += position => { if (_inputNode?.MediaSource != null) _inputNode?.Seek(position); };
        _timer.Elapsed += TimerOnElapsed;
        await CreateAudioGraph();
        return true;
    }

    public override async Task<bool> Load(MediaSource mediaSource)
    {
        if (mediaSource == null)
        {
            // 传入 Null 值默认为释放当前播放并返回
            _inputNode.Dispose();
            return false;
        }

        if (_inputNode?.MediaSource != null)
        {
            // 如果 InputNode 已经有过, 需要将其释放
            // 如果上一个文件正在播放需要将其停止
            _inputNode.Dispose();
            _nodes.Remove(_inputNode);
        }


        Status.Buffering = true;
        Status.PlayStatus = PlayingStatus.Loading;
        var res = await _graph.CreateMediaSourceAudioInputNodeAsync(mediaSource);
        if (res.Status != MediaSourceAudioInputNodeCreationStatus.Success)
        {
            LastError = MediaSourceAudioInputNodeCreationStatusStrings[res.Status];
            Status.PlayStatus = PlayingStatus.Failed;
            Events.RaiseFailedEvent();
            return false;
        }

        _inputNode = res.Node;
        _inputNode.Stop();
        _nodes.Add(res.Node);
        Status.Duration = _inputNode.Duration;
        _inputNode.AddOutgoingConnection(_outputNode);
        _inputNode.MediaSourceCompleted += (_, _) => Events.RaiseMediaEndEvent();
        Status.Buffering = false;
        Status.PlayStatus = PlayingStatus.Loaded;
        Events.RaiseMediaLoadedEvent();
        return true;
    }

    public override Task Play()
    {
        if (_inputNode?.MediaSource == null)
        {
            Status.PlayStatus = PlayingStatus.None;
            return Task.CompletedTask;
        }
        _inputNode.Start();
        Status.PlayStatus = PlayingStatus.Playing;
        _timer.Start();
        Events.RaisePlayEvent();
        return Task.CompletedTask;
    }

    public override Task Pause()
    {
        if (_inputNode?.MediaSource == null)
        {
            Status.PlayStatus = PlayingStatus.None;
            return Task.CompletedTask;
        }
        _inputNode.Stop();
        Status.PlayStatus = PlayingStatus.Paused;
        _timer.Stop();
        Events.RaisePauseEvent();
        return Task.CompletedTask;
    }

    public override Task Stop()
    {
        SortNodes();
        if (_inputNode?.MediaSource != null)
            _inputNode.Dispose();
        _nodes.Where(t => t.MediaSource != null).ToList().ForEach(node => { _nodes.Remove(node); node.Dispose(); });
        Status.PlayStatus = PlayingStatus.None;
        _timer.Stop();
        Events.RaiseStopEvent();
        return Task.CompletedTask;
    }

    private async void SortNodes()
    {
        if (_nodes.Count > 1)
            _nodes.Where(t => t != _inputNode).ToList().ForEach(node => { if (node.MediaSource != null) node.Dispose(); _nodes.Remove(node); });
    }

    public override async Task<bool> ChangeOutputDevice(PlayServiceOutgoingDeviceInfo device)
    {
        Status.PlayStatus = PlayingStatus.Loading;
        var source = _inputNode.MediaSource;
        _inputNode?.Dispose();
        _outputNode?.Dispose();
        await CreateAudioGraph(device.NativeDevice);
        await Load(source);
        return true;
    }

    public override async Task<bool> RefreshOutputDevice()
    {
        OutgoingDevices.Clear();
        var list = (await DeviceInformation.FindAllAsync(
            MediaDevice.GetAudioRenderSelector())).Select(t =>
            new PlayServiceOutgoingDeviceInfo
            {
                DeviceId = t.Id,
                Name = t.Name,
                NativeDevice = t
            });
        foreach (var playServiceOutgoingDeviceInfo in list)
        {
            OutgoingDevices.Add(playServiceOutgoingDeviceInfo);
        }

        return true;
    }


    public override Task<bool> SwitchBackground()
    {
        // No thing for it
        return Task.FromResult(true);
    }

    public override Task<bool> InitializeEffect()
    {
        // Code From AudioCreation by Microsoft
        // https://github.com/microsoft/Windows-universal-samples/tree/main/Samples/AudioCreation
        // LICENCED by MIT

        // create echo effect
        EchoEffectDefinition = new EchoEffectDefinition(_graph)
        {
            // http://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.xapofx.fxecho_parameters(v=vs.85).aspx
            // See the MSDN page for parameter explanations
            WetDryMix = 0.7f,
            Feedback = 0.5f,
            Delay = 500.0f
        };
        _outputNode.EffectDefinitions.Add(EchoEffectDefinition);
        _outputNode.DisableEffectsByDefinition(EchoEffectDefinition);


        // Create reverb effect
        ReverbEffectDefinition = new ReverbEffectDefinition(_graph)
        {
            // https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.xaudio2.xaudio2fx_reverb_parameters(v=vs.85).aspx
            // See the MSDN page for parameter explanations
            WetDryMix = 50,
            ReflectionsDelay = 120,
            ReverbDelay = 30,
            RearDelay = 3,
            DecayTime = 2
        };

        _outputNode.EffectDefinitions.Add(ReverbEffectDefinition);
        _outputNode.DisableEffectsByDefinition(ReverbEffectDefinition);


        // Create limiter effect
        LimiterEffectDefinition = new LimiterEffectDefinition(_graph)
        {
            Loudness = 500,
            Release = 10
        };

        _outputNode.EffectDefinitions.Add(LimiterEffectDefinition);
        _outputNode.DisableEffectsByDefinition(LimiterEffectDefinition);


        // See the MSDN page for parameter explanations
        // https://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.xapofx.fxeq_parameters(v=vs.85).aspx
        EqEffectDefinition = new EqualizerEffectDefinition(_graph);
        EqEffectDefinition.Bands[0].FrequencyCenter = 100.0f;
        EqEffectDefinition.Bands[0].Gain = 4f;
        EqEffectDefinition.Bands[0].Bandwidth = 1.5f;

        EqEffectDefinition.Bands[1].FrequencyCenter = 900.0f;
        EqEffectDefinition.Bands[1].Gain = 4f;
        EqEffectDefinition.Bands[1].Bandwidth = 1.5f;

        EqEffectDefinition.Bands[2].FrequencyCenter = 5000.0f;
        EqEffectDefinition.Bands[2].Gain = 4f;
        EqEffectDefinition.Bands[2].Bandwidth = 1.5f;

        EqEffectDefinition.Bands[3].FrequencyCenter = 12000.0f;
        EqEffectDefinition.Bands[3].Gain = 4f;
        EqEffectDefinition.Bands[3].Bandwidth = 2.0f;
        _outputNode.EffectDefinitions.Add(EqEffectDefinition);
        _outputNode.DisableEffectsByDefinition(EqEffectDefinition);
        return Task.FromResult(true);
    }
}