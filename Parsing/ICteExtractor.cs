using System.Collections.Generic;

namespace SsmsAutocompletion {

    internal interface ICteExtractor {
        /// <summary>Extrait les noms de toutes les CTEs définies dans le SQL.</summary>
        IReadOnlyList<string> Extract(string sql);
    }
}
