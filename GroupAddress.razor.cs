  protected override async Task OnInitializedAsync()
  {
      ViewMode = ActionType.Equals(ActionTypes.View);
      ResetAddressForm();
      await GetAddressTypesAsync();
  }

  private void ResetAddressForm()
  {
      // Assign the context to originalModel to keep a reference to the original data.
      // This is necessary because when the user opens the dialog form and modifies something,
      // we need to ensure that any changes can be reverted if the user clicks cancel.
      // Directly assigning context to Model would result in the modified data being stored in cache,
      // which is not desirable when the user cancels the operation.
      // Therefore, we clone the original object to a new instance (Model)
      // Cloning the original object to a new instance (Model).
      // This prevents the modified data from being stored in cache when the user cancels the operation.
      Address originalAddrModel;
      GroupAddress originalGrpAddrModel;

      if (Context != null)
      {
          var ctx = (dynamic)Context;
          originalAddrModel = ctx.Address as Address ?? new Address();
          originalGrpAddrModel = ctx.GroupAddress as GroupAddress ?? new GroupAddress();
      }
      else
      {
          originalAddrModel = new Address();
          originalGrpAddrModel = new GroupAddress();
      }

      originalAddrModel.Country = "United States";
      originalAddrModel.ModifiedBy = User?.UserName;

      Model = (Address)originalAddrModel.Clone();
      EditContext = new EditContext(Model);

      originalGrpAddrModel.ModifiedBy = User?.UserName;
      GroupAddrModel = (GroupAddress)originalGrpAddrModel.Clone();
      GroupEditContext = new EditContext(GroupAddrModel);

      GroupEditContext.OnValidationStateChanged += (sender, args) => MergeValidationMessages();
      EditContext.OnValidationStateChanged += (sender, args) => MergeValidationMessages();
  }

  private void MergeValidationMessages()
  {
      MergedValidationMsgs.Clear();
      MergedValidationMsgs.AddRange(GroupEditContext.GetValidationMessages());
      MergedValidationMsgs.AddRange(EditContext.GetValidationMessages());
  }
