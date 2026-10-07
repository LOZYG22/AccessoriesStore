using System.Text.Json.Serialization;

namespace AccessoriesStore.Application.DTOs.Payments;

public class PaymobTransactionCallbackRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("obj")]
    public PaymobTransactionObject Obj { get; set; } = new();
}

public class PaymobTransactionObject
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("amount_cents")]
    public long AmountCents { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error_occured")]
    public bool ErrorOccured { get; set; }

    [JsonPropertyName("has_parent_transaction")]
    public bool HasParentTransaction { get; set; }

    [JsonPropertyName("is_3d_secure")]
    public bool Is3DSecure { get; set; }

    [JsonPropertyName("is_auth")]
    public bool IsAuth { get; set; }

    [JsonPropertyName("is_capture")]
    public bool IsCapture { get; set; }

    [JsonPropertyName("is_refunded")]
    public bool IsRefunded { get; set; }

    [JsonPropertyName("is_standalone_payment")]
    public bool IsStandalonePayment { get; set; }

    [JsonPropertyName("is_voided")]
    public bool IsVoided { get; set; }

    [JsonPropertyName("integration_id")]
    public long IntegrationId { get; set; }

    [JsonPropertyName("owner")]
    public long Owner { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public PaymobOrderReference Order { get; set; } = new();

    [JsonPropertyName("source_data")]
    public PaymobSourceData SourceData { get; set; } = new();

    [JsonPropertyName("refunded_amount_cents")]
    public long? RefundedAmountCents { get; set; }

    [JsonPropertyName("captured_amount")]
    public long? CapturedAmount { get; set; }
}

public class PaymobOrderReference
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}

public class PaymobSourceData
{
    [JsonPropertyName("pan")]
    public string Pan { get; set; } = string.Empty;

    [JsonPropertyName("sub_type")]
    public string SubType { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}