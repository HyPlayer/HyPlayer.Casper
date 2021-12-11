using System;
using Windows.Media;
using Windows.Storage.Streams;
using HyPlayer.Casper.Model;

namespace HyPlayer.Casper.Service;

public class SmtcService
{
    public delegate void PlayAnotherEvent(bool isNext);

    public delegate void PlayPositionChangingEvent(TimeSpan position);

    public delegate void PlayStateChangingEvent(PlayingStatus status);

    public readonly SystemMediaTransportControlsTimelineProperties TimelineProperties = new();

    public SystemMediaTransportControls Smtc;
    public SystemMediaTransportControlsDisplayUpdater Updater;

    public event PlayStateChangingEvent OnPlayStateChanging;

    public event PlayAnotherEvent OnPlayAnother;

    public event PlayPositionChangingEvent OnPlayPositionChanging;

    public void InitializeService(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero) Smtc = SystemMediaTransportControls.GetForCurrentView();
        else Smtc = SystemMediaTransportControlsInterop.GetForWindow(windowHandle);
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
        Smtc.PlaybackPositionChangeRequested +=
            (_, args) => OnPlayPositionChanging?.Invoke(args.RequestedPlaybackPosition);
    }

    public void OnPlayPositionChanged(TimeSpan timeSpan)
    {
        TimelineProperties.Position = timeSpan;
    }

    public async void OnPlayItemChanged(SingleSong newItem, SingleSong previousItem)
    {
        Smtc.IsPlayEnabled = true;
        Smtc.IsPauseEnabled = true;
        Smtc.IsNextEnabled = true;
        Smtc.IsPreviousEnabled = true;
        Smtc.IsEnabled = true;
        Updater.MusicProperties.Title = newItem.Name;
        Updater.MusicProperties.Artist = newItem.ArtistsString;
        Updater.MusicProperties.AlbumTitle = newItem.Album.Name;
        Updater.Update();
        TimelineProperties.MaxSeekTime = newItem.Duration;
        Smtc.UpdateTimelineProperties(TimelineProperties);
        Updater.Thumbnail = RandomAccessStreamReference.CreateFromStream(await newItem.Album.GetCoverImageStream());
        Updater.Update();
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