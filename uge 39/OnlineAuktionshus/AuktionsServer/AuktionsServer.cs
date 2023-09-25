using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AuktionsServer
{
    public class AuktionsServer
    {
        private const int port = 30000;
        private bool Running { get; set; } = true;

        static void Main(string[] args)
        {
            AuktionsServer program = new AuktionsServer();

            program.Run();
        }

        private void Run()
        {
            IPAddress ipa = Tools.GetIPAddress();
            try
            {
                TcpListener listener = new TcpListener(ipa, port);
                listener.Start();
                Console.WriteLine("Server started...");

                while (Running)
                {
                    // Accept a new client connection and create a new thread to handle it
                    TcpClient client = listener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }

                listener.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                Console.WriteLine($"Client connected: {((IPEndPoint)client.Client.RemoteEndPoint).ToString()}");

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] bytes = new byte[1024];
                    int highestBid = 0;
                    string data;
                    Tools.State state = Tools.State.RUNNING;

                    do
                    {
                        int numBytes = stream.Read(bytes, 0, bytes.Length);
                        data = Encoding.ASCII.GetString(bytes, 0, numBytes);
                        data = data.ToUpper();

                        if (!ValidDataSyntax(data))
                        {
                            byte[] errorMessage = Encoding.ASCII.GetBytes("Wrong syntax");
                            stream.Write(errorMessage, 0, errorMessage.Length);
                        }
                        else
                        {
                            char messageType = data[0];
                            string argument = data.Substring(2);
                            argument = argument.Remove(argument.Length - 1);

                            switch (messageType)
                            {
                                case 'B':
                                    highestBid = Bid(stream, argument, highestBid);
                                    break;

                                case 'C':
                                    switch (argument)
                                    {
                                        case "EXIT":
                                            state = Tools.State.EXIT;
                                            byte[] exitMessage = Encoding.ASCII.GetBytes("Bye");
                                            stream.Write(exitMessage, 0, exitMessage.Length);
                                            break;

                                        case "CLOSESERVER":
                                            state = Tools.State.CLOSESERVER;
                                            byte[] closeMessage = Encoding.ASCII.GetBytes("Closing server");
                                            stream.Write(closeMessage, 0, closeMessage.Length);
                                            break;
                                    }
                                    break;
                            }
                        }
                    } while (state == Tools.State.RUNNING);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error handling client: {e.ToString()}");
            }
            finally
            {
                client.Close();
                Console.WriteLine($"Client disconnected: {((IPEndPoint)client.Client.RemoteEndPoint).ToString()}");
            }
        }

        private int Bid(NetworkStream stream, string argument, int highestBid)
        {
            int result = highestBid;

            if (int.TryParse(argument, out var bid))
            {
                if (bid > highestBid)
                {
                    result = bid;
                    byte[] successMessage = Encoding.ASCII.GetBytes("OK");
                    stream.Write(successMessage, 0, successMessage.Length);
                }
                else
                {
                    byte[] lowBidMessage = Encoding.ASCII.GetBytes("Bid is too low");
                    stream.Write(lowBidMessage, 0, lowBidMessage.Length);
                }
            }
            else
            {
                byte[] invalidBidMessage = Encoding.ASCII.GetBytes("Bid is not an integer");
                stream.Write(invalidBidMessage, 0, invalidBidMessage.Length);
            }

            return result;
        }

        private bool ValidDataSyntax(string data)
        {
            bool ok =
                 (data.Length >= 4) &&
                ((data[0] == 'B') || (data[0] == 'C')) &&
                 (data[1] == '@') &&
                 (data[data.Length - 1] == '#');

            return ok;
        }
    }
}