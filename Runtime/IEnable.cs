namespace ActionCode.Physics
{
    /// <summary>
    /// Interface used on objects able to be enabled.
    /// </summary>
    public interface IEnable
    {
        /// <summary>
        /// Whether this object is enabled.
        /// </summary>
        bool IsEnabled { get; set; }
    }
}