update ACClass set AssemblyQualifiedName = REPLACE(AssemblyQualifiedName, 'gip.core.reporthandler', 'gip.core.reporthandlerwpf') 
where AssemblyQualifiedName like '%gip.core.reporthandler.%'
and not (ACIdentifier = 'ACPrintManager'
or ACIdentifier = 'ACPrintServerBase'
or ACIdentifier = 'ReportConfigurationWrapper'
or ACIdentifier = 'PAOrderInfoManagerBase'
or ACIdentifier = 'PrinterInfo'
or ACIdentifier = 'PWNodePrint');