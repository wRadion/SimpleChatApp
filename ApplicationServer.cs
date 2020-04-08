using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace ChatApp
{
    /// <summary>
    /// <para>Représente l'objet qui permet de créer et lancer un serveur en arrière-plan.</para>
    /// <para>La classe implémente le design pattern "Singleton".</para>
    /// <para><see href="https://fr.wikipedia.org/wiki/Singleton_(patron_de_conception)"/></para>
    /// </summary>
    public class ApplicationServer
    {
        #region Singleton
        private static ApplicationServer Instance = null;
        public static ApplicationServer Get()
        {
            if (Instance == null)
                Instance = new ApplicationServer();
            return Instance;
        }
        #endregion

        private readonly IPAddress _localIPAddress;
        private readonly Socket _server;
        private readonly List<ChatClient> _clients;

        // Le constructeur de la classe est privé car on utilise un Singleton.
        private ApplicationServer()
        {
            _localIPAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(
                (ip) => ip.AddressFamily == AddressFamily.InterNetwork
             );
            _server = new Socket(_localIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _clients = new List<ChatClient>();
        }

        /// <summary>
        /// Démarrer le serveur en écoutant sur le port donné.
        /// </summary>
        /// <param name="port">Le port sur lequel le serveur écoute</param>
        public void Start(int port)
        {
            _server.Bind(new IPEndPoint(_localIPAddress, port));
            _server.Listen(10);
        }

        /// <summary>
        /// <para>Se met en attente de connexion d'un nouvel utilisateur.</para>
        /// <para>Le code de cette fonction n'est pas exécuté sur le thread principal.</para>
        /// </summary>
        public void StartAccepting()
        {
            // Task permet d'exécuter du code sur un autre thread pour éviter de bloquer l'application.
            Task.Run(() =>
            {
                while (true)
                {
                    // On se met en attente d'une nouvelle connexion
                    // Cette ligne est bloquante pour le programme, c'est pourquoi on l'exécute dans un autre thread.
                    Socket client = _server.Accept();
                    // On initialise tout les objets permettant d'envoyer/de recevoir des messages
                    NetworkStream stream = new NetworkStream(client);
                    BinaryWriter writer = new BinaryWriter(stream);
                    BinaryReader reader = new BinaryReader(stream);

                    // On récupère le pseudo de l'utilisateur qui vient de se connecté
                    string username = reader.ReadString();

                    // On vérifie que le pseudo n'est pas déjà pris par un autre utilisateur connecté sur le serveur
                    bool isUsernameTaken = _clients.Any((c) => c.Username == username);

                    // On envoie - True si le pseudo n'est pas pris (la connexion est possible)
                    //           - False si le pseudo est déjà pris (la connexion n'est pas possible)
                    writer.Write(!isUsernameTaken);

                    // Si le pseudo n'est pas pris (la connexion est validée)
                    if (!isUsernameTaken)
                    {
                        // On envoie à tout les utilisateurs connectés l'information/le message qui indique qu'un utilisateur vient de se connecter
                        SendMessage(MessageType.USER_JOINED, username);

                        // On créer le nouvel utilisateur (client) du tchat
                        ChatClient chatClient = new ChatClient(username, client, writer, reader);
                        // On l'ajoute dans la liste des utilisateurs connectés
                        _clients.Add(chatClient);

                        // On se met en écoute des messages que peut envoyer l'utilisateur (client)
                        StartListening(chatClient);
                        // On envoie à l'utilisateur (client) la liste des utilisateurs connectés (y compris lui-même)
                        SendUserList(writer);
                        // On envoie à l'utilisateur (client) un message de bienvenue
                        SendWelcomeMessage(writer, username);
                    }
                }
            });
        }

        /// <summary>
        /// <para>Ecoute les messages qu'envoie le client donné.</para>
        /// <para>Le code de cette fonction n'est pas exécuté sur le thread principal.</para>
        /// </summary>
        /// <param name="client">Le client qu'il faut écouter</param>
        public void StartListening(ChatClient client)
        {
            // Task permet d'exécuter du code sur un autre thread pour éviter de bloquer l'application.
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        // On attends que le client envoie un message (objet) (ici, de type string)
                        // Cette ligne est bloquante pour le programme, c'est pourquoi on l'exécute dans un autre thread.
                        string message = client.Reader.ReadString();
                        // On envoie le message que vient d'envoyer le client à tout les clients (y compris celui qui a envoyé le message)
                        SendMessage(MessageType.CHAT_MESSAGE, client.Username, message);
                    }
                }
                // On gère l'erreur qui se produit dans le cas où le client ne réponds plus :
                //      - soit y'a un problème de connexion entre le client et le serveur
                //      - soit le client a été fermé/déconnecté volontairement
                catch (IOException)
                {
                    // On déconnecte proprement la liaison du client vers le serveur
                    client.Disconnect();
                    // On retire l'utilisateur (client) de la liste des utilisateurs connectés
                    _clients.Remove(client);
                    // On envoie l'information aux autres utilisateurs (clients) que l'utilisateur (client) s'est déconnecté du serveur
                    SendMessage(MessageType.USER_LEFT, client.Username);
                }
            });
        }

        /// <summary>
        /// Envoi un message "typé" à tous les utilisateurs (clients) connectés.
        /// </summary>
        /// <param name="type">Le type du message à envoyer</param>
        /// <param name="args">Les arguments du message (le nombre varie selon le type)</param>
        public void SendMessage(MessageType type, params string[] args)
        {
            foreach (ChatClient client in _clients)
                client.SendMessage(type, args);
        }

        /// <summary>
        /// Envoi la liste des utilisateurs connectés au BinaryWriter donné.
        /// </summary>
        /// <param name="writer">Le BinaryWriter de la socket d'un client (utilisateur)</param>
        public void SendUserList(BinaryWriter writer)
        {
            string[] users = _clients.Select((c) => c.Username).ToArray();

            writer.Write(users.Length);

            foreach (string user in users)
                writer.Write(user);
        }

        /// <summary>
        /// Envoi le message de bienvenue au BinaryWriter donné.
        /// </summary>
        /// <param name="writer">Le BinaryWriter de la socket d'un client (utilisateur)</param>
        /// <param name="username">Le pseudo de l'utilisateur à accueilir</param>
        public void SendWelcomeMessage(BinaryWriter writer, string username)
        {
            writer.Write($"Bienvenue sur le server, { username } !");
        }

        /// <summary>
        /// Déconnecte tous les clients connectés proprement et ferme le serveur.
        /// </summary>
        public void Stop()
        {
            // Si le serveur n'est pas lancé (quand on est en mode Client uniquement), on ne fait rien (du coup)
            if (!_server.Connected) return;

            foreach (ChatClient client in _clients)
                client.Disconnect();

            _server.Close();
        }
    }
}
