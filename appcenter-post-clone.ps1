function Generate-VersionNumber() {
    $end = Get-Date
    $start = Get-Date "5/17/2017"

    $today = Get-Date
    $today = $today.ToShortDateString()
    $today = Get-Date $today

    $revisionNumber = New-TimeSpan -Start $start -End $end
    $minutes = New-TimeSpan -Start $today -End $end
	
	$buildNumber =  ($end-$start).Days
	$revisionNumber = ("{0}" -f [math]::Round($minutes.Hours)) + ("{0:00}" -f ([math]::Round($minutes.Minutes)))

	return "$buildNumber.$revisionNumber"
}

$scriptPath = (Split-Path $MyInvocation.MyCommand.Path);

$coreappdir = "$scriptPath\src\LagoVista.Simulator"
$uwpprojectdir = "$scriptPath\src\LagoVista.Simulator.Windows"
$uwpprojectfile = "$uwpprojectdir\LagoVista.Simulator.Windows.csproj"
$appmanifestFile = "$uwpprojectdir\Package.appxmanifest"
$storeAssociationFile = "$uwpprojectdir\Package.StoreAssociation.xml"
$uwpAppFile = "$uwpprojectdir\App.xaml.cs"
$assemblyInfoFile = "$uwpprojectdir\Properties\AssemblyInfo.cs"
$mainAppFile = "$coreappdir\App.xaml.cs"

"Copy Environment Specific Icons"
Copy-Item -Path ".\BuildAssets\UWP\$branch\*"  -Destination "$uwpprojectdir\Assets" -Force

$versionFile = "$scriptPath\version.txt"

[string] $versionContent = Get-Content $versionFile;
$revisionNumber = Generate-VersionNumber
$versionNumber = "$versionContent.$revisionNumber"
$packageVersionNumber = "$versionContent.0.0"
"Done setting version: $versionNumber"
"Package Version Number: $packageVersionNumber"

# Set the App Identity in the app manifest
[xml] $content = Get-Content  $appmanifestFile
#$content.Package.Properties

$content.Package.Identity.Name = $env:UWPAPPIDENTITY
$content.Package.Identity.Version = $packageVersionNumber
$content.Package.Properties.DisplayName = $env:APP_DISPLAY_NAME
#$content.Package.Applications.Application.DisplayName = $env:APP_DISPLAY_NAME

$content.GetElementsByTagName("VisualElements", "http://schemas.microsoft.com/appx/manifest/uap/windows10") | 
	ForEach-Object {
		$_.DisplayName = $env:APP_DISPLAY_NAME
	}

$content.save($appmanifestFile)
"Set App Identity: $env:UWPAPPIDENTITY and package version $packageVersionNumber in $appmanifestFile"

# Set the App Identity in the Store Association File
[xml] $storeContent = (Get-Content  $storeAssociationFile) 
$storeContent.StoreAssociation.ProductReservedInfo.MainPackageIdentityName = $env:UWPAPPIDENTITY
$storeContent.StoreAssociation.ProductReservedInfo.ReservedNames.ReservedName = $env:APP_DISPLAY_NAME
$storeContent.save($storeAssociationFile)
"Set App Identity: $env:UWPAPPIDENTITY in $storeAssociationFile"

# Set the App Center Id for the current app.
[string] $uwpAppFileContent = (Get-Content $uwpAppFile) -join "`r`n"
$regEx = "MOBILE_CENTER_KEY = \""[0-9a-f\-]+\"";"
$uwpAppFileContent = $uwpAppFileContent -replace $regEx, "MOBILE_CENTER_KEY = ""$env:APPCENTER_APPID"";";
$uwpAppFileContent | Set-Content $uwpAppFile
"Set App CenterId: $env:APPCENTER_APPID in $uwpAppFile"

# Set the Version Numbers in the AssemblyInfo.cs file.
[string] $assemblyInfoContent = (Get-Content $assemblyInfoFile) -join "`r`n"
$regEx = "assembly: AssemblyVersion\(\""[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\""\)"
$assemblyInfoContent = $assemblyInfoContent -replace $regEx,  "assembly: AssemblyVersion(""$versionNumber"")"
$regEx = "assembly: AssemblyFileVersion\(\""[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\""\)"
$assemblyInfoContent = $assemblyInfoContent -replace $regEx,  "assembly: AssemblyFileVersion(""$versionNumber"")"
$assemblyInfoContent | Set-Content  $assemblyInfoFile 
"Set version $versionNumber in $assemblyInfoFile"

# Set the server environment file in the Xamarin Forms App File
[string] $mainAppContent = (Get-Content $mainAppFile) -join "`r`n"
$envRegEx = "#define ENV_[A-Z]*"

$ucaseEnvironment = $env:APPCENTER_BRANCH.ToUpper();

"Tag replace [$ucaseEnvironment] [$env:APPCENTER_APPID]"

$mainAppContent = $mainAppContent -replace $envRegEx, "#define ENV_$ucaseEnvironment";

$mainAppContent | Set-Content $mainAppFile
"------------------------------------------------"
""
