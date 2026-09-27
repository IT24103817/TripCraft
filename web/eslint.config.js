import js from '@eslint/js';
import prettier from 'eslint-config-prettier';
import importPlugin from 'eslint-plugin-import';
import jsxA11y from 'eslint-plugin-jsx-a11y';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import globals from 'globals';
import tseslint from 'typescript-eslint';

const features = ['trips', 'resources', 'quotations', 'landing'];

export default tseslint.config(
  // .vite is Vite's dependency cache (created by the dev server); it is generated code, like dist.
  { ignores: ['dist', 'coverage', '.vite'] },
  {
    extends: [
      js.configs.recommended,
      ...tseslint.configs.recommended,
      jsxA11y.flatConfigs.recommended,
      prettier,
    ],
    files: ['**/*.{ts,tsx}'],
    languageOptions: { ecmaVersion: 2022, globals: globals.browser },
    plugins: { 'react-hooks': reactHooks, 'react-refresh': reactRefresh, import: importPlugin },
    settings: { 'import/resolver': { typescript: { project: './tsconfig.app.json' } } },
    rules: {
      ...reactHooks.configs.recommended.rules,
      'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],
      // Strict feature folders: a feature may import from shared (and auth), never from another feature.
      // Shared and auth never import from features.
      'import/no-restricted-paths': [
        'error',
        {
          zones: [
            ...features.map((feature) => ({
              target: `./src/features/${feature}`,
              from: './src/features',
              except: [`./${feature}`],
              message: 'A feature may not import another feature. Move shared code to src/shared.',
            })),
            {
              target: './src/shared',
              from: ['./src/features', './src/app', './src/auth'],
              message: 'shared must not depend on app, auth or features.',
            },
            {
              target: './src/auth',
              from: ['./src/features', './src/app'],
              message: 'auth must not depend on app or features.',
            },
          ],
        },
      ],
    },
  },
  {
    // Test helpers are never hot-reloaded, so the Fast Refresh export rule does not apply.
    files: ['src/test/**/*.{ts,tsx}', 'src/**/__tests__/**/*.{ts,tsx}'],
    rules: { 'react-refresh/only-export-components': 'off' },
  },
);
