#!/usr/bin/env bash

if [ "$APPCENTER_BRANCH" == "dev" ];
then
    plutil -replace "Loc IoT Sim" -string "Dev IoT Sim" $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist
    plutil -replace com.software-logistics.dev.simlocal -string com.software-logistics.dev.simdev $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist
fi

if [ "$APPCENTER_BRANCH" == "stage" ];
then
    plutil -replace "Loc IoT Sim" -string "Stg IoT Sim" $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist
    plutil -replace com.software-logistics.dev.local -string com.software-logistics.dev.simstg $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist
fi

if [ "$APPCENTER_BRANCH" == "master" ];
then
    plutil -replace "Loc IoT Sim" -string "IoT Simulator" $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist
    plutil -replace com.software-logistics.dev.local -string com.software-logistics.iot-simulator $APPCENTER_SOURCE_DIRECTORY/src/LagoVista.Simulator.iOS/Info.plist
fi
