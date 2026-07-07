# 🎓 Studentska Aplikacija — Vue 3 + Vite

## Pokretanje

```bash
npm install
npm run dev
```

Aplikacija se pokreće na `http://localhost:5173`.

## Struktura projekta

```
src/
├── components/
│   ├── AppButton.vue        # Višenamjensko dugme (GET/POST/PUT/DELETE stil)
│   ├── ToastContainer.vue   # Sistem obavještenja
│   ├── StudentCard.vue      # Kartica jednog studenta u listi
│   ├── EditStudent.vue      # Forma za izmjenu podataka
│   ├── StudentExams.vue     # Ispiti, prosjeк ocjena, uspješnost
│   └── ZapisnikView.vue     # Pregled zapisnika + statistika
├── composables/
│   └── useToast.js          # Centralizovane poruke + toast sustav
├── services/
│   └── api.js               # Sve API metode
├── App.vue                  # Glavna komponenta
└── main.js
```

## API Proxy

`vite.config.js` preusmjerava `/api/*` → `http://pabp.viser.edu.rs:5000/api/*`
što rješava CORS problem tokom razvoja.

## Funkcionalnosti

| Funkcionalnost               | Komponenta        |
|-----------------------------|-------------------|
| Lista studenata             | `App.vue` + `StudentCard` |
| Pretraga studenata          | `App.vue`         |
| Izmjena ime/prezime         | `EditStudent`     |
| Položeni ispiti             | `StudentExams`    |
| Prosječna ocjena            | `StudentExams`    |
| Uspješnost po predmetu      | `StudentExams`    |
| Pregled zapisnika           | `ZapisnikView`    |
| Statistika ocjena zapisnika | `ZapisnikView`    |
| Toast obavještenja          | `ToastContainer`  |

## Napomena

Ako API vraća drugačije nazive polja (npr. `studentId` vs `id`),
prilagoditi mapping u `StudentCard.vue` i `App.vue` (funkcija `formattedIndex`).
