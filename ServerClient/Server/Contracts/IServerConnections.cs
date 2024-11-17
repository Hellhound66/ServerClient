namespace Server.Contracts;

public interface IServerConnections
{
    void Start();
    Task Stop(CancellationToken cancellationToken);
}