using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using Mono.Options;

namespace tcp_connect
{
    class Program
    {
        private const int CONN_SUCCESSFUL = 1;
        private const int CONN_TIMEOUT = 2;
        private const int CONN_ERROR = 3;

        static void Main(string[] args)
        {
            bool help_requested = false;
            string file_path = @"C:\Users\Jonas\Documents\visual studio 2015\Projects\tcp_connect\tcp_connect\ips.txt";
            string single_host = "";
            int default_port = 80;
            int timeout = 2;
            List<Array> hosts_to_check = new List<Array>();

            var parameter = new OptionSet() {
                { "f|file=", "Path to the file of hosts, default: ips.txt", v => file_path = v },
                { "h|host=", "Single host to connect to. 'file' argument will be ignored", v => single_host = v },
                { "p|port=", "Default port, if none is available in the text file, default: 80", v => default_port = int.Parse(v) },
                { "t|timeout=", "Time to connect to host, default: 2", v => timeout = int.Parse(v) },
                { "help",  "show this message and exit", v => help_requested = v != null },
            };
            
            try
            {
                List<string> extra = parameter.Parse(args);
            }
            catch (Mono.Options.OptionException)
            {
                show_help(parameter);
            }


            if (help_requested)
            {
                show_help(parameter);
            }


            string[] lines = new string[] {};
            
            if (single_host.Trim() == "")
            {
                try {
                    lines = File.ReadAllLines(file_path);
                }
                catch (System.IO.FileNotFoundException) {
                    Console.WriteLine("Configuration file '{0}' not found", file_path);
                    System.Environment.Exit(1);
                }

                string[] splitted_line;

                foreach (string line in lines)
                {
                    if (line.Trim() == "")
                    {
                        continue;
                    }

                    splitted_line = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    hosts_to_check.Add(splitted_line);

                }
            }
            else
            {
                string[] line = { single_host, default_port.ToString() };
                hosts_to_check.Add(line);
            }


            int used_port = 0;
            foreach (string[] host in hosts_to_check)
            {
                if (host.Length >= 2)
                {
                    used_port = int.Parse(host[1]);
                }
                else
                {
                    used_port = default_port;
                }

                #region connect
                int connection_state = 0;

                Console.Write("Connecting to '{0}' on port {1}: ", host[0], used_port);
                connection_state = tcp_connect(host[0], used_port, timeout * 1000);
                switch (connection_state)
                {
                    case CONN_SUCCESSFUL:
                        Console.WriteLine("Successful!");
                        break;
                    case CONN_TIMEOUT:
                        Console.WriteLine("Timed out!");
                        break;
                    case CONN_ERROR:
                        Console.WriteLine("Socket error, host not found?");
                        break;
                }
                #endregion
            }



            Console.ReadLine();
        }

        static int tcp_connect(string client_address, int client_port, int timeout)
        {
            TcpClient tcp_client = new TcpClient();
            bool connect_success = false;
            int connection_state = 0;

            try
            {
                connect_success = tcp_client.ConnectAsync(client_address, client_port).Wait(timeout);
                if (connect_success)
                {
                    connection_state = CONN_SUCCESSFUL;
                }
                else
                {
                    connection_state = CONN_TIMEOUT;
                    
                }
            }
            catch (SocketException)
            {
                connection_state = CONN_ERROR;
            }
            catch (AggregateException)
            {
                connection_state = CONN_ERROR;
            }
            finally
            {
                tcp_client.Close();
            }
            return connection_state;
        }

        static void show_help(OptionSet parameter)
        {
            Console.WriteLine("Usage: tcp_connect [OPTIONS]");
            Console.WriteLine("Trys to connect to the hosts specified in a text file");
            Console.WriteLine();
            Console.WriteLine("Options:");
            parameter.WriteOptionDescriptions(Console.Out);
            System.Environment.Exit(1);
        }
    }
}
