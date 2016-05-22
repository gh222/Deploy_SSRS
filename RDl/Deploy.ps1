
	#################################################################
	###                     Deploy SSRS Reports                   ###
	#################################################################
	
	
	#################################################################
	###                   Octopus Parameters                      ###
	#################################################################
	
	#$environment 			=  "UAT"   #$OctopusParameters["Octopus.Environment.Name"] 
	$_SSRS_Server 			= "NIBS"
	$_ReportsLocation 		="./Reports";
	$_DataSource_Server 		= "NIBS"					#WGB01DB6022
	
	#################################################################
	###                   Fixed Parameters                        ###
	#################################################################
	
	$_DataSource_Database 		= "Books"			#Inventory
	$_DataSourceUserName 		= "GH"				#KPI_user
	$_DataSourceUserPassword 	= "GH"				#KPI_user

	$_SSRSReportFolder 			= "Coast"
	$_DataSourceName 			= $_SSRSReportFolder + "_DataSource"		
	$_DataSourceParent 			= "/" + $_SSRSReportFolder
	$_SSRSReportServerUrl  		= "http://"+$_SSRS_Server+"/ReportServer/ReportService2010.asmx"

	Write-Host -fore Green "Deploying reports to " $_ssrsreportserverurl

	#################################################################
	###                   Create Folders                          ###
	#################################################################
	
		./Report_Folder.ps1 -SSRSReportFolder $_SSRSReportFolder -SSRSReportServerUrl $_SSRSReportServerUrl
		#./Report_Folder.ps1 -SSRSReportFolder "Coast" -SSRSReportServerUrl "http://NIBS/ReportServer/ReportService2010.asmx"
		 #Write-Host -fore Green "Folder Created => " $_SSRSReportFolder
	
	#################################################################
	###                   Create Data Source                      ###
	#################################################################

		#./Report_DataSource.ps1 -DataSource_Server $_DataSource_Server 	-DataSource_Database $_DataSource_Database -DataSourceName $_DataSourceName -DataSourceUserName $_DataSourceUserName -DataSourceUserPassword $_DataSourceUserPassword -SSRS_Server $_SSRS_Server -SSRSReportFolder $_SSRSReportFolder -SSRSReportServerUrl $_SSRSReportServerUrl
		#Write-Host -fore Green "Data Source Created => " $_DataSourceName
		
	#################################################################
	###                   Upload Reports                      ###
	#################################################################
		
		
		#./Report_Upload.ps1
		