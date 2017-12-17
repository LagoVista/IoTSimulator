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

"Copy Environment Specific Icons"
$env:APPCENTER_BRANCH
Copy-Item -Path ".\BuildAssets\UWP\$env:APPCENTER_BRANCH\*"  -Destination ".\src\LagoVista.Simulator.Windows\Assets" -Force

$appmanifestFile = "$scriptPath\src\LagoVista.Simulator.Windows\Package.appxmanifest"
$storeAssociationFile = "$scriptPath\src\LagoVista.Simulator.Windows\Package.StoreAssociation.xml"
$versionFile = "$scriptPath\version.txt"
$assemblyInfoFile = "$scriptPath\src\LagoVista.Simulator.Windows\Properties\AssemblyInfo.cs"

[string] $versionContent = Get-Content $versionFile;
$revisionNumber = Generate-VersionNumber
$versionNumber = "$versionContent.$revisionNumber"
$versionNumber

[xml] $content = Get-Content  $appmanifestFile
$content.Package.Identity.Name
$content.Package.Identity.Name = $env:UWPAPPIDENTITY
$content.Package.Identity.Version = $versionNumber
$content.save($appmanifestFile)

[xml] $storeContent = (Get-Content  $storeAssociationFile) 
$storeContent.StoreAssociation.ProductReservedInfo.MainPackageIdentityName
$storeContent.StoreAssociation.ProductReservedInfo.MainPackageIdentityName = $env:UWPAPPIDENTITY
$storeContent.save($storeAssociationFile)

[string] $assemblyInfoContent = (Get-Content $assemblyInfoFile) -join "`r`n"
$regEx = "assembly: AssemblyVersion\(\""[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\""\)"
$assemblyInfoContent = $assemblyInfoContent -replace $regEx,  "assembly: AssemblyVersion(""$versionNumber"")"
$regEx = "assembly: AssemblyFileVersion\(\""[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\""\)"
$assemblyInfoContent = $assemblyInfoContent -replace $regEx,  "assembly: AssemblyFileVersion(""$versionNumber"")"
$assemblyInfoContent | Set-Content  $assemblyInfoFile 