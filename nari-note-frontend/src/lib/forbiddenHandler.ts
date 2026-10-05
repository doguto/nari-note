class ForbiddenHandler {
  private callback: ((message?: string) => void) | null = null;

  register(callback: (message?: string) => void) {
    if (this.callback !== null) {
      console.warn('[ForbiddenHandler] Callback is already registered. Overwriting with new callback.');
    }
    this.callback = callback;
  }

  unregister() {
    this.callback = null;
  }

  trigger(message?: string) {
    if (this.callback) {
      this.callback(message);
    } else {
      console.warn('[ForbiddenHandler] Trigger called but no callback is registered.');
    }
  }
}

export const forbiddenHandler = new ForbiddenHandler();
