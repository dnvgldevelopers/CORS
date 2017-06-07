param (	
	[string]$Dll
)
[System.Reflection.Assembly]::LoadWithPartialName("System.EnterpriseServices")| Out-Null
$dllLocation = Resolve-Path -Path $Dll		
$publish = New-Object System.EnterpriseServices.Internal.Publish
$publish.GacInstall($dllLocation)	