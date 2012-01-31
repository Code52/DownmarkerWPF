try 
{ 
  $name    = 'markpad'
  $url     = 'https://github.com/downloads/Code52/DownmarkerWPF/Markpad.1.0.1.zip'
  $tools   = Split-Path $MyInvocation.MyCommand.Definition
  $content = Join-Path (Split-Path $tools) 'content'
  $target  = Join-Path $content 'MarkPad.exe'
  
  Install-ChocolateyZipPackage $name $url $content
  Install-ChocolateyDesktopLink $target
  
  Write-ChocolateySuccess $name
} 
catch 
{
  Write-ChocolateyFailure $name "$($_.Exception.Message)"
  throw 
}