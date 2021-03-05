using System.Net;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;
using listening_party_server.Models;
using System.Collections.Generic;

namespace listening_party_server {

    public class Server {

        #region "Variables"

        Thread thread;
        bool stop;

        public EventArgs e;

        public Dictionary<string, ClientHandler> clientHandlers = new Dictionary<string, ClientHandler>();
        public IPAddress ListeningIP { get; }
        public int ListeningPort { get; }
        public string Entity { get; }

        #endregion

        #region "Events"

        public event ClientCommunicationHandler MessageReceived;
        public event ClientCommunicationHandler ConnectionLost;
        public event ClientConnectionHandler ClientConnected;

        #endregion

        #region "Delegates"

        public delegate void ClientCommunicationHandler(byte[] m, ClientHandler clientHandler, ClientEventArgs e);
        public delegate void ClientConnectionHandler(byte[] m, TcpClient socket, EventArgs e);

        #endregion

        public Server(IPAddress ip, int port, string entity)
        {
            Entity = entity;
            ListeningIP = ip;
            ListeningPort = port;
            thread = new Thread(() => StartListening(ip, port));
            thread.Start();
        }

        public void Shutdown()
        {
            stop = true;
        }

        /// <summary>
        /// Starts listening for incoming connections
        /// </summary>
        /// <param name="ip">Ip.</param>
        /// <param name="port">Port.</param>
        void StartListening(IPAddress ip, int port)
        {
            TcpListener serverSocket = new TcpListener(ip, port);
            TcpClient clientSocket = default(TcpClient);

            int counter = 0;
            serverSocket.Start();

            while (!stop)
            {
                if (serverSocket.Pending())
                {
                    counter += 1;
                    clientSocket = serverSocket.AcceptTcpClient();


                    NetworkStream stream = clientSocket.GetStream();

                    byte[] dataLength = new byte[4];
                    stream.Read(dataLength, 0, 4);
                    int dLength = BitConverter.ToInt32(dataLength, 0);

                    byte[] packetType = new byte[2];
                    stream.Read(packetType, 0, 2);
                    Int16 pType = BitConverter.ToInt16(packetType, 0);

                    byte[] isEnc = new byte[1];
                    stream.Read(isEnc, 0, 1);
                    bool isEncrypted = isEnc[0] != 0b0;

                    byte[] hasMac = new byte[1];
                    stream.Read(hasMac, 0, 1);
                    bool hasMessageAuth = isEnc[0] != 0b0;

                    Int16 cipherAlgorithm = 0;
                    byte[] cipherIV = null;
                    if (isEncrypted)
                    {
                        byte[] ciptherAlgo = new byte[2];
                        stream.Read(ciptherAlgo, 0, 2);
                        cipherAlgorithm = BitConverter.ToInt16(ciptherAlgo, 0);

                        byte[] cipherIvSize = new byte[4];
                        stream.Read(cipherIvSize, 0, 4);
                        cipherIV = new byte[BitConverter.ToInt32(cipherIvSize, 0)];
                        stream.Read(cipherIV, 0, BitConverter.ToInt32(cipherIvSize, 0));

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
                        stream.Read(macAlgo, 0, 2);
                        macAlgorithm = BitConverter.ToInt16(macAlgo, 0);

                        byte[] macSize = new byte[4];
                        stream.Read(macSize, 0, 4);
                        msgMac = new byte[BitConverter.ToInt32(macSize, 0)];
                        stream.Read(msgMac, 0, BitConverter.ToInt32(macSize, 0));

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
                    stream.Read(data, 0, dLength);

                    ClientEventArgs _e = new ClientEventArgs(pType, isEncrypted, hasMessageAuth, cAlgo, cipherIV, algo, msgMac, data);

                    ClientConnected(data, clientSocket, _e);

                }
            }

            /*BroadcastShutdownToClients();
            foreach (DictionaryEntry item in clientHandlers)
                ((ClientHandler)item.Value).Disconnect();
            if (clientSocket != null)
                clientSocket.Close();
            serverSocket.Stop();*/
        }

        public void AcceptClient(string entity, TcpClient socket)
        {
            byte[] accptedMessage = Client.BuildPacket(Encoding.ASCII.GetBytes("connected_" + Entity), type: -1234);
            SendMessage(accptedMessage, socket);
            ClientHandler client = new ClientHandler(entity, socket);
            client.MessageReceived += Client_MessageReceived;
            client.ConnectionLost += Client_ConnectionLost;
            clientHandlers.Add(entity, client);
        }

        private void Client_ConnectionLost(ClientHandler instance, ClientEventArgs e)
        {
            ConnectionLost(null, instance, e);
        }

        void SendMessage(byte[] packet, TcpClient socket)
        {
            new Thread(delegate ()
            {
                NetworkStream stream = socket.GetStream();
                stream.Write(packet, 0, packet.Length);
                stream.Flush();
            }).Start();
        }

        private void Client_MessageReceived(ClientHandler instance, ClientEventArgs e)
        {
            MessageReceived(null, instance, e);
        }
    }

}