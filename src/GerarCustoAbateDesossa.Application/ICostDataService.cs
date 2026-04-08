using System.Data;
using GerarCustoAbateDesossa.Domain;

namespace GerarCustoAbateDesossa.Application;

public interface ICostDataService
{
    DataTable LoadCosts(CostSearchRequest request);

    CostProcessResult ProcessCosts(
        CostProcessingRequest request,
        Func<DateTime, int, ExistingRecordDecision> resolveExistingRecords,
        Action<string>? reportStatus = null,
        CancellationToken cancellationToken = default);
}
