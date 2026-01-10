# korid.client

This template should help get you started developing with Vue 3 in Vite.

## Recommended IDE Setup

[VS Code](https://code.visualstudio.com/) + [Vue (Official)](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Recommended Browser Setup

- Chromium-based browsers (Chrome, Edge, Brave, etc.):
  - [Vue.js devtools](https://chromewebstore.google.com/detail/vuejs-devtools/nhdogjmejiglipccpnnnanhbledajbpd) 
  - [Turn on Custom Object Formatter in Chrome DevTools](http://bit.ly/object-formatters)
- Firefox:
  - [Vue.js devtools](https://addons.mozilla.org/en-US/firefox/addon/vue-js-devtools/)
  - [Turn on Custom Object Formatter in Firefox DevTools](https://fxdx.dev/firefox-devtools-custom-object-formatters/)

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) to make the TypeScript language service aware of `.vue` types.

## Customize configuration

See [Vite Configuration Reference](https://vite.dev/config/).

## Project Setup

```sh
npm install
```

### Compile and Hot-Reload for Development

```sh
npm run dev
```

### Type-Check, Compile and Minify for Production

```sh
npm run build
```

### Lint with [ESLint](https://eslint.org/)

```sh
npm run lint
```

## HTTP client and proxy (local development)

This project includes a small HTTP client setup that uses Vite's dev-server proxy to forward requests starting with `/api` to the backend (see `vite.config.ts`). The frontend code uses `/api` as the base URL so requests are proxied and CORS is avoided during development.

Files added:
- `src/services/httpClient.ts` ? Axios client configured with baseURL `/api` and request/response interceptors.
- `src/services/fetchWrapper.ts` ? small fetch-based wrapper with the same `/api` prefix and token handling.
- `src/components/UseApiExample.vue` ? example component that calls `/users/me` using both clients.

How to manually test the proxy and client:

1. Start the backend (KorID.API) on the address configured in `vite.config.ts` environment variables (or set `services__koridapi__http__0=http://localhost:5000`).
2. Start the frontend dev server:

```sh
npm run dev
```

3. Open the app in the browser (default: http://localhost:5173).
4. In DevTools -> Application -> Local Storage, set a token under key `korid_token` (e.g. `localStorage.setItem('korid_token','<your_jwt>')`).
5. Open the page containing `UseApiExample` (it is added to `App.vue`), and check Network tab for requests to `/api/users/me`. Vite will proxy them to the backend.
6. If the backend returns 401, a `korid:unauthorized` event will be dispatched on window ? you can listen for it to perform logout or redirect.

Notes and next steps:
- For production builds, configure the real API URL in the built app (e.g. use an env var and set `baseURL` accordingly).
- Consider using HttpOnly cookies or a refresh-token flow for improved security.
