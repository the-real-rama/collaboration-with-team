using DialogForms.Components.Enums;
using GMIS.Models.Entities;
using GMIS.Models.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GMIS.Web.Components.Pages.Group.ContactViews
{
    public partial class ContactDetailsStep : ComponentBase
    {
        public EditContext EditContext { get; set; }
        private ContactDetail CDetailModel = new();

        [Parameter] public ActionTypes ActionType { get; set; }
        [Parameter] public object? InitialData { get; set; }
        [Parameter] public EventCallback<ContactDetail> OnValidSubmitCallback { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> AdditionalParameters { get; set; }

        [CascadingParameter] private UserToken User { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (InitialData is ContactDetail existingDetail)
            {
                CDetailModel = (ContactDetail)existingDetail.Clone();
            }
            else
            {
                CDetailModel = new ContactDetail();
            }
            CDetailModel.ModifiedBy = User?.UserName;
            EditContext = new EditContext(CDetailModel);
        }

        private async Task OnValidSubmit()
        {
            if (OnValidSubmitCallback.HasDelegate)
            {
                await OnValidSubmitCallback.InvokeAsync(CDetailModel);
            }
        }
    }
}
