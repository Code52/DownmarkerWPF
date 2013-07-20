Param($ManifestFile) 
write-host "Fixing up Manifest File"
write-host $ManifestFile

[xml]$xml = get-content $ManifestFile

$elementsToRewrite = $xml.assembly.dependency | where {$_.dependentAssembly.codebase -ne $null -and ($_.dependentAssembly.codebase.Contains("CefSharp") -or $_.dependentAssembly.codebase.Contains("NHunspell")) }
foreach ($elementToRewrite in $elementsToRewrite)
{
	$fileNode = $xml.CreateElement("file", "urn:schemas-microsoft-com:asm.v2")
	$fileNode.SetAttribute("name", $elementToRewrite.dependentAssembly.codebase)	
	$fileNode.SetAttribute("size", $elementToRewrite.dependentAssembly.size)
	$fileNode.AppendChild($elementToRewrite.dependentAssembly.hash) 
	$xml.assembly.AppendChild($fileNode)
	[Void]$xml.assembly.RemoveChild($elementToRewrite)
}

$xml.Save($ManifestFile)

write-host "Fixed Manfiest File"
