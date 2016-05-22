param(		 [string]$SSRSReportFolder 
			,[string]$SSRSReportServerUrl 
		
	)
	
	#################################################################
	###                     Create SSRS Folder                    ###
	#################################################################
	
	function Create-ReportFolder
	{
	
	 param( [string]$SSRSReportFolder , [string]$reportPath)
		$ssrsProxy = New-WebServiceProxy -Uri $ssrsreportserverurl -UseDefaultCredential 

			try
			{
				$ssrsProxy.CreateFolder($SSRSReportFolder, $reportPath, $null)
				Write-Host "Created new folder: $SSRSReportFolder"
			}
			catch [System.Web.Services.Protocols.SoapException]
			{
				if ($_.Exception.Detail.InnerText -match "[^rsItemAlreadyExists400]")
				{
					Write-Host -fore Yellow "Folder: $SSRSReportFolder already exists."
				}
				else
				{
					$msg = "Error creating folder: $SSRSReportFolder. Msg: '{0}'" -f $_.Exception.Detail.InnerText
					Write-Error $msg
				}
			}
	}


	#################################################################
	###                     Create Main Folder                    ###
	#################################################################

	$reportPath  = "/"
	Create-ReportFolder  $SSRSReportFolder $reportPath

	


