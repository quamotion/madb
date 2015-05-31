| build |
|-------|
| [![Build Status](https://ci.appveyor.com/api/projects/status/github/quamotion/madb)](https://ci.appveyor.com/project/qmfrederik/madb/)

# madb
This is a Managed port of the Android Debug Bridge to allow communication from .NET applications to Android devices. 
This wraps the same methods that the ddms uses to directly communicate with ADB. 
This gives more flexibility to the developer then launching an adb process and executing one of its build in commands.

## FileSystem Methods / Properties
* Create 
* Move 
* Copy 
* MakeDirectory 
* Exists 
* Chmod 
* Delete 
* IsMountPointReadOnly 
* DeviceBlocks - Get a collection of the device blocks 
* Mount 
* Unmount 
* ResolveLink - Resolves a symbolic link to its full path

## Busybox Methods / Properties
* Available 
* Version 
* Commands 
* Supports ( command ) 
* Install 
* ExecuteShellCommand 
* ExecuteRootCommand

## Device Methods / Properties
* CanSU 
* State 
* MountPoints 
* Properties 
* EnvironmentVariables 
* GetProperty 
* FileSystem 
* BusyBox 
* IsOnline 
* IsOffline 
* IsEmulator 
* IsBootLoader 
* IsRecovery 
* RemountMountPoint 
* Reboot 
* Reboot ( into ) 
* SyncService 
* PackageManager 
* FileListingService 
* Screenshot 
* ExecuteShellCommand 
* ExecuteRootShellCommand 
* InstallPackage 
* SyncPackageToDevice 
* InstallRemotePackage 
* RemoveRemotePackage 
* UninstallPackage

## FileEntry Methods / Properties
* FindOrCreate *static 
* Find *static 
* Parent 
* Name 
* LinkName 
* Info 
* Permissions 
* Size 
* Date 
* Owner 
* Group 
* Type 
* IsApplicationPackage 
* IsRoot 
* IsExecutable 
* Children 
* IsLink 
* Exists 
* FindChild 
* IsDirectory 
* IsApplicationFileName 
* FullPath 
* FullResolvedPath 
* FullEscapedPath 
* PathSegments

## PackageManager Methods / Properties
* Packages 
* RefreshPackages 
* Exists 
* GetApkFileEntry 
* GetApkPath

## SyncService
* Pull 
* PullFile 
* Push 
* PushFile
