class CustomerById : CustomerByKey
{
    public CustomerById(int id, string ecv, long address) : base(id, ecv, address)
    {

    }

    public override byte[] GetByteKey()
    {
        using (var memoryStream = new MemoryStream())
        { 
            memoryStream.Write(BitConverter.GetBytes(Id), 0, sizeof(int));
            return memoryStream.ToArray();
        }
    }

    public override bool KeyUpdated(CustomerByKey other)
    {
        return Id != other.Id;
    }

    public override bool Equals(CustomerByKey other)
    {
        return Id == other.Id;
    }
}