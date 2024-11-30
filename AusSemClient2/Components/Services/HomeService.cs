class HomeService
{
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
        _wholeCustomers = new HeapFile<Customer>(9000, customer.CreateInstance(), "Customers");

        CustomerById customerById = new();
        _customersById = new ExtendibleHash<CustomerById>(500, customerById.CreateInstance(), "CustomerById");

        CustomerByEcv customerByEcv = new();
        _customersByEcv = new ExtendibleHash<CustomerByEcv>(500, customerByEcv.CreateInstance(), "CustomerByEcv");
    }

    public void Add(int id, string ecv, string name, string lastName) {
        long address = -1;

        Customer customer = new(id, ecv, name, lastName);
        address = _wholeCustomers.Insert(customer);

        CustomerById customerById = new(id, ecv, address);
        _customersById.Insert(customerById);

        CustomerByEcv customerByEcv = new(id, ecv, address);
        _customersByEcv.Insert(customerByEcv);

    }

    public Customer FindById(int id) {
        _customerByIdFind.Id = id;
        CustomerById customerByIdFound = _customersById.Find(_customerByIdFind);

        _customerFind.Id = customerByIdFound.Id;
        _customerFind.Ecv = customerByIdFound.Ecv;
        Customer customerFound = _wholeCustomers.Get(customerByIdFound.Address, _customerFind);
        
        Customer returnCustomer = new();
        returnCustomer = returnCustomer.CreateInstance();
        returnCustomer.CopyFrom(customerFound);
        
        return returnCustomer;

    }

    public Customer FindByEcv(string ecv) {
        _customerByEcvFind.Ecv = ecv;
        CustomerByEcv customerByEcvFound = _customersByEcv.Find(_customerByEcvFind);

        _customerFind.Id = customerByEcvFound.Id;
        _customerFind.Ecv = customerByEcvFound.Ecv;
        Customer customerFound = _wholeCustomers.Get(customerByEcvFound.Address, _customerFind);
        
        Customer returnCustomer = new();
        returnCustomer.CopyFrom(customerFound);
        
        return returnCustomer;

    }

}