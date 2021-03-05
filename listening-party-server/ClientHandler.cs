using System;
using System.Net.Sockets;
using System.Threading;

namespace listening_party_server {

    public class ClientHandler
    {
        bool stop;

        public delegate void ClientEvent(ClientHandler instance, ClientEventArgs e);
        public event ClientEvent MessageReceived;

        public event ClientEvent ConnectionLost;
        public delegate void ClientMessage(ClientHandler handler, byte[] m);

        public TcpClient Socket { get; }
        public string Entity { get; }

        readonly Thread thread;

        public ClientHandler(string entity, TcpClient socket)
        {
            Entity = entity;
            Socket = socket;

            thread = new Thread(StartListening)
            {
                IsBackground = true
            };
            thread.Start();
        }

        public void SendMessage(byte[] packet)
        {
            new Thread(delegate ()
            {
                NetworkStream stream = Socket.GetStream();
                stream.Write(packet, 0, packet.Length);
                stream.Flush();
            }).Start();
        }

        /// <summary>
        /// Starts listening for messages from the user
        /// </summary>
        void StartListening()
        {
            int requestCount = 0;
            requestCount = 0;

            while (!stop)
            {
                try
                {
                    requestCount = requestCount + 1;

                   
                    NetworkStream stream = Socket.GetStream();

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


                    ClientEventArgs e = new ClientEventArgs(pType, isEncrypted, hasMessageAuth, cAlgo, cipherIV, algo, msgMac, data);
                    MessageReceived(this, e);

                }
                catch (Exception)
                {
                    if (!stop)
                    {
                        ConnectionLost(this, null);
                        stop = true;
                    }
                }
            }
        }

        /// <summary>
        /// Closes the socket 
        /// </summary>
        public void Disconnected()
        {
            stop = true;
            Socket.Close();
            Socket.Dispose();
        }
    }

}