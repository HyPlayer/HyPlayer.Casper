using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace HyPlayer.PlayCore.Model
{
    public class PlayableItem
    {
        public string Id => ProviderId + PlaySourceType + ActualId;
        public string InProviderId => PlaySourceType + ActualId;
        public string Name;
        public string ProviderId;
        public string PlaySourceType;
        public string ActualId;

        public PlayableItem(string id)
        {
            ProviderId = Id.Substring(0, 3);
            PlaySourceType = Id.Substring(3, 2);
            ActualId = Id.Substring(5);
        }

        public PlayableItem(string providerId, string playSourceType, string actualId)
        {
            ProviderId = providerId;
            PlaySourceType = playSourceType;
            ActualId = actualId;
        }

        protected PlayableItem()
        {
            
        }
    }

    public class SongContainer : PlayableItem
    {
        public string Creator;
        public string CreatorId; // 此处 ID 需要加 ProviderId
        public string Description;
    }

    public class SingleSong : PlayableItem
    {
        public string TranslatedName;
        public string Description;
        public Album Album;
        public List<Artist> Artists;
        public TimeSpan Duration;
        public string ArtistsString => string.Join(" / ", Artists.Select(t => t.Name));
    }

    public abstract class Album : SongContainer
    {
        public abstract BitmapImage GetCoverImage();
        public abstract IRandomAccessStream GetCoverImageStream();
    }

    public class Artist : PlayableItem
    {
        // Empty
    }

    public class SongLyric
    {
        public static SongLyric PureSong = new SongLyric
            { HaveTranslation = false, LyricTime = TimeSpan.Zero, PureLyric = "纯音乐 请欣赏" };

        public static SongLyric NoLyric = new SongLyric
            { HaveTranslation = false, LyricTime = TimeSpan.Zero, PureLyric = "无歌词 请欣赏" };

        public static SongLyric LoadingLyric = new SongLyric
            { HaveTranslation = false, LyricTime = TimeSpan.Zero, PureLyric = "加载歌词中..." };

        public bool HaveTranslation;
        public TimeSpan LyricTime;
        public string PureLyric;
        public string Translation;
    }
}