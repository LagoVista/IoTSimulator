#!/usr/bin/env bash

plutil -replace CFBundleName -string $APPNAME $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist
plutil -replace CFBundleId -string $APPBUNDLEID $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist

# within the script, the branch name identifies the server to be used, however it must always be upper case
val=$(echo "$APPCENTER_BRANCH" | tr '[:lower:]' '[:upper:]' )

# set the proper environment, conditional compile statements in clode will be used to pickup the correct server uri
sed -i '' 's/#define ENV.*/#define ENV_'"$val"'/' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator/App.xaml.cs

echo hello there

# set the correct app center id for environment
sed -i '' 's/public const string MOBILE_CENTER_KEY = .*/public const string MOBILE_CENTER_KEY = \"'"$APPCENTERID"'\";/' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/AppDelegate.cs

# set the correct app center id for environment
sed -i '' 's/public const string MOBILE_CENTER_KEY = .*/public const string MOBILE_CENTER_KEY = \"'"$APPCENTERID"'\";/' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Droid.Simulator/MainActivity.cs

