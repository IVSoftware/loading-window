using System.Windows;

namespace loading_window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
                            MessageBox.Show("Not Authorized", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            Application.Current.Shutdown();
                            return;
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
}