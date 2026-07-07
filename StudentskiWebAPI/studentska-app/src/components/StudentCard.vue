<template>
  <div
    :class="['student-card', { 'student-card--active': active }]"
    @click="$emit('select', student)"
  >
    <div class="student-card__avatar">{{ initials }}</div>
    <div class="student-card__info">
      <div class="student-card__name">{{ student.ime }} {{ student.prezime }}</div>
      <!-- Format: smer-broj/godinaUpisa  npr. SW-12/2021 -->
      <div class="student-card__index">{{ student.smer }}-{{ student.broj }}/{{ student.godinaUpisa }}</div>
    </div>
    <div v-if="active" class="student-card__badge">▶</div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  student: { type: Object, required: true },
  active:  { type: Boolean, default: false },
})
defineEmits(['select'])

const initials = computed(() =>
  `${props.student.ime?.[0] ?? ''}${props.student.prezime?.[0] ?? ''}`.toUpperCase()
)
</script>

<style scoped>
.student-card {
  display: flex; align-items: center; gap: 12px;
  padding: 12px 16px; border-radius: 10px; cursor: pointer;
  transition: all 0.18s; border: 2px solid transparent;
}
.student-card:hover { background: #f1f5f9; }
.student-card--active { background: #eff6ff; border-color: #3b82f6; }
.student-card__avatar {
  width: 40px; height: 40px; border-radius: 50%;
  background: linear-gradient(135deg, #3b82f6, #8b5cf6);
  color: #fff; display: flex; align-items: center; justify-content: center;
  font-weight: 700; font-size: 0.85rem; flex-shrink: 0;
}
.student-card__info { flex: 1; min-width: 0; }
.student-card__name  { font-weight: 600; font-size: 0.95rem; color: #1e293b; }
.student-card__index { font-size: 0.8rem; color: #64748b; margin-top: 2px; }
.student-card__badge { font-size: 0.8rem; color: #3b82f6; }
</style>
