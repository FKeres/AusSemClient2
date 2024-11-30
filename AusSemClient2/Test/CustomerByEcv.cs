using System.Text;

class CustomerByEcv :CustomerByKey
{
    public CustomerByEcv(int id, string ecv, long address) : base(id, ecv, address)
    {

    }

    public override byte[] GetByteKey()
    {
        using (var memoryStream = new MemoryStream())
        { 
            string paddedEcv = Ecv?.PadRight(MaxEcvSize, '\0') ?? new string('\0', MaxEcvSize);
            byte[] ecvBytes = Encoding.Unicode.GetBytes(paddedEcv);
            memoryStream.Write(ecvBytes, 0, ecvBytes.Length);
            return memoryStream.ToArray();
        }
    }

    public override bool KeyUpdated(CustomerByKey other)
    {
        return Ecv != other.Ecv;
    }

    public override bool Equals(CustomerByKey other)
    {
        return Ecv == other.Ecv;
    }
}