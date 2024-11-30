using System.Security.Cryptography;
using System.Text;

class ExtendibleHash<T> where T : IExtendRec, IRecord<T>,  new()
{

    //nedavam do suboru prazdny blok
    //validcount a adresy mozu byt v operacnej pamati
    //data budu v heapfile a 2 rozsiritelne subory budu pre id a EVC a adresu heapfile a to druhe tiez 
    //cize bude jeden heapfile a 2x rozsiritelne
    //dlzku hashu treba obmedzit na 32 bitov
    private HeapFile<T> _heapFile;
    private BlockProps[] _blockProps;
    private int _actualDepth;

    public int ActualDepth { get => _actualDepth; set => _actualDepth = value; }
    internal HeapFile<T> HeapFile { get => _heapFile; set => _heapFile = value; }
    internal BlockProps[] BlockProps { get => _blockProps; set => _blockProps = value; }

    public ExtendibleHash(int size, T item, string filePath)
    {
        _heapFile = new HeapFile<T>(size, item, filePath);
        _actualDepth = 1;
        _blockProps = [new BlockProps(_heapFile.CreateBlock(), 1, 0), new BlockProps(_heapFile.CreateBlock(), 1, 0)];
    }

    public void Insert(T item) {
        long address = -1;
        byte[] byteKey = item.GetByteKey();
        Array.Reverse(byteKey);

        while(true) {
            int hashVal = HashItem(byteKey, _actualDepth);
            address = _blockProps[hashVal].Address;

            //check if there is valid address
            if(address < 0) {
                long newAddress = _heapFile.CreateBlock();
                address = newAddress;

                if(_blockProps[hashVal].BlockDepth != _actualDepth) {
                    int leftIf = HashAddBitZero(byteKey, _blockProps[hashVal].BlockDepth, _actualDepth - _blockProps[hashVal].BlockDepth -1, true);
                    int rightIf = HashAddBitOne(byteKey, _blockProps[hashVal].BlockDepth, _actualDepth - _blockProps[hashVal].BlockDepth -1, true);

                    ActualizeAddresses(leftIf, rightIf, newAddress);
                    ActualizeValidCountToZero(leftIf, rightIf);

                } else {
                    _blockProps[hashVal].Address = newAddress;
                    _blockProps[hashVal].ValidCount = 0;
                }
            }


            int left = HashAddBitZero(byteKey, _blockProps[hashVal].BlockDepth, _actualDepth - _blockProps[hashVal].BlockDepth -1, true);
            int right = HashAddBitOne(byteKey, _blockProps[hashVal].BlockDepth, _actualDepth - _blockProps[hashVal].BlockDepth -1, true);

            
            if(_blockProps[hashVal].ValidCount == _heapFile.Block.BlockSize) {
                if(_blockProps[hashVal].BlockDepth == _actualDepth) {
                    DoubleAddresses();
                }
                
                _heapFile.ReadBlock(address);

                Split(left, right);
            } else {
                
                _heapFile.InsertAtAddress( address, item);
                
                //checks if there is the same dimension on block and adress if no i need to update neighbours addresses too
                if(_blockProps[hashVal].BlockDepth != _actualDepth) {
                    ActualizeValidCountPlus(left, right);
                } else {
                    ++_blockProps[hashVal].ValidCount;
                }
                

                break;
            }
        }
    }

    public void Update(T item) {
        byte[] byteKey = item.GetByteKey();
        Array.Reverse(byteKey);

        int hashVal = HashItem(byteKey, _actualDepth);

        T foundItem = FindElementInBlock(_blockProps[hashVal].Address, item);
        
        if(foundItem.KeyUpdated(item)) {
            //Remove(foundItem);
            foundItem.Update(item);
            Insert(foundItem);
        } else {
            foundItem.Update(item);
        }

        
    }

    //actualizes validCount of blocks on provided indexes
    private void ActualizeValidCountToZero(int left, int right)
    {
        for (int i = left; i < right + (right - left); ++i)
        {
            _blockProps[i].ValidCount = 0;
        }

    }

    //actualizes validCount of blocks on provided indexes
    private void ActualizeValidCountMinus(int left, int right)
    {
        for (int i = left; i < right + (right - left); ++i)
        {
            --_blockProps[i].ValidCount;
        }

    }

    //actualizes validCount of blocks on provided indexes
    private void ActualizeValidCountPlus(int left, int right)
    {
        for (int i = left; i < right + (right - left); ++i)
        {
            ++_blockProps[i].ValidCount;
        }

    }

    //actualizes addresses of blocks on provided indexes
    private void ActualizeAddresses(int left, int right, long address)
    {
        for (int i = left; i < right + (right - left); ++i)
        {
            _blockProps[i].Address = address;
        }

    }

    public T Find(T item) {
        byte[] byteKey = item.GetByteKey();
        Array.Reverse(byteKey);

        int hashVal = HashItem(byteKey, _actualDepth);

        return FindElementInBlock(_blockProps[hashVal].Address, item);
    }

    public T FindElementInBlock(long address, T item) {
        _heapFile.ReadBlock(address);

        for(int i = 0; i < _heapFile.Block.ValidCount; ++i) {
            if(_heapFile.Block.Records[i].Equals(item)) {
                return _heapFile.Block.Records[i];
            }
        }

        throw new Exception($"This item {item.ToString()} is not in block {_heapFile.Block.Address}");

    }

    public int Hash(byte[] data, int bitCount) {
        int result = 0;

        for (int i = bitCount - 1; i >= 0; i--) {
            int bitPosition = (data.Length * 8) -1 - i; 

            int byteIndex = bitPosition / 8;
            int bitIndex = i % 8;

            int bitValue = (data[byteIndex] >> bitIndex) & 1;
            result = (result << 1) | bitValue;
        }

        return result;
    }

    public static int HashItem(byte[] data, int bitCount) {
        int result = 0;

        for (int i = 0; i < bitCount; i++) {
            int bitPosition = (data.Length * 8) -1 - i; 

            int byteIndex = bitPosition / 8;
            int bitIndex = i % 8;

            int bitValue = (data[byteIndex] >> bitIndex) & 1;
            result = (result << 1) | bitValue;
        }

        return result;
    }

    public int HashAddBitOne(byte[] data, int bitCount, int zeroCount, bool item){
        int result;

        if(item) {
            result = HashItem(data, bitCount);
        } else {
            result = Hash(data, bitCount);
        }
        
        result = (result << 1) | 1;

        for (int i = 0; i < zeroCount; i++) {
            result = result << 1;
        }

        return result;
    }

    public int HashAddBitZero(byte[] data, int bitCount, int zeroCount, bool item){
        int result;

        if(item) {
            result = HashItem(data, bitCount);
        } else {
            result = Hash(data, bitCount);
        }

        result = result << 1;

        for (int i = 0; i < zeroCount; i++) {
            result = result << 1;
        }

        return result;
    }


    public void Split(int left, int right)
    {
        long newAddress = -1;
        int blockDepth = 0;
        blockDepth = ActualizeDepths(left, right, blockDepth);

        List<T> transfer = new();

        byte[] byteKey;

        // transfer records that no longer belong to this address
        for(int i = 0; i < _heapFile.Block.ValidCount; ++i) {
            byteKey = _heapFile.Block.Records[i].GetByteKey();
            Array.Reverse(byteKey);


            //if (HashItem(byteKey, blockDepth) >= right)
            if (HashItem(byteKey, _actualDepth) >= right)
            {
                transfer.Add(_heapFile.Block.Records[i]);
            }

        }

        //remove items from actual block
        foreach(var item in transfer) {
            _heapFile.Block.Remove(item);
        }

        //if left block is empty transfer all to left but make it right 
        if (_heapFile.Block.IsEmpty())
        {

            foreach (var item in transfer)
            {
                _heapFile.Block.Insert(item);
            }

            _heapFile.WriteBlock(_heapFile.Block.Address);

            for (int j = left; j < right; ++j)
            {
                _blockProps[j].Address = -1;
                _blockProps[j].ValidCount = -1;
            }

        }
        //if right block is empty make right block point to invalid address
        else if (transfer.Count == 0)
        {

            for (int j = right; j < right + (right - left); ++j)
            {
                _blockProps[j].Address = -1;
                _blockProps[j].ValidCount = -1;
            }

        }
        else
        {
            //write left block that is already ready
            _heapFile.WriteBlock(_heapFile.Block.Address);

            //actualize ValidCOunts to the left
            for(int i = left; i < right; ++i) {
                _blockProps[i].ValidCount = _heapFile.Block.ValidCount;
            }

            //create new block
            newAddress = _heapFile.CreateBlock();

            //updates address for this block 
            for(int i = right; i < right + (right - left); ++i) {
                _blockProps[i].Address = newAddress;
            }

            _heapFile.ReadBlock(newAddress);

            foreach (var item in transfer)
            {
                _heapFile.Block.Insert(item);
            }

            _heapFile.WriteBlock(_heapFile.Block.Address);

            //actualize ValidCOunts to the right
            for(int i = right; i < right + (right - left); ++i) {
                _blockProps[i].ValidCount = _heapFile.Block.ValidCount;
            }
        }

    }

    //actualizes depths of blocks on provided indexes
    private int ActualizeDepths(int left, int right, int blockDepth)
    {
        for (int i = left; i < right + (right - left); ++i)
        {
            blockDepth = ++_blockProps[i].BlockDepth;
        }

        return blockDepth;
    }

    public void DoubleAddresses() {
        using (var memoryStream = new MemoryStream())
        { 
            
            int actualLength =  _blockProps.Length;
            BlockProps[] newBlockProps = new BlockProps[2*actualLength];

            BlockProps blockProps = new BlockProps(-1, 0, 0);

            for(int i = 0; i < 2*actualLength; ++i ) {
                newBlockProps[i] = blockProps.CreateInstance();
            }

            int hashValZero;
            int hashValOne;

            for(int i = 0; i < actualLength; ++i) {
                memoryStream.SetLength(0);
                memoryStream.Write(BitConverter.GetBytes(i), 0, sizeof(int));
                byte[] bytes = memoryStream.ToArray();

                Array.Reverse(bytes);

                hashValZero = HashAddBitZero(bytes, _actualDepth, 0, false);
                hashValOne = HashAddBitOne(bytes, _actualDepth, 0, false);
                
                newBlockProps[hashValZero].Address = _blockProps[i].Address;
                newBlockProps[hashValOne].Address = _blockProps[i].Address;

                newBlockProps[hashValZero].BlockDepth = _blockProps[i].BlockDepth;
                newBlockProps[hashValOne].BlockDepth = _blockProps[i].BlockDepth;

                newBlockProps[hashValZero].ValidCount = _blockProps[i].ValidCount;
                newBlockProps[hashValOne].ValidCount = _blockProps[i].ValidCount;

            }

            _blockProps = newBlockProps;
            ++_actualDepth;
        }
    }

    public IEnumerable<Block<T>?> SequenceIterate(){

        foreach(var prop in _blockProps) {
            
            if(prop.Address != -1) {
                _heapFile.ReadBlock(prop.Address);
                yield return _heapFile.Block;
            } else {
                yield return default;
            }

        }
    }

    public void CloseFile() {
        _heapFile.CloseFile();
    }
}
