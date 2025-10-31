using System.Data;

namespace MediaGallery.Web.Services;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
