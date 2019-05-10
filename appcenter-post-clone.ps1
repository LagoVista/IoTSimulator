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
Copy-Item -Path ".\BuildAssets\UWP\$branch\*"  -Destination "$uwpprojectdir" -Force

$versionFile = "$scriptPath\version.txt"

[string] $versionContent = Get-Content $versionFile;
$revisionNumber = Generate-VersionNumber
$versionNumber = "$versionContent.$revisionNumber"
$packageVersionNumber = "$versionContent.0.0"
"Done setting version: $versionNumber"
"Package Version Number: $packageVersionNumber"


# Set the App Identity in the app manifest
[xml] $content = Get-Content  $appmanifestFile
$content.Package.Identity.Name = %UWPAPPIDENTITY%
$content.Package.Identity.Version = $packageVersionNumber
$content.save($appmanifestFile)
"Set App Identity: %UWPAPPIDENTITY% and package version $packageVersionNumber in $appmanifestFile"

# Set the App Identity in the Store Association File
[xml] $storeContent = (Get-Content  $storeAssociationFile) 
$storeContent.StoreAssociation.ProductReservedInfo.MainPackageIdentityName = $uwpappidentity
$storeContent.save($storeAssociationFile)
"Set App Identity: %UWPAPPIDENTITY% in $storeAssociationFile"

# Set the App Center Id for the current app.
[string] $uwpAppFileContent = (Get-Content $uwpAppFile) -join "`r`n"
$regEx = "MOBILE_CENTER_KEY = \""[0-9a-f\-]+\"";"
$uwpAppFileContent = $uwpAppFileContent -replace $regEx, "MOBILE_CENTER_KEY = ""%APPCENTER_APPID%"";";
$uwpAppFileContent | Set-Content $uwpAppFile
"Set App CenterId: %APPCENTER_APPID% in $uwpAppFile"

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
"CUrrent Branch [$branch]"

$ucaseEnvironment = %APPCENTER_BRANCH%.ToUpper();

"Tag replace [$ucaseEnvironment%] [%APPCENTER_BRANCH%] [%APPCENTER_APPID%]"

$mainAppContent = $mainAppContent -replace $envRegEx, "#define ENV_$ucaseEnvironment";

"$mainAppContent"

$mainAppContent | Set-Content $mainAppFile
"Set $ucaseEnvironment in $mainAppFile"
"------------------------------------------------"
""
