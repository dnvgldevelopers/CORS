param (
	[switch]$Debug
)
############################################################
#
# This script is used to build a deployment package of 
# scripts and configuration for the Internal search farm
#
############################################################

# PowerShell Community Extensions 3.1.0
# http://pscx.codeplex.com/
# http://pscx.codeplex.com/downloads/get/744915

echo "-----------------------------------------------------------------"
echo "This script uses PowerShell Community Extensions"
echo "Download and install 3.1.0 er newer from http://pscx.codeplex.com"
echo "Note! You may need to restart your PowerShell console"
echo "-----------------------------------------------------------------"

Import-Module Pscx

$dir = $(Get-Location)
$deploy = $dir.Path + "\_deploy"

echo "--> Setting up temp folders"

if(Test-Path $deploy)
{	
	Remove-Item $deploy -Force -Recurse 
}
New-Item -ItemType Directory "_deploy" | Out-Null

echo "--> Copying scripts and customizations for to the temp folders"
copy-item ($dir.Path + "\*.txt") -destination $deploy -recurse
copy-item ($dir.Path + "\deploy-cors.ps1") -destination $deploy -recurse

if($Debug)
{
	Write-Host "--> Including Debug version of DLL in package" -ForegroundColor Yellow
	copy-item ($dir.Path + "\VerIT.WebExtensions.CORS\bin\Debug\VerIT.WebExtensions.CORS.dll") -destination $deploy 
}
else
{
	Write-Host "--> Including Release version of DLL in package" -ForegroundColor Yellow
	copy-item ($dir.Path + "\VerIT.WebExtensions.CORS\bin\Release\VerIT.WebExtensions.CORS.dll") -destination $deploy
}

echo "--> Packaging scripts and customizations"
cd $deploy
$package = $deploy + "\cors-httpmodule-" + $(Get-date -Format yyyy-MM-dd) + ".zip"
Write-Zip * -OutputPath $package | Out-Null
cd $dir
Write-Host "--> Deployment package  available at $($package)" -ForegroundColor Green
