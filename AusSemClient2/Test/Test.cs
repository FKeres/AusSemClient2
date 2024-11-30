using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

class Test
{
    private readonly Random _random;
    private int _operationsNum;
    //private HeapFile<Dummy> _heapFile;
    private HeapFile<Customer> _heapFile;

    public Test(int operationsNum, int heapSize, string filePath) {
        //Dummy dummy = new();
        //_heapFile = new(heapSize, dummy.CreateInstance(), filePath);
        Customer cust = new();
        _heapFile = new(heapSize, cust.CreateInstance(), filePath);
        _operationsNum = operationsNum;
        _random = new Random();
    }

    public Test(int operationsNum, int seed, int heapSize, string filePath) {
        //Dummy dummy = new();
        //_heapFile = new(heapSize, dummy.CreateInstance(), filePath);
        Customer cust = new();
        _heapFile = new(heapSize, cust.CreateInstance(), filePath);
        _operationsNum = operationsNum;
        _random = new Random(seed);
    }

    public void FirstTest() {
        List<long> addresses = new();

        for(int i = 0; i < 100; ++i) {
            //Dummy dummy = GenerateDummy(i);
            //addresses.Add(_heapFile.Insert(dummy));
            Customer cust = GenerateCustomer(i);
            addresses.Add(_heapFile.Insert(cust));
        }

        foreach(var block in _heapFile.SequenceIterate()) {
            Console.WriteLine($"adresa bloku - {block.Address} ");
            int i = 0;
            foreach(var item in block.Records) {
                Console.WriteLine($"record {i} bloku - {item.Id} ");
                i++;
            }
        }
    }

    /*
    public bool TestOperations() {
        int operation;

        Stopwatch stopwatch = new Stopwatch();
        List<long> addresses = new();
        List<Dummy> dummys = new(); 

        int j = 0;
        int i = 0;
        while(i < _operationsNum) {
            operation = GenerateOperation();
            if(operation == 1) {
                stopwatch.Start();
                Dummy dummy = GenerateDummy(j);
                Dummy dummyInList = new();
                dummyInList.Id = dummy.Id;
                dummys.Add(dummyInList);
                addresses.Add(_heapFile.Insert(dummy));
                stopwatch.Stop();
                ++j;
            } else if (operation == 0 ){
                stopwatch.Start();
                stopwatch.Stop();
            } else {
                stopwatch.Start();
                if(dummys.Count > 0) {
                    int randIndex = _random.Next(dummys.Count);
                    _heapFile.Remove(addresses[randIndex], dummys[randIndex]);
                    dummys.RemoveAt(randIndex);
                    addresses.RemoveAt(randIndex);
                }
                stopwatch.Stop();
            }
            ++i;
        }

        Console.WriteLine("sequence start");
        stopwatch.Start();
        int m = 0;
        foreach(var block in _heapFile.SequenceIterate()) {
            Console.WriteLine($"adresa bloku - {block.Address} ");
            for(int k = 0; k < block.ValidCount; ++k) {
                Console.WriteLine($"record {k} bloku - {block.Records[k].Id} ");
            }
            ++m;
        }

        for(int c = 0; c < dummys.Count; ++c) {
            _heapFile.Get(addresses[c], dummys[c]);
        }
        stopwatch.Stop();
        Console.WriteLine("sequence done : " + stopwatch.Elapsed);

        _heapFile.CloseFile();

        return false;
    }
    */

    public bool TestOperationsKont() {
        int operation;

        Stopwatch stopwatch = new Stopwatch();
        List<long> addresses = new();
        List<Customer> customers = new(); 

        int j = 0;
        int i = 0;
        while(i < _operationsNum) {
            operation = GenerateOperation();
            if(operation == 1) {
                stopwatch.Start();
                Customer customer = GenerateCustomer(j);
                Customer customerInList = new Customer(customer.Id, customer.Name, customer.LastName);

                for(int x = 0; x < customerInList.ServiceVisit.Length; ++x) {
                    customerInList.ServiceVisit[x].Id = customer.ServiceVisit[x].Id;
                    customerInList.ServiceVisit[x].Price = customer.ServiceVisit[x].Price;
                    customerInList.ServiceVisit[x].Description = customer.ServiceVisit[x].Description;
                    customerInList.ServiceVisit[x].Date = customer.ServiceVisit[x].Date;
                }

                /*
                int c = 0;
                Console.WriteLine($">>>>>>>>>>>>>>>>>>>>>>ITERATION<<<<<<<<<<<<<<<<<<<<");
                Console.WriteLine($">>  VKLADANY - {customer.Id} - {Convert.ToString(customer.Id, 2).PadLeft(32, '0')} <  <<<<<<<<<<");
                Console.WriteLine($">>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<");
                Console.WriteLine();
                foreach(var block in _heapFile.SequenceIterate()) {
                    Console.WriteLine($"*******************************************");
                    if(block is not null) {
                        Console.WriteLine($"adresa bloku - {block.Address} predchodca - {block.Previous} nasledovnik - {block.Next} valid Count - {block.ValidCount}  ");
                        
                        for(int k = 0; k < block.ValidCount; ++k) {
                            Console.WriteLine($"    {block.Records[k].Id} {block.Records[k].Name} {block.Records[k].LastName} {block.Records[k].ValidServiceNum}");
                        }

                    } else {
                        Console.WriteLine($"Index {c} - adresa bloku - -1 ");
                    }
                    
                    Console.WriteLine($"*******************************************");
                    Console.WriteLine();
                    
                    ++c;
                }
                */
                
                customers.Add(customerInList);
                addresses.Add(_heapFile.Insert(customer));
                stopwatch.Stop();
                ++j;
            } else if (operation == 0 ){
                stopwatch.Start();
                stopwatch.Stop();
            } else {
                stopwatch.Start();
                if(customers.Count > 0) {
                    int randIndex = _random.Next(customers.Count);
                    _heapFile.Remove(addresses[randIndex], customers[randIndex]);
                    customers.RemoveAt(randIndex);
                    addresses.RemoveAt(randIndex);
                }
                stopwatch.Stop();
            }
            ++i;
        }

        Console.WriteLine("FIND START");

        for(int c = 0; c < customers.Count; ++c) {
            var foundCustomer = _heapFile.Get(addresses[c], customers[c]);
            if(foundCustomer.ToStringFull() != customers[c].ToStringFull()) {
                throw new Exception($"This customer {customers[c].ToStringFull()} has not the same fields as {foundCustomer.ToStringFull()}");
            }
        }

        Console.WriteLine("FIND FINISH");

        stopwatch.Stop();

        _heapFile.CloseFile();

        return false;
    }

    public int GenerateOperation() {
        double number = _random.NextDouble();
        
        if(number < 0.7) {
            return 1;
        }
        else if(number >= 0.7 && number < 0.9) {
            return -1;
        } else {
            return 0;
        }
    }

    public Customer GenerateCustomer(int id) {
        string name = GenerateRandomString(5);
        string lastName = GenerateRandomString(10);
        Customer customer = new(id, name, lastName);

        for(int i = 0; i < customer.ServiceVisit.Length; ++i) {
            customer.AddServVisit(GenerateServVisit(i));
        }

        return customer;
    }

    public ServiceVisit GenerateServVisit(int id) {
        return new ServiceVisit(id, DateTime.Now, _random.NextDouble() * (1500.0 - 40.0) + 40.0, GenerateRandomString(18));
    }

    public Dummy GenerateDummy(int id) {
        
        string name = GenerateRandomString(2);

        int age = _random.Next(1, 81);

        double weight = _random.NextDouble() * (150.0 - 40.0) + 40.0;
        return new Dummy(id, name, age, weight);
    }

    private string GenerateRandomString(int length)
    {

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        StringBuilder stringBuilder = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            int index = _random.Next(chars.Length);
            stringBuilder.Append(chars[index]);
        }

        return stringBuilder.ToString();

    }

}