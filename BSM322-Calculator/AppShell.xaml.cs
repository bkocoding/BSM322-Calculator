namespace BSM322_Calculator
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            if (Application.Current != null)
            {
                ApplyThemeIcons(Application.Current.RequestedTheme);
                Application.Current.RequestedThemeChanged += (s, e) =>
                {
                    ApplyThemeIcons(e.RequestedTheme);
                };
            }
        }

        void ApplyThemeIcons(AppTheme? theme)
        {
            if (theme == AppTheme.Dark)
            {
                Standard.Icon = "standard_dark.png";
                Scientific.Icon = "scientific_dark.png";
            }
            else
            {
                Standard.Icon = "standard_light.png";
                Scientific.Icon = "scientific_light.png";
            }
        }
    }
}
