using System.Text;

class CustomerByKey : IRecord<CustomerByKey>, IExtendRec
{
    private int _id;
    private string _ecv = "xxxxxxxxxx";
    private static readonly int _maxEcvSize = 10;
    long _address;

    public int Id { get => _id; set => _id = value; }
    public string Ecv { get => _ecv; set => _ecv = value; }
    public long Address { get => _address; set => _address = value; }
    public static int MaxEcvSize => _maxEcvSize;

    public CustomerByKey(int id, string ecv, long address) {
        _id = id;
        _ecv = ecv;
        _address = address;
    }

    public bool Equals()
    {
        return false;
    }

    public int GetSize()
    {
        int size = 0;

        size += sizeof(int);
        size += sizeof(long);

        if (_ecv != null)
        {
            size +=  _maxEcvSize * sizeof(char);
        }

        return size;
    }

    public byte[] GetByteArray()
    {
         using (var memoryStream = new MemoryStream())
        { 
            memoryStream.Write(BitConverter.GetBytes(_id), 0, sizeof(int));

            string paddedEcv = _ecv?.PadRight(_maxEcvSize, '\0') ?? new string('\0', _maxEcvSize);
            byte[] ecvBytes = Encoding.Unicode.GetBytes(paddedEcv);
            memoryStream.Write(ecvBytes, 0, ecvBytes.Length);

            memoryStream.Write(BitConverter.GetBytes(_address), 0, sizeof(long));

            return memoryStream.ToArray();
        }
    }

    public void FromByteArray(byte[] bytes)
    {
        using (var memoryStream = new MemoryStream(bytes))
        {
            byte[] intBuffer = new byte[sizeof(int)];
            memoryStream.Read(intBuffer, 0, sizeof(int));
            _id = BitConverter.ToInt32(intBuffer, 0);

            byte[] ecvBytes = new byte[_maxEcvSize * sizeof(char)];
            memoryStream.Read(ecvBytes, 0, ecvBytes.Length);
            _ecv = Encoding.Unicode.GetString(ecvBytes).TrimEnd('\0');

           byte[] longBuffer = new byte[sizeof(long)];
            memoryStream.Read(longBuffer, 0, sizeof(long));
            _address = BitConverter.ToInt32(longBuffer, 0);
        }
    }

    public bool Equals(CustomerByKey other)
    {
        throw new NotImplementedException();
    }

    public CustomerByKey CreateInstance()
    {
        return new CustomerByKey(-1, "xxxxxxxxxx", -1);
    }

    public void Update(CustomerByKey other)
    {
        _id = _id != other.Id ? other.Id : _id;
        _ecv = _ecv != other.Ecv ? other.Ecv : _ecv;
    }

    public bool KeyUpdated(CustomerByKey other)
    {
        throw new NotImplementedException();
    }

    public byte[] GetByteKey()
    {
        throw new NotImplementedException();
    }
}