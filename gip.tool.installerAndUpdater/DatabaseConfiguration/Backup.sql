﻿BACKUP DATABASE -databaseName- TO  
DISK = N'-location-' 
WITH  COPY_ONLY, NOFORMAT, INIT,   
NAME = N'Variobatch_Data Database Backup',
SKIP, NOREWIND, NOUNLOAD,  STATS = 10
GO