using System.Data.Common;

namespace GameOfLife.Infrastructure;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}

