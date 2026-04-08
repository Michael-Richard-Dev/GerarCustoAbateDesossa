namespace GerarCustoAbateDesossa.Application;

public sealed record CostProcessResult(bool Cancelled, int ProcessedDays, int SkippedDays);
