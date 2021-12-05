using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace HyPlayer.Casper.Model;

public interface IMusicProvider
{
    public string ProviderId { get; }
    public string ProviderName { get; }
    public MusicProviderSettings ProviderSettings { get; }
    public Task<ProvidableItem> GetPlayItem(string inProviderId);
    public Task<MediaSource> GetPlayItemMediaSource(string inProviderId);
    public Task<List<SingleSong>> GetPlayListItems(string inProviderId);

    public Task<SingleSong> GetPlayListNextItem(string inProviderId);
    public Task<string> GetPlayItemLyric(string inProviderId);
}

public interface IOnlineMusicProvider : IMusicProvider
{
    public Task<string> GetPlayItemTranslatedLyric(string inProviderId);
}

public class MusicProviderSettings
{
    public MusicProviderSupports Supports;
}

public class MusicProviderSupports
{
    public Dictionary<string, string> ListMusicSourceTypes; // TypeId, Name
}