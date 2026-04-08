using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Desktop.Models;

public sealed record CostTypeOption(CostType Value, string DisplayName)
{
    public override string ToString() => DisplayName;
}
