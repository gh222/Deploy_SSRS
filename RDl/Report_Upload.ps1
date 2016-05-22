
	
	#################################################################
	###                   Octopus Parameters                      ###
	#################################################################
	
	$environment =  "UAT"   #$OctopusParameters["Octopus.Environment.Name"] 
	$SSRS_Server = "NIBS"
	$ReportsLocation ="./Reports";
	
	#################################################################
	###                   Fixed Parameters                        ###
	#################################################################
	
	$SSRSReportServerUrl  = "http://"+$SSRS_Server+"/ReportServer/ReportService2010.asmx"
	$SSRSReportFolder = "Coast"
	$DataSourceUserName 		= "GH"				#KPI_user
	$DataSourceUserPassword 	= "GH"				#KPI_user

	# If the Reporting Services Root (destination) directory does not exist - create it.
	#$ssrsProxy = New-WebServiceProxy -Uri $ssrsreportserverurl -UseDefaultCredential




	#################################################################
	###                   Function Create                         ###
	#################################################################

	function Create-Report
	(
		[ValidateScript({Test-Path $_})]
		[Parameter(Position=0,Mandatory=$true)]
		[Alias("rdl")]
		[string]$rdlFile,
	 
		[Parameter(Position=1)]
		[Alias("folder")]
		[string]$reportFolder="",
	 
		[Parameter(Position=2)]
		[Alias("name")]
		[string]$reportName="",
	 
		[Parameter(Position=3)]
		[string]$reportCredentialsUsername="",

		[Parameter(Position=4)]
		[string]$reportCredentialsPassword="",

		[switch]$force
	)
	{
	$ssrsProxy = New-WebServiceProxy -Uri $ssrsreportserverurl -UseDefaultCredential
		#Set reportname if blank, default will be the filename without extension
		if($reportName -ne "") {
			$reportName = [System.IO.Path]::GetFileNameWithoutExtension($rdlFile);
		}
		
		try
		{
			#Get Report content in bytes
			$byteArray = gc $rdlFile -encoding byte
			
			Write-Host "Uploading $reportName to $reportFolder"
	
	 
			$warnings =@();
			$warnings = $ssrsProxy.CreateCatalogItem("Report", $reportName,$reportFolder,$force,$byteArray,$null,[ref]$warnings)


			if($warnings -eq $null) { 
				Write-Host "Report uploaded" 
			}
			else { 
				$warnings | % { Write-Warning $_ }
			}

			$found = 0

			<# Report Path #>
			$dsPath = [string]::Concat($reportFolder, "/", $reportName)
			

			
			$ds = $ssrsProxy.GetItemDataSources($dsPath)
	
			$ds | ForEach-Object {
				
				
					Write-Host "Shared datasource found. setting path" 
					$SSRSSharedDataSourcePath = "/Coast"
					$SSRSSharedDataSourcePath

		
<#
						$proxyNamespace = $ssrsProxy.GetType().Namespace
						$_.Item = New-Object("$proxyNamespace.DataSourceReference")
						$_.Item.Reference = $SSRSSharedDataSourcePath
						$found = 1
					Write-Error ("Report $reportName had shared data source reference dsTAS but no SSRSdsTASSharedDataSourcePath variable provided")
#>
				
			}

		
		}
		catch [System.IO.IOException]
		{
			Write-Error ("Error while reading rdl file : '{0}', Message: '{1}'" -f $rdlFile, $_.Exception.Message)
		}
		catch [System.Web.Services.Protocols.SoapException]
		{
			Write-Error ("Error while uploading rdl file : '{0}', Message: '{1}'" -f $rdlFile, $_.Exception.Detail.InnerText)
		}
	}

	#################################################################
	###                   Function Delete                         ###
	#################################################################
	
	function Delete-Report
	{
	  param( [string]$SSRSReportFolder , [string]$RDLtoDelete)
	  
		$ssrsProxy = New-WebServiceProxy -Uri $ssrsreportserverurl -UseDefaultCredential 
			try
			{
				$ToDeleteRDL = $RDLtoDelete -replace ".rdl",""
				$RDLtoDelete = $reportPath +"/"+ $ToDeleteRDL
				$RDLtoDelete
				$ssrsProxy.DeleteItem($RDLtoDelete)
				Write-Host "Deleted Report: $RDLtoDelete"
			}
			catch [System.Web.Services.Protocols.SoapException]
			{
				if ($_.Exception.Detail.InnerText -match "[^rsItemAlreadyExists400]")
				{
					Write-Host -fore Yellow "Deleted: $RDLtoDelete already "
				}
				else
				{
					$msg = "Error Deleting Report: $RDLtoDelete. Msg: '{0}'" -f $_.Exception.Detail.InnerText
					Write-Error $msg
				}
			}
	}
	
	
	
	#################################################################
	###                     Upload Reports                        ###
	#################################################################
	
	$reportPath = [string]::Concat("/", $reportPath, $SSRSReportFolder)

	$reportPath
"ddddd"
	Get-ChildItem $ReportsLocation -Filter *.rdl | 
	Foreach-Object{
	$_.Name
	   Delete-Report $reportPath $_.Name
	  Create-Report $_.FullName -force -reportFolder $reportPath -reportName $_.Name -reportCredentialsUsername $DataSourceUserName  -reportCredentialsPassword $DataSourceUserPassword
	}

	
	
	#################################################################
	###                     Upload Reports                        ###
	#################################################################
	
	
	
