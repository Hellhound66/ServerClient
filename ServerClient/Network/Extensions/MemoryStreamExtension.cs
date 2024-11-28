namespace Messages.Extensions;

public static class MemoryStreamExtensions
{
    public static int Crop(this MemoryStream stream, byte[] dest, int size)
    {
        var position = (int)stream.Position;
        var bytesRead = stream.Read(dest, 0, size);
        var bytesToCrop = bytesRead + position;
        var buf = stream.GetBuffer();            
        Buffer.BlockCopy(buf, bytesToCrop, buf, 0, (int)stream.Length - bytesToCrop);
        stream.SetLength(stream.Length - bytesToCrop);
        stream.Position = Math.Min(position, stream.Length);
		
        return bytesRead;
    }
    
    public static async Task<int> CropAsync(this MemoryStream stream, byte[] dest, int size, CancellationToken cancellationToken)
    {
        var position = (int)stream.Position;
        var bytesRead = await stream.ReadAsync(dest.AsMemory(0, size), cancellationToken);
        var bytesToCrop = bytesRead + position;
        var buf = stream.GetBuffer();            
        Buffer.BlockCopy(buf, bytesToCrop, buf, 0, (int)stream.Length - bytesToCrop);
        stream.SetLength(stream.Length - bytesToCrop);
        stream.Position = Math.Min(position, stream.Length);
		
        return bytesRead;
    }
}