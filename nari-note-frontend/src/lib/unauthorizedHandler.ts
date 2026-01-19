/**
 * UnauthorizedHandler - Singleton
 * 
 * 401エラーが発生した時にモーダル表示をトリガーするためのシングルトン
 * axiosインターセプターとReactコンテキスト間の橋渡しを行う
 */
class UnauthorizedHandler {
  private callback: (() => void) | null = null;

  register(callback: () => void) {
    if (this.callback !== null) {
      console.warn('[UnauthorizedHandler] Callback is already registered. Overwriting with new callback.');
    }
    this.callback = callback;
  }

  unregister() {
    this.callback = null;
  }

  trigger() {
    if (this.callback) {
      this.callback();
    } else {
      console.warn('[UnauthorizedHandler] Trigger called but no callback is registered.');
    }
  }
}

export const unauthorizedHandler = new UnauthorizedHandler();
