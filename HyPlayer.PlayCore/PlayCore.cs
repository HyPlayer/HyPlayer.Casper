using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyPlayer.PlayCore.Model;

namespace HyPlayer.PlayCore
{
    public class PlayCore
    {
        #region Basic Information

        public readonly Dictionary<string, IMusicProvider> MusicProviders = new();
        public readonly List<PlayItem> PlayList = new();
        public readonly IPlaySource PlaySource = null;
        public readonly int NowPlayIndex = -1;
        public readonly PlayStatus PlayStatus = PlayStatus.None;
        public readonly PlayCoreSettings PlayCoreSettings = new();
        #endregion
        #region Basic Function

        public void AppendPlayItem(PlayItem item)
        {
            throw new NotImplementedException();
        }

        public void InsertPlayItem(PlayItem item, int index)
        {
            throw new NotImplementedException();
        }

        public void InsertPlayItemRange(IEnumerable<PlayItem> items)
        {
            throw new NotImplementedException();
        }
        
        public void AppendPlayItemRange(IEnumerable<PlayItem> items)
        {
            throw new NotImplementedException();
        }

        public int GetNextSongPointer()
        {
            // 请注意 Shuffle 的情况下 Pointer 可能会变动
            throw new NotImplementedException();
        }

        public void SongMoveTo(int id)
        {
            throw new NotImplementedException();
        }

        public void SongMoveNext()
        {
            throw new NotImplementedException();
        }

        public void SongMovePrevious()
        {
            throw new NotImplementedException();
        }

        public void LoadNowPlayingItemMedia()
        {
            throw new NotImplementedException();
        }

        public void SongAppendDone()
        {
            throw new NotImplementedException();
        }

        public void RemoveSong()
        {
            throw new NotImplementedException();
        }

        public void RemoveAllSong()
        {
            throw new NotImplementedException();
        }

        public void ReplacePlaySource(IPlaySource playSource)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class PlayCoreSettings
    {
    }

    public enum PlayStatus
    {
        Paused,
        Playing,
        Failed,
        None
    }

    public enum RollMode
    {
        DefaultRoll,
        SinglePlay,
        Shuffled
    }
}