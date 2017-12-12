#!/usr/bin/env bash

plutil -replace CFBundleDisplayName -string "$APPNAME" $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist
plutil -replace CFBundleId -string $APPBUNDLEID $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist

# set the package name for Android
sed -i '' 's/package.*=.*\".*\" /package=\"'"$PACKAGENAME"'\" /' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Droid.Simulator/Properties/AndroidManifest.xml

# set the app label Android
sed -i '' 's/android:label.*=.*\".*\" /android:label=\"'"$APPNAME"'\" /' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Droid.Simulator/Properties/AndroidManifest.xml

# set android version number
sed -i '' 's/android:versionCode.*=.*\".*\" /android:versionCode=\"'"$VERSIONCODE"'\" /' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Droid.Simulator/Properties/AndroidManifest.xml

# set android version code
sed -i '' 's/android:versionName.*=.*\".*\" /android:versionName=\"'"$VERSIONNAME"'\" /' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Droid.Simulator/Properties/AndroidManifest.xml

# within the script, the branch name identifies the server to be used, however it must always be upper case
val=$(echo "$APPCENTER_BRANCH" | tr '[:lower:]' '[:upper:]' )

# set the proper environment, conditional compile statements in clode will be used to pickup the correct server uri, will work for both iOS and Android
sed -i '' 's/#define ENV.*/#define ENV_'"$val"'/' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator/App.xaml.cs

# set the correct app center id for environment for iOS
sed -i '' 's/public const string MOBILE_CENTER_KEY = .*/public const string MOBILE_CENTER_KEY = \"'"$APPCENTERID"'\";/' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/AppDelegate.cs

# set the correct app center id for environment for Android
sed -i '' 's/public const string MOBILE_CENTER_KEY = .*/public const string MOBILE_CENTER_KEY = \"'"$APPCENTERID"'\";/' $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Droid.Simulator/MainActivity.cs

