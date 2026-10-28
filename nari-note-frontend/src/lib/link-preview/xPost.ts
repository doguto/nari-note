const X_POST_URL_PATTERN = /^https?:\/\/(?:www\.|mobile\.)?(?:x|twitter)\.com\/\w+\/status(?:es)?\/(\d+)/i;
const X_WIDGETS_SCRIPT_URL = 'https://platform.twitter.com/widgets.js';

interface XWidgets {
  widgets: {
    // 投稿が削除済み・非公開などで描画できない場合は undefined で解決される
    createTweet(postId: string, target: HTMLElement, options?: Record<string, unknown>): Promise<HTMLElement | undefined>;
  };
}

declare global {
  interface Window {
    twttr?: XWidgets;
  }
}

let loadingPromise: Promise<XWidgets> | null = null;

export function getXPostId(url: string): string | null {
  return url.match(X_POST_URL_PATTERN)?.[1] ?? null;
}

export function loadXWidgets(): Promise<XWidgets> {
  if (window.twttr?.widgets) return Promise.resolve(window.twttr);

  loadingPromise ??= new Promise<XWidgets>((resolve, reject) => {
    const script = document.createElement('script');
    script.src = X_WIDGETS_SCRIPT_URL;
    script.async = true;
    script.charset = 'utf-8';
    script.onload = () => {
      if (window.twttr?.widgets) {
        resolve(window.twttr);
      } else {
        reject(new Error('X widgets not available after script load'));
      }
    };
    script.onerror = () => {
      // 一時的な失敗なら次回の表示で再試行できるようにする
      loadingPromise = null;
      script.remove();
      reject(new Error('X widgets script load failed'));
    };
    document.head.appendChild(script);
  });
  return loadingPromise;
}
