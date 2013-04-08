Param($ManifestFile) 
write-host "Fixing up Manifest File"
write-host $ManifestFile

[xml]$xml = get-content $ManifestFile

$xml.assembly.deployment.SetAttribute("trustURLParameters", "true")
$xml.assembly.deployment.SetAttribute("mapFileExtensions", "true")

$xml.assembly.deployment.subscription.update.RemoveAll()
$updateNode = $xml.CreateElement("beforeApplicationStartup", "urn:schemas-microsoft-com:asm.v2")
$xml.assembly.deployment.subscription.Item("update").AppendChild($updateNode)

$updateNode = $xml.CreateElement("beforeApplicationStartup", "urn:schemas-microsoft-com:asm.v2")

$xml.Save($ManifestFile)

write-host "Fixed Manfiest File"
