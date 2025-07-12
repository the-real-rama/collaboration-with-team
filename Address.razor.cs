using DialogForms.Components.Enums;
using GMIS.Models.Entities;
using GMIS.Models.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace GMIS.Web.Components.Pages.Group.ContactViews
{
    public partial class AddressStep : ComponentBase
    {
        private EditContext EditContext { get; set; }
        private List<Address> Model = [];
        private Address CurrentAddr = new Address();
        private List<Address> Addresses = [];
        private List<ContactAddress> ContactAddresses = [];
        //private ContactAddress CurrentContactAddr = new ContactAddress();
        private PaginationState AddressPagination = new PaginationState();
        private IQueryable<VwListDropDown> AddressTypes { get; set; } = Enumerable.Empty<VwListDropDown>().AsQueryable();
        private bool IsEditMode = false;
        private ContactAddress ContactAddrModel { get; set; } = new ContactAddress();
        private EditContext CAddrEditContext { get; set; }
        private List<string> MergedValidationMsgs { get; set; } = new List<string>();
        private bool ViewMode { get; set; } = false;
        [Parameter] public ActionTypes ActionType { get; set; }
        [Parameter] public object? Context { get; set; }
        [Parameter] public EventCallback<(List<Address> Addresses, List<ContactAddress> ContactAddresses)> OnValidSubmitCallback { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalParameters { get; set; }
        //[Parameter][JsonIgnore] public StepWizard.Components.Controls.StepWizard Wizard { get; set; }
        //[Parameter][JsonIgnore] public string StepName { get; set; } = "";
        [CascadingParameter] private UserToken User { get; set; }
        [CascadingParameter] private IQueryable<VwListDropDown> CodeTypes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await InitializeAddressDataAsync();

            if (ContactAddrModel == null)
                ContactAddrModel = new ContactAddress { ModifiedBy = User?.UserName };

            if (CurrentAddr == null)
                CurrentAddr = new Address { ModifiedBy = User?.UserName };

            SetupContactAddressEditContext();

            EditContext ??= new EditContext(CurrentAddr);

            await GetAddressTypesAsync();
        }
        private Task InitializeAddressDataAsync()
        {
            if (Context is ValueTuple<List<Address>, List<ContactAddress>> tuple)
            {
                Addresses = tuple.Item1.Select(a => (Address)a.Clone()).ToList();
                ContactAddresses = tuple.Item2.Select(ca => (ContactAddress)ca.Clone()).ToList();
            }
            else
            {
                Addresses = new List<Address>();
                ContactAddresses = new List<ContactAddress>();
            }

            return Task.CompletedTask;
        }
        private void SetupContactAddressEditContext()
        {
            if (ContactAddrModel == null)
            {
                ContactAddrModel = new ContactAddress { ModifiedBy = User?.UserName };
            }

            if (CAddrEditContext == null || CAddrEditContext.Model != ContactAddrModel)
            {
                CAddrEditContext = new EditContext(ContactAddrModel);
                CAddrEditContext.OnValidationStateChanged += (sender, args) =>
                {
                    MergedValidationMsgs = CAddrEditContext.GetValidationMessages().ToList();
                    StateHasChanged();
                };
            }
        }
        private void ResetCurrentModels()
        {
            CurrentAddr = new Address { ModifiedBy = User?.UserName };
            EditContext = new EditContext(CurrentAddr);

            ContactAddrModel = new ContactAddress { ModifiedBy = User?.UserName };
            SetupContactAddressEditContext();

            StateHasChanged();
        }


        protected override async Task OnParametersSetAsync()
        {
            await InitializeAddressDataAsync();
            SetupContactAddressEditContext();
            await base.OnParametersSetAsync();
        }

        private async Task GetAddressTypesAsync()
        {
            AddressTypes = CodeTypes
                .Where(w => w.OptionKey.Trim().StartsWith(configuration.GetSection("DropDownTypes:AddressTypes").Value.ToString()));
        }

        private async Task HandleDone()
        {
            if (OnValidSubmitCallback.HasDelegate)
            {
                await OnValidSubmitCallback.InvokeAsync((Addresses, ContactAddresses));
            }
        }

        private void HandleAdd()
        {
            if (IsEditMode)
            {
                var index = Addresses.FindIndex(a => a == CurrentAddr);
                if (index != -1)
                {
                    Addresses[index] = CurrentAddr;
                    ContactAddresses[index] = ContactAddrModel;
                }
                IsEditMode = false;
            }
            else
            {
                Addresses.Add(CurrentAddr);
                ContactAddresses.Add(ContactAddrModel);
            }

            ResetCurrentModels();

            //if (OnValidSubmitCallback.HasDelegate)
            //{
            //    await OnValidSubmitCallback.InvokeAsync(Addresses);
            //}
        }

        private void OnEditClick(Address address)
        {
            var index = Addresses.FindIndex(a => a.AddressId == address.AddressId);

            if (index >= 0)
            {
                CurrentAddr = Addresses[index];
                EditContext = new EditContext(CurrentAddr);

                var existingContactAddr = ContactAddresses[index];
                ContactAddrModel = new ContactAddress
                {
                    ContactAddressId = existingContactAddr.ContactAddressId,
                    ContactId = existingContactAddr.ContactId,
                    AddressId = existingContactAddr.AddressId,
                    EffectiveDate = existingContactAddr.EffectiveDate,
                    TerminationDate = existingContactAddr.TerminationDate,
                    IsActive = existingContactAddr.IsActive,
                    ModifiedBy = existingContactAddr.ModifiedBy
                };

                SetupContactAddressEditContext();

                IsEditMode = true;
            }
        }

        private async Task OnDeleteClick(Address address)
        {
            var index = Addresses.FindIndex(a => a.AddressId == address.AddressId);

            if (index > 0)
            {
                Addresses.RemoveAt(index);
                ContactAddresses.RemoveAt(index);
            }

            if (OnValidSubmitCallback.HasDelegate)
            {
                await OnValidSubmitCallback.InvokeAsync((Addresses, ContactAddresses));
            }
        }

        private void HandleCancel()
        {
            ResetCurrentModels();
            IsEditMode = false;
            StateHasChanged(); 
        }

    }
}
