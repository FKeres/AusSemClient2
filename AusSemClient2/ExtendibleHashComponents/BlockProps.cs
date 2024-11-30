class BlockProps
{
    private long _address;
    private int _blockDepth;
    private int _validCount;

    public BlockProps(long addresses, int blockDepth, int validcount)
    {
        _address = addresses;
        _blockDepth = blockDepth;
        _validCount = validcount;
    }

    public long Address { get => _address; set => _address = value; }
    public int BlockDepth { get => _blockDepth; set => _blockDepth = value; }
    public int ValidCount { get => _validCount; set => _validCount = value; }

    public BlockProps CreateInstance() {
        return new BlockProps(-1, 0, 0);
    }
}