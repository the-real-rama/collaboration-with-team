private Task InitializeAddressDataAsync()
{
    if (Context is ValueTuple<List<Address>, List<ContactAddress>> tuple)
    {
        Addresses = tuple.Addresses.Select(a => (Address)a.Clone()).ToList();
        ContactAddresses = tuple.ContactAddresses.Select(ca => (ContactAddress)ca.Clone()).ToList();
    }
    else
    {
        Addresses = new List<Address>();
        ContactAddresses = new List<ContactAddress>();
    }
    // ...
}
