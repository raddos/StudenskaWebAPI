<template>
  <div class="app-layout">
    <!-- ═══════════════════════════════ SIDEBAR ═══════════════════════════ -->
    <aside class="sidebar">
      <div class="sidebar__header">
        <h1 class="sidebar__title">🎓 Studentska</h1>

        <!-- Pretraga – poziva GET /api/Students/pretraga -->
        <div class="search-wrap">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="🔍 Pretraži studente..."
            class="search-input"
            @input="onSearch"
          />
          <span v-if="searching" class="search-spinner">⏳</span>
        </div>

        <div v-if="searchQuery" class="search-meta">
          {{ filteredStudents.length }} rezultata
          <button class="clear-btn" @click="clearSearch">✕ Poništi</button>
        </div>
      </div>

      <div v-if="loadingStudents" class="sidebar__state">⏳ Učitavam studente...</div>
      <div v-else-if="!filteredStudents.length" class="sidebar__state">Nema studenata.</div>
      <div v-else class="sidebar__list">
        <StudentCard
          v-for="s in filteredStudents"
          :key="s.idStudenta"
          :student="s"
          :active="selectedStudent?.idStudenta === s.idStudenta"
          @select="selectStudent(s)"
        />
      </div>
    </aside>

    <!-- ═══════════════════════════ MAIN CONTENT ══════════════════════════ -->
    <main class="main-content">
      <!-- Placeholder -->
      <div v-if="!selectedStudent" class="placeholder">
        <div class="placeholder__icon">👈</div>
        <div class="placeholder__text">Izaberite studenta iz liste</div>
      </div>

      <template v-else>
        <!-- Student header -->
        <div class="student-header">
          <div>
            <div class="student-header__name">
              {{ selectedStudent.ime }} {{ selectedStudent.prezime }}
            </div>
            <div class="student-header__index">
              📇 {{ selectedStudent.brojIndeksa }}
            </div>
          </div>
          <AppButton variant="put" class="ml-auto" @click="showEdit = !showEdit">
            {{ showEdit ? '✖ Zatvori' : '✏️ Izmijeni podatke' }}
          </AppButton>
        </div>

        <!-- Edit form -->
        <Transition name="slide">
          <EditStudent
            v-if="showEdit"
            :student="selectedStudent"
            @saved="onStudentSaved"
            @cancel="showEdit = false"
          />
        </Transition>

        <!-- Tabs -->
        <div class="tabs">
          <button
            v-for="tab in tabs"
            :key="tab.id"
            :class="['tab-btn', { 'tab-btn--active': activeTab === tab.id }]"
            @click="activeTab = tab.id"
          >{{ tab.label }}</button>
        </div>

        <!-- Tab sadržaj -->
        <div class="tab-content">

          <!-- Ispiti & Uspješnost -->
          <StudentExams
            v-if="activeTab === 'ispiti'"
            :student="selectedStudent"
            :predmeti="predmeti"
            :ispitni-rokovi="ispitniRokovi"
            :ispiti="ispiti"
          />

          <!-- Izborna lista predmeta -->
          <IzbornaLista
            v-else-if="activeTab === 'izborna'"
            :student="selectedStudent"
            :predmeti="predmeti"
            @updated="" 
          />

          <!-- Prijava ispita -->
          <PrijavaIspita
            v-else-if="activeTab === 'prijava'"
            :student="selectedStudent"
            :ispiti="ispiti"
          />

          <!-- Zapisnici (pregled po ispitu) -->
          <ZapisnikView
            v-else-if="activeTab === 'zapisnici'"
            :predmeti="predmeti"
            :ispitni-rokovi="ispitniRokovi"
            :studenti="students"
            :ispiti="ispiti"
          />

        </div>
      </template>
    </main>

    <ToastContainer />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import StudentCard    from './components/StudentCard.vue'
import EditStudent    from './components/EditStudent.vue'
import StudentExams   from './components/StudentExams.vue'
import IzbornaLista   from './components/IzbornaLista.vue'
import PrijavaIspita  from './components/PrijavaIspita.vue'
import ZapisnikView   from './components/ZapisnikView.vue'
import AppButton      from './components/AppButton.vue'
import ToastContainer from './components/ToastContainer.vue'
import { StudentAPI, PredmetAPI, IspitniRokAPI, IspitAPI } from './services/api.js'
import { useToast, MESSAGES } from './composables/useToast.js'

const { success, error } = useToast()

// ─── State ────────────────────────────────────────────────────────────────────
const students         = ref([])
const predmeti         = ref([])
const ispitniRokovi    = ref([])
const ispiti           = ref([])
const loadingStudents  = ref(false)
const selectedStudent  = ref(null)
const showEdit         = ref(false)
const activeTab        = ref('ispiti')
const searchQuery      = ref('')
const searching        = ref(false)
const filteredStudents = ref([])

let searchTimeout = null

const tabs = [
  { id: 'ispiti',    label: '📚 Ispiti' },
  { id: 'izborna',   label: '📋 Izborna lista' },
  { id: 'prijava',   label: '📝 Prijava ispita' },
  { id: 'zapisnici', label: '📄 Zapisnici' },
]

// ─── Boot ─────────────────────────────────────────────────────────────────────
onMounted(async () => {
  loadingStudents.value = true
  try {
    const [s, p, r, i] = await Promise.allSettled([
      StudentAPI.getAll(),
      PredmetAPI.getAll(),
      IspitniRokAPI.getAll(),
      IspitAPI.getAll(),
    ])

    if (s.status === 'fulfilled') {
      students.value         = Array.isArray(s.value) ? s.value : []
      filteredStudents.value = students.value
      success(MESSAGES.STUDENTS_LOADED)
    } else {
      error(`${MESSAGES.STUDENT_LOAD_FAIL}: ${s.reason?.message}`)
    }

    if (p.status === 'fulfilled') predmeti.value     = Array.isArray(p.value) ? p.value : []
    if (r.status === 'fulfilled') ispitniRokovi.value = Array.isArray(r.value) ? r.value : []
    if (i.status === 'fulfilled') ispiti.value        = Array.isArray(i.value) ? i.value : []

  } finally {
    loadingStudents.value = false
  }
})

// ─── Pretraga (debounce 400ms → GET /api/Students/pretraga) ──────────────────
function onSearch() {
  clearTimeout(searchTimeout)
  if (!searchQuery.value.trim()) {
    filteredStudents.value = students.value
    return
  }
  searching.value = true
  searchTimeout = setTimeout(async () => {
    try {
      const res = await StudentAPI.pretraga({ q: searchQuery.value.trim() })
      filteredStudents.value = Array.isArray(res) ? res : []
    } catch {
      // Fallback: lokalna pretraga
      const q = searchQuery.value.toLowerCase()
      filteredStudents.value = students.value.filter(s =>
        `${s.ime} ${s.prezime} ${s.brojIndeksa}`.toLowerCase().includes(q)
      )
    } finally {
      searching.value = false
    }
  }, 400)
}

function clearSearch() {
  searchQuery.value      = ''
  filteredStudents.value = students.value
}

// ─── Akcije ───────────────────────────────────────────────────────────────────
function selectStudent(s) {
  selectedStudent.value = s
  showEdit.value        = false
  activeTab.value       = 'ispiti'
}

function onStudentSaved(updated) {
  // API vraća StudentListDto (idStudenta, ime, prezime, brojIndeksa)
  const idx = students.value.findIndex(s => s.idStudenta === updated.idStudenta)
  if (idx !== -1) students.value[idx] = updated
  selectedStudent.value = updated
  // Osvježi i u filtriranoj listi
  const fidx = filteredStudents.value.findIndex(s => s.idStudenta === updated.idStudenta)
  if (fidx !== -1) filteredStudents.value[fidx] = updated
  showEdit.value = false
}
</script>

<style>
*, *::before, *::after { box-sizing:border-box; margin:0; padding:0; }
body { font-family:'Inter',system-ui,sans-serif; background:#f0f4f8; color:#1e293b; min-height:100vh; }

.app-layout { display:flex; height:100vh; overflow:hidden; }

/* ── Sidebar ─────────────────────────────────────────────────────────────── */
.sidebar { width:300px; min-width:260px; background:#fff; border-right:1px solid #e2e8f0; display:flex; flex-direction:column; overflow:hidden; }
.sidebar__header { padding:18px 14px 12px; border-bottom:1px solid #f1f5f9; flex-shrink:0; }
.sidebar__title  { font-size:1.2rem; font-weight:800; color:#1e293b; margin-bottom:10px; }

.search-wrap  { position:relative; }
.search-input { width:100%; padding:8px 32px 8px 12px; border:1.5px solid #e2e8f0; border-radius:8px; font-size:.875rem; outline:none; background:#f8fafc; transition:border-color .2s; }
.search-input:focus { border-color:#3b82f6; background:#fff; }
.search-spinner { position:absolute; right:10px; top:50%; transform:translateY(-50%); font-size:.8rem; }

.search-meta { display:flex; align-items:center; justify-content:space-between; margin-top:6px; font-size:.78rem; color:#64748b; }
.clear-btn { background:none; border:none; color:#3b82f6; cursor:pointer; font-size:.78rem; font-weight:600; }
.clear-btn:hover { text-decoration:underline; }

.sidebar__list  { flex:1; overflow-y:auto; padding:8px; }
.sidebar__state { padding:20px; color:#9ca3af; font-size:.875rem; text-align:center; }

/* ── Main ─────────────────────────────────────────────────────────────────── */
.main-content { flex:1; overflow-y:auto; padding:28px 32px; display:flex; flex-direction:column; gap:18px; }

/* ── Placeholder ─────────────────────────────────────────────────────────── */
.placeholder { flex:1; display:flex; flex-direction:column; align-items:center; justify-content:center; gap:12px; color:#94a3b8; }
.placeholder__icon { font-size:3rem; }
.placeholder__text { font-size:1rem; font-weight:500; }

/* ── Student header ──────────────────────────────────────────────────────── */
.student-header { background:linear-gradient(135deg,#1e40af,#6d28d9); border-radius:16px; padding:22px 28px; color:#fff; display:flex; align-items:center; gap:16px; flex-wrap:wrap; }
.student-header__name  { font-size:1.35rem; font-weight:700; }
.student-header__index { font-size:.88rem; opacity:.8; margin-top:4px; }
.ml-auto { margin-left:auto; }

/* ── Tabs ─────────────────────────────────────────────────────────────────── */
.tabs { display:flex; gap:4px; background:#fff; border-radius:12px; padding:6px; box-shadow:0 1px 6px rgba(0,0,0,.06); flex-wrap:wrap; width:fit-content; }
.tab-btn { padding:8px 16px; border:none; background:transparent; border-radius:8px; font-size:.85rem; font-weight:500; cursor:pointer; color:#64748b; transition:all .2s; white-space:nowrap; }
.tab-btn:hover { background:#f1f5f9; color:#374151; }
.tab-btn--active { background:#3b82f6; color:#fff; font-weight:600; }

/* ── Tab content ─────────────────────────────────────────────────────────── */
.tab-content { background:#fff; border-radius:16px; padding:24px; box-shadow:0 2px 12px rgba(0,0,0,.06); }

/* ── Slide transition ────────────────────────────────────────────────────── */
.slide-enter-active,.slide-leave-active { transition:all .25s ease; }
.slide-enter-from,.slide-leave-to { opacity:0; transform:translateY(-10px); }

/* ── Scrollbar ───────────────────────────────────────────────────────────── */
::-webkit-scrollbar { width:6px; }
::-webkit-scrollbar-track { background:transparent; }
::-webkit-scrollbar-thumb { background:#cbd5e1; border-radius:3px; }
::-webkit-scrollbar-thumb:hover { background:#94a3b8; }
</style>
