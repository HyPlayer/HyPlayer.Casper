using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Media;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using HyPlayer.PlayCore.Model;
using HyPlayer.PlayCore.Service;
using HyPlayer.PlayCore.Service.PlayServices;

namespace HyPlayer.PlayCore
{
    public sealed class PlayCore
    {
        #region Basic Information

        public readonly Dictionary<string, IMusicProvider> MusicProviders = new();
        public readonly List<SingleSong> PlayList = new();
        public SongContainer PlaySource = null;
        public int NowPlayIndex = -1;
        public PlayRollMode PlayRollMode = PlayRollMode.DefaultRoll;
        public readonly PlayCoreSettings PlayCoreSettings = new();
        public readonly Random RandomGenerator = new();
        public readonly PlayService PlayService = null;
        public readonly SmtcService SmtcService = null;

        public static readonly Dictionary<string, PlayService> PlayServices = new()
        {
            { "AudioGraph", new AudioGraphService() }
        };

        public readonly PlayServiceEvents Events = new PlayServiceEvents();

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
                SmtcService.OnPlayAnother += (next) =>
                {
                    if (next) SongMoveNext();
                    else SongMovePrevious();
                };
                SmtcService.OnPlayPositionChanging += position => PlayService.Seek(position);
                SmtcService.OnPlayStateChanging += (status) =>
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

        public void AppendPlayItem(SingleSong item)
        {
            PlayList.Add(item);
        }

        public void InsertPlayItem(SingleSong item, int index)
        {
            PlayList.Insert(index, item);
        }

        public void InsertPlayItemToNext(SingleSong item)
        {
            PlayList.Insert(NowPlayIndex, item);
        }

        public void InsertPlayItemRange(IEnumerable<SingleSong> items, int index)
        {
            PlayList.InsertRange(index, items);
        }

        public void AppendPlayItemRange(IEnumerable<SingleSong> items)
        {
            PlayList.AddRange(items);
        }

        public void RemoveSong(int index)
        {
            if (PlayList.Count <= index) return;
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
                SongMoveTo(NowPlayIndex - 1);
                PlayList.RemoveAt(index);
            }
            else if (index > NowPlayIndex) //假如移除后面的我就不管了
                PlayList.RemoveAt(index);
        }

        public void RemoveAllSong()
        {
            PlayList.Clear();
        }

        public int GetNextSongPointer()
        {
            // 请注意 Shuffle 的情况下 Pointer 可能会变动
            int retPointer = -1;
            if (PlayList.Count == 0) return retPointer; // 没有歌曲的话直接切换为不播放
            switch (PlayRollMode)
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

        public void SongMoveTo(int index)
        {
            if (index < 0 && PlayList.Count <= index) return;
            NowPlayIndex = index;
        }

        public void SongMoveNext()
        {
            SongMoveTo(GetNextSongPointer());
        }

        public void SongMovePrevious()
        {
            if (PlayList.Count == 0) return;
            if (NowPlayIndex - 1 < 0)
                SongMoveTo(PlayList.Count - 1);
            else
                SongMoveTo(NowPlayIndex - 1);
        }

        public void ReplacePlaySource(SongContainer playSource)
        {
            PlaySource = playSource;
        }

        public void LoadNowPlayingItemMedia()
        {
            PlayService.Load(MusicProviders[PlayList[NowPlayIndex].ProviderId]
                .GetPlayItemMediaSource(PlayList[NowPlayIndex].InProviderId));
        }

        public void LoadPlaySource()
        {
            RemoveAllSong();
            AppendPlayItemRange(MusicProviders[PlaySource.ProviderId].GetPlayItems(PlaySource.InProviderId).Cast<SingleSong>());
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
}