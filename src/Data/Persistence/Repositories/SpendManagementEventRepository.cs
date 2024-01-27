using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Data.Persistence.Repositories
{
    public class SpendManagementEventRepository(SqlConnection sqlConnection, ILogger logger, IDbTransaction dbTransaction)
        : ISpendManagementEventRepository
    {
        private readonly SqlConnection _sqlConnection = sqlConnection;
        private readonly IDbTransaction _dbTransaction = dbTransaction;
        private readonly ILogger _logger = logger;
        private readonly StringBuilder QueryBuilder = new();

        public async Task<Guid> Add(SpendManagementEvent spendManagementEvent)
        {
            QueryBuilder.Clear();
            QueryBuilder.AppendLine("INSERT INTO [SpendManagementEvents]");
            QueryBuilder.AppendLine("( ");
            QueryBuilder.AppendLine("[RoutingKey], ");
            QueryBuilder.AppendLine("[DataEvent], ");
            QueryBuilder.AppendLine("[NameEvent], ");
            QueryBuilder.AppendLine("[EventBody] ");
            QueryBuilder.AppendLine(") ");
            QueryBuilder.AppendLine("OUTPUT Inserted.RoutingKey ");
            QueryBuilder.AppendLine(" VALUES ");
            QueryBuilder.AppendLine(" ( ");
            QueryBuilder.AppendLine("  @RoutingKey,  ");
            QueryBuilder.AppendLine("  @DataEvent,  ");
            QueryBuilder.AppendLine("  @NameEvent,  ");
            QueryBuilder.AppendLine("  @EventBody  ");
            QueryBuilder.AppendLine("  )  ");

            var result = await _sqlConnection.ExecuteScalarAsync(QueryBuilder.ToString(), spendManagementEvent, _dbTransaction);

            if (result != null && Guid.TryParse(result.ToString(), out Guid guidResult))
            {
                _logger.Information("Command or event inserted successfully on database {@entity}", spendManagementEvent);
                return guidResult;
            }

            _logger.Information("An error occured when tried insert the command", spendManagementEvent);
            return Guid.Empty;
        }
       
    }
}
