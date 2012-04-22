foreach ($ext in @("*.cs", "*.md", "*.csproject", "*.sln", "*.xaml", "*.targets"))  {
	(dir -Recurse -Filter $ext) | where{!$_.PsIsContainer} | foreach { 
		$file = gc $_.FullName
		$file | sc $_.FullName
		}
	
}