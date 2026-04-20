/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx}',
    './components/**/*.{js,ts,jsx,tsx}'
  ],
  theme: {
    extend: {
      colors: {
        ranch: {
          brown:  '#6B4226',
          green:  '#3A5A40',
          gold:   '#C9A84C',
          cream:  '#F5F0E8',
          dark:   '#1A1A1A'
        }
      },
      fontFamily: {
        serif: ['Georgia', 'serif'],
        sans:  ['Inter', 'sans-serif']
      }
    }
  },
  plugins: []
}
