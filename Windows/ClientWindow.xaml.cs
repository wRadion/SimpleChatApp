using System.Windows;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        public ClientWindow()
        {
            InitializeComponent();
        }

        // Cette fonction se déclenche lorsque l'utilisateur appuie sur le bouton "Se connecter"
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = IpAddress.Text;
            int port = int.Parse(Port.Text);
            string username = Username.Text;

            // On essaye de se connecter au serveur avec le pseudo qu'on a rentré
            if (ApplicationClient.Get().Connect(ipAddress, port, username))
            {
                // Si le pseudo n'est pas déjà pris par quelqu'un d'autre qui est connecté sur le serveur,
                // On peux créer une nouvelle ChatWindow (fenêtre de chat), l'afficher et fermer la fenêtre actuelle
                ChatWindow window = new ChatWindow();
                window.Show();
                Close();
            }
            else // Sinon, si le pseudo est déjà pris par quelqu'un d'autre
            {
                // On affiche un message d'erreur
                MessageBox.Show("Ce pseudo est déjà pris.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                // On vide (clear) la textbox du pseudo, mais on reste sur la même fenêtre ! (on change pas de fenêtre)
                Username.Text = string.Empty;
            }
        }
    }
}
