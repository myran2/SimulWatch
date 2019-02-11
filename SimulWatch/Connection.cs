using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimulWatch
{
    class Connection
    {
        private Socket sock;
        private IPEndPoint endpoint;
        private bool active;

        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        public Connection(string ipAddr, int port)
        {
            // parse IP from string and setup endpoint (combination of port and ip)
            IPAddress ip = IPAddress.Parse(ipAddr);
            endpoint = new IPEndPoint(ip, port);
            active = false;

            // create TCP socket
            sock = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // initial connection to endpoint
            sock.BeginConnect(endpoint, new AsyncCallback(ConnectCallback), sock);

            // wait here until we get the callback
            connectDone.WaitOne();

            // connection is active, so set that flag
            active = true;
        }

        public bool Active()
        {
            return active;
        }

        public void Close()
        {
            active = false;
            sock.Shutdown(SocketShutdown.Send);
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
        }

        public void Send(string data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(data);

            // start sending data to the server
            sock.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), sock);
        }

        private void SendCallback(IAsyncResult res)
        {

        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the server
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // Store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    
                    // All messages will end with a newline, so we're done recieving if the data ends with \n
                    if (state.sb.ToString().EndsWith("\n"))
                    {
                        // parse the command we just got
                        CommandHandler.ParseCommand(state.sb.ToString());
                        state.sb.Clear();
                    }

                    //  Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // No more data coming in so we can stop trying to receive data.
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void RecieveData(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);

                // Wait until we are done recieving data
                receiveDone.WaitOne();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult res)
        {
            Socket client = (Socket)res.AsyncState;
            client.EndConnect(res);

            System.Windows.Forms.MessageBox.Show(String.Format("Socket connected to {0}", client.RemoteEndPoint.ToString()));
            connectDone.Set();

            RecieveData(client);
        }
    }
}
