namespace GerarCustoAbateDesossa.Domain;

public static class UnitCatalog
{
    public static IReadOnlyList<UnitOption> All { get; } =
    [
        new(1, "01 - APARECIDA DO TABOADO - MS"),
        new(12, "12 - APARECIDA DO OESTE - SP"),
        new(16, "16 - VARZEA GRANDE - MT"),
        new(17, "17 - VILA MARIA - RS")
    ];
}
