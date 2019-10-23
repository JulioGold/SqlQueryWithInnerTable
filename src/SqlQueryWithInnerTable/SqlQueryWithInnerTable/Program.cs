using System;
using System.Linq;
using System.Text;

namespace SqlQueryWithInnerTable
{
    class Program
    {
        static void Main(string[] args)
        {
            string query = @"
IF OBJECT_ID('tempdb..#MyRealTable') IS NOT NULL
BEGIN
	DROP TABLE #MyRealTable;
END

CREATE TABLE #MyRealTable(Id int, [Description] varchar(80));

INSERT INTO #MyRealTable
SELECT 1 AS Id, 'Watermelon' AS [Description] UNION
SELECT 2 AS Id, 'Banana' AS [Description] UNION
SELECT 3 AS Id, 'Strawberry' AS [Description] UNION
SELECT 4 AS Id, 'Apple' AS [Description] UNION
SELECT 5 AS Id, 'Orange' AS [Description] UNION
SELECT 6 AS Id, 'Lemon' AS [Description] UNION
SELECT 7 AS Id, 'Pineapple' AS [Description] UNION
SELECT 8 AS Id, 'Plum' AS [Description] UNION
SELECT 9 AS Id, 'Grape' AS [Description];
/*IdsCreateTempTable*/
SELECT
	tbl.Id
	,tbl.[Description]
FROM #MyRealTable AS tbl
/*IdsInner*/
WHERE
	1 = 1
";

            int[] ids = { 7, 1, 3 };

            string innerLink = "tbl.Id";

            SetupTempTableIds(ref query, ids, innerLink, "/*IdsCreateTempTable*/", "/*IdsInner*/");
        }

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

        public static string MakeInsert(string tempTableName, int[] ids)
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
