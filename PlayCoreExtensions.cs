using HyPlayer.Casper.Model;

namespace HyPlayer.Casper;
// 此文件是为了使 Casper 的 PlayCore 简洁而确定设计的
// 此类中的操作更加符合日常逻辑
public static class PlayCoreExtensions
{
    public static void ReplacePlaySourceAndMoveToStart(this PlayCore playCore, SongContainer playListSource)
    {
        playCore.ReplacePlaySource(playListSource);
        playCore.LoadPlaySource();
        playCore.MoveTo(0);
    }
    
    public static async void MoveNextAndPlay(this PlayCore playCore)
    {
        await playCore.PlayService.Stop();
        playCore.MoveNext();
        await playCore.LoadNowPlayingItemMedia();
        await playCore.PlayService.Play();
    }
    
    public static async void MovePreviousAndPlay(this PlayCore playCore)
    {
        await playCore.PlayService.Stop();
        playCore.MovePrevious();
        await playCore.LoadNowPlayingItemMedia();
        await playCore.PlayService.Play();
    }
    
    public static async void MoveToAndPlay(this PlayCore playCore,int index)
    {
        await playCore.PlayService.Stop();
        playCore.MoveTo(index);
        await playCore.LoadNowPlayingItemMedia();
        await playCore.PlayService.Play();
    }
    
}