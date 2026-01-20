# PowerShell script to rename Category to Agent in frontend
$ErrorActionPreference = "Stop"
$rootPath = "C:\Users\hngo1_mantu\.claude-worktrees\ai-assistant\magical-haslett\assistant-ui-chat"

Write-Host "Starting Category to Agent rename for frontend..." -ForegroundColor Green

# Phase 1: Rename files
Write-Host "`nPhase 1: Renaming files..." -ForegroundColor Cyan

# Rename Category.ts to Agent.ts
if (Test-Path "$rootPath\src\types\Category.ts") {
    Move-Item "$rootPath\src\types\Category.ts" "$rootPath\src\types\Agent.ts" -Force
    Write-Host "  ✓ Renamed Category.ts to Agent.ts"
}

# Rename categoriesClient.ts to agentsClient.ts
if (Test-Path "$rootPath\src\api\categoriesClient.ts") {
    Move-Item "$rootPath\src\api\categoriesClient.ts" "$rootPath\src\api\agentsClient.ts" -Force
    Write-Host "  ✓ Renamed categoriesClient.ts to agentsClient.ts"
}

# Rename useCategories.ts to useAgents.ts
if (Test-Path "$rootPath\src\hooks\useCategories.ts") {
    Move-Item "$rootPath\src\hooks\useCategories.ts" "$rootPath\src\hooks\useAgents.ts" -Force
    Write-Host "  ✓ Renamed useCategories.ts to useAgents.ts"
}

# Phase 2: Rename directories
Write-Host "`nPhase 2: Renaming directories..." -ForegroundColor Cyan

# Rename CategoriesPage to AgentsPage
if (Test-Path "$rootPath\src\pages\CategoriesPage") {
    Move-Item "$rootPath\src\pages\CategoriesPage" "$rootPath\src\pages\AgentsPage" -Force
    Write-Host "  ✓ Renamed CategoriesPage directory to AgentsPage"
}

# Rename CategoryModal to AgentModal
if (Test-Path "$rootPath\src\components\CategoryModal") {
    Move-Item "$rootPath\src\components\CategoryModal" "$rootPath\src\components\AgentModal" -Force
    Write-Host "  ✓ Renamed CategoryModal directory to AgentModal"
}

# Phase 3: Rename files in moved directories
Write-Host "`nPhase 3: Renaming files in moved directories..." -ForegroundColor Cyan

# Rename files in AgentsPage
if (Test-Path "$rootPath\src\pages\AgentsPage\CategoriesPage.tsx") {
    Move-Item "$rootPath\src\pages\AgentsPage\CategoriesPage.tsx" "$rootPath\src\pages\AgentsPage\AgentsPage.tsx" -Force
    Write-Host "  ✓ Renamed CategoriesPage.tsx to AgentsPage.tsx"
}
if (Test-Path "$rootPath\src\pages\AgentsPage\CategoriesPage.css") {
    Move-Item "$rootPath\src\pages\AgentsPage\CategoriesPage.css" "$rootPath\src\pages\AgentsPage\AgentsPage.css" -Force
    Write-Host "  ✓ Renamed CategoriesPage.css to AgentsPage.css"
}
if (Test-Path "$rootPath\src\pages\AgentsPage\CategorySkillsPage.tsx") {
    Move-Item "$rootPath\src\pages\AgentsPage\CategorySkillsPage.tsx" "$rootPath\src\pages\AgentsPage\AgentSkillsPage.tsx" -Force
    Write-Host "  ✓ Renamed CategorySkillsPage.tsx to AgentSkillsPage.tsx"
}
if (Test-Path "$rootPath\src\pages\AgentsPage\CategorySkillsPage.css") {
    Move-Item "$rootPath\src\pages\AgentsPage\CategorySkillsPage.css" "$rootPath\src\pages\AgentsPage\AgentSkillsPage.css" -Force
    Write-Host "  ✓ Renamed CategorySkillsPage.css to AgentSkillsPage.css"
}

# Rename files in AgentModal
if (Test-Path "$rootPath\src\components\AgentModal\CategoryModal.tsx") {
    Move-Item "$rootPath\src\components\AgentModal\CategoryModal.tsx" "$rootPath\src\components\AgentModal\AgentModal.tsx" -Force
    Write-Host "  ✓ Renamed CategoryModal.tsx to AgentModal.tsx"
}
if (Test-Path "$rootPath\src\components\AgentModal\CategoryModal.css") {
    Move-Item "$rootPath\src\components\AgentModal\CategoryModal.css" "$rootPath\src\components\AgentModal\AgentModal.css" -Force
    Write-Host "  ✓ Renamed CategoryModal.css to AgentModal.css"
}

# Phase 4: Text replacements in files
Write-Host "`nPhase 4: Replacing text in files..." -ForegroundColor Cyan

function Replace-InFile {
    param(
        [string]$FilePath,
        [hashtable]$Replacements
    )

    if (Test-Path $FilePath) {
        $content = Get-Content $FilePath -Raw -Encoding UTF8
        $modified = $false

        foreach ($key in $Replacements.Keys) {
            if ($content -match [regex]::Escape($key)) {
                $content = $content -replace [regex]::Escape($key), $Replacements[$key]
                $modified = $true
            }
        }

        if ($modified) {
            Set-Content $FilePath $content -Encoding UTF8 -NoNewline
            return $true
        }
    }
    return $false
}

# Define replacements
$typeReplacements = @{
    "from `"./Category`"" = "from `"./Agent`""
    "Category" = "Agent"
    "CategoryWithSkills" = "AgentWithSkills"
    "CreateCategoryRequest" = "CreateAgentRequest"
    "UpdateCategoryRequest" = "UpdateAgentRequest"
}

$apiReplacements = @{
    "categoriesClient" = "agentsClient"
    "from `"./categoriesClient`"" = "from `"./agentsClient`""
    "/api/categories" = "/api/agents"
    "Category" = "Agent"
    "CreateCategoryRequest" = "CreateAgentRequest"
    "UpdateCategoryRequest" = "UpdateAgentRequest"
}

$skillApiReplacements = @{
    "CategoryWithSkills" = "AgentWithSkills"
    "getByCategory" = "getByAgent"
    "categoryId" = "agentId"
    "/by-category/" = "/by-agent/"
}

# Apply to Agent.ts
if (Replace-InFile "$rootPath\src\types\Agent.ts" $typeReplacements) {
    Write-Host "  ✓ Updated Agent.ts"
}

# Apply to agentsClient.ts
if (Replace-InFile "$rootPath\src\api\agentsClient.ts" $apiReplacements) {
    Write-Host "  ✓ Updated agentsClient.ts"
}

# Apply to skillsClient.ts
if (Replace-InFile "$rootPath\src\api\skillsClient.ts" $skillApiReplacements) {
    Write-Host "  ✓ Updated skillsClient.ts"
}

# Apply to index files
$indexReplacements = @{
    'export \* from "./Category"' = 'export * from "./Agent"'
    "categoriesClient" = "agentsClient"
    "CategoryModal" = "AgentModal"
    "CategorySkillsPage" = "AgentSkillsPage"
    "CategoriesPage" = "AgentsPage"
}

if (Replace-InFile "$rootPath\src\types\index.ts" $indexReplacements) {
    Write-Host "  ✓ Updated src/types/index.ts"
}
if (Replace-InFile "$rootPath\src\api\index.ts" $indexReplacements) {
    Write-Host "  ✓ Updated src/api/index.ts"
}
if (Replace-InFile "$rootPath\src\components\index.ts" $indexReplacements) {
    Write-Host "  ✓ Updated src/components/index.ts"
}
if (Replace-InFile "$rootPath\src\pages\index.ts" $indexReplacements) {
    Write-Host "  ✓ Updated src/pages/index.ts"
}

Write-Host "`n✓ Frontend file and directory renaming complete!" -ForegroundColor Green
Write-Host "Next step complete" -ForegroundColor Yellow
