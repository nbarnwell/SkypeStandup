using System;
using System.Windows;

namespace SkypeStandup
{
    /// <summary>
    /// Class implementing support for "minimize to tray" functionality.
    /// </summary>
    public static class HideOnMinimize
    {
        /// <summary>
        /// Enables "minimize to tray" behavior for the specified Window.
        /// </summary>
        /// <param name="window">Window to enable the behavior for.</param>
        public static void Enable(Window window)
        {
            // No need to track this instance; its event handlers will keep it alive
            new HideOnMinimizeInstance(window);
        }

        /// <summary>
        /// Class implementing "minimize to tray" functionality for a Window instance.
        /// </summary>
        private class HideOnMinimizeInstance
        {
            private Window _window;

            /// <summary>
            /// Initializes a new instance of the HideOnMinimizeInstance class.
            /// </summary>
            /// <param name="window">Window instance to attach to.</param>
            public HideOnMinimizeInstance(Window window)
            {
                _window = window;
                _window.StateChanged += HandleStateChanged;
            }

            /// <summary>
            /// Handles the Window's StateChanged event.
            /// </summary>
            /// <param name="sender">Event source.</param>
            /// <param name="e">Event arguments.</param>
            private void HandleStateChanged(object sender, EventArgs e)
            {
                var minimized = (_window.WindowState == WindowState.Minimized);
                _window.ShowInTaskbar = !minimized;
            }
        }
    }
}