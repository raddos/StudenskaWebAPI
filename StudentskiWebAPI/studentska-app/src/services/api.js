/**
 * ═══════════════════════════════════════════════════════════════════
 *  Studentski Web API – servis za sve HTTP pozive
 *  Vite proxy: /api  →  https://localhost:59499/api
 *
 *  Endpointi (iz našeg C# Web API-ja):
 *   GET    /api/Students                          – lista svih studenata
 *   GET    /api/Students/{id}                     – jedan student
 *   GET    /api/Students/pretraga?q=&smer=&...    – pretraga
 *   PUT    /api/Students/{id}/licni-podaci         – izmjena ime/prezime
 *   GET    /api/Students/{id}/polozeni-predmeti    – položeni + prosek
 *
 *   GET    /api/StudentPredmet/{idStudenta}             – izborna lista
 *   GET    /api/StudentPredmet/{idStudenta}/za-polaganje – za prijavu
 *   POST   /api/StudentPredmet                          – dodaj predmet
 *   DELETE /api/StudentPredmet/{idStudentPredmet}       – ukloni predmet
 *
 *   POST   /api/Prijava                           – prijavi ispit
 *   GET    /api/Prijava/ispit/{idIspita}          – prijave za ispit
 *   GET    /api/Prijava/student/{idStudenta}       – prijave studenta
 *   DELETE /api/Prijava/{idPrijave}               – odjavi ispit
 *
 *   GET    /api/Predmets                          – lista predmeta
 *   GET    /api/IspitniRoks                       – lista rokova
 *   GET    /api/Ispits                            – lista ispita
 * ═══════════════════════════════════════════════════════════════════
 */

const BASE = '/api'

async function request(method, path, body = null) {
  const opts = {
    method,
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
  }
  if (body !== null) opts.body = JSON.stringify(body)

  const res = await fetch(`${BASE}${path}`, opts)

  if (!res.ok) {
    let msg = `HTTP ${res.status}`
    try {
      const err = await res.json()
      msg = err.poruka || err.message || err.title || JSON.stringify(err)
    } catch {}
    throw new Error(msg)
  }

  if (res.status === 204) return null
  return res.json()
}

// ── Students ──────────────────────────────────────────────────────────────────
export const StudentAPI = {
  getAll:        ()              => request('GET',  '/Students'),
  getById:       (id)            => request('GET',  `/Students/${id}`),
  update:        (id, dto)       => request('PUT',  `/Students/${id}/licni-podaci`, dto),
  pretraga:      (params)        => request('GET',  `/Students/pretraga?${new URLSearchParams(params)}`),
  polozeniPredmeti: (id)         => request('GET',  `/Students/${id}/polozeni-predmeti`),
}

// ── StudentPredmet ────────────────────────────────────────────────────────────
export const StudentPredmetAPI = {
  getIzbornaLista: (idStudenta)       => request('GET',  `/StudentPredmet/${idStudenta}`),
  getZaPolaganje:  (idStudenta)       => request('GET',  `/StudentPredmet/${idStudenta}/za-polaganje`),
  dodaj:           (dto)              => request('POST', '/StudentPredmet', dto),
  obrisi:          (idStudentPredmet) => request('DELETE', `/StudentPredmet/${idStudentPredmet}`),
}

// ── Prijava ───────────────────────────────────────────────────────────────────
export const PrijavaAPI = {
  prijavi:          (dto)          => request('POST',   '/Prijava', dto),
  getZaIspit:       (idIspita)     => request('GET',    `/Prijava/ispit/${idIspita}`),
  getZaStudenta:    (idStudenta)   => request('GET',    `/Prijava/student/${idStudenta}`),
  odjavi:           (idPrijave)    => request('DELETE', `/Prijava/${idPrijave}`),
}

// ── Lookup šifarnici ──────────────────────────────────────────────────────────
export const PredmetAPI    = { getAll: () => request('GET', '/Predmets') }
export const IspitniRokAPI = { getAll: () => request('GET', '/IspitniRoks') }
export const IspitAPI      = { getAll: () => request('GET', '/Ispits') }
export const ZapisnikAPI = {
  getAll:  () => request('GET', '/Zapisniks'),
  getById: (id) => request('GET', `/Zapisniks/${id}`),
}