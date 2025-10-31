using System.Data;

namespace MediaGallery.Web.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
