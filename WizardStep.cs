namespace StepWizard.Components.Models;

public class WizardStep
{
    /// <summary>
    ///     Display name or title for this step in the wizard
    /// </summary>
    public string StepName { get; set; }

    /// <summary>
    ///     The Blazor component to render for this step
    ///     (e.g. typeof(MyFormComponent))
    /// </summary>
    public Type ComponentType { get; set; }

    /// <summary>
    ///     Whether the user has completed this step
    /// </summary>
    public bool IsCompleted { get; set; }
    /// <summary>
    /// Gets or sets a collection of parameters represented as key-value pairs.
    /// </summary>
    /// <remarks>Use this property to store and retrieve parameters dynamically. The keys must be unique, and
    /// the values can be of any type.</remarks>
    public Dictionary<string, object> Parameters { get; set; } = new();
}
