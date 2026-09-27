/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL: string;
  /** GitHub Release URL of the Android APK (landing page "Download APK"). */
  readonly VITE_APK_URL?: string;
  /** SE3090 group number shown in the landing page footer, e.g. "07". */
  readonly VITE_GROUP_NUMBER?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
