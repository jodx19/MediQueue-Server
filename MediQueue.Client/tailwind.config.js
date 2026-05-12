/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
    "./src/**/*.component.{html,ts}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        'mq-navy': {
          DEFAULT: '#0F172A',
          800: '#1E293B',
          700: '#334155',
          600: '#475569',
        },
        'mq-teal': {
          DEFAULT: '#0D9488',
          400: '#2DD4BF',
          600: '#0F766E',
          900: '#042F2E',
        },
        'mq-slate': {
          DEFAULT: '#F8FAFC',
          200: '#E2E8F0',
          400: '#94A3B8',
          600: '#475569',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      animation: {
        'float': 'float 6s ease-in-out infinite',
        'ticker': 'ticker 25s linear infinite',
        'pulse-dot': 'pulseDot 2s ease-in-out infinite',
        'fade-up': 'fadeUp 0.5s cubic-bezier(0.34,1.56,0.64,1) forwards',
        'skeleton': 'skeleton 1.5s ease-in-out infinite',
      },
      keyframes: {
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-12px)' },
        },
        ticker: {
          '0%': { transform: 'translateX(0)' },
          '100%': { transform: 'translateX(-50%)' },
        },
        pulseDot: {
          '0%, 100%': { opacity: '1', transform: 'scale(1)' },
          '50%': { opacity: '0.4', transform: 'scale(0.7)' },
        },
        fadeUp: {
          from: { opacity: '0', transform: 'translateY(24px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
        skeleton: {
          '0%, 100%': { opacity: '0.4' },
          '50%': { opacity: '1' },
        },
      },
      boxShadow: {
        'teal-glow': '0 0 40px rgba(13,148,136,0.3)',
        'teal-sm': '0 0 20px rgba(13,148,136,0.15)',
        'glass': '0 8px 40px rgba(0,0,0,0.25)',
        'card': '0 4px 24px rgba(0,0,0,0.08)',
      },
      backdropBlur: {
        'glass': '20px',
      },
    },
  },
  plugins: [],
};
