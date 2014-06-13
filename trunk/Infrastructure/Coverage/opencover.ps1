param(
    [parameter(Mandatory=$true)] $testAssemblySuffix
)

$Root=Split-Path $MyInvocation.MyCommand.Path
$Me	=Split-Path	$MyInvocation.MyCommand.Path -Leaf
$GenerateOnly='false'
$NoRegister='false'

$solutionName="OneCache"
$solutionFolder="$Root\..\..\"
$solutionPath= "$solutionFolder$solutionName.sln"




function Set-DefaultCommands()
{
	echo "Root: $Root"
	
	if(!$script:OpenCoverCommand)
	{
		NuGet-Deploy OpenCover 4.5.2506
		$script:OpenCoverCommand = "$Root\.generationTools\OpenCover.4.5.2506\OpenCover.Console.exe"
	}
	
	if(!$NunitCommand)
	{
		NuGet-Deploy NUnit.Runners 2.6.3
		$script:NunitCommand = "$Root\.generationTools\NUnit.Runners.2.6.3\tools\nunit-console.exe"
	}
	
	if(!$ReportGeneratorCommand)
	{
		NuGet-Deploy ReportGenerator 1.9.1		
		$script:ReportGeneratorCommand = "$Root\.generationTools\ReportGenerator.1.9.1.0\ReportGenerator.exe"
	}
	
	if(!$ReportDir)
	{
		$script:ReportDir="$Root\CoverageReports"
	}
}

function NuGet-Deploy($toolName, $toolVersion)
{
	.\nuget install $toolName -Version $toolVersion -outputDirectory .generationTools 
}

function Show-Usage()
{
		echo "opencover <<test assemblies suffix>>"
}
function Bootstrap-FileLocations($solution)
{
	if(!$solution)
	{
		throw "Solution must be specified";
	}

	if(!(Test-Path "$solution") )	
	{
		throw "Solution $solution not found"
	}

	Ensure-CommandExists $OpenCoverCommand "OpenCover" $OpenCoverVersion
	Ensure-CommandExists $NunitCommand "Nunit" $NunitVersion
	Ensure-CommandExists $ReportGeneratorCommand "Report Generator" $ReportGeneratorVersion
	
	echo "Ensuring that report dir $ReportDir exists."
	if(!(Test-Path $ReportDir))
	{
		New-Item -ItemType directory -Path $ReportDir
	} 
	
}

function Ensure-CommandExists($command, $tool, $version)	
{
	echo "Expecting [$tool] version [$version] at [$command]"
	if(!$command -or !(Test-Path $command))
	{
		throw "Could not find version $version of $tool at $command - please check you have it installed via chocolatey (cinst opencover, cinst reportgenerator) or nuget." 
	}
}


function Run-OpenCover($solution, $specificUnitTests)
{
	$targetArgs = "/nologo /noshadow"		

	if($specificUnitTests)
	{	
		if(Test-Path "$solutionFolder\$specificUnitTests.$testAssemblySuffix\bin\debug\")
		{
			$targetArgs = "$solutionFolder\$specificUnitTests.$testAssemblySuffix\bin\debug\$specificUnitTests.$testAssemblySuffix.dll $targetArgs"
		}
		else
		{
			throw "No unit tests found called $specificUnitTests.$testAssemblySuffix"
		}
	}
	else
	{
		$found=0;
		foreach($folder in (Get-ChildItem $solutionFolder))
		{
			 #echo $folder.Name
			 if(($folder.Name -ilike "$solutionName.*.$testAssemblySuffix" -or $folder.Name -ilike "$solutionName.$testAssemblySuffix") -and (Test-Path "$solutionFolder\$folder\bin\debug\$folder.dll"))
			 {
				$targetArgs="$solutionFolder\$folder\bin\debug\$folder.dll $targetArgs"
				$found=$found+1
			 }
		}
		if($found -eq 0)
		{
			throw "No unit tests found."
		}
	}

	echo "Running $OpenCoverCommand on $solution"
	$OpenCoverArgs = "-target:$NunitCommand", 
					 "-targetargs:$targetArgs",
					 "-output:$ReportDir\Coverage.xml",
					 "-oldstyle",
					 "-coverbytest:*.$testAssemblySuffix",
					 "-filter:+[(?i)($solutionName).*]* -[*.$testAssemblySuffix]*"
	if(!($NoRegister -eq 'true'))
	{
		$OpenCoverArgs += "-register"
	}
	echo $OpenCoverArgs
	&$OpenCoverCommand $OpenCoverArgs
}

function Generate-Report($solution)
{
	echo "Generating report for $solution"
	$ReportGeneratorArgs =	"-reports:$ReportDir\Coverage.xml",
							"-targetdir:$ReportDir"
	&$ReportGeneratorCommand $ReportGeneratorArgs
}

function Display-Report($solution)
{
	$location="$ReportDir\index.htm"
	if(!($GenerateOnly -eq "true"))
	{
		&$location
	}
	else
	{
		Write-Host "Your report is available at $location"
	}
}

try
{
	"1"
	Set-DefaultCommands
	"2"
	Bootstrap-FileLocations $solutionPath
	"3"
	Run-OpenCover $solutionPath $specificUnitTests
	"4"
	Generate-Report $solutionPath
	"5"
	Display-Report $solutionPath
	
}
catch 
{
	Write-Host "Error - $_"
	Show-Usage
	Write-Error "Error - $_"
}