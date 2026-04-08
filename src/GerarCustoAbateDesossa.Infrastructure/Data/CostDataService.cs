using System.Data;
using GerarCustoAbateDesossa.Application;
using GerarCustoAbateDesossa.Domain;
using Oracle.ManagedDataAccess.Client;

namespace GerarCustoAbateDesossa.Infrastructure.Data;

public sealed class CostDataService : ICostDataService
{
    private const string SelectAbateSql = """
        SELECT *
          FROM CCAMILO.CUSTO_ABATE
         WHERE ID_UNIDADE = :unidade
           AND DATA BETWEEN :data_inicial AND :data_final
         ORDER BY DATA
        """;

    private const string SelectDesossaSql = """
        SELECT *
          FROM CCAMILO.CUSTO_DESOSSA
         WHERE ID_UNIDADE = :unidade
           AND DATA BETWEEN :data_inicial AND :data_final
         ORDER BY DATA
        """;

    private const string CountAbateSql = """
        SELECT COUNT(1)
          FROM CCAMILO.CUSTO_ABATE
         WHERE DATA = :data
           AND ID_UNIDADE = :unidade
        """;

    private const string CountDesossaSql = """
        SELECT COUNT(1)
          FROM CCAMILO.CUSTO_DESOSSA
         WHERE DATA = :data
           AND ID_UNIDADE = :unidade
        """;

    private const string DeleteAbateSql = """
        DELETE FROM CCAMILO.CUSTO_ABATE
         WHERE DATA = :data
           AND ID_UNIDADE = :unidade
        """;

    private const string DeleteDesossaSql = """
        DELETE FROM CCAMILO.CUSTO_DESOSSA
         WHERE DATA = :data
           AND ID_UNIDADE = :unidade
        """;

    private const string InsertAbateSql = """
        INSERT INTO CCAMILO.CUSTO_ABATE
        (
            ID_UNIDADE,
            TIPO_GRUPO,
            DESCRICAO_GRUPO,
            DESCRICAO_SUBGRUPO,
            CODIGO_PRODUTO,
            SEQUENCIAL_PRODUTO,
            DESCRICAO_PRODUTO,
            CAIXAS,
            PECAS,
            PESO,
            VALOR_MATERIA_PRIMA,
            VALOR_EMBALAGEM,
            PRECO_UNITARIO,
            PRECO_TOTAL,
            VALOR_MATERIA_PRIMA_KG,
            VALOR_MOD_KG,
            VALOR_CIF_KG,
            VALOR_EMBALAGEM_KG,
            CUSTO_INDUSTRIAL_KG,
            CUSTO_INDUSTRIAL,
            VALOR_MOD,
            VALOR_CIF,
            DATA
        )
        SELECT ID_UNIDADE,
               TIPO_GRUPO,
               INITCAP(DESCRICAO_GRUPO) AS DESCRICAO_GRUPO,
               DESCRICAO_SUBGRUPO,
               CODIGO_PRODUTO,
               SEQUENCIAL_PRODUTO,
               DESCRICAO_PRODUTO,
               CAIXAS,
               PECAS,
               PESO,
               VALOR_MATERIA_PRIMA,
               VALOR_EMBALAGEM,
               PRECO_UNITARIO,
               PRECO_TOTAL,
               VALOR_MATERIA_PRIMA_KG,
               VALOR_MOD_KG,
               VALOR_CIF_KG,
               VALOR_EMBALAGEM_KG,
               CUSTO_INDUSTRIAL_KG,
               CUSTO_INDUSTRIAL,
               VALOR_MOD,
               VALOR_CIF,
               DATA
          FROM TABLE(SIGMA_CST.PKG_ABATE.PPL_APROVEITAMENTO(:unidade, :data_inicial, :data_final))
         ORDER BY DATA
        """;

    private const string InsertDesossaSql = """
        INSERT INTO CCAMILO.CUSTO_DESOSSA
        (
            ID_UNIDADE,
            TIPO_GRUPO,
            ID_FAMILIA,
            FAMILIA,
            DESCRICAO_GRUPO,
            DESCRICAO_SUBGRUPO,
            LINHA,
            CODIGO_PRODUTO,
            SEQUENCIAL_PRODUTO,
            DESCRICAO_PRODUTO,
            CAIXAS,
            PECAS,
            PESO,
            VALOR_MATERIA_PRIMA,
            VALOR_MOD_KG,
            VALOR_CIF_KG,
            VALOR_EMBALAGEM_KG,
            CUSTO_INDUSTRIAL_KG,
            CUSTO_INDUSTRIAL,
            VALOR_MOD,
            VALOR_CIF,
            VALOR_INSUMO,
            VALOR_EMBALAGEM,
            VALOR_MATERIA_PRIMA_KG,
            VALOR_INSUMO_KG,
            DATA
        )
        SELECT ID_UNIDADE,
               TIPO_GRUPO,
               ID_FAMILIA,
               FAMILIA,
               DESCRICAO_GRUPO,
               DESCRICAO_SUBGRUPO,
               LINHA,
               CODIGO_PRODUTO,
               SEQUENCIAL_PRODUTO,
               DESCRICAO_PRODUTO,
               CAIXAS,
               PECAS,
               PESO,
               VALOR_MATERIA_PRIMA,
               VALOR_MOD_KG,
               VALOR_CIF_KG,
               VALOR_EMBALAGEM_KG,
               CUSTO_INDUSTRIAL_KG,
               CUSTO_INDUSTRIAL,
               VALOR_MOD,
               VALOR_CIF,
               VALOR_INSUMO,
               VALOR_EMBALAGEM,
               VALOR_MATERIA_PRIMA_KG,
               VALOR_INSUMO_KG,
               DATA
          FROM TABLE(SIGMA_CST.PKG_INDUSTRIALIZADO.PPL_RESULTADO(:unidade, :data_inicial, :data_final))
         ORDER BY DATA
        """;

    private readonly string _connectionString;
    private readonly string? _tnsAdmin;

    public CostDataService(DatabaseOptions databaseOptions)
    {
        _connectionString = databaseOptions.ConnectionString;
        _tnsAdmin = databaseOptions.TnsAdmin;
    }

    public DataTable LoadCosts(CostSearchRequest request)
    {
        ValidateDateRange(request.StartDate, request.EndDate);

        using var connection = CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = request.Type == CostType.Abate ? SelectAbateSql : SelectDesossaSql;
        command.CommandTimeout = 0;

        AddDateRangeParameters(command, request.UnitId, request.StartDate, request.EndDate);

        using var adapter = new OracleDataAdapter((OracleCommand)command);
        adapter.SelectCommand = command;

        var dataTable = new DataTable();
        adapter.Fill(dataTable);
        return dataTable;
    }

    public CostProcessResult ProcessCosts(
        CostProcessingRequest request,
        Func<DateTime, int, ExistingRecordDecision> resolveExistingRecords,
        Action<string>? reportStatus = null,
        CancellationToken cancellationToken = default)
    {
        ValidateDateRange(request.StartDate, request.EndDate);

        using var connection = CreateOpenConnection();

        var processedDays = 0;
        var skippedDays = 0;

        for (var currentDate = request.StartDate.Date; currentDate <= request.EndDate.Date; currentDate = currentDate.AddDays(1))
        {
            cancellationToken.ThrowIfCancellationRequested();
            reportStatus?.Invoke($"Processando dia {currentDate:dd/MM/yyyy}");

            var existingCount = GetExistingRecordCount(connection, request.Type, request.UnitId, currentDate);
            if (existingCount > 0)
            {
                var decision = resolveExistingRecords(currentDate, existingCount);
                if (decision == ExistingRecordDecision.CancelProcessing)
                {
                    return new CostProcessResult(true, processedDays, skippedDays);
                }

                if (decision == ExistingRecordDecision.SkipDay)
                {
                    skippedDays++;
                    continue;
                }
            }

            using var transaction = connection.BeginTransaction();
            if (existingCount > 0)
            {
                DeleteExistingRecords(connection, transaction, request.Type, request.UnitId, currentDate);
            }

            ExecuteCollection(connection, transaction, request.Type, request.UnitId, currentDate);
            transaction.Commit();

            processedDays++;
        }

        return new CostProcessResult(false, processedDays, skippedDays);
    }

    private OracleConnection CreateOpenConnection()
    {
        var connection = new OracleConnection(_connectionString);
        if (!string.IsNullOrWhiteSpace(_tnsAdmin))
        {
            connection.TnsAdmin = _tnsAdmin;
        }

        connection.Open();
        return connection;
    }

    private static void ValidateDateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate.Date > endDate.Date)
        {
            throw new InvalidOperationException("A data inicial nao pode ser maior que a data final.");
        }
    }

    private static void AddDateRangeParameters(OracleCommand command, int unitId, DateTime startDate, DateTime endDate)
    {
        AddParameter(command, "unidade", unitId, OracleDbType.Int32);
        AddParameter(command, "data_inicial", startDate.Date, OracleDbType.Date);
        AddParameter(command, "data_final", endDate.Date, OracleDbType.Date);
    }

    private static void AddSingleDateParameters(OracleCommand command, int unitId, DateTime date)
    {
        AddParameter(command, "unidade", unitId, OracleDbType.Int32);
        AddParameter(command, "data", date.Date, OracleDbType.Date);
    }

    private static void AddParameter(OracleCommand command, string name, object value, OracleDbType dbType)
    {
        var parameter = new OracleParameter(name, dbType)
        {
            Value = value
        };
        command.Parameters.Add(parameter);
    }

    private static int GetExistingRecordCount(OracleConnection connection, CostType type, int unitId, DateTime date)
    {
        using var command = connection.CreateCommand();
        command.CommandText = type == CostType.Abate ? CountAbateSql : CountDesossaSql;
        command.CommandTimeout = 0;
        AddSingleDateParameters(command, unitId, date);

        var value = command.ExecuteScalar();
        return Convert.ToInt32(value);
    }

    private static void DeleteExistingRecords(OracleConnection connection, OracleTransaction transaction, CostType type, int unitId, DateTime date)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = type == CostType.Abate ? DeleteAbateSql : DeleteDesossaSql;
        command.CommandTimeout = 0;
        AddSingleDateParameters(command, unitId, date);
        command.ExecuteNonQuery();
    }

    private static void ExecuteCollection(OracleConnection connection, OracleTransaction transaction, CostType type, int unitId, DateTime date)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = type == CostType.Abate ? InsertAbateSql : InsertDesossaSql;
        command.CommandTimeout = 0;
        AddDateRangeParameters(command, unitId, date, date);
        command.ExecuteNonQuery();
    }
}
