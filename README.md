What should help your problem, is to **make the click handler async and await your processing task**. The excellent comment added by Selvin articulates the underlying issue of blocking the UI thread. Making the handler `async` fixes that. For example, you could disable the `Test` button, hide `Ready`, show `Busy`, run the work with a progress update on the UI thread, then reverse the status indicators.

```
private async void Button_Click(object sender, RoutedEventArgs e)
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
```
[![screenshots][1]][1]
___

Another possible point of failure is bindings that aren't bound to the intended models. If this is the case, it still won't work in spite of changing the click handler. So, out of an abundance of caution, here's the full code I used to test this answer.

##### Xaml

```
<Window x:Class="wpf_change_value_two_times.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:wpf_change_value_two_times"
        mc:Ignorable="d"
        Title="MainWindow" Height="300" Width="500">

    <Window.DataContext>
        <local:MainWindowViewModel/>
    </Window.DataContext>
    <Window.Resources>
        <local:MainWindowViewModel x:Key="ViewModel"/>
        <Style x:Key="CenteredContentStyle" TargetType="Control">
            <Setter Property="HorizontalAlignment" Value="Center"/>
            <Setter Property="VerticalAlignment" Value="Center"/>
            <Setter Property="HorizontalContentAlignment" Value="Center"/>
            <Setter Property="VerticalContentAlignment" Value="Center"/>
            <Setter Property="Height" Value="50"/>
            <Setter Property="Width" Value="100"/>
        </Style>
    </Window.Resources>

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <Grid Grid.Row="0">
            <Label 
                Style="{StaticResource CenteredContentStyle}"
                Content="Ready" 
                Visibility="{Binding Status.ReadyVisibility}"
                Background="Green"
                Foreground="White" />
            <Label
                Style="{StaticResource CenteredContentStyle}"
                Content="Busy"
                Visibility="{Binding Status.BusyVisibility}"  
                Grid.Column="0"
                Background="LightSalmon"
                Foreground="Yellow"  />
        </Grid>

        <Button 
            Style="{StaticResource CenteredContentStyle}"
            IsEnabled="{Binding Status.IsButtonEnabled}" 
            Content="Test"
            Grid.Row="1"
            Click="Button_Click"/>

        <ProgressBar 
            x:Name="progressBar" 
            Grid.Row="2"
            Minimum="0" 
            Maximum="100" 
            Height="20"
            HorizontalAlignment="Stretch"/>
    </Grid>
</Window>

```

##### C#

```
using System.ComponentModel;
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

        private async void Button_Click(object sender, RoutedEventArgs e)
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
    }
```
###### Main view model
```
    public class MainWindowViewModel
    {
        public Status Status { get; set; } = new Status();
    }
    
```
###### Status subclass
```
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
```


  [1]: https://i.sstatic.net/XXOCdYcg.png