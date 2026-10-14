# InventoryManagementApp

Ez a projekt egy .NET MAUI leltáralkalmazás, amely lokális SQLite adatbázisban kezeli a termékeket.

## Technikai döntések
- A termékadatok lokális SQLite adatbázisban tárolódnak `sqlite-net-pcl` segítségével.
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