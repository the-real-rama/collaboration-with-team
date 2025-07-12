// Assume Contacts is already populated with your contacts list

private List<ContactDetail> ContactDetails = new();
private List<ContactAddress> ContactAddresses = new();
private List<Address> Addresses = new();

private async Task LoadRelatedDataForContactsAsync()
{
    if (Contacts == null || Contacts.Count == 0)
        return; // No contacts, no related data to fetch

    var contactIds = Contacts.Select(c => c.ContactId).ToList();

    // 1. Fetch ContactDetails filtered by ContactIds
    var contactDetailsResp = await apiClient.GetAll<BaseResponseModel, ContactDetail>();
    if (contactDetailsResp != null && contactDetailsResp.Success)
    {
        var allDetails = JsonConvert.DeserializeObject<List<ContactDetail>>(contactDetailsResp.Data.ToString());
        ContactDetails = allDetails.Where(d => contactIds.Contains(d.ContactId)).ToList();
    }
    else
    {
        ContactDetails = new List<ContactDetail>();
    }

    // 2. Fetch ContactAddresses filtered by ContactIds
    var contactAddrResp = await apiClient.GetAll<BaseResponseModel, ContactAddress>();
    if (contactAddrResp != null && contactAddrResp.Success)
    {
        var allContactAddresses = JsonConvert.DeserializeObject<List<ContactAddress>>(contactAddrResp.Data.ToString());
        ContactAddresses = allContactAddresses.Where(ca => contactIds.Contains(ca.ContactId)).ToList();
    }
    else
    {
        ContactAddresses = new List<ContactAddress>();
    }

    // 3. From filtered ContactAddresses get AddressIds to filter Addresses
    var addressIds = ContactAddresses.Select(ca => ca.AddressId).Distinct().ToList();

    // 4. Fetch all Addresses and filter by those AddressIds
    var addressesResp = await apiClient.GetAll<BaseResponseModel, Address>();
    if (addressesResp != null && addressesResp.Success)
    {
        var allAddresses = JsonConvert.DeserializeObject<List<Address>>(addressesResp.Data.ToString());
        Addresses = allAddresses.Where(a => addressIds.Contains(a.AddressId)).ToList();
    }
    else
    {
        Addresses = new List<Address>();
    }
}
