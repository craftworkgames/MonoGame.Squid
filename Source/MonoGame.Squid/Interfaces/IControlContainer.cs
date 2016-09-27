using MonoGame.Squid.Util;

namespace MonoGame.Squid.Interfaces
{
    /// <summary>
    /// Interface IControlContainer
    /// </summary>
    public interface IControlContainer
    {
        /// <summary>
        /// Gets or sets the controls.
        /// </summary>
        /// <value>The controls.</value>
        [Hidden]
        ControlCollection Controls { get; set; }
    }
}
