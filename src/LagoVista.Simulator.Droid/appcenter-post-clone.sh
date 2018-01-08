#!/usr/bin/env bash

# Script to update files in android build to apply version numbers, icons and package name based on environment
# Expects environment variables setup as:
#  APPCENTER_APPID  Custom Variable with key for app center telemetry
#  PACKAGENAME Custom variable with name of package used to identify application, we need different package names in all environments
#  APPCENTER_SOURCE_DIRECTORY Varialbe from build environment as the root directory where all source code files live
#  APPCENTER_BRANCH dev, stage or master, used to identify environment to set URI and grab correct icons

# Set to directory name within /src where native project lives
droidProjectName=LagoVista.Simulator.Droid

# Set to directory name within /src where common Xamarin forms app lives
appProjectName=LagoVista.Simulator

# Version is pulled from a file in the root, it contains first two digits of full version, such as 1.0
version=$(<$APPCENTER_SOURCE_DIRECTORY/version.txt)

# Build number is number of days since May 17, 2017
platform=`uname -s`
if [ "$platform" = "Darwin" ]; then
    buildNumber=$(((`date +%s` - `date -jf"%Y%m%d%H%M%S" "20170517000000" +%s`)/86400))
    echo 'running on mac'
else
    buildNumber=$(((`date +%s` - `date -d"%Y%m%d%H%M%S" "20170517000000" +%s`)/86400))
    echo 'running on linux'
fi

# Revision is four digit number, always zero padded HHMM
revision=`date +"%H%M"`

# Version to be displayed to the user 1.0.219.0818
fullversion=$version.$buildNumber.$revision

# Version to be set in manifest for android build number 2190818
versionCode=$buildNumber$revision

# set the package name for Android, need to set main name as well as permissions for sending notifications
sed -i '' 's/package\s*=\s*\"[a-z\._]*\"/package=\"'"$PACKAGENAME"'\"/' $APPCENTER_SOURCE_DIRECTORY/src/$droidProjectName/Properties/AndroidManifest.xml
sed -i '' 's/name\s*=\s*\"[a-z\._]*C2D_MESSAGE\"/name=\"'"$PACKAGENAME.C2D_MESSAGE"'\"/' $APPCENTER_SOURCE_DIRECTORY/src/$droidProjectName/Properties/AndroidManifest.xml

# Update version code and version name from what we built earlier in the script
sed -i '' 's/versionCode\s*=\s*\"[0-9]*\"/versionCode=\"'"$versionCode"'\"/' $APPCENTER_SOURCE_DIRECTORY/src/$droidProjectName/Properties/AndroidManifest.xml
sed -i '' 's/versionName\s*=\s*\"[0-9\.]*\"/versionName=\"'"$fullversion"'\"/' $APPCENTER_SOURCE_DIRECTORY/src/$droidProjectName/Properties/AndroidManifest.xml

# within the script, the branch name identifies the server to be used, however it must always be upper case to match conditional compile in code
val=$(echo "$APPCENTER_BRANCH" | tr '[:lower:]' '[:upper:]' )
sed -i '' 's/#define ENV.*/#define ENV_'"$val"'/' $APPCENTER_SOURCE_DIRECTORY/src/$appProjectName/App.xaml.cs

# Set the unique mobile center key to capture telemetry
sed -i '' 's/MOBILE_CENTER_KEY = \"[0-9A-Fa-f\-]*\";/MOBILE_CENTER_KEY = \"'"$APPCENTER_APPID"'\";/' $APPCENTER_SOURCE_DIRECTORY/src/$droidProjectName/MainActivity.cs

# icons are setup by branch name so just copy everything over
cp -R $APPCENTER_SOURCE_DIRECTORY/BuildAssets/Android/$APPCENTER_BRANCH/ $APPCENTER_SOURCE_DIRECTORY/src/$droidProjectName/Resources
