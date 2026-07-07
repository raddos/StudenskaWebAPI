# 🎓 Studentski Web API

ASP.NET Core 8 Web API sa Entity Framework Core i LINQ za upravljanje
studentskom bazom podataka.

---

## 📋 Zahtjevi

| Softver | Verzija |
|---------|---------|
| Visual Studio | 2022 (17.8+) |
| .NET SDK | 8.0+ |
| SQL Server | 2019+ ili LocalDB |

---

## 🚀 Pokretanje

### 1. Kloniraj / otvori projekat u Visual Studio

```
File → Open → Project/Solution → StudentskiWebAPI.csproj
```

### 2. Konekcija na bazu (appsettings.json)

**Udaljeni server:**
```json
"StudentskaBaza": "Server=pabp.viser.edu.rs;Database=Studentska;User Id=student;Password=password;TrustServerCertificate=True"
```

**Lokalni SQL Server (LocalDB):**
```json
"StudentskaBaza": "Server=(localdb)\\mssqllocaldb;Database=Studentska;Trusted_Connection=True;TrustServerCertificate=True"
```

### 3. Primijeni migraciju (kreira tabelu Prijava_BrojIndeksa)

```
Package Manager Console (Tools → NuGet Package Manager → PMC):

PM> Update-Database
```

Ili putem CLI:
```bash
dotnet ef database update
```

> ⚠️ **Napomena:** Migracija kreira **SAMO** tabelu `Prijava_BrojIndeksa`.
> Sve ostale tabele (Student, Predmet, itd.) već postoje u bazi.

### 4. Pokretanje

```
F5 ili Ctrl+F5 u Visual Studio
```

Swagger UI se otvara automatski na: `https://localhost:{port}/swagger`

---

## 📡 API Endpoints

### 👨‍🎓 Students Controller

| Metoda | Ruta | Opis |
|--------|------|------|
| `GET` | `/api/Students` | Lista svih studenata (ime, prezime, broj indeksa) |
| `GET` | `/api/Students/{id}` | Jedan student po ID-u |
| `GET` | `/api/Students/pretraga` | Pretraga studenata |
| `PUT` | `/api/Students/{id}/licni-podaci` | Izmena SAMO imena i prezimena |
| `GET` | `/api/Students/{id}/polozeni-predmeti` | Položeni predmeti + prosek ocena |

**Pretraga – query parametri:**
```
GET /api/Students/pretraga?q=pera&smer=SW&godinaUpisa=2021&broj=12
```
- `q` → tekstualna pretraga (ime, prezime, broj indeksa)
- `smer` → filter po smeru (npr. "SW")
- `godinaUpisa` → filter po godini upisa
- `broj` → filter po rednom broju indeksa

**PUT /api/Students/{id}/licni-podaci – body:**
```json
{
  "ime": "Petar",
  "prezime": "Petrović"
}
```

---

### 📚 StudentPredmet Controller

| Metoda | Ruta | Opis |
|--------|------|------|
| `GET` | `/api/StudentPredmet/{idStudenta}` | Izborna lista predmeta studenta |
| `GET` | `/api/StudentPredmet/{idStudenta}/za-polaganje` | Nepoloženi predmeti (dostupni za prijavu) |
| `POST` | `/api/StudentPredmet` | Dodaj predmet u izbornu listu |
| `DELETE` | `/api/StudentPredmet/{idStudentPredmet}` | Ukloni predmet (ZAŠTITA: nije moguće ako je položen) |

**POST body:**
```json
{
  "idStudenta": 1,
  "idPredmeta": 5,
  "skolskaGodina": "2023/2024"
}
```

---

### 📝 Prijava Controller

| Metoda | Ruta | Opis |
|--------|------|------|
| `POST` | `/api/Prijava` | Prijava ispita za studenta |
| `GET` | `/api/Prijava/ispit/{idIspita}` | Sve prijave za jedan ispit |
| `GET` | `/api/Prijava/student/{idStudenta}` | Sve prijave jednog studenta |
| `DELETE` | `/api/Prijava/{idPrijave}` | Odjava ispita (brisanje prijave) |

**POST body:**
```json
{
  "idStudenta": 1,
  "idIspita": 10
}
```

**Pravila za prijavu ispita:**
1. ✅ Student mora postojati
2. ✅ Ispit mora postojati
3. ✅ Predmet mora biti u izbornoj listi studenta
4. ✅ Predmet ne smije biti već položen
5. ✅ Student nije već prijavljen na isti ispit

---

### 📋 Pomoćni Kontroleri

| Metoda | Ruta | Opis |
|--------|------|------|
| `GET` | `/api/Predmets` | Lista aktivnih predmeta |
| `GET` | `/api/IspitniRoks` | Lista ispitnih rokova |
| `GET` | `/api/Ispits` | Lista ispita (predmet + rok) |

---

## 🗄️ Baza podataka – novi model

```sql
-- Kreira se automatski putem migracije
CREATE TABLE Prijava_BrojIndeksa (
    IdPrijave     INT IDENTITY(1,1) PRIMARY KEY,
    IdStudenta    INT NOT NULL,
    BrojIndeksa   NVARCHAR(30) NOT NULL,   -- "SW-12/2021" (denorm.)
    IdIspita      INT NOT NULL,
    IdRoka        INT NOT NULL,
    DatumPrijave  DATETIME2 DEFAULT GETUTCDATE(),
    StatusPrijave INT DEFAULT 0,           -- 0=Prijavljen, 1=Izašao, 2=Odustao

    CONSTRAINT FK_Prijava_Student    FOREIGN KEY (IdStudenta) REFERENCES Student(IdStudenta),
    CONSTRAINT FK_Prijava_Ispit      FOREIGN KEY (IdIspita)   REFERENCES Ispit(IdIspita),
    CONSTRAINT FK_Prijava_IspitniRok FOREIGN KEY (IdRoka)     REFERENCES IspitniRok(IdRoka),
    CONSTRAINT UQ_Prijava            UNIQUE (IdStudenta, IdIspita)
);
```

---

## 🔧 Migracije – komande

```powershell
# Package Manager Console (Visual Studio):

# Dodaj novu migraciju
PM> Add-Migration ImeMigracije

# Primijeni migraciju na bazu
PM> Update-Database

# Pogledaj generisani SQL
PM> Script-Migration

# Vrati se na prethodnu migraciju
PM> Update-Database PrethodnaMigracija
```

---

## 🌐 CORS konfiguracija za Vue.js frontend

U `appsettings.json` dodaj origin svog frontend projekta:
```json
"Cors": {
  "AllowedOrigins": ["http://localhost:5173"]
}
```

---

## 📁 Struktura projekta

```
StudentskiWebAPI/
├── Controllers/
│   ├── StudentsController.cs          ← Student: lista, pretraga, izmena
│   ├── StudentPredmetController.cs    ← Izborna lista predmeta
│   ├── PrijavaController.cs           ← Prijave ispita (novi model)
│   └── SupportingControllers.cs      ← Predmeti, IspitniRoci, Ispiti
├── Data/
│   └── StudentskiContext.cs           ← EF DbContext
├── DTOs/
│   └── DTOs.cs                        ← Svi Data Transfer Objects
├── Migrations/
│   ├── 20240101000001_InitialCreate.cs ← Kreira Prijava_BrojIndeksa
│   └── StudentskiContextModelSnapshot.cs
├── Models/
│   └── Models.cs                      ← Svi domenski modeli
├── Program.cs                         ← Konfiguracija i startup
├── appsettings.json                   ← Konekcioni string, CORS
└── StudentskiWebAPI.csproj
```
