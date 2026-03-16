# Design System Rules

## Color Palette
- Primary: #1B2A4A (dark navy — headers, CTA backgrounds)
- Accent: #3B82F6 (blue — links, active states, venue indicators)
- Secondary: #8B5CF6 (purple — premium features, gifting, AI radar)
- Success: #10B981 (green — check-in confirmed, online status)
- Warning: #F59E0B (orange — activity level, pending states)
- Error: #EF4444 (red — errors, decline, block)
- Background: #F9FAFB (light gray) / #111827 (dark mode)
- Surface: #FFFFFF / #1F2937 (dark mode)
- Text Primary: #1F2937 / #F9FAFB (dark mode)
- Text Secondary: #6B7280 / #9CA3AF (dark mode)

## Typography (NativeWind classes)
- Display: text-3xl font-bold (venue name, screen titles)
- Heading: text-xl font-semibold (section headers)
- Body: text-base font-normal (regular text)
- Caption: text-sm text-gray-500 (metadata, timestamps)
- Tiny: text-xs (badges, counters)

## Spacing Scale
- xs: 4px, sm: 8px, md: 16px, lg: 24px, xl: 32px, 2xl: 48px

## Component Patterns
- Cards: rounded-2xl bg-white shadow-sm dark:bg-gray-800 p-4
- Buttons (primary): bg-blue-500 rounded-xl py-3 px-6 text-white font-semibold
- Buttons (secondary): border border-blue-500 rounded-xl py-3 px-6 text-blue-500
- Input fields: bg-gray-100 rounded-xl py-3 px-4 text-base
- Avatar: rounded-full, sizes: sm(32), md(48), lg(64), xl(96)
- Bottom sheet: rounded-t-3xl bg-white shadow-lg

## Animation Guidelines
- Screen transitions: 250ms ease-in-out
- Element appear: 200ms fade-in + scale(0.95 → 1.0)
- Gift animations: Lottie, full-screen, 2-4 seconds duration
- Pull-to-refresh: native platform behavior
- Chat messages: slide-in from bottom, 150ms

## Accessibility
- Minimum touch target: 44x44 points
- Color contrast: WCAG AA minimum (4.5:1 for text)
- All images require alt text
- Screen reader labels on all interactive elements
- Support dynamic type scaling (iOS) and font scaling (Android)
