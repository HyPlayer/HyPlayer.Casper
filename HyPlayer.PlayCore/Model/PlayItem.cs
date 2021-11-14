using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace HyPlayer.PlayCore.Model
{
    public class PlayItem
    {
        public string Id => ProviderId + MusicId;
        public string ProviderId;
        public string MusicId;
        public PlayItemInfo PlayItemInfo;
    }
    public class PlayItemInfo
    {
        public string Name;
        public string TranslatedName;
        public string Description;
        public Album Album;
        public List<Artist> Artists;
    }

    public class Album
    {
        public string Id => ProviderId + AlbumId;
        public string ProviderId;
        public string AlbumId;
        public string Name;
        public ImageSource CoverImage;
    }

    public class Artist
    {
        public string Id => ProviderId + ArtistId;
        public string ProviderId;
        public string ArtistId;
        public string Name;
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