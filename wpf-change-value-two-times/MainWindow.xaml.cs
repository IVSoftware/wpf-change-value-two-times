using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpf_change_value_two_times
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            base.DataContext = new Status();
        }
        new Status DataContext =>(Status)base.DataContext;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            DataContext.ReadyVisibility = Visibility.Hidden;
            // Processing ..
            await Task.Delay(TimeSpan.FromSeconds(2.5));

            DataContext.ReadyVisibility = Visibility.Visible;
        }
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