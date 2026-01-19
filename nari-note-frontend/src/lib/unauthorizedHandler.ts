/**
 * UnauthorizedHandler - Singleton
 * 
 * 401エラーが発生した時にモーダル表示をトリガーするためのシングルトン
 * axiosインターセプターとReactコンテキスト間の橋渡しを行う
 */
class UnauthorizedHandler {
  private callback: (() => void) | null = null;

  register(callback: () => void) {
    this.callback = callback;
  }

  unregister() {
    this.callback = null;
  }

  trigger() {
    if (this.callback) {
      this.callback();
    }
  }
}

export const unauthorizedHandler = new UnauthorizedHandler();
