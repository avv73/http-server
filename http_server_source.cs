using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Globalization;

public class HTTPServer
{  
    // Listening port, may be changed, as long as it's not used by another service or program
    private const int Port = 8081;

    public static void Main()
    {
        // Start tcp listener for incoming HTTP GET's on the local network
        TcpListener tcpResponseListener = new TcpListener(Port);

        Console.WriteLine($"Listening on port {Port}...");
        tcpResponseListener.Start();
        while (true)
        {
            // Set socket to the accepted response, buffer size may be reduced to an optimal count
            byte[] buffer = new byte[460];
            Socket acceptedRequest = tcpResponseListener.Server.Accept();

            // Save the response to the buffer, then convert it to string and print the GET request on the console
            int receivedBytes = acceptedRequest.Receive(buffer);
            string getRequest = System.Text.Encoding.UTF8.GetString(buffer).TrimEnd('\0');
            Console.WriteLine(getRequest);

            // Match the requested file in the saved GET request
            string pattern = @"^GET (.*?) HTTP";
            string requestedFile = Regex.Match(getRequest, pattern).Groups[1].Value;
            string serverResponse = string.Empty;

            // Compose a server response depending on the requested file, requested file will be searched in the "server" directory,
            // if requested file is not present, compose error response
            // favicion.ico will be requested by most browsers, so it's not an error case, the server won't respond to this kind of GET,
            // may implement a website icon support in the future
            // See the server folder for more info
            if (requestedFile == "/" || requestedFile == "/index.html" || requestedFile == "/index.htm")
            {
                string readedResponseFile = File.ReadAllText(@"server\index.html");
                serverResponse =
                    @"HTTP/1.1 200 OK
Connection: close

" + readedResponseFile;
            }

            else if (requestedFile == "/info")
            {
                string readedResponseFile = File.ReadAllText(@"server\info.html");
                serverResponse =
                    @"HTTP/1.1 200 OK
Connection: close

" + string.Format(readedResponseFile, DateTime.Now.Date.ToString("d", CultureInfo.InvariantCulture) + " " + DateTime.Now.TimeOfDay.ToString("c"), Environment.ProcessorCount);
            }
            else if (requestedFile != "/favicion.ico")
            {
                string readedResponseFile = File.ReadAllText(@"server\error.html");
                serverResponse =
                    @"HTTP/1.1 200 OK
Connection: close

" + readedResponseFile;
            }

            // Send the composed HTTP server response back to the socket in byte array format
            using (NetworkStream responseStream = new NetworkStream(acceptedRequest, false))
            {
                byte[] toBytes = System.Text.Encoding.ASCII.GetBytes(serverResponse);
                responseStream.Write(toBytes, 0, toBytes.Length);
            }

            // Interrupt connection to the current socket
            acceptedRequest.Close();
        }
    }
}
