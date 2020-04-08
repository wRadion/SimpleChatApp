namespace ChatApp
{
    /// <summary>
    /// Les différents types de message que le serveur peut envoyer.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Un utilisateur a envoyé un message sur le tchat.
        /// </summary>
        CHAT_MESSAGE = 0,

        /// <summary>
        /// Un utilisateur s'est connecté sur le serveur.
        /// </summary>
        USER_JOINED = 1,

        /// <summary>
        /// Un utilisateur s'est déconnecté du serveur.
        /// </summary>
        USER_LEFT = 2
    }
}
