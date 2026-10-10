// バックエンドの ErrorCode.EmailNotVerified と対応する
export const EMAIL_NOT_VERIFIED_CODE = 'EMAIL_NOT_VERIFIED';

type ForbiddenCallback = (message?: string, code?: string) => void;

class ForbiddenHandler {
  private callback: ForbiddenCallback | null = null;

  register(callback: ForbiddenCallback) {
    if (this.callback !== null) {
      console.warn('[ForbiddenHandler] Callback is already registered. Overwriting with new callback.');
    }
    this.callback = callback;
  }

  unregister() {
    this.callback = null;
  }

  trigger(message?: string, code?: string) {
    if (this.callback) {
      this.callback(message, code);
    } else {
      console.warn('[ForbiddenHandler] Trigger called but no callback is registered.');
    }
  }
}

export const forbiddenHandler = new ForbiddenHandler();
