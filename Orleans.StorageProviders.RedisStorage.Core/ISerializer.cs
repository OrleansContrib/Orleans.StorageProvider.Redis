namespace Orleans.StorageProviders
{
    public interface ISerializer
    {
        string Serialize(object data);
        object Deserialize(string data);
    }
}