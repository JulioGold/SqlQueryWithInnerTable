using System;
using System.Linq;
using System.Text;

namespace SqlQueryWithInnerTable
{
    public class QueryWithInnerTable
    {
        /// <summary>
        /// This is useful to substitute the IN clause in a query by the INNER table, to improve performance.
        /// </summary>
        /// <param name="query">Your SQL query with tha macros for replacement.</param>
        /// <param name="ids">IDs to link the INNER table with your IDs.</param>
        /// <param name="innerLink">The macro you want to replace by table.column to link with INNER table.</param>
        /// <param name="idsCreateTempTableTag">The macro where the create temp table will be inserted.</param>
        /// <param name="idsInnerTag">The macro where the INNER JOIN will be inserted.</param>
        public static void SetupTempTableIds(ref string query, int[] ids, string innerLink, string idsCreateTempTableTag, string idsInnerTag)
        {
            if (ids != null && ids.Length > 0 && !string.IsNullOrEmpty(innerLink))
            {
                ids = ids.Distinct().ToArray();

                string tempTableName = "IdsTempTable";
                string queryCreateTable = $@"
IF OBJECT_ID('tempdb..#{tempTableName}') IS NOT NULL
BEGIN
	DROP TABLE #{tempTableName};
END

CREATE TABLE #{tempTableName}(Id int);
CREATE CLUSTERED INDEX ix_{tempTableName} ON #{tempTableName} ([Id]);
{MakeInsert(tempTableName, ids)}
";

                query = query.Replace(idsCreateTempTableTag, queryCreateTable);

                query = query.Replace(idsInnerTag, $"INNER JOIN #{tempTableName} ON #{tempTableName}.Id = {innerLink}");
            }
        }

        private static string MakeInsert(string tempTableName, int[] ids)
        {
            Func<int, int, int> mod = (c, d) => c % d;
            Func<int, int, int> calcPages = (ids_Length, split_Size) => ((ids_Length - (mod(ids_Length, split_Size))) / split_Size);
            StringBuilder stringBuilder = new StringBuilder();
            int splitSize = 1000; // Blocks of 1000 rows, the SQL Server limit it in VALUES clauses
            int pages = mod(ids.Length, splitSize) > 0 ? calcPages(ids.Length, splitSize) + 1 : calcPages(ids.Length, splitSize);

            for (int i = 0; i < pages; i++)
            {
                stringBuilder.AppendLine($"INSERT INTO #{tempTableName} VALUES {string.Join(',', ids.Skip(i * splitSize).Take(splitSize).Select(s => $"({s.ToString()})").ToArray())};");
            }

            return stringBuilder.ToString();
        }
    }
}
