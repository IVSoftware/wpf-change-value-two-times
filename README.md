You're showing label visibility bound to properties:

```
<Label Name="Busy" Content="Busy" Visibility="{Binding Path=BusyVisibility, UpdateSourceTrigger=PropertyChanged}" />
<Label Name="Ready" Content="Ready" Visibility="{Binding Path=ReadyVisibility, UpdateSourceTrigger=PropertyChanged}" Grid.Row="0" />
```

From this it's a bit unclear whether these are looking at the correct binding context because on one hand your code reads `ReadyVisibility = false` which implies a bool, when the property should be of type `Visibility`. So I'm going to set up a binding context just to demo what I think is really going to help your problem, which is to **make the click handler `async` and await your processing task**.

___
OK, so let's add an `IsEnabled` binding for the `Button`.

```
<Button IsEnabled="{Binding IsButtonEnabled}" Content="Test" Grid.Row="1" HorizontalAlignment="Center" VerticalAlignment="Center" Height="50" Width="100" Click="Button_Click"/>
```

___

#### Binding setup for illustration

**For demonstration purposes only**, let's make some sufficiently functional bindings so that we can look at what might be the real issue.

```
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        base.DataContext = new Status();
    }
    new Status DataContext =>(Status)base.DataContext;
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
```

___
Then what I believe would go a long way toward making your click handler work the way you intend, is to make it `async` so that you can `await` the processing task.
```
public partial class MainWindow : Window
{
    .
    .
    .
    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        DataContext.ReadyVisibility = Visibility.Hidden;
        // Processing ..
        await Task.Delay(TimeSpan.FromSeconds(2.5));

        DataContext.ReadyVisibility = Visibility.Visible;
    }
}
```

[![screenshot][1]][1]


  [1]: https://i.sstatic.net/BHgIIJdz.png