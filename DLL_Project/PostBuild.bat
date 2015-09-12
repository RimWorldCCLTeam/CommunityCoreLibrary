REM DO NOT RECOMMIT THIS FILE!
REM This is a local file copy after build.  Get it once and your .gitnore
REM should handle it after that.  Make all your local copies at the end.

REM Set this to your local RimWorld install path and CCL Assemblies directory
Set InstalledCCLAssemblies="/badkarma/"

if NOT EXIST %InstalledCCLAssemblies% (
	echo Missing or invalid copy target:
	echo %InstalledCCLAssemblies%
	EXIT -1
)

echo Build Config: %1
echo Build Target: %2
echo Solution Path: %3
echo CCL Install Path: %InstalledCCLAssemblies%

echo Copy to RimWorld
copy %2 %InstalledCCLAssemblies%

if %1 == Debug (
	echo Copy to Modders Resource
	copy %2 "%3_Mod\Modders Resource\Community Core Library\Assemblies"
) else (
	echo Copy to User Release
	copy %2 "%3_Mod\User Release\Community Core Library\Assemblies"
)

REM Add any other local copies here
rem goto Finished

rem echo Copy to ModPile
rem copy %2 "C:\Utils\dev\Projects\ModPile\ModPile_ProjectDLL\Source-DLLs"

:Finished