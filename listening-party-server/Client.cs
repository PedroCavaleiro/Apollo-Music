using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace listening_party_server {

    public class ClientEventArgs : EventArgs {
        public Int16 PacketType { get; }
        public bool IsEncrypted { get; }
        public bool HasMessageAuth { get; }
        public CipherAlgo CipherAlgorithm { get; }
        public byte[] CipherIV { get; }
        public MessageAuthAlgo MessageAuthenticationAlgorithm { get; }
        public byte[] MessageAuthentication { get; }
        public byte[] Data { get; }
        public bool MessageAuthenticationMatch { get; }

        public ClientEventArgs(Int16 packetType, 
                               bool isEncrypted, 
                               bool hasMessageAuth, 
                               CipherAlgo cipherAlgo, 
                               byte[] cipherIV, 
                               MessageAuthAlgo messageAuthAlgo, 
                               byte[] messageAuthenticationCode, 
                               byte[] data)
        {
            PacketType = packetType;
            IsEncrypted = isEncrypted;
            HasMessageAuth = hasMessageAuth;
            CipherAlgorithm = cipherAlgo;
            CipherIV = cipherIV;
            MessageAuthenticationAlgorithm = messageAuthAlgo;
            MessageAuthentication = messageAuthenticationCode;
            Data = data;
        }

    }

    public class Client
    {

        public bool IsListening { get; set; }

        TcpClient clientSocket;
        Thread cThread;
        NetworkStream serverStream;

        // Client Settings
        public IPAddress ServerIP { get; }
        public int ServerPort { get; }

        // Client Instance info
        public IPAddress LocalIP => ((IPEndPoint)clientSocket.Client.LocalEndPoint).Address;
        public int LocalPort => ((IPEndPoint)clientSocket.Client.LocalEndPoint).Port;

        // Events & Delegates
        public delegate void ClientEvent(Client instance, ClientEventArgs e);
        public event ClientEvent MessageReceived;
        public event ClientEvent ConnectionFailed;
        public event ClientEvent Connected;
        public event ClientEvent ConnectionLost;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ClientLib.Client"/> class.
        /// The connection is not automatically established
        /// To connect to the server <see cref="T:ClientLib.Connect"/> method.
        /// </summary>
        /// <param name="remoteIp">The server ip to connect to.</param>
        /// <param name="remotePort">The server port to connect to.</param>
        /// <param name="requiresMessageAuth">If set to <c>true</c> requires message auth (HMAC or MAC).</param>
        /// <param name="messageAuthAlgo">All messages will be sent using HMAC or MAC, requires mac key.</param>
        /// <param name="messageAuthAlgoKey">Key to create the HMAC or MAC (required only if requiresMessageAuth is enabled).</param>
        /// <exception cref="ClientInitException">This exception is raised when one of the following errors occurs
        /// -> <paramref name="requiresMessageAuth"/> is <c>true</c> but <paramref name="messageAuthAlgo"/> is set to <c>ClientLib.MessageAuthAlgo.NOALGO</c>
        /// -> <paramref name="requiresMessageAuth"/> is <c>true</c> but <paramref name="messageAuthAlgoKey"/> is null
        /// -> <paramref name="remoteIp"/> has an invalid IP format
        /// </exception>
        public Client(string remoteIp, int remotePort)
        {
            bool parseIP = IPAddress.TryParse(remoteIp, out IPAddress serverIP);
            if (!parseIP)
                throw new ClientInitException("Invalid IP Address");

            ServerIP = serverIP;
            ServerPort = remotePort;

        }

        public void Connect(string entity)
        {
            byte[] connectMessage = Encoding.ASCII.GetBytes("attemptconnect_" + entity);
            byte[] connectPacket = BuildPacket(connectMessage);
            try
            {

                clientSocket = new TcpClient();
                clientSocket.Connect(ServerIP, ServerPort);
                serverStream = clientSocket.GetStream();
                serverStream.Write(connectPacket, 0, connectPacket.Length);
                serverStream.Flush();
                IsListening = true;
                cThread = new Thread(GetMessage);
                cThread.Start();
            }
            catch (Exception)
            {
                ConnectionFailed(this, null);
            }
        }

        public void SendRequest(byte[] message)
        {
            serverStream.Write(message, 0, message.Length);
            serverStream.Flush();
        }

        void GetMessage()
        {
            while (IsListening)
            {
                try {
                    serverStream = clientSocket.GetStream();

                    byte[] dataLength = new byte[4];
                    serverStream.Read(dataLength, 0, 4);
                    int dLength = BitConverter.ToInt32(dataLength, 0);

                    byte[] packetType = new byte[2];
                    serverStream.Read(packetType, 0, 2);
                    Int16 pType = BitConverter.ToInt16(packetType, 0);

                    byte[] isEnc = new byte[1];
                    serverStream.Read(isEnc, 0, 1);
                    bool isEncrypted = isEnc[0] != 0b0;

                    byte[] hasMac = new byte[1];
                    serverStream.Read(hasMac, 0, 1);
                    bool hasMessageAuth = isEnc[0] != 0b0;

                    Int16 cipherAlgorithm = 0;
                    byte[] cipherIV = null;
                    if (isEncrypted) {
                        byte[] ciptherAlgo = new byte[2];
                        serverStream.Read(ciptherAlgo, 0, 2);
                        cipherAlgorithm = BitConverter.ToInt16(ciptherAlgo, 0);

                        byte[] cipherIvSize = new byte[4];
                        serverStream.Read(cipherIvSize, 0, 4);
                        cipherIV = new byte[BitConverter.ToInt32(cipherIvSize, 0)];
                        serverStream.Read(cipherIV, 0, BitConverter.ToInt32(cipherIvSize, 0));

                    }

                    CipherAlgo cAlgo = CipherAlgo.NOALGO;
                    if (isEncrypted)
                    {
                        switch (cipherAlgorithm)
                        {
                            case 0:
                                cAlgo = CipherAlgo.AES256;
                                break;
                        }
                    }


                    Int16 macAlgorithm = 0;
                    byte[] msgMac = null;
                    if (hasMessageAuth)
                    {
                        byte[] macAlgo = new byte[2];
                        serverStream.Read(macAlgo, 0, 2);
                        macAlgorithm = BitConverter.ToInt16(macAlgo, 0);

                        byte[] macSize = new byte[4];
                        serverStream.Read(macSize, 0, 4);
                        msgMac = new byte[BitConverter.ToInt32(macSize, 0)];
                        serverStream.Read(msgMac, 0, BitConverter.ToInt32(macSize, 0));

                    }

                    MessageAuthAlgo algo = MessageAuthAlgo.NOALGO;
                    if (hasMessageAuth)
                    {
                        switch (macAlgorithm)
                        {
                            case 0:
                                algo = MessageAuthAlgo.HMACSHA256;
                                break;
                        }
                    }



                    byte[] data = new byte[dLength];
                    serverStream.Read(data, 0, dLength);


                    ClientEventArgs e = new ClientEventArgs(pType, isEncrypted, hasMessageAuth, cAlgo, cipherIV, algo, msgMac, data);

                    if (pType == -1234)
                        Connected(this, e);
                    else
                        MessageReceived(this, e);
                }
                catch (Exception) {
                    IsListening = false;
                    ConnectionLost(this, null);
                }
            }
        }

        /*
         * For a more generic package this is the structure of a package 
         * +-------------+-------------------------------+-------------------+-------------+----------------+-----------+--------------------+--------------------+---------------+--------------+
         * | NON SKIABLE | NON SKIPABLE|   NON SKIPABLE  |    NON SKIPABLE   |  SKIPABLE   |    SKIPABLE    |  SKIPABLE |      SKIPABLE      |      SKIPABLE      |   SKIPABLE    | NON SKIPABLE |
         * +-------------+-------------+-----------------+-------------------+-------------+----------------+-----------+--------------------+--------------------+---------------+--------------+
         * |  DATA SIZE  | PACKET TYPE |  | HAS HMAC/MAC/HASH | CIPHER ALGO | CIPHER IV SIZE | CIPHER IV | HMAC/MAC/HASH ALGO | HMAC/MAC/HASH SIZE | HMAC/MAC/HASH |     DATA     |
         * |    INT32    |    INT16    |        BYTE     |       BYTE        |    INT16    |     INT32      |   BYTE[]  |       INT16        |       INT32        |     BYTE[]    |    BYTE[]    |
         * |   4 BYTES   |   2 BYTES   |       1 BYTE    |      1 BYTE       |   2 BYTES   |     4 BYTES    |  VAR SIZE |      2 BYTES       |      4 BYTES       |    VAR SIZE   |   VAR SIZE   |
         * +-------------+-------------+-----------------+-------------------+-------------+----------------+-----------+--------------------+--------------------+---------------+--------------+
         */

        /// <summary>
        /// Builds the network packet
        /// </summary>
        /// <returns>The packet.</returns>
        /// <param name="message">Serializable object containing data.</param>
        /// <param name="cipherKey">Cipher key.</param>
        /// <param name="cipherIv">Cipher iv.</param>
        /// <param name="cipherAlgo">Cipher algo, to enable encryption change CipherAlgo to other algorithm.</param>
        /// <param name="messageAuthAlgoKey">Key to generate HMAC and MAC.</param>
        /// <param name="messageAuthAlgo">Algorithm used to create the HMAC or MAC to disable the MessageAuth use MessageAuthAlgo.NOALGO.</param>
        /// <param name="type">Type.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static byte[] BuildPacket<T>(T message, 
                                            byte[] cipherKey = null,
                                            byte[] cipherIv = null,
                                            CipherAlgo cipherAlgo = CipherAlgo.NOALGO,
                                            byte[] messageAuthAlgoKey = null, 
                                            MessageAuthAlgo messageAuthAlgo = MessageAuthAlgo.NOALGO, 
                                            Int16 type = -1)
        {

            if (messageAuthAlgo != MessageAuthAlgo.NOALGO)
                if (messageAuthAlgoKey == null)
                    throw new PacketGenerationException("Missing MAC/HMAC Key");
            if (cipherAlgo != CipherAlgo.NOALGO)
                if (cipherKey == null)
                    throw new PacketGenerationException("Missing encryption key");
            

            string json = JsonConvert.SerializeObject(message);

            // packet information
            byte[] packetType = BitConverter.GetBytes(type);
            byte[] isEncrypted = new byte[1];
            isEncrypted[0] = cipherAlgo != CipherAlgo.NOALGO ? (byte)0b1 : (byte)0b0;
            byte[] hasAuth = new byte[1];
            hasAuth[0] = messageAuthAlgo != MessageAuthAlgo.NOALGO ? (byte)0b1 : (byte)0b0;
            byte[] macAlgo = new byte[2];
            byte[] encAlgo = new byte[2];

            //packet data
            Int16 cipherAlgoId = 0;
            byte[] data =(cipherAlgo != CipherAlgo.NOALGO) ? BuildEncryptedMessage(Encoding.ASCII.GetBytes(json), cipherKey, cipherAlgo, out cipherAlgoId, cipherIv) : Encoding.ASCII.GetBytes(json);
            Int16 msgAuthAlgo = 0;
            byte[] mac = (messageAuthAlgo != MessageAuthAlgo.HMACSHA256) ? BuildMessageAuthAlgo(data, messageAuthAlgo, messageAuthAlgoKey, out msgAuthAlgo) : null;

            // update packet information
            if (messageAuthAlgo != MessageAuthAlgo.NOALGO)
                macAlgo = BitConverter.GetBytes(msgAuthAlgo);
            if (cipherAlgo != CipherAlgo.NOALGO)
                encAlgo = BitConverter.GetBytes(cipherAlgoId);

            // packet size calculation
            int packetSize = 4;
            if (cipherAlgo != CipherAlgo.NOALGO) {
                packetSize += 6;
                packetSize += cipherIv != null ? cipherIv.Length : 0;
            }
            if (messageAuthAlgo != MessageAuthAlgo.NOALGO) {
                packetSize += 6;
                packetSize += mac.Length;
            }
            packetSize += data.Length;

            byte[] size = BitConverter.GetBytes(data.Length);
            byte[] packet = new byte[packetSize + 4];

            // merge all information to one array
            Array.Copy(size, packet, 4);
            Array.Copy(packetType, 0, packet, 4, 2);
            Array.Copy(isEncrypted, 0, packet, 6, 1);
            Array.Copy(hasAuth, 0, packet, 7, 1);

            if (cipherAlgo != CipherAlgo.NOALGO) {
                int civl = cipherIv != null ? cipherIv.Length : 0;

                Array.Copy(encAlgo, 0, packet, 8, 2);
                Array.Copy(BitConverter.GetBytes(civl), 0, packet, 10, 4);
                if (cipherIv != null)
                    Array.Copy(cipherIv, 0, packet, 14, cipherIv.Length);

                // if it also contains Message Auth
                if (messageAuthAlgo != MessageAuthAlgo.NOALGO)
                {
                    Array.Copy(macAlgo, 0, packet, 14 + civl, 2);
                    Array.Copy(BitConverter.GetBytes(mac.Length), 0, packet, 16 + civl, 4);
                    Array.Copy(mac, 0, packet, 20 + civl, mac.Length);
                    Array.Copy(data, 0, packet, 20 + civl + mac.Length, data.Length);
                }
                else
                {
                    Array.Copy(data, 0, packet, 14 + civl, data.Length);
                }
            } else {
                if (messageAuthAlgo != MessageAuthAlgo.NOALGO)
                {
                    Array.Copy(macAlgo, 0, packet, 8, 2);
                    Array.Copy(BitConverter.GetBytes(mac.Length), 0, packet, 10, 4);
                    Array.Copy(mac, 0, packet, 14, mac.Length);
                    Array.Copy(data, 0, packet, 14 + mac.Length, data.Length);
                }
                else
                {
                    Array.Copy(data, 0, packet, 8, data.Length);
                }
            }

            return packet;
        }

        /// <summary>
        /// Builds the network packet
        /// </summary>
        /// <returns>The packet.</returns>
        /// <param name="message">Byte array that contains the data.</param>
        /// <param name="cipherKey">Cipher key.</param>
        /// <param name="cipherIv">Cipher iv.</param>
        /// <param name="cipherAlgo">Cipher algo, to enable encryption change CipherAlgo to other algorithm.</param>
        /// <param name="messageAuthAlgoKey">Key to generate HMAC and MAC.</param>
        /// <param name="messageAuthAlgo">Algorithm used to create the HMAC or MAC to disable the MessageAuth use MessageAuthAlgo.NOALGO.</param>
        /// <param name="type">Type.</param>
        public static byte[] BuildPacket(byte[] message,
                                            byte[] cipherKey = null,
                                            byte[] cipherIv = null,
                                            CipherAlgo cipherAlgo = CipherAlgo.NOALGO,
                                            byte[] messageAuthAlgoKey = null,
                                            MessageAuthAlgo messageAuthAlgo = MessageAuthAlgo.NOALGO,
                                            Int16 type = -1)
        {

            if (messageAuthAlgo != MessageAuthAlgo.NOALGO)
                if (messageAuthAlgoKey == null)
                    throw new PacketGenerationException("Missing MAC/HMAC Key");
            if (cipherAlgo != CipherAlgo.NOALGO)
                if (cipherKey == null)
                    throw new PacketGenerationException("Missing encryption key");


            // packet information
            byte[] packetType = BitConverter.GetBytes(type);
            byte[] isEncrypted = new byte[1];
            isEncrypted[0] = cipherAlgo != CipherAlgo.NOALGO ? (byte)0b1 : (byte)0b0;
            byte[] hasAuth = new byte[1];
            hasAuth[0] = messageAuthAlgo != MessageAuthAlgo.NOALGO ? (byte)0b1 : (byte)0b0;
            byte[] macAlgo = new byte[2];
            byte[] encAlgo = new byte[2];

            //packet data
            Int16 cipherAlgoId = 0;
            byte[] data = (cipherAlgo != CipherAlgo.NOALGO) ? BuildEncryptedMessage(message, cipherKey, cipherAlgo, out cipherAlgoId, cipherIv) : message;
            Int16 msgAuthAlgo = 0;
            byte[] mac = (messageAuthAlgo != MessageAuthAlgo.HMACSHA256) ? BuildMessageAuthAlgo(data, messageAuthAlgo, messageAuthAlgoKey, out msgAuthAlgo) : null;

            // update packet information
            if (messageAuthAlgo != MessageAuthAlgo.NOALGO)
                macAlgo = BitConverter.GetBytes(msgAuthAlgo);
            if (cipherAlgo != CipherAlgo.NOALGO)
                encAlgo = BitConverter.GetBytes(cipherAlgoId);

            // packet size calculation
            int packetSize = 4;
            if (cipherAlgo != CipherAlgo.NOALGO)
            {
                packetSize += 6;
                packetSize += cipherIv != null ? cipherIv.Length : 0;
            }
            if (messageAuthAlgo != MessageAuthAlgo.NOALGO)
            {
                packetSize += 6;
                packetSize += mac.Length;
            }
            packetSize += data.Length;

            byte[] size = BitConverter.GetBytes(data.Length);
            byte[] packet = new byte[packetSize + 4];

            // merge all information to one array
            Array.Copy(size, packet, 4);
            Array.Copy(packetType, 0, packet, 4, 2);
            Array.Copy(isEncrypted, 0, packet, 6, 1);
            Array.Copy(hasAuth, 0, packet, 7, 1);

            if (cipherAlgo != CipherAlgo.NOALGO)
            {
                int civl = cipherIv != null ? cipherIv.Length : 0;

                Array.Copy(encAlgo, 0, packet, 8, 2);
                Array.Copy(BitConverter.GetBytes(civl), 0, packet, 10, 4);
                if (cipherIv != null)
                    Array.Copy(cipherIv, 0, packet, 14, cipherIv.Length);

                // if it also contains Message Auth
                if (messageAuthAlgo != MessageAuthAlgo.NOALGO)
                {
                    Array.Copy(macAlgo, 0, packet, 14 + civl, 2);
                    Array.Copy(BitConverter.GetBytes(mac.Length), 0, packet, 16 + civl, 4);
                    Array.Copy(mac, 0, packet, 20 + civl, mac.Length);
                    Array.Copy(data, 0, packet, 20 + civl + mac.Length, data.Length);
                }
                else
                {
                    Array.Copy(data, 0, packet, 14 + civl, data.Length);
                }
            }
            else
            {
                if (messageAuthAlgo != MessageAuthAlgo.NOALGO)
                {
                    Array.Copy(macAlgo, 0, packet, 8, 2);
                    Array.Copy(BitConverter.GetBytes(mac.Length), 0, packet, 10, 4);
                    Array.Copy(mac, 0, packet, 14, mac.Length);
                    Array.Copy(data, 0, packet, 14 + mac.Length, data.Length);
                }
                else
                {
                    Array.Copy(data, 0, packet, 8, data.Length);
                }
            }

            return packet;
        }

        /// <summary>
        /// Builds the message HMAC or MAC
        /// </summary>
        /// <returns>HMAC or MAC.</returns>
        /// <param name="data">Data.</param>
        /// <param name="messageAuthAlg">Algorithm to create the HMAC or MAC.</param>
        /// <param name="key">Key to create the HMAC or MAC.</param>
        /// <param name="algo">Algorithm ID.</param>
        static byte[] BuildMessageAuthAlgo(byte[] data, MessageAuthAlgo messageAuthAlg, byte[] key, out Int16 algo) {
            switch (messageAuthAlg) {
                case MessageAuthAlgo.HMACSHA256:
                    algo = 1;
                    return SHA256hmac.ComputeHMAC(data, key);
                case MessageAuthAlgo.NOALGO:
                    algo = -1;
                    return null;
                default:
                    throw new PacketGenerationException("Message Auth Algorithm not implemented");
            }
        }

        /// <summary>
        /// Builds the encrypted message.
        /// </summary>
        /// <returns>The encrypted message.</returns>
        /// <param name="data">Data.</param>
        /// <param name="key">Key.</param>
        /// <param name="cipherAlgo">Algorithm used to encrypt.</param>
        /// <param name="algo">Algorithm ID.</param>
        /// <param name="iv">Iv.</param>
        static byte[] BuildEncryptedMessage(byte[] data, byte[] key, CipherAlgo cipherAlgo, out Int16 algo, byte[] iv = null) {
            switch (cipherAlgo) {
                case CipherAlgo.AES256:
                    algo = 1;
                    if (iv == null)
                        throw new PacketGenerationException("Invalid IV for AES256");
                    return AESCipher.EncryptData(data, key, iv);
                case CipherAlgo.NOALGO:
                    algo = -1;
                    return null;
                default:
                    throw new PacketGenerationException("Cipher Algorithm not implemented");
            }
        }

       

    }

    public class ClientInitException: Exception {
        public ClientInitException() { }
        public ClientInitException(string message) : base(message) { }
        public ClientInitException(string message, Exception inner) : base(message, inner) { }
    }

    public class PacketGenerationException: Exception {
        public PacketGenerationException() { }
        public PacketGenerationException(string message) : base(message) { }
        public PacketGenerationException(string message, Exception inner) : base(message, inner) { }
    }
}