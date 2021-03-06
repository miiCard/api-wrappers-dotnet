<# First, delete any existing package or nuspec file #>
del *.nuspec
del *.nupkg

<# Next up - create the default spec file from metadata #>
nuget spec

<# Transform the spec file to replace default values with suitable alternatives #>
(Get-Content miiCard.Consumers.nuspec) |
Foreach-Object {
	$_ -replace "<licenseUrl>.*<\/licenseUrl>", "<licenseUrl>https://github.com/miiCard/api-wrappers-dotnet/blob/master/LICENCE.TXT</licenseUrl>" `
	   -replace "<projectUrl>.*<\/projectUrl>", "<projectUrl>http://www.miicard.com/developers/libraries-components/net-library</projectUrl>" `
	   -replace "<iconUrl>.*<\/iconUrl>", "<iconUrl>http://www.miicard.com/sites/default/files/dev/nuget_32x32.png</iconUrl>" `
	   -replace "<releaseNotes>.*<\/releaseNotes>", "<releaseNotes></releaseNotes>" `
	   -replace "<tags>.*<\/tags>", "<tags>miiCard identity assurance security miicard.com</tags>"
} | Set-Content miiCard.Consumers.nuspec 

[xml]$nuspec = [System.Xml.XmlDocument](Get-Content "miiCard.Consumers.nuspec")
$dependencyElement = $nuspec.CreateElement("dependencies")
$dnoaElement = $nuspec.CreateElement("dependency")
$dnoaElement.SetAttribute("id", "DotNetOpenAuth.OAuth.Consumer")
$dnoaElement.SetAttribute("version", "4.0.2.12119")
$jsonElement = $nuspec.CreateElement("dependency")
$jsonElement.SetAttribute("id", "Newtonsoft.Json")
$jsonElement.SetAttribute("version", "5.0.8")

$dependencyElement.AppendChild($dnoaElement)
$dependencyElement.AppendChild($jsonElement)
$nuspec.DocumentElement.FirstChild.AppendChild($dependencyElement)
$nuspec.Save("miiCard.Consumers.nuspec")

<# Finally, package the thing #>
nuget pack miiCard.Consumers.csproj -Prop Configuration="Nuget Package" -IncludeReferencedProjects