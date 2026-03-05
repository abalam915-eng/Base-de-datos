using UTM_Market.Core.Entities;

namespace UTM_Market.Core.Filters;

/// <summary>
/// Defines the domain criteria for searching sales records.
/// Implementation uses C# 14 'field' keyword for property-level validation.
/// </summary>
public sealed class SaleFilter
{
    /// <summary>
    /// Filter for sales that occurred on or after this date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter for sales that occurred on or before this date.
    /// Includes validation to ensure the end date is not before the start date.
    /// </summary>
    public DateTime? EndDate
    {
        get => field;
        set
        {
            if (value < StartDate)
            {
                throw new ArgumentException("End date cannot be earlier than start date.", nameof(EndDate));
            }
            field = value;
        }
    }

    /// <summary>
    /// Filter by the status of the sale (e.g., Completed, Canceled).
    /// </summary>
    public SaleStatus? Status { get; set; }

    /// <summary>
    /// Optional: Filter by specific customer email.
    /// This property allows for future extensibility as per requirements.
    /// </summary>
    public string? CustomerEmail { get; set; }
}
