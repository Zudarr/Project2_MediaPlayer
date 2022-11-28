using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace Project2_MediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool MediaIsPlaying = false;
        private bool UserIsDraggingSlider = false;
        ObservableCollection<MediaFile> MediaList = new ObservableCollection<MediaFile>();
        List<string> PathList = new();
        private string PlayingMediaPath = "";

        public MainWindow()
        {
            InitializeComponent();
            PlayListView.ItemsSource = MediaList;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((MediaPlayer.Source != null) && (MediaPlayer.NaturalDuration.HasTimeSpan) && (!UserIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = MediaPlayer.Position.TotalSeconds;
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MediaPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            screen.Filter = "Media files (*.mp3;*.mp4;*.mkv)|*.mp3;*.mp4;*.mkv|All files (*.*)|*.*";
            screen.Multiselect = true;

            if (screen.ShowDialog() == true)
            {
                foreach(var file in screen.FileNames)
                {
                    var newFile = new MediaFile()
                    {
                        FileName = Path.GetFileName(file),
                    };
                    MediaList.Add(newFile);
                    PathList.Add(file);
                }
                PlayingMediaPath = PathList[0];
                MediaPlayer.Source = new Uri(PlayingMediaPath);

                MediaIsPlaying = true;
                MediaPlayer.Play();
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if(MediaIsPlaying)
            {
                MediaIsPlaying = false;
                MediaPlayer.Pause();
            }
            else
            {
                MediaIsPlaying = true;
                MediaPlayer.Play();
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var index = PathList.IndexOf(PlayingMediaPath);

            if (index == 0)
            {
                PlayingMediaPath = PathList[PathList.Count - 1];
            }
            else
            {
                PlayingMediaPath = PathList[index - 1];
            }

            MediaPlayer.Source = new Uri(PlayingMediaPath);
            MediaPlayer.Play();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var index = PathList.IndexOf(PlayingMediaPath);

            if (index == PathList.Count - 1)
            {
                PlayingMediaPath = PathList[0];
            }
            else
            {
                PlayingMediaPath = PathList[index + 1];
            }

            MediaPlayer.Source = new Uri(PlayingMediaPath);
            MediaPlayer.Play();
        }

        private void sliProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            UserIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            UserIsDraggingSlider = false;
            MediaPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabelCurrentTime.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void PlayShuffleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            var index = PathList.IndexOf(PlayingMediaPath);

            if (index == PathList.Count - 1)
            {
                PlayingMediaPath = PathList[0];
            }
            else
            {
                PlayingMediaPath = PathList[index + 1];
            }

            MediaPlayer.Source = new Uri(PlayingMediaPath);
            MediaPlayer.Play();
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            LabelMaximumTime.Text = TimeSpan.FromSeconds(MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds)
                                            .ToString(@"hh\:mm\:ss");
        }

        private void deleteFile_Click(object sender, RoutedEventArgs e)
        {
            int index = PlayListView.SelectedIndex;

            if (PlayingMediaPath == PathList[index])
            {
                if (index == PathList.Count - 1)
                {
                    PlayingMediaPath = PathList[0];
                }
                else
                {
                    PlayingMediaPath = PathList[index + 1];
                }

                MediaPlayer.Source = new Uri(PlayingMediaPath);
                MediaPlayer.Play();
            }

            PathList.RemoveAt(index);
            MediaList.RemoveAt(index);
        }
    }
}
