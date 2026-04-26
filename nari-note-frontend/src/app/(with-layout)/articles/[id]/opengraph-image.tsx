import { ImageResponse } from 'next/og'
import { getArticleContent } from '@/lib/api/server'
import { readFile } from 'node:fs/promises'
import { join } from 'node:path'

export const alt = '記事のOGP画像'
export const size = { width: 1200, height: 630 }
export const contentType = 'image/png'

async function fetchJapaneseFont(text: string): Promise<ArrayBuffer> {
  const url = `https://fonts.googleapis.com/css2?family=Noto+Sans+JP:wght@700&text=${encodeURIComponent(text)}&display=swap`
  const css = await fetch(url, {
    headers: { 'User-Agent': 'Mozilla/5.0 (compatible)' },
  }).then((r) => r.text())
  const match = css.match(/src: url\(([^)]+)\) format\('woff2'\)/)
  if (!match) throw new Error('font url not found')
  return fetch(match[1]).then((r) => r.arrayBuffer())
}

export default async function Image({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params

  let title = 'なりノート'
  try {
    const data = await getArticleContent({ id })
    title = data.article.title
  } catch {}

  const [fontData, bgBuffer] = await Promise.all([
    fetchJapaneseFont(title),
    readFile(join(process.cwd(), 'public/narinote_background.png')),
  ])

  const bgSrc = `data:image/png;base64,${bgBuffer.toString('base64')}`

  return new ImageResponse(
    (
      <div style={{ width: '100%', height: '100%', display: 'flex', position: 'relative' }}>
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={bgSrc}
          style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', objectFit: 'cover' }}
          alt=""
        />
        <div
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            padding: '60px 80px',
            backgroundColor: 'rgba(0, 0, 0, 0.45)',
          }}
        >
          <p
            style={{
              fontFamily: 'Noto Sans JP',
              fontSize: 56,
              fontWeight: 700,
              color: '#ffffff',
              textAlign: 'center',
              lineHeight: 1.5,
              margin: 0,
            }}
          >
            {title}
          </p>
        </div>
      </div>
    ),
    {
      ...size,
      fonts: [{ name: 'Noto Sans JP', data: fontData, style: 'normal', weight: 700 }],
    }
  )
}
