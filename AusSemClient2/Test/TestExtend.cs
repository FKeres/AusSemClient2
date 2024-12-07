using System.Diagnostics;
using System.Text;


class TestExtend
{
    private readonly Random _random;
    private int _operationsNum;
    private ExtendibleHash<Customer> _extendibleHash;

    public TestExtend(int operationsNum, int heapSize, string filePath) {
        Customer cust = new();
        _extendibleHash = new(heapSize, cust.CreateInstance(), filePath);
        _operationsNum = operationsNum;
        _random = new Random();
    }

    public TestExtend(int operationsNum, int seed, int heapSize, string filePath) {
        Customer cust = new();
        _extendibleHash = new(heapSize, cust.CreateInstance(), filePath);
        _operationsNum = operationsNum;
        _random = new Random(seed);
    }

    public string TestOperationsKont() {
        int operation;

        Stopwatch stopwatch = new Stopwatch();
        List<long> addresses = new();
        List<Customer> customers = new(); 

        int min = 0;
        int max = 3000;

        List<int> numbers = new List<int>();
        for (int c = min; c <= max; c++)
        {
            numbers.Add(c);
        }

        int j = 0;
        int i = 0;
        while(i < _operationsNum) {
            operation = GenerateOperation();
            if(operation == 1) {
                stopwatch.Start();

                int index = _random.Next(numbers.Count);
                int id = numbers[index];
                numbers.RemoveAt(index);

                Customer customer = GenerateCustomer(id);
                Customer customerInList = new Customer(customer.Id, "xxxxxxxxxx", customer.Name, customer.LastName);

                for(int x = 0; x < customerInList.ServiceVisit.Length; ++x) {
                    customerInList.ServiceVisit[x].Id = customer.ServiceVisit[x].Id;
                    customerInList.ServiceVisit[x].Price = customer.ServiceVisit[x].Price;
                    for(int y = 0; y < customerInList.ServiceVisit[x].Description.Length; ++y){
                        customerInList.ServiceVisit[x].Description[y] = customer.ServiceVisit[x].Description[y];
                    } 
                    customerInList.ServiceVisit[x].Date = customer.ServiceVisit[x].Date;
                }

                customers.Add(customerInList);
                _extendibleHash.Insert(customer);
                /*
                int c = 0;
                Console.WriteLine($">>>>>>>>>>>>>>>>>>>>>>ITERATION<<<<<<<<<<<<<<<<<<<<");
                Console.WriteLine($">>  VKLADANY - {customer.Id} - {Convert.ToString(customer.Id, 2).PadLeft(32, '0')} <  <<<<<<<<<<");
                Console.WriteLine($">>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<");
                Console.WriteLine();
                foreach(var block in _extendibleHash.SequenceIterate()) {
                    Console.WriteLine($"*******************************************");
                    if(block is not null) {
                        Console.WriteLine($"Index {c} - adresa bloku - {block.Address} - block depth {_extendibleHash.BlockProps[c].BlockDepth} ");
                        for(int k = 0; k < block.ValidCount; ++k) {
                        //Console.WriteLine($"         {block.Records[k].Id} ");
                        int idc = block.Records[k].Id;

                        string binaryRepresentation = Convert.ToString(idc, 2).PadLeft(32, '0');

                        Console.WriteLine($"         {idc} ({binaryRepresentation})");
                    }
                    } else {
                        Console.WriteLine($"Index {c} - adresa bloku - -1 ");
                    }
                    
                    Console.WriteLine($"*******************************************");
                    Console.WriteLine();
                    
                    ++c;
                }
                */
                stopwatch.Stop();
                ++j;
            } else if (operation == 0 ){
                stopwatch.Start();
                stopwatch.Stop();
            } else {
                stopwatch.Start();
                if(customers.Count > 0) {
                    /*
                    int randIndex = _random.Next(customers.Count);
                    _heapFile.Remove(addresses[randIndex], customers[randIndex]);
                    customers.RemoveAt(randIndex);
                    addresses.RemoveAt(randIndex);
                    */
                }
                stopwatch.Stop();
            }

            if(i%1000 == 0) {

                for(int c = 0; c < customers.Count; ++c) {
                    var foundCustomer = _extendibleHash.Find(customers[c]);
                    if(foundCustomer.ToStringFull() != customers[c].ToStringFull()) {
                        return $"This customer {customers[c].ToStringFull()} has not the same fields as {foundCustomer.ToStringFull()}";
                    }
                }

            }
            ++i;
        }

        Console.WriteLine("FIND START");
        for(int c = 0; c < customers.Count; ++c) {
            var foundCustomer = _extendibleHash.Find(customers[c]);
            if(foundCustomer.ToStringFull() != customers[c].ToStringFull()) {
                return $"This customer {customers[c].ToStringFull()} has not the same fields as {foundCustomer.ToStringFull()}";
            }
        }
        Console.WriteLine("FIND FINISH");

        _extendibleHash.CloseFile();

        return $"Test finished Sucessfully";
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
        Customer customer = new(id, "xxxxxxxxxx", name, lastName);

        for(int i = 0; i < customer.ServiceVisit.Length; ++i) {
            customer.AddServVisit(GenerateServVisit(i));
            for(int j = 0; j < customer.ServiceVisit[i].Description.Length; ++j){
                customer.ServiceVisit[i].AddDescription(GenerateRandomString(11));
            }
        }

        return customer;
    }

    public ServiceVisit GenerateServVisit(int id) {
        return new ServiceVisit(id, DateTime.Now, _random.NextDouble() * (1500.0 - 40.0) + 40.0);
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