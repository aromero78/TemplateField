param($installPath, $toolsPath, $package, $project)

$assemblies = $package.AssemblyReferences | %{$_.Name}
foreach ($reference in $project.Object.References)
{
  if ($assemblies -contains $reference.Name + ".dll")
  {
    $reference.CopyLocal = $false;
  }
}
