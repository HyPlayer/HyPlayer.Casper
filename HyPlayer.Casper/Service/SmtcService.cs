using System;
using Windows.Media;
using Windows.Storage.Streams;
using HyPlayer.Casper.Model;

namespace HyPlayer.Casper.Service;

public class SmtcService
{
    public SystemMediaTransportControls Smtc;
    public SystemMediaTransportControlsDisplayUpdater Updater;

    public readonly SystemMediaTransportControlsTimelineProperties TimelineProperties =
        new SystemMediaTransportControlsTimelineProperties();
    
    public delegate void PlayStateChangingEvent(PlayingStatus status);

    public event PlayStateChangingEvent OnPlayStateChanging;
    
    public delegate void PlayAnotherEvent(bool isNext);

    public event PlayAnotherEvent OnPlayAnother;
    
    public delegate void PlayPositionChangingEvent(TimeSpan position);

    public event PlayPositionChangingEvent OnPlayPositionChanging;
    

    public void InitializeService()
    {
        Smtc = SystemMediaTransportControls.GetForCurrentView();
        Updater = Smtc.DisplayUpdater;
        Updater.Type = MediaPlaybackType.Music;
        Smtc.UpdateTimelineProperties(TimelineProperties);
        Smtc.ButtonPressed += (_, args) =>
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    OnPlayStateChanging?.Invoke(PlayingStatus.Playing);
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    OnPlayStateChanging?.Invoke(PlayingStatus.Paused);
                    break;
                case SystemMediaTransportControlsButton.Stop:
                    OnPlayStateChanging?.Invoke(PlayingStatus.None);
                    break;
                case SystemMediaTransportControlsButton.Next:
                    OnPlayAnother?.Invoke(true);
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    OnPlayAnother?.Invoke(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
        Smtc.PlaybackPositionChangeRequested += (_, args) => OnPlayPositionChanging?.Invoke(args.RequestedPlaybackPosition);
    }

    public void OnPlayPositionChanged(TimeSpan timeSpan)
    {
        TimelineProperties.Position = timeSpan;
    }

    public void OnPlayItemChanged(SingleSong newItem, SingleSong previousItem)
    {
        Smtc.IsEnabled = true;
        Updater.MusicProperties.Title = newItem.Name;
        Updater.MusicProperties.Artist = newItem.ArtistsString;
        Updater.MusicProperties.AlbumTitle = newItem.Album.Name;
        TimelineProperties.MaxSeekTime = newItem.Duration;
        Updater.Thumbnail = RandomAccessStreamReference.CreateFromStream(newItem.Album.GetCoverImageStream());
    }

    public void OnPlay()
    {
        Smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
    }

    public void OnPause()
    {
        Smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
    }

    public void OnStop()
    {
        Smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
    }
}