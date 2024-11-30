using System.Collections;
using System.Text;

class Dummy : IRecord<Dummy>, IExtendRec
{
    private int _id;
    private string _name;
    private int _age;
    private double _weight;
    private static readonly int _maxNameLength = 10;
    private int _nameLength;

    public Dummy(int id, string name, int age, double weight) {
        _id = id;
        _name = name;
        _nameLength = _name.Length;
        _age = age;
        _weight = weight;
    }

    public Dummy() {

    }

    public int Id { get => _id; set => _id = value; }
    public string Name { get => _name; set => _name = value; }
    public int Age { get => _age; set => _age = value; }
    public double Weight { get => _weight; set => _weight = value; }
    public int NameLength { get => _nameLength; set => _nameLength = value; }

    public bool Equals()
    {
        return false;
    }

    public int GetSize()
    {
        int size = 0;

        size += sizeof(int);
        size += sizeof(int);
        size += sizeof(double);

        if (_name != null)
        {
            size +=  _maxNameLength * sizeof(char);
        }

        return size;
    }

    public bool Equals(Dummy other) {
        if (other == null) {
            return false;
        }

        return _id == other._id;
    }

    public Dummy CreateInstance()
    {
        return new Dummy(0, "xxxxxxxxxx", 0, 0.0);
    }

    public byte[] GetByteArray()
    {
        using (var memoryStream = new MemoryStream())
        { 
            memoryStream.Write(BitConverter.GetBytes(_id), 0, sizeof(int));
            
            memoryStream.Write(BitConverter.GetBytes(_age), 0, sizeof(int));
            memoryStream.Write(BitConverter.GetBytes(_weight), 0, sizeof(double));

            string paddedName = _name?.PadRight(_maxNameLength, '\0') ?? new string('\0', _maxNameLength);
            byte[] nameBytes = Encoding.Unicode.GetBytes(paddedName);
            memoryStream.Write(nameBytes, 0, nameBytes.Length);

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

            memoryStream.Read(intBuffer, 0, sizeof(int));
            _age = BitConverter.ToInt32(intBuffer, 0);

            byte[] longBuffer = new byte[sizeof(double)];
            memoryStream.Read(longBuffer, 0, sizeof(double));
            _weight = BitConverter.ToDouble(longBuffer, 0);

             byte[] nameBytes = new byte[_maxNameLength * sizeof(char)];
            memoryStream.Read(nameBytes, 0, nameBytes.Length);
            _name = Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');

        }
        
    }

    public override string ToString()
    {
        return $"Dummy Id: {_id}";
    }

    public byte[] GetByteKey()
    {
        throw new NotImplementedException();
    }

    public void Update(Dummy other)
    {
        throw new NotImplementedException();
    }

    public bool KeyUpdated(Dummy other)
    {
        throw new NotImplementedException();
    }
}