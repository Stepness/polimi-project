namespace PolimiProject.Models;

public class BlobEntity
{
    public string Id { get; set; }
    public byte[] Data { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
}