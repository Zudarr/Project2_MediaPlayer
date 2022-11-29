using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        ObservableCollection<MediaFile> MediaList = new ObservableCollection<MediaFile>(); //list chứa tên file để hiển thị ra UI
        List<string> PathList = new(); //list chứa đường dẫn đầy đủ của từng file, thao tác với file media trên list này
        private string PlayingMediaPath = "";
        private bool IsPlayLoop = false;
        private bool IsPlayShuffle = false;

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PathList = File.ReadAllLines("LatestPlaylist.txt").ToList();

            foreach (var path in PathList)
            {
                var newFile = new MediaFile()
                {
                    FileName = Path.GetFileName(path),
                };
                MediaList.Add(newFile);
            }

            PlayListView.ItemsSource = MediaList;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            File.WriteAllLines("LatestPlaylist.txt", PathList);
        }

        private string NextMedia() //hàm quyết định file tiếp theo được phát là file nào 
        {
            var index = PathList.IndexOf(PlayingMediaPath);

            if (IsPlayLoop)
            {
                return PathList[index];
            }

            if (IsPlayShuffle)
            {
                Random rnd = new Random();
                int NextMediaIndex = rnd.Next(PathList.Count - 1);
                return PathList[NextMediaIndex];
            }

            if (index == PathList.Count - 1)
            {
                return PathList[0];
            }
            else
            {
                return PathList[index + 1];
            }
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
                    if (!PathList.Contains(file))
                    {
                        PathList.Add(file);
                        var newFile = new MediaFile()
                        {
                            FileName = Path.GetFileName(file),
                        };
                        MediaList.Add(newFile);
                    }
                }

                if (PlayingMediaPath == "")
                {
                    PlayingMediaPath = PathList[0];
                    MediaPlayer.Source = new Uri(PlayingMediaPath); // sau dòng này gọi hàm MediaPlayer_MediaOpened

                    MediaIsPlaying = true;
                    MediaPlayer.Play();
                }
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (MediaIsPlaying)
            {
                MediaIsPlaying = false;
                MediaPlayer.Pause();
                PlayPauseButton.Content = FindResource("Play");
            }
            else
            {
                MediaIsPlaying = true;
                MediaPlayer.Play();
                PlayPauseButton.Content = FindResource("Pause");
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
            MediaIsPlaying = true;
            MediaPlayer.Play();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            PlayingMediaPath = NextMedia();

            MediaPlayer.Source = new Uri(PlayingMediaPath);
            MediaIsPlaying = true;
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
            if (IsPlayShuffle)
            {
                IsPlayShuffle = false;
                PlayShuffleButton.Background = Brushes.White;
            }
            else
            {
                IsPlayShuffle = true;
                PlayShuffleButton.Background = Brushes.LightGreen;
            }
        }

        private void PlayLoopOneButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsPlayLoop)
            {
                IsPlayLoop = false;
                PlayLoopOneButton.Background = Brushes.White;
            }
            else
            {
                IsPlayLoop = true;
                PlayLoopOneButton.Background = Brushes.LightGreen;
            }
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayingMediaPath = NextMedia();

            MediaPlayer.Source = new Uri(PlayingMediaPath);
            MediaIsPlaying = true;
            MediaPlayer.Play();
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {//khi file media được mở thì dòng lệnh này mới chạy được
            LabelMaximumTime.Text = TimeSpan.FromSeconds(MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds)
                                            .ToString(@"hh\:mm\:ss");
        }

        private void deleteFile_Click(object sender, RoutedEventArgs e)
        {
            int index = PlayListView.SelectedIndex;

            if (PlayingMediaPath == PathList[index]) //xử lý khi xóa file đang chạy
            {
                PlayingMediaPath = NextMedia();

                MediaPlayer.Source = new Uri(PlayingMediaPath);
                MediaIsPlaying = true;
                MediaPlayer.Play();
            }

            PathList.RemoveAt(index);
            MediaList.RemoveAt(index);

            if (PathList.Count == 0)
            {
                PlayingMediaPath = "";
                MediaIsPlaying = false;
                MediaPlayer.Stop();
            }
        }

        private void PlayListItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var fileName = sender as TextBlock;
            var filePath = PathList.Where(p => Path.GetFileName(p) == fileName.Text);

            foreach(var file in filePath)
            {
                PlayingMediaPath = file;
                MediaPlayer.Source = new Uri(PlayingMediaPath);

                MediaIsPlaying = true;
                MediaPlayer.Play();
            }
        }
    }
}