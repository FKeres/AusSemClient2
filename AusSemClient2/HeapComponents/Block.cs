class Block<T> where T:IRecord<T>, new()
{
    private T[] _records;
    private long _next;
    private long _previous;
    private int _validCount;
    private long _address;
    private int _blockSize;
    private int _actualSize;
    //private int _blockDepth;

    public T[] Records { get => _records; set => _records = value; }
    public long Next { get => _next; set => _next = value; }
    public long Previous { get => _previous; set => _previous = value; }
    public int ValidCount { get => _validCount; set => _validCount = value; }
    public long Address { get => _address; set => _address = value; }
    public int BlockSize { get => _blockSize; set => _blockSize = value; }
    public int ActualSize { get => _actualSize; set => _actualSize = value; }
    //public int BlockDepth { get => _blockDepth; set => _blockDepth = value; }

    public Block (int size, int itemSize) {
        _next = -1;
        _previous = -1;
        _validCount = 0;
        _blockSize = (int)Math.Floor((decimal)((size - 2*sizeof(int) - 3*sizeof(long))/itemSize));
        _records = new T[_blockSize];
        _actualSize = size;   
        //_blockDepth = 1;
        InitializeRecs();
    }

    public Block(int size, int next, int previous) {
        _records = new T[size];
        _next = next;
        _previous = previous;
        _validCount = 0;
        _blockSize = size;
        //_blockDepth = 1;
        InitializeRecs();
    }

    /// <summary>
    /// makes all records to a full size so it can be written to file
    /// </summary>
    public void InitializeRecs() {
        int i = 0;
        T instance = new T();

        while(i < _blockSize) {
            _records[i] = instance.CreateInstance();
            ++i;
        }

    }

    /// <summary>
    /// initializes Block 
    /// </summary>
    public void Initialize() {
        _validCount = 0;
        _next = -1;
        _previous = -1;
        _address = -1;
        //_blockDepth = 1;
        InitializeRecs();
    }

    /// <summary>
    /// inserts item to block, and increases valid count
    /// </summary>
    /// <param name="item"></param>
    public void Insert(T item) {
        if(_validCount >= 0 && _validCount <= _blockSize) {
            _records[_validCount] = item;
            ++_validCount;
        }
    }

    /// <summary>
    /// returns item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public T Get(T item) {
        int i = 0;
        while(i < _validCount) {
            if(_records[i].Equals(item)) {
                return _records[i];
            }
            ++i;
        }
        throw new Exception($"This item {item.ToString} does not exist on address {_address}");
    }

    /// <summary>
    /// removes item from block and decreases valid count
    /// </summary>
    /// <param name="item"></param>
    public void Remove(T item) {
        int index = 0;
        while(index < _validCount) {
            if(_records[index].Equals(item)) {
                SwapItems(index);
                --_validCount;
                break;
            }
            ++index;
        }
    }

    /// <summary>
    /// returns size of block
    /// </summary>
    /// <returns></returns>
    public int GetSize() {
        /*
        int recordSize = _blockSize * _records[0].GetSize();
        int metadataSize = sizeof(int) + sizeof(int) + sizeof(int) + sizeof(long) + sizeof(int);
        return recordSize + metadataSize;
        */
        return _actualSize;
    }

    public bool IsPartlyEmpty() {
        return _validCount > 0 && _validCount < _blockSize;
    }

    public bool IsEmpty() {
        return _validCount == 0;
    }

    public bool IsFull() {
        return _validCount == _blockSize;
    }

    public void SwapItems(int index) {
        var tmp = _records[index];
        _records[index] = _records[_validCount -1];
        _records[_validCount -1] = tmp;
    }

    /// <summary>
    /// returns all attributes and records in byte[]
    /// </summary>
    /// <returns></returns>
    public byte[] GetByteArray() {
        using (var memoryStream = new MemoryStream())
        {
            //memoryStream.Write(BitConverter.GetBytes(_blockDepth), 0, sizeof(int));
            memoryStream.Write(BitConverter.GetBytes(_next), 0, sizeof(long));
            memoryStream.Write(BitConverter.GetBytes(_previous), 0, sizeof(long));
            memoryStream.Write(BitConverter.GetBytes(_validCount), 0, sizeof(int));
            memoryStream.Write(BitConverter.GetBytes(_address), 0, sizeof(long));
            memoryStream.Write(BitConverter.GetBytes(_blockSize), 0, sizeof(int));

            foreach (var rec in _records)
            {
                byte[] recordBytes = rec.GetByteArray();
                memoryStream.Write(recordBytes, 0, recordBytes.Length);
            }

            while (memoryStream.Length < _actualSize)
            {
                memoryStream.WriteByte(0);
            }

            return memoryStream.ToArray();
        }
    }

    /// <summary>
    /// initializes all attributes and records from byte[]
    /// </summary>
    /// <returns></returns>
    public void FromByteArray(byte[] bytes) {
        using (var memoryStream = new MemoryStream(bytes))
        {
            byte[] intBuffer = new byte[sizeof(int)];
            byte[] longBuffer = new byte[sizeof(long)];

            //memoryStream.Read(intBuffer, 0, sizeof(int));
            //_blockDepth = BitConverter.ToInt32(intBuffer, 0);

            memoryStream.Read(longBuffer, 0, sizeof(long));
            _next = BitConverter.ToInt64(longBuffer, 0);

            memoryStream.Read(longBuffer, 0, sizeof(long));
            _previous = BitConverter.ToInt64(longBuffer, 0);

            memoryStream.Read(intBuffer, 0, sizeof(int));
            _validCount = BitConverter.ToInt32(intBuffer, 0);

            memoryStream.Read(longBuffer, 0, sizeof(long));
            _address = BitConverter.ToInt64(longBuffer, 0);

            memoryStream.Read(intBuffer, 0, sizeof(int));
            _blockSize = BitConverter.ToInt32(intBuffer, 0);
            
            int i = 0;
            while (i < _blockSize)
            {
                byte[] recordBytes = new byte[_records[i].GetSize()];
                memoryStream.Read(recordBytes, 0, recordBytes.Length);

                _records[i].FromByteArray(recordBytes);
                ++i;
            }

        }
    }
}