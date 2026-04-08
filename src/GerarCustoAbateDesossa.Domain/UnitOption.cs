namespace GerarCustoAbateDesossa.Domain;

public sealed record UnitOption(int Id, string DisplayName)
{
    public override string ToString() => DisplayName;
}
