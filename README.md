# Battery Monitor
It's just a simply Android Battery Monitor...

Tracks your battery percent and sends a notification if it drops below a set percentage.

Download from releases.
the insanely long command i have to run to compiple it 😅 
`dotnet publish -f net8.0-android -c Release /p:AndroidPackageFormat=apk /p:AndroidKeyStore=True /p:AndroidSigningKeyStore="batterymonitorkey.keystore" /p:AndroidSigningStorePass="11092011" /p:AndroidSigningKeyAlias="batterymonitorkey" /p:AndroidSigningKeyAlias="11092011" /p:EmbedAssembliesIntoApk=true /p:EnableDexRunner=false /p:DebugSymbols=false /p:DebugSymbols=false /p:AndroidSdkDirectory="D:\Program Files\Visual Studio\Shared Commponents\Android\android-sdk" /p:JavaSdkDirectory="D:\Program Files\Visual Studio\Shared Commponents\Android\openjdk\jdk-17.0.14"`

made with .NET framwork 9 and hopes and dreams.
