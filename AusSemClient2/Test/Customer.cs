using System.Text;

class Customer : IRecord<Customer>, IExtendRec
{

    private int _id;
    private string _name;
    private static readonly int _maxNameSize = 15;
    private string _lastName;
    private static readonly int _maxLastNameSize = 20;
    private ServiceVisit[] _serviceVisit;
    private int _validServiceNum;

    public int Id { get => _id; set => _id = value; }
    public string Name { get => _name; set => _name = value; }
    public string LastName { get => _lastName; set => _lastName = value; }
    internal ServiceVisit[] ServiceVisit { get => _serviceVisit; set => _serviceVisit = value; }
    public int ValidServiceNum { get => _validServiceNum; set => _validServiceNum = value; }

    public Customer() {
        _serviceVisit = new ServiceVisit[5];
    }

    public Customer(int id, string name, string lastName) {
        _id = id;
        _name = name;
        _lastName = lastName;
        _serviceVisit = new ServiceVisit[5];

        var tempServVisit = new ServiceVisit();
        for(int i = 0; i < _serviceVisit.Length; ++i) {
            _serviceVisit[i] = tempServVisit.CreateInstance();
        }
    }

    public void AddServVisit(ServiceVisit serviceVisit) {
        if(_validServiceNum <= _serviceVisit.Length) {
            _serviceVisit[_validServiceNum] = serviceVisit;
            ++_validServiceNum;
        }   
    }

    public bool Equals()
    {
        return false;
    }

    public byte[] GetByteArray()
    {
        using (var memoryStream = new MemoryStream())
        { 
            memoryStream.Write(BitConverter.GetBytes(_id), 0, sizeof(int));

            string paddedName = _name?.PadRight(_maxNameSize, '\0') ?? new string('\0', _maxNameSize);
            byte[] nameBytes = Encoding.Unicode.GetBytes(paddedName);
            memoryStream.Write(nameBytes, 0, nameBytes.Length);

            string paddedLastName = _lastName?.PadRight(_maxLastNameSize, '\0') ?? new string('\0', _maxLastNameSize);
            byte[] lastNameBytes = Encoding.Unicode.GetBytes(paddedLastName);
            memoryStream.Write(lastNameBytes, 0, lastNameBytes.Length);

            memoryStream.Write(BitConverter.GetBytes(_validServiceNum), 0, sizeof(int));

            foreach (var rec in _serviceVisit)
            {
                byte[] recordBytes = rec.GetByteArray();
                memoryStream.Write(recordBytes, 0, recordBytes.Length);
            }

            return memoryStream.ToArray();
        }
    }

    public bool Equals(Customer other)
    {

        if(_id == other.Id) {
            for(int i = 0; i < _serviceVisit.Length; ++i)
            {
                if(!_serviceVisit[i].Equals(other.ServiceVisit[i])) {
                    return false;
                }
            }
            return true;
        } 
        
        return false;
    }

    public Customer CreateInstance()
    {
        ServiceVisit service = new ServiceVisit();
        Customer customer = new Customer(0, "xxxxxxxxxxxxxxx", "xxxxxxxxxxxxxxxxxxxx");

        for(int i = 0; i < _serviceVisit.Length; ++i) {
            customer.AddServVisit(service.CreateInstance());
        }

        return customer;
    }

    public int GetSize()
    {
        int size = 0;

        size += sizeof(int);
        size += sizeof(int);

        if (_name != null)
        {
            size +=  _maxNameSize * sizeof(char);
        }

        if (_lastName != null)
        {
            size +=  _maxLastNameSize * sizeof(char);
        }

        foreach(var item in _serviceVisit) {
            size += item.GetSize();
        }

        return size;
    }

    public void FromByteArray(byte[] bytes)
    {
        using (var memoryStream = new MemoryStream(bytes))
        {
            byte[] intBuffer = new byte[sizeof(int)];
            memoryStream.Read(intBuffer, 0, sizeof(int));
            _id = BitConverter.ToInt32(intBuffer, 0);

            byte[] nameBytes = new byte[_maxNameSize * sizeof(char)];
            memoryStream.Read(nameBytes, 0, nameBytes.Length);
            _name = Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');

            byte[] lastNameBytes = new byte[_maxLastNameSize * sizeof(char)];
            memoryStream.Read(lastNameBytes, 0, lastNameBytes.Length);
            _lastName = Encoding.Unicode.GetString(lastNameBytes).TrimEnd('\0');

            memoryStream.Read(intBuffer, 0, sizeof(int));
            _validServiceNum = BitConverter.ToInt32(intBuffer, 0);

            foreach(var item in _serviceVisit) {
                 byte[] serviceVisitBytes = new byte[
                    sizeof(int) +
                    sizeof(long) +
                    sizeof(double) +
                    20 * sizeof(char)
                ];

                memoryStream.Read(serviceVisitBytes, 0, serviceVisitBytes.Length);
                item.FromByteArray(serviceVisitBytes);
            }
        }
    }

    public byte[] GetByteKey()
    {
        using (var memoryStream = new MemoryStream())
        { 
            memoryStream.Write(BitConverter.GetBytes(_id), 0, sizeof(int));
            return memoryStream.ToArray();
        }
    }

    public override string ToString()
    {
        return _id.ToString();
    }

    public string ToStringFull() {
        string result = "";

        foreach(var item in _serviceVisit) {
            result += item.ToString();
        }
        return _id.ToString() + _name + _lastName; 
    }
}