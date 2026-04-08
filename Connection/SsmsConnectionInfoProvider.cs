using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using System.Collections.Specialized;
using System.Reflection;

namespace SsmsAutocompletion {

    internal sealed class SsmsConnectionInfoProvider : IConnectionInfoProvider {

        public ConnectionKey GetConnectionKey() {
            var (server, database, _, _, _) = GetCurrentConnectionInfo();
            if (string.IsNullOrEmpty(server)) return new ConnectionKey(null);
            return new ConnectionKey($"{server}|{database}");
        }

        public ServerConnection BuildServerConnection() {
            var (server, database, windowsAuth, user, password) = GetCurrentConnectionInfo();
            if (string.IsNullOrEmpty(server)) return null;
            var connection = new ServerConnection(server) { DatabaseName = database };
            if (windowsAuth) {
                connection.LoginSecure = true;
                return connection;
            }
            connection.LoginSecure = false;
            connection.Login       = user;
            connection.Password    = password;
            return connection;
        }

        private static (string server, string database, bool windowsAuth, string user, string password)
            GetCurrentConnectionInfo() {
            try {
                var scriptFactory  = ServiceCache.ScriptFactory;
                var activeWndInfo  = scriptFactory?.CurrentlyActiveWndConnectionInfo;
                if (activeWndInfo == null) return default;

                var uiConnProp = activeWndInfo.GetType().GetProperty("UIConnectionInfo");
                if (uiConnProp == null) return default;

                object uiConnInfo = uiConnProp.GetValue(activeWndInfo);
                if (uiConnInfo == null) return default;

                string server   = GetProperty(uiConnInfo, "ServerName") as string ?? "";
                string userName = GetProperty(uiConnInfo, "UserName")   as string ?? "";
                string password = GetProperty(uiConnInfo, "Password")   as string ?? "";
                int authType    = (int)(GetProperty(uiConnInfo, "AuthenticationType") ?? 0);

                string database = "master";
                var advancedOptions = GetProperty(uiConnInfo, "AdvancedOptions") as NameValueCollection;
                if (advancedOptions != null && !string.IsNullOrWhiteSpace(advancedOptions["DATABASE"]))
                    database = advancedOptions["DATABASE"];

                return (server, database, authType == 0, userName, password);
            }
            catch { return default; }
        }

        private static object GetProperty(object source, string name) =>
            source.GetType().GetProperty(name)?.GetValue(source, null);
    }
}
