using System.IO;
using System.Windows;
using System.Windows.Controls;
using OpenZH.DataViewer.Framework;

namespace OpenZH.DataViewer.Views
{
    public partial class AudioVideoFileContentView : UserControl
    {
        private bool WasPlaying = false;

        private RelayCommand m_PauseCommand = null;
        private RelayCommand m_PlayCommand = null;
        private RelayCommand m_StopCommand = null;

        /// <summary>
        /// Gets the pause command.
        /// </summary>
        /// <value>
        /// The pause command.
        /// </value>
        public RelayCommand PauseCommand
        {
            get
            {
                if (m_PauseCommand == null)
                    m_PauseCommand = new RelayCommand(() => { Media.Pause(); }, null);

                return m_PauseCommand;
            }
        }

        /// <summary>
        /// Gets the play command.
        /// </summary>
        /// <value>
        /// The play command.
        /// </value>
        public RelayCommand PlayCommand
        {
            get
            {
                if (m_PlayCommand == null)
                    m_PlayCommand = new RelayCommand(() => { Media.Play(); }, null);

                return m_PlayCommand;
            }
        }

        /// <summary>
        /// Gets the stop command.
        /// </summary>
        /// <value>
        /// The stop command.
        /// </value>
        public RelayCommand StopCommand
        {
            get
            {
                if (m_StopCommand == null)
                    m_StopCommand = new RelayCommand(() => { Media.Stop(); }, null);

                return m_StopCommand;
            }
        }

        public AudioVideoFileContentView()
        {
            InitializeComponent();

            // TODO: Put this in a sensible location.
            Unosquare.FFME.MediaElement.FFmpegDirectory = Path.GetFullPath(@"..\..\..\..\lib\FFMediaElement\Native");

            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Media.Dispose();
        }

        /// <summary>
        /// Handles the MouseDown event of the PositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void PositionSlider_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WasPlaying = Media.IsPlaying;
            Media.Pause();
        }

        /// <summary>
        /// Handles the MouseUp event of the PositionSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void PositionSlider_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (WasPlaying) Media.Play();
        }
    }
}
