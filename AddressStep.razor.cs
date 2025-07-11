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
        private Address Current = new Address();
        private List<Address> Addresses = [];
        private PaginationState AddressPagination = new PaginationState();
        private IQueryable<VwListDropDown> AddressTypes { get; set; } = Enumerable.Empty<VwListDropDown>().AsQueryable();
        private bool IsEditMode = false;

        [Parameter] public ActionTypes ActionType { get; set; }
        [Parameter] public object? Context { get; set; }
        [Parameter] public EventCallback<List<Address>> OnValidSubmitCallback { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalParameters { get; set; }
        //[Parameter][JsonIgnore] public StepWizard.Components.Controls.StepWizard Wizard { get; set; }
        //[Parameter][JsonIgnore] public string StepName { get; set; } = "";
        [CascadingParameter] private UserToken User { get; set; }
        [CascadingParameter] private IQueryable<VwListDropDown> CodeTypes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InitializeAddressDataAsync();
            await GetAddressTypesAsync();
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
                Addresses = [];
            }

            Current = new Address();
            Current.ModifiedBy = User?.UserName;
            EditContext = new EditContext(Current);
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

        private void HandleAdd()
        {
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

            //if (OnValidSubmitCallback.HasDelegate)
            //{
            //    await OnValidSubmitCallback.InvokeAsync(Addresses);
            //}
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

    }
}
