	$SSRS_Server = "NIBS"
	$ReportsLocation ="./Reports";
	
	#################################################################
	###                   Fixed Parameters                        ###
	#################################################################
	
	$SSRSReportServerUrl  = "http://"+$SSRS_Server+"/ReportServer/ReportService2010.asmx"
$reportserver = "NIBS";
$newDataSourcePath = "/Coast"
$newDataSourceName = "Temp_DataSource";
$reportFolderPath = "/Coast"
#------------------------------------------------------------------------
#$ssrs  = "http://"+$SSRS_Server+"/ReportServer/ReportService2010.asmx"
#$ssrs = New-WebServiceProxy -uri $url -UseDefaultCredential
	$ssrsProxy = New-WebServiceProxy -Uri $ssrsreportserverurl -UseDefaultCredential
	
$reports = $ssrsProxy.ListChildren($reportFolderPath, $false)
$reports | ForEach-Object {
            $reportPath = $_.path

			
			if($reportPath -ne "/Coast/Temp_DataSource")
			{ 
			Write-Host "Report: "  $reportPath 
			 $dataSources = $ssrsProxy.GetItemDataSources($reportPath); 
			 
			 
			 
    if ($datasources.count -gt 0){ 
        for ($i = 0; $i -lt $dataSources.count; $i++) { 
		
		Write-Host "Reportssssss: "  $reportPath

		
            if ($DataSources[$i].name -eq $newDataSourceName){ 
				write-output "*************"
				$newDataSourceName 
		$DataSources[$i].name
			write-output "*************"
                write-output "Updating $($DataSources[$i].name)"; 
                $proxyNamespace = $DataSources[$i].GetType().Namespace; 
                $DataSources[$i].Item = New-Object ("$proxyNamespace.DataSourceReference"); 
                $DataSources[$i].Item.Reference = "Coast/ACT0074"#$DataSourceURL; 
				write-output "------------"; 
				$reportPath
				$DataSources[$i]
				write-output "------------"; 
				
                $ssrsProxy.SetItemDataSources($reportPath, $DataSources[$i]) 
                write-output "Done"; 
            } 
        } 
    } 
			
			}
		#	
			#$dataSources
			}
<#
$reports | ForEach-Object {
            $reportPath = $_.path
            Write-Host "Report: " $reportPath
            $dataSources = $ssrs.GetItemDataSources($reportPath)
            $dataSources | ForEach-Object {
                            $proxyNamespace = $_.GetType().Namespace
                            $myDataSource = New-Object ("$proxyNamespace.DataSource")
                            $myDataSource.Name = $newDataSourceName
                            $myDataSource.Item = New-Object ("$proxyNamespace.DataSourceReference")
                            $myDataSource.Item.Reference = $newDataSourcePath

                            $_.item = $myDataSource.Item

                            $ssrs.SetItemDataSources($reportPath, $_)

                            Write-Host "Report's DataSource Reference ($($_.Name)): $($_.Item.Reference)"
                            }

            Write-Host "------------------------" 
            }
	
	#>	
	