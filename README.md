# InventoryManagementApp

Egyszerű, .NET MAUI-alapú leltáralkalmazás termékek és készletmennyiségek helyi kezelésére. A termékek SQLite-adatbázisban tárolódnak; az alkalmazás többek között keresést, lapozást, termékfelvételt, -szerkesztést és vonalkód-kezelést biztosít.

## Funkciók

- Terméklista névvel, vonalkóddal, árral és készletmennyiséggel.
- Keresés terméknév és vonalkód alapján, kis- és nagybetűtől függetlenül.
- Lapozás választható, 10, 25, 50 vagy 100 termékes oldalmérettel.
- Nulla készletű termékek piros színnel jelennek meg; pozitív készlet esetén a készletjelzés eltérő színű.
- A listában a készlet növelhető és csökkenthető. Nulla készletnél a csökkentés helyett törlés érhető el.
- Üres lista és sikertelen keresés esetén a lista megfelelő üresállapot-üzenetet mutat.
- Új termék felvétele külön űrlapon, mezőnkénti validációval és hibaüzenetekkel.
- Termék részleteinek megtekintése és módosítása külön oldalon.
- A vonalkód képként is megjelenik a listában.
- Androidon és iOS-en kameraalapú vonalkód-beolvasás indítható az új termék és a szerkesztő űrlapról. A beolvasott kód visszakerül az űrlap vonalkódmezőjébe.
- Első indításkor, üres adatbázis esetén 5 000 teszttermék jön létre; a nevek és vonalkódok egyediek, és a mintában nulla készletű termékek is vannak.

## A feladat követelményeinek teljesítése

| A PDF-ben szereplő követelmény | Megvalósítás |
|---|---|
| Helyi SQLite-adatbázis használata | `DatabaseService`, `sqlite-net-pcl`; az adatbázis fájlja a MAUI `FileSystem.AppDataDirectory` könyvtárába kerül. |
| A lista az adatbázisból töltődjön be, nagyobb elemszámnál is legyen használható | A főoldal `CollectionView`-t használ, a termékek lapozhatók. |
| Nulla készlet vizuális megkülönböztetése | `QuantityToColorConverter`; a nulla készlet piros színnel látható. |
| Keresés név és vonalkód alapján | A `ProductListViewModel` a betöltött termékek között mindkét mezőben keres. |
| Termék felvétele külön oldalon, SQLite-mentéssel | `AddProductPage` és `AddProductViewModel`; sikeres mentés után a felvett termék az adatbázisból történő következő betöltéskor jelenik meg. |
| Termék részleteinek szerkesztése és mentése | `ProductDetailPage` a `IProductRepository` absztrakción keresztül olvas és módosít. |
| README-ben technikai döntések, idő esetén újragondolandó részek és továbbfejlesztési ötletek | Lásd a dokumentum megfelelő fejezeteit. |

## Alkalmazásfelépítés

Az alkalmazás .NET MAUI Shell-alapú, MVVM mintát használ. A felhasználói felület XAML-ben, a nézetekhez tartozó eseménykezelés és életciklus-logika a code-behind fájlokban található.

```text
InventoryApp/
├── Controls/       # Újrafelhasználható megjelenítési vezérlők
├── Converters/     # Készletértékből színt és művelet-láthatóságot képző konverterek
├── Data/           # IProductRepository és SQLite-alapú DatabaseService
├── Models/         # Product SQLite-adatmodell
├── Services/       # Navigációs absztrakció és Shell-implementáció
├── ViewModels/     # Lista- és újtermék-nézetmodellek
├── Views/          # Lista-, felvételi, részletező és vonalkódolvasó oldalak
├── Platforms/      # Platformspecifikus engedélyek és konfiguráció
└── InventoryApp.Tests/
                  # xUnit unit- és SQLite-integrációs tesztek
```

### Adatelérés és Dependency Inversion

- Az `IProductRepository` írja le a termékek lekérdezéséhez, felvételéhez, módosításához, törléséhez, azonosító szerinti kereséséhez és vonalkód-ellenőrzéséhez szükséges műveleteket.
- A `DatabaseService` az interfész SQLite-implementációja. Létrehozza a táblát, üres adatbázisnál előállítja a tesztadatokat, majd az adatbázis-műveleteket végzi.
- A `ProductListViewModel`, az `AddProductViewModel` és a `ProductDetailPage` az `IProductRepository` interfésztől függ, nem a konkrét SQLite-osztálytól. Az implementációt a `MauiProgram` dependency injection konfigurációja adja meg.
- Ez a felépítés a Dependency Inversion Principle-t alkalmazza, és lehetővé teszi, hogy a ViewModel-tesztekben a repository mock objektummal helyettesíthető legyen.
- Az alkalmazás Shell-navigációja az `IAppNavigation` interfészen keresztül érhető el ott, ahol a ViewModel navigációt kezdeményez.

### Adatmodell és tárolás

A `Product` modell adattagjai:

| Mező | Típus | Leírás |
|---|---|---|
| `Id` | `int` | SQLite elsődleges kulcs, automatikusan növekszik |
| `Name` | `string` | Terméknév |
| `Barcode` | `string` | Vonalkód értéke |
| `Price` | `decimal` | Ár |
| `Quantity` | `decimal` | Készletmennyiség |

Az adatbázis neve `inventory.db3`. A `DatabaseService` inicializálása aszinkron; a repository műveletei megvárják az inicializálást, így a tábla létrehozása és az induló adatok előállítása befejeződik az első lekérdezés vagy írás előtt.

### Képernyők és felhasználói folyamatok

**Terméklista** (`ProductListPage`)

- Az adatbázisból betöltött termékeket kártyás listában jeleníti meg.
- A lista tetején található kereső név és vonalkód alapján szűr.
- Az oldalméret-választó és az előző/következő vezérlők a lapozást kezelik. Kereséskor a lapozás az első oldalra áll vissza.
- Üres adatbázisnál és nulla találatnál külön üresállapot-szöveg jelenik meg.
- A kártya kiválasztása megnyitja a termék részletező oldalát. A készletműveletek közvetlenül a kártyán érhetők el.
- A nulla készletű termék törlése a listán lévő „Törlés” művelettel lehetséges.

**Új termék** (`AddProductPage`, `AddProductViewModel`)

- A név és vonalkód kötelező; a vonalkódnak egyedinek kell lennie.
- Az árnak nem negatív, érvényes számnak, a készletnek nem negatív egész értéknek kell lennie.
- A mezők alatti követelmények és a mezőspecifikus hibák segítik a kitöltést. A Mentés gomb csak érvényes adatoknál aktív.
- Sikeres adatbázis-mentés után az oldal visszanavigál a terméklistára.

**Termékrészletek** (`ProductDetailPage`)

- A kiválasztott terméket azonosító alapján tölti be, és lehetővé teszi az adatainak szerkesztését.
- A módosításokat az `IProductRepository` menti SQLite-ba.

**Vonalkódolvasó** (`BarcodeScannerPage`)

- A ZXing.Net.Maui.Controls `CameraBarcodeReaderView` vezérlőjét használja.
- Kameraengedélyt kér; a sikeres első találatot a fő szálon dolgozza fel, leállítja a detektálást, majd visszaadja a kódot az előző űrlapnak.
- A kamera detektálása az oldal megjelenésekor indul, eltűnésekor leáll.
- Androidon a kameraengedély az `AndroidManifest.xml` fájlban, iOS-en az adatvédelmi célú használati szöveg az `Info.plist` fájlban szerepel. A kameraolvasás Androidon és iOS-en érhető el; a Windows célplatform nem támogatja a ZXing kamerás olvasását.

### Vonalkódkép megjelenítése

A lista a vonalkódképet a `barcode.tec-it.com` HTTPS szolgáltatás URI-jából tölti be, `UriImageSource` gyorsítótárazással. Emiatt a kép megjelenítéséhez hálózati kapcsolat szükséges, és a vonalkód értéke elküldésre kerül a szolgáltatásnak. Ha a termékadatok nem hagyhatják el az eszközt, célszerű a képgenerálást helyben megvalósítani a ZXing csomaggal.

## Technikai döntések

- **.NET MAUI és Shell:** közös alkalmazásstruktúra a projektben célzott Android, iOS, Mac Catalyst és Windows platformokhoz, egyszerű oldalnavigációval.
- **MVVM és CommunityToolkit.Mvvm:** az állapot, a parancsok és az adat-hozzáférés elkülönül a felülettől; a Toolkit forrásgenerált megfigyelhető tulajdonságokat és parancsokat biztosít.
- **Repository interfész:** az adatforrás el van választva az alkalmazási logikától; ez támogatja a DIP-et és a mock-alapú tesztelést.
- **SQLite és `sqlite-net-pcl`:** az adatok helyben, külső szerver nélkül is megmaradnak az alkalmazás újraindításai között.
- **`CollectionView` és lapozás:** a teljes termékhalmaz nem kerül egyszerre a felületre, ami több ezer elemnél is áttekinthetőbb megjelenítést ad.
- **ZXing.Net.Maui.Controls:** a kameraolvasó a vonalkód-bevitelt segíti. A listában megjelenő vonalkódkép ettől külön, jelenleg távoli HTTPS-szolgáltatáson keresztül készül.
- **Automatikus mintadatok:** csak üres adatbázisnál futnak, így meglévő adatbázis esetén nem írják felül a felhasználó termékeit.

## Követelmények és futtatás

Szükséges a .NET 9 SDK és a célplatformhoz tartozó .NET MAUI workload. Android/iOS buildhez az adott platform SDK-ja és fejlesztői eszközei is szükségesek.

A parancsokat az `InventoryApp` könyvtárból futtasd.

Windows build:

```powershell
dotnet build InventoryApp.csproj -f net9.0-windows10.0.19041.0 -p:EnableWindowsTargeting=true
```

Android build:

```powershell
dotnet build InventoryApp.csproj -f net9.0-android
```

iOS build (macOS és megfelelő Apple fejlesztői környezet szükséges):

```powershell
dotnet build InventoryApp.csproj -f net9.0-ios
```

Az alkalmazás indítása Visual Studio MAUI fejlesztői környezetből, a kívánt telepített emulátort vagy eszközt kiválasztva javasolt. A kameraolvasás teszteléséhez valódi kamerával rendelkező Android- vagy iOS-eszköz, illetve engedélyezett kamera-hozzáférés szükséges. A funkció saját Xiaomi android készüléken lett tesztesztelve.

## Automatizált tesztek

Az xUnit projekt a ViewModel-funkciókat Moq mock objektumokkal ellenőrzi. A SQLite-indulóadatok és CRUD műveletek külön integrációs tesztje ideiglenes adatbázist használ.

```powershell
dotnet test InventoryApp.Tests\InventoryApp.Tests.csproj -p:EnableWindowsTargeting=true
```

A jelenlegi tesztek a következőket fedik le:

- felvételi űrlap kötelező mezőit, validációját és mentését;
- ismétlődő vonalkód elutasítását és a scanner oldalra navigálást;
- keresést, üres keresési eredményt és lapozást;
- készletnövelést, -csökkentést és nulla készletnél törlést;
- készlethez tartozó szín- és láthatósági konvertereket;
- az 5 000 induló termék egyediségét, nulla készletű tesztadatokat és az SQLite CRUD műveleteit.

## Továbbfejlesztési lehetőségek

- A lista vonalkódképeinek helyi generálása, hogy ne kelljen a vonalkódokat külső szolgáltatásnak elküldeni, és hálózat nélkül is működjön.
- Külön `ProductDetailViewModel` létrehozása, valamint a részletező oldal validációjának egységesítése az új termék űrlapjával.
- Adatbázis-migrációk és a hibák egységes, felhasználóbarát kezelése.
- Rendezés, összetettebb készletszűrők, kategóriák és termékekhez kapcsolódó további adatok.
- Tesztek kiegészítése a részletező képernyővel, navigációs visszatérési adatokkal, adatbázis-hibákkal és különböző kultúrák szerinti számformátumokkal.
- Kameraolvasási integrációs tesztek valós Android- és iOS-eszközökön.
- Termékek importálása/exportálása CSV vagy JSON formátumban.

## Több rendelkezésre álló idő esetén

A kezdeti megoldásnál a részletező oldal egyszerű code-behind logikát használ; ezt külön ViewModelbe és közös validációs szolgáltatásba szervezném át. A vonalkódkép külső szolgáltatásból történő betöltését helyi generálásra cserélném, hogy a kódok ne kerüljenek harmadik félhez, és a megjelenítés internetkapcsolat nélkül is működjön. Emellett a többplatformos viselkedést és az adatbázis-hibák kezelését szélesebb automatizált és eszközön futó tesztkészlettel ellenőrizném.
