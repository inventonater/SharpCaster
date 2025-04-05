namespace Inventonater.Chromecast.Models.Media
{
    /// <summary>
    /// Represents the type of player state
    /// </summary>
    public enum PlayerStateType
    {
        /// <summary>
        /// Idle state
        /// </summary>
        Idle,
        
        /// <summary>
        /// Playing state
        /// </summary>
        Playing,
        
        /// <summary>
        /// Paused state
        /// </summary>
        Paused,
        
        /// <summary>
        /// Buffering state
        /// </summary>
        Buffering,
        
        /// <summary>
        /// Loading state
        /// </summary>
        Loading
    }
}
