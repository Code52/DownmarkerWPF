foreach ($ext in @("*.cs", "*.md", "*.csproject", "*.sln", "*.xaml", "*.targets"))  {
	(dir -Recurse -Filter $ext) | foreach { 
		$file = gc $_.FullName
		$file | sc $_.FullName
		}
	
}