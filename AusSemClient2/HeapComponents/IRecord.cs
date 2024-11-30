interface IRecord<T>
{
    public bool Equals();
    public int GetSize();
    public bool Equals(T other);
    public T CreateInstance();
    public byte[] GetByteArray();
    public void FromByteArray(byte[] bytes); 

}