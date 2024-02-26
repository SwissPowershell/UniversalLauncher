Param()
Clear-Host
Write-Host ''
Write-Host "*****************************"
Write-Host "**** Example of PS1 File ****"
Write-Host "*****************************"
Write-Host ''
Write-Host "Script root: $($PSScriptRoot)"
Write-Host "Current Location (WorkingDir): $(Get-Location)"

Write-Host "Value of Path:"
$SplitedPath = $env:path -split ';'
ForEach($pathpart in $SplitedPath){
Write-Host "`t $($pathpart)"
}
Write-Host "Value of Toto: $($Env:Toto)"
Write-Host "Value of Tata: $($Env:Tata)"
Write-Host "Argument received: $($args)"
Write-Host ''

Read-Host "Press any key to exit" | out-null