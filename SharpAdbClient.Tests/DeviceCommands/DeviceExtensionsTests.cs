using Xunit;
using Moq;
using SharpAdbClient.DeviceCommands;
using System.Linq;

namespace SharpAdbClient.Tests.DeviceCommands
{
    public class DeviceExtensionsTests
    {
        public DeviceExtensionsTests()
        {
            Factories.Reset();
        }

        [Fact]
        public void StatTest()
        {
            FileStatistics stats = new FileStatistics();

            var client = new Mock<IAdbClient>();
            var mock = new Mock<ISyncService>();
            mock.Setup(m => m.Stat("/test")).Returns(stats);

            Factories.SyncServiceFactory = (c, d) => mock.Object;

            var device = new DeviceData();
            Assert.Equal(stats, client.Object.Stat(device, "/test"));
        }

        [Fact]
        public void GetEnvironmentVariablesTest()
        {
            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add(EnvironmentVariablesReceiver.PrintEnvCommand, "a=b");

            var device = new DeviceData();

            var variables = adbClient.GetEnvironmentVariables(device);
            Assert.NotNull(variables);
            Assert.Single(variables.Keys);
            Assert.True(variables.ContainsKey("a"));
            Assert.Equal("b", variables["a"]);
        }

        [Fact]
        public void UninstallPackageTests()
        {
            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add("pm list packages -f", "");
            adbClient.Commands.Add("pm uninstall com.example", "");

            var device = new DeviceData();
            device.State = DeviceState.Online;
            adbClient.UninstallPackage(device, "com.example");

            Assert.Equal(2, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm list packages -f", adbClient.ReceivedCommands[0]);
            Assert.Equal("pm uninstall com.example", adbClient.ReceivedCommands[1]);
        }

        [Fact]
        public void GetPackageVersionTest()
        {
            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add("pm list packages -f", "");
            adbClient.Commands.Add("dumpsys package com.example",
@"Activity Resolver Table:
  Non-Data Actions:
      com.android.providers.contacts.DUMP_DATABASE:
        310a0bd8 com.android.providers.contacts/.debug.ContactsDumpActivity

Receiver Resolver Table:
  Schemes:
      package:
        31f30b31 com.android.providers.contacts/.PackageIntentReceiver (4 filters)

Registered ContentProviders:
  com.android.providers.contacts/.debug.DumpFileProvider:
    Provider{2b000d84 com.android.providers.contacts/.debug.DumpFileProvider}

ContentProvider Authorities:
  [com.android.voicemail]:
    Provider{316ea633 com.android.providers.contacts/.VoicemailContentProvider}
      applicationInfo=ApplicationInfo{1327df0 com.android.providers.contacts}

Key Set Manager:
  [com.android.providers.contacts]
      Signing KeySets: 3

Packages:
  Package [com.android.providers.contacts] (3d5205d5):
    versionCode=22 targetSdk=22
    versionName=5.1-eng.buildbot.20151117.204057
    splits=[base]

Shared users:
  SharedUser [android.uid.shared] (3341dee):
    userId=10002 gids=[3003, 1028, 1015]
    grantedPermissions:
      android.permission.WRITE_SETTINGS");

            var device = new DeviceData();
            device.State = DeviceState.Online;
            var version = adbClient.GetPackageVersion(device, "com.example");

            Assert.Equal(22, version.VersionCode);
            Assert.Equal("5.1-eng.buildbot.20151117.204057", version.VersionName);

            Assert.Equal(2, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm list packages -f", adbClient.ReceivedCommands[0]);
            Assert.Equal("dumpsys package com.example", adbClient.ReceivedCommands[1]);
        }

        [Fact]
        public void GetPackageVersionTest2()
        {
            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add("pm list packages -f", "");
            adbClient.Commands.Add("dumpsys package jp.co.cyberagent.stf",
@"Activity Resolver Table:
  Schemes:
      package:
        423fa100 jp.co.cyberagent.stf/.IconActivity filter 427ae628

  Non-Data Actions:
      jp.co.cyberagent.stf.ACTION_IDENTIFY:
        423fa4d8 jp.co.cyberagent.stf/.IdentityActivity filter 427c76a8

Service Resolver Table:
  Non-Data Actions:
      jp.co.cyberagent.stf.ACTION_STOP:
        423fc3d8 jp.co.cyberagent.stf/.Service filter 427e4ca8
      jp.co.cyberagent.stf.ACTION_START:
        423fc3d8 jp.co.cyberagent.stf/.Service filter 427e4ca8

Packages:
  Package [jp.co.cyberagent.stf] (428c8c10):
    userId=10153 gids=[3003, 1015, 1023, 1028]
    sharedUser=null
    pkg=Package{42884220 jp.co.cyberagent.stf}
    codePath=/data/app/jp.co.cyberagent.stf-1.apk
    resourcePath=/data/app/jp.co.cyberagent.stf-1.apk
    nativeLibraryPath=/data/app-lib/jp.co.cyberagent.stf-1
    versionCode=4
    applicationInfo=ApplicationInfo{4287f2e0 jp.co.cyberagent.stf}
    flags=[ HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP ]
    versionName=2.1.0
    dataDir=/data/data/jp.co.cyberagent.stf
    targetSdk=22
    supportsScreens=[small, medium, large, xlarge, resizeable, anyDensity]
    timeStamp=2017-09-08 15:52:21
    firstInstallTime=2017-09-08 15:52:21
    lastUpdateTime=2017-09-08 15:52:21
    signatures=PackageSignatures{419a7e60 [41bb3628]}
    permissionsFixed=true haveGids=true installStatus=1
    pkgFlags=[ HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP ]
    packageOnlyForOwnerUser: false
    componentsOnlyForOwerUser:
    User 0:  installed=true stopped=true notLaunched=true enabled=0
    grantedPermissions:
      android.permission.READ_EXTERNAL_STORAGE
      android.permission.READ_PHONE_STATE
      android.permission.DISABLE_KEYGUARD
      android.permission.WRITE_EXTERNAL_STORAGE
      android.permission.INTERNET
      android.permission.CHANGE_WIFI_STATE
      android.permission.MANAGE_ACCOUNTS
      android.permission.ACCESS_WIFI_STATE
      android.permission.GET_ACCOUNTS
      android.permission.ACCESS_NETWORK_STATE
      android.permission.WAKE_LOCK
mPackagesOnlyForOwnerUser:
  package : com.android.mms
  package : com.android.phone
  package : com.sec.knox.containeragent
mComponentsOnlyForOwnerUser:
  package : com.android.contacts
    cmp : com.android.contacts.activities.DialtactsActivity

mEnforceCopyingLibPackages:

mSkippingApks:

mSettings.mPackages:
the number of packages is 223
mPackages:
the number of packages is 223
End!!!!");

            var device = new DeviceData();
            device.State = DeviceState.Online;
            var version = adbClient.GetPackageVersion(device, "jp.co.cyberagent.stf");

            Assert.Equal(4, version.VersionCode);
            Assert.Equal("2.1.0", version.VersionName);

            Assert.Equal(2, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm list packages -f", adbClient.ReceivedCommands[0]);
            Assert.Equal("dumpsys package jp.co.cyberagent.stf", adbClient.ReceivedCommands[1]);
        }

        [Fact]
        public void GetPackageVersionTest3()
        {
            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add("pm list packages -f", "");
            adbClient.Commands.Add("dumpsys package jp.co.cyberagent.stf",
@"Activity Resolver Table:
  Schemes:
      package:
        de681a8 jp.co.cyberagent.stf/.IconActivity filter 2863eca
          Action: ""jp.co.cyberagent.stf.ACTION_ICON""
          Category: ""android.intent.category.DEFAULT""
          Scheme: ""package""

  Non-Data Actions:
      jp.co.cyberagent.stf.ACTION_IDENTIFY:
        69694c1 jp.co.cyberagent.stf/.IdentityActivity filter 30bda35
          Action: ""jp.co.cyberagent.stf.ACTION_IDENTIFY""
          Category: ""android.intent.category.DEFAULT""

Service Resolver Table:
  Non-Data Actions:
      jp.co.cyberagent.stf.ACTION_STOP:
        db65466 jp.co.cyberagent.stf/.Service filter 7c0646c
          Action: ""jp.co.cyberagent.stf.ACTION_START""
          Action: ""jp.co.cyberagent.stf.ACTION_STOP""
          Category: ""android.intent.category.DEFAULT""
      jp.co.cyberagent.stf.ACTION_START:
        db65466 jp.co.cyberagent.stf/.Service filter 7c0646c
          Action: ""jp.co.cyberagent.stf.ACTION_START""
          Action: ""jp.co.cyberagent.stf.ACTION_STOP""
          Category: ""android.intent.category.DEFAULT""

Key Set Manager:
  [jp.co.cyberagent.stf]
      Signing KeySets: 57

Packages:
  Package [jp.co.cyberagent.stf] (13d33a7):
    userId=11261
    pkg=Package{6f61054 jp.co.cyberagent.stf}
    codePath=/data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==
    resourcePath=/data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==
    legacyNativeLibraryDir=/data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==/lib
    primaryCpuAbi=null
    secondaryCpuAbi=null
    versionCode=4 minSdk=9 targetSdk=22
    versionName=2.1.0
    splits=[base]
    apkSigningVersion=2
    applicationInfo=ApplicationInfo{4b6bbfd jp.co.cyberagent.stf}
    flags=[ HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP ]
    dataDir=/data/user/0/jp.co.cyberagent.stf
    supportsScreens=[small, medium, large, xlarge, resizeable, anyDensity]
    timeStamp=2017-09-08 22:06:05
    firstInstallTime=2017-09-08 22:06:07
    lastUpdateTime=2017-09-08 22:06:07
    signatures=PackageSignatures{1c350f2 [37b7ecb5]}
    installPermissionsFixed=true installStatus=1
    pkgFlags=[ HAS_CODE ALLOW_CLEAR_USER_DATA ALLOW_BACKUP ]
    requested permissions:
      android.permission.DISABLE_KEYGUARD
      android.permission.READ_PHONE_STATE
      android.permission.WAKE_LOCK
      android.permission.INTERNET
      android.permission.ACCESS_NETWORK_STATE
      android.permission.WRITE_EXTERNAL_STORAGE
      android.permission.GET_ACCOUNTS
      android.permission.MANAGE_ACCOUNTS
      android.permission.CHANGE_WIFI_STATE
      android.permission.ACCESS_WIFI_STATE
      android.permission.READ_EXTERNAL_STORAGE
    install permissions:
      android.permission.MANAGE_ACCOUNTS: granted=true
      android.permission.INTERNET: granted=true
      android.permission.READ_EXTERNAL_STORAGE: granted=true
      android.permission.READ_PHONE_STATE: granted=true
      android.permission.CHANGE_WIFI_STATE: granted=true
      android.permission.ACCESS_NETWORK_STATE: granted=true
      android.permission.DISABLE_KEYGUARD: granted=true
      android.permission.GET_ACCOUNTS: granted=true
      android.permission.WRITE_EXTERNAL_STORAGE: granted=true
      android.permission.ACCESS_WIFI_STATE: granted=true
      android.permission.WAKE_LOCK: granted=true
    User 0: ceDataInode=409220 installed=true hidden=false suspended=false stopped=true notLaunched=true enabled=0 instant=false
      gids=[3003]
      runtime permissions:
    User 10: ceDataInode=0 installed=true hidden=false suspended=false stopped=true notLaunched=true enabled=0 instant=false
      gids=[3003]
      runtime permissions:

Package Changes:
  Sequence number=45
  User 0:
    seq=6, package=com.google.android.gms
    seq=9, package=be.brusselsairport.appyflight
    seq=11, package=com.android.vending
    seq=13, package=app.qrcode
    seq=15, package=com.android.chrome
    seq=16, package=com.google.android.apps.docs
    seq=17, package=com.google.android.inputmethod.latin
    seq=18, package=com.google.android.music
    seq=20, package=com.google.android.apps.walletnfcrel
    seq=21, package=com.google.android.youtube
    seq=22, package=com.google.android.calendar
    seq=44, package=jp.co.cyberagent.stf
  User 10:
    seq=10, package=com.android.vending
    seq=14, package=com.google.android.apps.walletnfcrel
    seq=15, package=com.android.chrome
    seq=16, package=com.google.android.apps.docs
    seq=17, package=com.google.android.inputmethod.latin
    seq=18, package=com.google.android.music
    seq=19, package=com.google.android.youtube
    seq=22, package=com.google.android.calendar
    seq=44, package=jp.co.cyberagent.stf


Dexopt state:
  [jp.co.cyberagent.stf]
    Instruction Set: arm64
      path: /data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==/base.apk
      status: /data/app/jp.co.cyberagent.stf-Q3jXaNJMy6AIVndbPuclbg==/oat/arm64/base.odex[status=kOatUpToDate, compilati
      on_filter=quicken]


Compiler stats:
  [jp.co.cyberagent.stf]
     base.apk - 1084");

            var device = new DeviceData();
            device.State = DeviceState.Online;
            var version = adbClient.GetPackageVersion(device, "jp.co.cyberagent.stf");

            Assert.Equal(4, version.VersionCode);
            Assert.Equal("2.1.0", version.VersionName);

            Assert.Equal(2, adbClient.ReceivedCommands.Count);
            Assert.Equal("pm list packages -f", adbClient.ReceivedCommands[0]);
            Assert.Equal("dumpsys package jp.co.cyberagent.stf", adbClient.ReceivedCommands[1]);
        }

        [Fact]
        public void ListProcessesTest()
        {
            var adbClient = new DummyAdbClient();

            adbClient.Commands.Add(@"SDK=""$(/system/bin/getprop ro.build.version.sdk)""
if [ $SDK -lt 24 ]
then
    /system/bin/ls /proc/
else
    /system/bin/ls -1 /proc/
fi".Replace("\r\n", "\n"), 
@"1
2
3
acpi
asound");
            adbClient.Commands.Add("cat /proc/1/stat /proc/2/stat /proc/3/stat ",
@"1 (init) S 0 0 0 0 -1 1077944576 2680 83280 0 179 0 67 16 39 20 0 1 0 2 17735680 143 18446744073709551615 134512640 135145076 4288071392 4288070744 134658736 0 0 0 65536 18446744071580117077 0 0 17 1 0 0 0 0 0 135152736 135165080 142131200 4288073690 4288073696 4288073696 4288073714 0
2 (kthreadd) S 0 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 20 0 1 0 2 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579254310 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
3 (ksoftirqd/0) S 2 0 0 0 -1 69238848 0 0 0 0 0 23 0 0 20 0 1 0 7 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579284070 0 0 17 0 0 0 0 0 0 0 0 0 0 0 0 0 0");
            adbClient.Commands.Add("cat /proc/1/cmdline /proc/1/stat /proc/2/cmdline /proc/2/stat /proc/3/cmdline /proc/3/stat ",
@"
1 (init) S 0 0 0 0 -1 1077944576 2680 83280 0 179 0 67 16 39 20 0 1 0 2 17735680 143 18446744073709551615 134512640 135145076 4288071392 4288070744 134658736 0 0 0 65536 18446744071580117077 0 0 17 1 0 0 0 0 0 135152736 135165080 142131200 4288073690 4288073696 4288073696 4288073714 0

2 (kthreadd) S 0 0 0 0 -1 2129984 0 0 0 0 0 0 0 0 20 0 1 0 2 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579254310 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0

3 (ksoftirqd/0) S 2 0 0 0 -1 69238848 0 0 0 0 0 23 0 0 20 0 1 0 7 0 0 18446744073709551615 0 0 0 0 0 0 0 2147483647 0 18446744071579284070 0 0 17 0 0 0 0 0 0 0 0 0 0 0 0 0 0");

            var device = new DeviceData();
            var processes = adbClient.ListProcesses(device).ToArray();

            Assert.Equal(3, processes.Length);
            Assert.Equal("init", processes[0].Name);
            Assert.Equal("kthreadd", processes[1].Name);
            Assert.Equal("ksoftirqd/0", processes[2].Name);
        }
    }
}
