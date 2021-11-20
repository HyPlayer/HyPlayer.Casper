using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyPlayer.PlayCore.Model;
using HyPlayer.PlayCore.Service;

namespace HyPlayer.PlayCore
{
    public class PlayCore
    {
        #region Basic Information

        public readonly Dictionary<string, IMusicProvider> MusicProviders = new();
        public readonly List<PlayableItem> PlayList = new();
        public SongContainer PlaySource = null;
        public int NowPlayIndex = -1;
        public PlayRollMode PlayRollMode = PlayRollMode.DefaultRoll;
        public readonly PlayCoreSettings PlayCoreSettings = new();
        public readonly Random RandomGenerator = new();
        public readonly PlayService PlayService = null;

        #endregion
        #region Basic Public Function

        public void AppendPlayItem(PlayableItem item)
        {
            PlayList.Add(item);
        }

        public void InsertPlayItem(PlayableItem item, int index)
        {
            PlayList.Insert(index, item);
        }

        public void InsertPlayItemToNext(PlayableItem item)
        {
            PlayList.Insert(NowPlayIndex, item);
        }

        public void InsertPlayItemRange(IEnumerable<PlayableItem> items, int index)
        {
            PlayList.InsertRange(index, items);
        }

        public void AppendPlayItemRange(IEnumerable<PlayableItem> items)
        {
            PlayList.AddRange(items);
        }

        public void RemoveSong(int index)
        {
            if (PlayList.Count <= index) return;
            if (PlayList.Count - 1 == 0)
            {
                RemoveAllSong();
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
            PlayService.Load(MusicProviders[PlayList[NowPlayIndex].ProviderId].GetPlayItemMediaSource(PlayList[NowPlayIndex].Id));
        }

        public void SongAppendDone()
        {
            throw new NotImplementedException();
        }

        public void LoadPlaySource()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PlayCoreSettings
    {
        public bool SyncSMTC; // 同步 SMTC
        public bool AutoSyncSMTC; // 自动同步 SMTC
    }



    public enum PlayRollMode
    {
        DefaultRoll,
        SinglePlay,
        Shuffled
    }
}