using DialogForms.Components.Enums;
using GMIS.Models.Entities;
using GMIS.Models.Resp;
using GMIS.Models.User;
using GMIS.Web.Shared.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using StepWizard.Components.Models;

namespace GMIS.Web.Components.Pages.Group.ContactViews
{
    public partial class ContactWizard : ComponentBase
    {
        private List<WizardStep> WizardSteps { get; set; }
        private StepWizard.Components.Controls.StepWizard Wizard;
        [Parameter] public ActionTypes ActionType { get; set; }
        private Contact ContactModel = new Contact();
        private ContactDetail ContactDetailModel = new ContactDetail();
        private List<Address> AddressModel = new List<Address>();
        [CascadingParameter] private UserToken User { get; set; }

        protected override void OnInitialized()
        {
            SetupWizardSteps();
        }

        private void SetupWizardSteps()
        {
            WizardSteps = new List<WizardStep>
                {
                    new WizardStep
                    {
                        StepName = "Contact",
                        ComponentType = typeof(ContactStep),
                        Parameters = new Dictionary<string, object>
                        {
                            { "Context", ContactModel },
                            { nameof(ContactStep.OnValidSubmitCallback), EventCallback.Factory.Create<Contact>(this, OnContactStepValidSubmit) }
                        }
                    },
                    new WizardStep
                    {
                        StepName = "Contact Details",
                        ComponentType = typeof(ContactDetailsStep),
                        Parameters = new Dictionary<string, object>
                        {
                            { "InitialData", ContactDetailModel },
                            { nameof(ContactDetailsStep.OnValidSubmitCallback), EventCallback.Factory.Create<ContactDetail>(this, OnContactDetailsStepValidSubmit) }
                        }
                    },
                    new WizardStep
                    {
                        StepName = "Address",
                        ComponentType = typeof(AddressStep),
                        Parameters = new Dictionary<string, object>
                        {
                            { "Context", AddressModel },
                            { nameof(AddressStep.OnValidSubmitCallback), EventCallback.Factory.Create<List<Address>>(this, OnAddressStepValidSubmit) }
                        }
                    }
                };
        }

        private async Task OnWizardCompleted(Dictionary<string, object> results)
        {
            Contact contact = results["Contact"] as Contact;
            ContactDetail details = results["Contact Details"] as ContactDetail;
            List<Address> addresses = results["Addresses"] as List<Address>;

            if (contact == null || details == null || addresses == null)
            {
                return;
            }

            BaseResponseModel responseModel = new BaseResponseModel();

            if (ActionType == ActionTypes.Add)
            {
                contact.IsActive = true;
                responseModel = await apiClient.Add<BaseResponseModel, Contact>(contact);
                if (responseModel != null && responseModel.Success)
                {
                    var insertedContact = JsonConvert.DeserializeObject<Contact>(responseModel.Data.ToString());

                    details.ContactId = insertedContact.ContactId;
                    details.IsActive = true;
                    responseModel = await apiClient.Add<BaseResponseModel, ContactDetail>(details);
                    var insertedDetail = JsonConvert.DeserializeObject<ContactDetail>(responseModel.Data.ToString());

                    var tasks = addresses.Select(async address =>
                    {
                        address.IsActive = true;
                        var addressResponse = await apiClient.Add<BaseResponseModel, Address>(address);
                        if (addressResponse != null && addressResponse.Success)
                        {
                            var insertedAddress = JsonConvert.DeserializeObject<Address>(addressResponse.Data.ToString());

                            var contactAddress = new ContactAddress
                            {
                                ContactId = insertedContact.ContactId,
                                AddressId = insertedAddress.AddressId,
                                IsActive = true,
                                ModifiedBy = User?.UserName,
                            };

                            var contactAddressResponse = await apiClient.Add<BaseResponseModel, ContactAddress>(contactAddress);
                            if (contactAddressResponse?.Success != true)
                            {
                                // Handle error inserting ContactAddress (optional: throw or log)
                            }
                        }
                    });

                    await Task.WhenAll(tasks);
                }
            }
            else if (ActionType.Equals(ActionTypes.Update))
            {
                //responseModel = await apiClient.Update<BaseResponseModel, Contact>(contact);
                //if (responseModel != null)
                //{
                //    responseModel.Message = responseModel.Success ? "Contact updated successfully." : $"Unable to update Contact record. {responseModel.Message}";
                //}
            }
            //return Task.CompletedTask;
        }


        private async Task ProcessAddressAsync(BaseResponseModel responseModel, ApiClient apiClient, Address address)
        {
            address.IsActive = true;
            responseModel = await apiClient.Add<BaseResponseModel, Address>(address);
            if (responseModel != null)
            {
                responseModel.Message = responseModel.Success ? "Contact Detail added successfully." : $"Unable to add Contact Detail record. {responseModel.Message}";
            }
        }
        private void OnContactStepValidSubmit(Contact updatedContact)
        {
            ContactModel = updatedContact;
            SetupWizardSteps();
            StateHasChanged();
            Wizard.OnStepValidSubmit("Contact", updatedContact);
        }

        private void OnContactDetailsStepValidSubmit(ContactDetail updatedDetail)
        {
            ContactDetailModel = updatedDetail;
            SetupWizardSteps();
            StateHasChanged();
            Wizard.OnStepValidSubmit("Contact Details", updatedDetail);
        }

        private void OnAddressStepValidSubmit(List<Address> updatedAddresses)
        {
            AddressModel = updatedAddresses;
            SetupWizardSteps(); 
            StateHasChanged();
            Wizard.OnStepValidSubmit("Addresses", updatedAddresses);
        }

    }
}
