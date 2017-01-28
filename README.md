# ExecuteSqlScriptByBatches
C# command line utility helps to execute large MS SQL script files.

Sometimes we can generate so big SQL you can't open it in Management Studio.
This utility helps you to execute the file line by line or batch by batch.

Parameters:
```
GlobalOption            Description
File* (-F)
FindGo (-Fi)            [Default='True']
BatchSize (-B)          [Default='0']
ConnectionString (-C)
ShowQuery (-S)          [Default='False']
```

By default the programm split a file by `GO` statements and doesn't split by another method.
Use `BatchSize` parameter, if a file doesn't contain `GO` statements.

Example:
```
ExecuteSqlScriptByBatches.exe -F test.sql -S True -C "Data Source=(local);Initial Catalog=test;Integrated Security = SSPI"
```