using Microsoft.SqlServer.Management.Common;

namespace SsmsAutocompletion {

    internal interface IConnectionInfoProvider {
        ConnectionKey GetConnectionKey();
        ServerConnection BuildServerConnection();
    }
}
