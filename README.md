I believe that what you're seeing is the same problem behavior and the same solution as described here: https://stackoverflow.com/a/79006823

In short, one needs to be _very_ careful about showing a window before the `MainWindow` handle is created. Otherwize the framework can interpret that transient window as being the application main with unpredictable consequences. This means that an ideal place to show something like `LoadingWindow` is in the `Loaded` event of `MainWindow` after making sure that you keep the main hidden until authorization has been obtained.


~~~
public partial class MainWindow : Window
{
    public MainWindow()
    {
        // Minimize the window
        WindowState = WindowState.Minimized;
        // Hide TaskBar icon so there's no temptation to un-minimize it until we say so! 
        ShowInTaskbar = false;
        Width = 0;
        Height = 0;
        WindowStyle = WindowStyle.None;

        InitializeComponent();
        Loaded += async (sender, e) =>
        {
            // NOW the native hWnd exists AND it's the first hWnd to come
            // into existence, making this class (MainWindow) the "official"
            // application main window. This means that when it closes, the
            // app will exit as long as we make our other window handles "behave".

            var loadingWindow = new LoadingWindow();
            while (loadingWindow.LoginState != LoginState.Authorized)
            {
                switch (loadingWindow.LoginState)
                {
                    case LoginState.None:
                        await loadingWindow.PromptForUidAsync();
                        break;
                    case LoginState.Invalid:
                        MessageBox.Show("UID cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        loadingWindow.LoginState = LoginState.None;
                        break;
                    case LoginState.PasswordRequired:
                        await loadingWindow.PromptForPasswordAsync();
                        break;
                    case LoginState.Authorized:
                        break;
                    case LoginState.Canceled:
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            Title = $"Welcome {loadingWindow.UserName}";

            WindowState = WindowState.Normal;

            // Set the MAIN WINDOW dimensions that you actually want here, and turn the TaskBar icon back on.
            Width = 500;
            Height = 300;
            ShowInTaskbar = true;
            localCenterToScreen();
            WindowStyle = WindowStyle.SingleBorderWindow;
            // ^^^^ Do all this BEFORE closing the splash. It's a smoke-and-
            //      mirrors trick that hides some of the ugly transient draws.
            loadingWindow.Close();

            #region L o c a l M e t h o d s
            void localCenterToScreen()
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;
                double left = (screenWidth - Width) / 2;
                double top = (screenHeight - Height) / 2;
                Left = left;
                Top = top;
            }
            #endregion L o c a l M e t h o d s
        };
    }
}
~~~

**Minimal Example**

As a proof of concept only, you could implement `LoadingWindow` like this:

~~~
public enum LoginState
{
    None,
    Invalid,
    PasswordRequired,
    Authorized,
    Canceled,
}
public partial class LoadingWindow : Window
{
    public LoadingWindow()
    {
        InitializeComponent();
        buttonSubmit.Click += (sender, e) =>
        {
            // Validate the input
            switch (LoginState)
            {
                case LoginState.None:
                    if(string.IsNullOrWhiteSpace(textBoxUserInput.Text))
                    {
                        LoginState = LoginState.Invalid;
                    }
                    else
                    {
                        LoginState = LoginState.PasswordRequired;
                    }
                    break;
                case LoginState.PasswordRequired:
                    // Validate based on credential
                    LoginState = LoginState.Authorized;
                    break;
            }
            _awaiter.Release();
        };
    }
    SemaphoreSlim _awaiter = new SemaphoreSlim(1, 1);
    public async Task PromptForUidAsync()
    {
        textBlockPrompt.Text = "Please enter your User ID:";
        textBoxUserInput.Focus();
        _awaiter.Wait(0);
        Show();
        await _awaiter.WaitAsync();
    }

    internal async Task PromptForPasswordAsync()
    {
        textBlockPrompt.Text = "Please enter your Password:";
        textBoxUserInput.Visibility = Visibility.Hidden;
        passwordBoxUserInput.Visibility = Visibility.Visible;
        passwordBoxUserInput.Focus();
        _awaiter.Wait(0);
        Show();
        await _awaiter.WaitAsync();
    }

    public LoginState LoginState { get; set; }
    public string UserName => textBoxUserInput.Text;
}
~~~

