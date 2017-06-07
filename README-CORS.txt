#--------------------------------------------------------------------------------
| How to build and package
#--------------------------------------------------------------------------------

Build using Visual Studio

.\package-cors.ps1 -Debug		# Include Debug version of DLL in package
.\package-cors.ps1			# Include Release version of DLL in package

#--------------------------------------------------------------------------------
| For Deployment
#--------------------------------------------------------------------------------

# Intranet
.\deploy-cors.ps1 -Install -Dll .\VerIT.WebExtensions.CORS.dll
.\deploy-cors.ps1 -Configure -Env prod -Url https://internalwebapps.mydomain.com
.\deploy-cors.ps1 -Enable -Url https://internalwebapps.mydomain.com

