using System.Diagnostics;
using System.Text;

class HomeService
{
    private readonly Random _random  =  new Random();

    private HeapFile<Customer> _wholeCustomers;
    private ExtendibleHash<CustomerById> _customersById;
    private ExtendibleHash<CustomerByEcv> _customersByEcv;

    private CustomerById _customerByIdFind;
    private CustomerByEcv _customerByEcvFind;
    private Customer _customerFind;

    public HomeService() {
        _customerByEcvFind = new(-1, "xxxxxxxxxx", -1);
        _customerByIdFind = new(-1, "xxxxxxxxxx", -1);
        
        Customer customer = new();

        _customerFind = customer.CreateInstance();
        _wholeCustomers = new HeapFile<Customer>(8000, customer.CreateInstance(), "Customers");

        CustomerById customerById = new();
        _customersById = new ExtendibleHash<CustomerById>(500, customerById.CreateInstance(), "CustomerById");

        CustomerByEcv customerByEcv = new();
        _customersByEcv = new ExtendibleHash<CustomerByEcv>(500, customerByEcv.CreateInstance(), "CustomerByEcv");
    }

    public string Add(int id, string ecv, string name, string lastName) {
        long address = -1;

        CustomerById customerById = new(id, ecv, address);
        CustomerByEcv customerByEcv = new(id, ecv, address);

        if(_customersById.Find(customerById) is not null) {
            return $"ID {id} already exists!";
        } else if (_customersByEcv.Find(customerByEcv) is not null ) {
            return $"ECV {ecv} already exists!";
        } else {
            Customer customer = new(id, ecv, name, lastName);
            address = _wholeCustomers.Insert(customer);

            customerById.Address = address;
            customerByEcv.Address = address;
    
            _customersById.Insert(customerById);
            _customersByEcv.Insert(customerByEcv);

            return "OK";
        }

    }

    public Customer? FindById(int id) {
        _customerByIdFind.Id = id;
        CustomerById customerByIdFound = _customersById.Find(_customerByIdFind);

        if(customerByIdFound is not null) {

            _customerFind.Id = customerByIdFound.Id;
            _customerFind.Ecv = customerByIdFound.Ecv;
            Customer customerFound = _wholeCustomers.Get(customerByIdFound.Address, _customerFind);
            
            Customer returnCustomer = new();
            returnCustomer = returnCustomer.CreateInstance();
            returnCustomer.CopyFrom(customerFound);
            
            return returnCustomer;
        } 

        return default;

    }

    public Customer? FindByEcv(string ecv) {
        _customerByEcvFind.Ecv = ecv;
        CustomerByEcv customerByEcvFound = _customersByEcv.Find(_customerByEcvFind);

        if(customerByEcvFound is not null) {

            _customerFind.Id = customerByEcvFound.Id;
            _customerFind.Ecv = customerByEcvFound.Ecv;
            Customer customerFound = _wholeCustomers.Get(customerByEcvFound.Address, _customerFind);
            
            Customer returnCustomer = new();
            returnCustomer = returnCustomer.CreateInstance();
            returnCustomer.CopyFrom(customerFound);
            
            return returnCustomer;
        }

        return default;

    }

    public void UpdateCustomer(Customer customer) {
        _customerByIdFind.Id = customer.Id;
        CustomerById customerByIdFound = _customersById.Find(_customerByIdFind);

        _wholeCustomers.Update(customerByIdFound.Address, customer);
    }

    public void Generate(int num) {
        int min = 0;
        int max = num;

        List<int> numbers = new List<int>();
        for (int c = min; c <= max; c++)
        {
            numbers.Add(c);
        }

        int k = 0;

        while(k < num) {
            int index = _random.Next(numbers.Count);
            int id = numbers[index];
            numbers.RemoveAt(index);

            Customer customer = GenerateCustomer(id);
            CustomerByEcv custEcv = new(customer.Id, customer.Ecv, -1);

            while (_customersByEcv.Find(custEcv) is not null)
            {
                custEcv.Ecv = GenerateRandomEcv();
            }

            customer.Ecv = custEcv.Ecv;
            
            long address = _wholeCustomers.Insert(customer);

            CustomerByEcv customerByEcv = new(customer.Id, customer.Ecv, address);
            CustomerById customerById = new(customer.Id, customer.Ecv, address);

            _customersById.Insert(customerById);

            _customersByEcv.Insert(customerByEcv);
            ++k;
        }
    }

    public Customer GenerateCustomer(int id) {
        string name = GenerateRandomString(5);
        string lastName = GenerateRandomString(10);
        string ecv = GenerateRandomEcv();
        Customer customer = new(id, ecv, name, lastName);

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

    private string GenerateRandomEcv()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_-+=<>?áéíóúýĺŕčďľňšťžÁÉÍÓÚÝĹŔČĎĽŇŠŤŽ";
        StringBuilder stringBuilder = new StringBuilder(10);

        for (int i = 0; i < 10; i++)
        {
            int index = _random.Next(chars.Length);
            stringBuilder.Append(chars[index]);
        }

        return stringBuilder.ToString();

    }

    public string ShowSequenceHeap() {
        int c = 0;
        string output = $"***** FirstEmpty = {_wholeCustomers.FirstEmpty} ***  FirstPartlyEmpty = {_wholeCustomers.FirstPartlyEmpty}\n";

        foreach(var block in _wholeCustomers.SequenceIterate()) {
            output += $"******************************NOVÝ BLOK********************************\n";
            if(block is not null) {
                output += $"adresa bloku - {block.Address} predchodca - {block.Previous} nasledovnik - {block.Next} valid Count - {block.ValidCount}  \n";
                
                for(int k = 0; k < block.ValidCount; ++k) {
                    output += $"------------------------------------------------------------------------\n";
                    output += $"    ID - {block.Records[k].Id} EČV - {block.Records[k].Ecv} Meno - {block.Records[k].Name} Priezvisko - {block.Records[k].LastName} \n";

                    for (int i = 0; i < block.Records[k].ValidServiceNum; i++)
                    {
                        output += $"        ServiceId - {block.Records[k].ServiceVisit[i].Id} Dátum - {block.Records[k].ServiceVisit[i].Date} Cena - {block.Records[k].ServiceVisit[i].Price}  \n";
                        
                        for (int j = 0; j < block.Records[k].ServiceVisit[i].ValidDesc; j++)
                        {
                            output += $"            Popis - {block.Records[k].ServiceVisit[i].Description[j]} \n";
                        }
                    }
                    output += $"------------------------------------------------------------------------\n";     
                }

            } else {
                output += $"Index {c} - adresa bloku - -1 \n";
            }
            
            output += $"********************************************************************\n";
            output += "\n";
            
            ++c;
        }

        return output;
    }

    public string ShowSequenceById() {
        int c = 0;
        string output = $"***** ActualDepth = {_customersById.ActualDepth} ********************************\n";

        foreach(var block in _customersById.SequenceIterate()) {
                    output += $"********************************************************************\n";
                    if(block is not null) {
                        output += $"Index {c} - adresa bloku - {block.Address} - block depth {_customersById.BlockProps[c].BlockDepth} \n";
                        for(int k = 0; k < block.ValidCount; ++k) {
                        int idc = block.Records[k].Id;

                        string binaryRepresentation = Convert.ToString(idc, 2).PadLeft(32, '0');

                        output += $"         {idc} ({binaryRepresentation}) \n";
                    }
                    } else {
                        output += $"Index {c} - adresa bloku - -1 \n";
                    }
                    
                    output += $"********************************************************************\n";
                    output += "\n";

                    ++c;
                }

        return output;
    }

    public string ShowSequenceByEcv() {
        int c = 0;
        string output = $"***** ActualDepth = {_customersById.ActualDepth} ********************************\n";

        foreach(var block in _customersById.SequenceIterate()) {
                    output += $"********************************************************************\n";
                    if(block is not null) {
                        output += $"Index {c} - adresa bloku - {block.Address} - block depth {_customersById.BlockProps[c].BlockDepth} \n";
                        for(int k = 0; k < block.ValidCount; ++k) {
                            string ecv = block.Records[k].Ecv;
                            StringBuilder ecvBinaryRepresentation = new StringBuilder();

                            foreach (char character in ecv)
                            {
                                int asciiValue = (int)character;
                                string binaryChar = Convert.ToString(asciiValue, 2).PadLeft(8, '0');
                                ecvBinaryRepresentation.Append($"{character} ({binaryChar}) ");
                            }

                            output += $"         {ecv} ({ecvBinaryRepresentation.ToString().TrimEnd()}) \n";
                        }
                    } else {
                        output += $"Index {c} - adresa bloku - -1 \n";
                    }
                    
                    output += $"********************************************************************\n";
                    output += "\n";

                    ++c;
                }

        return output;
    }

    public void SaveState() {
         using (StreamWriter writer = new StreamWriter("Metadata"))
        {
            writer.WriteLine(_wholeCustomers.GetHeader());
            writer.WriteLine(_wholeCustomers.GetBody());
            writer.WriteLine(_customersById.GetHeader());
            writer.WriteLine(_customersById.GetBody());
            writer.WriteLine(_customersByEcv.GetHeader());
            writer.WriteLine(_customersByEcv.GetBody());
        }

        _wholeCustomers.SaveState();
        _customersById.HeapFile.SaveState();
        _customersByEcv.HeapFile.SaveState();

    }

    public void LoadState() {
        using (StreamReader reader = new StreamReader("Metadata"))
        {
            string body;
            reader.ReadLine();
            body = reader.ReadLine();

            _wholeCustomers.Load(body);

            reader.ReadLine();
            body = reader.ReadLine();

            _customersById.Load(body);

            reader.ReadLine();
            body = reader.ReadLine();

            _customersByEcv.Load(body);

        }

        _wholeCustomers.LoadState();
        _customersById.HeapFile.LoadState();
        _customersByEcv.HeapFile.LoadState();
    }

}