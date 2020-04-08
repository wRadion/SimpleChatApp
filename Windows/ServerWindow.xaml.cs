using System.Windows;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        public ServerWindow()
        {
            InitializeComponent();
        }

        // Cette fonction se déclenche lorsque l'utilisateur appuie sur le bouton "Démarrer le serveur"
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            int port = int.Parse(Port.Text);
            string username = Username.Text;

            // Démarrer le serveur
            ApplicationServer.Get().Start(port);
            // Le met sur écoute, en attente de la connexion de nouveau(x) client(s)
            ApplicationServer.Get().StartAccepting();

            // Connecte directement un client pour pouvoir utiliser l'application en mode client ET avoir un serveur qui écoute en arrière-plan
            ApplicationClient.Get().Connect("127.0.0.1", port, username);

            // Créer une nouvelle ChatWindow (la fenêtre de tchat), l'affiche, et ferme la fenêtre actuelle
            ChatWindow window = new ChatWindow();
            window.Show();
            Close();
        }
    }
}
