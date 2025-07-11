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

                // For each Address, insert Address and then ContactAddress linking it
                foreach (var address in addresses)
                {
                    // Insert Address
                    responseModel = await apiClient.Add<BaseResponseModel, Address>(address);
                    if (responseModel?.Success == true)
                    {
                        var insertedAddress = JsonConvert.DeserializeObject<Address>(responseModel.Data.ToString());

                        // Create ContactAddress linking Contact, ContactDetail, and Address
                        var contactAddress = new ContactAddress
                        {
                            ContactId = insertedContact.ContactId,
                            ContactDetailId = insertedDetail.ContactDetailId,
                            AddressId = insertedAddress.AddressId,
                            IsActive = true,
                            ModifiedBy = contact.ModifiedBy // or from context
                        };

                        // Insert ContactAddress
                        responseModel = await apiClient.Add<BaseResponseModel, ContactAddress>(contactAddress);
                        if (responseModel?.Success != true)
                        {
                            // Handle error inserting ContactAddress
                        }
                    }
                    else
                    {
                        // Handle error inserting Address
                    }
                }
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
        // Similar logic for updates...
    }
}
