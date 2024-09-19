using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace wpf_change_value_two_times
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
        new MainWindowViewModel DataContext => (MainWindowViewModel)base.DataContext;

        private async void Test_Click(object sender, RoutedEventArgs e)
        {
            DataContext.Status.ReadyVisibility = Visibility.Hidden;

            var progress = new Progress<int>(percent =>
            {
                progressBar.Value = percent;
            });
            await Task.Run(() =>
            {
                // Simulate a 2 S background processing task.
                for (int i = 0; i <= 20; i++)
                {
                    ((IProgress<int>)progress).Report(i * 5);
                    Thread.Sleep(100);
                }
            });

            DataContext.Status.ReadyVisibility = Visibility.Visible;
        }

        private void TestProgrammatic_Click(object sender, RoutedEventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();
            Test_Click(sender, e);
            stopwatch.Stop();
            Dispatcher.BeginInvoke(() =>
                MessageBox.Show($"{nameof(Test_Click)} has returned after {stopwatch.ElapsedMilliseconds} ms."));
        }
    }
    public class MainWindowViewModel
    {
        public Status Status { get; set; } = new Status();
    }
    public class Status : INotifyPropertyChanged
    {
        public Visibility ReadyVisibility
        {
            get => _readyVisibility;
            set
            {
                if (!Equals(_readyVisibility, value))
                {
                    _readyVisibility = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BusyVisibility));
                    OnPropertyChanged(nameof(IsButtonEnabled));
                }
            }
        }
        Visibility _readyVisibility = Visibility.Visible;
        public Visibility BusyVisibility => 
            Equals(ReadyVisibility, Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;

        public bool IsButtonEnabled => Equals(ReadyVisibility, Visibility.Visible);

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}