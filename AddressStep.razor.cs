using DialogForms.Components.Enums;
using GMIS.Models.Entities;
using GMIS.Models.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.QuickGrid;
using System.ComponentModel.DataAnnotations;

namespace GMIS.Web.Components.Pages.Group.ContactViews
{
    public partial class AddressStep : ComponentBase
    {
        private EditContext EditContext { get; set; }
        private List<Address> Model = new List<Address>();
        private Address Current = new Address();
        private List<Address> Addresses = new List<Address>();
        private PaginationState AddressPagination = new PaginationState();
        private IQueryable<VwListDropDown> AddressTypes { get; set; } = Enumerable.Empty<VwListDropDown>().AsQueryable();
        private bool IsEditMode = false;

        [Parameter] public ActionTypes ActionType { get; set; }
        [Parameter] public object? Context { get; set; }
        [Parameter] public EventCallback<List<Address>> OnValidSubmitCallback { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalParameters { get; set; }
        [CascadingParameter] private UserToken User { get; set; }
        [CascadingParameter] private IQueryable<VwListDropDown> CodeTypes { get; set; }

        // New properties for ContactAddress editing and validation
        private ContactAddress ContactAddrModel { get; set; } = new ContactAddress();
        private EditContext CAddrEditContext { get; set; }
        private List<string> MergedValidationMsgs { get; set; } = new List<string>();
        private bool ViewMode { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InitializeAddressDataAsync();
            await GetAddressTypesAsync();

            // Initialize EditContext for ContactAddrModel with validation listener
            CAddrEditContext = new EditContext(ContactAddrModel);
            CAddrEditContext.OnValidationStateChanged += (sender, args) =>
            {
                MergedValidationMsgs = CAddrEditContext.GetValidationMessages().ToList();
                StateHasChanged();
            };
        }

        protected override async Task OnParametersSetAsync()
        {
            await InitializeAddressDataAsync();
            await base.OnParametersSetAsync();
        }

        private Task InitializeAddressDataAsync()
        {
            if (Context is List<Address> existingAddresses)
            {
                Addresses = existingAddresses.Select(a => (Address)a.Clone()).ToList();
            }
            else
            {
                Addresses = new List<Address>();
            }

            Current = new Address();
            Current.ModifiedBy = User?.UserName;
            EditContext = new EditContext(Current);

            // Reset ContactAddrModel and EditContext if needed
            ContactAddrModel = new ContactAddress();
            CAddrEditContext = new EditContext(ContactAddrModel);

            return Task.CompletedTask;
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
                await OnValidSubmitCallback.InvokeAsync(Addresses);
            }
        }

        private async Task HandleAdd()
        {
            // Validate ContactAddrModel dates
            if (!ValidateContactAddressDates())
            {
                return; // don't proceed if validation fails
            }

            if (IsEditMode)
            {
                var index = Addresses.FindIndex(a => a == Current);
                if (index != -1)
                {
                    Addresses[index] = Current;
                }
                IsEditMode = false;
            }
            else
            {
                if (!Addresses.Contains(Current))
                {
                    Addresses.Add(Current);
                }
            }

            Current = new Address();
            Current.ModifiedBy = User?.UserName;
            EditContext = new EditContext(Current);

            if (OnValidSubmitCallback.HasDelegate)
            {
                await OnValidSubmitCallback.InvokeAsync(Addresses);
            }
        }

        private void OnEditClick(Address address)
        {
            Current = address;
            EditContext = new EditContext(Current);

            IsEditMode = true;
        }

        private async Task OnDeleteClick(Address address)
        {
            Addresses.Remove(address);

            if (OnValidSubmitCallback.HasDelegate)
            {
                await OnValidSubmitCallback.InvokeAsync(Addresses);
            }
        }

        private void HandleCancel()
        {
            Current = new Address();
            EditContext = new EditContext(Current);
            IsEditMode = false;
            StateHasChanged();
        }

        private bool ValidateContactAddressDates()
        {
            MergedValidationMsgs.Clear();

            if (ContactAddrModel.EffectiveDate == null)
            {
                MergedValidationMsgs.Add("Effective Date is required.");
            }
            if (ContactAddrModel.TerminationDate != null && ContactAddrModel.TerminationDate <= ContactAddrModel.EffectiveDate)
            {
                MergedValidationMsgs.Add("Termination Date must be after Effective Date.");
            }

            return MergedValidationMsgs.Count == 0;
        }
    }

    public class ContactAddress
    {
        public int ContactAddressId { get; set; }
        public int ContactId { get; set; }
        public int ContactDetailId { get; set; }
        public int AddressId { get; set; }
        public bool IsActive { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? TerminationDate { get; set; }
    }
}
