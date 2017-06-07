
#CORS Module

-	Cross Origin Resource Sharing
-	Cross Origin Framing

#OVERVIEW

##Cross Origin Resource Sharing

The background for developing the CORS module is that SharePoint is installed on multiple farms and also set up using host named site collections. This means that web pages are served from multiple domain names.
Due to using client side technology for much of the SharePoint solutions and relying on search for much of the data, the primary usecase is doing client side AJAX calls from intranet.mydomain.com to search. mydomain.com. Another sample is to support ajax call from DMS (or any web application) to intranet taxonomy API, which allows the mega menu to get the menu items.
SharePoint by configuration does not have any CORS modules built in, thus we are installing an HTTP module to the WFE’s in SharePoint at the search farm to allow CORS between the domains.

##Cross Origin Framing

The component has been extended to support rendering SharePoint content in iframes originating from other domains.

 
#ARCHITECTURE

![alt text](https://github.com/dnvgldevelopers/CORS/blob/master/architecture/diagram1.png?raw=true)


![alt text](https://github.com/dnvgldevelopers/CORS/blob/master/architecture/diagram2.png?raw=true)

#CUSTOMIZATIONS

The CORS module consists of a DLL assembly, a text configuration file and modification to web.config to register the HTTP module. The text configuration file is not needed to support cross origin framing.
The module has to be installed on all WFE’s responding to CORS requests and on all WFE’s rendering content to be displayed in iframes on originating from other domains.

##Cross Origin Resource Sharing

The configuration file allows configuration of both the listener domain and the client caller domain in any configuration which allows host named site collections to be used on both the server and client farm.
The feature adds the Access-Control-Allow-Origin HTTP response header and sets the value to the ORIGIN HTTP request header if the origin is configured for the requested domain. The Access-Control-Allow-Credentials HTTP response header is set to true under the same conditions.
The CORS feature is only enabled if Web.Config AppSettings contains the key CorsConfigFile with a file reference to the configuration file, and that the configuration file can be successfully read.

##Cross Origin Framing

The HttpModule has been extended to remove the X-FRAME-OPTIONS HTTP response header that SharePoint 2013 sends.
This feature is only enabled if Web.Config AppSettings contains the key AllowFraming with value true.
The feature can be enabled on SharePoint web applications by using –AllowFraming switch when enabling the cors module.

#BUILD AND PACKAGE

##Prerequisites

 - Powershell Community Extensions https://pscx.codeplex.com/
 - Sharepoint Farm DLL.


##Build using Visual Studio

- .\package-cors.ps1 -Debug	               (Include Debug version of DLL in package)
- .\package-cors.ps1			           (Include Release version of DLL in package)

#DEPLOYMENT

Follow the steps  modifying the url by the correct one.

- .\deploy-cors.ps1 -Install -Dll .\VerIT.WebExtensions.CORS.dll
- .\deploy-cors.ps1 -Configure -Env prod -Url https://internalwebapps.mydomain.com
- .\deploy-cors.ps1 -Enable -Url https://internalwebapps.mydomain.com
 

 --
