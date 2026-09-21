<#
    Chay toan bo kiem chung cua basecode. Dung truoc moi lan commit.

        powershell -ExecutionPolicy Bypass -File verify.ps1

    File nay giu ASCII thuan: Windows PowerShell 5.1 doc .ps1 khong BOM
    theo codepage ANSI, nen ky tu co dau se lam hong parser.

    Muoi buoc:
      1.  GopetCmd.cs con khop voi GopetCMD.cs cua server khong
      2.  Cac asmdef khai bao du tham chieu (loi nay chi Unity moi thay)
      3.  Asset lay tu jar J2ME (.dat/.png/.wav) con khop nguon khong
      4.  Tang Net compile duoc duoi netstandard2.1 (rang buoc that cua Unity)
      5.  Unit test, gom doi chieu TEA voi ban port doc lap
      6.  Tang Runtime compile duoc voi DLL that cua Unity (bat sai API UnityEngine)
      7.  Tang Editor compile duoc voi DLL that cua Unity (AssetPostprocessor)
      8.  PlayMode test compile duoc (chay duoc thi phai dong Editor - xem run-playmode-tests.ps1)
      9.  LiveSmoke con compile duoc (chi build, KHONG chay - khong can server)
      10. Khong file .cs nao vuot 200 dong (tru ngoai le da ghi trong README)
#>

$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
$env:DOTNET_NOLOGO = 1

$root = $PSScriptRoot
$failed = $false

function Step($name, $block) {
    Write-Host ""
    Write-Host "--- $name" -ForegroundColor Cyan
    try {
        & $block
        Write-Host "    OK" -ForegroundColor Green
    } catch {
        Write-Host "    FAIL: $_" -ForegroundColor Red
        $script:failed = $true
    }
}

Step "1/10  Opcode khop voi server" {
    Push-Location "$root\tools"
    try {
        node gen-gopet-cmd/index.js --check
        if ($LASTEXITCODE -ne 0) { throw "GopetCmd.cs lech so voi GopetCMD.cs. Chay: npm run gen:cmd" }

        node check-protocol-coverage/index.js
        if ($LASTEXITCODE -ne 0) { throw "Co server route chua duoc Unity xu ly hoac phan loai" }
    } finally { Pop-Location }
}

Step "2/10  Asmdef khai bao du tham chieu" {
    # Cac du an compile-only gop nhieu thu muc vao MOT assembly nen ranh gioi
    # asmdef bien mat - chung khong the thay "Gopet.Runtime dung Gopet.UiLogic
    # ma quen khai bao". Unity thi tu choi compile. Da dinh mot lan, va chi
    # phat hien luc chay PlayMode test, tuc la sau khi da bat nguoi dung dong Editor.
    Push-Location "$root\tools"
    try {
        node check-asmdef-refs/index.js
        if ($LASTEXITCODE -ne 0) { throw "Co asmdef thieu tham chieu" }
    } finally { Pop-Location }
}

Step "3/10  Asset tu jar J2ME khop nguon" {
    # tools/unpack-jar-dat giai .dat -> PNG + copy PNG/WAV roi. tools/extract-jar-strings
    # boc bang chuoi VN+EN. Ca hai tat dinh: chay lai phai ra dung byte do. --check
    # bat khi ai do sua tay file da giai, hoac jar nguon doi ma quen chay lai tool.
    Push-Location "$root\tools"
    try {
        node unpack-jar-dat/index.js --check
        if ($LASTEXITCODE -ne 0) { throw "Asset trong Assets/Resources/Jar lech so voi client.jar_Decompiler.com. Chay: node tools/unpack-jar-dat/index.js" }

        node extract-jar-strings/index.js --check
        if ($LASTEXITCODE -ne 0) { throw "Bang chuoi lech so voi a.java. Chay: node tools/extract-jar-strings/index.js" }
    } finally { Pop-Location }
}

Step "4/10  Compile duoi netstandard2.1 (rang buoc Unity)" {
    Push-Location "$root\tests\Gopet.Net.UnityCompat"
    try {
        dotnet build --nologo -v quiet
        if ($LASTEXITCODE -ne 0) { throw "Khong compile duoc duoi netstandard2.1 - Unity se tu choi" }
    } finally { Pop-Location }
}

Step "5/10  Unit test" {
    Push-Location "$root\tests\Gopet.Net.Tests"
    try {
        dotnet test --nologo -v quiet
        if ($LASTEXITCODE -ne 0) { throw "Co test fail" }
    } finally { Pop-Location }
}

Step "6/10  Tang Runtime compile voi DLL Unity that" {
    # Gopet.Net.UnityCompat chi phu Assets/Scripts/Net. Runtime/ dung UnityEngine
    # nen nam ngoai tam no - RemoteAssetCache.cs tung khong duoc THU GI compile
    # cho toi khi nguoi dung focus vao Editor. Buoc nay bit lo do.
    Push-Location "$root\tests\Gopet.Runtime.UnityCompat"
    try {
        dotnet build --nologo -v quiet
        if ($LASTEXITCODE -ne 0) { throw "Tang Runtime khong compile duoc - Unity se tu choi" }
    } finally { Pop-Location }
}

Step "7/10  Tang Editor compile voi DLL Unity that" {
    # Assets/Editor dung UnityEditor (AssetPostprocessor, TextureImporter,
    # AudioImporter...) - khong asmdef nao trong Assets/Scripts phu toi. Bit lo
    # cung ly do voi buoc Runtime: sai ten API (vd assetTarget thay vi
    # assetImporter) chi lo ra khi mo Unity Editor neu khong co buoc nay.
    Push-Location "$root\tests\Gopet.Editor.UnityCompat"
    try {
        dotnet build --nologo -v quiet
        if ($LASTEXITCODE -ne 0) { throw "Tang Editor khong compile duoc - Unity se tu choi" }
    } finally { Pop-Location }
}

Step "8/10  PlayMode test compile duoc" {
    # PlayMode test chi CHAY duoc khi Editor da dong. Neu chung khong compile
    # duoc thi ca lan chay hong - ma luc do nguoi dung da dong Editor roi.
    Push-Location "$root\tests\Gopet.PlayMode.Compile"
    try {
        dotnet build --nologo -v quiet
        if ($LASTEXITCODE -ne 0) { throw "PlayMode test khong compile duoc" }
    } finally { Pop-Location }
}

Step "9/10  LiveSmoke con compile duoc" {
    # Chi build. LiveSmoke can server that de CHAY, nhung neu khong build o day
    # thi doi ten mot API trong Assets/Scripts/Net se lam harness muc am tham -
    # den luc can dung nhat moi phat hien.
    Push-Location "$root\tests\Gopet.Net.LiveSmoke"
    try {
        dotnet build --nologo -v quiet
        if ($LASTEXITCODE -ne 0) { throw "LiveSmoke khong compile duoc" }
    } finally { Pop-Location }
}

Step "10/10  Kich thuoc file (rule 200 dong)" {
    # Tea.cs la ngoai le co chu dich - thuat toan lien khoi, xem README.
    # Baseline legacy exceptions are documented in CODE_HEALTH_EXCEPTIONS.md.
    # Files added for parity work are deliberately not exempt.
    $allowed = @(
        'Tea.cs',
        'GopetBootstrap.cs', 'GopetClient.cs', 'GenericMenuView.cs',
        'LoginFormView.Actions.cs', 'LoginFormView.cs', 'LoginScreens.cs',
        'UiRoot.cs', 'CharacterHud.cs', 'CurrencyBar.cs',
        'GameSession.cs', 'MapPortalView.cs', 'MapRenderer.cs', 'MapScene.cs',
        'LoginFormViewTests.cs'
    )

    # Quet ca tests/ va Assets/Editor: file test/editor cung phai giu duoi 200
    # dong, va truoc day chung nam ngoai tam quet nen am tham phinh len.
    $over = @("$root\Assets\Scripts", "$root\Assets\Tests", "$root\Assets\Editor", "$root\tests") |
        ForEach-Object { Get-ChildItem $_ -Filter *.cs -Recurse } |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
        Where-Object { $allowed -notcontains $_.Name } |
        ForEach-Object {
            # Dem dong VAT LY. `Measure-Object -Line` bo qua dong trong, nen mot file
            # 260 dong voi 60 dong trong van lot - guard nhu vay la guard hong.
            $n = @(Get-Content $_.FullName).Count
            if ($n -gt 200) { "$($_.Name) ($n dong)" }
        }

    if ($over) { throw "Vuot 200 dong: $($over -join ', ')" }
}

Write-Host ""
if ($failed) {
    Write-Host "VERIFY THAT BAI" -ForegroundColor Red
    exit 1
}
Write-Host "VERIFY OK" -ForegroundColor Green
