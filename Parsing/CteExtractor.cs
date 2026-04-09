using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SsmsAutocompletion {

    /// <summary>
    /// Extrait les noms de CTEs (Common Table Expressions) à partir d'une requête SQL.
    /// Reconnait les formes :
    ///   WITH NomCte AS (...)
    ///   WITH Cte1 AS (...), Cte2 AS (...)
    /// </summary>
    internal sealed class CteExtractor : ICteExtractor {

        // Capture le nom après WITH ou , suivi de AS (
        // Exemples :
        //   WITH OrdersByCustomer AS (    → capture "OrdersByCustomer"
        //   , TotalSales AS (             → capture "TotalSales"
        private static readonly Regex CteNameRegex = new Regex(
            @"(?:\bWITH\b|,)\s*(\w+)\s+AS\s*\(",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

        public IReadOnlyList<string> Extract(string sql) {
            if (string.IsNullOrEmpty(sql)) return Array.Empty<string>();
            var names = new List<string>();
            var seen  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in CteNameRegex.Matches(sql)) {
                string name = match.Groups[1].Value;
                if (!string.IsNullOrEmpty(name) && seen.Add(name))
                    names.Add(name);
            }
            return names.AsReadOnly();
        }
    }
}
