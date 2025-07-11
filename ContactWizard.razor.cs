if (ActionType == ActionTypes.Add)
    {
        responseModel = await apiClient.Add<BaseResponseModel, Contact>(contact);
        if (responseModel != null && responseModel.Success)
        {
            var insertedContact = JsonConvert.DeserializeObject<Contact>(responseModel.Data.ToString());

            // 2. Use inserted contact ID to link ContactDetail
            details.ContactId = insertedContact.ContactId;

            responseModel = await apiClient.Add<BaseResponseModel, ContactDetail>(details);
            if (responseModel != null && responseModel.Success)
            {
                var insertedDetail = JsonConvert.DeserializeObject<ContactDetail>(responseModel.Data.ToString());

                // 3. Link addresses to contact
                var tasks = addresses.Select(async address =>
                {
                    address.ContactId = insertedContact.ContactId;
                    address.ContactDetailId = insertedDetail.ContactDetailId;
                    await apiClient.Add<BaseResponseModel, Address>(address);
                });
                await Task.WhenAll(tasks);
            }
        }
    }
