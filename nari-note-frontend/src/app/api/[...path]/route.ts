import { getEnv } from '../../../../utils/env';
import { type NextRequest } from 'next/server';

function getApiUrl(): string {
  return getEnv('API_URL') || 'http://localhost:5243';
}

async function proxyRequest(
  request: NextRequest,
  { params }: { params: Promise<{ path: string[] }> }
): Promise<Response> {
  const { path } = await params;
  const apiUrl = getApiUrl();
  const pathStr = path.join('/');
  const { search } = new URL(request.url);
  const targetUrl = `${apiUrl}/api/${pathStr}${search}`;

  const headers = new Headers(request.headers);
  headers.delete('host');

  const init: RequestInit = {
    method: request.method,
    headers,
    redirect: 'manual',
  };

  if (!['GET', 'HEAD'].includes(request.method)) {
    const body = await request.arrayBuffer();
    if (body.byteLength > 0) {
      (init as RequestInit & { body: ArrayBuffer }).body = body;
    }
  }

  return fetch(targetUrl, init);
}

export async function GET(request: NextRequest, context: { params: Promise<{ path: string[] }> }) {
  return proxyRequest(request, context);
}

export async function POST(request: NextRequest, context: { params: Promise<{ path: string[] }> }) {
  return proxyRequest(request, context);
}

export async function PUT(request: NextRequest, context: { params: Promise<{ path: string[] }> }) {
  return proxyRequest(request, context);
}

export async function DELETE(request: NextRequest, context: { params: Promise<{ path: string[] }> }) {
  return proxyRequest(request, context);
}
