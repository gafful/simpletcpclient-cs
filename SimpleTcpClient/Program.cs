// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SimpleHttpClient;

internal class Program
{
    private static void Main(string[] args)
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
}
