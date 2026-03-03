[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
$loginResponse = Invoke-RestMethod -Uri 'https://localhost:7161/api/Auth/login' -Method Post -ContentType 'application/json' -Body '{"email":"Admin@gmail.com","password":"Admin@123"}'
$token = $loginResponse.token
Write-Host "Obtained Token"
$dashboardResponse = Invoke-RestMethod -Uri 'https://localhost:7161/api/Admin/dashboard' -Method Get -Headers @{ Authorization = "Bearer $token" }
Write-Host "Dashboard Response:"
$dashboardResponse | ConvertTo-Json -Depth 5
