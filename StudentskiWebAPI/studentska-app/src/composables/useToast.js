import { ref } from 'vue'

// ─── Centralizovane poruke ────────────────────────────────────────────────────
export const MESSAGES = {
  STUDENTS_LOADED:          '✅ Studenti su uspješno učitani.',
  STUDENT_UPDATED:          '✅ Lični podaci su sačuvani.',
  STUDENT_UPDATE_FAIL:      '❌ Greška pri izmjeni podataka.',
  STUDENT_LOAD_FAIL:        '❌ Greška pri učitavanju studenata.',
  ISPITI_LOADED:            '✅ Ispiti su učitani.',
  ISPITI_LOAD_FAIL:         '❌ Greška pri učitavanju ispita.',
  ZAPISNIK_LOADED:          '✅ Zapisnik je učitan.',
  ZAPISNIK_LOAD_FAIL:       '❌ Greška pri učitavanju zapisnika.',
  ZAPISNICI_LOAD_FAIL:      '❌ Greška pri učitavanju zapisnika.',
  SIFARNICI_LOAD_FAIL:      '❌ Greška pri učitavanju šifarnika.',
  VALIDATION_REQUIRED:      '⚠️ Popunite sva obavezna polja.',
  VALIDATION_NAME:          '⚠️ Ime i prezime smiju sadržati samo slova.',
  PREDMET_DODAN:            '✅ Predmet je dodan u izbornu listu.',
  PREDMET_DODAN_FAIL:       '❌ Greška pri dodavanju predmeta.',
  PREDMET_OBRISAN:          '✅ Predmet je uklonjen iz izborne liste.',
  PREDMET_OBRISAN_FAIL:     '❌ Nije moguće ukloniti predmet.',
  IZBORNA_LISTA_LOADED:     '✅ Izborna lista je učitana.',
  IZBORNA_LISTA_LOAD_FAIL:  '❌ Greška pri učitavanju izborne liste.',
  PRIJAVA_USPJESNA:         '✅ Ispit je uspješno prijavljen.',
  PRIJAVA_FAIL:             '❌ Greška pri prijavi ispita.',
  ODJAVA_USPJESNA:          '✅ Prijava ispita je otkazana.',
  ODJAVA_FAIL:              '❌ Greška pri odjavi ispita.',
  PRIJAVE_LOADED:           '✅ Prijave su učitane.',
  PRIJAVE_LOAD_FAIL:        '❌ Greška pri učitavanju prijava.',
}

// ─── Reaktivni toast sistem ──────────────────────────────────────────────────
const toasts = ref([])
let nextId = 1

export function useToast() {
  function show(message, type = 'info', duration = 3800) {
    const id = nextId++
    toasts.value.push({ id, message, type })
    setTimeout(() => {
      toasts.value = toasts.value.filter(t => t.id !== id)
    }, duration)
  }

  return {
    toasts,
    success: (msg) => show(msg, 'success'),
    error:   (msg) => show(msg, 'error'),
    warn:    (msg) => show(msg, 'warning'),
    info:    (msg) => show(msg, 'info'),
  }
}
