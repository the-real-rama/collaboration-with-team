private async Task LoadFullContactDataAsync(int contactId)
{
    // Load ContactDetail
    var detailResp = await apiClient.Get<BaseResponseModel, ContactDetail>(new ContactDetail { ContactId = contactId });
    CurrentContactDetail = detailResp?.Success == true 
        ? JsonConvert.DeserializeObject<ContactDetail>(detailResp.Data.ToString()) 
        : new ContactDetail();

    // Load Addresses
    var addressResp = await apiClient.GetAll<BaseResponseModel, Address>(new Address { ContactId = contactId });
    CurrentAddresses = addressResp?.Success == true
        ? JsonConvert.DeserializeObject<List<Address>>(addressResp.Data.ToString())
        : new List<Address>();

    // Load ContactAddresses
    var contactAddrResp = await apiClient.GetAll<BaseResponseModel, ContactAddress>(new ContactAddress { ContactId = contactId });
    CurrentContactAddresses = contactAddrResp?.Success == true
        ? JsonConvert.DeserializeObject<List<ContactAddress>>(contactAddrResp.Data.ToString())
        : new List<ContactAddress>();
}


private async Task OnDisplayClick(Contact contact)
{
    CurrentContact = contact;
    await LoadFullContactDataAsync(contact.ContactId);

    await DialogModal.ShowAsync(new ModalOption
    {
        Title = "View Contact",
        ChildComponent = typeof(ContactWizard),
        Parameters = new Dictionary<string, object>
        {
            { "ActionType", ActionTypes.View },
            { "ContactModel", CurrentContact },
            { "ContactDetailModel", CurrentContactDetail },
            { "AddressContext", (CurrentAddresses, CurrentContactAddresses) }
        },
        ModalSize = ModalSize.ExtraLarge,
        ShowBody = true
    });
}

private async Task OnEditClick(Contact contact)
{
    CurrentContact = contact;
    await LoadFullContactDataAsync(contact.ContactId);

    await DialogModal.ShowAsync(new ModalOption
    {
        Title = "Edit Contact",
        ChildComponent = typeof(EditPage),
        Parameters = new Dictionary<string, object>
        {
            { "Context", CurrentContact },
            { "ContactDetailModel", CurrentContactDetail },
            { "Addresses", CurrentAddresses },
            { "ContactAddresses", CurrentContactAddresses },
            { "ActionType", ActionTypes.Update },
            { "ButtonType", ButtonTypes.SaveCancel },
            { "OnSubmitCallback", EventCallback.Factory.Create<BaseResponseModel>(this, RefreshQuickGrid) }
        },
        ModalSize = ModalSize.ExtraLarge,
        ShowBody = true
    });
}



