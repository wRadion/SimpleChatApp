using System.Windows;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();

            // Lorsque la fenêtre se ferme, on déconnecte proprement le Client et on ferme proprement le Serveur
            Closed += (sender, e) =>
            {
                ApplicationClient.Get().Disconnect();
                ApplicationServer.Get().Stop();
            };

            // On récupère la liste des pseudos des utilisateurs connectés sur le serveur
            // Et pour chaque pseudo, on l'ajoute dans la listbox UserList
            foreach (string user in ApplicationClient.Get().ReceiveUserList())
                UserList.Items.Add(user);

            // On ajoute le message de bienvenue dans la chatbox
            AddChatBoxLine($"{ ApplicationClient.Get().ReceiveWelcomeMessage() }");

            // Lorsque le client reçoit un message de type "CHAT_MESSAGE", on l'ajoute dans la chatbox
            // CHAT_MESSAGE = un utilisateur a envoyé un message dans le tchat
            ApplicationClient.Get().ChatMessage += (args) =>
            {
                // Le Dispatcher permet de modifier l'état de l'interface en dehors du thread principal
                // Ici, on ajoute une ligne dans la chatbox
                Dispatcher.Invoke(() =>
                {
                    AddChatBoxLine($"{ args.Username }: { args.Message }");
                });
            };

            // Lorsque le client reçoit un message de type "USER_JOINED", on l'ajoute dans la liste des utilisateurs connectés
            // USER_JOINED = un utilisateur s'est connecté sur le serveur
            ApplicationClient.Get().UserJoined += (args) =>
            {
                // Le Dispatcher permet de modifier l'état de l'interface en dehors du thread principal
                // Ici, on ajoute une ligne dans la chatbox et un élément dans la liste des utilisateurs connectés
                Dispatcher.Invoke(() =>
                {
                    AddChatBoxLine($"{ args.Username } vient de se connecter.");
                    UserList.Items.Add(args.Username);
                });
            };

            // Lorsque le client reçoit un message de type "USER_LEFT", on le retire de la liste des utilisateurs connectés
            // USER_LEFT = un utilisateur s'est déconnecté du serveur
            ApplicationClient.Get().UserLeft += (args) =>
            {
                // Le Dispatcher permet de modifier l'état de l'interface en dehors du thread principal
                // Ici, on ajoute une ligne dans la chatbox et on retire un élément de la liste des utilisateurs connectés
                Dispatcher.Invoke(() =>
                {
                    AddChatBoxLine($"{ args.Username } s'est déconnecté.");
                    UserList.Items.Remove(args.Username);
                });
            };

            // On écoute tous le serveur. Si on reçoit un message, le code que l'on a indiqué juste avant va être exécuté (en fonction du message)
            ApplicationClient.Get().StartListening();
        }

        /// <summary>
        /// Ajoute une ligne dans la chatbox.
        /// </summary>
        /// <param name="line">La ligne à ajouter</param>
        private void AddChatBoxLine(string line)
        {
            // On ajoute la ligne dans la chatbox
            ChatBox.Items.Add(line);
            // On scroll automatiquement uniquement si l'utilisateur est déjà tout en bas du scrolling
            if (ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight)
                ScrollViewer.ScrollToEnd();
        }

        // Cette fonction se déclenche lorsque l'utilisateur appuie sur le bouton "Envoyer"
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            // On envoie le message au serveur
            ApplicationClient.Get().SendChatMessage(Message.Text);
            // On vide (clear) la textbox Message
            Message.Text = string.Empty;
        }
    }
}
