using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Server.Extensions;

public static class TcpClientExtension
{
    public static TcpState GetState(this TcpClient tcpClient)
    {
        var foo = IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpConnections()
            .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
        return foo?.State ?? TcpState.Unknown;
    }
}