# E2E test for Event Booking System on http://localhost:5000
$base = "http://localhost:5000"
$results = @()

function Get-Token($html) {
    if ($html -match 'name="__RequestVerificationToken"[^>]*value="([^"]+)"') { return $Matches[1] }
    return $null
}

function Step($name, $ok, $detail) {
    $script:results += [pscustomobject]@{ Test = $name; Pass = $ok; Detail = $detail }
    Write-Output ("[{0}] {1} {2}" -f ($(if($ok){"PASS"}else{"FAIL"}), $name, $detail))
}

# ---------- 1. Public pages ----------
foreach ($p in @("/", "/Events", "/Account/Login", "/Account/Register", "/Events/Details/1", "/Events/Details/2", "/Events/Details/3")) {
    try {
        $r = Invoke-WebRequest -Uri "$base$p" -UseBasicParsing
        Step "GET $p" ($r.StatusCode -eq 200) "$($r.StatusCode)"
    } catch { Step "GET $p" $false "$_" }
}

# Search/filter
try {
    $r = Invoke-WebRequest -Uri "$base/Events?search=rock&category=Concert" -UseBasicParsing
    Step "Search filter" ($r.StatusCode -eq 200 -and $r.Content -match "Rock Concert") "found Rock Concert"
} catch { Step "Search filter" $false "$_" }

# Nonexistent event -> 404
try {
    $r = Invoke-WebRequest -Uri "$base/Events/Details/999" -UseBasicParsing
    Step "404 unknown event" $false "got $($r.StatusCode), expected 404"
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    Step "404 unknown event" ($code -eq 404) "$code"
}

# ---------- 2. Customer login ----------
$cust = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$r = Invoke-WebRequest -Uri "$base/Account/Login" -WebSession $cust -UseBasicParsing
$tok = Get-Token $r.Content
$body = @{ email = "fatima@ebs.com"; password = "customer123"; __RequestVerificationToken = $tok }
$r = Invoke-WebRequest -Uri "$base/Account/Login" -Method POST -Body $body -WebSession $cust -UseBasicParsing
Step "Customer login" ($r.Content -match "Fatima" -or $r.BaseResponse.ResponseUri -match "Events") "landed $($r.BaseResponse.ResponseUri)"

# Wrong password rejected
$bad = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$r = Invoke-WebRequest -Uri "$base/Account/Login" -WebSession $bad -UseBasicParsing
$tok2 = Get-Token $r.Content
$body2 = @{ email = "fatima@ebs.com"; password = "WRONG"; __RequestVerificationToken = $tok2 }
$r = Invoke-WebRequest -Uri "$base/Account/Login" -Method POST -Body $body2 -WebSession $bad -UseBasicParsing
Step "Bad password rejected" ($r.Content -match "Invalid credentials") "error shown"

# ---------- 3. Checkout flow (Credit Card) ----------
$r = Invoke-WebRequest -Uri "$base/Bookings/Checkout?eventId=1" -WebSession $cust -UseBasicParsing
$tok = Get-Token $r.Content
Step "Checkout page" ($r.StatusCode -eq 200 -and $tok) "token ok"

# discover ticket category ids from inputs name="quantities[N]"
$catIds = [regex]::Matches($r.Content, 'quantities\[(\d+)\]') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
Step "Ticket categories found" ($catIds.Count -ge 1) "ids: $($catIds -join ',')"

$body = @{
    "quantities[$($catIds[0])]" = "2"
    paymentMethod = "CreditCard"
    cardNumber = "4111111111111111"
    __RequestVerificationToken = $tok
}
$r = Invoke-WebRequest -Uri "$base/Bookings/Checkout?eventId=1" -Method POST -Body $body -WebSession $cust -UseBasicParsing
Step "Booking + payment" ($r.Content -match "Booking Confirmed") "confirmation page"
$bookingRef = if ($r.Content -match 'B(\d{5})') { [int]$Matches[1] } else { 0 }
Step "Booking ref" ($bookingRef -gt 0) "ref B$bookingRef"

# History shows it
$r = Invoke-WebRequest -Uri "$base/Bookings/History" -WebSession $cust -UseBasicParsing
Step "History lists booking" ($r.Content -match "Rock Concert") "found"

# Notifications (Observer)
$r = Invoke-WebRequest -Uri "$base/Home/Notifications" -WebSession $cust -UseBasicParsing
Step "Customer notification (Observer)" ($r.Content -match "Confirmed") "notification present"

# ---------- 4. E-Wallet booking on event 3 ----------
$r = Invoke-WebRequest -Uri "$base/Bookings/Checkout?eventId=3" -WebSession $cust -UseBasicParsing
$tok = Get-Token $r.Content
$catIds3 = [regex]::Matches($r.Content, 'quantities\[(\d+)\]') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
$body = @{
    "quantities[$($catIds3[0])]" = "1"
    paymentMethod = "EWallet"
    walletId = "EW-777"
    __RequestVerificationToken = $tok
}
$r = Invoke-WebRequest -Uri "$base/Bookings/Checkout?eventId=3" -Method POST -Body $body -WebSession $cust -UseBasicParsing
Step "E-Wallet strategy" ($r.Content -match "Booking Confirmed" -and $r.Content -match "E-Wallet") "paid via e-wallet"

# ---------- 5. Failed payment (invalid card) rolls back ----------
$r = Invoke-WebRequest -Uri "$base/Bookings/Checkout?eventId=2" -WebSession $cust -UseBasicParsing
$tok = Get-Token $r.Content
$catIds2 = [regex]::Matches($r.Content, 'quantities\[(\d+)\]') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
$availBefore = if ($r.Content -match '(\d+) available') { $Matches[1] } else { "?" }
$body = @{
    "quantities[$($catIds2[0])]" = "1"
    paymentMethod = "CreditCard"
    cardNumber = "12"   # invalid
    __RequestVerificationToken = $tok
}
$r = Invoke-WebRequest -Uri "$base/Bookings/Checkout?eventId=2" -Method POST -Body $body -WebSession $cust -UseBasicParsing
Step "Failed payment handled" ($r.Content -match "Payment failed") "error surfaced"
$r = Invoke-WebRequest -Uri "$base/Bookings/Checkout?eventId=2" -WebSession $cust -UseBasicParsing
$availAfter = if ($r.Content -match '(\d+) available') { $Matches[1] } else { "?" }
Step "Stock restored after fail" ($availBefore -eq $availAfter) "before=$availBefore after=$availAfter"

# ---------- 6. Zero tickets rejected ----------
$r = Invoke-WebRequest -Uri "$base/Bookings/Checkout?eventId=1" -WebSession $cust -UseBasicParsing
$tok = Get-Token $r.Content
$body = @{ paymentMethod = "CreditCard"; cardNumber = "4111111111111111"; __RequestVerificationToken = $tok }
$r = Invoke-WebRequest -Uri "$base/Bookings/Checkout?eventId=1" -Method POST -Body $body -WebSession $cust -UseBasicParsing
Step "Zero tickets rejected" ($r.Content -match "at least one ticket") "validation shown"

# ---------- 7. Cancel booking ----------
$r = Invoke-WebRequest -Uri "$base/Bookings/History" -WebSession $cust -UseBasicParsing
$tok = Get-Token $r.Content
$cancelId = if ($r.Content -match 'Cancel" asp-route-id') { 0 } else { $bookingRef }
$body = @{ __RequestVerificationToken = $tok }
$r = Invoke-WebRequest -Uri "$base/Bookings/Cancel/$bookingRef" -Method POST -Body $body -WebSession $cust -UseBasicParsing
Step "Cancel booking" ($r.Content -match "Booking cancelled" -or $r.Content -match "Cancelled") "cancelled B$bookingRef"

# ---------- 8. Register new customer ----------
$new = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$r = Invoke-WebRequest -Uri "$base/Account/Register" -WebSession $new -UseBasicParsing
$tok = Get-Token $r.Content
$em = "test$(Get-Random)@ebs.com"
$body = @{ name = "Test User"; email = $em; password = "test1234"; role = "Customer"; __RequestVerificationToken = $tok }
$r = Invoke-WebRequest -Uri "$base/Account/Register" -Method POST -Body $body -WebSession $new -UseBasicParsing
Step "Register new customer (Factory)" ($r.Content -match "Test User") "created $em"

# Duplicate email rejected
$dup = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$r = Invoke-WebRequest -Uri "$base/Account/Register" -WebSession $dup -UseBasicParsing
$tok = Get-Token $r.Content
$body = @{ name = "Dup"; email = "fatima@ebs.com"; password = "test1234"; role = "Customer"; __RequestVerificationToken = $tok }
$r = Invoke-WebRequest -Uri "$base/Account/Register" -Method POST -Body $body -WebSession $dup -UseBasicParsing
Step "Duplicate email rejected" ($r.Content -match "already in use") "error shown"

# Admin registration blocked
$adm0 = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$r = Invoke-WebRequest -Uri "$base/Account/Register" -WebSession $adm0 -UseBasicParsing
$tok = Get-Token $r.Content
$body = @{ name = "Evil"; email = "evil$(Get-Random)@x.com"; password = "test1234"; role = "Admin"; __RequestVerificationToken = $tok }
$r = Invoke-WebRequest -Uri "$base/Account/Register" -Method POST -Body $body -WebSession $adm0 -UseBasicParsing
Step "Admin signup blocked" ($r.Content -match "not allowed") "blocked"

# ---------- 9. Organizer flow ----------
$org = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$r = Invoke-WebRequest -Uri "$base/Account/Login" -WebSession $org -UseBasicParsing
$tok = Get-Token $r.Content
$body = @{ email = "leen@ebs.com"; password = "organizer123"; __RequestVerificationToken = $tok }
$r = Invoke-WebRequest -Uri "$base/Account/Login" -Method POST -Body $body -WebSession $org -UseBasicParsing
Step "Organizer login" ($r.Content -match "Leen") "ok"

$r = Invoke-WebRequest -Uri "$base/Organizer" -WebSession $org -UseBasicParsing
Step "Organizer dashboard" ($r.StatusCode -eq 200 -and $r.Content -match "Rock Concert") "events listed"

$r = Invoke-WebRequest -Uri "$base/Organizer/Bookings" -WebSession $org -UseBasicParsing
Step "Organizer sees bookings" ($r.Content -match "Fatima") "customer booking visible"

$r = Invoke-WebRequest -Uri "$base/Organizer/Reports" -WebSession $org -UseBasicParsing
Step "Organizer reports" ($r.StatusCode -eq 200 -and $r.Content -match "Revenue") "report renders"

# Create event
$r = Invoke-WebRequest -Uri "$base/Organizer/Create" -WebSession $org -UseBasicParsing
$tok = Get-Token $r.Content
$body = @{
    Title = "E2E Test Gala"; Category = "Workshop"; Venue = "Test Hall"
    EventDate = (Get-Date).AddDays(60).ToString("yyyy-MM-ddTHH:mm")
    Description = "Created by automated test"
    catName = @("General",""," "); catPrice = @("10","0","0"); catQty = @("100","0","0")
    __RequestVerificationToken = $tok
}
$r = Invoke-WebRequest -Uri "$base/Organizer/Create" -Method POST -Body $body -WebSession $org -UseBasicParsing
Step "Organizer create event" ($r.Content -match "E2E Test Gala") "event listed"

# Organizer notification from booking (Observer)
$r = Invoke-WebRequest -Uri "$base/Home/Notifications" -WebSession $org -UseBasicParsing
Step "Organizer notification (Observer)" ($r.Content -match "New booking") "present"

# ---------- 10. Admin flow ----------
$adm = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$r = Invoke-WebRequest -Uri "$base/Account/Login" -WebSession $adm -UseBasicParsing
$tok = Get-Token $r.Content
$body = @{ email = "admin@ebs.com"; password = "admin123"; __RequestVerificationToken = $tok }
$r = Invoke-WebRequest -Uri "$base/Account/Login" -Method POST -Body $body -WebSession $adm -UseBasicParsing
Step "Admin login" ($r.Content -match "System Admin") "ok"

$r = Invoke-WebRequest -Uri "$base/Admin" -WebSession $adm -UseBasicParsing
Step "Admin dashboard" ($r.StatusCode -eq 200 -and $r.Content -match "Gross Revenue") "KPIs render"

$r = Invoke-WebRequest -Uri "$base/Admin/Users" -WebSession $adm -UseBasicParsing
Step "Admin users list" ($r.Content -match "fatima@ebs.com") "users listed"

$r = Invoke-WebRequest -Uri "$base/Admin/Bookings" -WebSession $adm -UseBasicParsing
Step "Admin all bookings" ($r.StatusCode -eq 200) "$($r.StatusCode)"

$r = Invoke-WebRequest -Uri "$base/Admin/Reports" -WebSession $adm -UseBasicParsing
Step "Admin reports" ($r.StatusCode -eq 200 -and $r.Content -match "Failed Payments") "renders"

# Admin notification (Observer)
$r = Invoke-WebRequest -Uri "$base/Home/Notifications" -WebSession $adm -UseBasicParsing
Step "Admin audit notification (Observer)" ($r.Content -match "Audit") "present"

# ---------- 11. Access control ----------
$anon = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$r = Invoke-WebRequest -Uri "$base/Admin" -WebSession $anon -UseBasicParsing
Step "Anon blocked from /Admin" ($r.Content -match "Login") "redirected to login"
$r = Invoke-WebRequest -Uri "$base/Organizer" -WebSession $cust -UseBasicParsing
Step "Customer blocked from /Organizer" ($r.Content -match "Login") "redirected"
$r = Invoke-WebRequest -Uri "$base/Bookings/History" -WebSession $org -UseBasicParsing
Step "Organizer blocked from customer history" ($r.Content -match "Login") "redirected"

# ---------- Summary ----------
Write-Output ""
Write-Output "================ SUMMARY ================"
$fail = $results | Where-Object { -not $_.Pass }
Write-Output ("Total: {0}  Pass: {1}  Fail: {2}" -f $results.Count, ($results.Count - $fail.Count), $fail.Count)
if ($fail) { $fail | ForEach-Object { Write-Output "FAILED: $($_.Test) - $($_.Detail)" } }
