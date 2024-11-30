using System.Text;

class ServiceVisit : IRecord<ServiceVisit>
{
    private int _id;
    private DateTime _date;
    private double _price;
    private string _description;
    private static readonly int _maxDescSize = 20;
    public int Id { get => _id; set => _id = value; }

    public static int MaxDescSize => _maxDescSize;

    public double Price { get => _price; set => _price = value; }
    public string Description { get => _description; set => _description = value; }
    public DateTime Date { get => _date; set => _date = value; }

    public ServiceVisit() {

    }

    public ServiceVisit(int id, DateTime date, double price, string description) {
        _id = id;
        _description = description;
        _price = price;
        _date = date;
    }


    public ServiceVisit CreateInstance()
    {
        return new ServiceVisit(-1, DateTime.MinValue, 0.0, "xxxxxxxxxxxxxxxxxxxx");
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

            byte[] descBytes = new byte[_maxDescSize * sizeof(char)];
            memoryStream.Read(descBytes, 0, descBytes.Length);
            _description = Encoding.Unicode.GetString(descBytes).TrimEnd('\0');

        }
    }


    public byte[] GetByteArray()
    {
        using (var memoryStream = new MemoryStream())
        { 
            memoryStream.Write(BitConverter.GetBytes(_id), 0, sizeof(int));
            memoryStream.Write(BitConverter.GetBytes(_date.Ticks), 0, sizeof(long));
            memoryStream.Write(BitConverter.GetBytes(_price), 0, sizeof(double));

            string paddedDesc = _description?.PadRight(_maxDescSize, '\0') ?? new string('\0', _maxDescSize);
            byte[] nameBytes = Encoding.Unicode.GetBytes(paddedDesc);
            memoryStream.Write(nameBytes, 0, nameBytes.Length);

            return memoryStream.ToArray();
        }
    }

    public int GetSize()
    {
        int idSize = sizeof(int);
        int dateSize = sizeof(long);
        int doubleSize = sizeof(double);
        int stringSize = _maxDescSize * sizeof(char);

        return idSize + dateSize + doubleSize + stringSize;
    }

    public override string ToString()
    {
        return _id.ToString() + _price.ToString() + _description + _date.ToString();
    }
}