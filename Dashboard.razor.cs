using Blazored.Toast.Services;
using DialogForms.Components.Enums;
using DialogForms.Components.Models;
using GMIS.Models.Dto;
using GMIS.Models.Entities;
using GMIS.Models.Resp;
using GMIS.Models.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Newtonsoft.Json;
using StepWizard.Components.Models;
using System.Runtime.InteropServices;

namespace GMIS.Web.Components.Pages.Group.ContactViews;

public partial class Dashboard : ComponentBase
{
    private List<Contact> ContactList { get; set; } = new List<Contact>();
    private const string Title = "Contact with Address";
    protected PaginationState ContactPagination = new PaginationState { ItemsPerPage = 5 };

    private Contact CurrentContact { get; set; }
    private ContactDetail ContactDetails { get; set; }
    private List<Address> Addresses { get; set; }
    private List<ContactAddress> ContactAddresses { get; set; }
    [CascadingParameter] private UserToken User { get; set; }


    List<WizardStep> WizardSteps = new()
      {
        new WizardStep {
          StepName      = "Contact",
          ComponentType = typeof(EditPage)
        },
        new WizardStep {
          StepName      = "Contact Details",
          ComponentType = typeof(ContactDetailsViews.EditPage)
        },
        new WizardStep {
          StepName      = "Address",
          ComponentType = typeof(AddressViews.EditPage)
        }
    };
    protected override async Task OnInitializedAsync()
    {
        await GetContactsAsync();
    }

    private async Task LoadFullContactDataAsync(int contactId)
    {
        if (ContactList == null || ContactList.Count == 0)
            return;

        var contactIds = ContactList.Select(c => c.ContactId).ToList();

        var contactDetailsResp = await apiClient.GetAll<BaseResponseModel, ContactDetail>();
        if (contactDetailsResp != null && contactDetailsResp.Success)
        {
            var allDetails = JsonConvert.DeserializeObject<List<ContactDetail>>(contactDetailsResp.Data.ToString());
            ContactDetails = allDetails.Where(d => contactIds.Contains(d.ContactId)).FirstOrDefault();
        }
        else
        {
            ContactDetails = new ContactDetail();
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
        var addressIds = ContactAddresses
          .Where(ca => ca.ContactId == contactId)
          .Select(ca => ca.AddressId)
          .Distinct()
          .ToList();

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

    private async Task GetContactsAsync()
    {
        var res = await apiClient.GetAll<BaseResponseModel, Contact>();
        if (res != null && res.Success)
        {
            var result = JsonConvert.DeserializeObject<List<Contact>>(res.Data.ToString());
            ContactList.AddRange(result);
        }
    }

    private async Task OnAddClick()
    {
        await DialogModal.ShowAsync(new ModalOption()
        {
            Title = Title,
            ChildComponent = typeof(ContactWizard),
            Parameters = new()
            {
                { "ActionType", ActionTypes.Add },
                { "OnSubmitCallback", EventCallback.Factory.Create<BaseResponseModel>(this, RefreshQuickGrid) },
            },
            //ActionType = ActionTypes.Add,
            ModalSize = ModalSize.ExtraLarge,
            ShowBody = true
        });
        StateHasChanged();
    }

    private async Task OnDisplayClick(Contact contact)
    {
        CurrentContact = contact;
        await LoadFullContactDataAsync(contact.ContactId);

        await DialogModal.ShowAsync(new ModalOption
        {
            Title = "View Contact Details",
            ChildComponent = typeof(ContactWizard),
            Parameters = new Dictionary<string, object>
        {
            { "ActionType", ActionTypes.View },
            { "ContactModel", CurrentContact },
            { "ContactDetailModel", ContactDetails },
            { "Context", (Addresses, ContactAddresses) }
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
            { "ContactModel", CurrentContact },
            { "ContactDetailModel", ContactDetails },
            { "ContactAddresses", ContactAddresses },
            { "ActionType", ActionTypes.Update },
            { "Context", (Addresses, ContactAddresses) },
            { "OnSubmitCallback", EventCallback.Factory.Create<BaseResponseModel>(this, RefreshQuickGrid) }
        },
            ModalSize = ModalSize.ExtraLarge,
            ShowBody = true
        });
    }

    private async Task OnDeleteClick(Contact contextItem)
    {
        bool isDeleteConfirmed = await DialogModal.ShowConfirmationAsync("Confirmation", $"Are you sure?. Do you want to delete this record?");
        if (isDeleteConfirmed)
        {
            ContactDto contactItem = new ContactDto { ContactId = contextItem.ContactId, ModifiedBy = User?.UserName };
            BaseResponseModel res = await apiClient.Delete<BaseResponseModel, ContactDto>(contactItem);
            if (res != null)
            {
                res.Message = res.Success ? "Deleted Contact successfully." : $"Unable to delete Contact {res.Message}";
            }
            await RefreshQuickGrid(res);
        }
    }
    private async Task RefreshQuickGrid(BaseResponseModel responseModel)
    {
        if (responseModel != null)
        {
            ContactList = [];
            await GetContactsAsync();
            toastService.ShowToast(responseModel.Success ? responseModel.StatusCode.Equals(304) ? ToastLevel.Warning : ToastLevel.Success : ToastLevel.Error, responseModel.Message);
            StateHasChanged();
            DialogModal.OnClose();

        }
    }
}
