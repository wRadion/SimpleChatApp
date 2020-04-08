using System.Windows;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for ChooseModeWindow.xaml
    /// </summary>
    public partial class ChooseModeWindow : Window
    {
        public ChooseModeWindow()
        {
            InitializeComponent();
        }

        // Cette fonction se déclenche lorsque l'utilisateur clique sur le bouton "Mode Serveur"
        private void Server_Click(object sender, RoutedEventArgs e)
        {
            // Créer une nouvelle fenêtre de création de serveur, l'affiche et ferme la fenêtre actuelle
            ServerWindow window = new ServerWindow();
            window.Show();
            Close();
        }

        // Cette fonction se déclenche lorsque l'utilisateur clique sur le bouton "Mode Client"
        private void Client_Click(object sender, RoutedEventArgs e)
        {
            // Créer une nouvelle fenêtre de connexion à un serveur (mode client), l'affiche et ferme la fenêtre actuelle
            ClientWindow window = new ClientWindow();
            window.Show();
            Close();
        }
    }
}
