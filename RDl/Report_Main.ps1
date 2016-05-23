	
	
	#################################################################
	###                     Deploy SSRS Reports                   ###
	#################################################################
	
	<# Create Reporting Folder #>
	#./PowerShell/Report_Folder.ps1 -SSRSReportFolder $SSRS_ReportFolder -SSRSReportServerUrl "http://$SSRS_Report_Server/ReportServer/ReportService2010.asmx"

	<# Create Reporting Data Source #>
	#./PowerShell/Report_DataSource.ps1 -DataSource_Server $DataSource_Server 	-DataSource_Database $DataSource_Database -DataSourceName $SSRS_DataSourceName -DataSourceUserName $DataSourceUserName -DataSourceUserPassword $DataSourceUserPassword -SSRS_Server $SSRS_Server -SSRSReportFolder $SSRS_ReportFolder -SSRSReportServerUrl "http://$SSRS_Report_Server/ReportServer/ReportService2010.asmx"

	<# Create Reporting Upload Reports RDL #>
	#./PowerShell/Report_Upload.ps1
		