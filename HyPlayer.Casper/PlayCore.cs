using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HyPlayer.Casper.Model;
using HyPlayer.Casper.Service;
using HyPlayer.Casper.Service.PlayServices;
using Microsoft.UI.Xaml;

namespace HyPlayer.Casper;

public sealed class PlayCore : DependencyObject
{
    #region Basic Information

    /// <summary>
    ///     音乐提供者列表
    /// </summary>
    public readonly Dictionary<string, IMusicProvider> MusicProviders = new();

    /// <summary>
    ///     当前播放列表
    /// </summary>
    public readonly List<SingleSong> PlayList = new();

    /// <summary>
    ///     播放列表来源
    /// </summary>
    public SongContainer PlayListSource
    {
        get => (SongContainer)GetValue(PlayListSourceProperty);
        set => SetValue(PlayListSourceProperty, value);
    }

    /// <summary>
    ///     当前播放指针
    /// </summary>
    public int NowPlayIndex
    {
        get => (int)GetValue(NowPlayIndexProperty);
        set => SetValue(NowPlayIndexProperty, value);
    }

    /// <summary>
    ///     播放模式
    /// </summary>
    public PlayRollMode PlayRollingMode
    {
        get => (PlayRollMode)GetValue(PlayRollingModeProperty);
        set => SetValue(PlayRollingModeProperty, value);
    }

    /// <summary>
    ///     播放列表变化指示器
    ///     这是一个十分巧妙的指示器. 在播放列表发生变化时, 此属性将会更改.
    ///     可以通过 Converter 将此属性的 Bind 转换为列表
    /// </summary>
    public bool PlayListChangedIndicator
    {
        get => (bool)GetValue(PlayListChangedIndicatorProperty);
        set => SetValue(PlayListChangedIndicatorProperty, value);
    }

    /// <summary>
    ///     播放设置
    /// </summary>
    public readonly PlayCoreSettings PlayCoreSettings = new();

    /// <summary>
    ///     随机数生成器
    /// </summary>
    public readonly Random RandomGenerator = new();

    /// <summary>
    ///     当前播放服务
    /// </summary>
    public readonly PlayService PlayService;

    /// <summary>
    ///     SMTC 服务
    /// </summary>
    public readonly SmtcService SmtcService;

    /// <summary>
    ///     当前播放歌曲
    /// </summary>
    public SingleSong NowPlayingSong
    {
        get => (SingleSong)GetValue(NowPlayingSongProperty);
        set => SetValue(NowPlayingSongProperty, value);
    }

    public static readonly Dictionary<string, PlayService> PlayServices = new()
    {
        { "AudioGraph", new AudioGraphService() }
    };

    public readonly PlayServiceEvents Events = new();

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty PlayListSourceProperty = DependencyProperty.Register(
        "PlayListSource", typeof(SongContainer), typeof(PlayCore), new PropertyMetadata(default(SongContainer)));

    public static readonly DependencyProperty NowPlayIndexProperty = DependencyProperty.Register(
        "NowPlayIndex", typeof(int), typeof(PlayCore), new PropertyMetadata(-1));

    public static readonly DependencyProperty PlayRollingModeProperty = DependencyProperty.Register(
        "PlayRollingMode", typeof(PlayRollMode), typeof(PlayCore), new PropertyMetadata(default(PlayRollMode)));

    public static readonly DependencyProperty NowPlayingSongProperty = DependencyProperty.Register(
        "NowPlayingSong", typeof(SingleSong), typeof(PlayCore), new PropertyMetadata(default(SingleSong)));

    public static readonly DependencyProperty PlayListChangedIndicatorProperty = DependencyProperty.Register(
        "PlayListChangedIndicator", typeof(bool), typeof(PlayCore), new PropertyMetadata(default(bool)));

    #endregion

    #region Basic Public Function

    public PlayCore()
    {
        // Select PlayService
        // TODO: Allow User To Select Which Service To Use
        PlayService = PlayServices[PlayServices.Keys.First()];
        PlayService.InitializeService();
        PlayService.Events = Events;
        if (PlayCoreSettings.SyncSmtc)
        {
            SmtcService = new SmtcService();
            SmtcService.InitializeService();
            Events.OnPlayItemChanged += SmtcService.OnPlayItemChanged;
            Events.OnPlay += SmtcService.OnPlay;
            Events.OnPause += SmtcService.OnPause;
            Events.OnStop += SmtcService.OnStop;
            SmtcService.OnPlayAnother += next =>
            {
                if (next) MoveNext();
                else MovePrevious();
            };
            SmtcService.OnPlayPositionChanging += position => PlayService.Seek(position);
            SmtcService.OnPlayStateChanging += status =>
            {
                switch (status)
                {
                    case PlayingStatus.Paused:
                        PlayService.Pause();
                        break;
                    case PlayingStatus.Playing:
                        PlayService.Play();
                        break;
                    case PlayingStatus.None:
                        PlayService.Stop();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(status), status, null);
                }
            };
            if (PlayCoreSettings.SyncSmtcTime)
                Events.OnPositionChanged += () => SmtcService.OnPlayPositionChanged(PlayService.Status.Position);
        }
    }

    /// <summary>
    ///     添加一个音乐单曲到播放列表
    ///     要添加多个单曲请使用 AppendPlayItemRange
    /// </summary>
    /// <param name="item">音乐单曲</param>
    public void AppendPlayItem(SingleSong item)
    {
        PlayList.Add(item);
        Events.RaisePlayListChangedEvent();
        PlayListChangedIndicator = !PlayListChangedIndicator;
    }

    /// <summary>
    ///     添加一个音乐单曲到播放列表指定位置
    ///     要添加多个单曲请使用 InsertPlayItemRange
    /// </summary>
    /// <param name="item">音乐单曲</param>
    /// <param name="index">位置</param>
    public void InsertPlayItem(SingleSong item, int index)
    {
        PlayList.Insert(index, item);
        Events.RaisePlayListChangedEvent();
        PlayListChangedIndicator = !PlayListChangedIndicator;
    }

    /// <summary>
    ///     添加音乐单曲下一首播放
    /// </summary>
    /// <param name="item">音乐单曲</param>
    public void InsertPlayItemToNext(SingleSong item)
    {
        InsertPlayItem(item, Math.Max(NowPlayIndex, 0));
        // 0 是防止越界
    }

    /// <summary>
    ///     添加多个音乐单曲到播放列表指定位置
    /// </summary>
    /// <param name="items">音乐单曲</param>
    /// <param name="index">位置</param>
    public void InsertPlayItemRange(IEnumerable<SingleSong> items, int index)
    {
        PlayList.InsertRange(index, items);
        Events.RaisePlayListChangedEvent();
        PlayListChangedIndicator = !PlayListChangedIndicator;
    }

    /// <summary>
    ///     添加多个音乐单曲到播放列表
    /// </summary>
    /// <param name="items">音乐单曲</param>
    public void AppendPlayItemRange(IEnumerable<SingleSong> items)
    {
        PlayList.AddRange(items);
        Events.RaisePlayListChangedEvent();
        PlayListChangedIndicator = !PlayListChangedIndicator;
    }

    /// <summary>
    ///     删除指定位置的音乐单曲
    /// </summary>
    /// <param name="index">位置</param>
    public void RemoveSong(int index)
    {
        if (PlayList.Count <= index || index < -1) return; // 越界
        if (PlayList.Count - 1 == 0)
        {
            RemoveAllSong();
            PlayService.Stop();
            return;
        }

        if (index == NowPlayIndex)
        {
            PlayList.RemoveAt(index);
            LoadNowPlayingItemMedia();
        }
        else if (index < NowPlayIndex)
        {
            //需要将序号向前挪动
            MoveTo(NowPlayIndex - 1);
            PlayList.RemoveAt(index);
        }
        else if (index > NowPlayIndex) //假如移除后面的我就不管了
        {
            PlayList.RemoveAt(index);
        }
    }


    /// <summary>
    ///     删除播放列表中的所有音乐单曲
    ///     请不要在 PlayCore 内部调用 RemoveAllSong
    /// </summary>
    public void RemoveAllSong()
    {
        // 此方法调用后会触发 PlayListCleared 事件
        // 可能会导致播放条消失等问题
        PlayList.Clear();
        Events.RaisePlayListClearedEvent();
    }

    /// <summary>
    ///     获取下一首歌曲的索引 ID
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">播放模式未知</exception>
    public int GetNextSongPointer()
    {
        // 请注意 Shuffle 的情况下 Pointer 可能会变动
        var retPointer = NowPlayIndex;
        if (PlayList.Count == 0) return -1; // 没有歌曲的话直接切换为不播放
        switch (PlayRollingMode)
        {
            case PlayRollMode.DefaultRoll:
                //正常Roll的话,id++
                if (NowPlayIndex + 1 >= PlayList.Count)
                    retPointer = 0;
                else
                    retPointer++;
                break;
            case PlayRollMode.SinglePlay:
                retPointer = NowPlayIndex;
                break;
            case PlayRollMode.Shuffled:
                retPointer = RandomGenerator.Next(PlayList.Count - 1);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return retPointer;
    }

    /// <summary>
    ///     移动当前播放指针到指定位置
    /// </summary>
    /// <param name="index">位置</param>
    public void MoveTo(int index)
    {
        if (index < 0 || PlayList.Count <= index) return;
        NowPlayIndex = index;
        NowPlayingSong = PlayList[NowPlayIndex];
    }

    /// <summary>
    ///     移动指针到下一首歌曲
    /// </summary>
    public async void MoveNext()
    {
        if (PlayListSource.PlayListSourceType == PlayListSourceType.Interactive)
        {
            // 交互式列表将会把所有歌曲移除并且添加一首新的歌曲
            PlayList.Clear();
            AppendPlayItem(await MusicProviders[PlayListSource.ProviderId]
                .GetPlayListNextItem(PlayListSource.InProviderId));
            // 此时需要 Events.OnPlayListChanged
            Events.RaisePlayListChangedEvent();
            PlayListChangedIndicator = !PlayListChangedIndicator;
        }

        MoveTo(GetNextSongPointer());
    }

    /// <summary>
    ///     移动指针到上一首歌曲
    /// </summary>
    public void MovePrevious()
    {
        if (PlayList.Count == 0) return;
        if (NowPlayIndex - 1 < 0)
            MoveTo(PlayList.Count - 1);
        else
            MoveTo(NowPlayIndex - 1);
    }

    /// <summary>
    ///     注册音乐提供者
    /// </summary>
    /// <param name="musicProvider">音乐提供者</param>
    public void RegisterMusicProvider(IMusicProvider musicProvider)
    {
        MusicProviders[musicProvider.ProviderId] = musicProvider;
    }

    /// <summary>
    ///     注册音乐提供者
    /// </summary>
    /// <param name="musicProviders">音乐提供者</param>
    public void RegisterMusicProviders(IEnumerable<IMusicProvider> musicProviders)
    {
        musicProviders.ToList().ForEach(RegisterMusicProvider);
    }

    /// <summary>
    ///     替换当前播放来源
    /// </summary>
    /// <param name="playListSource"></param>
    public void ReplacePlaySource(SongContainer playListSource)
    {
        PlayListSource = playListSource;
        Events.RaisePlaySourceChangedEvent(playListSource);
    }

    /// <summary>
    ///     加载当前播放歌曲
    /// </summary>
    public async Task LoadNowPlayingItemMedia()
    {
        await PlayService.Load(await MusicProviders[PlayList[NowPlayIndex].ProviderId]
            .GetPlayItemMediaSource(PlayList[NowPlayIndex].InProviderId));
        Events.RaiseMediaLoadedEvent();
    }

    /// <summary>
    ///     加载当前播放来源歌曲
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">播放来源类型未知</exception>
    public async void LoadPlaySource()
    {
        PlayList.Clear();
        switch (PlayListSource.PlayListSourceType)
        {
            case PlayListSourceType.Liner:
                AppendPlayItemRange(await MusicProviders[PlayListSource.ProviderId]
                    .GetPlayListItems(PlayListSource.InProviderId));
                break;
            case PlayListSourceType.Interactive:
                AppendPlayItem(await MusicProviders[PlayListSource.ProviderId]
                    .GetPlayListNextItem(PlayListSource.InProviderId));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Events.RaisePlayListChangedEvent();
        PlayListChangedIndicator = !PlayListChangedIndicator;
    }

    #endregion
}

public class PlayCoreSettings
{
    public bool SyncSmtc; // 同步 SMTC
    public bool SyncSmtcTime; // 同步 SMTC 的时间属性
}

public enum PlayRollMode
{
    DefaultRoll,
    SinglePlay,
    Shuffled
}