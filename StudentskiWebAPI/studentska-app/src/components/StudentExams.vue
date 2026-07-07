<template>
  <div class="exams-panel">
    <div v-if="loading" class="loading">⏳ Učitavam podatke studenta...</div>

    <template v-else>
      <!-- Stats -->
      <div class="stats-row">
        <div class="stat-box stat-box--blue">
          <div class="stat-box__value">{{ prosek.brojPolozenih }}</div>
          <div class="stat-box__label">Položenih ispita</div>
        </div>
        <div class="stat-box stat-box--green">
          <div class="stat-box__value">{{ prosek.prosekOcena ?? '—' }}</div>
          <div class="stat-box__label">Prosječna ocjena</div>
        </div>
      </div>

      <!-- Položeni ispiti (iz /polozeni-predmeti) -->
      <div class="section-title">📋 Položeni ispiti</div>
      <div v-if="!prosek.predmeti?.length" class="empty">Student nije položio nijedan ispit.</div>
      <table v-else class="data-table">
        <thead>
          <tr><th>Predmet</th><th>ESPB</th><th>Ocjena</th><th>Rok</th><th>Školska god.</th></tr>
        </thead>
        <tbody>
          <tr v-for="p in prosek.predmeti" :key="p.nazivPredmeta + p.nazivRoka">
            <td>{{ p.nazivPredmeta }}</td>
            <td>{{ p.espb }}</td>
            <td><span :class="['badge', ocjenaBadge(p.ocena)]">{{ p.ocena }}</span></td>
            <td>{{ p.nazivRoka }}</td>
            <td>{{ p.skolskaGod ?? '—' }}</td>
          </tr>
        </tbody>
      </table>

      <!-- Uspješnost po predmetu (računamo iz izborne liste + položenih) -->
      <div class="section-title" style="margin-top:24px">📊 Uspješnost polaganja</div>
      <div v-if="!uspjesnostList.length" class="empty">Nema podataka o uspješnosti.</div>
      <table v-else class="data-table">
        <thead>
          <tr><th>Predmet</th><th>Ocjena</th><th>Pokušaji</th><th>Uspješnost</th></tr>
        </thead>
        <tbody>
          <tr v-for="u in uspjesnostList" :key="u.idPredmeta">
            <td>{{ u.naziv }}</td>
            <td><span :class="['badge', ocjenaBadge(u.ocjena)]">{{ u.ocjena }}</span></td>
            <td>{{ u.ukupno }}</td>
            <td>
              <div class="progress-wrap">
                <div class="progress-bar" :style="{ width: u.uspjesnost+'%', background: progressColor(u.uspjesnost) }"></div>
                <span class="progress-label">{{ u.uspjesnost }}%</span>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { StudentAPI, ZapisnikAPI } from '../services/api.js'
import { useToast, MESSAGES } from '../composables/useToast.js'

const props = defineProps({
  student:    { type: Object, required: true },
  predmeti:   { type: Array, default: () => [] },
  ispitniRokovi: { type: Array, default: () => [] },
  ispiti:     { type: Array, default: () => [] },
})

const { error, success } = useToast()
const loading         = ref(false)
const prosek          = ref({ predmeti: [], prosekOcena: 0, brojPolozenih: 0 })
const sviZapisnici    = ref([])   // svi zapisnici za uspješnost

watch(() => props.student, loadData, { immediate: true })

async function loadData() {
  loading.value = true
  prosek.value  = { predmeti: [], prosekOcena: 0, brojPolozenih: 0 }
  sviZapisnici.value = []
  try {
    const id = props.student.idStudenta

    // GET /api/Students/{id}/polozeni-predmeti  →  prosek + lista
    const podaci = await StudentAPI.polozeniPredmeti(id)
    prosek.value = podaci

    // Učitaj sve zapisnike za računanje uspješnosti (filter po studentu)
    // Koristimo /api/Zapisniks (ako postoji) – inače računamo samo iz položenih
    try {
      const svi = await fetch('/api/Zapisniks').then(r => r.ok ? r.json() : [])
      sviZapisnici.value = Array.isArray(svi) ? svi.filter(z => z.idStudenta === id) : []
    } catch { /* Zapisnik endpoint opcioni */ }

    success(MESSAGES.ISPITI_LOADED)
  } catch (e) {
    error(`${MESSAGES.ISPITI_LOAD_FAIL}: ${e.message}`)
  } finally {
    loading.value = false
  }
}

// Uspješnost: grupišemo zapisnike po predmetu
const uspjesnostList = computed(() => {
  // Ako nemamo Zapisnik endpoint, pravimo listu samo od položenih (100% za sve)
  if (!sviZapisnici.value.length && prosek.value.predmeti?.length) {
    return prosek.value.predmeti.map(p => ({
      idPredmeta: p.nazivPredmeta,
      naziv:      p.nazivPredmeta,
      ocjena:     p.ocena,
      ukupno:     1,
      uspjesnost: 100,
    }))
  }

  const map = {}
  for (const z of sviZapisnici.value) {
    const ispit   = props.ispiti.find(i => i.idIspita === z.idIspita)
    const pid     = ispit?.idPredmeta ?? z.idIspita
    const predmet = props.predmeti.find(p => p.idPredmeta === pid)
    const naziv   = predmet?.naziv ?? `Predmet #${pid}`

    if (!map[pid]) map[pid] = { naziv, ukupno: 0, nepolozenih: 0, ocjena: null }
    map[pid].ukupno++
    if ((z.ocena ?? 0) >= 6) map[pid].ocjena = z.ocena
    else map[pid].nepolozenih++
  }

  return Object.entries(map)
    .filter(([, v]) => v.ocjena !== null)
    .map(([pid, v]) => ({
      idPredmeta:  pid,
      naziv:       v.naziv,
      ocjena:      v.ocjena,
      ukupno:      v.ukupno,
      nepolozenih: v.nepolozenih,
      uspjesnost:  v.nepolozenih === 0 ? 100 : Math.round(100 / v.nepolozenih),
    }))
    .sort((a, b) => b.uspjesnost - a.uspjesnost)
})

function ocjenaBadge(o) {
  if (o >= 9) return 'badge--green'
  if (o >= 7) return 'badge--blue'
  if (o >= 6) return 'badge--yellow'
  return 'badge--red'
}
function progressColor(p) {
  if (p === 100) return '#16a34a'
  if (p >= 50)   return '#f59e0b'
  return '#ef4444'
}
</script>

<style scoped>
.loading { color:#64748b; padding:20px 0; }
.stats-row { display:flex; gap:14px; margin-bottom:22px; flex-wrap:wrap; }
.stat-box { flex:1; min-width:120px; padding:16px; border-radius:12px; text-align:center; box-shadow:0 2px 8px rgba(0,0,0,.06); }
.stat-box--blue  { background:#eff6ff; }
.stat-box--green { background:#f0fdf4; }
.stat-box__value { font-size:1.6rem; font-weight:700; color:#1e293b; }
.stat-box__label { font-size:.78rem; color:#64748b; margin-top:4px; }
.section-title { font-weight:700; font-size:.9rem; color:#374151; margin-bottom:10px; }
.empty { color:#9ca3af; font-size:.88rem; padding:12px 0; }
.data-table { width:100%; border-collapse:collapse; font-size:.875rem; }
.data-table th { background:#f8fafc; padding:10px 12px; text-align:left; font-weight:600; color:#64748b; font-size:.78rem; text-transform:uppercase; letter-spacing:.4px; border-bottom:1px solid #e2e8f0; }
.data-table td { padding:10px 12px; border-bottom:1px solid #f1f5f9; color:#374151; }
.data-table tr:last-child td { border-bottom:none; }
.badge { padding:3px 10px; border-radius:20px; font-size:.8rem; font-weight:600; }
.badge--green  { background:#dcfce7; color:#166534; }
.badge--blue   { background:#dbeafe; color:#1e40af; }
.badge--yellow { background:#fef9c3; color:#713f12; }
.badge--red    { background:#fee2e2; color:#991b1b; }
.progress-wrap { position:relative; background:#e5e7eb; border-radius:20px; height:22px; min-width:120px; overflow:hidden; }
.progress-bar  { height:100%; border-radius:20px; transition:width .4s; min-width:4px; }
.progress-label { position:absolute; inset:0; display:flex; align-items:center; justify-content:center; font-size:.75rem; font-weight:700; color:#1e293b; }
</style>
