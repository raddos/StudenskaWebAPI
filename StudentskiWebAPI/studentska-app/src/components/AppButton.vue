<template>
  <button
    :class="['app-btn', `app-btn--${variant}`, { 'app-btn--loading': loading }]"
    :disabled="disabled || loading"
    @click="$emit('click', $event)"
  >
    <span v-if="loading" class="app-btn__spinner"></span>
    <slot />
  </button>
</template>

<script setup>
defineProps({
  /**
   * Tip akcije određuje boju:
   *  get    → plava  (učitaj/prikaži)
   *  post   → zelena (dodaj)
   *  put    → narandžasta (izmijeni)
   *  delete → crvena (briši)
   *  cancel → siva
   */
  variant:  { type: String, default: 'get' },
  loading:  { type: Boolean, default: false },
  disabled: { type: Boolean, default: false },
})
defineEmits(['click'])
</script>

<style scoped>
.app-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 18px;
  border: none;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  letter-spacing: 0.3px;
}
.app-btn:disabled { opacity: 0.55; cursor: not-allowed; }

/* GET – plava */
.app-btn--get    { background: #3b82f6; color: #fff; }
.app-btn--get:hover:not(:disabled) { background: #2563eb; }

/* POST – zelena */
.app-btn--post   { background: #22c55e; color: #fff; }
.app-btn--post:hover:not(:disabled) { background: #16a34a; }

/* PUT – narandžasta */
.app-btn--put    { background: #f59e0b; color: #fff; }
.app-btn--put:hover:not(:disabled)  { background: #d97706; }

/* DELETE – crvena */
.app-btn--delete { background: #ef4444; color: #fff; }
.app-btn--delete:hover:not(:disabled) { background: #dc2626; }

/* CANCEL – siva */
.app-btn--cancel { background: #6b7280; color: #fff; }
.app-btn--cancel:hover:not(:disabled) { background: #4b5563; }

/* Spinner */
.app-btn__spinner {
  width: 14px; height: 14px;
  border: 2px solid rgba(255,255,255,0.4);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
</style>
