using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using ChatApp.Events;

namespace ChatApp
{
    /// <summary>
    /// <para>Représente l'objet qui permet de se connecter à un serveur (client).</para>
    /// <para>La classe implémente le design pattern "Singleton".</para>
    /// <para><see href="https://fr.wikipedia.org/wiki/Singleton_(patron_de_conception)"/></para>
    /// </summary>
    public class ApplicationClient
    {
        #region Singleton
        private static ApplicationClient Instance = null;
        public static ApplicationClient Get()
        {
            if (Instance == null)
                Instance = new ApplicationClient();
            return Instance;
        }
        #endregion

        // Un delegate représente un type de fonction et ils sont utilisés ici par les events (voir juste en dessous)
        //      Une valeur "standard" peut être de type int, string, char, ...
        //      Les fonctions peuvent être d'un certain, c'est-à-dire
        //          - prendre en argument un certain nombre et certain type d'argument
        //          - renvoyer un certain type de valeur (ou aucune valeur = void)
        public delegate void ChatMessageEventHandler(ChatMessageEventArgs args);
        public delegate void UserJoinedEventHandler(UserJoinedEventArgs args);
        public delegate void UserLeftEventHandler(UserLeftEventArgs args);

        // Lorsqu'un event se "produit", toutes les fonctions qui ont été "ajoutés" (à l'aide du +=) vont être appelées à ce moment-là
        // Ces fonctions seront du type indiqué, ici les ________EventHandler (voir juste au dessus)
        public event ChatMessageEventHandler ChatMessage;
        public event UserJoinedEventHandler UserJoined;
        public event UserLeftEventHandler UserLeft;

        private Socket _server;
        private BinaryWriter _writer;
        private BinaryReader _reader;

        // Le constructeur de la classe est privé car on utilise un Singleton.
        private ApplicationClient()
        {
            _server = null;
            _writer = null;
            _reader = null;
        }

        /// <summary>
        /// Permet de se connecter au serveur donné.
        /// </summary>
        /// <param name="ipAddress">L'adresse IP du serveur sur lequel on souhaite se connecter</param>
        /// <param name="port">Le port sur lequel le serveur écoute</param>
        /// <param name="username">Le pseudo qu'a rentré/écrit l'utilisateur dans l'interface</param>
        /// <returns>True si la connexion est validée, False si le pseudo est déjà pris par quelqu'un d'autre (la connexion n'est pas validée)</returns>
        public bool Connect(string ipAddress, int port, string username)
        {
            IPAddress address = Dns.GetHostEntry(ipAddress).AddressList.First((ip) => ip.AddressFamily == AddressFamily.InterNetwork);
            
            _server = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _server.Connect(new IPEndPoint(address, port));

            NetworkStream stream = new NetworkStream(_server);
            _writer = new BinaryWriter(stream);
            _reader = new BinaryReader(stream);

            // On envoie au serveur le pseudo désiré
            _writer.Write(username);
            
            // On reçoit le booléen qui décide si oui ou non la connexion est validée (ici, simplement si le pseudo est déjà pris -false- ou non -true-)
            return _reader.ReadBoolean();
        }

        /// <summary>
        /// <para>Ecoute les messages qu'envoie le serveur.</para>
        /// <para>Le code de cette fonction n'est pas exécuté sur le thread principal.</para>
        /// </summary>
        public void StartListening()
        {
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        // On attends que le serveur envoie un message (objet) (ici, de type int qui va être casté en MessageType)
                        // Cette ligne est bloquante pour le programme, c'est pourquoi on l'exécute dans un autre thread.
                        MessageType type = (MessageType)_reader.ReadInt32();
                        // On lit le premier argument du message (ici, le premier argument sera toujours le pseudo de l'utilisateur)
                        string username = _reader.ReadString();

                        switch (type)
                        {
                            case MessageType.CHAT_MESSAGE:
                                // On lit le deuxième argument qui sera le message du tchat envoyé
                                string message = _reader.ReadString();
                                // On déclenche l'évènement ChatMessage (et ainsi appelle toutes les fonctions qui ont été ajoutées)
                                ChatMessage?.Invoke(new ChatMessageEventArgs(username, message));
                                break;
                            case MessageType.USER_JOINED:
                                // On déclenche l'évènement UserJoined (et ainsi appelle toutes les fonctions qui ont été ajoutées)
                                UserJoined?.Invoke(new UserJoinedEventArgs(username));
                                break;
                            case MessageType.USER_LEFT:
                                // On déclenche l'évènement UserLeft (et ainsi appelle toutes les fonctions qui ont été ajoutées)
                                UserLeft?.Invoke(new UserLeftEventArgs(username));
                                break;
                        }
                    }
                }
                // On gère l'erreur qui se produit dans le cas où le serveur ne réponds plus :
                //      - soit y'a un problème de connexion entre le client et le serveur
                //      - soit le serveur a été fermé/déconnecté volontairement
                catch (IOException)
                {
                    // On affiche un message d'erreur
                    MessageBox.Show("Le serveur n'est plus accessible.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    // On quitte entièrement l'application
                    Application.Current.Shutdown();
                }
            });
        }

        /// <summary>
        /// Envoi le message de tchat entré par l'utilisateur (client).
        /// </summary>
        /// <param name="message">Le message à envoyer</param>
        public void SendChatMessage(string message)
        {
            _writer.Write(message);
        }

        /// <summary>
        /// Récupère la liste des utilisateurs connectés envoyée par le serveur.
        /// </summary>
        /// <returns>La liste des pseudos des utilisateurs connectés</returns>
        public string[] ReceiveUserList()
        {
            int length = _reader.ReadInt32();
            string[] users = new string[length];

            for (int i = 0; i < length; ++i)
                users[i] = _reader.ReadString();

            return users;
        }

        /// <summary>
        /// Récupère le message de bienvenue envoyé par le serveur.
        /// </summary>
        /// <returns></returns>
        public string ReceiveWelcomeMessage()
        {
            return _reader.ReadString();
        }

        /// <summary>
        /// Déconnecte et ferme proprement la connexion avec le serveur.
        /// </summary>
        public void Disconnect()
        {
            _server.Shutdown(SocketShutdown.Both);
            _server.Close();
        }
    }
}
