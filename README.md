Create a query replacing `IN` condition to `INNER`.

This code helps to create a `INNER JOIN` clause to substitute a `IN` clause in a SQL query for performance reasons (This was created for SQL server, but I believe which it runs at others SGBDs).

Lets see a sample:

SQL query with IN clause:
```sql
SELECT
	tbl.Id
	,tbl.[Description]
FROM #MyRealTable AS tbl
WHERE
	tbl.Id IN ( ..., ..., ... )
```

SQL query with INNER clause:
```sql
SELECT
	tbl.Id
	,tbl.[Description]
FROM #MyRealTable AS tbl
INNER JOIN #IdsTempTable ON #IdsTempTable.Id = tbl.Id
```
  
TODO:  
- Encapsulate into a class.
- Create a nuget package.
- Create CI/CD pipeline.
  
Danke  