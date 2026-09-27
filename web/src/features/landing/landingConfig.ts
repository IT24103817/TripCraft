/** Build-time settings for the public landing page (set in Vercel; see web/.env.example). */
export const APK_URL = import.meta.env.VITE_APK_URL?.trim() || '';

/** "Group SE3090_G07 · " when VITE_GROUP_NUMBER is set, otherwise empty (the footer still names the module). */
export const GROUP_LABEL = import.meta.env.VITE_GROUP_NUMBER?.trim()
  ? `Group SE3090_G${import.meta.env.VITE_GROUP_NUMBER.trim()} · `
  : '';
