class HomeService
{
    private HeapFile<Customer> _wholeCustomers;
    private ExtendibleHash<CustomerById> _customersById;
    private ExtendibleHash<CustomerByEcv> _customersByEcv;

    public HomeService() {
        Customer customer = new();
        _wholeCustomers = new HeapFile<Customer>(500, customer.CreateInstance(), "Customers");

        CustomerById customerById = new();
        _customersById = new ExtendibleHash<CustomerById>(500, customerById.CreateInstance(), "CustomerById");

        CustomerByEcv customerByEcv = new();
        _customersByEcv = new ExtendibleHash<CustomerByEcv>(500, customerByEcv.CreateInstance(), "CustomerByEcv");
    }

}