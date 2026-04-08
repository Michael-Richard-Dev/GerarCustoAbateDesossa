using System.Data;
using System.Globalization;
using GerarCustoAbateDesossa.Application;
using GerarCustoAbateDesossa.Domain;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GerarCustoAbateDesossa.Infrastructure.Data;

public sealed class CostDataService : ICostDataService
{
    private static readonly HashSet<string> FiveDecimalColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "VALOR_MATERIA_PRIMA_KG",
        "VALOR_MOD_KG",
        "VALOR_CIF_KG",
        "VALOR_EMBALAGEM_KG"
    };

    private const string SelectAbateSql = """
        SELECT *
          FROM CCAMILO.CUSTO_ABATE
         WHERE ID_UNIDADE = :unidade
           AND DATA >= :data_inicial
           AND DATA < :data_limite
         ORDER BY DATA
        """;

    private const string SelectDesossaSql = """
        SELECT *
          FROM CCAMILO.CUSTO_DESOSSA
         WHERE ID_UNIDADE = :unidade
           AND DATA >= :data_inicial
           AND DATA < :data_limite
         ORDER BY DATA
        """;

    private const string CountAbateSql = """
        SELECT COUNT(1)
          FROM CCAMILO.CUSTO_ABATE
         WHERE DATA >= :data_inicial
           AND DATA < :data_limite
           AND ID_UNIDADE = :unidade
        """;

    private const string CountDesossaSql = """
        SELECT COUNT(1)
          FROM CCAMILO.CUSTO_DESOSSA
         WHERE DATA >= :data_inicial
           AND DATA < :data_limite
           AND ID_UNIDADE = :unidade
        """;

    private const string DeleteAbateSql = """
        DELETE FROM CCAMILO.CUSTO_ABATE
         WHERE DATA >= :data_inicial
           AND DATA < :data_limite
           AND ID_UNIDADE = :unidade
        """;

    private const string DeleteDesossaSql = """
        DELETE FROM CCAMILO.CUSTO_DESOSSA
         WHERE DATA >= :data_inicial
           AND DATA < :data_limite
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

    private const string CountGeneratedAbateSql = """
        SELECT COUNT(1)
          FROM TABLE(SIGMA_CST.PKG_ABATE.PPL_APROVEITAMENTO(:unidade, :data_inicial, :data_final))
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

    private const string CountGeneratedDesossaSql = """
        SELECT COUNT(1)
          FROM TABLE(SIGMA_CST.PKG_INDUSTRIALIZADO.PPL_RESULTADO(:unidade, :data_inicial, :data_final))
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
        ConfigureCommand(command, request.Type == CostType.Abate ? SelectAbateSql : SelectDesossaSql);

        AddSearchDateRangeParameters(command, request.UnitId, request.StartDate, request.EndDate);

        var dataTable = new DataTable();
        using var reader = command.ExecuteReader();

        for (var fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
        {
            dataTable.Columns.Add(reader.GetName(fieldIndex), typeof(string));
        }

        while (reader.Read())
        {
            var row = dataTable.NewRow();
            for (var fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
            {
                row[fieldIndex] = NormalizeFieldValue(reader, fieldIndex, reader.GetName(fieldIndex));
            }

            dataTable.Rows.Add(row);
        }

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

            var generatedRows = CountGeneratedRows(connection, transaction, request.Type, request.UnitId, currentDate);
            if (generatedRows == 0)
            {
                transaction.Rollback();
                reportStatus?.Invoke($"Nenhum registro foi retornado pela rotina Oracle no dia {currentDate:dd/MM/yyyy}.");
                skippedDays++;
                continue;
            }

            ExecuteCollection(connection, transaction, request.Type, request.UnitId, currentDate);

            var persistedRows = GetExistingRecordCount(connection, request.Type, request.UnitId, currentDate, transaction);
            if (persistedRows == 0)
            {
                transaction.Rollback();
                throw new InvalidOperationException(
                    $"A rotina Oracle retornou dados para {currentDate:dd/MM/yyyy}, mas nenhum registro foi localizado na tabela de destino.");
            }

            transaction.Commit();
            reportStatus?.Invoke($"Dia {currentDate:dd/MM/yyyy}: {persistedRows} registro(s) gravado(s).");

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

    private static void AddSearchDateRangeParameters(OracleCommand command, int unitId, DateTime startDate, DateTime endDate)
    {
        AddParameter(command, "unidade", unitId, OracleDbType.Int32);
        AddParameter(command, "data_inicial", startDate.Date, OracleDbType.Date);
        AddParameter(command, "data_limite", endDate.Date.AddDays(1), OracleDbType.Date);
    }

    private static void AddCollectionDateRangeParameters(OracleCommand command, int unitId, DateTime startDate, DateTime endDate)
    {
        AddParameter(command, "unidade", unitId, OracleDbType.Int32);
        AddParameter(command, "data_inicial", startDate.Date, OracleDbType.Date);
        AddParameter(command, "data_final", endDate.Date, OracleDbType.Date);
    }

    private static void AddSingleDateParameters(OracleCommand command, int unitId, DateTime date)
    {
        AddParameter(command, "unidade", unitId, OracleDbType.Int32);
        AddParameter(command, "data_inicial", date.Date, OracleDbType.Date);
        AddParameter(command, "data_limite", date.Date.AddDays(1), OracleDbType.Date);
    }

    private static void AddParameter(OracleCommand command, string name, object value, OracleDbType dbType)
    {
        var parameter = new OracleParameter(name, dbType)
        {
            Value = value
        };
        command.Parameters.Add(parameter);
    }

    private static object NormalizeFieldValue(OracleDataReader reader, int fieldIndex, string columnName)
    {
        if (reader.IsDBNull(fieldIndex))
        {
            return DBNull.Value;
        }

        var value = reader.GetOracleValue(fieldIndex);
        return value switch
        {
            OracleDate oracleDate => oracleDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
            OracleTimeStamp oracleTimeStamp => oracleTimeStamp.Value.ToString("dd/MM/yyyy HH:mm:ss"),
            OracleTimeStampLTZ oracleTimeStampLtz => oracleTimeStampLtz.Value.ToString("dd/MM/yyyy HH:mm:ss"),
            OracleTimeStampTZ oracleTimeStampTz => oracleTimeStampTz.Value.ToString(),
            OracleDecimal oracleDecimal => FormatOracleDecimal(oracleDecimal, columnName),
            OracleString oracleString => oracleString.Value,
            OracleClob oracleClob => oracleClob.Value,
            OracleBlob oracleBlob => Convert.ToBase64String(oracleBlob.Value),
            OracleBinary oracleBinary => Convert.ToBase64String(oracleBinary.Value),
            OracleIntervalDS oracleIntervalDs => oracleIntervalDs.ToString(),
            OracleIntervalYM oracleIntervalYm => oracleIntervalYm.ToString(),
            DateTime dateTime => dateTime.ToString("dd/MM/yyyy HH:mm:ss"),
            decimal decimalValue => FormatDecimal(decimalValue, columnName),
            double doubleValue => FormatDecimal((decimal)doubleValue, columnName),
            float floatValue => FormatDecimal((decimal)floatValue, columnName),
            byte[] bytes => Convert.ToBase64String(bytes),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatOracleDecimal(OracleDecimal value, string columnName)
    {
        if (FiveDecimalColumns.Contains(columnName) && !value.IsNull)
        {
            return FormatDecimal(value.Value, columnName);
        }

        return value.ToString();
    }

    private static string FormatDecimal(decimal value, string columnName)
    {
        if (FiveDecimalColumns.Contains(columnName))
        {
            return value.ToString("N5", CultureInfo.CurrentCulture);
        }

        return value.ToString(CultureInfo.CurrentCulture);
    }

    private static int GetExistingRecordCount(OracleConnection connection, CostType type, int unitId, DateTime date, OracleTransaction? transaction = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        ConfigureCommand(command, type == CostType.Abate ? CountAbateSql : CountDesossaSql);
        AddSingleDateParameters(command, unitId, date);

        var value = command.ExecuteScalar();
        return Convert.ToInt32(value);
    }

    private static int CountGeneratedRows(OracleConnection connection, OracleTransaction transaction, CostType type, int unitId, DateTime date)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        ConfigureCommand(command, type == CostType.Abate ? CountGeneratedAbateSql : CountGeneratedDesossaSql);
        AddCollectionDateRangeParameters(command, unitId, date, date);

        var value = command.ExecuteScalar();
        return Convert.ToInt32(value);
    }

    private static void DeleteExistingRecords(OracleConnection connection, OracleTransaction transaction, CostType type, int unitId, DateTime date)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        ConfigureCommand(command, type == CostType.Abate ? DeleteAbateSql : DeleteDesossaSql);
        AddSingleDateParameters(command, unitId, date);
        command.ExecuteNonQuery();
    }

    private static void ExecuteCollection(OracleConnection connection, OracleTransaction transaction, CostType type, int unitId, DateTime date)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        ConfigureCommand(command, type == CostType.Abate ? InsertAbateSql : InsertDesossaSql);
        AddCollectionDateRangeParameters(command, unitId, date, date);
        command.ExecuteNonQuery();
    }

    private static void ConfigureCommand(OracleCommand command, string sql)
    {
        command.BindByName = true;
        command.CommandText = sql;
        command.CommandTimeout = 0;
    }
}
