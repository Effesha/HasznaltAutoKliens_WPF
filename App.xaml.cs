using HasznaltAuto.Desktop.Singletons;
using System.Windows;

namespace HasznaltAutoKliens
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static GrpcServiceProvider GrpcService { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            GrpcService = GrpcServiceProvider.Instance;
            base.OnStartup(e);
        }
    }
}
