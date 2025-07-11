private void SetupWizardSteps()
{
    WizardSteps = new List<WizardStep>
    {
        // ... Contact and ContactDetails steps unchanged ...
        new WizardStep
        {
            StepName = "Address",
            ComponentType = typeof(AddressStep),
            Parameters = new Dictionary<string, object>
            {
                { "Context", (Addresses: AddressModel, ContactAddresses: ContactAddressModel) },
                { nameof(AddressStep.OnValidSubmitCallback), EventCallback.Factory.Create<(List<Address>, List<ContactAddress>)>(this, OnAddressStepValidSubmit) }
            }
        }
    };
}
