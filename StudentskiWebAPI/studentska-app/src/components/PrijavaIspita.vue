<template>
  <div class="prijava-panel">

    <!-- ── Prijavi ispit ──────────────────────────────────────────────────── -->
    <div class="section-title">📝 Prijava ispita</div>
    <div v-if="loadingZaPolaganje" class="loading">⏳ Učitavam dostupne predmete...</div>
    <template v-else>
      <div v-if="!predmetiZaPolaganje.length" class="empty">
        Nema predmeta dostupnih za prijavu (svi su položeni ili nema odabranih predmeta).
      </div>
      <div v-else class="prijava-form">
        <div class="form-row">
          <div class="form-group">
            <label>Predmet (nepoloženi odabrani)</label>
            <select v-model="odabraniPredmetId" class="form-select" @change="onPredmetChange">
              <option value="">— Odaberi predmet —</option>
              <option v-for="sp in predmetiZaPolaganje" :key="sp.idPredmeta" :value="sp.idPredmeta">
                {{ sp.nazivPredmeta }}
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Ispit (predmet + rok)</label>
            <select v-model="odabraniIspitId" class="form-select" :disabled="!odabraniPredmetId">
              <option value="">— Odaberi rok —</option>
              <option v-for="i in ispitiFiltrirani" :key="i.idIspita" :value="i.idIspita">
                {{ i.nazivRoka }} {{ i.skolskaGod ? `(${i.skolskaGod})` : '' }}
                {{ i.datum ? ' — ' + formatDate(i.datum) : '' }}
              </option>
            </select>
          </div>
        </div>
        <AppButton
          variant="post"
          :loading="prijavljivanje"
          :disabled="!odabraniIspitId"
          @click="prijaviIspit"
        >
          ✅ Prijavi ispit
        </AppButton>
      </div>
    </template>

    <!-- ── Prijave studenta ───────────────────────────────────────────────── -->
    <div class="section-title" style="margin-top:28px">
      📋 Moje prijave
      <AppButton variant="get" style="margin-left:12px" @click="ucitajPrijave">🔄</AppButton>
    </div>

    <div v-if="loadingPrijave" class="loading">⏳ Učitavam prijave...</div>
    <div v-else-if="!prijave.length" class="empty">Nema prijavljenih ispita.</div>
    <table v-else class="data-table">
      <thead>
        <tr><th>Predmet</th><th>Rok</th><th>Školska god.</th><th>Datum prijave</th><th>Status</th><th></th></tr>
      </thead>
      <tbody>
        <tr v-for="p in prijave" :key="p.idPrijave">
          <td>{{ p.nazivPredmeta }}</td>
          <td>{{ p.nazivRoka }}</td>
          <td>{{ p.skolskaGod ?? '—' }}</td>
          <td>{{ formatDate(p.datumPrijave) }}</td>
          <td>
            <span :class="['badge', statusBadge(p.statusPrijave)]">
              {{ p.statusNaziv }}
            </span>
          </td>
          <td>
            <AppButton
              v-if="p.statusPrijave === 0"
              variant="delete"
              title="Odjavi ispit"
              @click="odjaviIspit(p)"
            >
              ✖ Odjavi
            </AppButton>
          </td>
        </tr>
      </tbody>
    </table>

  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import AppButton from './AppButton.vue'
import { StudentPredmetAPI, PrijavaAPI } from '../services/api.js'
import { useToast, MESSAGES } from '../composables/useToast.js'

const props = defineProps({
  student: { type: Object, required: true },
  ispiti:  { type: Array, default: () => [] },   // svi ispiti (lookup)
})

const { success, error } = useToast()

const predmetiZaPolaganje = ref([])
const loadingZaPolaganje  = ref(false)
const prijave             = ref([])
const loadingPrijave      = ref(false)
const prijavljivanje      = ref(false)

const odabraniPredmetId   = ref('')
const odabraniIspitId     = ref('')

watch(() => props.student, () => {
  odabraniPredmetId.value = ''
  odabraniIspitId.value   = ''
  ucitajZaPolaganje()
  ucitajPrijave()
}, { immediate: true })

// Ispiti filtrirani po odabranom predmetu
const ispitiFiltrirani = computed(() => {
  if (!odabraniPredmetId.value) return []
  return props.ispiti.filter(i => i.idPredmeta === odabraniPredmetId.value)
})

function onPredmetChange() {
  odabraniIspitId.value = ''
}

async function ucitajZaPolaganje() {
  loadingZaPolaganje.value = true
  try {
    const data = await StudentPredmetAPI.getZaPolaganje(props.student.idStudenta)
    predmetiZaPolaganje.value = Array.isArray(data) ? data : []
  } catch (e) {
    error(`Greška pri učitavanju predmeta za polaganje: ${e.message}`)
  } finally {
    loadingZaPolaganje.value = false
  }
}

async function ucitajPrijave() {
  loadingPrijave.value = true
  try {
    const data = await PrijavaAPI.getZaStudenta(props.student.idStudenta)
    prijave.value = Array.isArray(data) ? data : []
    success(MESSAGES.PRIJAVE_LOADED)
  } catch (e) {
    error(`${MESSAGES.PRIJAVE_LOAD_FAIL}: ${e.message}`)
  } finally {
    loadingPrijave.value = false
  }
}

async function prijaviIspit() {
  if (!odabraniIspitId.value) return
  prijavljivanje.value = true
  try {
    await PrijavaAPI.prijavi({
      idStudenta: props.student.idStudenta,
      idIspita:   odabraniIspitId.value,
    })
    success(MESSAGES.PRIJAVA_USPJESNA)
    odabraniPredmetId.value = ''
    odabraniIspitId.value   = ''
    await ucitajZaPolaganje()
    await ucitajPrijave()
  } catch (e) {
    error(`${MESSAGES.PRIJAVA_FAIL}: ${e.message}`)
  } finally {
    prijavljivanje.value = false
  }
}

async function odjaviIspit(p) {
  if (!confirm(`Otkazati prijavu za "${p.nazivPredmeta}" — ${p.nazivRoka}?`)) return
  try {
    await PrijavaAPI.odjavi(p.idPrijave)
    success(MESSAGES.ODJAVA_USPJESNA)
    await ucitajZaPolaganje()
    await ucitajPrijave()
  } catch (e) {
    error(`${MESSAGES.ODJAVA_FAIL}: ${e.message}`)
  }
}

function formatDate(d) {
  if (!d) return '—'
  return new Date(d).toLocaleDateString('sr-Latn-RS')
}

function statusBadge(s) {
  if (s === 0) return 'badge--blue'
  if (s === 1) return 'badge--green'
  return 'badge--red'
}
</script>

<style scoped>
.section-title { font-weight:700; font-size:.9rem; color:#374151; margin-bottom:12px; display:flex; align-items:center; }
.loading { color:#64748b; font-size:.88rem; padding:10px 0; }
.empty   { color:#9ca3af; font-size:.88rem; padding:12px 0; }
.prijava-form { background:#f8fafc; border-radius:12px; padding:16px; margin-bottom:4px; }
.form-row { display:flex; gap:16px; flex-wrap:wrap; margin-bottom:14px; }
.form-group { flex:1; min-width:200px; }
.form-group label { display:block; font-size:.82rem; font-weight:600; color:#374151; margin-bottom:6px; }
.form-select { width:100%; padding:9px 12px; border:1.5px solid #d1d5db; border-radius:8px; font-size:.88rem; outline:none; background:#fff; }
.form-select:focus { border-color:#22c55e; }
.form-select:disabled { background:#f1f5f9; cursor:not-allowed; }
.data-table { width:100%; border-collapse:collapse; font-size:.875rem; }
.data-table th { background:#f8fafc; padding:10px 12px; text-align:left; font-weight:600; color:#64748b; font-size:.78rem; text-transform:uppercase; letter-spacing:.4px; border-bottom:1px solid #e2e8f0; }
.data-table td { padding:10px 12px; border-bottom:1px solid #f1f5f9; color:#374151; vertical-align:middle; }
.data-table tr:last-child td { border-bottom:none; }
.badge { padding:3px 10px; border-radius:20px; font-size:.8rem; font-weight:600; }
.badge--blue   { background:#dbeafe; color:#1e40af; }
.badge--green  { background:#dcfce7; color:#166534; }
.badge--red    { background:#fee2e2; color:#991b1b; }
</style>
