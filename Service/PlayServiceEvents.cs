using System;
using HyPlayer.Casper.Model;

namespace HyPlayer.Casper.Service;

public class PlayServiceEvents
{
    public delegate void FailedEvent();

    public delegate void MediaEndEvent();

    public delegate void MediaLoadedEvent();

    public delegate void PauseEvent();

    public delegate void PlayEvent();

    public delegate void PlayItemChangedEvent(SingleSong newSingleSong, SingleSong oldSingleSong);

    public delegate bool PlayItemChangingEvent(SingleSong newSingleSong, SingleSong oldSingleSong);

    public delegate void PlayListChangedEvent();

    public delegate void PlayListClearedEvent();

    public delegate void PlaySourceChangedEvent(SongContainer source);

    public delegate void PositionChangedEvent();

    public delegate void StopEvent();

    public event PlayEvent OnPlay;
    public event PauseEvent OnPause;
    public event StopEvent OnStop;
    public event FailedEvent OnFailed;
    public event PositionChangedEvent OnPositionChanged;
    public event MediaEndEvent OnMediaEnd;
    public event MediaEndEvent OnMediaLoaded;
    [Obsolete] public event PlayItemChangingEvent OnPlayItemChanging;
    public event PlayItemChangedEvent OnPlayItemChanged;
    public event PlayListChangedEvent OnPlayListChanged;
    public event PlayListClearedEvent OnPlayListCleared;
    public event PlaySourceChangedEvent OnPlaySourceChanged;

    public void RaisePlayEvent()
    {
        OnPlay?.Invoke();
    }

    public void RaisePauseEvent()
    {
        OnPause?.Invoke();
    }

    public void RaiseStopEvent()
    {
        OnStop?.Invoke();
    }

    public void RaiseFailedEvent()
    {
        OnFailed?.Invoke();
    }

    public void RaisePositionChangedEvent()
    {
        OnPositionChanged?.Invoke();
    }

    public void RaiseMediaEndEvent()
    {
        OnMediaEnd?.Invoke();
    }

    public void RaiseMediaLoadedEvent()
    {
        OnMediaLoaded?.Invoke();
    }

    [Obsolete]
    public void RaisePlayItemChangingEvent(SingleSong newItem, SingleSong oldItem)
    {
        OnPlayItemChanging?.Invoke(newItem, oldItem);
    }

    public void RaisePlayItemChangedEvent(SingleSong newItem, SingleSong oldItem)
    {
        OnPlayItemChanged?.Invoke(newItem, oldItem);
    }

    public void RaisePlayListChangedEvent()
    {
        OnPlayListChanged?.Invoke();
    }

    public void RaisePlayListClearedEvent()
    {
        OnPlayListCleared?.Invoke();
    }

    public void RaisePlaySourceChangedEvent(SongContainer source)
    {
        OnPlaySourceChanged?.Invoke(source);
    }
}