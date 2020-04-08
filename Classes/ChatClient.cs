using System.IO;
using System.Net.Sockets;

namespace ChatApp
{
    /// <summary>
    /// Représente un client vu par le serveur.
    /// </summary>
    public class ChatClient
    {
        public string Username;
        private readonly Socket _client;
        public readonly BinaryWriter Writer;
        public readonly BinaryReader Reader;

        /// <summary>
        /// Initialise un utilisateur (client) du tchat.
        /// </summary>
        /// <param name="username">Le pseudo de l'utilisateur</param>
        /// <param name="client">La socket récupérée via <c>_server.Accept()</c></param>
        /// <param name="writer">Le BinaryWriter (l'objet qui permet d'envoyer des messages au serveur) associé à la socket</param>
        /// <param name="reader">Le BinaryReader (l'objet qui permet de recevoir des messages du serveur) associé à la socket</param>
        public ChatClient(string username, Socket client, BinaryWriter writer, BinaryReader reader)
        {
            Username = username;
            _client = client;
            Writer = writer;
            Reader = reader;
        }

        /// <summary>
        /// Envoie un message "typé" à l'utilisateur (client).
        /// </summary>
        /// <param name="type">Le type de message</param>
        /// <param name="args">Les arguments nécessaires pour envoyer le message</param>
        public void SendMessage(MessageType type, params string[] args)
        {
            Writer.Write((int)type);
            foreach (string arg in args)
                Writer.Write(arg);
        }

        /// <summary>
        /// Déconnecte et ferme proprement la connexion avec l'utilisateur (client).
        /// </summary>
        public void Disconnect()
        {
            Reader.Close();
            Writer.Close();
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }
    }
}
