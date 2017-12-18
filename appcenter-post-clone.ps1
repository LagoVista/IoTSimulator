function Generate-VersionNumber() {
    $end = Get-Date
    $start = Get-Date "5/17/2017"

    $today = Get-Date
    $today = $today.ToShortDateString()
    $today = Get-Date $today

    $revisionNumber = New-TimeSpan -Start $start -End $end
    $minutes = New-TimeSpan -Start $today -End $end
	
	$buildNumber =  ("{0:00}" -f ([math]::Round($end.Month))) + ("{0:00}" -f ([math]::Round($end.Day)))
	$revisionNumber = ("{0:00}" -f [math]::Round($minutes.Hours)) + ("{0:00}" -f ([math]::Round($minutes.Minutes)))

	return "$buildNumber.$revisionNumber"

}

$scriptPath = (Split-Path $MyInvocation.MyCommand.Path);
$scriptPath

"Copy Environment Specific Icons"
$env:APPCENTER_BRANCH
Copy-Item -Path ".\BuildAssets\UWP\$env:APPCENTER_BRANCH\*"  -Destination ".\LagoVista.Simulator.Windows\Assets" -Force

$versionFile = "$scriptPath\version.txt"

[string] $versionContent = Get-Content $versionFile;
$revisionNumber = Generate-VersionNumber
$versionNumber = "$versionContent.$revisionNumber"
"Done setting version: $versionNumber"

# Set the App Identity in the app manifest
$appmanifestFile = "$scriptPath\src\LagoVista.Simulator.Windows\Package.appxmanifest"
$appmanifestFile
[xml] $content = Get-Content  $appmanifestFile
$content.Package.Identity.Name
$content.Package.Identity.Name = $env:UWPAPPIDENTITY
$content.Package.Identity.Version = $versionNumber
$content.save($appmanifestFile)

# Set the App Identity in the Store Association File
$storeAssociationFile = "$scriptPath\src\LagoVista.Simulator.Windows\Package.StoreAssociation.xml"
[xml] $storeContent = (Get-Content  $storeAssociationFile) 
$storeContent.StoreAssociation.ProductReservedInfo.MainPackageIdentityName
$storeContent.StoreAssociation.ProductReservedInfo.MainPackageIdentityName = $env:UWPAPPIDENTITY
$storeContent.save($storeAssociationFile)

# Set the App Center Id for the current app.
$uwpAppFile = "$scriptPath\src\LagoVista.Simulator.Windows\App.xaml.cs"
[string] $uwpAppFileContent = (Get-Content $uwpAppFile) -join "`r`n"
$regEx = "MOBILE_CENTER_KEY = \""[0-9a-f\-]+\"";"
$uwpAppFileContent = $uwpAppFileContent -replace $regEx, "MOBILE_CENTER_KEY = ""$env:APPCENTER_APPID"";";
$uwpAppFileContent | Set-Content $uwpAppFile
"Set $env:APPCENTERID in UWP\App.xaml.cs"

# Set the Version Numbers in the AssemblyInfo.cs file.
$assemblyInfoFile = "$scriptPath\src\LagoVista.Simulator.Windows\Properties\AssemblyInfo.cs"
[string] $assemblyInfoContent = (Get-Content $assemblyInfoFile) -join "`r`n"
$regEx = "assembly: AssemblyVersion\(\""[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\""\)"
$assemblyInfoContent = $assemblyInfoContent -replace $regEx,  "assembly: AssemblyVersion(""$versionNumber"")"
$regEx = "assembly: AssemblyFileVersion\(\""[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\""\)"
$assemblyInfoContent = $assemblyInfoContent -replace $regEx,  "assembly: AssemblyFileVersion(""$versionNumber"")"
$assemblyInfoContent | Set-Content  $assemblyInfoFile 
"Set version $versionNumber in AssemblyInfo.cs"

$mainAppFile = "$scriptPath\src\LagoVista.Simulator.Windows\App.xaml.cs"
[string] $mainAppContent = (Get-Content $mainAppFile) -join "`r`n"
$envRegEx = "#define ENV_[A-Z]+"
$ucaseEnvironment = $env:APPCENTER_BRANCH.ToUpper();
$mainAppContent = $mainAppContent -replace $envRegEx, "#define ENV_$ucaseEnvironment";
$mainAppContent | Set-Content $mainAppFile
"Set $env:APPCENTER_BRANCH in Simulator\App.xaml.cs"
