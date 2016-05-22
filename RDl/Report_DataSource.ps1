param(		 [string]$DataSource_Server
			,[string]$DataSource_Database 
			,[string]$DataSourceName 
			,[string]$DataSourceUserName 
			,[string]$DataSourceUserPassword 
			,[string]$SSRS_Server 
			,[string]$SSRSReportFolder 
			,[string]$SSRSReportServerUrl 
		
	)
	
	#################################################################
	###                   Parameters                      ###
	#################################################################
	<#
	$SSRS_Server = "NIBS"
	$ReportsLocation ="./Reports";
	$SSRSReportFolder = "Coast"
	$DataSource_Database = "Books"
	$DataSource_Server = "NIBS"
	$DataSourceUserName = "GH"
	$DataSourceUserPassword = "GH"
	$DataSourceName = "Coast_DataSource"
	$SSRSReportServerUrl  = "http://"+$SSRS_Server+"/ReportServer/ReportService2010.asmx"
	#>

	
	#################################################################
	###                  Create DataSource                        ###
	#################################################################
	
	function Create-DataSource
	{
		$ssrsProxy = New-WebServiceProxy -Uri $ssrsreportserverurl -UseDefaultCredential
		$Name = $DataSourceName
		$Parent = "/" + $SSRSReportFolder
		$ConnectString = "data source=$DataSource_Server;initial catalog=$DataSource_Database"
		$type = $ssrsProxy.GetType().Namespace
		$DSDdatatype = ($type + '.DataSourceDefinition')
		$DSD = new-object ($DSDdatatype)
		if($DSD -eq $null){
			  Write-Error Failed to create data source definition object
		}
		$CredentialDataType = ($type + '.CredentialRetrievalEnum')
		$CredentialDataType
		$Cred = new-object ($CredentialDataType)
	    $Cred.value__=  1
		$DSD.CredentialRetrieval =$Cred
		$DSD.ConnectString = $ConnectString
		$DSD.Enabled = $true
		$DSD.UserName = $DataSourceUserName
		$DSD.Password = $DataSourceUserPassword 
		$DSD.EnabledSpecified = $false
		$DSD.Extension = "SQL"
		$DSD.ImpersonateUserSpecified = $false
		$DSD.Prompt = $null
		$DSD.WindowsCredentials = $false
		$HiddenType = ($type + '.Property')
		$HiddenProp = New-Object ($HiddenType)
		$HiddenProp.Name = 'Hidden'
		$HiddenProp.Value = 'true'
		$newDSD = $ssrsProxy.CreateDataSource($Name,$Parent,$true,$DSD,$HiddenProp)
	}
	
		
		Create-DataSource
	
	
