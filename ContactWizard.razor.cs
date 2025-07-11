private async Task OnWizardCompleted(Dictionary<string, object> results)
{
    var contact = results["Contact"] as Contact;
    var details = results["Contact Details"] as ContactDetail;
    var addresses = results["Addresses"] as List<Address>;

    if (contact == null || details == null || addresses == null)
    {
        // Handle missing data error
        return;
    }

    BaseResponseModel responseModel;

    if (ActionType == ActionTypes.Add)
    {
        // Insert Contact
        responseModel = await apiClient.Add<BaseResponseModel, Contact>(contact);
        if (responseModel?.Success == true)
        {
            var insertedContact = JsonConvert.DeserializeObject<Contact>(responseModel.Data.ToString());
            details.ContactId = insertedContact.ContactId;

            // Insert ContactDetail
            responseModel = await apiClient.Add<BaseResponseModel, ContactDetail>(details);
            if (responseModel?.Success == true)
            {
                var insertedDetail = JsonConvert.DeserializeObject<ContactDetail>(responseModel.Data.ToString());

                // Use LINQ to create a collection of tasks to insert Addresses and ContactAddresses
                var tasks = addresses.Select(async address =>
                {
                    // Insert Address
                    var addressResponse = await apiClient.Add<BaseResponseModel, Address>(address);
                    if (addressResponse?.Success == true)
                    {
                        var insertedAddress = JsonConvert.DeserializeObject<Address>(addressResponse.Data.ToString());

                        // Create ContactAddress linking Contact, ContactDetail, and Address
                        var contactAddress = new ContactAddress
                        {
                            ContactId = insertedContact.ContactId,
                            ContactDetailId = insertedDetail.ContactDetailId,
                            AddressId = insertedAddress.AddressId,
                            IsActive = true,
                            ModifiedBy = contact.ModifiedBy // or appropriate user
                        };

                        // Insert ContactAddress
                        var contactAddressResponse = await apiClient.Add<BaseResponseModel, ContactAddress>(contactAddress);
                        if (contactAddressResponse?.Success != true)
                        {
                            // Handle error inserting ContactAddress (optional: throw or log)
                        }
                    }
                    else
                    {
                        // Handle error inserting Address (optional: throw or log)
                    }
                });

                // Await all insert tasks to complete
                await Task.WhenAll(tasks);
            }
            else
            {
                // Handle error inserting ContactDetail
            }
        }
        else
        {
            // Handle error inserting Contact
        }
    }
    else if (ActionType == ActionTypes.Update)
    {
        // Similar update logic here...
    }
}
