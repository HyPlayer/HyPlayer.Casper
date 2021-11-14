using System.Collections.Generic;
using Windows.Media.Core;

namespace HyPlayer.PlayCore.Model
{
    public interface IMusicProvider
    {
        public string Id { get; }
        public string Name { get; }
        public PlayItemInfo GetPlayItemInfo(string id);
        public MediaSource GetPlayItemMediaSource(string id);
        public List<PlayItem> GetPlaySourceItems(string id);
        public List<IPlaySource> GetPlayLists(string id);
        public string GetPlayItemLyric(string id);
    }

    public interface IOnlineMusicProvider : IMusicProvider
    {
        public string GetPlayItemTranslatedLyric(string id);
    }
    
    public interface IPlaySource
    {
        public string Id { get; }
        public string ProviderId { get; }
        public string Name { get; }
        public string PlaySourceType { get; }
        public string ActualPlaySourceId { get; }
    }
}