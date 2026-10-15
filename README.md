# InventoryManagementApp

Ez a projekt egy .NET MAUI leltáralkalmazás, amely lokális SQLite adatbázisban kezeli a termékeket.

## Technikai döntések
- A termékadatok lokális SQLite adatbázisban tárolódnak `sqlite-net-pcl` segítségével.
- A terméklistában a vonalkódok Code 128 formátumú képként jelennek meg a `ZXing.Net.Maui.Controls` csomaggal.
- Androidon és iOS-en a termék felvételekor vagy szerkesztésekor kamera segítségével is beolvasható a vonalkód; az alkalmazás kameraengedélyt kér, a beolvasott kódot pedig visszaadja az űrlapnak.
- A terméklista a blokkoló UI frissítése helyett a SQLite lekérdezésekből töltődik be, így a listázás és a keresés egyszerűen skálázható.
- A képernyőket kis, jól elkülöníthető részekre bontottam, hogy a funkciók könnyen bővíthetők legyenek.

## Ha több idő állna rendelkezésre
- A felületet tovább finomítanám külön ViewModel-ekkel és jobb validációs logikával.
- A navigációt és az adatmodell-reprezentációt is tovább tudtam volna egyszerűsíteni, különösen a nagyobb adatfolyamok és az új funkciók esetén.

## További fejlesztések
- termék törlése
- rendezés és szűrés a listában
- közvetlen mennyiségmódosítás a listából
- import/export CSV vagy JSON formátumban
- jobb hibakezelés és felhasználói visszajelzések

## Automatizált tesztek

A Windows xUnit tesztprojekt a ViewModel-validációt, keresést, lapozást, készletmódosítást, készletjelző konvertereket, valamint az SQLite induló adatait és CRUD műveleteit ellenőrzi. Futtatás az `InventoryApp` könyvtárból:

```powershell
dotnet test InventoryApp.Tests\InventoryApp.Tests.csproj -p:EnableWindowsTargeting=true
```