using HyPlayer.PlayCore.Model;

namespace HyPlayer.PlayCore.Service;

public class PlayServiceEvents
{
    public delegate void PlayEvent();
    public delegate void PauseEvent();
    public delegate void StopEvent();
    public delegate void FailedEvent();
    public delegate void PositionChangeEvent();
    public delegate void MediaEndEvent();
    public delegate void MediaLoadedEvent();
    public delegate bool PlayItemChangingEvent(SingleSong newPlayableItem, SingleSong oldPlayableItem);
    public delegate void PlayItemChangedEvent(SingleSong newPlayableItem, SingleSong oldPlayableItem);
    public delegate void PlayItemAddedEvent();
    public delegate void PlayItemRemovedEvent();
    public event PlayEvent OnPlay;
    public event PauseEvent OnPause;
    public event StopEvent OnStop;
    public event FailedEvent OnFailed;
    public event PositionChangeEvent OnPositionChange;
    public event MediaEndEvent OnMediaEnd;
    public event MediaEndEvent OnMediaLoaded;
    public event PlayItemChangingEvent OnPlayItemChanging;
    public event PlayItemChangedEvent OnPlayItemChanged;
    public event PlayItemAddedEvent OnPlayItemAdded;
    public event PlayItemRemovedEvent OnPlayItemRemoved;

    public void RaisePlayEvent() => OnPlay?.Invoke();
    public void RaisePauseEvent() => OnPause?.Invoke();
    public void RaiseStopEvent() => OnStop?.Invoke();
    public void RaiseFailedEvent() => OnFailed?.Invoke();
    public void RaisePositionChangeEvent() => OnPositionChange?.Invoke();
    public void RaiseMediaEndEvent() => OnMediaEnd?.Invoke();
    public void RaiseMediaLoadedEvent() => OnMediaLoaded?.Invoke();
    public void RaisePlayItemChangingEvent(SingleSong newItem,SingleSong oldItem) => OnPlayItemChanging?.Invoke(newItem,oldItem);
    public void RaisePlayItemChangedEvent(SingleSong newItem,SingleSong oldItem) => OnPlayItemChanged?.Invoke(newItem,oldItem);
    public void RaisePlayItemAddedEvent() => OnPlayItemAdded?.Invoke();
    public void RaisePlayItemRemovedEvent() => OnPlayItemRemoved?.Invoke();
}