using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace loading_window
{
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
        protected override void OnClosed(EventArgs e)
        {
            if(!_awaiter.Wait(0))
            {
                _awaiter.Release();
                LoginState = LoginState.Canceled;
            }
            _awaiter.Dispose();
            base.OnClosed(e);
        }
    }
}
