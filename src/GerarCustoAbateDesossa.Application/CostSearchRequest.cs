using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Application;

public sealed record CostSearchRequest(
    DateTime StartDate,
    DateTime EndDate,
    int UnitId,
    CostType Type);
