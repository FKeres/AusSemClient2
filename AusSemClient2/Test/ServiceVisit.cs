using System.Text;

public class ServiceVisit : IRecord<ServiceVisit>
{
    private int _id;
    private DateTime _date;
    private double _price;
    private string[] _description;
    private int _validDesc;
    private static readonly int _maxDescSize = 20;
    public int Id { get => _id; set => _id = value; }

    public static int MaxDescSize => _maxDescSize;

    public double Price { get => _price; set => _price = value; }
    public string[] Description { get => _description; set => _description = value; }
    public DateTime Date { get => _date; set => _date = value; }
    public int ValidDesc { get => _validDesc; set => _validDesc = value; }

    public ServiceVisit() {
        _description = new string[10];
    }

    public ServiceVisit(int id, DateTime date, double price) {
        _id = id;
        _description = new string[10];
        _price = price;
        _date = date;
        _validDesc = 0;

        var tmpString = "xxxxxxxxxx";
        for(int i = 0; i < _description.Length; ++i) {
            _description[i] = tmpString;
        }
    }


    public ServiceVisit CreateInstance()
    {
        return new ServiceVisit(-1, DateTime.MinValue, 0.0);
    }

    public bool Equals()
    {
        return false;
    }

    public bool Equals(ServiceVisit other)
    {
        return _id == other.Id;
    }

   public void FromByteArray(byte[] byteArray)
    {
        using (var memoryStream = new MemoryStream(byteArray))
        {
            byte[] intBuffer = new byte[sizeof(int)];
            memoryStream.Read(intBuffer, 0, sizeof(int));
            _id = BitConverter.ToInt32(intBuffer, 0);

            byte[] dateBytes = new byte[sizeof(long)];
            memoryStream.Read(dateBytes, 0, sizeof(long));
            _date = new DateTime(BitConverter.ToInt64(dateBytes, 0));

            byte[] priceBytes = new byte[sizeof(double)];
            memoryStream.Read(priceBytes, 0, sizeof(double));
            _price = BitConverter.ToDouble(priceBytes, 0);

            //fikesk
            byte[] validDescBytes = new byte[sizeof(int)];
            memoryStream.Read(validDescBytes, 0, sizeof(int));
            _validDesc = BitConverter.ToInt32(validDescBytes, 0);
            //fikesk

            for(int i = 0; i < _description.Length; ++i) {
                byte[] descBytes = new byte[_maxDescSize * sizeof(char)];
                memoryStream.Read(descBytes, 0, descBytes.Length);
                _description[i] = Encoding.Unicode.GetString(descBytes).TrimEnd('\0');
            }

        }
    }


    public byte[] GetByteArray()
    {
        using (var memoryStream = new MemoryStream())
        { 
            memoryStream.Write(BitConverter.GetBytes(_id), 0, sizeof(int));
            memoryStream.Write(BitConverter.GetBytes(_date.Ticks), 0, sizeof(long));
            memoryStream.Write(BitConverter.GetBytes(_price), 0, sizeof(double));
            //fikesk
            memoryStream.Write(BitConverter.GetBytes(_validDesc), 0, sizeof(int));
            //fikesk

            foreach (var rec in _description)
            {
                string paddedDesc = rec?.PadRight(_maxDescSize, '\0') ?? new string('\0', _maxDescSize);
                byte[] descBytes = Encoding.Unicode.GetBytes(paddedDesc);
                memoryStream.Write(descBytes, 0, descBytes.Length);
            }

            return memoryStream.ToArray();
        }
    }

    public int GetSize()
    {
        int idSize = sizeof(int);
        int dateSize = sizeof(long);
        int doubleSize = sizeof(double);
        int validDescSize = sizeof(int);
        int stringSize = _maxDescSize * sizeof(char) * _description.Length;

        return idSize + dateSize + doubleSize + validDescSize + stringSize;
    }

    public override string ToString()
    {
        string tmpString =  _id.ToString() + _price.ToString() + _date.ToString();  
        foreach(var desc in _description) {
            tmpString += desc;
        }

        return tmpString;
    }

    public void AddDescription(string description) {
        if(_validDesc < description.Length) {
            _description[_validDesc] = description;
            ++_validDesc;
        }
    }

    public void Update(ServiceVisit other)
    {
        _date = other.Date != _date ? other.Date : _date;
        for(int i = 0; i < other.ValidDesc; ++i) {
            _description[i] = other.Description[i] != _description[i] ? other.Description[i] : _description[i];
        }
        _validDesc = other.ValidDesc != _validDesc ? other.ValidDesc : _validDesc;
        _price = other.Price != _price ? other.Price : _price;
    }

    public bool KeyUpdated(ServiceVisit other)
    {
        throw new NotImplementedException();
    }

    public void CopyFrom(ServiceVisit other) {
        _date = other.Date;
        _id = other.Id;
        _price = other.Price;
        _validDesc = other.ValidDesc;

        for (int i = 0; i < _description.Length; i++)
        {
            _description[i] = other.Description[i];
        }

    }

    public void RemoveDescriptions() {
        _validDesc = 0;
    }
}