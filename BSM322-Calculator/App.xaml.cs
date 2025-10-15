namespace BSM322_Calculator
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new(new AppShell())
            {
                Title = "BKO Calculator",
                Width = 450,
                Height = 900
            };
            return window;
        }
    }
}