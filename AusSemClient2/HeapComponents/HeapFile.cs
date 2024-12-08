
using System.Runtime.CompilerServices;

class HeapFile<T> where T:IRecord<T>, new()
{
    private long _firstEmpty;
    private long _firstPartlyEmpty;
    private string _filePath;
    private Block<T> _block;
    private int _size;
    private FileStream _fs;

    public long FirstEmpty { get => _firstEmpty; set => _firstEmpty = value; }
    public long FirstPartlyEmpty { get => _firstPartlyEmpty; set => _firstPartlyEmpty = value; }
    public int Size { get => _size; set => _size = value; }
    public FileStream Fs { get => _fs; set => _fs = value; }
    internal Block<T> Block { get => _block; set => _block = value; }

    public HeapFile(int size, T item, string filePath) {
        _filePath = filePath;
        _firstEmpty = -1;
        _firstPartlyEmpty = -1;
        _size = size;

        _block = new(size, item.GetSize());

        _fs = new FileStream(_filePath, FileMode.Create, FileAccess.ReadWrite);

    }


    /// <summary>
    /// Reads data from given address
    /// </summary>
    /// <param name="address"></param>
    public void ReadBlock(long address) {
        _fs.Seek(address, SeekOrigin.Begin);
        int actualBlockSize = _block.GetSize();
        byte[] blockBytes = new byte[actualBlockSize];
        _fs.Read(blockBytes, 0, blockBytes.Length);
        _block.FromByteArray(blockBytes);
    }

    /// <summary>
    /// writes data to block at given addreaa
    /// </summary>
    /// <param name="address"></param>
    public void WriteBlock(long address) {
        _fs.Seek(address, SeekOrigin.Begin);
        byte[] updatedBlockBytes = _block.GetByteArray();
        _fs.Write(updatedBlockBytes, 0, updatedBlockBytes.Length);
    }


    /// <summary>
    /// inserts record to block primarly to partly empty and empty block
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public long Insert(T item) {
        long address = -1;
        bool written = false;

        //checks if partly empty block exists 
        if (_firstPartlyEmpty != -1) {
            address = _firstPartlyEmpty;
            ReadBlock(_firstPartlyEmpty);

            _block.Insert(item);

            //if partly empty after insert is full than is removed from partly empty
            if (_block.IsFull()) {
                _firstPartlyEmpty = _block.Next;
                _block.Next = -1;
                WriteBlock(address);
                written = true;
                ActualizeBlockReferences(_firstPartlyEmpty, -1, null);
            }

            if(!written) {
                WriteBlock(address);
            }
            
        }
        else if (_firstEmpty != -1) {
            address = _firstEmpty;
            ReadBlock(_firstEmpty);

            _block.Insert(item);

            _firstEmpty = _block.Next;

            //if empty block after insert is not full than becomes first partly empty
            if(!_block.IsFull()) {
                if (FirstPartlyEmptyIsEmpty()) {
                    _firstPartlyEmpty = _block.Address;
                }
            }

            _block.Next = -1;
            WriteBlock(address);

            ActualizeBlockReferences(_firstEmpty, -1, null);

        }
        else {
            _fs.Seek(0, SeekOrigin.End);

            _block.Initialize();
            _block.Insert(item);
            _block.Address = _fs.Position;

            address = _block.Address;

            //if empty block after insert is not full than becomes first partly empty
            if(!_block.IsFull()) {
                if (FirstPartlyEmptyIsEmpty()) {
                    _firstPartlyEmpty = _block.Address;
                }
            }

            WriteBlock(_block.Address);
        }
        
        return address;
    }

    /// <summary>
    /// inserts record to given address
    /// </summary>
    /// <param name="address"></param>
    /// <param name="item"></param>
    public void InsertAtAddress(long address, T item) {
        ReadBlock(address);
        _block.Insert(item);
        WriteBlock(address);
    }


    /// <summary>
    /// returns item from given address
    /// </summary>
    /// <param name="address"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public T Get(long address, T item) {
       
        ReadBlock(address);
        
        return _block.Get(item);
    }

    /// <summary>
    /// removes given item at given address
    /// </summary>
    /// <param name="address"></param>
    /// <param name="item"></param>
    public void Remove(long address, T item) {

        ReadBlock(address);
        bool wasPartlyEmpty = _block.IsPartlyEmpty();
        _block.Remove(item);

        if(_block.IsEmpty()) {

            long previous = _block.Previous;
            long next = _block.Next;

            if(IsLast(_fs.Length, _block.Address)) {
                if(_firstPartlyEmpty == _block.Address) {
                    _firstPartlyEmpty = _block.Next;
                }
                //if is last than removed entirely block
                _fs.SetLength(address);
                //removes empty blocks from end of the file
                CLearEnd();
            } else {
                if(_firstPartlyEmpty == _block.Address) {
                    _firstPartlyEmpty = _block.Next;
                }
                _block.Next = _firstEmpty;
                _firstEmpty = _block.Address;
                WriteBlock(address);
                ActualizeBlockReferences(_block.Next, _block.Address, null);
            }

            ActualizeBlockReferences(previous, null, next);
            ActualizeBlockReferences(next, previous, null);
            
        } else if (_block.IsPartlyEmpty()) {
            
            //if already was partly empty no need to actualize references
            if(!wasPartlyEmpty) {
                _block.Next = _firstPartlyEmpty;
                _firstPartlyEmpty = _block.Address;
                WriteBlock(address);
                ActualizeBlockReferences(_block.Next, _block.Address, null);
            } else {
                WriteBlock(address);
            }

        }
    }

    /// <summary>
    /// updates items attributes
    /// </summary>
    /// <param name="address"></param>
    /// <param name="item"></param>
    public void Update(long address, T item) {
        T found = Get(address, item);
        found.Update(item);
        WriteBlock(address);
    }

    /// <summary>
    /// removes empty blocks from end of the file
    /// </summary>
    private void CLearEnd() {
        long actualAddress = _fs.Position - _block.GetSize();

        while(true) {
            if(actualAddress >= 0) {
                ReadBlock(actualAddress);
                if(IsLast(_fs.Length, _block.Address) && _block.IsEmpty()) {
                    
                    //if is first empty than next is new first empty
                    if(_firstEmpty == _block.Address) {
                        _firstEmpty = _block.Next;
                    }

                    long next = _block.Next;
                    long previous = _block.Previous;

                    //removed from empty references
                    ActualizeBlockReferences(previous, null, next);
                    ActualizeBlockReferences(next, previous, null);
                    
                    _fs.SetLength(actualAddress);
                } else {
                    //ends if is not last or is not empty 
                    break;
                }
                actualAddress -= _block.GetSize();
            }
            else {
                break;
            }
        }

    }

    //actualizes block references to next and previous at given address ignored if address is -1
    private void ActualizeBlockReferences(long address, long? previous, long? next) {
        if(address >= 0) {
            ReadBlock(address);
            
            if(previous is not null) {
                _block.Previous = (long)previous;
            }

            if (next is not null) {
                _block.Next = (long)next;
            } 

            WriteBlock(address);
        }
    }

    //sequence iterares throught heap file
    public IEnumerable<Block<T>> SequenceIterate(){

        long fileSize = _fs.Length;
        int blockSize = _block.GetSize();
        long actualAddress = 0;

        while (actualAddress < fileSize)
        {
            ReadBlock(actualAddress);

            yield return _block;

            actualAddress += blockSize;
        }
    }


    public bool FirstEmptyIsEmpty() {
        return _firstEmpty < 0 ? true : false;
    }

    public bool FirstPartlyEmptyIsEmpty() {
        return _firstPartlyEmpty < 0 ? true : false;
    }

    private bool IsLast(long fileLength, long address) {
        return address + _block.GetSize() == fileLength;
    }

    /// <summary>
    /// closes the file
    /// </summary>
    public void CloseFile() {
        _fs?.Close();
        _fs = null;
    }

    /// <summary>
    /// creates new block at the end of file and returns blocks address
    /// </summary>
    /// <returns></returns>
    public long CreateBlock() {
        long address = -1;
        _fs.Seek(0, SeekOrigin.End);

        _block.Initialize();
        _block.Address = _fs.Position;
        address = _block.Address;

        _block.Next = _firstEmpty;
        _firstEmpty = _block.Address;

        WriteBlock(_block.Address);
        ActualizeBlockReferences(_block.Next, _block.Address, null);
        
        return address;
    }

    /// <summary>
    /// returns string of heap file attributes in csv format
    /// </summary>
    /// <returns></returns>
    public string GetHeader() {
        return "FIRSTEMPTY,FIRSTPARTLYEMPTY,SIZE";
    }
    
    /// <summary>
    /// returns string of heap file attributes in csv format
    /// </summary>
    /// <returns></returns>
    public string GetBody() {
        return $"{_firstEmpty},{_firstPartlyEmpty},{_size}";
    }

    /// <summary>
    /// loads attributes data from file
    /// </summary>
    /// <param name="body"></param>
    public void Load(string body) {
        string[] parts = body.Split(',');
        _firstEmpty = int.Parse(parts[0]);
        _firstPartlyEmpty = int.Parse(parts[1]);
        _size = int.Parse(parts[2]);
    }

    /// <summary>
    /// makes copy of used file
    /// </summary>
    public void SaveState()
    {
        string savePath = _filePath + "-save";

        _fs?.Close();

        File.Copy(_filePath, savePath, overwrite: true);

        _fs = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite);

        Console.WriteLine($"State saved to {savePath}");
    }

    /// <summary>
    /// loads copy of used file
    /// </summary>

    public void LoadState()
    {
        string savePath = _filePath + "-save";

        if (File.Exists(savePath))
        {
            _fs?.Close();

            File.Copy(savePath, _filePath, overwrite: true);

            _fs = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite);

            Console.WriteLine($"State loaded from {savePath}");
        }
        else
        {
            Console.WriteLine($"Save file not found: {savePath}");
        }
    }

}