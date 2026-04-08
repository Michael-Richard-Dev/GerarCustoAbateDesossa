using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Application;

public sealed record CostProcessingRequest(
    DateTime StartDate,
    DateTime EndDate,
    int UnitId,
    CostType Type);
