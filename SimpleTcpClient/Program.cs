// See https://aka.ms/new-console-template for more information

using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace SimpleHttpClient;

internal class Program
{
    private static void Main(string[] args)
    {
        // SimpleGet();
        SimplePostAndGetOverHttps();
    }

    private static void SimpleGet()
    {
        using var client = new TcpClient();

        var hostname = "example.com";
        client.Connect(hostname, 80);

        using var networkStream = client.GetStream();
        networkStream.ReadTimeout = 2000;

        using var writer = new StreamWriter(networkStream);

        var message = @"GET / HTTP/1.1
Accept: text/html, charset=utf-8
Accept-Language: en-US
User-Agent: C# program
Connection: close
Host: example.com" + "\r\n\r\n";

        Console.WriteLine(message);

        using var reader = new StreamReader(networkStream, Encoding.UTF8);
        var bytes = Encoding.UTF8.GetBytes(message);

        networkStream.Write(bytes, 0, bytes.Length);
        Console.WriteLine(reader.ReadToEnd());
    }

    private static void SimplePostAndGetOverHttps()
    {
        var tcpClient = new TcpClient("example.com", 443);
        var sslStream = new SslStream(tcpClient.GetStream());

        // Initiate server authentication
        sslStream.AuthenticateAsClient("example.com");

        // Encode a test message into a byte array.
        // Signal the end of the message using the "<EOF>".
        var message = @"GET / HTTP/1.1
Accept: text/html, charset=utf-8
Accept-Language: en-US
User-Agent: C# program
Connection: close
Host: example.com

<EOF>";
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        // Send message to the server.
        sslStream.Write(messageBytes);
        sslStream.Flush();
        
        // Read message from the server.
        var serverMessage = ReadMessage(sslStream);
        Console.WriteLine("Response for POST request: {0}", serverMessage);

        
        // Send message to the server.
        sslStream.Write(messageBytes);
        sslStream.Flush();
        
        // Read message from the server.
        var serverMessage2 = ReadMessage(sslStream);
        Console.WriteLine("Response for GET request: {0}", serverMessage2);
        
        
        // Close the client connection.
        tcpClient.Close();
        Console.WriteLine("Client closed.");
    }

    private static string ReadMessage(SslStream sslStream)
    {
        // Read the  message sent by the server.
        // The end of the message is signaled using the
        // "<EOF>" marker.
        var buffer = new byte[2048];
        var messageData = new StringBuilder();
        var bytes = -1;
        do
        {
            bytes = sslStream.Read(buffer, 0, buffer.Length);

            // Use Decoder class to convert from bytes to UTF8
            // in case a character spans two buffers.
            var decoder = Encoding.UTF8.GetDecoder();
            var chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
            decoder.GetChars(buffer, 0, bytes, chars, 0);
            messageData.Append(chars);
            // Check for EOF.
            if (messageData.ToString().IndexOf("<EOF>") != -1) break;
        } while (bytes != 0);

        return messageData.ToString();
    }
}