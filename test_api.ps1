$apiUrl = "http://localhost:5000"
$pass = 0; $fail = 0
$results = @()

function Test-Endpoint {
    param($Name, $Method, $Uri, $Token, $Body, $ExpectedStatus = 200)
    try {
        $headers = @{}
        if ($Token) { $headers["Authorization"] = "Bearer $Token" }
        $params = @{ Uri = $Uri; Method = $Method; Headers = $headers; ContentType = "application/json" }
        if ($Body) { $params["Body"] = ($Body | ConvertTo-Json -Depth 5) }
        $resp = Invoke-RestMethod @params -ErrorAction Stop
        $ok = $resp.success -eq $true
        if ($ok) { $script:pass++ } else { $script:fail++ }
        $results += [PSCustomObject]@{ Name = $Name; Status = if ($ok) { "PASS" } else { "FAIL" }; Detail = if ($ok) { "200 OK" } else { $resp.message } }
        if ($ok) { Write-Host "  + $Name" -ForegroundColor Green } else { Write-Host "  x $Name - $($resp.message)" -ForegroundColor Red }
        return $resp.data
    } catch {
        $script:fail++
        $code = $_.Exception.Response.StatusCode.value__
        $body = try { $_.ErrorDetails.Message } catch { $_ }
        $results += [PSCustomObject]@{ Name = $Name; Status = "FAIL"; Detail = "$code $body" }
        Write-Host "  x $Name - $code" -ForegroundColor Red
        return $null
    }
}

Write-Host "============ BloodCenterOS API Test Suite ============" -ForegroundColor Cyan
Write-Host "Target: $apiUrl`n" -ForegroundColor Gray

# 1. Auth
Write-Host "[1] Auth" -ForegroundColor Yellow
$login = Test-Endpoint -Name "POST /api/auth/login" -Method Post -Uri "$apiUrl/api/auth/login" -Body @{ userName = "admin"; password = "admin@123" }
$token = $login.token

if (-not $token) { Write-Host "`nABORT - Login failed" -ForegroundColor Red; exit }

# 2. Donor CRUD
Write-Host "[2] Donor" -ForegroundColor Yellow
$donorBody = @{
    firstName = "Ramesh"
    lastName = "Sharma"
    gender = "Male"
    dateOfBirth = "1985-06-15T00:00:00"
    bloodGroup = "O+"
    phone = "9876543210"
    email = "ramesh@example.com"
    city = "Mumbai"
    pincode = "400001"
    occupation = "Engineer"
}
$donor = Test-Endpoint -Name "POST /api/donors" -Method Post -Uri "$apiUrl/api/donors" -Token $token -Body $donorBody
$donorId = $donor.donorId
if ($donorId) {
    Test-Endpoint -Name "GET /api/donors/$donorId" -Method Get -Uri "$apiUrl/api/donors/$donorId" -Token $token
    Test-Endpoint -Name "PUT /api/donors/$donorId" -Method Put -Uri "$apiUrl/api/donors/$donorId" -Token $token -Body @{ firstName = "Ramesh"; lastName = "Sharma"; gender = "Male"; dateOfBirth = "1985-06-15T00:00:00"; bloodGroup = "O+"; phone = "9876543211"; email = "ramesh@example.com"; city = "Mumbai" }
    Test-Endpoint -Name "GET /api/donors/search" -Method Get -Uri ($apiUrl + "/api/donors/search?keyword=ramesh&page=1&size=10") -Token $token
    Test-Endpoint -Name "GET /api/donors/by-phone" -Method Get -Uri ($apiUrl + "/api/donors/by-phone?phone=9876543211") -Token $token
}

# 3. Camp CRUD
Write-Host "[3] Camp" -ForegroundColor Yellow
$campBody = @{
    campCode = "CAMP-001"
    campName = "Mumbai Mega Camp"
    venue = "Andheri Sports Complex"
    city = "Mumbai"
    campDate = "2026-08-15T00:00:00"
    startTime = "08:00:00"
    endTime = "17:00:00"
    totalDonorsExpected = 200
}
$camp = Test-Endpoint -Name "POST /api/camps" -Method Post -Uri "$apiUrl/api/camps" -Token $token -Body $campBody
$campId = $camp.campId
if ($campId) {
    Test-Endpoint -Name "GET /api/camps/$campId" -Method Get -Uri "$apiUrl/api/camps/$campId" -Token $token
    Test-Endpoint -Name "GET /api/camps/upcoming" -Method Get -Uri "$apiUrl/api/camps/upcoming" -Token $token
}

# 4. Hospital
Write-Host "[4] Hospital" -ForegroundColor Yellow
$hospitalBody = @{
    hospitalCode = "HOS-001"
    hospitalName = "Lilavati Hospital"
    address = "Bandra West, Mumbai"
    contactPerson = "Dr. Patel"
    phone = "022-26420000"
    email = "info@lilavati.in"
}
$hospital = Test-Endpoint -Name "POST /api/hospitals" -Method Post -Uri "$apiUrl/api/hospitals" -Token $token -Body $hospitalBody
$hospitalId = $hospital.hospitalId

# 5. Collection (needs a blood bag record first)
Write-Host "[5] Blood Bag + Collection" -ForegroundColor Yellow
# Insert a blood bag directly via SQL for testing
$env:PGPASSWORD = "postgres"
& "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -h localhost -d bloodcenter -c "INSERT INTO bloodbagmaster (centerid, bagnumber, barcode, lotnumber, bloodgroup, volume_ml, collecteddate, createdat) VALUES (1, 'BAG-00001', '8901234567890', 'LOT-2026-001', 'O+', 450, NOW(), NOW()) RETURNING bagid;" 2>$null | Out-Null
$bagId = & "C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -h localhost -d bloodcenter -t -A -c "SELECT bagid FROM bloodbagmaster WHERE bagnumber='BAG-00001' LIMIT 1;" 2>$null
Write-Host "  Using BagId: $bagId" -ForegroundColor Gray

$collectionBody = @{
    donorId = $donorId
    campId = $campId
    bloodBagNumber = "BAG-00001"
    bagBarcode = "8901234567890"
    bagLotNumber = "LOT-2026-001"
    bagVolumeMl = 450
    collectionLocationType = "Camp"
    collectionStartTime = "2026-07-16T09:30:00"
    collectionEndTime = "2026-07-16T09:45:00"
    notes = "Smooth collection"
}
$collection = Test-Endpoint -Name "POST /api/collections" -Method Post -Uri "$apiUrl/api/collections" -Token $token -Body $collectionBody
$collectionId = $collection.collectionId
if ($collectionId) {
    Test-Endpoint -Name "GET /api/collections/$collectionId" -Method Get -Uri "$apiUrl/api/collections/$collectionId" -Token $token
}

# 6. Inventory
Write-Host "[6] Inventory" -ForegroundColor Yellow
Test-Endpoint -Name "POST /api/inventory/adjust" -Method Post -Uri ($apiUrl + "/api/inventory/adjust?componentType=PRBC&bloodGroup=O%2B&available=50&reserved=5&quarantined=2") -Token $token
Test-Endpoint -Name "GET /api/inventory/stock" -Method Get -Uri "$apiUrl/api/inventory/stock" -Token $token
Test-Endpoint -Name "GET /api/inventory/summary" -Method Get -Uri "$apiUrl/api/inventory/summary" -Token $token

# 7. Component
Write-Host "[7] Component" -ForegroundColor Yellow
if ($bagId) {
    $component = Test-Endpoint -Name "POST /api/components/prepare" -Method Post -Uri ($apiUrl + "/api/components/prepare?bagId=$bagId&componentType=PRBC&volume=350") -Token $token
    $componentId = $component
    if ($componentId) {
        Test-Endpoint -Name "GET /api/components/available" -Method Get -Uri "$apiUrl/api/components/available" -Token $token
        Test-Endpoint -Name "GET /api/components/available?bloodGroup=O%2B" -Method Get -Uri ($apiUrl + "/api/components/available?bloodGroup=O%2B") -Token $token
    }
}

# 8. Emergency Request
Write-Host "[8] Emergency" -ForegroundColor Yellow
if ($hospitalId) {
    $emergBody = @{
        hospitalId = $hospitalId
        patientName = "Patient X"
        bloodGroup = "O+"
        componentType = "PRBC"
        unitsRequired = 2
        notes = "Accident case urgent"
    }
    Test-Endpoint -Name "POST /api/emergency/requests" -Method Post -Uri "$apiUrl/api/emergency/requests" -Token $token -Body $emergBody
}

# 9. Patient Request (pending)
Write-Host "[9] Patient Requests" -ForegroundColor Yellow
Test-Endpoint -Name "GET /api/issues/pending-requests" -Method Get -Uri "$apiUrl/api/issues/pending-requests" -Token $token

# 10. Issue (needs hospital + component)
Write-Host "[10] Issue" -ForegroundColor Yellow
if ($hospitalId -and $componentId) {
    $issueBody = @{
        componentId = $componentId
        bagId = [long]$bagId
        patientName = "Patient Y"
        hospitalId = $hospitalId
        issueType = "Crossmatch"
        issueSlipNumber = "SLIP-001"
        notes = "Routine issue"
    }
    Test-Endpoint -Name "POST /api/issues" -Method Post -Uri "$apiUrl/api/issues" -Token $token -Body $issueBody
}

# 11. Billing
Write-Host "[11] Billing" -ForegroundColor Yellow
$billingBody = @{
    invoiceNumber = "INV-2026-001"
    patientId = 1001
    totalAmount = 3500.00
    taxAmount = 630.00
    discount = 300.00
    paymentStatus = "Paid"
    paymentMode = "Online"
}
$billing = Test-Endpoint -Name "POST /api/billing" -Method Post -Uri "$apiUrl/api/billing" -Token $token -Body $billingBody
$billId = $billing.billingTransactionId
if ($billId) {
    Test-Endpoint -Name "POST /api/billing/$billId/payment" -Method Post -Uri ($apiUrl + "/api/billing/$billId/payment?amount=3500&mode=Online&reference=TXN-001") -Token $token
}

# Summary
Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "  RESULTS: $pass passed / $fail failed" -ForegroundColor $(if ($fail -eq 0) { "Green" } else { "Red" })
Write-Host "==========================================" -ForegroundColor Cyan
$results | Format-Table -AutoSize | Out-Host
