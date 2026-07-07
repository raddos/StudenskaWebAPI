<template>
  <div class="izborna-panel">
    <!-- ── Dodaj predmet ─────────────────────────────────────────────────── -->
    <div class="add-row">
      <select v-model="odabraniPredmet" class="form-select">
        <option value="">— Odaberi predmet za dodavanje —</option>
        <option
          v-for="p in dostupniZaDodavanje"
          :key="p.idPredmeta"
          :value="p.idPredmeta"
        >
          {{ p.naziv }} ({{ p.espb }} ESPB)
        </option>
      </select>
      <AppButton variant="post" :loading="adding" :disabled="!odabraniPredmet" @click="dodajPredmet">
        ➕ Dodaj
      </AppButton>
    </div>

    <!-- ── Lista odabranih predmeta ──────────────────────────────────────── -->
    <div v-if="loadingLista" class="loading">⏳ Učitavam izbornu listu...</div>
    <div v-else-if="!izbornaLista.length" class="empty">
      Student još nema odabranih predmeta.
    </div>
    <table v-else class="data-table">
      <thead>
        <tr><th>Predmet</th><th>ESPB</th><th>Školska god.</th><th>Status</th><th></th></tr>
      </thead>
      <tbody>
        <tr v-for="sp in izbornaLista" :key="sp.idStudentPredmet">
          <td>{{ sp.nazivPredmeta }}</td>
          <td>{{ sp.espb }}</td>
          <td>{{ sp.skolskaGodina ?? '—' }}</td>
          <td>
            <span :class="['badge', sp.jePolozio ? 'badge--green' : 'badge--yellow']">
              {{ sp.jePolozio ? '✅ Položen' : '⏳ Nije položen' }}
            </span>
          </td>
          <td>
            <AppButton
              variant="delete"
              :disabled="sp.jePolozio"
              :title="sp.jePolozio ? 'Nije moguće ukloniti – predmet je već položen' : 'Ukloni predmet'"
              @click="obrisiPredmet(sp)"
            >
              🗑
            </AppButton>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import AppButton from './AppButton.vue'
import { StudentPredmetAPI } from '../services/api.js'
import { useToast, MESSAGES } from '../composables/useToast.js'

const props = defineProps({
  student:  { type: Object, required: true },
  predmeti: { type: Array, default: () => [] },   // svi predmeti (šifarnik)
})
const emit = defineEmits(['updated'])

const { success, error } = useToast()

const izbornaLista    = ref([])
const loadingLista    = ref(false)
const adding          = ref(false)
const odabraniPredmet = ref('')

watch(() => props.student, ucitajListu, { immediate: true })

async function ucitajListu() {
  loadingLista.value = true
  try {
    const data = await StudentPredmetAPI.getIzbornaLista(props.student.idStudenta)
    izbornaLista.value = Array.isArray(data) ? data : []
    success(MESSAGES.IZBORNA_LISTA_LOADED)
  } catch (e) {
    error(`${MESSAGES.IZBORNA_LISTA_LOAD_FAIL}: ${e.message}`)
  } finally {
    loadingLista.value = false
  }
}

// Predmeti koji NISU već u izbornoj listi
const odabraniIds = computed(() => new Set(izbornaLista.value.map(sp => sp.idPredmeta)))
const dostupniZaDodavanje = computed(() =>
  props.predmeti.filter(p => !odabraniIds.value.has(p.idPredmeta))
)

async function dodajPredmet() {
  if (!odabraniPredmet.value) return
  adding.value = true
  try {
    await StudentPredmetAPI.dodaj({
      idStudenta:    props.student.idStudenta,
      idPredmeta:    odabraniPredmet.value,
      skolskaGodina: null,
    })
    odabraniPredmet.value = ''
    success(MESSAGES.PREDMET_DODAN)
    await ucitajListu()
    emit('updated')
  } catch (e) {
    error(`${MESSAGES.PREDMET_DODAN_FAIL}: ${e.message}`)
  } finally {
    adding.value = false
  }
}

async function obrisiPredmet(sp) {
  if (sp.jePolozio) return
  if (!confirm(`Ukloniti predmet "${sp.nazivPredmeta}" iz izborne liste?`)) return
  try {
    await StudentPredmetAPI.obrisi(sp.idStudentPredmet)
    success(MESSAGES.PREDMET_OBRISAN)
    await ucitajListu()
    emit('updated')
  } catch (e) {
    error(`${MESSAGES.PREDMET_OBRISAN_FAIL}: ${e.message}`)
  }
}
</script>

<style scoped>
.add-row { display:flex; gap:10px; margin-bottom:18px; align-items:center; flex-wrap:wrap; }
.form-select { flex:1; min-width:220px; padding:9px 12px; border:1.5px solid #d1d5db; border-radius:8px; font-size:.88rem; outline:none; background:#fff; cursor:pointer; }
.form-select:focus { border-color:#22c55e; }
.loading { color:#64748b; font-size:.88rem; padding:10px 0; }
.empty   { color:#9ca3af; font-size:.88rem; padding:12px 0; }
.data-table { width:100%; border-collapse:collapse; font-size:.875rem; }
.data-table th { background:#f8fafc; padding:10px 12px; text-align:left; font-weight:600; color:#64748b; font-size:.78rem; text-transform:uppercase; letter-spacing:.4px; border-bottom:1px solid #e2e8f0; }
.data-table td { padding:10px 12px; border-bottom:1px solid #f1f5f9; color:#374151; vertical-align:middle; }
.data-table tr:last-child td { border-bottom:none; }
.badge { padding:3px 10px; border-radius:20px; font-size:.8rem; font-weight:600; }
.badge--green  { background:#dcfce7; color:#166534; }
.badge--yellow { background:#fef9c3; color:#713f12; }
</style>
