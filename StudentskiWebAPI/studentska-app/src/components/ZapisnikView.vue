<template>
  <div class="zapisnik-panel">
    <h3 class="section-title">📄 Pregled zapisnika ispita</h3>

    <div class="form-row">
      <label>Izaberi ispit (predmet + rok):</label>
      <select v-model="selectedIspitId" class="form-select">
        <option value="">— Odaberi ispit —</option>
        <option v-for="i in ispiti" :key="i.idIspita" :value="i.idIspita">
          {{ formatIspit(i) }}
        </option>
      </select>
      <AppButton variant="get" :loading="loadingDetails" :disabled="!selectedIspitId" @click="loadDetails">
        📥 Prikaži zapisnik
      </AppButton>
    </div>

    <div v-if="loadingList" class="loading">⏳ Učitavam listu ispita...</div>

    <!-- Detalj zapisnika za izabrani ispit -->
    <template v-if="selectedIspitId && detalji.length">
      <!-- Statistika ocjena -->
      <div class="subsection-title">📊 Statistika ocjena</div>
      <div class="grade-stats">
        <div v-for="(count, ocjena) in ocjeneStats" :key="ocjena" class="grade-bar-item">
          <div class="grade-label">{{ ocjena === '5' ? '5 (NP)' : `Ocjena ${ocjena}` }}</div>
          <div class="grade-bar-wrap">
            <div
              class="grade-bar"
              :style="{
                width: maxCount ? (count / maxCount * 100) + '%' : '0%',
                background: ocjena === '5' ? '#ef4444' : '#3b82f6'
              }"
            ></div>
            <span class="grade-count">{{ count }}</span>
          </div>
        </div>
      </div>

      <!-- Tabela studenti + ocene -->
      <div class="subsection-title" style="margin-top:20px">👥 Studenti u zapisniku ({{ detalji.length }})</div>
      <table class="data-table">
        <thead>
          <tr><th>Student</th><th>Ocjena</th><th>Bodovi</th></tr>
        </thead>
        <tbody>
          <tr v-for="z in detalji" :key="z._key">
            <td>{{ z._studentIme }}</td>
            <td><span :class="['badge', ocjenaBadge(z.ocena)]">{{ z.ocena }}</span></td>
            <td>{{ z.bodovi ?? '—' }}</td>
          </tr>
        </tbody>
      </table>
    </template>

    <div v-else-if="selectedIspitId && !loadingDetails" class="empty">
      Nema unosa u zapisniku za ovaj ispit.
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import AppButton from './AppButton.vue'
import { ZapisnikAPI } from '../services/api.js'
import { useToast, MESSAGES } from '../composables/useToast.js'

const props = defineProps({
  predmeti:      { type: Array, default: () => [] },
  ispitniRokovi: { type: Array, default: () => [] },
  studenti:      { type: Array, default: () => [] },
  ispiti:        { type: Array, default: () => [] },
})

const { error, success } = useToast()

const selectedIspitId = ref('')
const allZapisnici    = ref([])
const loadingList     = ref(false)
const loadingDetails  = ref(false)

// Učitaj sve zapisnike jednom
onMounted(async () => {
  loadingList.value = true
  try {
    const data = await ZapisnikAPI.getAll()
    allZapisnici.value = Array.isArray(data) ? data : []
  } catch (e) {
    error(`${MESSAGES.ZAPISNICI_LOAD_FAIL}: ${e.message}`)
  } finally {
    loadingList.value = false
  }
})

// Zapisnici za izabrani ispit
const detalji = computed(() => {
  if (!selectedIspitId.value) return []
  const id = Number(selectedIspitId.value)
  return allZapisnici.value
    .filter(z => z.idIspita === id)
    .map((z, i) => {
      const s = props.studenti.find(s => s.idStudenta === z.idStudenta)
      return {
        ...z,
        _key:       `${z.idStudenta}-${z.idIspita}-${i}`,
        _studentIme: s ? `${s.ime} ${s.prezime}` : `Student #${z.idStudenta}`,
      }
    })
})

// Fake "load" za toast poruku
async function loadDetails() {
  loadingDetails.value = true
  try {
    // Podaci su već u allZapisnici, samo simuliramo korak
    await new Promise(r => setTimeout(r, 200))
    success(MESSAGES.ZAPISNIK_LOADED)
  } finally {
    loadingDetails.value = false
  }
}

// Statistika ocjena (5-10)
const ocjeneStats = computed(() => {
  const stats = {}
  for (let i = 5; i <= 10; i++) stats[String(i)] = 0
  for (const z of detalji.value) {
    const k = String(z.ocena ?? 5)
    if (stats[k] !== undefined) stats[k]++
  }
  return stats
})
const maxCount = computed(() => Math.max(...Object.values(ocjeneStats.value), 1))

// Helpers
function formatIspit(i) {
  const p = props.predmeti.find(p => p.idPredmeta === i.idPredmeta)
  const r = props.ispitniRokovi.find(r => r.idRoka === i.idRoka)
  const predNaziv = p?.naziv ?? `Predmet #${i.idPredmeta}`
  const rokNaziv  = r?.naziv ?? `Rok #${i.idRoka}`
  const skolskaGod = r?.skolskaGod ?? ''
  return `${predNaziv} — ${rokNaziv}${skolskaGod ? ' (' + skolskaGod + ')' : ''}`
}

function ocjenaBadge(o) {
  if (o >= 9) return 'badge--green'
  if (o >= 7) return 'badge--blue'
  if (o >= 6) return 'badge--yellow'
  return 'badge--red'
}
</script>

<style scoped>
.section-title    { font-weight: 700; font-size: 0.95rem; color: #374151; margin: 0 0 16px; }
.subsection-title { font-weight: 700; font-size: 0.85rem; color: #374151; margin: 0 0 10px; }
.form-row { display: flex; align-items: center; gap: 10px; margin-bottom: 18px; flex-wrap: wrap; }
.form-row label { font-size: 0.85rem; font-weight: 600; color: #374151; white-space: nowrap; }
.form-select { flex: 1; min-width: 200px; padding: 9px 12px; border: 1.5px solid #d1d5db; border-radius: 8px; font-size: 0.88rem; outline: none; background: #fff; cursor: pointer; }
.form-select:focus { border-color: #3b82f6; }
.loading { color: #64748b; font-size: 0.88rem; padding: 10px 0; }
.empty   { color: #9ca3af; font-size: 0.88rem; padding: 12px 0; }

/* Grade stats */
.grade-stats { display: flex; flex-direction: column; gap: 8px; }
.grade-bar-item { display: flex; align-items: center; gap: 10px; }
.grade-label { width: 90px; font-size: 0.8rem; color: #374151; font-weight: 500; text-align: right; flex-shrink: 0; }
.grade-bar-wrap { flex: 1; background: #e5e7eb; border-radius: 20px; height: 22px; position: relative; overflow: hidden; }
.grade-bar { height: 100%; border-radius: 20px; transition: width 0.4s ease; min-width: 4px; }
.grade-count { position: absolute; right: 10px; top: 50%; transform: translateY(-50%); font-size: 0.8rem; font-weight: 700; color: #1e293b; }

/* Table */
.data-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
.data-table th { background: #f8fafc; padding: 10px 12px; text-align: left; font-weight: 600; color: #64748b; font-size: 0.78rem; text-transform: uppercase; letter-spacing: 0.4px; border-bottom: 1px solid #e2e8f0; }
.data-table td { padding: 10px 12px; border-bottom: 1px solid #f1f5f9; color: #374151; }
.data-table tr:last-child td { border-bottom: none; }
.badge { padding: 3px 10px; border-radius: 20px; font-size: 0.8rem; font-weight: 600; }
.badge--green  { background: #dcfce7; color: #166534; }
.badge--blue   { background: #dbeafe; color: #1e40af; }
.badge--yellow { background: #fef9c3; color: #713f12; }
.badge--red    { background: #fee2e2; color: #991b1b; }
</style>
