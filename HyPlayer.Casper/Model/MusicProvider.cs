using System.Collections.Generic;
using Windows.Media.Core;

namespace HyPlayer.Casper.Model
{
    public interface IMusicProvider
    {
        public string Id { get; }
        public string Name { get; }
        public MusicProviderSettings Settings { get; }
        public PlayableItem GetPlayItem(string id);
        public MediaSource GetPlayItemMediaSource(string id);
        public List<PlayableItem> GetPlayItems(string id);
        public string GetPlayItemLyric(string id);
    }

    public interface IOnlineMusicProvider : IMusicProvider
    {
        public string GetPlayItemTranslatedLyric(string id);
    }
    
    public class MusicProviderSettings
    {
        public MusicProviderSupports Supports;
    }
    
    public class MusicProviderSupports
    {
        public Dictionary<string, string> ListMusicSourceTypes; // TypeId, Name
    }
}