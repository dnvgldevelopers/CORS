param (
	[switch]$Install,
	[switch]$Uninstall,
	[switch]$Enable,
	[switch]$Disable,
	[switch]$Configure,
	[switch]$AllowFraming,
	
	[string]$Dll,
	[string]$Env,
	[string]$Url
)

function Install()
{
	if(![System.Diagnostics.EventLog]::SourceExists($eventSource))
	{
		Write-Host "Registering '" $eventSource "' as windows event source"
		[System.Diagnostics.EventLog]::CreateEventSource($eventSource, "Application")
		[System.Diagnostics.EventLog]::WriteEntry($eventSource, $eventSource + " registered as windows event source", [System.Diagnostics.EventLogEntryType]::Information);
		Write-Host "Server has to be re-booted once in order for source to work correctly" -ForegroundColor Yellow
	}

	Write-Host "Installing 'VerIT.WebExtensions.CORS.dll' to GAC"
	[System.Reflection.Assembly]::LoadWithPartialName("System.EnterpriseServices")| Out-Null
	$dllLocation = Resolve-Path -Path $Dll		
	$publish = New-Object System.EnterpriseServices.Internal.Publish
	$publish.GacInstall($dllLocation)	
	
	Write-Host "Complete" -ForegroundColor Green
}

function Uninstall()
{
	if([System.Diagnostics.EventLog]::SourceExists($eventSource))
	{
		Write-Host "Unregistering '" $eventSource "' as windows event source"
		[System.Diagnostics.EventLog]::WriteEntry($eventSource, $eventSource + " deleted as windows event source", [System.Diagnostics.EventLogEntryType]::Information);	
		[System.Diagnostics.EventLog]::DeleteEventSource($eventSource)		
	}
	
	Write-Host "Deleting 'VerIT.WebExtensions.CORS.dll' from GAC ..."
	Remove-Item "C:\Windows\Microsoft.NET\assembly\GAC_MSIL\VerIT.WebExtensions.CORS\*" -recurse
}

function Enable()
{
	if($Url -eq $null -or $Url.Length -lt 1)
	{	
		Write-Host ".\deploy-cors.ps1 -Enable -Url 'SharePoint web application url' " -ForegroundColor Red		
	} 
	else
	{
		Write-Host "Enabling CORS Http Module 'VerIT.WebExtensions.CORS' in Web.Config"

		# Modifying Web.Config using Microsoft.SharePoint.Administration.SPWebConfigModification

		$webapp = Get-SPWebApplication -Identity $Url
		$mods = $webapp.WebConfigModifications
		$webConfigModFound = $false
		$webConfigAppFound = $false
		$webConfigIFrameFound = $false
		$changed = $false
		$updated = $false
		foreach($mod in $mods)
		{
			if($mod.Name -eq $webConfigModName) 
			{		
				$webConfigModFound = $true		
				if($mod.Value -ne $webConfigModValue)
				{
					Write-Host "Updating web configuration modification -> " $mod.Name $webConfigModValue 
					$mod.Value = $webConfigModValue
					$changed= $true
				}
			}
			if($mod.Name -eq $webConfigAppName) 
			{
				$webConfigAppFound = $true		
				if($mod.Value -ne $webConfigAppValue)
				{ 
					Write-Host "Updating web configuration modification -> " $mod.Name  $webConfigAppValue	
					$mod.Value = $webConfigAppValue			
					$changed= $true		
				}
			}
			if($mod.Name -eq $webConfigIFrameName) 
			{
				$webConfigIFrameFound= $true		
				if($mod.Value -ne $webConfigIFrameValue)
				{
					Write-Host "Updating web configuration modification -> " $mod.Name $webConfigIFrameValue
					$mod.Value = $webConfigIFrameValue			
					$changed= $true		
				}
			}
		}

		if($changed) 
		{
			Write-Host "Saving... "
			$webapp.Update()
			Write-Host "Applying... "
			$webapp.Parent.ApplyWebConfigModifications()
			Write-Host "Complete" -ForegroundColor Green
			$updated = $true
		}
		
		if(!$webConfigModFound) 
		{
			Write-Host "Adding web configuration modification -> " $webConfigModName $webConfigModValue
			$configCorsModule = New-Object Microsoft.SharePoint.Administration.SPWebConfigModification
			$configCorsModule.Path = "/configuration/system.webServer/modules"
			$configCorsModule.Name = $webConfigModName
			$configCorsModule.Value = $webConfigModValue
			$webapp.WebConfigModifications.Add($configCorsModule)
			$changed= $true	
		}
		
		if(!$webConfigAppFound) 
		{
			Write-Host "Adding web configuration modification -> " $webConfigAppName $webConfigAppValue
			$configCorsFile = New-Object Microsoft.SharePoint.Administration.SPWebConfigModification
			$configCorsFile.Path = "configuration/appSettings"
			$configCorsFile.Name = $webConfigAppName
			$configCorsFile.Value = $webConfigAppValue			
			$webapp.WebConfigModifications.Add($configCorsFile)
			$changed= $true	
		}
		
		if(!$webConfigIFrameFound) 
		{
			Write-Host "Adding web configuration modification -> " $webConfigIFrameName $webConfigIFrameValue
			$configAllowFraming = New-Object Microsoft.SharePoint.Administration.SPWebConfigModification
			$configAllowFraming.Path = "configuration/appSettings"
			$configAllowFraming.Name = $webConfigIFrameName
			$configAllowFraming.Value = $webConfigIFrameValue			
			$webapp.WebConfigModifications.Add($configAllowFraming)
			$changed= $true	
		}
		
		if($changed) 
		{
			Write-Host "Saving... "
			$webapp.Update()
			Write-Host "Applying... "
			$webapp.Parent.ApplyWebConfigModifications()
			Write-Host "Complete" -ForegroundColor Green
			$updated = $true
		}

		if(!$updated)
		{
			Write-Host "No updates needed" -ForegroundColor Yellow
		}
	}
}

function Disable()
{
	if($Url -eq $null -or $Url.Length -lt 1)
	{	
		Write-Host ".\deploy-cors.ps1 -Disable -Url 'SharePoint web application url' " -ForegroundColor Red		
	} 
	else
	{
		Write-Host "Disabling CORS Http Module 'VerIT.WebExtensions.CORS' in Web.Config"
		$webapp = Get-SPWebApplication -Identity $Url
		$mods = $webapp.WebConfigModifications
		$found = $false
		for($i = ($mods.Count -1); $i -ge 0; $i--)
		{
			$mod = $mods[$i]
			if($mod.Name -eq $webConfigModName) 
			{
				$found= $true								
				Write-Host "Removing web configuration modification ->" $mod.Name
				$webapp.WebConfigModifications.Remove($mod)	| Out-Null			
			}

			if($mod.Name -eq $webConfigAppName) 
			{
				$found= $true								
				Write-Host "Removing web configuration modification ->" $mod.Name
				$webapp.WebConfigModifications.Remove($mod)	| Out-Null				
			}

			if($mod.Name -eq $webConfigIFrameName) 
			{
				$found= $true								
				Write-Host "Removing web configuration modification ->" $mod.Name
				$webapp.WebConfigModifications.Remove($mod)	| Out-Null				
			}
		}

		if($found)
		{
			Write-Host "Saving... "
			$webapp.Update()
			Write-Host "Applying... "
			$webapp.Parent.ApplyWebConfigModifications()
			Write-Host "Complete" -ForegroundColor Green
		}
		else
		{
			Write-Host "No web configuration modifications to remove found" -ForegroundColor Yellow
		}
	}
}

function Configure()
{
	if( $Env -eq $null -or $Url -eq $null -or $Url.Length -lt 1)
	{	
		Write-Host ".\deploy-cors.ps1 -Configure -Env environment -Url 'SharePoint web application url'" -ForegroundColor Red		
	} 
	else
	{
		Write-Host "Configuring CORS origin whitelist 'VerIT.WebExtensions.CORS' in ~/App_Data/cors-config.txt for " $Url
		
		$webapp = Get-SPWebApplication -Identity $Url		

		[System.Configuration.Configuration] $webConfig = [System.Web.Configuration.WebConfigurationManager]::OpenWebConfiguration("/",$webapp.Name)
		$length = $webConfig.FilePath.LastIndexOf("\")
		$virtualPath = $webConfig.FilePath.Substring(0,$length)
		$app_data = $virtualPath + "\App_Data" 
		[System.IO.Directory]::CreateDirectory($app_data) | Out-Null
		$source = "cors-config-" + $Env + ".txt"
		$destination = $app_data + "\cors-config.txt"
		Copy-Item -Path $source -Destination $destination -Force
		Write-Host "Copied" $source "to" $destination -ForegroundColor Green
	}
}

Write-Host "============================================================"
Write-Host "|"
Write-Host "|  Cross Origin Resource Sharing (CORS) Deployment"
Write-Host "|" 
Write-Host "============================================================"

$eventSource = "VerIT.WebExtensions.CORS.CorsHttpModule"

$webConfigModName = "add[@name='CorsHttpModule']"
$webConfigModValue = "<add name='CorsHttpModule' preCondition='integratedMode' type='VerIT.WebExtensions.CORS.CorsHttpModule, VerIT.WebExtensions.CORS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a46e2ed1027771e2' />"

$webConfigAppName = "add[@key='CorsConfigFile']"
$webConfigAppValue = "<add key='CorsConfigFile' value='App_Data\cors-config.txt' />"

$webConfigIFrameName = "add[@key='AllowFraming']"
if($AllowFraming)
{
	$webConfigIFrameValue = "<add key='AllowFraming' value='true' />"
}
else 
{
	$webConfigIFrameValue = "<add key='AllowFraming' value='false' />"
}

if ($Install)
{
	Install
}
elseif ($Uninstall)
{
	Uninstall
}
elseif ($Enable)
{
	Enable	
}
elseif ($Disable)
{
	Disable
}
elseif ($Configure)
{
	Configure
}
else
{
	Write-Host ""
	Write-Host "What would you like to do?" -ForegroundColor  Yellow
	Write-Host ""
	Write-Host " .\deploy-cors.ps1 -Install -Dll path/to/VerIT.WebExtensions.CORS.dll          (Install DLL into GAC and register new event source)" -ForegroundColor  Yellow
	Write-Host " .\deploy-cors.ps1 -Uninstall                                                  (Uninstall DLL from GAC and unregister event source)" -ForegroundColor  Yellow
	Write-Host " .\deploy-cors.ps1 -Enable -Url https://sharepoint-webapp -AllowFraming        (Enable CorsHttpModule for sp web application (web.config))" -ForegroundColor  Yellow
	Write-Host " .\deploy-cors.ps1 -Disable -Url https://sharepoint-webapp                     (Disable CorsHttpModule for sp web application (web.config))" -ForegroundColor  Yellow
	Write-Host " .\deploy-cors.ps1 -Configure -Url https://sharepoint-webapp -Env environment  (Configure CorsHttpModule for web application (cors-config.txt))" -ForegroundColor  Yellow
	Write-Host ""
}