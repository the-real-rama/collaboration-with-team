<EditForm EditContext="@CAddrEditContext">
    <DataAnnotationsValidator />
    <div class="row px-2 pt-2">
    @if (MergedValidationMsgs?.Count > 0)
        {
            <ul class="alert alert-danger validateSummary">
                @foreach (var msg in MergedValidationMsgs)
                {
                    <li class="text-danger">
                        @msg
                    </li>
                }
            </ul>
        }
        <div class="col-12">
            <div class="d-flex justify-content-between">
                <div class="col-6 w-49">
                    <div class="input-group">
                        <div class="input-group-prepend">
                            <span class="input-group-text" id="EffectiveDate">Effective Date:</span>
                        </div>
                        <InputDate id="EffectiveDate" class="form-control" Type="InputDateType.Date" @bind-Value="ContactAddrModel.EffectiveDate" readonly="@ViewMode" />
                    </div>
                </div>
                <div class="col-6 w-49">
                    <div class="input-group">
                        <div class="input-group-prepend">
                            <span class="input-group-text" id="TerminationDate">Termination Date:</span>
                        </div>
                        <InputDate id="TerminationDate" class="form-control" Type="InputDateType.Date" @bind-Value="ContactAddrModel.TerminationDate" readonly="@ViewMode" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</EditForm>

<EditForm EditContext="EditContext" OnValidSubmit="HandleAdd">
    <AddressDetails Model="@CurrentAddr"
                    ActionType="@ActionType"
                    T="@GMIS.Models.Entities.Address"
                    AddressTypes="@AddressTypes"
                    EditContext="EditContext">
    </AddressDetails>
    <div class="d-flex justify-content-between px-2">
        <button type="submit" class="btn btn-success" title="Save">
            <span class="bi-floppy"></span>
        </button>
        @if (IsEditMode)
        {
            <button type="button" autofocus class="btn btn-primary" title="Close" @onclick="HandleCancel">
                <span class="bi-x-square"></span>
            </button>
        }
        <button type="button" class="btn btn-primary mt-2" @onclick="HandleDone">Done</button>
    </div>
</EditForm>


@if (Addresses.Count > 0)
{
    <div class="card pt-4">
        <div class="card-body">
            <div class="row">
                <QuickGrid TGridItem="Address" Items="@Addresses.AsQueryable()" Pagination="AddressPagination">
                    <PropertyColumn Property="@(a => a.AddressType)" Title="Address Type" Sortable="true" />
                    <PropertyColumn Property="@(a => a.Address1)" Title="Address" Sortable="true" />
                    <PropertyColumn Property="@(a => a.City)" Title="City" Sortable="true" />
                    <PropertyColumn Property="@(a => a.StateCode)" Title="State Code" Sortable="true" />
                    <PropertyColumn Property="@(a => a.County)" Title="County" Sortable="true" />
                    <PropertyColumn Property="@(a => a.ZipCode)" Title="Zip Code" Sortable="true" />
                    <TemplateColumn Title="Actions" Align="Align.Center" Context="ActionContext" Class="custom-width-10">
                        <UserAccess T="Address"
                                    ActionContext="ActionContext"
                                    OnEditClickCallback="OnEditClick"
                                    BtnActionType="@ActionTypes.Update" />
                        <UserAccess T="Address"
                                    ActionContext="ActionContext"
                                    OnDeleteClickCallback="OnDeleteClick"
                                    BtnActionType="@ActionTypes.Delete" />
                    </TemplateColumn>
                </QuickGrid>
                <Paginator State="@AddressPagination" />
            </div>
        </div>
    </div>
}
