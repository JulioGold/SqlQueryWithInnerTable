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
                int idsLength = ids.Length;
                int indexLastItem = ids.Length - 1;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"INSERT INTO #{tempTableName}");
                string union = " UNION";

                for (int i = 0; i < idsLength; i++)
                {
                    if (i == indexLastItem)
                    {
                        union = ";";
                    }

                    stringBuilder.AppendLine($"SELECT {ids[i]} AS Id{union}");
                }

                string queryCreateTable = $@"
IF OBJECT_ID('tempdb..#{tempTableName}') IS NOT NULL
BEGIN
	DROP TABLE #{tempTableName};
END

CREATE TABLE #{tempTableName}(Id int);
CREATE CLUSTERED INDEX ix_{tempTableName} ON #{tempTableName} ([Id]);
{stringBuilder.ToString()}
";

                query = query.Replace(idsCreateTempTableTag, queryCreateTable);

                query = query.Replace(idsInnerTag, $"INNER JOIN #{tempTableName} ON #{tempTableName}.Id = {innerLink}");
            }
        }
    }
}
