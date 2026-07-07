<template>
  <div class="edit-form">
    <h3 class="edit-form__title">✏️ Izmjena ličnih podataka</h3>

    <div class="form-group">
      <label>Ime <span class="req">*</span></label>
      <input
        v-model.trim="form.ime"
        :class="['form-control', { 'form-control--error': errors.ime }]"
        type="text" placeholder="Unesite ime" maxlength="50"
      />
      <span v-if="errors.ime" class="form-error">{{ errors.ime }}</span>
    </div>

    <div class="form-group">
      <label>Prezime <span class="req">*</span></label>
      <input
        v-model.trim="form.prezime"
        :class="['form-control', { 'form-control--error': errors.prezime }]"
        type="text" placeholder="Unesite prezime" maxlength="80"
      />
      <span v-if="errors.prezime" class="form-error">{{ errors.prezime }}</span>
    </div>

    <div class="edit-form__actions">
      <AppButton variant="put" :loading="saving" @click="submit">💾 Sačuvaj izmjene</AppButton>
      <AppButton variant="cancel" @click="$emit('cancel')">Otkaži</AppButton>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import AppButton from './AppButton.vue'
import { StudentAPI } from '../services/api.js'
import { useToast, MESSAGES } from '../composables/useToast.js'

const props = defineProps({ student: { type: Object, required: true } })
const emit  = defineEmits(['saved', 'cancel'])
const { success, error, warn } = useToast()

const form   = ref({ ime: '', prezime: '' })
const errors = ref({})
const saving = ref(false)

watch(() => props.student, s => {
  form.value   = { ime: s.ime ?? '', prezime: s.prezime ?? '' }
  errors.value = {}
}, { immediate: true })

const nameRegex = /^[A-Za-zÀ-žА-Яа-яĐđŠšČčĆćŽž\s\-']+$/

function validate() {
  errors.value = {}
  if (!form.value.ime)                          errors.value.ime = 'Ime je obavezno.'
  else if (!nameRegex.test(form.value.ime))     errors.value.ime = 'Nedozvoljeni znakovi.'
  if (!form.value.prezime)                      errors.value.prezime = 'Prezime je obavezno.'
  else if (!nameRegex.test(form.value.prezime)) errors.value.prezime = 'Nedozvoljeni znakovi.'
  return !Object.keys(errors.value).length
}

async function submit() {
  if (!validate()) { warn(MESSAGES.VALIDATION_NAME); return }
  saving.value = true
  try {
    // PUT /api/Students/{id}/licni-podaci  → šalje { ime, prezime }
    const updated = await StudentAPI.update(props.student.idStudenta, {
      ime:     form.value.ime,
      prezime: form.value.prezime,
    })
    success(MESSAGES.STUDENT_UPDATED)
    emit('saved', updated)
  } catch (e) {
    error(`${MESSAGES.STUDENT_UPDATE_FAIL}: ${e.message}`)
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.edit-form { background:#fff; border-radius:14px; padding:24px; box-shadow:0 2px 12px rgba(0,0,0,.08); }
.edit-form__title { margin:0 0 20px; font-size:1rem; color:#1e293b; font-weight:700; }
.form-group { margin-bottom:16px; }
.form-group label { display:block; font-size:.85rem; font-weight:600; margin-bottom:6px; color:#374151; }
.req { color:#ef4444; }
.form-control { width:100%; padding:9px 14px; border:1.5px solid #d1d5db; border-radius:8px; font-size:.9rem; outline:none; box-sizing:border-box; transition:border-color .2s; }
.form-control:focus { border-color:#3b82f6; }
.form-control--error { border-color:#ef4444; }
.form-error { font-size:.78rem; color:#ef4444; margin-top:4px; display:block; }
.edit-form__actions { display:flex; gap:10px; margin-top:20px; }
</style>
