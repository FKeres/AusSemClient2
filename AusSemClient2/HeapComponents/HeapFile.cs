
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

    public void ReadBlock(long address) {
        _fs.Seek(address, SeekOrigin.Begin);
        int actualBlockSize = _block.GetSize();
        byte[] blockBytes = new byte[actualBlockSize];
        _fs.Read(blockBytes, 0, blockBytes.Length);
        _block.FromByteArray(blockBytes);
    }

    public void WriteBlock(long address) {
        _fs.Seek(address, SeekOrigin.Begin);
        byte[] updatedBlockBytes = _block.GetByteArray();
        _fs.Write(updatedBlockBytes, 0, updatedBlockBytes.Length);
    }


    public long Insert(T item) {
        long address = -1;
        bool written = false;

        if (_firstPartlyEmpty != -1) {
            address = _firstPartlyEmpty;
            ReadBlock(_firstPartlyEmpty);

            _block.Insert(item);

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

            if(!_block.IsFull()) {
                if (FirstPartlyEmptyIsEmpty()) {
                    _firstPartlyEmpty = _block.Address;
                }
            }

            WriteBlock(_block.Address);
        }
        
        return address;
    }

    public void InsertAtAddress(long address, T item) {
        ReadBlock(address);
        _block.Insert(item);
        WriteBlock(address);
    }


    public T Get(long address, T item) {
       
        ReadBlock(address);
        
        return _block.Get(item);
    }

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
                _fs.SetLength(address);
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
            
            if(!wasPartlyEmpty) {
                _block.Next = _firstPartlyEmpty;
                _firstPartlyEmpty = _block.Address;
                WriteBlock(address);
                ActualizeBlockReferences(_block.Next, _block.Address, null);
            }

        }
    }

    public void Update(long address, T item) {
        T found = Get(address, item);
        found.Update(item);
        WriteBlock(address);
    }

    private void CLearEnd() {
        long actualAddress = _fs.Position - _block.GetSize();

        while(true) {
            if(actualAddress >= 0) {
                ReadBlock(actualAddress);
                if(IsLast(_fs.Length, _block.Address) && _block.IsEmpty()) {
                    _firstEmpty = _block.Next;
                    _block.Next = -1;

                    if(_firstPartlyEmpty == _block.Address) {
                        _firstPartlyEmpty = _block.Next;
                    }

                    ActualizeBlockReferences(_firstEmpty, -1, null);
                    _fs.SetLength(actualAddress);
                } else {
                    break;
                }
                actualAddress -= _block.GetSize();
            }
            else {
                break;
            }
        }

    }

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

    public void CloseFile() {
        _fs?.Close();
        _fs = null;
    }

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
    
}