Create a query replacing `IN` condition to `INNER`.

This code helps to create a `INNER JOIN` clause to substitute a `IN` clause in a SQL query for performance reasons (This was created for SQL server, but I belive wich it runs at other sgbds).

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
  
danke  