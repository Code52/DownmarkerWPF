namespace MarkPad
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();

            var bootstrapper = new AppBootstrapper();
            Resources.Add("bootstrapper", bootstrapper);
        }

        public App(string path)
        {
            InitializeComponent();

            var bootstrapper = new AppBootstrapper();
            bootstrapper.OpenFile(path);
            Resources.Add("bootstrapper", bootstrapper);
        }
    }
}
