
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
		.\nuget install OpenCover -Version 4.5.2506 -outputDirectory .generationTools
		$script:OpenCoverCommand = "$Root\.generationTools\OpenCover.4.5.2506\OpenCover.Console.exe"
	}
	
	if(!$NunitCommand)
	{
		.\nuget install NUnit.Runners -Version 2.6.3 -outputDirectory .generationTools
		$script:NunitCommand = "$Root\.generationTools\NUnit.Runners.2.6.3\tools\nunit-console.exe"
	}
	
	if(!$ReportGeneratorCommand)
	{
		.\nuget install ReportGenerator -Version 1.9.1 -outputDirectory .generationTools
		$script:ReportGeneratorCommand = "$Root\.generationTools\ReportGenerator.1.9.1.0\ReportGenerator.exe"
	}
	
	if(!$ReportDir)
	{
		$script:ReportDir="$Root\CoverageReports"
	}
}

function Show-Usage()
{
		echo "TODO"
}
function Bootstrap-FileLocations($solution)
{
	if(!$solution)
	{
		throw "Solution must be specified";
	}

	if(!(Test-Path "$solution") -and $solution -ne "Payment")	# Temporary: allow "Payment" solution even though it doesn't exist yet (eoconnor)
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
		if(Test-Path "$solutionFolder\$specificUnitTests.UnitTests\bin\debug\")
		{
			$targetArgs = "$solutionFolder\$specificUnitTests.UnitTests\bin\debug\$specificUnitTests.UnitTests.dll $targetArgs"
		}
		else
		{
			throw "No unit tests found called $specificUnitTests.UnitTests"
		}
	}
	else
	{
		$found=0;
		foreach($folder in (Get-ChildItem $solutionFolder))
		{
			 #echo $folder.Name
			 if(($folder.Name -ilike "$solutionName.*.UnitTests" -or $folder.Name -ilike "$solutionName.UnitTests") -and (Test-Path "$solutionFolder\$folder\bin\debug\$folder.dll"))
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
					 "-coverbytest:*.UnitTests",
					 "-filter:+[(?i)($solutionName).*]* -[*.UnitTests]*"
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